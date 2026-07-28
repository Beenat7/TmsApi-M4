using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

using Scalar.AspNetCore;
using TmsApi.Api.Authentication;
using TmsApi.Api.Filters;
using TmsApi.Api.Middleware;

using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;

using Asp.Versioning;
using TmsApi.Infrastructure.Persistence.Repositories;

using FluentValidation;
using MediatR;
using TmsApi.Api.ExceptionHandlers;
using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;

using Microsoft.Extensions.Caching.Hybrid;

using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Api.RateLimiting;


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddAuthentication("DemoScheme")
    .AddScheme<AuthenticationSchemeOptions, DemoAuthenticationHandler>(
        "DemoScheme",
        options => { });

builder.Services.AddAuthorization();


builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Register TmsDbContext scoped for incoming HTTP requests
builder.Services.AddDbContext<TmsDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
.LogTo(Console.WriteLine, LogLevel.Information) // Log SQLto output window
.EnableSensitiveDataLogging()); // Show parameters in querylogs (dev only)

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(EnrollStudentHandler).Assembly));

builder.Services.AddValidatorsFromAssembly(
    typeof(EnrollStudentValidator).Assembly);

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(LoggingBehavior<,>));

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(2)
    };
});

builder.Services.AddScoped<ICachedCourseService, CachedCourseService>();

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        {
            var (partitionKey, tier) =
                ApiKeyResolver.Resolve(httpContext);

            return tier switch
            {
                ApiKeyTier.Paid =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: $"paid:{partitionKey}",
                        factory: _ =>
                            new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = 200,
                                TokensPerPeriod = 100,
                                ReplenishmentPeriod =
                                    TimeSpan.FromSeconds(10),
                                QueueLimit = 0,
                                AutoReplenishment = true
                            }),

                ApiKeyTier.Free =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: $"free:{partitionKey}",
                        factory: _ =>
                            new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = 30,
                                TokensPerPeriod = 10,
                                ReplenishmentPeriod =
                                    TimeSpan.FromSeconds(10),
                                QueueLimit = 0,
                                AutoReplenishment = true
                            }),

                _ =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: $"anon:{partitionKey}",
                        factory: _ =>
                            new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = 10,
                                TokensPerPeriod = 5,
                                ReplenishmentPeriod =
                                    TimeSpan.FromSeconds(10),
                                QueueLimit = 0,
                                AutoReplenishment = true
                            })
            };
        });


    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;


    options.OnRejected = async (context, ct) =>
    {
        var retryAfter = "10";
        if (context.Lease.TryGetMetadata(
            MetadataName.RetryAfter,
            out var retry))
        {
            retryAfter =
                ((int)retry.TotalSeconds).ToString();
        }

        context.HttpContext.Response.Headers.RetryAfter = retryAfter;
        context.HttpContext.Response.ContentType = "application/problem+json";

        await context.HttpContext.Response
            .WriteAsJsonAsync(
                new ProblemDetails
                {
                    Title = "Rate limit exceeded",
                    Detail =
                        $"Too many requests. Retry after {retryAfter} seconds.",
                    Status =
                        StatusCodes.Status429TooManyRequests,
                    Type =
                        "https://tms.local/errors/rate_limit_exceeded"
                },
                ct);
    };

    options.AddConcurrencyLimiter(
    "transcripts",
    options =>
    {
        options.PermitLimit = 5;
        options.QueueLimit = 20;
        options.QueueProcessingOrder =
            QueueProcessingOrder.OldestFirst;
    });

    options.AddTokenBucketLimiter("search", opt =>
    {
        opt.TokenLimit = 10;
        opt.TokensPerPeriod = 5;
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        opt.QueueLimit = 2;
        opt.AutoReplenishment = true;
    });


});


var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<V1DeprecationMiddleware>();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException(
        "Simulated database failure for ProblemDetails testing");
});

// Seed test data at startup
using (var scope = app.Services.CreateScope())
{
var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
context.Database.Migrate(); // Applies any pending migrations; keeps migration history intact
if (!context.Students.Any())
{
var students = new List<Student>
{
new() { RegistrationNumber = "TMS-2026-0001", Name = "Alice Smith", GPA = 3.8m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0002", Name = "BobJones", GPA = 2.9m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0003", Name = "Charlie Brown", GPA = 3.4m, IsActive = false },
new() { RegistrationNumber = "TMS-2026-0004", Name = "DianaPrince", GPA = 3.9m, IsActive = true },
new() { RegistrationNumber = "TMS-2026-0005", Name = "EvanWright", GPA = 2.5m, IsActive = true }
};
context.Students.AddRange(students);
var courses = new List<Course>
{
new() { Code = "CS-101", Title = "Introduction to ComputerScience", MaxCapacity = 30 },
new() { Code = "CS-201", Title = "Data Structures and Algorithms", MaxCapacity = 25 },
new() { Code = "MAT-101", Title = "Calculus I", MaxCapacity=40 }
};
context.Courses.AddRange(courses);
context.SaveChanges();
var enrollments = new List<Enrollment>
{
new() { StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m },
new() { StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m },
new() { StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m },
new() { StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m }
};
context.Enrollments.AddRange(enrollments);
context.SaveChanges();
}
}

if (app.Environment.IsDevelopment())
{
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider
.GetRequiredService<TmsDbContext>();
await DataSeeder.SeedAsync(context);
}

app.Run();

