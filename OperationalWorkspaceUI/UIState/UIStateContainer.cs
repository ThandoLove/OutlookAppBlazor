using OperationalWorkspaceApplication.Dtos;
using OperationalWorkspaceApplication.Responses;
using OperationalWorkspaceApplication.Responses.WorkspaceContextResponse;
using System;

namespace OperationalWorkspaceUI.UIState;

public class UIStateContainer
{
    // Centralized Memory Tracking Primitives
    public bool IsAuthenticated { get; private set; }
    public string SenderEmail { get; private set; } = string.Empty;
    public string SenderName { get; private set; } = string.Empty;
    public string ActiveUserEmail { get; private set; } = string.Empty;
    public string UserRoleScope { get; private set; } = "Sales;Consultant";

    // Cached Customer 360 Payload Model State
    public WorkspaceContextResponse? CurrentContextResponse { get; private set; }

    // STABILIZATION REFACTOR: Split monolithic global event channel into dedicated domain streams
    public event Action? AuthenticationChanged;
    public event Action? WorkspaceContextChanged;
    public event Action? ActiveUserChanged;

    public void SetAuthenticatedState(bool isAuthenticated)
    {
        if (IsAuthenticated == isAuthenticated) return;
        IsAuthenticated = isAuthenticated;
        NotifyAuthenticationChanged();
    }

    public void SetIdentityContext(string senderEmail, string senderName, string activeUserEmail)
    {
        SenderEmail = senderEmail?.Trim() ?? string.Empty;
        SenderName = senderName?.Trim() ?? string.Empty;
        ActiveUserEmail = activeUserEmail?.Trim() ?? string.Empty;
        NotifyWorkspaceContextChanged();
    }

    public void SetUserRoleScope(string roleScope)
    {
        if (!string.IsNullOrWhiteSpace(roleScope))
        {
            UserRoleScope = roleScope.Trim();
            NotifyActiveUserChanged();
        }
    }

    public void SetWorkspaceContextResponse(WorkspaceContextResponse response)
    {
        CurrentContextResponse = response;
        NotifyWorkspaceContextChanged();
    }

    /// <summary>
    /// Batches multi-property session updates into atomic domain triggers 
    /// to completely eliminate infinite layout rendering feedback loops.
    /// </summary>
    public void UpdateSession(bool isAuthenticated, string senderEmail, string senderName, string activeUserEmail, string? roleScope, WorkspaceContextResponse? response)
    {
        bool authStatusChanged = IsAuthenticated != isAuthenticated;
        bool userContextChanged = ActiveUserEmail != activeUserEmail?.Trim() || UserRoleScope != roleScope?.Trim();

        IsAuthenticated = isAuthenticated;
        SenderEmail = senderEmail?.Trim() ?? string.Empty;
        SenderName = senderName?.Trim() ?? string.Empty;
        ActiveUserEmail = activeUserEmail?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(roleScope))
        {
            UserRoleScope = roleScope.Trim();
        }

        CurrentContextResponse = response;

        // Fire targeted atomic events exactly once per transaction loop scope
        if (authStatusChanged)
        {
            NotifyAuthenticationChanged();
        }
        if (userContextChanged)
        {
            NotifyActiveUserChanged();
        }

        NotifyWorkspaceContextChanged();
    }

    public void ClearSessionStore()
    {
        IsAuthenticated = false;
        SenderEmail = string.Empty;
        SenderName = string.Empty;
        ActiveUserEmail = string.Empty;
        CurrentContextResponse = null;

        NotifyAuthenticationChanged();
        NotifyActiveUserChanged();
        NotifyWorkspaceContextChanged();
    }

    // Isolated Domain Telemetry Event Triggers
    private void NotifyAuthenticationChanged() => AuthenticationChanged?.Invoke();
    private void NotifyWorkspaceContextChanged() => WorkspaceContextChanged?.Invoke();
    private void NotifyActiveUserChanged() => ActiveUserChanged?.Invoke();
}
