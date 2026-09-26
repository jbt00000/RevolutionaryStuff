namespace RevolutionaryStuff.Core;

/// <summary>
/// Provides helpers for executing actions and handling their exceptions.
/// </summary>
public static class ExceptionHelpers
{
    /// <summary>
    /// Executes all supplied actions, then rethrows any exceptions they produced.
    /// </summary>
    /// <param name="actions">The actions to execute.</param>
    /// <exception cref="Exception">The sole exception thrown by an action, when exactly one action fails.</exception>
    /// <exception cref="AggregateException">An aggregate of the exceptions thrown when multiple actions fail.</exception>
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


