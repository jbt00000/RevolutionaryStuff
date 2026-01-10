namespace RevolutionaryStuff.Core;

public static partial class WebHelpers
{
    public static class GnutellaHeaderStrings
    {
        /// <summary>
        /// Represents the 'X-Gnutella-Alternate-Location' custom header.
        /// </summary>
        public const string AltLocation = "X-Gnutella-Alternate-Location";

        // Custom header, no official documentation link available
        /// <summary>
        /// Represents the 'X-Alt' custom header.
        /// </summary>
        public const string GnutellaAltLocation = "X-Alt";

        // Custom header, no official documentation link available
        /// <summary>
        /// Represents the 'X-NAlt' custom header.
        /// </summary>
        public const string GnutellaNegativeAltLocation = "X-NAlt";

        // Custom header, no official documentation link available
        /// <summary>
        /// Represents the 'X-Gnutella-maxSlots' custom header.
        /// </summary>
        public const string MaxSlots = "X-Gnutella-maxSlots";

        // Custom header, no official documentation link available
        /// <summary>
        /// Represents the 'X-Gnutella-maxSlotsPerHost' custom header.
        /// </summary>
        public const string MaxSlotsPerHost = "X-Gnutella-maxSlotsPerHost";

        // Custom header, no official documentation link available
        /// <summary>
        /// Represents the 'X-Gnutella-Servent-ID' custom header.
        /// </summary>
        public const string ServentID = "X-Gnutella-Servent-ID";
    }
}
