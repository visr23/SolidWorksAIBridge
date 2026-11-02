using System;

namespace SolidWorksAIBridge
{
    public static class UnitConverter
    {
        public static double MillimetersToMeters(double millimeters)
        {
            if (double.IsNaN(millimeters) || double.IsInfinity(millimeters))
                throw new ArgumentOutOfRangeException(nameof(millimeters), "Length must be finite.");
            return millimeters / 1000.0;
        }
    }
}
