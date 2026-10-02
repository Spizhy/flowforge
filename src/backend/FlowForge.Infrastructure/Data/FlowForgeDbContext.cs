using Microsoft.EntityFrameworkCore;
using FlowForge.Core.Entities;

namespace FlowForge.Infrastructure.Data;

public class FlowForgeDbContext : DbContext
{
    public FlowForgeDbContext(DbContextOptions<FlowForgeDbContext> options) : base(options)
    {
    }

    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowRun> WorkflowRuns => Set<WorkflowRun>();
    public DbSet<StepExecution> StepExecutions => Set<StepExecution>();
    public DbSet<StepLog> StepLogs => Set<StepLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WorkflowDefinition>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(150);
            e.HasIndex(x => x.Name);
        });

        modelBuilder.Entity<WorkflowRun>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.WorkflowDefinition)
             .WithMany(w => w.Runs)
             .HasForeignKey(x => x.WorkflowDefinitionId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.QueuedAtUtc);
        });

        modelBuilder.Entity<StepExecution>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.WorkflowRun)
             .WithMany(r => r.StepExecutions)
             .HasForeignKey(x => x.WorkflowRunId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StepLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.StepExecution)
             .WithMany(s => s.Logs)
             .HasForeignKey(x => x.StepExecutionId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.TimestampUtc);
        });
    }
}
