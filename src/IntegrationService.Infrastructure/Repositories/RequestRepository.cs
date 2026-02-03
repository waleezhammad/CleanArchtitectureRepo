using IntegrationService.Application.Common.Interfaces;
using IntegrationService.Domain.Entities;
using IntegrationService.Domain.Enums;
using IntegrationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IntegrationService.Infrastructure.Repositories;

/// <summary>
/// Implementation of IRequestRepository
/// </summary>
public class RequestRepository : IRequestRepository
{
    private readonly ApplicationDbContext _context;

    public RequestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Request> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Request> GetByRequestIdAsync(string requestId, CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .FirstOrDefaultAsync(r => r.RequestId == requestId, cancellationToken);
    }

    public async Task<Request> GetByExternalRequestIdAsync(string externalRequestId, CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .FirstOrDefaultAsync(r => r.ExternalRequestId == externalRequestId, cancellationToken);
    }

    public async Task<IEnumerable<Request>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Request>> GetByStatusAsync(RequestStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Request request, CancellationToken cancellationToken = default)
    {
        await _context.Requests.AddAsync(request, cancellationToken);
    }

    public Task UpdateAsync(Request request, CancellationToken cancellationToken = default)
    {
        _context.Requests.Update(request);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Request request, CancellationToken cancellationToken = default)
    {
        _context.Requests.Remove(request);
        return Task.CompletedTask;
    }
}
