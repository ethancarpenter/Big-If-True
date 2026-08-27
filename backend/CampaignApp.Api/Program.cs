using System.Text.Json.Serialization;
using CampaignApp.Api.Infrastructure;
using CampaignApp.Application.Interfaces;
using CampaignApp.Application.Services;
using CampaignApp.Infrastructure.Persistence;
using CampaignApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

const string DevCorsPolicy = "DevCorsPolicy";

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString!, name: "postgres");

builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<INpcRepository, NpcRepository>();
builder.Services.AddScoped<INpcService, NpcService>();
builder.Services.AddScoped<INpcLocationRepository, NpcLocationRepository>();
builder.Services.AddScoped<INpcLocationService, NpcLocationService>();
builder.Services.AddScoped<IQuestRepository, QuestRepository>();
builder.Services.AddScoped<IQuestService, QuestService>();
builder.Services.AddScoped<IQuestObjectiveRepository, QuestObjectiveRepository>();
builder.Services.AddScoped<IQuestObjectiveService, QuestObjectiveService>();
builder.Services.AddScoped<IQuestNpcRepository, QuestNpcRepository>();
builder.Services.AddScoped<IQuestNpcService, QuestNpcService>();
builder.Services.AddScoped<IQuestLocationRepository, QuestLocationRepository>();
builder.Services.AddScoped<IQuestLocationService, QuestLocationService>();
builder.Services.AddScoped<ICurrentUserProvider, PlaceholderCurrentUserProvider>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(DevCorsPolicy);

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/api/health");

app.Run();

// Exposed so CampaignApp.Tests can bootstrap this app via WebApplicationFactory<Program>.
public partial class Program { }
