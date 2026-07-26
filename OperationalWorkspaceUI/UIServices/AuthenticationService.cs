using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OperationalWorkspaceApplication.Exceptions;
using OperationalWorkspaceApplication.Dtos;
using OperationalWorkspaceApplication.Abstractions;
using OperationalWorkspaceUI.UIState;
using System;
using System.Threading.Tasks;

namespace OperationalWorkspaceUI.UIServices
{
    public class AuthenticationService
    {
        private readonly IWorkspaceApiService _apiService;
        private readonly UIStateContainer _stateContainer;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            IWorkspaceApiService apiService,
            UIStateContainer stateContainer,
            IConfiguration config,
            ILogger<AuthenticationService> logger)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _stateContainer = stateContainer ?? throw new ArgumentNullException(nameof(stateContainer));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task LoginAsync()
        {
            _logger.LogInformation("Authentication process initiated.");

            bool useMocks = _config.GetValue<bool>("SageX3Settings:UseMocks");
            bool useMockAuth = _config.GetValue<bool>("SageX3Settings:UseMockAuth");

            if (useMocks || useMockAuth)
            {
                _logger.LogInformation("Applying development environment mock identity credentials.");

                string mockSenderEmail = _config["SageX3Settings:MockData:SenderEmail"] ?? "finance.manager@acmecorp.com";
                string mockSenderName = _config["SageX3Settings:MockData:SenderName"] ?? "Jane Smith";
                string mockActiveUser = _config["SageX3Settings:MockData:ActiveUserEmail"] ?? "admin.manager@yourcompany.com";

                _stateContainer.UpdateSession(
                    isAuthenticated: true,
                    senderEmail: mockSenderEmail,
                    senderName: mockSenderName,
                    activeUserEmail: mockActiveUser,
                    roleScope: "Admin;Finance;Sales;Consultant",
                    response: null
                );
                return;
            }

            _logger.LogInformation("Redirecting session context out to the external identity challenge endpoint.");
            var challengeResult = await _apiService.InitializeOAuthChallengeAsync();

            if (challengeResult == null || !challengeResult.IsSuccess || challengeResult.Value == null)
            {
                _logger.LogError("Failed to securely resolve structural OAuth initialization tokens from Syracuse.");
                throw new SageAuthenticationException("Authentication initialization failed.");
            }
        }
        public async Task ExchangeTokenAsync(string secureCallbackCode)
        {
            if (string.IsNullOrWhiteSpace(secureCallbackCode))
            {
                _logger.LogWarning("Token exchange rejected: Provided verification string parameter is empty.");
                throw new SageAuthenticationException("Invalid authentication code.");
            }

            try
            {
                _logger.LogInformation("Dispatching verification callback code context to the identity broker.");
                var result = await _apiService.ExchangeTokenCodeAsync(secureCallbackCode);

                if (result != null && result.IsSuccess && result.Value != null)
                {
                    var payload = result.Value;
                    _logger.LogInformation("Token handshake completed successfully. Authorizing session.");

                    var contextResult = await _apiService.GetWorkspaceContextAsync(
                        _stateContainer.SenderEmail,
                        _stateContainer.SenderName,
                        "authenticated.user@yourcompany.com",
                        payload.Token ?? string.Empty
                    );

                    _stateContainer.UpdateSession(
                        isAuthenticated: true,
                        senderEmail: _stateContainer.SenderEmail,
                        senderName: _stateContainer.SenderName,
                        activeUserEmail: "authenticated.user@yourcompany.com",
                        roleScope: payload.AssignedUserScope ?? "Sales;Consultant",
                        response: (contextResult != null && contextResult.IsSuccess) ? contextResult.Value : null
                    );
                }
                else
                {
                    _logger.LogError("The identity broker service rejected the transaction verification tokens.");
                    throw new SageAuthenticationException("Exchange verification rejected.");
                }
            }
            catch (Exception ex) when (ex is not SageAuthenticationException)
            {
                _logger.LogCritical(ex, "A system exception occurred during the secure validation loop.");
                throw;
            }
        }
        public void Logout()
        {
            _logger.LogInformation("Terminating active user workspace session.");
            try
            {
                _stateContainer.ClearSessionStore();
                _logger.LogInformation("Session stores flushed. Application user unauthenticated.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanly discard user parameters from memory stores.");
                throw new SageAuthenticationException("Logout processing failed.", ex);
            }
        }
    }
}
