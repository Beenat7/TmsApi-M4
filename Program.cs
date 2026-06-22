using Microsoft.AspNetCore.Authentication;
using TmsApi.Authentication;
using TmsApi.Middleware;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddAuthentication("DemoScheme")
    .AddScheme<AuthenticationSchemeOptions, DemoAuthenticationHandler>(
        "DemoScheme",
        options => { });

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseExceptionHandler("/error");
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

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
