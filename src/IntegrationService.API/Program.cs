using FluentValidation;
using IntegrationService.Application.Common.Interfaces;
using IntegrationService.Application.Requests.Commands.AddRequest;
using IntegrationService.Infrastructure.Configuration;
using IntegrationService.Infrastructure.ExternalServices;
using IntegrationService.Infrastructure.Persistence;
using IntegrationService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Integration Service API",
        Version = "v1",
        Description = "API for managing external integration requests"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configure database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Use SQL Server or SQLite based on configuration
    if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("Data Source="))
    {
        options.UseSqlite(connectionString ?? "Data Source=integration.db");
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

// Configure Integration settings
builder.Services.Configure<IntegrationSettings>(
    builder.Configuration.GetSection(IntegrationSettings.SectionName));

// Register HttpClient for integration
builder.Services.AddHttpClient<IIntegrationClient, IntegrationClient>((serviceProvider, client) =>
{
    var settings = builder.Configuration
        .GetSection(IntegrationSettings.SectionName)
        .Get<IntegrationSettings>();

    client.BaseAddress = new Uri(settings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
    
    if (!string.IsNullOrEmpty(settings.ApiKey))
    {
        client.DefaultRequestHeaders.Add("X-API-Key", settings.ApiKey);
    }
});

// Register repositories
builder.Services.AddScoped<IRequestRepository, RequestRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register MediatR
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(AddRequestCommand).Assembly);
});

// Register FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(AddRequestCommand).Assembly);

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Integration Service API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
