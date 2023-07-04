using Microsoft.Extensions.DependencyInjection;

namespace RevolutionaryStuff.Core.Services.Correlation;

public class CorrectionIdFindOrCreate : ICorrectionIdFindOrCreate
{
    private readonly ICorrelationIdFactory Factory;
    private readonly IServiceProvider ServiceProvider;

    public CorrectionIdFindOrCreate(ICorrelationIdFactory factory, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        Factory = factory;
        ServiceProvider = serviceProvider;
    }

    string ICorrectionIdFindOrCreate.CorrelationId
    {
        get
        {
            if (CorrelationIdField == null)
            {
                var finders = ServiceProvider.GetServices<ICorrelationIdFinder>();
                foreach (var finder in finders)
                {
                    var ids = finder.CorrelationIds;
                    if (ids != null && ids.Count > 0)
                    {
                        CorrelationIdField = ids[0];
                        break;
                    }
                }
                CorrelationIdField ??= Factory.Create();
            }
            return CorrelationIdField;
        }
    }
    private string CorrelationIdField;
}
