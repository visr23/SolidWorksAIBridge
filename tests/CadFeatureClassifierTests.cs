using System;
using System.Collections.Generic;

internal static partial class RegressionTests
{
    private static void FeatureClassificationTests()
    {
        foreach (string normal in new[] { "Boss", "Extrusion" })
        {
            string type = normal;
            Test("classifier: normal " + type + " without fallback", () =>
            {
                string resolved;
                if (!CadFeatureClassifier.TryResolveOnce("slab", type,
                    () => { throw new Exception("Normal feature must not query underlying type."); },
                    new HashSet<string>(), out resolved) || !CadFeatureClassifier.IsBoss(resolved))
                    throw new Exception("Normal boss was not recognized.");
            });
        }
        foreach (string underlying in new[] { "Boss", "Extrusion" })
        {
            string type = underlying;
            Test("classifier: ICE wrapping " + type, () =>
            {
                int calls = 0;
                string resolved;
                if (!CadFeatureClassifier.TryResolveOnce("slab", "ICE", () => { calls++; return type; },
                    new HashSet<string>(), out resolved) || !CadFeatureClassifier.IsBoss(resolved) || calls != 1)
                    throw new Exception("Wrapped boss was not resolved exactly once.");
            });
        }
        Test("classifier: duplicate traversal does not double-count wrapped boss", () =>
        {
            var seen = new HashSet<string>();
            int count = 0, calls = 0;
            string resolved;
            Func<string> underlying = () => { calls++; return "Boss"; };
            for (int i = 0; i < 2; i++)
                if (CadFeatureClassifier.TryResolveOnce("slab", "ICE", underlying, seen, out resolved) &&
                    CadFeatureClassifier.IsBoss(resolved)) count++;
            if (count != 1 || calls != 1) throw new Exception("Duplicate wrapped feature was counted/resolved again.");
        });
        Test("classifier: distinct normal and wrapped bosses count two", () =>
        {
            var seen = new HashSet<string>();
            int count = 0;
            string resolved;
            if (CadFeatureClassifier.TryResolveOnce("wall", "Extrusion", () => "unused", seen, out resolved) &&
                CadFeatureClassifier.IsBoss(resolved)) count++;
            if (CadFeatureClassifier.TryResolveOnce("slab", "ICE", () => "Boss", seen, out resolved) &&
                CadFeatureClassifier.IsBoss(resolved)) count++;
            if (count != 2) throw new Exception("Distinct bosses were not counted separately.");
        });
        Test("classifier: wrapped cut is not a boss", () =>
        {
            string resolved;
            CadFeatureClassifier.TryResolveOnce("cut", "ICE", () => "Cut", new HashSet<string>(), out resolved);
            if (resolved != "Cut" || CadFeatureClassifier.IsBoss(resolved)) throw new Exception("Cut misclassified.");
        });
        Test("classifier: repeated absorbed sketch counted once", () =>
        {
            var seen = new HashSet<string>();
            string resolved;
            bool first = CadFeatureClassifier.TryResolveOnce("sketch", "ProfileFeature", () => "unused", seen, out resolved);
            if (!first || resolved != "ProfileFeature" ||
                CadFeatureClassifier.TryResolveOnce("sketch", "ProfileFeature", () => "unused", seen, out resolved))
                throw new Exception("Repeated sketch was not deduplicated.");
        });
    }
}
