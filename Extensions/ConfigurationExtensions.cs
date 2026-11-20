namespace AssetManagement.Web.Extensions;

public static class ConfigurationExtensions
{
    public static WebApplicationBuilder UseConfiguredEnvironment(this WebApplicationBuilder builder, string[] args)
    {
        var chosen = builder.Configuration["EnvironmentConfig:CurrentEnvironment"];
        if (string.IsNullOrEmpty(chosen) || chosen == builder.Environment.EnvironmentName) return builder;

        builder.Environment.EnvironmentName = chosen;
        builder.Configuration
            .AddJsonFile($"appsettings.{chosen}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        return builder;
    }
}
