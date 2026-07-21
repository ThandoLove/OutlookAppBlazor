
namespace OperationalWorkspaceApplication.Dtos;


public record CustomerDto(
    string Id,
    string CompanyName,
    string ContactName,
    string Email,
    string AccountStatus,
    decimal CreditLimit,
    decimal BalanceDue,
    decimal AvailableCredit,
    bool IsRiskExposureExceeded
);
