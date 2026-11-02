using System;
using System.Collections.Generic;
using System.Globalization;
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
        private bool hasWindow;
        private string latestWindowSketchName;
        private string latestWindowCutName;
        private int nextRowIndex = 1;
        // Bounds in model meters for features created by this modeler.
        private readonly List<OpeningBounds> occupiedBounds = new List<OpeningBounds>();

        private sealed class OpeningBounds
        {
            public double Left, Right, Bottom, Top;
        }

        private double windowWidth;
        private double windowHeight;
        private double windowCenterX;
        private double windowCenterY;

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
            hasWindow = false;
            latestWindowSketchName = null;
            latestWindowCutName = null;
            nextRowIndex = 1;
            occupiedBounds.Clear();
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
            return CreateWindowCore(width, height, centerX, centerY, "WindowSketch", "WindowCut");
        }

        // Internal lengths are already converted to meters by the public entry points.
        private ModelDoc2 CreateWindowCore(double width, double height, double centerX, double centerY,
            string sketchName, string cutName)
        {
            if (wallModel == null || mainWall == null)
                throw new InvalidOperationException("Call CreateWall successfully before creating a window.");
            // Strictly inside: touching an outer edge would create a notch, not a window.
            if (width >= wallWidth || Math.Abs(centerX) >= (wallWidth - width) / 2)
                throw new ArgumentException("The window must fit strictly inside the wall's left and right boundaries. Reduce widthMm or move centerXmm toward zero.");
            if (height >= wallHeight || Math.Abs(centerY) >= (wallHeight - height) / 2)
                throw new ArgumentException("The window must fit strictly inside the wall's top and bottom boundaries. Reduce heightMm or move centerYmm toward zero.");

            ValidateNoOverlap(width, height, centerX, centerY);
            ModelDoc2 model = wallModel;
            if (!ReferenceEquals(application.ActiveDoc, model))
                throw new InvalidOperationException("Activate the wall Part created by this modeler before creating a window.");
            var part = (PartDoc)model;
            if (part.FeatureByName(sketchName) != null || part.FeatureByName(cutName) != null)
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
                sketchFeature.Name = sketchName;

                AddFaceRectangle(model, sketch, width, height, centerX, centerY,
                    "WindowWidth", "WindowHeight", "");

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
                cut.Name = cutName;
                if (!model.EditRebuild3())
                    throw new InvalidOperationException("The window was created, but SOLIDWORKS reported a rebuild failure.");
                model.ClearSelection2(true);
                model.ViewZoomtofit2();
                windowWidth = width;
                windowHeight = height;
                windowCenterX = centerX;
                windowCenterY = centerY;
                hasWindow = true;
                latestWindowSketchName = sketchName;
                latestWindowCutName = cutName;
                RememberBounds(width, height, centerX, centerY);
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

        /// <summary>
        /// Adds an exterior sill below the most recently created window.
        /// All lengths are millimeters; the sill top meets the opening bottom.
        /// </summary>
        public ModelDoc2 CreateWindowSill(double widthMm, double thicknessMm, double projectionMm)
        {
            double width = PositiveMeters(widthMm, nameof(widthMm));
            double height = PositiveMeters(thicknessMm, nameof(thicknessMm));
            double projection = PositiveMeters(projectionMm, nameof(projectionMm));
            return CreateWindowSillCore(width, height, projection, "WindowSillSketch", "WindowSill");
        }

        private ModelDoc2 CreateWindowSillCore(double width, double height, double projection,
            string sketchName, string sillName)
        {
            if (wallModel == null || mainWall == null || !hasWindow)
                throw new InvalidOperationException("Call CreateWindow successfully before creating a sill.");
            double centerX = windowCenterX;
            double bottom = windowCenterY - windowHeight / 2;
            double centerY = bottom - height / 2;
            if (width > wallWidth || Math.Abs(centerX) > (wallWidth - width) / 2)
                throw new ArgumentException("The centered sill must fit within the wall width.", "widthMm");
            // Require contact with the wall below the opening across the full thickness.
            if (bottom - height < -wallHeight / 2)
                throw new ArgumentException("The sill thickness must fit below the window within the wall height.", "thicknessMm");

            ValidateNoOverlap(width, height, centerX, centerY);
            ModelDoc2 model = wallModel;
            if (!ReferenceEquals(application.ActiveDoc, model))
                throw new InvalidOperationException("Activate the wall Part before creating a sill.");
            var part = (PartDoc)model;
            if (part.FeatureByName(latestWindowSketchName) == null || part.FeatureByName(latestWindowCutName) == null)
                throw new InvalidOperationException("The most recent window features no longer exist.");
            if (part.FeatureByName(sketchName) != null || part.FeatureByName(sillName) != null)
                throw new InvalidOperationException("This wall already contains a sill or an unfinished WindowSillSketch.");

            SketchManager sketches = model.SketchManager;
            if (sketches.ActiveSketch != null)
                throw new InvalidOperationException("Exit the active sketch before creating a sill.");
            Face2 frontFace = FindWallFrontFace();
            bool sketchOpen = false;
            bool originalAddToDb = sketches.AddToDB;
            int inputPreference = (int)swUserPreferenceToggle_e.swInputDimValOnCreate;
            bool originalDimensionPrompt = application.GetUserPreferenceToggle(inputPreference);

            try
            {
                model.ClearSelection2(true);
                if (!((Entity)frontFace).Select4(false, null))
                    throw new InvalidOperationException("Could not select MainWall's front face for the sill sketch.");
                sketches.InsertSketch(true);
                Sketch sketch = sketches.ActiveSketch;
                if (sketch == null)
                    throw new InvalidOperationException("Could not start a sill sketch on MainWall's front face.");
                sketchOpen = true;
                Feature sketchFeature = (Feature)model.FeatureByPositionReverse(0);
                if (sketchFeature == null || sketchFeature.GetTypeName2() != "ProfileFeature")
                    throw new InvalidOperationException("Could not find the new sill sketch feature.");
                sketchFeature.Name = sketchName;

                AddFaceRectangle(model, sketch, width, height, centerX, centerY,
                    "SillWidth", "SillThickness", "");

                model.ClearSelection2(true);
                sketches.InsertSketch(true);
                sketchOpen = false;
                model.ClearSelection2(true);
                if (!sketchFeature.Select2(false, 0))
                    throw new InvalidOperationException("Could not select WindowSillSketch for the boss extrusion.");

                // A face sketch's normal is +Z: extrude outward from the front
                // face only, without extending through the wall thickness.
                Feature sill = model.FeatureManager.FeatureExtrusion2(
                    true, false, false,
                    (int)swEndConditions_e.swEndCondBlind,
                    (int)swEndConditions_e.swEndCondBlind,
                    projection, projection,
                    false, false, false, false, 0, 0,
                    false, false, false, false,
                    true, true, true,
                    (int)swStartConditions_e.swStartSketchPlane, 0, false);
                if (sill == null)
                    throw new InvalidOperationException("Could not create the outward sill boss on MainWall.");
                sill.Name = sillName;
                if (!model.EditRebuild3())
                    throw new InvalidOperationException("The sill was created, but SOLIDWORKS reported a rebuild failure.");
                model.ClearSelection2(true);
                model.ViewZoomtofit2();
                RememberBounds(width, height, centerX, centerY);
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

        /// <summary>
        /// Creates a centered horizontal row. Spacing is the clear gap between openings.
        /// All public lengths and positions are millimeters.
        /// </summary>
        public ModelDoc2 CreateWindowRow(int count, double windowWidthMm, double windowHeightMm,
            double spacingMm, double centerXmm, double centerYmm, bool createSills,
            double sillThicknessMm, double sillProjectionMm)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be at least one.");
            double width = PositiveMeters(windowWidthMm, nameof(windowWidthMm));
            double height = PositiveMeters(windowHeightMm, nameof(windowHeightMm));
            double spacing = PositionMeters(spacingMm, nameof(spacingMm));
            if (spacingMm < 0)
                throw new ArgumentOutOfRangeException(nameof(spacingMm), "Clear spacing must be nonnegative.");
            double centerX = PositionMeters(centerXmm, nameof(centerXmm));
            double centerY = PositionMeters(centerYmm, nameof(centerYmm));
            double sillThickness = createSills ? PositiveMeters(sillThicknessMm, nameof(sillThicknessMm)) : 0;
            double sillProjection = createSills ? PositiveMeters(sillProjectionMm, nameof(sillProjectionMm)) : 0;
            if (wallModel == null || mainWall == null)
                throw new InvalidOperationException("Call CreateWall successfully before creating a window row.");
            double rowWidth = count * width + (count - 1.0) * spacing;
            if (double.IsInfinity(rowWidth) || rowWidth >= wallWidth ||
                Math.Abs(centerX) >= (wallWidth - rowWidth) / 2)
                throw new ArgumentException("The entire row must fit strictly inside the wall horizontally.");
            if (height >= wallHeight || Math.Abs(centerY) >= (wallHeight - height) / 2)
                throw new ArgumentException("All windows must fit strictly inside the wall vertically.");
            if (createSills && centerY - height / 2 - sillThickness < -wallHeight / 2)
                throw new ArgumentException("The sill thickness must fit below the windows within the wall height.", nameof(sillThicknessMm));
            if (!ReferenceEquals(application.ActiveDoc, wallModel))
                throw new InvalidOperationException("Activate the wall Part before creating a window row.");
            if (wallModel.SketchManager.ActiveSketch != null)
                throw new InvalidOperationException("Exit the active sketch before creating a window row.");
            if (nextRowIndex == int.MaxValue)
                throw new InvalidOperationException("The row feature numbering limit has been reached.");

            double pitch = width + spacing;
            double firstCenter = centerX - (count - 1.0) * pitch / 2;
            double sillY = centerY - height / 2 - sillThickness / 2;
            string suffix = RowSuffix(nextRowIndex);
            var part = (PartDoc)wallModel;
            foreach (string prefix in new[] { "WindowRowSketch", "WindowRowCut", "WindowRowSillSketch", "WindowRowSills" })
                if (part.FeatureByName(prefix + suffix) != null)
                    throw new InvalidOperationException("A row feature already exists: " + prefix + suffix);
            // Validate the complete layout before modifying the document.
            for (int i = 0; i < count; i++)
            {
                double x = firstCenter + i * pitch;
                ValidateNoOverlap(width, height, x, centerY);
                if (createSills)
                    ValidateNoOverlap(width, sillThickness, x, sillY);
            }

            // Reserve the row number even on failure, leaving partial features visible.
            nextRowIndex++;
            CreateValidatedRow(count, width, height, firstCenter, pitch, centerY,
                createSills, sillThickness, sillProjection,
                "WindowRowSketch" + suffix, "WindowRowCut" + suffix,
                "WindowRowSillSketch" + suffix, "WindowRowSills" + suffix);
            wallModel.ClearSelection2(true);
            wallModel.ViewZoomtofit2();
            return wallModel;
        }

        /// <summary>
        /// Creates separately editable floors, numbered from bottom to top.
        /// Gaps are clear spaces between windows; all public lengths are millimeters.
        /// The center describes the window grid, with sills extending below each row.
        /// </summary>
        public ModelDoc2 CreateWindowGrid(int floorCount, int windowsPerFloor,
            double windowWidthMm, double windowHeightMm, double horizontalGapMm, double verticalGapMm,
            double centerXMm, double centerYMm, bool createSills,
            double sillThicknessMm, double sillProjectionMm)
        {
            if (floorCount < 1)
                throw new ArgumentOutOfRangeException(nameof(floorCount), "Floor count must be greater than zero.");
            if (windowsPerFloor < 1)
                throw new ArgumentOutOfRangeException(nameof(windowsPerFloor), "Windows per floor must be greater than zero.");
            double width = PositiveMeters(windowWidthMm, nameof(windowWidthMm));
            double height = PositiveMeters(windowHeightMm, nameof(windowHeightMm));
            double horizontalGap = NonnegativeMeters(horizontalGapMm, nameof(horizontalGapMm));
            double verticalGap = NonnegativeMeters(verticalGapMm, nameof(verticalGapMm));
            double centerX = PositionMeters(centerXMm, nameof(centerXMm));
            double centerY = PositionMeters(centerYMm, nameof(centerYMm));
            double sillThickness = createSills ? PositiveMeters(sillThicknessMm, nameof(sillThicknessMm)) : 0;
            double sillProjection = createSills ? PositiveMeters(sillProjectionMm, nameof(sillProjectionMm)) : 0;
            if (wallModel == null || mainWall == null)
                throw new InvalidOperationException("Call CreateWall successfully before creating a window grid.");

            double gridWidth = windowsPerFloor * width + (windowsPerFloor - 1.0) * horizontalGap;
            double gridHeight = floorCount * height + (floorCount - 1.0) * verticalGap;
            if (double.IsInfinity(gridWidth) || gridWidth >= wallWidth ||
                Math.Abs(centerX) >= (wallWidth - gridWidth) / 2)
                throw new ArgumentException("The complete grid must fit strictly inside MainWall horizontally.");
            if (double.IsInfinity(gridHeight) || gridHeight >= wallHeight ||
                Math.Abs(centerY) >= (wallHeight - gridHeight) / 2)
                throw new ArgumentException("The complete grid must fit strictly inside MainWall vertically.");
            if (createSills && centerY - gridHeight / 2 - sillThickness < -wallHeight / 2)
                throw new ArgumentException("The lowest floor's sill extends below MainWall.", nameof(sillThicknessMm));
            // With aligned columns, an upper sill must fit in the gap above the lower opening.
            // Positive dimensions and nonnegative gaps also guarantee that new windows
            // do not overlap each other horizontally or vertically.
            if (createSills && floorCount > 1 && sillThickness > verticalGap)
                throw new ArgumentException("Sill thickness must not exceed the vertical clear gap between floors.", nameof(sillThicknessMm));
            if (!ReferenceEquals(application.ActiveDoc, wallModel))
                throw new InvalidOperationException("Activate the wall Part before creating a window grid.");
            if (wallModel.SketchManager.ActiveSketch != null)
                throw new InvalidOperationException("Exit the active sketch before creating a window grid.");

            double horizontalPitch = width + horizontalGap;
            double verticalPitch = height + verticalGap;
            double firstX = centerX - (windowsPerFloor - 1.0) * horizontalPitch / 2;
            double firstY = centerY - (floorCount - 1.0) * verticalPitch / 2;
            var part = (PartDoc)wallModel;
            // Preflight ALL floors before the first sketch or feature is inserted.
            for (int floor = 0; floor < floorCount; floor++)
            {
                string prefix = "Floor" + RowSuffix(floor + 1);
                foreach (string ending in new[] { "_WindowSketch", "_WindowCut", "_WindowSillSketch", "_WindowSills" })
                    if (part.FeatureByName(prefix + ending) != null)
                        throw new InvalidOperationException("A facade feature already exists: " + prefix + ending);
                double y = firstY + floor * verticalPitch;
                for (int column = 0; column < windowsPerFloor; column++)
                {
                    double x = firstX + column * horizontalPitch;
                    ValidateNoOverlap(width, height, x, y);
                    if (createSills)
                        ValidateNoOverlap(width, sillThickness, x, y - height / 2 - sillThickness / 2);
                }
            }

            for (int floor = 0; floor < floorCount; floor++)
            {
                string prefix = "Floor" + RowSuffix(floor + 1);
                CreateValidatedRow(windowsPerFloor, width, height, firstX, horizontalPitch,
                    firstY + floor * verticalPitch, createSills, sillThickness, sillProjection,
                    prefix + "_WindowSketch", prefix + "_WindowCut",
                    prefix + "_WindowSillSketch", prefix + "_WindowSills", prefix);
            }
            wallModel.ClearSelection2(true);
            wallModel.ViewZoomtofit2();
            return wallModel;
        }

        private static double NonnegativeMeters(double millimeters, string parameterName)
        {
            if (double.IsNaN(millimeters) || double.IsInfinity(millimeters) || millimeters < 0)
                throw new ArgumentOutOfRangeException(parameterName, "Clear gaps must be finite and nonnegative.");
            return UnitConverter.MillimetersToMeters(millimeters);
        }

        // Called only after the complete row or grid passes preflight validation.
        private void CreateValidatedRow(int count, double width, double height, double firstCenter,
            double pitch, double centerY, bool createSills, double sillThickness, double sillProjection,
            string windowSketchName, string windowCutName, string sillSketchName, string sillsName,
            string rowContext = null)
        {
            double sillY = centerY - height / 2 - sillThickness / 2;
            CreateRowFeature(count, width, height, firstCenter, pitch, centerY, 0,
                windowSketchName, windowCutName, false, rowContext);
            for (int i = 0; i < count; i++)
                RememberBounds(width, height, firstCenter + i * pitch, centerY);
            // Preserve the most-recent-window contract for the standalone sill command.
            windowWidth = width;
            windowHeight = height;
            windowCenterX = firstCenter + (count - 1.0) * pitch;
            windowCenterY = centerY;
            latestWindowSketchName = windowSketchName;
            latestWindowCutName = windowCutName;
            hasWindow = true;

            if (createSills)
            {
                CreateRowFeature(count, width, sillThickness, firstCenter, pitch, sillY, sillProjection,
                    sillSketchName, sillsName, true, rowContext);
                for (int i = 0; i < count; i++)
                    RememberBounds(width, sillThickness, firstCenter + i * pitch, sillY);
            }
        }

        // Include context even if COM fails while reading initial state or restoring it.
        private void CreateRowFeature(int count, double width, double height, double firstCenter,
            double pitch, double centerY, double projection, string sketchName, string featureName, bool additive,
            string rowContext = null)
        {
            try
            {
                CreateRowFeatureCore(count, width, height, firstCenter, pitch, centerY, projection,
                    sketchName, featureName, additive, rowContext);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException((rowContext ?? featureName) + " Windows_01.." +
                    count.ToString("D2", CultureInfo.InvariantCulture) + ": failed while building " + featureName +
                    ". " + ex.Message, ex);
            }
        }

        // One face sketch with all row contours, followed by exactly one cut or boss.
        private void CreateRowFeatureCore(int count, double width, double height, double firstCenter,
            double pitch, double centerY, double projection, string sketchName, string featureName, bool additive,
            string rowContext = null)
        {
            string context = (rowContext ?? featureName) + " Windows_01.." + count.ToString("D2", CultureInfo.InvariantCulture);
            string operation = "preparing " + sketchName;
            ModelDoc2 model = wallModel;
            SketchManager sketches = model.SketchManager;
            bool sketchOpen = false;
            bool originalAddToDb = sketches.AddToDB;
            int inputPreference = (int)swUserPreferenceToggle_e.swInputDimValOnCreate;
            bool originalDimensionPrompt = application.GetUserPreferenceToggle(inputPreference);
            try
            {
                operation = "selecting the wall front face for " + sketchName;
                model.ClearSelection2(true);
                if (!((Entity)FindWallFrontFace()).Select4(false, null))
                    throw new InvalidOperationException("Could not select MainWall's front face for " + sketchName + ".");
                operation = "opening " + sketchName;
                sketches.InsertSketch(true);
                Sketch sketch = sketches.ActiveSketch;
                if (sketch == null)
                    throw new InvalidOperationException("Could not start " + sketchName + ".");
                sketchOpen = true;
                Feature sketchFeature = (Feature)model.FeatureByPositionReverse(0);
                if (sketchFeature == null || sketchFeature.GetTypeName2() != "ProfileFeature")
                    throw new InvalidOperationException("Could not find " + sketchName + ".");
                operation = "naming " + sketchName;
                sketchFeature.Name = sketchName;
                var rectangles = new List<RowRectangleGeometry>();
                operation = "disabling dimension prompts";
                application.SetUserPreferenceToggle(inputPreference, false);
                for (int i = 0; i < count; i++)
                {
                    string dimensionSuffix = RowSuffix(i + 1);
                    context = (rowContext ?? featureName) + " Window" + dimensionSuffix;
                    operation = "creating rectangle";
                    rectangles.Add(AddRowRectangle(model, sketch, width, height, firstCenter + i * pitch, centerY,
                        (additive ? "SillWidth" : "WindowWidth") + dimensionSuffix,
                        (additive ? "SillThickness" : "WindowHeight") + dimensionSuffix,
                        pitch == width && i > 0, pitch == width && i < count - 1,
                        (rowContext ?? featureName) + " Window" + dimensionSuffix, additive));
                }
                // Recheck earlier rectangles after all subsequent dimensions/relations.
                operation = "verifying completed sketch geometry";
                foreach (RowRectangleGeometry rectangle in rectangles)
                {
                    context = rectangle.Context;
                    VerifyRowRectangle(rectangle);
                }
                context = (rowContext ?? featureName) + " Windows_01.." + count.ToString("D2", CultureInfo.InvariantCulture);
                operation = "closing " + sketchName;
                model.ClearSelection2(true);
                sketches.InsertSketch(true);
                sketchOpen = false;
                model.ClearSelection2(true);
                operation = "selecting " + sketchName + " for " + featureName;
                if (!sketchFeature.Select2(false, 0))
                    throw new InvalidOperationException("Could not select " + sketchName + ".");

                operation = additive ? "extruding outward sills" : "creating the Through All window cut";
                Feature feature;
                if (additive)
                {
                    // Blind extrusion along the front face's +Z normal; merge with MainWall.
                    feature = model.FeatureManager.FeatureExtrusion2(
                        true, false, false,
                        (int)swEndConditions_e.swEndCondBlind,
                        (int)swEndConditions_e.swEndCondBlind,
                        projection, projection,
                        false, false, false, false, 0, 0,
                        false, false, false, false,
                        true, true, true,
                        (int)swStartConditions_e.swStartSketchPlane, 0, false);
                }
                else
                {
                    feature = model.FeatureManager.FeatureCut3(
                        true, false, false,
                        (int)swEndConditions_e.swEndCondThroughAll,
                        (int)swEndConditions_e.swEndCondThroughAll,
                        wallThickness, wallThickness,
                        false, false, false, false, 0, 0,
                        false, false, false, false,
                        false, true, true, false, false, false,
                        (int)swStartConditions_e.swStartSketchPlane, 0, false);
                }
                if (feature == null)
                    throw new InvalidOperationException("Could not create " + featureName + ".");
                operation = "naming " + featureName;
                feature.Name = featureName;
                operation = "rebuilding " + featureName;
                if (!model.EditRebuild3())
                    throw new InvalidOperationException("SOLIDWORKS reported a rebuild failure for " + featureName + ".");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(context + ": failed while " + operation + ". " + ex.Message, ex);
            }
            finally
            {
                try
                {
                    operation = "closing the active sketch during cleanup";
                    model.ClearSelection2(true);
                    if (sketchOpen)
                        sketches.InsertSketch(true);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(context + ": failed while " + operation + ". " + ex.Message, ex);
                }
                finally
                {
                    model.ClearSelection2(true);
                    try { sketches.AddToDB = originalAddToDb; }
                    finally { application.SetUserPreferenceToggle(inputPreference, originalDimensionPrompt); }
                }
            }
        }

        private sealed class RowRectangleGeometry
        {
            public SketchSegment[] Edges;
            public double[][] Corners;
            public string Context;
        }

        // Shared rows use only explicitly created lines at their final coordinates.
        // Edges/corners belong to this rectangle; no entity lookup or auto-definition.
        private RowRectangleGeometry AddRowRectangle(ModelDoc2 model, Sketch sketch,
            double width, double height, double centerX, double centerY,
            string widthName, string heightName,
            bool internalLeft, bool internalRight, string context, bool sill)
        {
            SketchManager sketches = model.SketchManager;
            bool originalAddToDb = sketches.AddToDB;
            string operation = "calculating " + (sill ? "sill" : "window") + " corners";
            try
            {
                model.ClearSelection2(true);
                var math = (MathUtility)application.GetMathUtility();
                MathTransform toSketch = sketch.ModelToSketchTransform;
                MathTransform toModel = (MathTransform)toSketch.Inverse();
                double z = wallThickness / 2;
                double left = centerX - width / 2;
                double right = centerX + width / 2;
                double bottom = centerY - height / 2;
                double top = centerY + height / 2;
                var rectangle = new RowRectangleGeometry
                {
                    Context = context,
                    Corners = new[] {
                        TransformPoint(math, toSketch, left, bottom, z),
                        TransformPoint(math, toSketch, right, bottom, z),
                        TransformPoint(math, toSketch, right, top, z),
                        TransformPoint(math, toSketch, left, top, z)
                    },
                    Edges = new SketchSegment[4]
                };
                string[] sideNames = { "bottom", "right", "top", "left" };
                sketches.AddToDB = true; // Disable inference/merging with other rectangles.
                for (int i = 0; i < 4; i++)
                {
                    operation = "creating " + sideNames[i] + " line";
                    double[] start = rectangle.Corners[i];
                    double[] end = rectangle.Corners[(i + 1) % 4];
                    model.ClearSelection2(true);
                    rectangle.Edges[i] = sketches.CreateLine(start[0], start[1], 0, end[0], end[1], 0);
                    if (rectangle.Edges[i] == null)
                        throw new InvalidOperationException("SOLIDWORKS returned no sketch line.");
                    rectangle.Edges[i].ConstructionGeometry = false;
                }
                sketches.AddToDB = false;
                for (int i = 0; i < 4; i++)
                {
                    operation = "adding " + sideNames[i] + " orientation relation";
                    double[] start = rectangle.Corners[i];
                    double[] end = rectangle.Corners[(i + 1) % 4];
                    model.ClearSelection2(true);
                    if (!rectangle.Edges[i].Select4(false, null))
                        throw new InvalidOperationException("Could not select the current side.");
                    // A face transform can swap/reverse sketch X/Y; test local coordinates.
                    if (Math.Abs(start[2]) > 1e-8 || Math.Abs(end[2]) > 1e-8 ||
                        (Math.Abs(start[0] - end[0]) > 1e-10 && Math.Abs(start[1] - end[1]) > 1e-10))
                        throw new InvalidOperationException("The front face sketch axes do not align with the wall.");
                    model.SketchAddConstraints(Math.Abs(start[1] - end[1]) < 1e-10 ? "sgHORIZONTAL" : "sgVERTICAL");
                    model.ClearSelection2(true);

                    operation = "joining " + sideNames[i] + " corner within the current rectangle";
                    var line = (SketchLine)rectangle.Edges[i];
                    var next = (SketchLine)rectangle.Edges[(i + 1) % 4];
                    var endPoint = (SketchPoint)line.GetEndPoint2();
                    var nextStart = (SketchPoint)next.GetStartPoint2();
                    if (!endPoint.Select4(false, null) || !nextStart.Select4(true, null))
                        throw new InvalidOperationException("Could not select the current corner's endpoints.");
                    model.SketchAddConstraints("sgCOINCIDENT");
                    model.ClearSelection2(true);
                }
                VerifyRowRectangle(rectangle);

                operation = "adding width dimension";
                AddRowSizeDimension(model, rectangle.Edges[0], widthName, width,
                    math, toModel, rectangle.Corners[0][0], rectangle.Corners[0][1] - height);
                VerifyRowRectangle(rectangle);
                operation = "adding " + (sill ? "thickness" : "height") + " dimension";
                AddRowSizeDimension(model, rectangle.Edges[1], heightName, height,
                    math, toModel, rectangle.Corners[1][0] + width, rectangle.Corners[1][1]);
                VerifyRowRectangle(rectangle);

                // For zero gap, shared interior sides must not be duplicate profile edges.
                operation = "marking shared interior sides as construction geometry";
                if (internalRight) rectangle.Edges[1].ConstructionGeometry = true;
                if (internalLeft) rectangle.Edges[3].ConstructionGeometry = true;
                return rectangle;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(context + ": failed while " + operation + ". " + ex.Message, ex);
            }
            finally
            {
                try { model.ClearSelection2(true); }
                finally { sketches.AddToDB = originalAddToDb; }
            }
        }

        private static void AddRowSizeDimension(ModelDoc2 model, SketchSegment edge, string name,
            double expectedMeters, MathUtility math, MathTransform toModel, double textX, double textY)
        {
            model.ClearSelection2(true);
            try
            {
                if (!edge.Select4(false, null))
                    throw new InvalidOperationException("Could not select the current rectangle side.");
                double[] text = TransformPoint(math, toModel, textX, textY, 0);
                var display = (DisplayDimension)model.AddDimension2(text[0], text[1], text[2]);
                if (display == null)
                    throw new InvalidOperationException("Could not add " + name + ".");
                var dimension = (Dimension)display.GetDimension2(0);
                if (dimension == null)
                    throw new InvalidOperationException("Could not access " + name + ".");
                dimension.Name = name;
                // Accept the measured value: never set SystemValue to reposition geometry.
                if (dimension.DrivenState != (int)swDimensionDrivenState_e.swDimensionDriving ||
                    Math.Abs(dimension.SystemValue - expectedMeters) > 1e-8)
                    throw new InvalidOperationException("The editable dimension does not match its calculated side length.");
            }
            finally { model.ClearSelection2(true); }
        }

        private static void VerifyRowRectangle(RowRectangleGeometry rectangle)
        {
            for (int i = 0; i < 4; i++)
            {
                var line = (SketchLine)rectangle.Edges[i];
                var start = (SketchPoint)line.GetStartPoint2();
                var end = (SketchPoint)line.GetEndPoint2();
                double[] expectedStart = rectangle.Corners[i];
                double[] expectedEnd = rectangle.Corners[(i + 1) % 4];
                if (Math.Abs(start.X - expectedStart[0]) > 1e-8 || Math.Abs(start.Y - expectedStart[1]) > 1e-8 ||
                    Math.Abs(end.X - expectedEnd[0]) > 1e-8 || Math.Abs(end.Y - expectedEnd[1]) > 1e-8)
                    throw new InvalidOperationException(rectangle.Context + ": rectangle moved from its calculated corner coordinates.");
            }
        }

        // Standalone commands retain their existing rectangle implementation.
        private void AddFaceRectangle(ModelDoc2 model, Sketch sketch, double width, double height,
            double centerX, double centerY, string widthName, string heightName, string dimensionSuffix,
            bool internalLeft = false, bool internalRight = false)
        {
            model.ClearSelection2(true);
            try
            {
                SketchManager sketches = model.SketchManager;
                int inputPreference = (int)swUserPreferenceToggle_e.swInputDimValOnCreate;
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
                model.ClearSelection2(true);
                sketches.AddToDB = true;
                var segments = sketches.CreateCenterRectangle(seed[0], seed[1], 0,
                    seed[0] + localWidth / 2, seed[1] + localHeight / 2, 0) as object[];
                sketches.AddToDB = false;
                model.ClearSelection2(true);
                if (segments == null || segments.Length < 6)
                    throw new InvalidOperationException("Could not create the window rectangle and its construction diagonals.");

                VerifyRectangleGeometry(segments, seed[0], seed[1], localWidth, localHeight);
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

                // Establish relations BEFORE driving dimensions move the seed geometry.
                // Never run the relation solver over the rest of this shared sketch.
                model.ClearSelection2(true);
                try
                {
                    foreach (object item in segments)
                        if (!((SketchSegment)item).Select4(true, null))
                            throw new InvalidOperationException("Could not select the current rectangle for relations.");
                    int relations = (int)(
                        swSketchFullyDefineRelationType_e.swSketchFullyDefineRelationType_Horizontal |
                        swSketchFullyDefineRelationType_e.swSketchFullyDefineRelationType_Vertical |
                        swSketchFullyDefineRelationType_e.swSketchFullyDefineRelationType_Coincident |
                        swSketchFullyDefineRelationType_e.swSketchFullyDefineRelationType_Midpoint);
                    sketches.FullyDefineSketch(false, true, relations, false, 0, null, 0, null, 0, 0);
                }
                finally
                {
                    model.ClearSelection2(true);
                }

                application.SetUserPreferenceToggle(inputPreference, false);
                AddWindowSizeDimension(model, xIsHorizontal ? horizontal : vertical,
                    widthName, width, math, toModel, seed[0], seed[1] - localHeight);
                AddWindowSizeDimension(model, xIsHorizontal ? vertical : horizontal,
                    heightName, height, math, toModel, seed[0] + localWidth, seed[1]);
                Dimension xPosition = AddWindowPositionDimension(model, diagonal, xIsHorizontal,
                    "CenterX" + dimensionSuffix, Math.Abs(centerX), math, toModel, seed[0], seed[1] - localHeight * 1.5);
                Dimension yPosition = AddWindowPositionDimension(model, diagonal, yIsHorizontal,
                    "CenterY" + dimensionSuffix, Math.Abs(centerY), math, toModel, seed[0] - localWidth * 1.5, seed[1]);

                if (xPosition.DrivenState != (int)swDimensionDrivenState_e.swDimensionDriving ||
                    yPosition.DrivenState != (int)swDimensionDrivenState_e.swDimensionDriving)
                    throw new InvalidOperationException("Window center dimensions must remain driving dimensions.");
                VerifyWindowCenter(diagonal, math, toModel, centerX, centerY);
                double[] expectedCenter = TransformPoint(math, toSketch, centerX, centerY, faceZ);
                VerifyRectangleGeometry(segments, expectedCenter[0], expectedCenter[1], localWidth, localHeight);
                // At zero clear gap, adjacent rectangles share an edge. Keep those internal
                // sides as construction geometry so the cut/boss has a clean outer contour.
                if (internalLeft || internalRight)
                {
                    foreach (object item in segments)
                    {
                        var segment = (SketchSegment)item;
                        if (segment.ConstructionGeometry || segment.GetType() != (int)swSketchSegments_e.swSketchLINE)
                            continue;
                        var line = (SketchLine)segment;
                        var start = (SketchPoint)line.GetStartPoint2();
                        var end = (SketchPoint)line.GetEndPoint2();
                        double[] startModel = TransformPoint(math, toModel, start.X, start.Y, 0);
                        double[] endModel = TransformPoint(math, toModel, end.X, end.Y, 0);
                        if (Math.Abs(startModel[0] - endModel[0]) < 1e-10 &&
                            ((internalLeft && Math.Abs(startModel[0] - (centerX - width / 2)) < 1e-10) ||
                             (internalRight && Math.Abs(startModel[0] - (centerX + width / 2)) < 1e-10)))
                            segment.ConstructionGeometry = true;
                    }
                }
            }
            finally
            {
                model.ClearSelection2(true);
            }
        }

        // Inspect only handles returned for this rectangle, never global sketch segment order.
        // A correct midpoint alone cannot detect a stretched/cross-window diagonal.
        private static void VerifyRectangleGeometry(object[] segments, double centerX, double centerY,
            double width, double height)
        {
            const double tolerance = 1e-8;
            int sides = 0;
            int diagonals = 0;
            int sideMask = 0;
            int diagonalMask = 0;
            foreach (object item in segments)
            {
                var segment = item as SketchSegment;
                if (segment == null || segment.GetType() != (int)swSketchSegments_e.swSketchLINE)
                    throw new InvalidOperationException("The current rectangle returned unexpected sketch geometry.");
                var line = (SketchLine)segment;
                var start = (SketchPoint)line.GetStartPoint2();
                var end = (SketchPoint)line.GetEndPoint2();
                int startCorner = RectangleCorner(start, centerX, centerY, width, height, tolerance);
                int endCorner = RectangleCorner(end, centerX, centerY, width, height, tolerance);
                int changedAxes = startCorner ^ endCorner;
                if (segment.ConstructionGeometry && changedAxes == 3)
                {
                    diagonals++;
                    diagonalMask |= 1 << Math.Min(startCorner, endCorner);
                }
                else if (!segment.ConstructionGeometry && (changedAxes == 1 || changedAxes == 2))
                {
                    sides++;
                    // Unique bit for each undirected corner pair.
                    sideMask |= 1 << (Math.Min(startCorner, endCorner) * 4 + Math.Max(startCorner, endCorner));
                }
                else
                    throw new InvalidOperationException("A rectangle side or construction diagonal has become distorted.");
            }
            const int expectedSides = (1 << 1) | (1 << 2) | (1 << 7) | (1 << 11);
            if (sides != 4 || diagonals != 2 || sideMask != expectedSides || diagonalMask != 3)
                throw new InvalidOperationException("Expected four independent rectangle sides and two internal construction diagonals.");
        }

        private static int RectangleCorner(SketchPoint point, double centerX, double centerY,
            double width, double height, double tolerance)
        {
            double x = point.X - centerX;
            double y = point.Y - centerY;
            if (Math.Abs(Math.Abs(x) - width / 2) > tolerance ||
                Math.Abs(Math.Abs(y) - height / 2) > tolerance)
                throw new InvalidOperationException("A sketch entity extends outside its own rectangle.");
            return (x > 0 ? 1 : 0) | (y > 0 ? 2 : 0);
        }

        private static string RowSuffix(int index)
        {
            return "_" + index.ToString("D2", CultureInfo.InvariantCulture);
        }

        private void ValidateNoOverlap(double width, double height, double centerX, double centerY)
        {
            // Permit shared boundaries (including the explicitly supported zero gap),
            // but reject positive-area intersections with existing windows or sills.
            const double tolerance = 1e-10;
            foreach (OpeningBounds bounds in occupiedBounds)
                if (Math.Min(centerX + width / 2, bounds.Right) - Math.Max(centerX - width / 2, bounds.Left) > tolerance &&
                    Math.Min(centerY + height / 2, bounds.Top) - Math.Max(centerY - height / 2, bounds.Bottom) > tolerance)
                    throw new ArgumentException("The requested window or sill overlaps an existing window or sill created by this modeler.");
        }

        private void RememberBounds(double width, double height, double centerX, double centerY)
        {
            occupiedBounds.Add(new OpeningBounds
            {
                Left = centerX - width / 2, Right = centerX + width / 2,
                Bottom = centerY - height / 2, Top = centerY + height / 2
            });
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
            try
            {
                if (!edge.Select4(false, null))
                    throw new InvalidOperationException("Could not select the window edge for " + name + ".");
                double[] text = TransformPoint(math, toModel, textX, textY, 0);
                var display = (DisplayDimension)model.AddDimension2(text[0], text[1], text[2]);
                ConfigureWindowDimension(display, name, meters);
            }
            finally
            {
                model.ClearSelection2(true);
            }
        }

        private static Dimension AddWindowPositionDimension(ModelDoc2 model, SketchSegment diagonal,
            bool horizontal, string name, double meters, MathUtility math, MathTransform toModel,
            double textX, double textY)
        {
            model.ClearSelection2(true);
            try
            {
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
                return dimension;
            }
            finally
            {
                model.ClearSelection2(true);
            }
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
                throw new ArgumentOutOfRangeException(parameterName, "Dimensions must be finite and greater than zero.");
            double meters = UnitConverter.MillimetersToMeters(millimeters);
            if (meters <= 0)
                throw new ArgumentOutOfRangeException(parameterName, "Dimension is too small.");
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
