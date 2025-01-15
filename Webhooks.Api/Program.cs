using Microsoft.EntityFrameworkCore;
using Webhooks.Api.Data;
using Webhooks.Api.Extensions;
using Webhooks.Api.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

builder.Services.AddScoped<WebhookDispatcher>();

builder.Services.AddDbContext<WebhooksDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.ApplyMigrationAsync();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseSerilogPlusRequestLogging();

app.MapControllers();

app.Run();
