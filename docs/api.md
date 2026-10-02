# FlowForge REST & SignalR API Reference

Base URL: `/api/v1`

---

## 1. REST Endpoints

### `GET /workflows`
Lists registered pipeline blueprints with aggregated run counts and last status.

### `GET /workflows/{id}`
Returns workflow definition including the serialized DAG steps JSON.

### `GET /runs/history`
Returns paginated execution history across all pipelines.
- **Query Parameters:**
  - `limit` (int, default: 20).

### `GET /runs/{id}`
Returns comprehensive run telemetry, step execution timestamps, and log lines.

### `POST /runs/trigger/{workflowId}`
Queues a new pipeline run for execution.
- **Request Body:**
```json
{
  "triggeredBy": "DataEngineerLead",
  "inputPayload": "{\"batchDate\": \"2026-10-02\", \"records\": 10000}"
}
```
- **Response 200 OK:**
```json
{
  "runId": "4c606e17-...",
  "status": "Queued",
  "message": "Pipeline successfully queued for distributed worker execution."
}
```

---

## 2. SignalR Hub: `/hubs/workflows`

- **Client Invocation:**
  - `SubscribeToRun(runId)`: Joins the execution broadcast group for `runId`.
  - `UnsubscribeFromRun(runId)`: Leaves the execution group.
- **Server Events Emitted:**
  - `StepStatusChanged`: `{ runId, stepId, status }`
  - `LogEmitted`: `{ runId, level, message, timestamp }`
