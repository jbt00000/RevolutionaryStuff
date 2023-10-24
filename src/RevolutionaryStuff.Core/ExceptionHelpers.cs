namespace RevolutionaryStuff.Core;

public static class ExceptionHelpers
{
    public static void AggregateExceptionsAndReThrow(params Action[] actions)
    {
        List<Exception> exceptions = null;
        foreach (var action in actions)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                (exceptions ??= []).Add(ex);
            }
        }
        if (exceptions != null)
        {
            if (exceptions.Count == 1)
            {
                throw exceptions[0];
            }

            throw new AggregateException(exceptions);
        }
    }
}


