namespace RevolutionaryStuff.Crm;

public static class MailingAddressHelpers
{
    public record FreeFormAddressOptions(string SegmentSeparator = "\n", bool Force2CharacterStateNamesToUpperCase = true)
    { }

    public static readonly FreeFormAddressOptions SingleLineFreeFormAddressOptions = new(", ");
    public static readonly FreeFormAddressOptions MultiLineFreeFormAddressOptions = new(", ");
    private static readonly FreeFormAddressOptions DefaultFreeFormAddressOptions = SingleLineFreeFormAddressOptions;

    public static string CreateFreeform(this IMailingAddress address, FreeFormAddressOptions? options = null)
    {
        options ??= DefaultFreeFormAddressOptions;
        var ff = address.AddressLine1.TrimOrNull() ?? "";
        if (!string.IsNullOrWhiteSpace(address.AddressLine2))
        {
            if (ff.Length > 0)
                ff += options.SegmentSeparator;
            ff += address.AddressLine2.TrimOrNull() ?? "";
        }
        if (!string.IsNullOrWhiteSpace(address.City))
        {
            var state = address.State;
            if (state?.Length == 2 && options.Force2CharacterStateNamesToUpperCase)
                state = state.ToUpper();
            if (ff.Length > 0 && !ff.EndsWith(options.SegmentSeparator))
                ff += options.SegmentSeparator;
            ff += $"{address.City}, {address.State} {address.PostalCode}";
            if (!string.IsNullOrWhiteSpace(address.Country))
                ff += $", {address.Country}";
        }
        return ff;
    }
}
