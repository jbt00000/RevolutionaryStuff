namespace RevolutionaryStuff.Core;

public static class ExceptionHelpers
{
    public static void AggregationExceptionsAndReThrow(params Action[] actions)
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
                (exceptions ??= new List<Exception>()).Add(ex);
            }
        }
        if (exceptions != null)
        {
            if (exceptions.Count == 1)
            {
                throw exceptions[0];
            }
            else
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}


