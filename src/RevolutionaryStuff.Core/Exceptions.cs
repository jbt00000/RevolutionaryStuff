namespace RevolutionaryStuff.Core;

public abstract class BaseCodedException : Exception
{
    public bool IsPermanent { get; protected set; }

    public abstract object GetCode();

    public static object GetCode(Exception ex, object missing = null)
    {
        return ex is not BaseCodedException bce ? missing : bce.GetCode();
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
public class CodedException<TCode> : BaseCodedException where TCode : System.Enum
{
    public readonly TCode Code;

    #region Constructors

    /// <summary>
    /// Construct an empty instance
    /// </summary>
    public CodedException(TCode code)
        : base(code.ToString())
    {
        Code = code;
    }

    /// <summary>
    /// Construct an instance with the given message as extra information
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public CodedException(TCode code, string message)
        : base($"{code}: {message}")
    {
        Code = code;
    }

    /// <summary>
    /// Construct an instance with the given message as extra information
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or a null reference</param>
    public CodedException(TCode code, Exception inner)
        : base(code.ToString(), inner)
    {
        Code = code;
    }

    #endregion

    public override string ToString()
    {
        var s = $"{GetType().Name}({Code})\n{base.ToString()}";
        if (InnerException != null)
        {
            s = $"{s}\n{InnerException.GetType()}:{InnerException.Message}";
        }
        return s;
    }

    public override object GetCode()
        => Code;
}

public class ItemNotFoundException : KeyNotFoundException
{
    public ItemNotFoundException(string reason)
        : base(reason)
    { }

    public static void ThrowIfNull<T>(T item, string itemKey) where T : class
    {
        if (item == null)
        {
            throw new ItemNotFoundException($"Could not find {typeof(T)} with key=[{itemKey}]");
        }
    }
}

public class PermanentException : Exception
{
    #region Constructors

    public PermanentException()
    { }

    public PermanentException(Exception inner)
        : this("Permanent condition", inner)
    { }

    public PermanentException(string message)
        : base(message)
    { }

    public PermanentException(string message, Exception inner)
        : base(message, inner)
    { }

    #endregion

    public static T TryAndRethrow<T>(Func<T> f, string message = default)
    {
        try
        {
            return f();
        }
        catch (PermanentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw message == default ? throw new PermanentException(ex) : throw new PermanentException(message, ex);
        }
    }

    public static void TryAndRethrow(Action a, string message = default)
    {
        try
        {
            a();
        }
        catch (PermanentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw message == default ? throw new PermanentException(ex) : throw new PermanentException(message, ex);
        }
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
        : base($"Did not expect val [{o}] in the switch statement")
    {
    }
}

public class InvalidMappingException : Exception
{
    public InvalidMappingException(object from, object to)
        : base($"Could not map {from} to {to}")
    {
    }
}

public class ParameterizedMessageException : Exception
{
    public readonly object[] MessageArgs;

    public ParameterizedMessageException(Exception inner, string message, params object[] args)
        : base(message, inner)
    {
        MessageArgs = args;
    }

    public ParameterizedMessageException(string message, params object[] args)
        : base(message)
    {
        MessageArgs = args;
    }
}

public static class ExceptionExtensions
{
    public static IEnumerable<Exception> InnerExceptions(this Exception exception)
    {
        var ex = exception;

        while (ex != null)
        {
            yield return ex;
            ex = ex.InnerException;
        }
    }
}

public class HttpStatusCodeException : CodedException<System.Net.HttpStatusCode>
{
    #region Constructors

    /// <summary>
    /// Construct an empty instance
    /// </summary>
    public HttpStatusCodeException(System.Net.HttpStatusCode code)
        : base(code)
    { }

    /// <summary>
    /// Construct an instance with the given message as extra information
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public HttpStatusCodeException(System.Net.HttpStatusCode code, string message)
        : base(code, message)
    { }

    /// <summary>
    /// Construct an instance with the given message as extra information
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or a null reference</param>
    public HttpStatusCodeException(System.Net.HttpStatusCode code, Exception inner)
        : base(code, inner)
    { }

    #endregion
}
