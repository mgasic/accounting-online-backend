using System.Net;
using ERPAccounting.Common.Constants;

namespace ERPAccounting.Common.Exceptions;

public sealed class ConflictException(
    string detail,
    string? resourceId = null,
    string? resourceType = null,
    string? expectedETag = null,
    string? currentETag = null) : DomainException(HttpStatusCode.Conflict, ErrorMessages.ConflictTitle, detail, ErrorCodes.ConcurrencyConflict)
{
    public string? ResourceId { get; } = resourceId;

    public string? ResourceType { get; } = resourceType;

    public string? ExpectedETag { get; } = expectedETag;

    public string? CurrentETag { get; } = currentETag;
}
