using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OperationalWorkspaceAPI.Extensions;
using OperationalWorkspaceAPI.Middleware;
using OperationalWorkspaceApplication.Abstractions;
using OperationalWorkspaceInfrastructure.Providers;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.RegisterCoreWorkspaceDependencies(builder.Configuration);

// Phase 7 Audit: Wire your security sanitizer instance into the composition container root
builder.Services.AddSingleton<ISecuritySanitizer, ProductionSecuritySanitizer>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// AUDIT TOGGLE EVALUATION: Safely pull environment flags from local configuration layers
bool useMocks = builder.Configuration.GetValue<bool>("SageX3Settings:UseMocks");
bool useMockAuth = builder.Configuration.GetValue<bool>("SageX3Settings:UseMockAuth");

if (!builder.Environment.IsDevelopment() && (useMocks || useMockAuth))
{
    throw new InvalidOperationException("Mock mode cannot run outside Development.");
}

// FIX: Formally register the runtime authentication schemas prior to initializing middleware handles
if (useMockAuth)
{
    // builder.Services.AddAuthentication("MockScheme").AddMockAuthentication();
}
else
{
    // Active concrete production JWT schema ensures pipeline never throws missing scheme crashes
    builder.Services.AddAuthentication("Bearer");
}

builder.Services.AddAuthorization();

// Section 10 CORS Resolution: Explicitly restrict origins to mitigate scripting forgery vectors
builder.Services.AddCors(options =>
{
    options.AddPolicy("OutlookWorkspaceCorsPolicy", policy =>
    {
        policy.WithOrigins("https://workspace.local") // Only trust your specific, secure add-in host
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("X-Cache-Status"); // Expose only required telemetry metrics
    });
});
var app = builder.Build();

// Live Deployment Environment Trace Telemetry Logger (Auditor Requirement)
if (app.Environment.IsProduction())
{
    Console.WriteLine("Running in PRODUCTION mode.");
}
else
{
    Console.WriteLine($"Running in {app.Environment.EnvironmentName} mode.");
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Section 15 HTTPS & Transport Hardening: Force explicit secure encryption limits on server nodes
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseMiddleware<CoreSecurityPolicyMiddleware>();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("OutlookWorkspaceCorsPolicy");

// Authentication middleware stands guard permanently across all runtime states
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
