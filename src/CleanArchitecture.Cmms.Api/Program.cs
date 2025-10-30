using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CleanArchitecture.Cmms.Api.Configurations;
using CleanArchitecture.Cmms.Api.Filters;
using CleanArchitecture.Cmms.Api.Middlewares;
using CleanArchitecture.Cmms.Application;
using CleanArchitecture.Cmms.Infrastructure;
using CleanArchitecture.Cmms.Infrastructure.Persistence;
using CleanArchitecture.Cmms.Infrastructure.Persistence.EfCore;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Outbox;
using Microsoft.AspNetCore.Mvc;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting up CleanArchitecture.Cmms API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.EnvironmentName);

    // Add outbox with same connection string (shares database)
    builder.Services.AddOutbox(builder.Configuration.GetConnectionString("WriteDb")!);

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ResultToHttpStatusFilter>();
    })
        .ConfigureApiBehaviorOptions(options =>
    {
        //TODO Shift to a util or service
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .Select(e => new
                {
                    Field = e.Key,
                    Messages = e.Value!.Errors.Select(x => x.ErrorMessage).ToArray()
                })
                .ToList();

            var errorMessage = string.Join("; ", errors.SelectMany(e => e.Messages));
            var error = Error.Validation("Validation.ModelState", errorMessage);
            var result = Result.Failure(error);

            return new BadRequestObjectResult(result);
        };
    });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddTransient<ExceptionHandlingMiddleware>();

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();
    });

    var app = builder.Build();




    if (app.Environment.IsDevelopment())
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
            await DatabaseSeeder.SeedAsync(db);
        }

        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var groupName in provider.ApiVersionDescriptions.Select(a => a.GroupName))
            {
                options.SwaggerEndpoint(
                    $"/swagger/{groupName}/swagger.json",
                    groupName.ToUpperInvariant());
            }
        });
    }

    app.UseSerilogRequestLogging();

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

// Make Program class accessible for integration tests
public partial class Program { }
