namespace RevolutionaryStuff.Applets.WebhookReceiverHost;

public record WebhookReceiverHostProgramSettings(
    Applets.Use.Settings? RevolutionaryStuffAppletsUseSettings,
    AspNetCore.Use.Settings? AspNetCoreUseSettings,
    Azure.Use.Settings? AzureUseSettings
    );
