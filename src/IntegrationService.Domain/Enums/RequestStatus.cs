namespace IntegrationService.Domain.Enums;

/// <summary>
/// Represents the status of a request
/// </summary>
public enum RequestStatus
{
    Pending = 0,
    Submitted = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Retrying = 5,
    Cancelled = 6
}
