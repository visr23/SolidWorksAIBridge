using System;
using SldWorks;
using SwConst;

namespace SolidWorksAIBridge
{
    public sealed partial class SolidWorksModeler
    {
        /// <summary>Creates one unmerged slab on the current MainWall exterior face.
        /// Inputs are resolved miniature mm; elevation is slab top above the wall bottom.
        /// Host wall must not have been manually resized/reoriented since CreateWall.</summary>
        public ModelDoc2 CreateProjectingBalconySlab(ProjectingBalconySlabInstance instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (wallModel == null || mainWall == null)
                throw new InvalidOperationException("Call CreateWall successfully before creating a balcony slab.");
            var record = ProjectingBalconySlabDefinition.Validate(instance,
                new BalconySlabHost("MainWall", wallWidth * 1000, wallHeight * 1000, wallThickness * 1000));
            ModelDoc2 model = wallModel;
            if (!ReferenceEquals(application.ActiveDoc, model))
                throw new InvalidOperationException("Activate the host wall Part before creating a balcony slab.");
            var part = (PartDoc)model;
            if (part.FeatureByName("MainWall") == null || part.FeatureByName("WallSketch") == null)
                throw new InvalidOperationException("The host wall dependency no longer exists.");
            if (part.FeatureByName(record.SketchName) != null || part.FeatureByName(record.FeatureName) != null)
                throw new InvalidOperationException("A slab with this ID or its unfinished sketch already exists.");
            SketchManager sketches = model.SketchManager;
            if (sketches.ActiveSketch != null)
                throw new InvalidOperationException("Exit the active sketch before creating a balcony slab.");
            double width = UnitConverter.MillimetersToMeters(instance.WidthMm);
            double thickness = UnitConverter.MillimetersToMeters(instance.ThicknessMm);
            double depth = UnitConverter.MillimetersToMeters(instance.DepthMm);
            double centerX = UnitConverter.MillimetersToMeters(instance.CenterXMm);
            double centerY = UnitConverter.MillimetersToMeters(record.CenterYMm);
            // Conservative facade footprint: no positive-area overlap with known apertures,
            // sills or prior slabs. Shared boundaries (e.g. a door threshold) are allowed.
            ValidateNoOverlap(width, thickness, centerX, centerY);
            Face2 face = FindWallFrontFace();
            int bodiesBefore = SolidBodyCount(part);
            bool sketchOpen = false;
            bool originalAddToDb = sketches.AddToDB;
            int preference = (int)swUserPreferenceToggle_e.swInputDimValOnCreate;
            bool originalPrompt = application.GetUserPreferenceToggle(preference);
            try
            {
                model.ClearSelection2(true);
                if (!((Entity)face).Select4(false, null))
                    throw new InvalidOperationException("Could not select the host exterior face.");
                sketches.InsertSketch(true);
                Sketch sketch = sketches.ActiveSketch;
                if (sketch == null) throw new InvalidOperationException("Could not start balcony slab sketch.");
                sketchOpen = true;
                Feature sketchFeature = (Feature)model.FeatureByPositionReverse(0);
                if (sketchFeature == null || sketchFeature.GetTypeName2() != "ProfileFeature")
                    throw new InvalidOperationException("Could not locate balcony slab sketch feature.");
                sketchFeature.Name = record.SketchName;
                AddFaceRectangle(model, sketch, width, thickness, centerX, centerY,
                    "SlabWidth", "SlabThickness", "_" + instance.Id);

                var math = (MathUtility)application.GetMathUtility();
                MathTransform toModel = (MathTransform)sketch.ModelToSketchTransform.Inverse();
                double[] origin = TransformPoint(math, toModel, 0, 0, 0);
                double[] axis = TransformPoint(math, toModel, 0, 0, 1);
                // Bosses follow sketch +Z by default. Never infer outward from screen orientation.
                double alignment = axis[2] - origin[2];
                if (Math.Abs(Math.Abs(alignment) - 1) > 1e-8)
                    throw new InvalidOperationException("V1 slab requires the host facade normal to align with model Z.");
                model.ClearSelection2(true);
                sketches.InsertSketch(true);
                sketchOpen = sketches.ActiveSketch != null;
                if (sketchOpen) throw new InvalidOperationException("Could not exit balcony slab sketch.");
                if (!sketchFeature.Select2(false, 0))
                    throw new InvalidOperationException("Could not select balcony slab sketch for extrusion.");
                Feature slab = model.FeatureManager.FeatureExtrusion2(
                    true, false, alignment < 0,
                    (int)swEndConditions_e.swEndCondBlind, (int)swEndConditions_e.swEndCondBlind,
                    depth, depth, false, false, false, false, 0, 0,
                    false, false, false, false,
                    false, false, false, // Merge=false: preserve a separate manufacturing body.
                    (int)swStartConditions_e.swStartSketchPlane, 0, false);
                if (slab == null) throw new InvalidOperationException("Could not extrude the separate balcony slab.");
                slab.Name = record.FeatureName;
                // Give the editable extrusion depth a role-based name.
                var depthDimension = (Dimension)model.Parameter("D1@" + record.FeatureName);
                if (depthDimension == null) throw new InvalidOperationException("Could not locate slab depth dimension.");
                depthDimension.Name = "SlabDepth";
                if (!model.EditRebuild3()) throw new InvalidOperationException("Balcony slab rebuild failed.");
                if (SolidBodyCount(part) != bodiesBefore + 1)
                    throw new InvalidOperationException("Expected exactly one new, separate solid slab body.");
                RememberBounds(width, thickness, centerX, centerY);
                model.ClearSelection2(true);
                model.ViewZoomtofit2();
                return model;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(record.FeatureName + ": " + ex.Message, ex);
            }
            finally
            {
                try { if (sketchOpen) sketches.InsertSketch(true); }
                finally
                {
                    model.ClearSelection2(true);
                    try { sketches.AddToDB = originalAddToDb; }
                    finally { application.SetUserPreferenceToggle(preference, originalPrompt); }
                }
            }
        }

        private static int SolidBodyCount(PartDoc part)
        {
            var bodies = part.GetBodies2((int)swBodyType_e.swSolidBody, false) as object[];
            return bodies == null ? 0 : bodies.Length;
        }
    }
}
