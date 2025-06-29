using Law4Hire.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Law4Hire.Infrastructure.Data.Contexts;

public class Law4HireDbContext(DbContextOptions<Law4HireDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<ServicePackage> ServicePackages { get; set; }
    public DbSet<IntakeSession> IntakeSessions { get; set; }
    public DbSet<IntakeQuestion> IntakeQuestions { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }
    public DbSet<LocalizedContent> LocalizedContents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // This will automatically apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}