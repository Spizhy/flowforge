# FlowForge Pipeline Step Specification

Pipeline steps are defined in JSON as a directed acyclic graph:

```json
[
  {
    "stepId": "extract-logs",
    "name": "Ingest Ingress Logs",
    "type": "Http",
    "dependsOn": []
  },
  {
    "stepId": "filter-4xx",
    "name": "Parse Client Errors",
    "type": "Transform",
    "dependsOn": ["extract-logs"]
  },
  {
    "stepId": "alert-pagerduty",
    "name": "Send Ops Alert",
    "type": "Http",
    "dependsOn": ["filter-4xx"]
  }
]
```

### Supported Step Types:
- `Http`: Webhook requests and REST API triggers.
- `Transform`: In-memory data sanitization, schema mapping, and PII masking.
- `Script`: Shell/Python worker tasks.
- `Sql`: Database query executions and migrations.
