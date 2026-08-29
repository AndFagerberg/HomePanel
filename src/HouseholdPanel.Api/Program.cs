using HouseholdPanel.Application;
using HouseholdPanel.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

const string DevCorsPolicy = "AngularDevServer";

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Allows `ng serve` (default port 4200) to call the API during development.
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors(DevCorsPolicy);
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

// Serves the Angular production build so the frontend and API share a single origin.
app.MapFallbackToFile("index.html");

app.Run();

// Exposes Program for WebApplicationFactory in HouseholdPanel.IntegrationTests.
public partial class Program;


