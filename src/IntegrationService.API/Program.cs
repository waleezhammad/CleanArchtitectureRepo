using FluentValidation;
using IntegrationService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Diagnostics;
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

// Register validation pipeline behavior (runs validators before handlers)
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(IntegrationService.Application.Common.Behaviors.ValidationBehavior<,>));

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

// Handle FluentValidation.ValidationException and return 400 with error details
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.ContentType = "application/problem+json";
        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionHandlerFeature?.Error;

        if (exception is ValidationException validationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "One or more validation errors occurred.",
                status = StatusCodes.Status400BadRequest,
                errors
            });
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "An error occurred while processing your request.",
                status = StatusCodes.Status500InternalServerError
            });
        }
    });
});

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
