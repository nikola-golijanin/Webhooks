using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Webhooks.API.Data;
using Webhooks.API.Extensions;
using Webhooks.API.Repositories;
using Webhooks.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi();

builder.Services.AddSingleton<InMemoryOrderRepository>();

builder.Services.AddHttpClient();

builder.Services.AddScoped<WebhookDispatcher>();

builder.Services.AddDbContext<WebhooksDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("WebhooksDatabase"));
});

builder.Services.AddHostedService<WebhookProcessor>();

builder.Services.AddSingleton(_ =>
{
    return Channel.CreateBounded<WebhookDispatch>(new BoundedChannelOptions(100)
    {
        FullMode = BoundedChannelFullMode.Wait
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.ApplyMigrationsAsync();
}

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
