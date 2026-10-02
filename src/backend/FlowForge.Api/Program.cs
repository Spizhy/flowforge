using Microsoft.EntityFrameworkCore;
using FlowForge.Api.Hubs;
using FlowForge.Api.Workers;
using FlowForge.Infrastructure.Data;
using FlowForge.Infrastructure.Engine;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<FlowForgeDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=flowforge.db");
});

builder.Services.AddSignalR();
builder.Services.AddScoped<IWorkflowEngine, WorkflowEngine>();
builder.Services.AddHostedService<PipelineWorker>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:80", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");

app.MapControllers();
app.MapHub<WorkflowHub>("/hubs/workflows");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FlowForgeDbContext>();
    await DbInitializer.SeedAsync(db);
}

app.Run();
