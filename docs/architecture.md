# FlowForge Architecture & Pipeline Specification

FlowForge is an event-driven distributed workflow orchestrator designed around **Directed Acyclic Graph (DAG)** pipelines, asynchronous execution queues, and live bidirectional WebSocket telemetry.

---

## 1. System Topology

```
+-------------------------------------------------------------+
|                  Interactive DAG Web Studio                 |
|          (React 18 + TypeScript + Canvas SVG + SignalR)     |
+------------------------------+------------------------------+
                               |
               HTTP REST / WS  |  Live node status transitions
                               |  and terminal log lines
                               v
+-------------------------------------------------------------+
|               ASP.NET Core 8 Web API & Hub                  |
|  - WorkflowsController: Query & provision DAG blueprints    |
|  - RunsController: Trigger, inspect, and query telemetry    |
|  - WorkflowHub: Multi-tenant SignalR execution rooms        |
+------------------------------+------------------------------+
                               |
                               v
+-------------------------------------------------------------+
|              WorkflowEngine & BackgroundWorker              |
|  - Asynchronous background queue consumer                   |
|  - Topological step sorting & dependency resolution         |
|  - Task executor runtime (HTTP, SQL, Python, Transform)     |
|  - Real-time log capture & event dispatch                   |
+------------------------------+------------------------------+
                               |
                               v
+-------------------------------------------------------------+
|                  SQLite / PostgreSQL Storage                |
|  - WorkflowDefinitions, WorkflowRuns, StepExecutions, Logs  |
+-------------------------------------------------------------+
```

---

## 2. Key Architectural Invariants

1. **DAG Topological Execution**:
   - Each workflow step declares a set of dependencies (`dependsOn`).
   - A step remains in `Pending` status until all antecedent dependencies achieve `Succeeded`.

2. **Real-Time Streaming Telemetry**:
   - Rather than polling execution states, the `WorkflowHub` pushes instantaneous state deltas (`StepStatusChanged`) and log lines (`LogEmitted`) to subscribers.

3. **Resilient Background Worker Lifecycle**:
   - Decouples client request duration from pipeline execution length via hosted .NET `BackgroundService` queue workers.
