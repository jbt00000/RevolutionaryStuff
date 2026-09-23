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

    public static IPostalAddress CreateFromFreeform(string freeform, string country = null)
    {
        var lines = freeform
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToArray();
        string street;
        string addressLine2 = null;
        string cityStateZip;

        if (lines.Length == 1)
        {
            var commaParts = lines[0]
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToArray();
            if (commaParts.Length < 3)
            {
                throw new ArgumentException("Freeform address must contain a street, city, state, and ZIP.");
            }

            street = commaParts[0];
            addressLine2 = commaParts.Length > 3 ? string.Join(", ", commaParts.Skip(1).Take(commaParts.Length - 3)) : null;
            var parsedCity = commaParts[^2];
            cityStateZip = $"{parsedCity}, {commaParts[^1]}";
        }
        else
        {
            street = lines[0];
            addressLine2 = lines.Length > 2 ? lines[1] : null;
            cityStateZip = lines[^1];
        }

        var cityStateZipCommaParts = cityStateZip.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var cityStateZipParts = cityStateZipCommaParts.Length > 1
            ? cityStateZipCommaParts[^1].Split(' ', StringSplitOptions.RemoveEmptyEntries)
            : cityStateZip.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (cityStateZipParts.Length < 2 || (cityStateZipCommaParts.Length == 1 && cityStateZipParts.Length < 3))
        {
            throw new ArgumentException("City/State/ZIP line must have at least three parts: city, state, and ZIP.");
        }

        var city = cityStateZipCommaParts.Length > 1
            ? cityStateZipCommaParts[0].Trim()
            : cityStateZipParts[0];
        var stateIndex = cityStateZipCommaParts.Length > 1 ? 0 : 1;
        var postalCodeStartIndex = stateIndex + 1;
        var state = cityStateZipParts[stateIndex];
        var postalCode = string.Join(" ", cityStateZipParts.Skip(postalCodeStartIndex));
        return new PostalAddress
        {
            AddressLine1 = street,
            AddressLine2 = addressLine2,
            City = city,
            State = state,
            PostalCode = postalCode,
            Country = country,
            FreeForm = freeform
        };
    }
}

