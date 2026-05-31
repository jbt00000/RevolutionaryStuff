namespace RevolutionaryStuff.Core.ApplicationParts.PostalAddress;

public static class PostalAddressHelpers
{
    public static string CreateFreeform(this IPostalAddress address)
    {
        var ff = address.AddressLine1;
        if (!string.IsNullOrWhiteSpace(address.AddressLine2))
        {
            ff += $"\n{address.AddressLine2}";
        }
        if (!string.IsNullOrWhiteSpace(address.City))
        {
            var state = address.State;
            if (state?.Length == 2)
            {
                state = state.ToUpper();
            }
            ff += $"\n{address.City}, {address.State} {address.PostalCode}";
        }
        return ff;
    }
}

