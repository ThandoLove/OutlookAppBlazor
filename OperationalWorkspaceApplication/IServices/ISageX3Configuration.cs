
namespace OperationalWorkspaceApplication.IServices;
public interface ISageX3Configuration
{
    string BaseUrl { get; }
    string EndpointFolderName { get; }
    bool UseMocks { get; }
    bool UseMockAuth { get; }
    int HttpClientTimeoutSeconds { get; }
    string ClientId { get; }
    string ClientSecret { get; }
}
