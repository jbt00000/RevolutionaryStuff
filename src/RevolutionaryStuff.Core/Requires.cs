using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Core
{
    public static class Requires
    {
        public static void Url(string arg, string argName)
        {
            Requires.Text(arg, argName);
            try
            {
                new Uri(arg);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"{argName} must be a valid url", ex);
            }
        }

        public static void SetMembership<T>(ICollection<T> set, string setName, T arg, string argName, bool nullInputOk = false)
        {
            if (nullInputOk && arg == null) return;
            Requires.NonNull(set, setName);
            if (!set.Contains(arg)) throw new ArgumentOutOfRangeException(argName, $"[{arg}] not a member of {setName}.");
        }

        public static void ArrayArg(IList arg, int offset, int size, string argName, int minSize=0, bool nullable=false)
        {
            if (!nullable) Requires.NonNull(arg, argName);
            if (size < minSize) throw new ArgumentException(string.Format("size must be >= {0}", minSize));
            if (offset < 0) throw new ArgumentException("offset must be >= 0");
            if (size + offset > arg.Count)
                throw new ArgumentException(string.Format("size+offset must be <= {0}.Count", argName));
        }

        public static void ListArg<T>(IList<T> arg, string argName, int minSize = 0)
        {
            Requires.NonNull(arg, argName);
            if (arg.Count < minSize) throw new ArgumentOutOfRangeException(argName, $"Length must be >0 {minSize}");
        }

        public static void Valid(IValidate arg, string argName, bool canBeNull=false)
        {
            if (arg == null && canBeNull) return;
            NonNull(arg, argName);
            arg.Validate();
        }

        public static void SingleCall(ref bool alreadyCalled)
        {
            if (alreadyCalled) throw new SingleCallException();
            alreadyCalled = true;
        }

        public static void True(bool arg, string argName, string message)
        {
            if (arg == true) return;
            throw new ArgumentOutOfRangeException(argName, message);
        }

        public static void FileExtension(string arg, string argName)
        {
            Requires.Text(arg, argName);
            if (Path.GetExtension(arg) != arg)
            {
                throw new ArgumentException("Not a valid extension", argName);
            }
        }

        public static void FileExists(string arg, string argName)
        {
            Text(arg, argName);
            if (!File.Exists(arg))
                throw new ArgumentException($"File=[{arg}] does not exist", argName);
        }

        public static void DirectoryExists(string arg, string argName)
        {
            Text(arg, argName);
            if (!Directory.Exists(arg))
                throw new ArgumentException($"Directory=[{arg}] does not exist", argName);
        }

        public static void HasData(IEnumerable arg, string argName)
        {
            NonNull(arg, argName);
            var e = arg.GetEnumerator();
            if (!e.MoveNext())
            {
                throw new ArgumentOutOfRangeException(argName, "This enumerable does not have any values");
            }
        }

        public static void False(bool arg, string argName)
        {
            if (arg) throw new ArgumentOutOfRangeException(argName, "Must be false");
        }

        public static void True(bool arg, string argName)
        {
            if (!arg) throw new ArgumentOutOfRangeException(argName, "Must be true");
        }

        public static void NonNull(object arg, string argName)
        {
            if (null == arg) throw new ArgumentNullException(argName);
        }

        public static void Null(object arg, string argName)
        {
            if (null != arg) throw new ArgumentOutOfRangeException(argName, "Must be null");
        }

        public static void ExactlyOneNonNull(params object[] items)
        {
            int cntNotNull = 0;
            foreach (var i in items)
            {
                cntNotNull += i == null ? 0 : 1;
            }
            if (cntNotNull != 1) throw new ArgumentException($"EXACTLY 1 of the {items.Length} MUST be non-null. {cntNotNull} were not null");
        }

        public static void IsType(Type testType, Type isType)
        {
            NonNull(testType, nameof(testType));
            NonNull(isType, nameof(isType));
            if (isType.GetTypeInfo().IsAssignableFrom(testType)) return;
            throw new ArgumentException(string.Format("{0} is not a {1}", testType, isType));
        }

        public static void Zero(double arg, string argName)
        {
            if (arg != 0) throw new ArgumentOutOfRangeException(argName, "Must be = 0");
        }

        public static void NonNegative(double arg, string argName)
        {
            if (arg < 0) throw new ArgumentOutOfRangeException(argName, "Must be >= 0");
        }

        public static void NonNegative(long arg, string argName)
        {
            if (arg < 0) throw new ArgumentOutOfRangeException(argName, "Must be >= 0");
        }

        public static void NonPositive(double arg, string argName)
        {
            if (arg > 0) throw new ArgumentOutOfRangeException(argName, "Must be <= 0");
        }

        public static void NonPositive(long arg, string argName)
        {
            if (arg > 0) throw new ArgumentOutOfRangeException(argName, "Must be <= 0");
        }

        public static void Positive(double arg, string argName)
        {
            if (arg <= 0) throw new ArgumentOutOfRangeException(argName, "Must be > 0");
        }

        public static void Positive(long arg, string argName)
        {
            if (arg <= 0) throw new ArgumentOutOfRangeException(argName, "Must be > 0");
        }

        public static void Negative(double arg, string argName)
        {
            if (arg >= 0) throw new ArgumentOutOfRangeException(argName, "Must be < 0");
        }

        public static void Negative(long arg, string argName)
        {
            if (arg >= 0) throw new ArgumentOutOfRangeException(argName, "Must be < 0");
        }

        public static void Match(Regex r, string arg, string argName)
        {
            Text(arg, argName, false, 0);
            if (!r.IsMatch(arg))
            {
                throw new ArgumentOutOfRangeException(argName, "does not match pattern");
            }
        }

        public static void Text(string arg, string argName, bool allowNull=false, int minLen=1, int maxLen=int.MaxValue)
        {
            if (minLen > maxLen) throw new ArgumentException("minLen cannot be > than maxLen");
            if (!allowNull && null == arg) throw new ArgumentNullException(argName);
            arg = arg ?? "";
            if (arg.Length < minLen)
            {
                throw new ArgumentException(string.Format("{0} must be >= {1} chars", argName, minLen), argName);
            }
            if (maxLen > -1 && arg.Length > maxLen)
            {
                throw new ArgumentException(string.Format("{0} must be <= {1} chars", argName, maxLen), argName);
            }
        }

        public static void EmailAddress(string arg, string argName)
        {
            Match(RegexHelpers.Common.EmailAddress, arg, argName);
        }

        public static void StreamArg(Stream stream, string argName, bool mustBeReadable, bool mustBeWriteable,
                                     bool mustBeSeekable)
        {
            Requires.NonNull(stream, argName);
            if (mustBeReadable && !stream.CanRead) throw new ArgumentException("Cannot read from this stream", argName);
            if (mustBeWriteable && !stream.CanWrite)
                throw new ArgumentException("Cannot write to this stream", argName);
            if (mustBeSeekable && !stream.CanSeek) throw new ArgumentException("Cannot seek in this stream", argName);
        }

        public static void WriteableStreamArg(Stream stream, string argName)
        {
            StreamArg(stream, argName, false, true, false);
        }

        public static void WriteableStreamArg(Stream stream, string argName, bool mustBeSeekable)
        {
            StreamArg(stream, argName, false, true, mustBeSeekable);
        }

        public static void ReadableStreamArg(Stream stream, string argName)
        {
            StreamArg(stream, argName, true, false, false);
        }

        public static void ReadableStreamArg(Stream stream, string argName, bool mustBeSeekable)
        {
            StreamArg(stream, argName, true, false, mustBeSeekable);
        }

        public static void Between(long val, string argName, long? minLength = null, long? maxLength = null)
        {
            if (val < minLength.GetValueOrDefault(long.MinValue)) throw new ArgumentOutOfRangeException(argName, $"must be >= {minLength}");
            if (val > maxLength.GetValueOrDefault(long.MaxValue)) throw new ArgumentOutOfRangeException(argName, $"must be <= {maxLength}");
        }

        public static void Buffer(byte[] buf, string argName, long? minLength=null, long? maxLength=null)
        {
            Requires.NonNull(buf, argName);
            Requires.Between(buf.Length, $"{argName}.Length", minLength.GetValueOrDefault(0), maxLength.GetValueOrDefault(int.MaxValue));
        }

        public static void PortNumber(int portNumber, string argName, bool allowZero = false) 
            => Requires.Between(portNumber, argName, allowZero ? 0 : 1, 65536);

        public static void ZeroRows(DataTable dt, string argName = null)
        {
            Requires.NonNull(dt, argName ?? nameof(dt));
            if (dt.Rows.Count > 0) throw new ArgumentException("dt must not already have any rows", nameof(dt));
        }

        public static void ZeroColumns(DataTable dt, string argName = null)
        {
            Requires.NonNull(dt, argName ?? nameof(dt));
            if (dt.Columns.Count > 0) throw new ArgumentException("dt must not already have any columns", nameof(dt));
        }

        public static void Xml(string xml, string argName)
        {
            Requires.Text(xml, argName);
            XDocument.Parse(xml);
        }
    }
}
