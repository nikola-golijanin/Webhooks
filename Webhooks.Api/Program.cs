using Serilog;
using Webhooks.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddOpenApi();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpClient();

builder.Services.AddServices();
builder.Services.AddDatabaseContext(builder.Configuration);
builder.Services.AddMassTransitServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsDocker())
{
    app.MapOpenApi();
    app.AddScalarUi();
    await app.ApplyMigrationAsync();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseSerilogPlusRequestLogging();

app.MapControllers();

app.Run();