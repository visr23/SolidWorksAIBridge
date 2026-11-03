using SolidWorksAIBridge;

/// <summary>Explicit placement arithmetic for the requested integration fixture.
/// No CAD calls; V1 currently rejects this below-host slab placement.</summary>
internal static class BalconyDoorThresholdCase
{
    internal const double WallWidthMm = 180;
    internal const double WallHeightMm = 220;
    internal const double WallThicknessMm = 3;
    internal const double DoorWidthMm = 30;
    internal const double DoorHeightMm = 60;
    internal const double DoorCenterXMm = 0;
    internal const double SideOverhangMm = 13;

    internal static ProjectingBalconySlabInstance CreateInstance()
    {
        double doorThresholdModelYMm = -WallHeightMm / 2;
        double slabTopAboveHostBottomMm = doorThresholdModelYMm + WallHeightMm / 2;
        double slabWidthMm = DoorWidthMm + 2 * SideOverhangMm;
        return new ProjectingBalconySlabInstance("door-threshold", "MainWall",
            slabWidthMm, 24, 3, DoorCenterXMm, slabTopAboveHostBottomMm);
    }

    internal static BalconySlabValidationRecord Preflight()
    {
        return ProjectingBalconySlabDefinition.Validate(CreateInstance(),
            new BalconySlabHost("MainWall", WallWidthMm, WallHeightMm, WallThicknessMm));
    }
}
