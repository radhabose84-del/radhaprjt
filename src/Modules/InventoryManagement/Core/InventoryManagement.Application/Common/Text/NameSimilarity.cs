// InventoryManagement.Application/Common/Text/NameSimilarity.cs
using System.Text.RegularExpressions;

namespace InventoryManagement.Application.Common.Text
{
    public static class NameSimilarity
    {
        // Keep letters+digits, collapse whitespace, lowercase.
        public static string Normalize(string? s)
        {
            s ??= string.Empty;
            s = s.ToLowerInvariant();
            // replace all non-alphanumeric with a single space
            s = Regex.Replace(s, "[^a-z0-9]+", " ").Trim();
            return Regex.Replace(s, "\\s+", " ");
        }

        // Split into alphanumeric tokens (keeps numbers like 18, 40, 14, 256 etc.)
        public static List<string> Tokens(string s)
            => Regex.Matches(Normalize(s), "[a-z]+|[0-9]+(?:\\.[0-9]+)?")
                    .Select(m => m.Value).ToList();

        // Salient differences that should immediately mark names as "different enough"
        public static bool HasSalientDifference(List<string> a, List<string> b)
        {
            var numsA = a.Where(t => char.IsDigit(t[0])).ToHashSet();
            var numsB = b.Where(t => char.IsDigit(t[0])).ToHashSet();
            if (!numsA.SetEquals(numsB)) return true; // different numeric tokens → allow

            // Optional: color/size keys (extend as you like)
            string[] colorWords = ["black","white","blue","red","green","yellow","grey","gray","silver","gold","pink","purple"];
            var colorsA = a.Intersect(colorWords).ToHashSet();
            var colorsB = b.Intersect(colorWords).ToHashSet();
            if (!colorsA.SetEquals(colorsB)) return true; // different colors → allow

            return false;
        }

        // Jaccard similarity on token sets (helps reduce false positives)
        public static double Jaccard(List<string> a, List<string> b)
        {
            var sa = a.ToHashSet();
            var sb = b.ToHashSet();
            var inter = sa.Intersect(sb).Count();
            var union = sa.Union(sb).Count();
            return union == 0 ? 0 : (double)inter / union;
        }

        // Standard JW (same as you had)
        public static double JaroWinkler(string s1, string s2)
        {
            // ––– implementation omitted for brevity –––
            // keep your existing implementation here
            throw new NotImplementedException();
        }

        // Main predicate used by the validator
        public static bool IsTooSimilarToAny(string candidate, IEnumerable<string> existingNames,
                                             double jwThreshold = 0.965, double jaccardThreshold = 0.60)
        {
            var ct = Tokens(candidate);

            foreach (var ex in existingNames)
            {
                var et = Tokens(ex);

                // If there’s a salient difference, skip similarity fail
                if (HasSalientDifference(ct, et)) continue;

                var jw = JaroWinkler(string.Join(' ', ct), string.Join(' ', et));
                var jac = Jaccard(ct, et);

                if (jw >= jwThreshold && jac >= jaccardThreshold)
                    return true; // too similar
            }
            return false;
        }
    }
}
