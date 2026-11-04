using System;
using SldWorks;
using SwConst;

namespace SolidWorksAIBridge
{
    public sealed partial class SolidWorksModeler
    {
        /// <summary>Additive equal-height candidate. Requires an unedited, front-facing single-solid host.
        /// CAD behavior remains unverified until the explicit comparison case is approved/run.</summary>
        public ModelDoc2 CreateTopFaceBalconyAssembly(BalconyAssemblyInstance instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (wallModel == null || mainWall == null) throw new InvalidOperationException("Create the host wall first.");
            var plan = TopFaceBalconyAssembly.Validate(instance,
                new BalconySlabHost("MainWall", wallWidth * 1000, wallHeight * 1000, wallThickness * 1000));
            var model = wallModel;
            var part = (PartDoc)model;
            if (!ReferenceEquals(application.ActiveDoc, model) || model.SketchManager.ActiveSketch != null)
                throw new InvalidOperationException("Activate the host part and exit any sketch.");
            if (SolidBodyCount(part) != 1) throw new InvalidOperationException("Candidate requires a single-solid host.");
            foreach (string name in new[] { plan.Slab.SketchName, plan.Slab.FeatureName,
                TopFaceBalconyAssembly.SketchName(plan), TopFaceBalconyAssembly.FeatureName(plan) })
                if (part.FeatureByName(name) != null) throw new InvalidOperationException("Existing/unfinished component: " + name);
            foreach (var wall in new[] { plan.Left, plan.Right })
                ValidateNoOverlap(wall.ThicknessMm / 1000, wall.HeightMm / 1000, wall.CenterXMm / 1000, wall.CenterYMm / 1000);
            var facade = (Body2)FindWallFrontFace().GetBody();
            double facadeVolume = ((double[])facade.GetMassProperties(1))[3];
            double[] facadeBounds = AssemblyVertexBounds(facade);
            CreateProjectingBalconySlab(instance.Slab);
            AddTopFaceWalls(model, plan);
            VerifyAssemblyBodies(part, facadeVolume, facadeBounds, plan.VolumeMm3);
            GetOnlyBalconyBody(part).Name = "BalconyAssembly_" + instance.Id;
            foreach (var wall in new[] { plan.Left, plan.Right })
                RememberBounds(wall.ThicknessMm / 1000, wall.HeightMm / 1000, wall.CenterXMm / 1000, wall.CenterYMm / 1000);
            model.ClearSelection2(true);
            model.ViewZoomtofit2();
            return model;
        }

        private void AddTopFaceWalls(ModelDoc2 model, BalconyAssemblyPlan plan)
        {
            var sketches = model.SketchManager;
            bool sketchOpen = false;
            bool originalAddToDb = sketches.AddToDB;
            int prompt = (int)swUserPreferenceToggle_e.swInputDimValOnCreate;
            bool originalPrompt = application.GetUserPreferenceToggle(prompt);
            try
            {
                model.ClearSelection2(true);
                Face2 top = FindSlabTopFace(GetOnlyBalconyBody((PartDoc)model), plan);
                if (!((Entity)top).Select4(false, null)) throw new InvalidOperationException("Cannot select slab top face.");
                sketches.InsertSketch(true);
                Sketch sketch = sketches.ActiveSketch;
                if (sketch == null) throw new InvalidOperationException("Cannot open slab-top sketch.");
                sketchOpen = true;
                Feature profile = (Feature)model.FeatureByPositionReverse(0);
                if (profile == null || profile.GetTypeName2() != "ProfileFeature")
                    throw new InvalidOperationException("Cannot locate slab-top sketch feature.");
                profile.Name = TopFaceBalconyAssembly.SketchName(plan);
                application.SetUserPreferenceToggle(prompt, false);
                foreach (var wall in new[] { plan.Left, plan.Right }) AddTopFaceProfile(model, sketch, plan, wall);
                var math = (MathUtility)application.GetMathUtility();
                var toModel = (MathTransform)sketch.ModelToSketchTransform.Inverse();
                var origin = TransformPoint(math, toModel, 0, 0, 0);
                var axis = TransformPoint(math, toModel, 0, 0, 1);
                double alignment = axis[1] - origin[1];
                if (double.IsNaN(alignment) || Math.Abs(Math.Abs(alignment) - 1) > 1e-8)
                    throw new InvalidOperationException("Sketch normal does not align with upward Y.");
                model.ClearSelection2(true);
                sketches.InsertSketch(true);
                sketchOpen = sketches.ActiveSketch != null;
                if (sketchOpen) throw new InvalidOperationException("Cannot exit slab-top sketch.");
                if (!profile.Select2(false, 0)) throw new InvalidOperationException("Cannot select shared wall profile.");
                // Same explicit selected-body policy as the preserved CAD-tested baseline.
                Body2 target = GetOnlyBalconyBody((PartDoc)model);
                var selection = (SelectData)((SelectionMgr)model.SelectionManager).CreateSelectData();
                selection.Mark = 8;
                if (!target.Select2(true, selection) || ((SelectionMgr)model.SelectionManager).GetSelectedObjectCount2(8) != 1)
                    throw new InvalidOperationException("Exactly one exclusive balcony merge target is required.");
                Feature boss = model.FeatureManager.FeatureExtrusion3(true, false, alignment < 0,
                    (int)swEndConditions_e.swEndCondBlind, (int)swEndConditions_e.swEndCondBlind,
                    plan.Left.HeightMm / 1000, plan.Left.HeightMm / 1000, false, false, false, false, 0, 0,
                    false, false, false, false, true, true, false,
                    (int)swStartConditions_e.swStartSketchPlane, 0, false);
                if (boss == null) throw new InvalidOperationException("Scoped upward shared-wall Boss-Extrude failed.");
                boss.Name = TopFaceBalconyAssembly.FeatureName(plan);
                var height = (Dimension)model.Parameter("D1@" + boss.Name);
                if (height == null) throw new InvalidOperationException("Common wall-height dimension missing.");
                height.Name = "WallHeight";
                var equations = (EquationMgr)model.GetEquationMgr();
                foreach (string equation in TopFaceBalconyAssembly.Equations(plan))
                    if (equations.Add2(-1, equation, true) < 0) throw new InvalidOperationException("Cannot add binding: " + equation);
                equations.EvaluateAll();
                if (!model.EditRebuild3()) throw new InvalidOperationException("Shared-wall rebuild failed.");
            }
            catch (Exception ex) { throw new InvalidOperationException(TopFaceBalconyAssembly.FeatureName(plan) + ": " + ex.Message, ex); }
            finally
            {
                try { if (sketchOpen) sketches.InsertSketch(true); }
                finally
                {
                    model.ClearSelection2(true);
                    try { sketches.AddToDB = originalAddToDb; }
                    finally { application.SetUserPreferenceToggle(prompt, originalPrompt); }
                }
            }
        }

        private static Face2 FindSlabTopFace(Body2 body, BalconyAssemblyPlan plan)
        {
            Face2 found = null;
            foreach (object item in body.GetFaces() as object[] ?? new object[0])
            {
                var face = (Face2)item;
                var normal = face.Normal as double[];
                if (!((Surface)face.GetSurface()).IsPlane() || normal == null ||
                    Math.Abs(normal[0]) > 1e-8 || Math.Abs(normal[1] - 1) > 1e-8 || Math.Abs(normal[2]) > 1e-8) continue;
                // GetBox is approximate. Measure actual edge endpoints instead.
                var bounds = new[] { double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,
                    double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity };
                int count = 0;
                foreach (object edgeItem in face.GetEdges() as object[] ?? new object[0])
                {
                    var edge = (Edge)edgeItem;
                    foreach (Vertex vertex in new[] { (Vertex)edge.GetStartVertex(), (Vertex)edge.GetEndVertex() })
                    {
                        if (vertex == null) throw new InvalidOperationException("Unexpected curved slab boundary.");
                        var p = (double[])vertex.GetPoint();
                        for (int i = 0; i < 3; i++) { bounds[i] = Math.Min(bounds[i], p[i]); bounds[i + 3] = Math.Max(bounds[i + 3], p[i]); }
                        count++;
                    }
                }
                double[] expected = { plan.Slab.LeftMm, plan.Slab.TopMm, plan.Slab.RearMm,
                    plan.Slab.RightMm, plan.Slab.TopMm, plan.Slab.FrontMm };
                bool matches = count == 8;
                for (int i = 0; i < 6; i++) matches &= Math.Abs(bounds[i] * 1000 - expected[i]) < .001;
                matches &= Math.Abs(face.GetArea() * 1e6 - plan.Instance.Slab.WidthMm * plan.Instance.Slab.DepthMm) < .001;
                if (!matches) continue;
                if (found != null) throw new InvalidOperationException("Ambiguous slab top face.");
                found = face;
            }
            return found ?? throw new InvalidOperationException("Unique upward rectangular slab top face not found.");
        }

        private void AddTopFaceProfile(ModelDoc2 model, Sketch sketch, BalconyAssemblyPlan plan, BalconySideWallPlan wall)
        {
            var math = (MathUtility)application.GetMathUtility();
            MathTransform toSketch = sketch.ModelToSketchTransform;
            var toModel = (MathTransform)toSketch.Inverse();
            double y = plan.Slab.TopMm / 1000;
            var origin = TransformPoint(math, toSketch, 0, y, 0);
            var x = TransformPoint(math, toSketch, 1, y, 0);
            var z = TransformPoint(math, toSketch, 0, y, 1);
            if (Math.Abs(origin[0]) > 1e-8 || Math.Abs(origin[1]) > 1e-8 || Math.Abs(origin[2]) > 1e-8)
                throw new InvalidOperationException("Unsupported translated slab-top sketch origin.");
            bool xHorizontal = TopFaceBalconyAssembly.XIsHorizontal(x[0] - origin[0], x[1] - origin[1], z[0] - origin[0], z[1] - origin[1]);
            double thickness = wall.ThicknessMm / 1000, depth = wall.DepthMm / 1000;
            double centerX = wall.CenterXMm / 1000, centerZ = (wall.RearMm + wall.FrontMm) / 2000;
            // Seed on the same signed branches as the requested model coordinates.
            var seed = TransformPoint(math, toSketch, centerX < 0 ? -.023 : .023, y, .017);
            double width = xHorizontal ? thickness : depth, height = xHorizontal ? depth : thickness;
            var sketches = model.SketchManager;
            model.ClearSelection2(true);
            sketches.AddToDB = true;
            var segments = sketches.CreateCenterRectangle(seed[0], seed[1], 0, seed[0] + width / 2, seed[1] + height / 2, 0) as object[];
            sketches.AddToDB = false;
            if (segments == null || segments.Length != 6) throw new InvalidOperationException("Expected four profile edges and two construction diagonals.");
            VerifyRectangleGeometry(segments, seed[0], seed[1], width, height);
            SketchSegment horizontal = null, vertical = null, diagonal = null;
            foreach (object item in segments)
            {
                var segment = (SketchSegment)item;
                var line = (SketchLine)segment;
                var a = (SketchPoint)line.GetStartPoint2();
                var b = (SketchPoint)line.GetEndPoint2();
                if (segment.ConstructionGeometry) diagonal = segment;
                else if (Math.Abs(a.Y - b.Y) < 1e-10) horizontal = segment;
                else if (Math.Abs(a.X - b.X) < 1e-10) vertical = segment;
            }
            if (horizontal == null || vertical == null || diagonal == null) throw new InvalidOperationException("Cannot resolve current footprint edges.");
            model.ClearSelection2(true);
            foreach (object item in segments)
                if (!((SketchSegment)item).Select4(true, null)) throw new InvalidOperationException("Cannot select footprint relations.");
            int relations = (int)(swSketchFullyDefineRelationType_e.swSketchFullyDefineRelationType_Horizontal |
                swSketchFullyDefineRelationType_e.swSketchFullyDefineRelationType_Vertical |
                swSketchFullyDefineRelationType_e.swSketchFullyDefineRelationType_Coincident |
                swSketchFullyDefineRelationType_e.swSketchFullyDefineRelationType_Midpoint);
            sketches.FullyDefineSketch(false, true, relations, false, 0, null, 0, null, 0, 0);
            model.ClearSelection2(true);
            AddWindowSizeDimension(model, xHorizontal ? horizontal : vertical, "Thickness_" + wall.Role,
                thickness, math, toModel, seed[0], seed[1] - height);
            AddWindowSizeDimension(model, xHorizontal ? vertical : horizontal, "Depth_" + wall.Role,
                depth, math, toModel, seed[0] + width, seed[1]);
            AddWindowPositionDimension(model, diagonal, xHorizontal, "CenterX_" + wall.Role,
                Math.Abs(centerX), math, toModel, seed[0], seed[1] - height * 1.5);
            AddWindowPositionDimension(model, diagonal, !xHorizontal, "CenterZ_" + wall.Role,
                centerZ, math, toModel, seed[0] - width * 1.5, seed[1]);
            var expected = TransformPoint(math, toSketch, centerX, y, centerZ);
            VerifyRectangleGeometry(segments, expected[0], expected[1], width, height);
            model.ClearSelection2(true);
        }
    }
}
