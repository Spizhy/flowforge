# FlowForge — Distributed Workflow Orchestrator & Live DAG Engine

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4.svg?logo=dotnet)]()
[![React](https://img.shields.io/badge/React-18.2-61DAFB.svg?logo=react)]()
[![TypeScript](https://img.shields.io/badge/TypeScript-5.2-3178C6.svg?logo=typescript)]()
[![SignalR](https://img.shields.io/badge/SignalR-WebSockets-512BD4.svg)]()
[![SQLite](https://img.shields.io/badge/SQLite-Supported-003B57.svg?logo=sqlite)]()
[![Docker](https://img.shields.io/badge/Docker-Containers-2496ED.svg?logo=docker)]()
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

> **FlowForge** is a modern, event-driven distributed workflow orchestration platform inspired by Apache Airflow and Temporal. It features an interactive **visual DAG workflow canvas**, background execution worker pools, and **real-time bi-directional SignalR WebSocket telemetry** delivering live step transitions and streaming task execution logs directly to the browser.

---

## Key Features

- **Interactive Visual DAG Canvas**: Renders workflow steps as topological graph cards interconnected with smooth dynamic bezier curves and live animated status rings (`Pending`, `Running`, `Succeeded`, `Failed`).
- **Asynchronous Background Worker Engine**: Decouples client HTTP trigger requests from long-running task executions using .NET `BackgroundService` hosted worker queues.
- **Sub-Second Streaming Telemetry**: Broadcasts real-time step status transitions and console logs line-by-line using ASP.NET Core SignalR groups.
- **Topological Dependency Resolution**: Ensures task sequences strictly satisfy prerequisite `dependsOn` relationships before execution.
- **Live Terminal Log Console**: Terminal-style stream viewer with auto-scroll, severity levels (`[INFO]`, `[WARN]`, `[ERROR]`), and historical log inspection.
- **Execution History & Telemetry**: Track run durations, success ratios, trigger identity, and input parameters.
- **Extensible Step Types**: Out-of-the-box support for `Http` webhooks, `Transform` data cleaning, `Script` runner steps, and `Sql` operations.

---

## Visual DAG Canvas Preview

```
+---------------------------------------------------------------------------------------------------------+
| [⚡ FlowForge] DAG ORCHESTRATOR    [Visual DAG Canvas]  [Execution History]     [▶ Trigger Pipeline Run] |
+---------------------------------------------------------------------------------------------------------+
| ENTERPRISE CUSTOMER INGESTION & VECTOR PIPELINE                                                         |
| Current Run: 4c606e17... | Status: RUNNING                                                              |
|                                                                                                         |
| DAG TOPOLOGICAL EXECUTION GRAPH                                                                         |
| +-------------------+       +-------------------+       +-------------------+       +-----------------+ |
| | HTTP              |       | TRANSFORM         |       | TRANSFORM         |       | SCRIPT          | |
| | Ingest S3 Batch   |=====> | Validate Schema   |=====> | Anonymize PII     |=====> | Gen Embeddings  | |
| | [Succeeded (6s)]  |       | [Succeeded (7s)]  |       | [Running (Pulsing)]       | [Pending]       | |
| +-------------------+       +-------------------+       +-------------------+       +-----------------+ |
|                                                                                                         |
| $ LIVE EXECUTION STREAM (Anonymize PII & Redact Secrets)                                    [Clear Logs]|
| 14:02:15 [INFO]  Initializing task runner for step 'Anonymize PII & Redact Secrets' [Transform]         |
| 14:02:16 [INFO]  Scanning 45,210 customer event records for sensitive identifiers (email, credit card)|
| 14:02:17 [INFO]  SHA-256 masking applied to 1,290 PII fields. Output payload sanitized.                 |
+---------------------------------------------------------------------------------------------------------+
```

---

## Tech Stack

| Technology | Role | Justification |
| :--- | :--- | :--- |
| **C# / .NET 8** | Backend API & Queue Engine | Resilient background workers, strong typing, async concurrency, and low CPU overhead. |
| **ASP.NET Core Web API** | REST API Layer | Clean routing, Swagger OpenAPI specification, and dependency injection. |
| **SignalR WebSockets** | Real-Time Streaming | Instant server-to-client push for DAG node states and console logs without polling. |
| **Entity Framework Core 8** | Persistence | Object-relational mapping, migrations, and relationship management for DAG definitions. |
| **SQLite / PostgreSQL** | Database | Zero-configuration local database with clean migration path to PostgreSQL in production. |
| **React 18 & TypeScript** | Visual Frontend | SVG canvas rendering, stateful node updates, and modular component architecture. |
| **Vite** | Frontend Build Tool | Rapid development server with Hot Module Replacement and production bundling. |
| **Docker & Compose** | Deployment | Reproducible containerized execution across backend and frontend containers. |

---

## Project Structure

```
FlowForge/
├── docker-compose.yml             # Orchestrates backend worker and frontend UI
├── src/
│   ├── backend/
│   │   ├── FlowForge.sln
│   │   ├── FlowForge.Core/        # WorkflowDefinition, WorkflowRun, StepExecution entities
│   │   ├── FlowForge.Infrastructure/
│   │   │   ├── Data/              # FlowForgeDbContext & Seed Initializer
│   │   │   └── Engine/            # DAG Topological WorkflowEngine & step runners
│   │   └── FlowForge.Api/
│   │       ├── Hubs/              # WorkflowHub (SignalR WebSocket gateway)
│   │       ├── Workers/           # PipelineWorker (BackgroundService loop)
│   │       └── Controllers/       # Workflows & Runs REST controllers
│   └── frontend/
│       └── src/
│           ├── components/        # WorkflowCanvas, ExecutionLogs, RunsHistoryTable
│           ├── api/               # Typed REST API & SignalR client
│           └── styles/            # FlowForge dark theme design system
└── docs/
    ├── architecture.md            # Execution engine topology
    ├── api.md                     # REST & WebSocket endpoint specs
    └── pipeline-spec.md           # JSON DAG format specification
```

---

## Quick Start (Docker Compose)

Launch FlowForge with Docker:

```bash
docker-compose up --build
```

- **Visual Workflow Studio**: http://localhost
- **Backend API & Swagger**: http://localhost:5060/swagger

---

## Local Development Setup

### 1. Backend API & Pipeline Worker
```bash
cd src/backend/FlowForge.Api
dotnet run
```
API runs on `http://localhost:5060`. Seed workflows are automatically initialized on startup.

### 2. Frontend React Studio
```bash
cd src/frontend
npm install
npm run dev
```
Open `http://localhost:5173`.

---

## Running Automated Tests

```bash
dotnet test tests/FlowForge.Tests/FlowForge.Tests.csproj
```

---

## Publishing to GitHub

```bash
cd FlowForge
git init
git add .
git commit -m "feat: initial commit of FlowForge distributed workflow orchestrator"
git branch -M main
git remote add origin https://github.com/<your-username>/flowforge.git
git push -u origin main
```

---

## License

Distributed under the MIT License. See [LICENSE](LICENSE) for details.
