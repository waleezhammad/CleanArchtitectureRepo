using IntegrationService.Domain.Entities;

namespace IntegrationService.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Request entity
/// </summary>
public interface IRequestRepository
{
    Task<Request> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Request> GetByRequestIdAsync(string requestId, CancellationToken cancellationToken = default);
    Task<Request> GetByExternalRequestIdAsync(string externalRequestId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Request>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Request>> GetByStatusAsync(Domain.Enums.RequestStatus status, CancellationToken cancellationToken = default);
    Task AddAsync(Request request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Request request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Request request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Unit of Work interface for managing transactions
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
