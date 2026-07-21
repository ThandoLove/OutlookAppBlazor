


namespace OperationalWorkspaceApplication.Dtos;


public record AuditReportDto(int TotalTicketsFiled, int OpenCount, int HighPriorityCount, DateTime GeneratedAt, List<TicketDto> TicketLogs);


