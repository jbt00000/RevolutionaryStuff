﻿using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using RevolutionaryStuff.Core.EncoderDecoders;

namespace RevolutionaryStuff.Core.Crypto;

public sealed class Hash
{
    public static readonly IDictionary<string, Func<HashAlgorithm>> HashAlgorithmCreationMap = new Dictionary<string, Func<HashAlgorithm>>(Comparers.CaseInsensitiveStringComparer);

    public static class CommonHashAlgorithmNames
    {
        public const string Sha512 = "sha512";
        public const string Sha384 = "sha384";
        public const string Sha256 = "sha256";
        public const string Sha1 = "sha1";
        public const string Md5 = "md5";
        //public const string Tiger = "tiger";
        public static string Default = Sha1;
    }

    private static readonly Regex HashNameHashVersionExpr = new(@"^([A-Z]+)(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public static void RegisterHashAlgorithmCreator(Func<HashAlgorithm> creator, params string[] alternateNames)
    {
        ArgumentNullException.ThrowIfNull(creator);
        var ha = creator();
        ArgumentNullException.ThrowIfNull(ha);
        var t = ha.GetType();
        HashAlgorithmCreationMap[t.FullName] = creator;
        var name = t.Name;
        HashAlgorithmCreationMap[name] = creator;
        if (name.EndsWith("Managed"))
        {
            name = name.LeftOf("Managed");
            HashAlgorithmCreationMap[name] = creator;
        }
        var m = HashNameHashVersionExpr.Match(name);
        if (m.Success)
        {
            HashAlgorithmCreationMap[$"{m.Groups[1].Value}-{m.Groups[2].Value}"] = creator;
        }
        if (alternateNames != null)
        {
            foreach (var alternateName in alternateNames)
            {
                HashAlgorithmCreationMap[alternateName] = creator;
            }
        }
    }

    static Hash()
    {
        RegisterHashAlgorithmCreator(SHA1.Create, CommonHashAlgorithmNames.Sha1);
        RegisterHashAlgorithmCreator(SHA256.Create, CommonHashAlgorithmNames.Sha256);
        RegisterHashAlgorithmCreator(SHA384.Create, CommonHashAlgorithmNames.Sha384);
        RegisterHashAlgorithmCreator(SHA512.Create, CommonHashAlgorithmNames.Sha512);
        RegisterHashAlgorithmCreator(MD5.Create, CommonHashAlgorithmNames.Md5);
    }

    public static string GetHashAlgorithmName(HashAlgorithm ha)
    {
        return ha?.GetType().Name;
    }

    public static bool IsHashAlgorithmInstalled(string hashName)
    {
        return hashName != null && HashAlgorithmCreationMap.ContainsKey(hashName);
    }

    public static HashAlgorithm CreateHashAlgorithm(string hashName)
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
        return !HashAlgorithmCreationMap.ContainsKey(hashName) ? throw new NotSupportedException() : HashAlgorithmCreationMap[hashName]();
    }

    private static readonly IDictionary<string, string> NameByNameLowerMap = new Dictionary<string, string>(Comparers.CaseInsensitiveStringComparer);
    public static readonly Hash[] NoHashes = new Hash[0];

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

    public string NameDashBase64
        => $"{HashName}-{Base64.Encode(Data)}";

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
    {
        return Urn.GetHashCode();
    }

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
        return new Hash(hashAlgorithmName, ha.ComputeHash(st));
    }

    private static Hash Compute(Stream st, HashAlgorithm ha)
    {
        Requires.ReadableStreamArg(st);
        ArgumentNullException.ThrowIfNull(ha);
        var hashAlgorithmName = GetHashAlgorithmName(ha);
        return new Hash(hashAlgorithmName, ha.ComputeHash(st));
    }

    public static Hash Compute(byte[] buf, string hashAlgorithmName = null)
    {
        ArgumentNullException.ThrowIfNull(buf);
        hashAlgorithmName ??= CommonHashAlgorithmNames.Default;
        var ha = CreateHashAlgorithm(hashAlgorithmName);
        return new Hash(hashAlgorithmName, ha.ComputeHash(buf));
    }

    private static Hash Compute(byte[] buf, HashAlgorithm ha)
    {
        ArgumentNullException.ThrowIfNull(buf, "buf");
        ArgumentNullException.ThrowIfNull(ha);
        var hashAlgorithmName = GetHashAlgorithmName(ha);
        return new Hash(hashAlgorithmName, ha.ComputeHash(buf));
    }
}
