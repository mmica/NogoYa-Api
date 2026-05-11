using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using NogoYa.API.Extensions;
using NogoYa.API.Middleware;
using NogoYa.Application;
using NogoYa.Application.Common;
using NogoYa.Application.Validators;
using NogoYa.Infrastructure;
using NogoYa.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---- Connection string resolution -------------------------------------------
// In production we accept a single DATABASE_URL env var (Render / Neon style)
// and translate it to the Npgsql format. Local dev keeps using appsettings.json.
var dbUrlConn = DatabaseUrlParser.FromEnvironment();
if (dbUrlConn is not null)
{
    builder.Configuration["ConnectionStrings:DefaultConnection"] = dbUrlConn;
}

// ---- Logging (Serilog) ------------------------------------------------------
builder.Host.UseSerilog((ctx, _, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "NogoYa.API")
    .WriteTo.Console());

// ---- Services ---------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Nogo-Ya API", Version = "v1" });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateStoreValidator>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ---- CORS (configurable; supports comma-separated list in env var) ----------
const string CorsPolicy = "ConfiguredCors";
var allowedOrigins = (builder.Configuration["Cors:AllowedOrigins"]
    ?? "http://localhost:4200")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// Forwarded headers (Render terminates TLS at the proxy).
builder.Services.Configure<Microsoft.AspNetCore.HttpOverrides.ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
                       | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    o.KnownNetworks.Clear();
    o.KnownProxies.Clear();
});

var app = builder.Build();

// ---- Auto-apply migrations on startup ---------------------------------------
// Idempotent: only pending migrations run. Wrapped in try/catch so a transient
// DB outage doesn't crash the container loop indefinitely.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var ctx = scope.ServiceProvider.GetRequiredService<NogoYaDbContext>();
        logger.LogInformation("Applying database migrations…");
        await ctx.Database.MigrateAsync();
        logger.LogInformation("Migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Migration failure on startup. The app will keep running so /health stays available.");
    }
}

// ---- Pipeline ---------------------------------------------------------------
app.UseForwardedHeaders();
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger always on for the MVP — easy testing of the deployed API.
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(CorsPolicy);
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }));
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
