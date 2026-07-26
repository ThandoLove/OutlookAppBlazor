using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OperationalWorkspaceUI.Components;
using OperationalWorkspaceUI.UIState;
using OperationalWorkspaceUI.UIServices;
using Microsoft.FluentUI.AspNetCore.Components;
using System;

var builder = WebApplication.CreateBuilder(args);

// Senior Engineer Refactor Pass: Clear diagnostic pipeline logging engines
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// 1. Session State Container Core Lifetime Realignment (Section 1)
// STABILIZATION REFACTOR: Set UIStateContainer as a Singleton to prevent state erasure across page route transitions
builder.Services.AddSingleton<UIStateContainer>();
builder.Services.AddFluentUIComponents();
builder.Services.AddScoped<GlobalState>();

// 2. Authentication and Presentation Workflow Orchestrator Registrations
builder.Services.AddScoped<AuthenticationService>();

// 3. Environmental Mock/Live Signal Variable Extractions
bool useMocks = builder.Configuration.GetValue<bool>("SageX3Settings:UseMocks");
bool useMockAuth = builder.Configuration.GetValue<bool>("SageX3Settings:UseMockAuth");

// Section 8 Resolution: Core Security Guard Verification Boundary Barrier Loop Check
if (!builder.Environment.IsDevelopment() && (useMocks || useMockAuth))
{
    throw new InvalidOperationException("Critical Security Violation: Execution parameters mapping to Mock layers are disabled inside non-Development hosting environments.");
}

// 4. DYNAMIC DEPENDENCY INJECTION ENGINE DECOUPLING INTERFACE MATRIX
if (useMocks)
{
    // Register the local mock data provider class for offline development checking
    builder.Services.AddScoped<IWorkspaceApiService, MockWorkspaceApiService>();
}
else
{
    // Register the live production endpoint service layer
    builder.Services.AddHttpClient<IWorkspaceApiService, WorkspaceApiService>(client =>
    {
        // FIX: Added null-coalescing operator declaration to satisfy C# compiler nullable reference checks completely
        string hostUrl = builder.Configuration["SageX3Settings:BaseUrl"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(hostUrl))
        {
            throw new InvalidOperationException("Configuration Failure: Base address string 'SageX3Settings:BaseUrl' is undefined.");
        }
        client.BaseAddress = new Uri(hostUrl);
    });
}

// 5. Initialize Server Side Interactive Blazor Component Services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// 6. Map Middleware Pipeline Constraints Handlers
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Exposes root wwwroot CSS bundles natively to clear out 404 network errors
app.UseAntiforgery();

// 7. Route and Instantiates Core Blazor WebSocket Circuits
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
