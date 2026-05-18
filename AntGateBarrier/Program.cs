using AntGateBarrier.BackgroundServices;
using AntGateBarrier.Domain.Controller;
using AntGateBarrier.Domain.Factory;
using AntGateBarrier.ScopedServices;
using AntGateBarrier.SingletonServices;
using log4net.Config;
using Microsoft.Extensions.Logging.EventLog;

var MyAllowSpecificOrigins = Guid.NewGuid().ToString().Replace("-", "").ToUpper();


IAppSettings _appSettings = new CAppSettings();

WebApplicationOptions options = new()
{
    ContentRootPath = AppContext.BaseDirectory,
    Args = args
};


string path_json = string.Concat(AppContext.BaseDirectory, "log4net.config");
XmlConfigurator.Configure(new FileInfo(path_json));


var builder = WebApplication.CreateBuilder(options);

// Add services to the container.
builder.Services.AddSingleton<IAppSettings, CAppSettings>();
builder.Services.AddSingleton<IRealtimeWs,CRealtimeWs>();
builder.Services.AddSingleton<IKioskoWs, CKioskoWs>();
builder.Services.AddSingleton<IRfidWs, CRfidWs>();
builder.Services.AddSingleton<IWeightWs, CWeightWs>();

builder.Services.AddSingleton<IAnntSingletonService, CAnntSingletonService>();
builder.Services.AddSingleton<IWeightSingletonService, CWeightSingletonService>();

builder.Services.AddScoped<ISqlServerContextFactory, SqlServerContextFactory>();
builder.Services.AddScoped<IMemoryContextFactory, MemoryContextFactory>();
builder.Services.AddScoped<ISqlServerController, SqlServerController>();
builder.Services.AddScoped<ISqlMemoryController, SqlMemoryController>();
builder.Services.AddScoped<IVehicleScopedService, CVehicleScopedService>();
builder.Services.AddScoped<IAnntScopedService, CAnntScopedService>();
builder.Services.AddScoped<IWeightScopedService, CWeightScopedService>();

builder.Services.AddHostedService<WeightBackgroundServices>();
builder.Services.AddHostedService<VehicleBackgroundServices>();
builder.Services.AddHostedService<AnntBackgroundServices>();





builder.Services.AddControllers();


/*
 * Adicional de builder añadidos por mi
 */

builder.Host.UseWindowsService(options =>
{
    options.ServiceName = _appSettings.GetSettings().ServiceName;
});

builder.Services.Configure<EventLogSettings>(config =>
{
    config.LogName = string.Empty;
    config.SourceName = _appSettings.GetSettings().ServiceName;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins(_appSettings.GetCors().Urls)
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

/*
 * Final de adicional del builder
 */


var app = builder.Build();

/*
 * Añadir header de seguridad
 */
app.Use((ctx, next) =>
{
    var headers = ctx.Response.Headers;
    headers.Append("Server", "GWS/2.1");
    headers.Append("X-Frame-Options", "DENY");
    headers.Append("X-XSS-Protection", "1; mode=block");
    headers.Append("X-Content-Type-Options", "nosniff");
    headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    headers.Remove("X-Powered-By");
    headers.Remove("x-aspnet-version");
    return next();
});

app.UseCors(MyAllowSpecificOrigins);

/*
 * Fin de header seguridad
 */

// Configure the HTTP request pipeline.
app.UseWebSockets();
app.UseAuthorization();
app.MapControllers();
await app.RunAsync();
