namespace RevolutionaryStuff.Crm;

public static class MailingAddressHelpers
{
    public record FreeFormAddressOptions(string SegmentSeparator = "\n", bool Force2CharacterStateNamesToUpperCase = true)
    { }

    public static readonly FreeFormAddressOptions SingleLineFreeFormAddressOptions = new(", ");
    public static readonly FreeFormAddressOptions MultiLineFreeFormAddressOptions = new(", ");
    private static readonly FreeFormAddressOptions DefaultFreeFormAddressOptions = SingleLineFreeFormAddressOptions;

    extension(IMailingAddress source)
    {
        public string CreateFreeform(FreeFormAddressOptions? options = null)
        {
            options ??= DefaultFreeFormAddressOptions;
            var ff = source.AddressLine1.TrimOrNull() ?? "";
            if (!string.IsNullOrWhiteSpace(source.AddressLine2))
            {
                if (ff.Length > 0)
                    ff += options.SegmentSeparator;
                ff += source.AddressLine2.TrimOrNull() ?? "";
            }
            if (!string.IsNullOrWhiteSpace(source.City))
            {
                var state = source.State;
                if (state?.Length == 2 && options.Force2CharacterStateNamesToUpperCase)
                    state = state.ToUpper();
                if (ff.Length > 0 && !ff.EndsWith(options.SegmentSeparator))
                    ff += options.SegmentSeparator;
                ff += $"{source.City}, {source.State} {source.PostalCode}";
                if (!string.IsNullOrWhiteSpace(source.Country))
                    ff += $", {source.Country}";
            }
            return ff;
        }
    }
}
