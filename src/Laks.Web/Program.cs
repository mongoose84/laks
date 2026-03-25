using Laks.Web.Data;
using Laks.Web.Data.Repositories;
using Laks.Web.Services;
using Serilog;
using Serilog.Events;

// Bootstrap logger – captures startup errors before full config is loaded.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ----------------------------------------------------------------
    // Serilog
    // ----------------------------------------------------------------
    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: Path.Combine("logs", "laks-.log"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"));

    // ----------------------------------------------------------------
    // Data access
    // ----------------------------------------------------------------
    var connectionString = builder.Configuration.GetConnectionString("Laks")
        ?? throw new InvalidOperationException("Connection string 'Laks' is not configured.");

    builder.Services.AddSingleton<IDbConnectionFactory>(
        _ => new MySqlConnectionFactory(connectionString));

    builder.Services.AddScoped<ICatchRepository, CatchRepository>();
    builder.Services.AddScoped<ISeasonRepository, SeasonRepository>();
    builder.Services.AddScoped<IAnglerRepository, AnglerRepository>();

    builder.Services.AddMemoryCache();
    builder.Services.AddHttpClient<IWeatherService, WeatherService>(client =>
    {
        client.BaseAddress = new Uri("https://api.met.no/");
        client.Timeout = TimeSpan.FromSeconds(5);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("LaksDashboard/2.0 (contact@laks.local)");
    });
    builder.Services.AddHttpClient<IWaterLevelService, WaterLevelService>(client =>
    {
        client.BaseAddress = new Uri("https://hydapi.nve.no/");
        client.Timeout = TimeSpan.FromSeconds(5);
        var apiKey = builder.Configuration["NveHydApi:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey) && !apiKey.Equals("REPLACE_IN_PRODUCTION", StringComparison.OrdinalIgnoreCase))
        {
            client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        }
        else
        {
            Log.Warning("NVE HydAPI key is missing or placeholder. Configure NveHydApi:ApiKey to enable water level data.");
        }
    });

    // ----------------------------------------------------------------
    // Razor Pages + response compression
    // ----------------------------------------------------------------
    builder.Services.AddRazorPages();
    builder.Services.AddResponseCompression(opts => opts.EnableForHttps = true);

    // ----------------------------------------------------------------
    // Health checks
    // ----------------------------------------------------------------
    builder.Services
        .AddHealthChecks()
        .AddMySql(connectionString, name: "mysql", tags: ["db", "ready"]);

    // ----------------------------------------------------------------
    // HTTP pipeline
    // ----------------------------------------------------------------
    var app = builder.Build();

    app.UseSerilogRequestLogging();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseResponseCompression();
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            // Cache static files for 7 days in production.
            if (!app.Environment.IsDevelopment())
                ctx.Context.Response.Headers.CacheControl = "public,max-age=604800";
        }
    });

    app.UseRouting();
    app.UseAuthorization();

    app.MapRazorPages();

    // Minimal API endpoints for chart data (consumed by Razor Pages via fetch)
    app.MapGet("/api/stats/catches-per-year", async (ICatchRepository repo) =>
        Results.Ok(await repo.GetCatchesPerYearAsync()))
        .WithName("CatchesPerYear");

    app.MapGet("/api/stats/catches-per-angler", async (ICatchRepository repo, int? year) =>
        Results.Ok(await repo.GetCatchesPerAnglerAsync(year)))
        .WithName("CatchesPerAngler");

    app.MapGet("/api/stats/catches-by-type", async (ICatchRepository repo, int? year) =>
        Results.Ok(await repo.GetCatchesByTypeAsync(year)))
        .WithName("CatchesByType");

    app.MapGet("/api/stats/catches-by-species", async (ICatchRepository repo, int? year) =>
        Results.Ok(await repo.GetCatchesByTypeAsync(year)))
        .WithName("CatchesBySpecies");

    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;

// Required for integration testing
public partial class Program { }
