using OperationalWorkspaceApplication.Abstractions;
using OperationalWorkspaceApplication.Dtos;
using OperationalWorkspaceApplication.Requests.TicketRequest;
using OperationalWorkspaceApplication.Responses.TicketResponse;
using OperationalWorkspaceApplication.Responses.WorkspaceContextResponse;
using OperationalWorkspaceUI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OperationalWorkspaceUI.UIServices
{
    public class MockWorkspaceApiService : IWorkspaceApiService
    {
        public async Task<Result<OauthInitResponseDto>> InitializeOAuthChallengeAsync()
        {
            await Task.Delay(50);
            var mockChallenge = new OauthInitResponseDto("https://localhost:7013/mock-auth", "MOCK_STATE_GUID");
            return Result<OauthInitResponseDto>.Success(mockChallenge);
        }

        public async Task<Result<TokenExchangeResponseDto>> ExchangeTokenCodeAsync(string code)
        {
            await Task.Delay(50);
            var mockToken = new TokenExchangeResponseDto("MOCK_DEVELOPMENT_JWT_TOKEN", "Admin;Finance;Sales;Consultant");
            return Result<TokenExchangeResponseDto>.Success(mockToken);
        }

        public async Task<Result<WorkspaceContextResponse>> GetWorkspaceContextAsync(string email, string name, string activeUser, string userToken)
        {
            await Task.Delay(100);

            var mockCustomer = new CustomerDto(
                "BPC-ACME001",
                "Acme Global Manufacturing Ltd",
                name ?? "Jane Smith",
                email ?? "finance.manager@acmecorp.com",
                "Active",
                50000.00m,
                12450.00m,
                37550.00m,
                false
            );

            var mockDocs = new List<SageDocumentDto>
            {
                new SageDocumentDto("INV-2026-004", "Invoice", DateTime.UtcNow.AddDays(-2), 8450.00m, "USD", "Unpaid", "application/pdf", "BLOB-811", true),
                new SageDocumentDto("INV-2026-001", "Invoice", DateTime.UtcNow.AddDays(-30), 4000.00m, "USD", "Overdue", "application/pdf", "BLOB-204", true)
            };

            var mockContextResponse = new WorkspaceContextResponse(
                mockCustomer,
                mockDocs,
                "Customer needs 50 units of ABC — wants pricing & delivery time.",
                false,
                "✔ Credit lines standing clear inside safe operating metrics.",
                "Success"
            );

            return Result<WorkspaceContextResponse>.Success(mockContextResponse);
        }

        public async Task<Result<TicketActionResponse>> SubmitIncidentTicketAsync(CreateTicketCommand command, string userToken)
        {
            await Task.Delay(80);

            // FIX: Aligned parameters directly with the application record definition model (bool, string, string, string)
            var mockResponse = new TicketActionResponse(
                true,
                "TKT-2026-091",
                "High",
                "Incident logged cleanly into local mock data workspace structures."
            );

            return Result<TicketActionResponse>.Success(mockResponse);
        }
    }
}
