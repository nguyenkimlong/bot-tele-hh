using App_Driver.Worker;
using HelloBotNET.AppService;
using HelloBotNET.AppService.Database;
using HelloBotNET.AppService.Services;

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
builder.Services.AddHostedService<Worker>();
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
