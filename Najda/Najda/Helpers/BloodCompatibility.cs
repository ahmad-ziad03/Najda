namespace Najda.Helpers;

// Red-blood-cell compatibility: which donor types may be given to a patient
// of a given (needed) type. Used for matching and for the verification
// unit-counting rule.
public static class BloodCompatibility
{
    // recipient (needed) type -> donor types that are compatible with it
    private static readonly Dictionary<string, string[]> CanReceiveFrom = new()
    {
        ["O-"] = new[] { "O-" },
        ["O+"] = new[] { "O-", "O+" },
        ["A-"] = new[] { "O-", "A-" },
        ["A+"] = new[] { "O-", "O+", "A-", "A+" },
        ["B-"] = new[] { "O-", "B-" },
        ["B+"] = new[] { "O-", "O+", "B-", "B+" },
        ["AB-"] = new[] { "O-", "A-", "B-", "AB-" },
        ["AB+"] = new[] { "O-", "O+", "A-", "A+", "B-", "B+", "AB-", "AB+" },
    };

    // The donor types a patient of `neededType` can receive.
    public static string[] CompatibleDonorTypes(string neededType)
        => CanReceiveFrom.TryGetValue(neededType, out var types) ? types : Array.Empty<string>();

    // Can a donor of `donorType` give to a patient who needs `neededType`?
    public static bool IsCompatible(string neededType, string donorType)
        => CompatibleDonorTypes(neededType).Contains(donorType);

    // The needed (recipient) types that a donor of `donorType` can serve —
    // the reverse lookup, used to find matching requests for a donor.
    public static string[] NeededTypesFor(string donorType)
        => CanReceiveFrom
            .Where(kv => kv.Value.Contains(donorType))
            .Select(kv => kv.Key)
            .ToArray();
}
