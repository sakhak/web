using Serilog;

namespace AssetManagement.Web.Extensions;

public static class LoggingExtensions
{
    public static IHostBuilder AddSerilogLogging(this IHostBuilder host) =>
        host.UseSerilog((context, services, logger) => logger
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("wwwroot/logs/log-.log", rollingInterval: RollingInterval.Day));
}
