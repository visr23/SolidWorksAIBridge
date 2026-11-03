using System;
using System.Runtime.InteropServices;
using SldWorks;
using SwConst;

namespace SolidWorksAIBridge
{
    public sealed partial class SolidWorksModeler
    {
        /// <summary>Both-wall composite on the current unedited, single-solid host.
        /// No Combine or facade merge. Failed features remain open for diagnosis.</summary>
        public ModelDoc2 CreateBalconyAssembly(BalconyAssemblyInstance instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (wallModel == null || mainWall == null) throw new InvalidOperationException("Create the host wall first.");
            var plan = BalconyAssemblyDefinition.Validate(instance,
                new BalconySlabHost("MainWall", wallWidth * 1000, wallHeight * 1000, wallThickness * 1000));
            var model = wallModel;
            var part = (PartDoc)model;
            if (!ReferenceEquals(application.ActiveDoc, model) || model.SketchManager.ActiveSketch != null)
                throw new InvalidOperationException("Activate the host part and exit any sketch.");
            if (SolidBodyCount(part) != 1) throw new InvalidOperationException("First assembly requires a single-solid host.");
            foreach (string name in new[] { plan.Slab.SketchName, plan.Slab.FeatureName, plan.Left.SketchName,
                plan.Left.FeatureName, plan.Right.SketchName, plan.Right.FeatureName })
                if (part.FeatureByName(name) != null) throw new InvalidOperationException("Existing/unfinished component: " + name);
            foreach (var wall in new[] { plan.Left, plan.Right })
                ValidateNoOverlap(wall.ThicknessMm / 1000, wall.HeightMm / 1000, wall.CenterXMm / 1000, wall.CenterYMm / 1000);
            var facade = (Body2)FindWallFrontFace().GetBody();
            double facadeVolume = ((double[])facade.GetMassProperties(1))[3];
            double[] facadeBox = AssemblyVertexBounds(facade);
            CreateProjectingBalconySlab(instance.Slab); // Validated implementation remains untouched.
            double expected = instance.Slab.WidthMm * instance.Slab.DepthMm * instance.Slab.ThicknessMm;
            foreach (var wall in new[] { plan.Left, plan.Right })
            {
                AddAssemblySideWall(model, plan, wall);
                expected += wall.VolumeMm3;
                VerifyAssemblyBodies(part, facadeVolume, facadeBox, expected);
            }
            var assemblyBody = GetOnlyBalconyBody(part);
            assemblyBody.Name = "BalconyAssembly_" + instance.Id;
            foreach (var wall in new[] { plan.Left, plan.Right })
                RememberBounds(wall.ThicknessMm / 1000, wall.HeightMm / 1000, wall.CenterXMm / 1000, wall.CenterYMm / 1000);
            model.ClearSelection2(true);
            model.ViewZoomtofit2();
            return model;
        }

        private void AddAssemblySideWall(ModelDoc2 model, BalconyAssemblyPlan plan, BalconySideWallPlan wall)
        {
            SketchManager sketches = model.SketchManager;
            bool sketchOpen = false;
            bool originalAddToDb = sketches.AddToDB;
            int prompt = (int)swUserPreferenceToggle_e.swInputDimValOnCreate;
            bool originalPrompt = application.GetUserPreferenceToggle(prompt);
            try
            {
                model.ClearSelection2(true);
                Face2 face = FindWallFrontFace();
                if (!((Entity)face).Select4(false, null)) throw new InvalidOperationException("Cannot select exterior facade plane.");
                sketches.InsertSketch(true);
                Sketch sketch = sketches.ActiveSketch;
                if (sketch == null) throw new InvalidOperationException("Cannot open side-wall sketch.");
                sketchOpen = true;
                Feature sketchFeature = (Feature)model.FeatureByPositionReverse(0);
                if (sketchFeature == null || sketchFeature.GetTypeName2() != "ProfileFeature")
                    throw new InvalidOperationException("Cannot locate side-wall sketch.");
                sketchFeature.Name = wall.SketchName;
                AddFaceRectangle(model, sketch, wall.ThicknessMm / 1000, wall.HeightMm / 1000,
                    wall.CenterXMm / 1000, wall.CenterYMm / 1000, "SideWallThickness", "SideWallHeight", wall.DimensionSuffix);
                var math = (MathUtility)application.GetMathUtility();
                var toModel = (MathTransform)sketch.ModelToSketchTransform.Inverse();
                var origin = TransformPoint(math, toModel, 0, 0, 0);
                var axis = TransformPoint(math, toModel, 0, 0, 1);
                double alignment = axis[2] - origin[2];
                if (double.IsNaN(alignment) || Math.Abs(Math.Abs(alignment) - 1) > 1e-8)
                    throw new InvalidOperationException("Unsupported facade orientation.");
                model.ClearSelection2(true);
                sketches.InsertSketch(true);
                sketchOpen = sketches.ActiveSketch != null;
                if (sketchOpen) throw new InvalidOperationException("Cannot exit side-wall sketch.");
                if (!sketchFeature.Select2(false, 0)) throw new InvalidOperationException("Cannot select side-wall profile.");
                // Re-resolve after every merge. Never include MainWall or auto-select touching bodies.
                Body2 target = GetOnlyBalconyBody((PartDoc)model);
                var selection = (SelectData)((SelectionMgr)model.SelectionManager).CreateSelectData();
                selection.Mark = 8;
                if (!target.Select2(true, selection)) throw new InvalidOperationException("Cannot select exclusive balcony merge target.");
                if (((SelectionMgr)model.SelectionManager).GetSelectedObjectCount2(8) != 1)
                    throw new InvalidOperationException("Exactly one explicit merge target is required.");
                Feature feature = model.FeatureManager.FeatureExtrusion3(true, false, alignment < 0,
                    (int)swEndConditions_e.swEndCondBlind, (int)swEndConditions_e.swEndCondBlind,
                    wall.DepthMm / 1000, wall.DepthMm / 1000, false, false, false, false, 0, 0,
                    false, false, false, false,
                    true, true, false, // Merge Result, selected-body scope, no auto-selection.
                    (int)swStartConditions_e.swStartSketchPlane, 0, false);
                if (feature == null) throw new InvalidOperationException("Scoped side-wall Boss-Extrude failed.");
                feature.Name = wall.FeatureName;
                var depth = (Dimension)model.Parameter("D1@" + wall.FeatureName);
                if (depth == null) throw new InvalidOperationException("Side-wall depth dimension missing.");
                depth.Name = "SideWallDepth";
                BindAssemblyWall(model, plan, wall);
                if (!model.EditRebuild3()) throw new InvalidOperationException("Side-wall rebuild failed.");
            }
            catch (Exception ex) { throw new InvalidOperationException(wall.FeatureName + ": " + ex.Message, ex); }
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

        private static void BindAssemblyWall(ModelDoc2 model, BalconyAssemblyPlan plan, BalconySideWallPlan wall)
        {
            var equations = (EquationMgr)model.GetEquationMgr();
            string slabSketch = plan.Slab.SketchName;
            string slabX = (plan.Instance.Slab.CenterXMm < 0 ? "-" : "") + Quote("CenterX_" + plan.Instance.Slab.Id + "@" + slabSketch);
            string slabY = (plan.Slab.CenterYMm < 0 ? "-" : "") + Quote("CenterY_" + plan.Instance.Slab.Id + "@" + slabSketch);
            string halfClearWidth = "(" + Quote("SlabWidth@" + slabSketch) + " - " + Quote("SideWallThickness@" + wall.SketchName) + ") / 2";
            string[] bindings = {
                BalconyAssemblyDefinition.DepthEquation(plan, wall),
                Quote("CenterX" + wall.DimensionSuffix + "@" + wall.SketchName) + " = abs(" + slabX +
                    (wall.Role == "Left" ? " - " : " + ") + halfClearWidth + ")",
                Quote("CenterY" + wall.DimensionSuffix + "@" + wall.SketchName) + " = abs(" + slabY + " + " +
                    Quote("SlabThickness@" + slabSketch) + " / 2 + " + Quote("SideWallHeight@" + wall.SketchName) + " / 2)"
            };
            foreach (string binding in bindings)
                if (equations.Add2(-1, binding, true) < 0) throw new InvalidOperationException("Cannot bind side-wall parameter: " + binding);
            // EvaluateAll has no useful success return; rebuild and actual geometry checks are authoritative.
            equations.EvaluateAll();
        }

        private static string Quote(string value) => "\"" + value + "\"";

        private Body2 GetOnlyBalconyBody(PartDoc part)
        {
            var bodies = part.GetBodies2((int)swBodyType_e.swSolidBody, false) as object[];
            if (bodies == null || bodies.Length != 2) throw new InvalidOperationException("Expected exactly MainWall plus one balcony solid.");
            Body2 facade = (Body2)FindWallFrontFace().GetBody();
            Body2 balcony = null;
            foreach (object item in bodies)
                if (!SameAssemblyComObject(item, facade))
                {
                    if (balcony != null) throw new InvalidOperationException("Ambiguous balcony body.");
                    balcony = (Body2)item;
                }
            return balcony ?? throw new InvalidOperationException("Balcony body not found.");
        }

        private static bool SameAssemblyComObject(object first, object second)
        {
            IntPtr a = Marshal.GetIUnknownForObject(first), b = IntPtr.Zero;
            try { b = Marshal.GetIUnknownForObject(second); return a == b; }
            finally { Marshal.Release(a); if (b != IntPtr.Zero) Marshal.Release(b); }
        }

        private void VerifyAssemblyBodies(PartDoc part, double originalFacadeVolume, double[] originalFacadeBounds, double expectedBalconyVolumeMm3)
        {
            Body2 balcony = GetOnlyBalconyBody(part);
            Body2 facade = (Body2)FindWallFrontFace().GetBody();
            if (Math.Abs(((double[])facade.GetMassProperties(1))[3] - originalFacadeVolume) > 1e-11)
                throw new InvalidOperationException("MainWall volume changed during balcony merge.");
            var actualBox = AssemblyVertexBounds(facade);
            for (int i = 0; i < 6; i++)
                if (Math.Abs(actualBox[i] - originalFacadeBounds[i]) > 1e-6)
                    throw new InvalidOperationException("MainWall bounds changed during balcony merge.");
            if (Math.Abs(((double[])balcony.GetMassProperties(1))[3] * 1e9 - expectedBalconyVolumeMm3) > .01 ||
                balcony.GetVertexCount() - balcony.GetEdgeCount() + balcony.GetFaceCount() != 2)
                throw new InvalidOperationException("Balcony volume/topology does not match connected prismatic geometry.");
        }

        private static double[] AssemblyVertexBounds(Body2 body)
        {
            var bounds = new[] { double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,
                double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity };
            var vertices = body.GetVertices() as object[] ?? new object[0];
            if (vertices.Length == 0) throw new InvalidOperationException("Expected solid body vertices.");
            foreach (object item in vertices)
            {
                var point = (double[])((Vertex)item).GetPoint();
                for (int i = 0; i < 3; i++) { bounds[i] = Math.Min(bounds[i], point[i]); bounds[i + 3] = Math.Max(bounds[i + 3], point[i]); }
            }
            return bounds;
        }
    }
}
