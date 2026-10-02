export type RunStatus = 'Queued' | 'Running' | 'Succeeded' | 'Failed' | 'Cancelled';
export type StepStatus = 'Pending' | 'Running' | 'Succeeded' | 'Failed' | 'Skipped';

export interface DagStep {
  stepId: string;
  name: string;
  type: string;
  dependsOn?: string[];
}

export interface WorkflowDefinition {
  id: string;
  name: string;
  description: string;
  category: string;
  stepsJson: string;
  totalRuns: number;
  lastRunStatus?: string;
  createdAtUtc: string;
}

export interface StepLog {
  logLevel: string;
  message: string;
  timestampUtc: string;
}

export interface StepExecution {
  id: string;
  stepId: string;
  name: string;
  type: string;
  status: StepStatus;
  startedAtUtc?: string | null;
  completedAtUtc?: string | null;
  logs: StepLog[];
}

export interface WorkflowRunDetail {
  id: string;
  workflowDefinitionId: string;
  workflowName: string;
  status: RunStatus;
  triggeredBy: string;
  inputPayloadJson: string;
  queuedAtUtc: string;
  startedAtUtc?: string | null;
  completedAtUtc?: string | null;
  steps: StepExecution[];
}

export interface RunHistoryItem {
  id: string;
  workflowDefinitionId: string;
  workflowName: string;
  status: RunStatus;
  triggeredBy: string;
  queuedAtUtc: string;
  startedAtUtc?: string;
  completedAtUtc?: string;
  durationSeconds: number;
  stepsCount: number;
  succeededSteps: number;
}
