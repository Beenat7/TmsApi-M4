using Microsoft.AspNetCore.Authentication;
using TmsApi.Authentication;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddAuthentication("DemoScheme")
    .AddScheme<AuthenticationSchemeOptions, DemoAuthenticationHandler>(
        "DemoScheme",
        options => { });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();

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
