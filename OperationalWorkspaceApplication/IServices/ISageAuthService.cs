namespace OperationalWorkspaceApplication.IServices;


public interface ISageAuthService
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
