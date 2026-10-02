using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using FlowForge.Core.Entities;
using FlowForge.Core.Enums;
using FlowForge.Infrastructure.Data;
using FlowForge.Infrastructure.Engine;
using Xunit;

namespace FlowForge.Tests;

public class WorkflowEngineTests
{
    private static FlowForgeDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FlowForgeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new FlowForgeDbContext(options);
    }

    [Fact]
    public async Task QueueRunAsync_CreatesRunAndExtractsDagSteps()
    {
        // Arrange
        var context = CreateContext();
        var def = new WorkflowDefinition
        {
            Name = "Test DAG",
            StepsJson = @"[
                {""stepId"": ""step-1"", ""name"": ""Extract"", ""type"": ""Http""},
                {""stepId"": ""step-2"", ""name"": ""Transform"", ""type"": ""Script""}
            ]"
        };
        context.WorkflowDefinitions.Add(def);
        await context.SaveChangesAsync();

        var engine = new WorkflowEngine(context, NullLogger<WorkflowEngine>.Instance);

        // Act
        var run = await engine.QueueRunAsync(def.Id, "UnitTestRunner", "{}");

        // Assert
        run.Should().NotBeNull();
        run.Status.Should().Be(RunStatus.Queued);
        run.StepExecutions.Should().HaveCount(2);
        run.StepExecutions.Select(s => s.StepId).Should().Contain(new[] { "step-1", "step-2" });
    }

    [Fact]
    public async Task ExecuteNextPendingRunAsync_ExecutesAllStepsAndTransitionsStatus()
    {
        // Arrange
        var context = CreateContext();
        var def = new WorkflowDefinition
        {
            Name = "Execution Test",
            StepsJson = @"[{""stepId"": ""step-1"", ""name"": ""Ping"", ""type"": ""Http""}]"
        };
        context.WorkflowDefinitions.Add(def);
        await context.SaveChangesAsync();

        var engine = new WorkflowEngine(context, NullLogger<WorkflowEngine>.Instance);
        var run = await engine.QueueRunAsync(def.Id, "TestTrigger", "{}");

        var statusEvents = new List<string>();
        var logEvents = new List<string>();

        // Act
        await engine.ExecuteNextPendingRunAsync(
            (runId, stepId, status) =>
            {
                statusEvents.Add($"{stepId}:{status}");
                return Task.CompletedTask;
            },
            (runId, level, msg) =>
            {
                logEvents.Add(msg);
                return Task.CompletedTask;
            }
        );

        // Assert
        var updatedRun = await context.WorkflowRuns
            .Include(r => r.StepExecutions)
            .FirstAsync(r => r.Id == run.Id);

        updatedRun.Status.Should().Be(RunStatus.Succeeded);
        updatedRun.StepExecutions.First().Status.Should().Be(StepStatus.Succeeded);
        statusEvents.Should().Contain("step-1:Running");
        statusEvents.Should().Contain("step-1:Succeeded");
        logEvents.Should().NotBeEmpty();
    }
}
