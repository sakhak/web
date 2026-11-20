using AssetManagement.Application;
using AssetManagement.Infrastructure;
using AssetManagement.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.UseConfiguredEnvironment(args);
builder.Host.AddSerilogLogging();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddWebServices(builder.Configuration)
    .AddAuthenticationAndAuthorization(
        builder.Configuration,
        AuthenticationExtensions.IsLocalLoginEnabled(builder.Configuration, builder.Environment));

var app = builder.Build();

app.UseWebPipeline();

app.Run();

public partial class Program;
