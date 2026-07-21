using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OperationalWorkspaceUI.Components;
using OperationalWorkspaceUI.UIState;
using OperationalWorkspaceUI.UIServices;
using Microsoft.FluentUI.AspNetCore.Components;
using System;

var builder = WebApplication.CreateBuilder(args);

// 1. Session State Container and Fluent UI Layout Core Requirements
builder.Services.AddScoped<UIStateContainer>();
builder.Services.AddFluentUIComponents();
builder.Services.AddScoped<GlobalState>();

// 2. AUDIT VERIFIED ENVIRONMENT SECURITY GUARD
bool useMocks = builder.Configuration.GetValue<bool>("SageX3Settings:UseMocks");
bool useMockAuth = builder.Configuration.GetValue<bool>("SageX3Settings:UseMockAuth");

if (!builder.Environment.IsDevelopment() && (useMocks || useMockAuth))
{
    throw new InvalidOperationException("Mock mode cannot run outside Development.");
}

// 3. FIX: Dynamic Dependency Injection Toggles Between Live and Mock Services Neatly
if (useMocks)
{
    // Registers the permanent local mock service implementation for rapid offline testing
    builder.Services.AddScoped<IWorkspaceApiService, MockWorkspaceApiService>();
}
else
{
    // Registers the live production network service connecting straight to your ERP endpoints
    builder.Services.AddHttpClient<IWorkspaceApiService, WorkspaceApiService>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["SageX3Settings:BaseUrl"] ?? "https://yourcompany.com");
    });
}

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (app.Environment.IsProduction())
{
    Console.WriteLine("Running in PRODUCTION mode.");
}
else
{
    Console.WriteLine($"Running in {app.Environment.EnvironmentName} mode.");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
