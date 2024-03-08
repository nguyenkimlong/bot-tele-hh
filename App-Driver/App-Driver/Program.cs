using App_Driver.Worker;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using HelloBotNET.AppService;
using HelloBotNET.AppService.Database;
using HelloBotNET.AppService.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDistributedMemoryCache();
// Add bot properties service.
builder.Services.AddSingleton<HelloBotProperties>();

// Add bot service.
builder.Services.AddScoped<HelloBot>();
builder.Services.Configure<HostOptions>(options =>
{
    //Service Behavior in case of exceptions - defautls to StopHost
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
    //Host will try to wait 30 seconds before stopping the service. 
    options.ShutdownTimeout = TimeSpan.FromSeconds(60);
});
builder.Services.AddHostedService<Worker>();

builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
{
    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs/log.txt");
    loggerConfiguration
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .ReadFrom.Configuration(hostingContext.Configuration)
        .WriteTo.File(path, rollingInterval: RollingInterval.Day);
});

new DatabaseHandler();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
