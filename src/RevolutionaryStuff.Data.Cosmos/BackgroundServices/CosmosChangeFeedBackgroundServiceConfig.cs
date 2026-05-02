using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Data.Cosmos.BackgroundServices;

public sealed class CosmosChangeFeedBackgroundServiceConfig : IValidate, IPostConfigure
{
    public const string ConfigSectionName = "CosmosChangeFeedProcessorWorkerConfig";

    public string ConnectionStringName { get; set; }

    public bool AuthenticateWithWithDefaultAzureCredentials { get; set; } = true;

    public IList<Execution> Executions { get; set; }

    public string LeaseContainerName { get; set; }

    public string DatabaseName { get; set; }

    public IDictionary<string, string> DocumentJsonPathToPropertyName { get; set; }

    public string MessageIdFormat { get; set; } = "{0}/{3}";

    public void Validate()
        => ExceptionHelpers.AggregateExceptionsAndReThrow(
        () => Executions.ForEach(z => z.Validate())
        );

    void IPostConfigure.PostConfigure()
    {
        Executions ??= [];
        Executions.ForEach(z => z.PostConfigure());
    }


    public class Execution : IValidate, IPostConfigure
    {
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public DateTime? StartTime { get; set; }
        public string ConnectionStringName { get; set; }
        public string DatabaseName { get; set; }
        public string ContainerName { get; set; }
        public string LeaseContainerName { get; set; }
        public IDictionary<string, string> DocumentJsonPathToPropertyName { get; set; }
        public void Validate()
            => ExceptionHelpers.AggregateExceptionsAndReThrow(
            () => { if (Enabled) Requires.Text(ContainerName); }
            );

        public void PostConfigure()
            => Name ??= $"{ConnectionStringName} on {DatabaseName}.{ContainerName}";

    }
}
