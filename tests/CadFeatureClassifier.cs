using System;
using System.Collections.Generic;

/// <summary>Test-only feature traversal classification, including Instant3D wrappers.</summary>
internal static class CadFeatureClassifier
{
    internal static bool TryResolveOnce(string name, string typeName2, Func<string> underlyingType,
        HashSet<string> seen, out string resolvedType)
    {
        resolvedType = null;
        // An absorbed sketch or wrapped feature can be encountered through more than
        // one traversal path. Resolve and count the named feature only once.
        if (!seen.Add(name)) return false;
        resolvedType = typeName2 == "ICE" ? underlyingType() : typeName2;
        return true;
    }

    internal static bool IsBoss(string resolvedType) => resolvedType == "Boss" || resolvedType == "Extrusion";
}
