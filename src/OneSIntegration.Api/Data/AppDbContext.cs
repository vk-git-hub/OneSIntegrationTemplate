using Microsoft.EntityFrameworkCore;
using OneSIntegration.Api.Models; 
 
namespace OneSIntegration.Api.Data; 
 
public class AppDbContext : DbContext 
{ 
    public DbSet<IntegrationLog> IntegrationLogs => Set<IntegrationLog>(); 
 
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { } 
 
    protected override void OnModelCreating(ModelBuilder modelBuilder) 
    { 
        modelBuilder.Entity<IntegrationLog>(entity => 
        { 
            entity.HasKey(e => e.Id);
            //entity.Property(e => e.Request).HasColumnType("jsonb");
            //entity.Property(e => e.Response).HasColumnType("jsonb"); 
        }); 
    } 
} 
