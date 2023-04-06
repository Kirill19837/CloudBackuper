namespace DbBackuper.Core.LeaseWeb.Contract;

public record ErrorDetails(string ErrorCode, string ErrorMessage, Guid CorrelationId, string UserMessage,
    string Reference);