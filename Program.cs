using Microsoft.AspNetCore.Authentication;
using TmsApi.Authentication;
using TmsApi.Middleware;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddAuthentication("DemoScheme")
    .AddScheme<AuthenticationSchemeOptions, DemoAuthenticationHandler>(
        "DemoScheme",
        options => { });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Register TmsDbContext scoped for incoming HTTP requests
builder.Services.AddDbContext<TmsDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase")));

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/api/enrollments/worker-smoke",
    (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});
app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException(
        "Simulated database failure for ProblemDetails testing");
});

app.Run();









// var builder = WebApplication.CreateBuilder(args);

// // Add services to the container.

// builder.Services.AddControllers();

// var app = builder.Build();

// // Configure the HTTP request pipeline.

// app.UseHttpsRedirection();

// app.UseAuthorization();

// app.MapControllers();

// app.Run();
