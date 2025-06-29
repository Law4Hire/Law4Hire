using Microsoft.EntityFrameworkCore;
using Law4Hire.Core.Entities;
using Law4Hire.Core.Interfaces; // Ensure this using directive is present
using Law4Hire.Infrastructure.Data.Contexts;

namespace Law4Hire.Infrastructure.Data.Repositories;

// Added ': IUserRepository' to implement the interface
public class UserRepository(Law4HireDbContext context) : IUserRepository
{
    private readonly Law4HireDbContext _context = context;

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.IntakeSessions)
            .Include(u => u.ServiceRequests)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> CreateAsync(User user)
    {
        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}

// Added ': IServicePackageRepository'
public class ServicePackageRepository(Law4HireDbContext context) : IServicePackageRepository
{
    private readonly Law4HireDbContext _context = context;

    public async Task<IEnumerable<ServicePackage>> GetAllActiveAsync()
    {
        return await _context.ServicePackages
            .Where(sp => sp.IsActive)
            .OrderBy(sp => sp.Type)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ServicePackage?> GetByIdAsync(int id)
    {
        return await _context.ServicePackages
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.Id == id);
    }

    public async Task<ServicePackage> CreateAsync(ServicePackage package)
    {
        package.CreatedAt = DateTime.UtcNow;
        _context.ServicePackages.Add(package);
        await _context.SaveChangesAsync();
        return package;
    }

    public async Task<ServicePackage> UpdateAsync(ServicePackage package)
    {
        _context.ServicePackages.Update(package);
        await _context.SaveChangesAsync();
        return package;
    }
}

// Added ': IIntakeSessionRepository'
public class IntakeSessionRepository(Law4HireDbContext context) : IIntakeSessionRepository
{
    private readonly Law4HireDbContext _context = context;

    public async Task<IntakeSession?> GetByIdAsync(Guid id)
    {
        return await _context.IntakeSessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IntakeSession> CreateAsync(IntakeSession session)
    {
        session.Id = Guid.NewGuid();
        session.StartedAt = DateTime.UtcNow;

        _context.IntakeSessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<IntakeSession> UpdateAsync(IntakeSession session)
    {
        _context.IntakeSessions.Update(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<IEnumerable<IntakeSession>> GetByUserIdAsync(Guid userId)
    {
        return await _context.IntakeSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartedAt)
            .AsNoTracking()
            .ToListAsync();
    }
}

// Added ': IServiceRequestRepository'
public class ServiceRequestRepository(Law4HireDbContext context) : IServiceRequestRepository
{
    private readonly Law4HireDbContext _context = context;

    public async Task<ServiceRequest?> GetByIdAsync(Guid id)
    {
        return await _context.ServiceRequests
            .Include(r => r.User)
            .Include(r => r.ServicePackage)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<ServiceRequest> CreateAsync(ServiceRequest request)
    {
        request.Id = Guid.NewGuid();
        request.CreatedAt = DateTime.UtcNow;

        _context.ServiceRequests.Add(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<ServiceRequest> UpdateAsync(ServiceRequest request)
    {
        _context.ServiceRequests.Update(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<IEnumerable<ServiceRequest>> GetByUserIdAsync(Guid userId)
    {
        return await _context.ServiceRequests
            .Where(r => r.UserId == userId)
            .Include(r => r.ServicePackage)
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }
}

// Added ': ILocalizedContentRepository'
public class LocalizedContentRepository(Law4HireDbContext context) : ILocalizedContentRepository
{
    private readonly Law4HireDbContext _context = context;

    public async Task<LocalizedContent?> GetContentAsync(string key, string language)
    {
        return await _context.LocalizedContents
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ContentKey == key && c.Language == language);
    }

    public async Task<IEnumerable<LocalizedContent>> GetAllForLanguageAsync(string language)
    {
        return await _context.LocalizedContents
            .Where(c => c.Language == language)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<LocalizedContent> CreateOrUpdateAsync(LocalizedContent content)
    {
        var existing = await _context.LocalizedContents
            .FirstOrDefaultAsync(c => c.ContentKey == content.ContentKey && c.Language == content.Language);

        if (existing != null)
        {
            existing.Content = content.Content;
            existing.Description = content.Description;
            existing.LastUpdated = DateTime.UtcNow;
            _context.LocalizedContents.Update(existing);
        }
        else
        {
            content.LastUpdated = DateTime.UtcNow;
            _context.LocalizedContents.Add(content);
        }

        await _context.SaveChangesAsync();
        return existing ?? content;
    }
}