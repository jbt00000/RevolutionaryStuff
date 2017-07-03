using System;
using System.Collections.Generic;

namespace RevolutionaryStuff.Core
{
    public abstract class BaseCodedException : Exception
    {
        public abstract object GetCode();

        public static object GetCode(Exception ex, object missing = null)
        {
            var bce = ex as BaseCodedException;
            return bce == null ? missing : bce.GetCode();
        }

        #region Constructors

        public BaseCodedException()
        {
        }

        public BaseCodedException(string message) : base(message)
        {
        }

        public BaseCodedException(string message, Exception inner) : base(message, inner)
        {
        }
        #endregion
    }

    /// <summary>
    /// The base class for all coded exceptions
    /// </summary>
    public class CodedException<T> : BaseCodedException where T : struct
    {
        public readonly T Code;

        #region Constructors

        /// <summary>
        /// Construct an empty instance
        /// </summary>
        public CodedException(T code)
            : base(code.ToString())
        {
            Code = code;
        }

        /// <summary>
        /// Construct an instance with the given message as extra information
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CodedException(T code, string message)
            : base(string.Format("{0}: {1}", code, message))
        {
            Code = code;
        }

        /// <summary>
        /// Construct an instance with the given message as extra information
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference</param>
        public CodedException(T code, Exception inner)
            : base(code.ToString(), inner)
        {
            Code = code;
        }

        #endregion

        public override string ToString()
        {
            var s = string.Format("{0}({1})\n{2}", this.GetType().Name, Code, base.ToString());
            if (InnerException != null)
            {
                s = string.Format("{0}\n{1}:{2}", s, InnerException.GetType(), InnerException.Message);
            }
            return s;
        }

        public override object GetCode()
        {
            return Code;
        }
    }


    public class NotNowException : Exception
    {
        #region Constructors

        public NotNowException()
        {
        }

        public NotNowException(string message) : base(message)
        {
        }

        public NotNowException(string message, Exception inner) : base(message, inner)
        {
        }

        #endregion
    }

    public class ReadOnlyException : NotNowException
    {
        #region Constructors

        public ReadOnlyException()
        {
        }

        public ReadOnlyException(string message) : base(message)
        {
        }

        public ReadOnlyException(string message, Exception inner) : base(message, inner)
        {
        }

        #endregion
    }

    public class SingleCallException : NotNowException
    {
        #region Constructors

        public SingleCallException()
        {
        }

        public SingleCallException(string message) : base(message)
        {
        }

        public SingleCallException(string message, Exception inner) : base(message, inner)
        {
        }

        #endregion
    }

    public class MustOverrideException : Exception
    {
        #region Constructors

        public MustOverrideException()
        {
        }

        public MustOverrideException(string message) : base(message)
        {
        }

        public MustOverrideException(string message, Exception inner) : base(message, inner)
        {
        }
        #endregion
    }

    public class UnexpectedSwitchValueException : Exception
    {
        public UnexpectedSwitchValueException(object o)
            : base(string.Format("Did not expect val [{0}] in the switch statement", o))
        {
        }
    }

    public class InvalidMappingException : Exception
    {
        public InvalidMappingException(object from, object to)
            : base(string.Format("Could not map {0} to {1}", from, to))
        {
        }
    }

    public static class ExceptionExtensions
    {
        public static IEnumerable<Exception> InnerExceptions(this Exception exception)
        {
            Exception ex = exception;

            while (ex != null)
            {
                yield return ex;
                ex = ex.InnerException;
            }
        }
    }
}