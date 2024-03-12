using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using RevolutionaryStuff.Core.EncoderDecoders;
using System.IO.Hashing;

namespace RevolutionaryStuff.Core.Crypto;

public sealed class Hash
{
    public delegate byte[] HasherDelegate(Stream st);

    private static readonly IDictionary<string, HasherDelegate> HasherMap = new Dictionary<string, HasherDelegate>(Comparers.CaseInsensitiveStringComparer);

    public static class CommonHashAlgorithmNames
    {
        public static readonly IEnumerable<string> All = [.. CryptographicHashAlgorithms.All, .. NonCryptographicHashAlgorithms.All];
        //public const string Tiger = "tiger";
        public static string Default = CryptographicHashAlgorithms.Sha1;

        public static class CryptographicHashAlgorithms
        {
            public static readonly IEnumerable<string> All = [Sha1, Md5, .. Sha2.All];//, .. Sha3.All];

            public const string Sha1 = "sha1";

            public static class Sha2
            {
                public static readonly IEnumerable<string> All = [Sha2_512, Sha2_384, Sha2_256];
                public const string Sha2_512 = "sha512";
                public const string Sha2_384 = "sha384";
                public const string Sha2_256 = "sha256";
            }

            //always seems to return not supported
            internal static class Sha3
            {
                public static readonly IEnumerable<string> All = [Sha3_256, Sha3_384, Sha3_512];
                public const string Sha3_256 = "SHA3-256";
                public const string Sha3_384 = "SHA3-384";
                public const string Sha3_512 = "SHA3-512";
            }

            public const string Md5 = "md5";
        }

        public static class NonCryptographicHashAlgorithms
        {
            public static readonly IEnumerable<string> All = [.. XxHash.All, .. Crc.All];

            public static class XxHash
            {
                public static readonly IEnumerable<string> All = [XxHash32, XxHash64, XxHash128];
                public const string XxHash32 = "xxh32";
                public const string XxHash64 = "xxh64";
                public const string XxHash128 = "xxh128";
            }

            public static class Crc
            {
                public static readonly IEnumerable<string> All = [Crc32];//, Crc64];
                public const string Crc32 = "crc32";
                //public const string Crc64 = "crc64"; doesn't seem to work
            }
        }
    }

    public static void RegisterHashAlgorithmCreator(Func<NonCryptographicHashAlgorithm> creator, params string[] alternateNames)
    {
        Type hType;

        //Not all hash algorithms are supported on all platforms
        try
        {
            var z = creator();
            hType = z.GetType();
            Stuff.Dispose(z);
        }
        catch (CryptographicException)
        {
            return;
        }

        RegisterHasher(
            st =>
            {
                var h = creator();
                h.Append(st);
                var buf = h.GetHashAndReset();
                Stuff.Dispose(h);
                return buf;
            },
            [.. alternateNames, hType.Name, hType.FullName]);
    }

    public static void RegisterHashAlgorithmCreator(Func<HashAlgorithm> creator, params string[] alternateNames)
    {
        Type hType;

        //Not all hash algorithms are supported on all platforms
        try
        {
            using (var z = creator())
            {
                hType = z.GetType();
            }
        }
        catch (CryptographicException)
        {
            return;
        }

        RegisterHasher(
            st =>
            {
                using var ha = creator();
                return ha.ComputeHash(st);
            },
            [.. alternateNames, hType.Name, hType.FullName]);
    }

    private static void RegisterHasher(HasherDelegate hasher, params string[] names)
    {
        ArgumentNullException.ThrowIfNull(hasher);

        foreach (var name in names)
        {
            HasherMap[name] = hasher;
            if (name.EndsWith("Managed"))
            {
                HasherMap[name.LeftOf("Managed")] = hasher;
            }
        }
    }

    static Hash()
    {
        RegisterHashAlgorithmCreator(SHA1.Create, CommonHashAlgorithmNames.CryptographicHashAlgorithms.Sha1);
        RegisterHashAlgorithmCreator(MD5.Create, CommonHashAlgorithmNames.CryptographicHashAlgorithms.Md5);

        RegisterHashAlgorithmCreator(SHA256.Create, CommonHashAlgorithmNames.CryptographicHashAlgorithms.Sha2.Sha2_256);
        RegisterHashAlgorithmCreator(SHA384.Create, CommonHashAlgorithmNames.CryptographicHashAlgorithms.Sha2.Sha2_384);
        RegisterHashAlgorithmCreator(SHA512.Create, CommonHashAlgorithmNames.CryptographicHashAlgorithms.Sha2.Sha2_512);

        if (SHA3_256.IsSupported)
        {
            RegisterHashAlgorithmCreator(SHA3_256.Create, CommonHashAlgorithmNames.CryptographicHashAlgorithms.Sha3.Sha3_256);
        }
        if (SHA3_384.IsSupported)
        {
            RegisterHashAlgorithmCreator(SHA3_384.Create, CommonHashAlgorithmNames.CryptographicHashAlgorithms.Sha3.Sha3_384);
        }
        if (SHA3_512.IsSupported)
        {
            RegisterHashAlgorithmCreator(SHA3_512.Create, CommonHashAlgorithmNames.CryptographicHashAlgorithms.Sha3.Sha3_512);
        }

        RegisterHashAlgorithmCreator(() => new XxHash32(), CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash32);
        RegisterHashAlgorithmCreator(() => new XxHash64(), CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash64);
        RegisterHashAlgorithmCreator(() => new XxHash128(), CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.XxHash.XxHash128);
        RegisterHashAlgorithmCreator(() => new Crc32(), CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.Crc.Crc32);
        //RegisterHashAlgorithmCreator(() => new Crc64(), CommonHashAlgorithmNames.NonCryptographicHashAlgorithms.Crc.Crc64);
    }

    public static string GetHashAlgorithmName(HashAlgorithm ha)
        => ha?.GetType().Name;

    public static bool IsHashAlgorithmInstalled(string hashName)
        => hashName != null && HasherMap.ContainsKey(hashName);

    public static HasherDelegate CreateHashAlgorithm(string hashName)
    {
        Requires.Text(hashName);
        if (hashName.IndexOf(':') > -1)
        {
            var parts = hashName.Split(':');
            if (parts.Length == 2 && parts[0].ToLower() == "tree")
            {
                throw new NotSupportedException("MerkleHashTree not yet supported...");
                //                    return new MerkleHashTree(CreateDelegate, parts[1]);
            }
        }
        return HasherMap.ContainsKey(hashName) ? HasherMap[hashName] : throw new NotSupportedException($"{hashName} has not been registered");
    }

    public static readonly Hash[] NoHashes = [];

    private static readonly Regex UrnTypeExpression = new("[^:]+:([^:]+):.+", RegexOptions.Compiled);
    private static readonly Regex UrnExpr = new(@"urn:(.*):(.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public byte[] Data;

    /// <summary>
    /// The name of the hash algorithm
    /// </summary>
    public string HashName;

    #region Constructors

    public Hash()
    {
    }

    public Hash(string hashAlgorithmName, byte[] hash)
    {
        Requires.Text(hashAlgorithmName);
        Requires.Buffer(hash, nameof(hash), 1);
        HashName = hashAlgorithmName;
        Data = hash;
    }

    #endregion

    public override string ToString()
        => $"{GetType().Name} {Urn}";

    public static string CreateUrn(string hashName, byte[] hashBytes)
        => $"urn:{hashName}:{Base32.Encode(hashBytes)}";

    /// <summary>
    /// e.g., [{SHA1}Mzk4YTI5OWFjMWViMTEwZmE2Yzg5OWZhYjg1Y2EyMGI5NzQyOGI=]
    /// </summary>
    public string NameBracketsBase64
        => $"{{{HashName}}}{Base64.Encode(Data)}";

    /// <summary>
    /// e.g., [SHA1 - Mzk4YTI5OWFjMWViMTEwZmE2Yzg5OWZhYjg1Y2EyMGI5NzQyOGI=]
    /// </summary>
    public string NameDashBase64
        => $"{HashName} - {Base64.Encode(Data)}";

    /// <summary>
    /// e.g., [SHA-256:3e23e8160039594a33894f6564e1b1348bbd7a008b1a92347225b65f1fc8f431]
    /// </summary>
    public string NameColonBase16
        => $"{HashName}:{Base16.Encode(Data, 0, Data.Length, false)}";

    public string Urn
        => $"urn:{HashName}:{DataHuman}";

    /// <remarks>http://www.ietf.org/mail-archive/web/urn-nid/current/msg00043.html</remarks>
    public string ThiemannHashUrn
        => $"urn:hash::{HashName}:{DataHuman}";

    public string DataHuman
    {
        [DebuggerStepThrough]
        get { return Base32.Encode(Data); }
        [DebuggerStepThrough]
        set { Data = Base32.Decode(value); }
    }

    public static string GetUrnType(string urn)
        => UrnTypeExpression.GetGroupValue(urn);

    public static string GetBitprint(string[] urns)
    {
        try
        {
            string tiger = null;
            string sha1 = null;
            for (var x = urns.Length - 1; x >= 0; --x)
            {
                var urn = urns[x];
                var type = GetUrnType(urn);
                switch (type)
                {
                    case "bitprint":
                        return urn;
                    case "sha1":
                        sha1 = urn;
                        break;
                    case "tiger":
                        tiger = urn;
                        break;
                }
            }
            if (tiger != null && sha1 != null)
            {
                string bitprint;
                var s = Parse(sha1);
                var t = Parse(tiger);
                bitprint = $"urn:bitprint:{s.DataHuman}.{t.DataHuman}";
                return bitprint;
            }
        }
        catch (Exception)
        {
        }
        return null;
    }

    public override int GetHashCode()
        => Urn.GetHashCode();

    public override bool Equals(object o)
    {
        if (o is null or not Hash) return false;
        var that = (Hash)o;
        return this == that || Urn == that.Urn;
    }

    public static bool operator ==(Hash a, Hash b)
    {
        // If both are null, or both are same instance, return true.
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        // If one is null, but not both, return false.
        return a is not null && b is not null && a.Urn == b.Urn;
    }

    public static bool operator !=(Hash a, Hash b)
    {
        return !(a == b);
    }

    public static Hash Parse(string urn)
    {
        var h = new Hash();
        var parts = UrnExpr.GetGroupValues(urn);
        if (2 != parts.Count) throw new FormatException($"Invalid urn format = {urn}");
        h.HashName = parts[0];
        h.Data = Base32.Decode(parts[1]);
        return h;
    }

    public static Hash Compute(Stream st, string hashAlgorithmName = null)
    {
        Requires.ReadableStreamArg(st);
        hashAlgorithmName ??= CommonHashAlgorithmNames.Default;
        var ha = CreateHashAlgorithm(hashAlgorithmName);
        return new Hash(hashAlgorithmName, ha(st));
    }

    public static Hash Compute(byte[] buf, string hashAlgorithmName = null)
    {
        using var st = new MemoryStream(buf);
        return Compute(st, hashAlgorithmName);
    }
}
