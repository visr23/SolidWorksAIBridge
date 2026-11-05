using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using SldWorks;
using SwConst;

namespace SolidWorksAIBridge.Pilot001
{
    internal sealed class PilotBuilder
    {
        private readonly SldWorks.SldWorks application;
        private readonly R2Plan plan;
        private ModelDoc2 model;
        private EquationMgr equations;
        private readonly BodyIdentity<Body2> bodyIdentity;
        public ModelDoc2 Model { get { return model; } }
        public string Stage { get; private set; }
        private static string Q(string value) { return "\"" + value + "\""; }
        private static void Require(bool condition, string message) { if(!condition) throw new InvalidOperationException(message); }
        public PilotBuilder(SldWorks.SldWorks application, R2Plan plan)
        {
            this.application = application;
            this.plan = plan;
            bodyIdentity = new BodyIdentity<Body2>(() => Bodies().Cast<Body2>(), (a, b) => Same(a, b));
        }
        private void Bind(string left, string right)
        {
            Require(equations.Add2(-1,Q(left)+" = "+right,true)>=0,"Cannot bind "+left);
        }
        public void Run()
        {
            Stage="CreateWall";
            var legacy=new SolidWorksModeler(application);
            model=legacy.CreateWall(224.5,99.8,12);
            Console.WriteLine("NEW_UNSAVED_PART "+model.GetTitle());
            Require(string.IsNullOrEmpty(model.GetPathName()),"New test must be unsaved.");
            Track("MainWall", (Feature)((PartDoc)model).FeatureByName("MainWall"));
            Check(1,"MainWall",224.5*99.8*12);
            Stage="CreateWindow_Right";
            legacy.CreateWindow(23.4,30.4,54.85,-3.1);
            Track("MainWall", (Feature)((PartDoc)model).FeatureByName("WindowCut"));
            BodyGeometry.VerifyRebuild(model.EditRebuild3());
            Check(1,"MainWall",(224.5*99.8-23.4*30.4)*12);
            VerifyRightWindow();
            Stage = "ParameterBindings";
            equations=(EquationMgr)model.GetEquationMgr();
            foreach(var parameter in plan.Parameters) Bind(parameter.Key,parameter.Value.ToString("R",CultureInfo.InvariantCulture)+"mm");
            Bind("Width@WallSketch",Q("FacadeWidth"));
            Bind("Height@WallSketch",Q("WallHeight"));
            Bind("D1@MainWall",Q("BackingThickness"));
            Bind("WindowWidth@WindowSketch",Q("RightRight")+" - "+Q("RightLeft"));
            Bind("WindowHeight@WindowSketch",Q("RightHead")+" - "+Q("RightBottom"));
            Bind("CenterX@WindowSketch","abs(("+Q("RightLeft")+" + "+Q("RightRight")+") / 2 - "+Q("FacadeWidth")+" / 2)");
            Bind("CenterY@WindowSketch","abs(("+Q("RightBottom")+" + "+Q("RightHead")+") / 2 - "+Q("WallHeight")+" / 2)");
            equations.EvaluateAll();
            Require(model.EditRebuild3(),"Parameter bindings rebuild failed.");
            foreach(var operation in plan.Operations())
            {
                Stage=operation.Name;
                Console.WriteLine("BEGIN "+Stage);
                CreateOperation(operation);
                Check(operation.ExpectedBodies,operation.Target!=null?operation.Target:operation.Name,operation.ExpectedVolume);
                Console.WriteLine("PASS "+Stage);
            }
            Stage="FinalRebuildAndBounds";
            Require(model.EditRebuild3(),"Final rebuild failed.");
            var bodies=Bodies();
            double[] box=UnionBounds(bodies);
            Require(Math.Abs(box[4]-box[1]-180)<=0.05,"Architectural height differs from 180.");
            Require(Math.Abs(box[0]-(-10.5))<=0.05 && Math.Abs(box[3]-232.5)<=0.05 && Math.Abs(box[1])<=0.05,"Envelope XY differs from R2.");
            Require(Math.Abs(box[2]-(-12))<=0.05 && Math.Abs(box[5]-8)<=0.05,"Envelope Z differs from R2.");
            Console.WriteLine("FINAL_REBUILD PASS; HeightMm={0:R}; WidthMm={1:R}; ReliefDepthMm={2:R}; Bodies={3}; Saved=false",box[4]-box[1],box[3]-box[0],box[5]-box[2],bodies.Length);
        }
        private object[] Bodies() { return ((PartDoc)model).GetBodies2((int)swBodyType_e.swSolidBody,false) as object[] ?? new object[0]; }
        private Body2 Body(string role) { return bodyIdentity.Resolve(role); }

        private void Track(string role, Feature feature)
        {
            Require(feature != null, "Missing feature anchor for " + role);
            // Re-query all face owners on each access: prior Body2 pointers can be
            // invalidated by a cut/rebuild. Membership is checked against live solids.
            bodyIdentity.Track(role, () =>
            {
                var faces = feature.GetFaces() as object[];
                return faces == null ? null : faces.Cast<Face2>().Select(face => (Body2)face.GetBody()).ToArray();
            });
        }

        private static BodyGeometry Geometry(Body2 body)
        {
            var mass = body.GetMassProperties(1) as double[];
            Require(mass != null && mass.Length > 3, "Body mass properties unavailable.");
            return new BodyGeometry(mass[3] * 1e9, Bounds(body), body.GetVertexCount(), body.GetEdgeCount(), body.GetFaceCount());
        }

        private void VerifyRightWindow()
        {
            foreach (var item in new[]
            {
                Tuple.Create("WindowWidth@WindowSketch", 23.4), Tuple.Create("WindowHeight@WindowSketch", 30.4),
                Tuple.Create("CenterX@WindowSketch", 54.85), Tuple.Create("CenterY@WindowSketch", 3.1),
                Tuple.Create("D1@WindowCut", 12.2)
            })
            {
                var dimension = (Dimension)model.Parameter(item.Item1);
                Require(dimension != null, "Missing opening dimension " + item.Item1);
                BodyGeometry.VerifyDimension(dimension.SystemValue * 1000, item.Item2,
                    dimension.DrivenState == (int)swDimensionDrivenState_e.swDimensionDriving);
                Console.WriteLine("VERIFIED_DIMENSION {0}; ValueMm={1:R}", item.Item1, dimension.SystemValue * 1000);
            }
            var cut = (Feature)((PartDoc)model).FeatureByName("WindowCut");
            var faces = cut.GetFaces() as object[];
            Require(faces != null && faces.Length == 4, "Expected four rectangular through-cut side faces.");
            var points = faces.Cast<Face2>().SelectMany(face => face.GetEdges() as object[] ?? new object[0])
                .Cast<Edge>().SelectMany(edge => new[] { (Vertex)edge.GetStartVertex(), (Vertex)edge.GetEndVertex() });
            var box = new[] { double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,
                double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity };
            foreach (var vertex in points)
            {
                Require(vertex != null, "Rectangular cut edge has no vertex.");
                var point = (double[])vertex.GetPoint();
                var value = new[] { point[0] * 1000 + 112.25, point[1] * 1000 + 49.9, point[2] * 1000 - 6 };
                for (int i = 0; i < 3; i++) { box[i] = Math.Min(box[i], value[i]); box[i + 3] = Math.Max(box[i + 3], value[i]); }
            }
            var expected = new[] { 155.4, 31.6, -12.0, 178.8, 62.0, 0.0 };
            BodyGeometry.VerifyAperture(faces.Length, box, expected);
            Console.WriteLine("RIGHT_WINDOW_GEOMETRY PASS; four side faces; aperture bounds match R2; rebuild passed");
        }
        private static bool Same(object a, object b)
        {
            IntPtr pa=Marshal.GetIUnknownForObject(a),pb=Marshal.GetIUnknownForObject(b);
            try{return pa==pb;}finally{Marshal.Release(pa);Marshal.Release(pb);}
        }
        private static double[] ModelPoint(MathUtility math, MathTransform transform, double x,double y,double z)
        {
            return (double[])((MathPoint)((MathPoint)math.CreatePoint(new[]{x,y,z})).MultiplyTransform(transform)).ArrayData;
        }
        private Feature Profile(Operation operation, out double alignment)
        {
            var sm=model.SketchManager;
            bool oldAdd=sm.AddToDB,open=false;
            int pref=(int)swUserPreferenceToggle_e.swInputDimValOnCreate;
            bool oldPrompt=application.GetUserPreferenceToggle(pref);
            alignment=0;
            try
            {
                model.ClearSelection2(true);
                Require(model.Extension.SelectByID2("Front Plane","PLANE",0,0,0,false,0,null,0),"Front Plane unavailable.");
                sm.InsertSketch(true);
                var sketch=sm.ActiveSketch;
                Require(sketch!=null,"Cannot open profile sketch."); open=true;
                var feature=(Feature)model.FeatureByPositionReverse(0);
                Require(feature!=null && feature.GetTypeName2()=="ProfileFeature","New sketch identity unavailable.");
                feature.Name="Pilot001_"+operation.Name+"Sketch";
                var math=(MathUtility)application.GetMathUtility();
                MathTransform toSketch=sketch.ModelToSketchTransform;
                MathTransform toModel=(MathTransform)toSketch.Inverse();
                double[] normalOrigin=ModelPoint(math,toModel,0,0,0),normalEnd=ModelPoint(math,toModel,0,0,1);
                alignment=normalEnd[2]-normalOrigin[2];
                Require(Math.Abs(Math.Abs(alignment)-1)<1e-8,"Front sketch not parallel to facade.");
                application.SetUserPreferenceToggle(pref,false);
                int loopIndex=0;
                foreach(var polygon in operation.Loops)
                {
                    var edges=new List<SketchLine>();
                    sm.AddToDB=true;
                    for(int i=0;i<polygon.Length;i++)
                    {
                        var a=polygon[i]; var b=polygon[(i+1)%polygon.Length];
                        double[] sa=ModelPoint(math,toSketch,(a.X.Value-112.25)/1000,(a.Y.Value-49.9)/1000,0);
                        double[] sb=ModelPoint(math,toSketch,(b.X.Value-112.25)/1000,(b.Y.Value-49.9)/1000,0);
                        var edge=sm.CreateLine(sa[0],sa[1],0,sb[0],sb[1],0);
                        Require(edge!=null,"Cannot create edge "+i);
                        edges.Add((SketchLine)edge);
                    }
                    sm.AddToDB=false;
                    for(int i=0;i<edges.Count;i++)
                    {
                        var end=(SketchPoint)edges[i].GetEndPoint2();
                        var next=(SketchPoint)edges[(i+1)%edges.Count].GetStartPoint2();
                        if(!Same(end,next))
                        {
                            model.ClearSelection2(true);
                            Require(end.Select4(false,null)&&next.Select4(true,null),"Cannot select profile join.");
                            model.SketchAddConstraints("sgCOINCIDENT");
                        }
                    }
                    for(int i=0;i<edges.Count;i++)
                    {
                        var point=(SketchPoint)edges[i].GetStartPoint2();
                        Point expected=polygon[i];
                        double[] origin=ModelPoint(math,toSketch,0,0,0),x=ModelPoint(math,toSketch,1,0,0);
                        bool xHorizontal=Math.Abs(x[0]-origin[0])>0.999999;
                        AddCoordinate(point,true,xHorizontal,"L"+loopIndex+"V"+i+"X",expected.X,feature,math,toModel);
                        AddCoordinate(point,false,!xHorizontal,"L"+loopIndex+"V"+i+"Y",expected.Y,feature,math,toModel);
                    }
                    for(int i=0;i<edges.Count;i++)
                    {
                        var a=(SketchPoint)edges[i].GetStartPoint2();
                        double[] actual=ModelPoint(math,toModel,a.X,a.Y,0);
                        Require(Math.Abs(actual[0]*1000+112.25-polygon[i].X.Value)<0.001 && Math.Abs(actual[1]*1000+49.9-polygon[i].Y.Value)<0.001,"Dimension solver moved profile vertex.");
                    }
                    loopIndex++;
                }
                model.ClearSelection2(true);
                sm.InsertSketch(true);open=false;
                return feature;
            }
            finally
            {
                try{if(open)sm.InsertSketch(true);}
                finally{sm.AddToDB=oldAdd;application.SetUserPreferenceToggle(pref,oldPrompt);model.ClearSelection2(true);}
            }
        }
        private void AddCoordinate(SketchPoint point,bool isX,bool horizontal,string name,Expression value,Feature profile,MathUtility math,MathTransform toModel)
        {
            // Part origin projected onto Front Plane; dimensions are unsigned but
            // creation retains the original quadrant, checked after every profile.
            Expression offset=isX?Expression.Half(plan.P("FacadeWidth")):Expression.Half(plan.P("WallHeight"));
            Expression coordinate=value-offset;
            model.ClearSelection2(true);
            Require(point.Select4(false,null),"Cannot select vertex dimension.");
            Require(model.Extension.SelectByID2("Point1@Origin","EXTSKETCHPOINT",0,0,0,true,0,null,0),"Cannot select Part origin.");
            double[] text=ModelPoint(math,toModel,point.X+0.004,point.Y+0.004,0);
            var display=(DisplayDimension)(horizontal?model.AddHorizontalDimension2(text[0],text[1],text[2]):model.AddVerticalDimension2(text[0],text[1],text[2]));
            Require(display!=null,"Cannot create coordinate "+name);
            var dimension=(Dimension)display.GetDimension2(0);
            Require(dimension!=null,"Cannot access coordinate "+name);
            dimension.Name=name;
            Require(dimension.DrivenState==(int)swDimensionDrivenState_e.swDimensionDriving,"Coordinate is not driving: "+name);
            Require(Math.Abs(dimension.SystemValue*1000-Math.Abs(coordinate.Value))<0.001,"Coordinate measured value mismatch: "+name);
            Bind(name+"@"+profile.Name,"abs("+coordinate.Text+")");
            model.ClearSelection2(true);
        }
        private void CreateOperation(Operation operation)
        {
            bodyIdentity.VerifyInventory(bodyIdentity.Roles.Length);
            var beforeState = bodyIdentity.Roles.ToDictionary(role => role, role => Geometry(Body(role)));
            Body2 target = operation.Target == null ? null : Body(operation.Target);
            double alignment;
            Feature profile=Profile(operation,out alignment);
            Require(profile.Select2(false,0),"Cannot explicitly select profile.");
            if(target!=null)
            {
                var selection=(SelectData)((SelectionMgr)model.SelectionManager).CreateSelectData();
                selection.Mark=8;
                Require(target.Select2(true,selection),"Cannot select exclusive target body.");
                Require(((SelectionMgr)model.SelectionManager).GetSelectedObjectCount2(8)==1,"Target scope is not exactly one body.");
            }
            double start=operation.StartZ.Value+6;
            int startCondition=Math.Abs(start)<1e-10?(int)swStartConditions_e.swStartSketchPlane:(int)swStartConditions_e.swStartOffset;
            bool flipStart=start*alignment<0;
            Feature feature;
            if(operation.Cut)
            {
                feature=model.FeatureManager.FeatureCut3(true,false,alignment<0,
                    (int)swEndConditions_e.swEndCondBlind,(int)swEndConditions_e.swEndCondBlind,
                    operation.Depth.Value/1000,operation.Depth.Value/1000,false,false,false,false,0,0,
                    false,false,false,false,false,true,false,false,false,false,
                    startCondition,Math.Abs(start)/1000,flipStart);
            }
            else
            {
                bool merge=target!=null;
                feature=model.FeatureManager.FeatureExtrusion3(true,false,operation.Direction*alignment<0,
                    (int)swEndConditions_e.swEndCondBlind,(int)swEndConditions_e.swEndCondBlind,
                    operation.Depth.Value/1000,operation.Depth.Value/1000,false,false,false,false,0,0,
                    false,false,false,false,merge,merge,false,
                    startCondition,Math.Abs(start)/1000,flipStart);
            }
            Require(feature!=null,"CAD feature returned null: "+operation.Name);
            feature.Name="Pilot001_"+operation.Name+"_PROVISIONAL";
            var depth=(Dimension)model.Parameter("D1@"+feature.Name);
            Require(depth!=null,"Depth dimension unavailable.");
            depth.Name="Depth";
            Bind("Depth@"+feature.Name,operation.Depth.Text);
            equations.EvaluateAll();
            Require(model.EditRebuild3(),"Rebuild failed: "+operation.Name);
            Track(operation.Target ?? operation.Name, feature);
            bodyIdentity.VerifyInventory(operation.ExpectedBodies);
            foreach (var state in beforeState)
            {
                if (state.Key == operation.Target) continue;
                state.Value.VerifyUnchanged(Geometry(Body(state.Key)));
            }
            model.ClearSelection2(true);
        }
        private void Check(int expectedCount,string target,double expectedVolume)
        {
            bodyIdentity.VerifyInventory(expectedCount);
            var body = Body(target);
            var geometry = Geometry(body);
            double[] expectedBounds = target == "MainWall" ? new[] { 0.0, 0.0, -12.0, 224.5, 99.8, 0.0 } : null;
            geometry.Verify(expectedVolume, expectedBounds);
            double actual = geometry.VolumeMm3;
            Console.WriteLine("MEASURE {0}; Bodies={1}; TargetVolumeMm3={2:R}; Bounds={3}",Stage,Bodies().Length,actual,string.Join(",",Bounds(body).Select(v=>v.ToString("R",CultureInfo.InvariantCulture))));
        }
        public static double[] Bounds(Body2 body)
        {
            var box=new[]{double.PositiveInfinity,double.PositiveInfinity,double.PositiveInfinity,double.NegativeInfinity,double.NegativeInfinity,double.NegativeInfinity};
            var vertices=body.GetVertices() as object[] ?? new object[0];
            Require(vertices.Length>0,"No exact vertices for bounds.");
            foreach(Vertex vertex in vertices)
            {
                var p=(double[])vertex.GetPoint();
                double[] v={p[0]*1000+112.25,p[1]*1000+49.9,p[2]*1000-6};
                for(int i=0;i<3;i++){box[i]=Math.Min(box[i],v[i]);box[i+3]=Math.Max(box[i+3],v[i]);}
            }
            return box;
        }
        private static double[] UnionBounds(object[] bodies)
        {
            var box=new[]{double.PositiveInfinity,double.PositiveInfinity,double.PositiveInfinity,double.NegativeInfinity,double.NegativeInfinity,double.NegativeInfinity};
            foreach(Body2 body in bodies)
            {
                double[] b=Bounds(body);
                for(int i=0;i<3;i++){box[i]=Math.Min(box[i],b[i]);box[i+3]=Math.Max(box[i+3],b[i+3]);}
            }
            return box;
        }
        public void Report(bool success)
        {
            if(model==null){Console.WriteLine("NO_TEST_DOCUMENT_RETURNED; legacy creation may have failed before returning.");return;}
            Console.WriteLine("RESULT_DOCUMENT {0}; Path={1}; LastStage={2}; Success={3}",model.GetTitle(),model.GetPathName(),Stage,success);
            int features=0,sketches=0,dimensions=0;
            var seen=new HashSet<string>();
            Action<Feature> inspect=null;
            inspect=f=>
            {
                if(!seen.Add(f.Name))return;
                string type=f.GetTypeName2();
                Console.WriteLine("FEATURE {0}; Type={1}",f.Name,type);
                if(type=="ProfileFeature")sketches++;
                if(f.Name=="MainWall"||f.Name=="WindowCut"||f.Name.EndsWith("_PROVISIONAL",StringComparison.Ordinal))features++;
                for(var display=(DisplayDimension)f.GetFirstDisplayDimension();display!=null;display=(DisplayDimension)display.GetNext3())
                {
                    var dim=(Dimension)display.GetDimension2(0);
                    if(dim!=null){dimensions++;Console.WriteLine("DIMENSION {0}; ValueMm={1:R}; Driving={2}",dim.FullName,dim.SystemValue*1000,dim.DrivenState);}
                }
                for(var child=(Feature)f.GetFirstSubFeature();child!=null;child=(Feature)child.GetNextSubFeature())inspect(child);
            };
            for(var f=(Feature)model.FirstFeature();f!=null;f=(Feature)f.GetNextFeature())inspect(f);
            foreach(Body2 body in Bodies())Console.WriteLine("BODY {0}; VolumeMm3={1:R}; Bounds={2}",body.Name,((double[])body.GetMassProperties(1))[3]*1e9,string.Join(",",Bounds(body).Select(v=>v.ToString("R",CultureInfo.InvariantCulture))));
            if(Bodies().Length>0){var b=UnionBounds(Bodies());Console.WriteLine("ACTUAL_ENVELOPE WidthMm={0:R}; HeightMm={1:R}; DepthMm={2:R}",b[3]-b[0],b[4]-b[1],b[5]-b[2]);}
            Console.WriteLine("COUNTS ModelingFeatures={0}; Sketches={1}; DimensionsEnumerated={2}; Bodies={3}",features,sketches,dimensions,Bodies().Length);
        }
        public void Present()
        {
            if(model==null)return;
            model.ClearSelection2(true);
            model.ShowNamedView2("*Front",(int)swStandardViews_e.swFrontView);
            model.ViewZoomtofit2();model.GraphicsRedraw2();
        }
    }
}
