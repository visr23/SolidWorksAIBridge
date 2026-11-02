using System;
using SldWorks;
using SwConst;

namespace SolidWorksAIBridge
{
    public sealed class SolidWorksModeler
    {
        private readonly SldWorks.SldWorks application;
        private ModelDoc2 wallModel;
        private Feature mainWall;
        private double wallWidth;
        private double wallHeight;
        private double wallThickness;

        public SolidWorksModeler(SldWorks.SldWorks application)
        {
            this.application = application ?? throw new ArgumentNullException(nameof(application));
        }

        public ModelDoc2 CreateNewPart()
        {
            string template = application.GetUserPreferenceStringValue(
                (int)swUserPreferenceStringValue_e.swDefaultTemplatePart);
            if (string.IsNullOrWhiteSpace(template))
                throw new InvalidOperationException("Configure a default Part template in SOLIDWORKS before creating a part.");

            var model = (ModelDoc2)application.NewDocument(template, 0, 0, 0);
            if (model == null)
                throw new InvalidOperationException("SOLIDWORKS could not create a Part. Check the default Part template: " + template);
            return model;
        }

        /// <summary>Creates an unsaved, editable wall centered on the origin. All lengths are millimeters.</summary>
        public ModelDoc2 CreateWall(double widthMm, double heightMm, double thicknessMm)
        {
            double width = PositiveMeters(widthMm, nameof(widthMm));
            double height = PositiveMeters(heightMm, nameof(heightMm));
            double thickness = PositiveMeters(thicknessMm, nameof(thicknessMm));
            // A failed new wall must not leave an older wall as the opening target.
            wallModel = null;
            mainWall = null;
            ModelDoc2 model = CreateNewPart();
            SketchManager sketches = model.SketchManager;
            bool sketchOpen = false;
            bool originalAddToDb = sketches.AddToDB;
            int inputPreference = (int)swUserPreferenceToggle_e.swInputDimValOnCreate;
            bool originalDimensionPrompt = application.GetUserPreferenceToggle(inputPreference);

            try
            {
                model.ClearSelection2(true);
                if (!model.Extension.SelectByID2("Front Plane", "PLANE", 0, 0, 0, false, 0, null, 0))
                    throw new InvalidOperationException("Could not select Front Plane. Use a Part template with that plane name.");

                sketches.InsertSketch(true);
                Sketch sketch = sketches.ActiveSketch;
                if (sketch == null)
                    throw new InvalidOperationException("Could not start the wall sketch on Front Plane.");
                sketchOpen = true;

                // Retain the center rectangle's horizontal/vertical sides and construction
                // diagonals, with relation inferencing enabled at the sketch origin.
                sketches.AddToDB = false;
                var segments = sketches.CreateCenterRectangle(0, 0, 0, width / 2, height / 2, 0) as object[];
                if (segments == null || segments.Length < 4)
                    throw new InvalidOperationException("Could not create the centered wall rectangle.");

                SketchSegment horizontal = null;
                SketchSegment vertical = null;
                foreach (object item in segments)
                {
                    var segment = (SketchSegment)item;
                    if (segment.ConstructionGeometry || segment.GetType() != (int)swSketchSegments_e.swSketchLINE)
                        continue;
                    var line = (SketchLine)segment;
                    var start = (SketchPoint)line.GetStartPoint2();
                    var end = (SketchPoint)line.GetEndPoint2();
                    if (Math.Abs(start.Y - end.Y) < height * 1e-8)
                        horizontal = segment;
                    if (Math.Abs(start.X - end.X) < width * 1e-8)
                        vertical = segment;
                }

                application.SetUserPreferenceToggle(inputPreference, false);
                AddDrivingDimension(model, horizontal, "Width", width, 0, -height * 0.75);
                AddDrivingDimension(model, vertical, "Height", height, width * 0.75, 0);

                // Complete geometric constraints where possible without fixing the edges
                // or adding dimensions that compete with the editable Width and Height.
                model.ClearSelection2(true);
                int relations = (int)(
                    swSketchFullyDefineRelationType_e.swSketchFullyDefineRelationType_Horizontal |
                    swSketchFullyDefineRelationType_e.swSketchFullyDefineRelationType_Vertical |
                    swSketchFullyDefineRelationType_e.swSketchFullyDefineRelationType_Coincident |
                    swSketchFullyDefineRelationType_e.swSketchFullyDefineRelationType_Midpoint);
                // The API does not define a return status for FullyDefineSketch.
                sketches.FullyDefineSketch(true, true, relations, false,
                    0, null, 0, null, 0, 0);

                // A newly inserted sketch is the last feature in the document.
                // The configured COM Sketch interface does not expose GetFeature().
                Feature sketchFeature = (Feature)model.FeatureByPositionReverse(0);
                if (sketchFeature == null || sketchFeature.GetTypeName2() != "ProfileFeature")
                    throw new InvalidOperationException("Could not find the newly created wall sketch feature.");
                sketchFeature.Name = "WallSketch";
                model.ClearSelection2(true);
                sketches.InsertSketch(true);
                sketchOpen = false;

                // Select explicitly instead of relying on the selection left by sketch editing.
                model.ClearSelection2(true);
                if (!sketchFeature.Select2(false, 0))
                    throw new InvalidOperationException("Could not select WallSketch for extrusion.");

                Feature wall = model.FeatureManager.FeatureExtrusion2(
                    true, false, false,
                    (int)swEndConditions_e.swEndCondMidPlane,
                    (int)swEndConditions_e.swEndCondBlind,
                    thickness, thickness,
                    false, false, false, false, 0, 0,
                    false, false, false, false,
                    true, true, true,
                    (int)swStartConditions_e.swStartSketchPlane, 0, false);
                if (wall == null)
                    throw new InvalidOperationException("Could not extrude WallSketch into the wall.");
                wall.Name = "MainWall";

                if (!model.EditRebuild3())
                    throw new InvalidOperationException("The wall was created, but SOLIDWORKS reported a rebuild failure.");
                model.ClearSelection2(true);
                model.ViewZoomtofit2();
                wallModel = model;
                mainWall = wall;
                wallWidth = width;
                wallHeight = height;
                wallThickness = thickness;
                return model;
            }
            finally
            {
                // Leave a failed part open for inspection; never save or close user documents.
                try
                {
                    if (sketchOpen)
                        sketches.InsertSketch(true);
                }
                finally
                {
                    try { sketches.AddToDB = originalAddToDb; }
                    finally { application.SetUserPreferenceToggle(inputPreference, originalDimensionPrompt); }
                }
            }
        }

        /// <summary>
        /// Cuts one unsaved rectangular opening in the wall created by this modeler.
        /// X/Y are signed model coordinates, measured in millimeters from the wall origin.
        /// </summary>
        public ModelDoc2 CreateWindow(double widthMm, double heightMm, double centerXmm, double centerYmm)
        {
            double width = PositiveMeters(widthMm, nameof(widthMm));
            double height = PositiveMeters(heightMm, nameof(heightMm));
            double centerX = PositionMeters(centerXmm, nameof(centerXmm));
            double centerY = PositionMeters(centerYmm, nameof(centerYmm));
            if (wallModel == null || mainWall == null)
                throw new InvalidOperationException("Call CreateWall successfully before creating a window.");
            // Strictly inside: touching an outer edge would create a notch, not a window.
            if (width >= wallWidth || Math.Abs(centerX) >= (wallWidth - width) / 2)
                throw new ArgumentException("The window must fit strictly inside the wall's left and right boundaries. Reduce widthMm or move centerXmm toward zero.");
            if (height >= wallHeight || Math.Abs(centerY) >= (wallHeight - height) / 2)
                throw new ArgumentException("The window must fit strictly inside the wall's top and bottom boundaries. Reduce heightMm or move centerYmm toward zero.");

            ModelDoc2 model = wallModel;
            if (!ReferenceEquals(application.ActiveDoc, model))
                throw new InvalidOperationException("Activate the wall Part created by this modeler before creating a window.");
            var part = (PartDoc)model;
            if (part.FeatureByName("WindowSketch") != null || part.FeatureByName("WindowCut") != null)
                throw new InvalidOperationException("This wall already contains a window or an unfinished WindowSketch. Remove it before creating another window.");

            SketchManager sketches = model.SketchManager;
            if (sketches.ActiveSketch != null)
                throw new InvalidOperationException("Exit the active sketch before creating a window.");
            Face2 frontFace = FindWallFrontFace();
            bool sketchOpen = false;
            bool originalAddToDb = sketches.AddToDB;
            int inputPreference = (int)swUserPreferenceToggle_e.swInputDimValOnCreate;
            bool originalDimensionPrompt = application.GetUserPreferenceToggle(inputPreference);

            try
            {
                model.ClearSelection2(true);
                if (!((Entity)frontFace).Select4(false, null))
                    throw new InvalidOperationException("Could not select MainWall's front face for the window sketch.");
                sketches.InsertSketch(true);
                Sketch sketch = sketches.ActiveSketch;
                if (sketch == null)
                    throw new InvalidOperationException("Could not start a window sketch on MainWall's front face.");
                sketchOpen = true;
                Feature sketchFeature = (Feature)model.FeatureByPositionReverse(0);
                if (sketchFeature == null || sketchFeature.GetTypeName2() != "ProfileFeature")
                    throw new InvalidOperationException("Could not find the new window sketch feature.");
                sketchFeature.Name = "WindowSketch";

                var math = (MathUtility)application.GetMathUtility();
                MathTransform toSketch = sketch.ModelToSketchTransform;
                MathTransform toModel = (MathTransform)toSketch.Inverse();
                double faceZ = wallThickness / 2;
                double[] origin = TransformPoint(math, toSketch, 0, 0, faceZ);
                double[] xAxis = TransformPoint(math, toSketch, 1, 0, faceZ);
                double[] yAxis = TransformPoint(math, toSketch, 0, 1, faceZ);
                bool xIsHorizontal = Math.Abs(xAxis[0] - origin[0]) > 0.999999;
                bool yIsHorizontal = Math.Abs(yAxis[0] - origin[0]) > 0.999999;
                if (xIsHorizontal == yIsHorizontal ||
                    Math.Abs(origin[0]) > 1e-8 || Math.Abs(origin[1]) > 1e-8 || Math.Abs(origin[2]) > 1e-8)
                    throw new InvalidOperationException("The wall face sketch axes do not align with the wall origin and X/Y axes.");

                // Seed away from the axes so no inferred origin/coincident relation can
                // replace a zero-valued position dimension. The dimensions move it below.
                double seedX = (centerX < 0 ? -1 : 1) * wallWidth * 0.123;
                double seedY = (centerY < 0 ? -1 : 1) * wallHeight * 0.173;
                double[] seed = TransformPoint(math, toSketch, seedX, seedY, faceZ);
                double localWidth = xIsHorizontal ? width : height;
                double localHeight = xIsHorizontal ? height : width;
                sketches.AddToDB = true;
                var segments = sketches.CreateCenterRectangle(seed[0], seed[1], 0,
                    seed[0] + localWidth / 2, seed[1] + localHeight / 2, 0) as object[];
                sketches.AddToDB = false;
                if (segments == null || segments.Length < 6)
                    throw new InvalidOperationException("Could not create the window rectangle and its construction diagonals.");

                SketchSegment horizontal = null;
                SketchSegment vertical = null;
                SketchSegment diagonal = null;
                foreach (object item in segments)
                {
                    var segment = (SketchSegment)item;
                    if (segment.GetType() != (int)swSketchSegments_e.swSketchLINE)
                        continue;
                    var line = (SketchLine)segment;
                    var start = (SketchPoint)line.GetStartPoint2();
                    var end = (SketchPoint)line.GetEndPoint2();
                    if (segment.ConstructionGeometry)
                    {
                        if (Math.Abs(start.X - end.X) > localWidth * 0.5 &&
                            Math.Abs(start.Y - end.Y) > localHeight * 0.5)
                            diagonal = segment;
                    }
                    else if (Math.Abs(start.Y - end.Y) < localHeight * 1e-8)
                        horizontal = segment;
                    else if (Math.Abs(start.X - end.X) < localWidth * 1e-8)
                        vertical = segment;
                }
                if (horizontal == null || vertical == null || diagonal == null)
                    throw new InvalidOperationException("Could not identify the window sides and center diagonal.");

                application.SetUserPreferenceToggle(inputPreference, false);
                AddWindowSizeDimension(model, xIsHorizontal ? horizontal : vertical,
                    "WindowWidth", width, math, toModel, seed[0], seed[1] - localHeight);
                AddWindowSizeDimension(model, xIsHorizontal ? vertical : horizontal,
                    "WindowHeight", height, math, toModel, seed[0] + localWidth, seed[1]);
                Dimension xPosition = AddWindowPositionDimension(model, diagonal, xIsHorizontal,
                    "CenterX", Math.Abs(centerX), math, toModel, seed[0], seed[1] - localHeight * 1.5);
                Dimension yPosition = AddWindowPositionDimension(model, diagonal, yIsHorizontal,
                    "CenterY", Math.Abs(centerY), math, toModel, seed[0] - localWidth * 1.5, seed[1]);

                // Center rectangles carry their geometric relations. Complete any missing
                // H/V and coincident relations without fixed geometry or extra dimensions.
                model.ClearSelection2(true);
                int relations = (int)(
                    swSketchFullyDefineRelationType_e.swSketchFullyDefineRelationType_Horizontal |
                    swSketchFullyDefineRelationType_e.swSketchFullyDefineRelationType_Vertical |
                    swSketchFullyDefineRelationType_e.swSketchFullyDefineRelationType_Coincident |
                    swSketchFullyDefineRelationType_e.swSketchFullyDefineRelationType_Midpoint);
                sketches.FullyDefineSketch(true, true, relations, false, 0, null, 0, null, 0, 0);
                if (xPosition.DrivenState != (int)swDimensionDrivenState_e.swDimensionDriving ||
                    yPosition.DrivenState != (int)swDimensionDrivenState_e.swDimensionDriving)
                    throw new InvalidOperationException("Window center dimensions must remain driving dimensions.");
                VerifyWindowCenter(diagonal, math, toModel, centerX, centerY);

                model.ClearSelection2(true);
                sketches.InsertSketch(true);
                sketchOpen = false;
                model.ClearSelection2(true);
                if (!sketchFeature.Select2(false, 0))
                    throw new InvalidOperationException("Could not select WindowSketch for the cut.");

                // A face sketch's normal points outward. The default cut points into the
                // wall (opposite that normal), so a single Through All direction suffices.
                Feature cut = model.FeatureManager.FeatureCut3(
                    true, false, false,
                    (int)swEndConditions_e.swEndCondThroughAll,
                    (int)swEndConditions_e.swEndCondThroughAll,
                    wallThickness, wallThickness,
                    false, false, false, false, 0, 0,
                    false, false, false, false,
                    false, true, true, false, false, false,
                    (int)swStartConditions_e.swStartSketchPlane, 0, false);
                if (cut == null)
                    throw new InvalidOperationException("Could not create a Through All window cut into MainWall.");
                cut.Name = "WindowCut";
                if (!model.EditRebuild3())
                    throw new InvalidOperationException("The window was created, but SOLIDWORKS reported a rebuild failure.");
                model.ClearSelection2(true);
                model.ViewZoomtofit2();
                return model;
            }
            finally
            {
                // Keep failures visible for inspection, without saving or deleting features.
                try
                {
                    if (sketchOpen)
                        sketches.InsertSketch(true);
                }
                finally
                {
                    try { sketches.AddToDB = originalAddToDb; }
                    finally { application.SetUserPreferenceToggle(inputPreference, originalDimensionPrompt); }
                }
            }
        }

        private Face2 FindWallFrontFace()
        {
            var faces = mainWall.GetFaces() as object[];
            Face2 front = null;
            double largestArea = 0;
            if (faces != null)
            {
                foreach (object item in faces)
                {
                    var face = (Face2)item;
                    var surface = (Surface)face.GetSurface();
                    var normal = face.Normal as double[];
                    if (surface == null || !surface.IsPlane() || normal == null ||
                        normal.Length < 3 || normal[2] < 0.999999)
                        continue;
                    double area = face.GetArea();
                    if (area > largestArea)
                    {
                        front = face;
                        largestArea = area;
                    }
                }
            }
            if (front == null)
                throw new InvalidOperationException("Could not find MainWall's large planar front (+Z) face.");
            return front;
        }

        private static double PositionMeters(double millimeters, string parameterName)
        {
            if (double.IsNaN(millimeters) || double.IsInfinity(millimeters))
                throw new ArgumentOutOfRangeException(parameterName, "Window center positions must be finite millimeter values.");
            return UnitConverter.MillimetersToMeters(millimeters);
        }

        private static double[] TransformPoint(MathUtility math, MathTransform transform,
            double x, double y, double z)
        {
            var point = (MathPoint)math.CreatePoint(new double[] { x, y, z });
            return (double[])((MathPoint)point.MultiplyTransform(transform)).ArrayData;
        }

        private static void AddWindowSizeDimension(ModelDoc2 model, SketchSegment edge,
            string name, double meters, MathUtility math, MathTransform toModel, double textX, double textY)
        {
            model.ClearSelection2(true);
            if (!edge.Select4(false, null))
                throw new InvalidOperationException("Could not select the window edge for " + name + ".");
            double[] text = TransformPoint(math, toModel, textX, textY, 0);
            var display = (DisplayDimension)model.AddDimension2(text[0], text[1], text[2]);
            ConfigureWindowDimension(display, name, meters);
            model.ClearSelection2(true);
        }

        private static Dimension AddWindowPositionDimension(ModelDoc2 model, SketchSegment diagonal,
            bool horizontal, string name, double meters, MathUtility math, MathTransform toModel,
            double textX, double textY)
        {
            model.ClearSelection2(true);
            // Dimension the construction diagonal's midpoint: no fixed point or
            // auxiliary geometry that could move independently of the rectangle.
            if (!diagonal.Select4(false, null))
                throw new InvalidOperationException("Could not select the window center for " + name + ".");
            model.SelectMidpoint();
            if (!model.Extension.SelectByID2("Point1@Origin", "EXTSKETCHPOINT", 0, 0, 0, true, 0, null, 0))
                throw new InvalidOperationException("Could not select the Part origin for " + name + ".");
            double[] text = TransformPoint(math, toModel, textX, textY, 0);
            var display = (DisplayDimension)(horizontal
                ? model.AddHorizontalDimension2(text[0], text[1], text[2])
                : model.AddVerticalDimension2(text[0], text[1], text[2]));
            Dimension dimension = ConfigureWindowDimension(display, name, meters);
            model.ClearSelection2(true);
            return dimension;
        }

        private static Dimension ConfigureWindowDimension(DisplayDimension display, string name, double meters)
        {
            if (display == null)
                throw new InvalidOperationException("Could not add the editable " + name + " dimension.");
            var dimension = (Dimension)display.GetDimension2(0);
            if (dimension == null)
                throw new InvalidOperationException("Could not access the " + name + " dimension.");
            dimension.DrivenState = (int)swDimensionDrivenState_e.swDimensionDriving;
            dimension.Name = name;
            dimension.SystemValue = meters;
            if (dimension.DrivenState != (int)swDimensionDrivenState_e.swDimensionDriving ||
                Math.Abs(dimension.SystemValue - meters) > Math.Max(1e-10, meters * 1e-8))
                throw new InvalidOperationException("SOLIDWORKS did not accept the driving " + name + " dimension.");
            return dimension;
        }

        private static void VerifyWindowCenter(SketchSegment diagonal, MathUtility math,
            MathTransform toModel, double centerX, double centerY)
        {
            var line = (SketchLine)diagonal;
            var start = (SketchPoint)line.GetStartPoint2();
            var end = (SketchPoint)line.GetEndPoint2();
            double[] actual = TransformPoint(math, toModel,
                (start.X + end.X) / 2, (start.Y + end.Y) / 2, 0);
            if (Math.Abs(actual[0] - centerX) > 1e-8 || Math.Abs(actual[1] - centerY) > 1e-8)
                throw new InvalidOperationException("The window sketch solver did not place the center at the requested signed X/Y position.");
        }

        private static double PositiveMeters(double millimeters, string parameterName)
        {
            if (double.IsNaN(millimeters) || double.IsInfinity(millimeters) || millimeters <= 0)
                throw new ArgumentOutOfRangeException(parameterName, "Wall dimensions must be finite and greater than zero.");
            double meters = UnitConverter.MillimetersToMeters(millimeters);
            if (meters <= 0)
                throw new ArgumentOutOfRangeException(parameterName, "Wall dimension is too small.");
            return meters;
        }

        private static void AddDrivingDimension(ModelDoc2 model, SketchSegment segment,
            string name, double meters, double textX, double textY)
        {
            model.ClearSelection2(true);
            if (segment == null || !segment.Select4(false, null))
                throw new InvalidOperationException("Could not select the wall edge for " + name + ".");
            var display = (DisplayDimension)model.AddDimension2(textX, textY, 0);
            if (display == null)
                throw new InvalidOperationException("Could not add the driving " + name + " dimension.");
            var dimension = (Dimension)display.GetDimension2(0);
            dimension.DrivenState = (int)swDimensionDrivenState_e.swDimensionDriving;
            if (dimension.DrivenState != (int)swDimensionDrivenState_e.swDimensionDriving)
                throw new InvalidOperationException(name + " could not be made a driving dimension.");
            dimension.Name = name;
            dimension.SystemValue = meters;
            if (Math.Abs(dimension.SystemValue - meters) > meters * 1e-8)
                throw new InvalidOperationException("SOLIDWORKS did not accept the " + name + " dimension value.");
            model.ClearSelection2(true);
        }
    }
}
