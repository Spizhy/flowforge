import * as signalR from '@microsoft/signalr';
import { RunHistoryItem, WorkflowDefinition, WorkflowRunDetail } from '../types/workflow';

const BASE_URL = '/api/v1';

export class FlowForgeApi {
  private hubConnection: signalR.HubConnection | null = null;

  async initHub(
    runId: string,
    onStepStatus: (stepId: string, status: string) => void,
    onLog: (level: string, message: string, timestamp: string) => void
  ) {
    if (this.hubConnection) {
      await this.hubConnection.stop();
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/workflows')
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('StepStatusChanged', (data: { runId: string; stepId: string; status: string }) => {
      if (data.runId.toLowerCase() === runId.toLowerCase()) {
        onStepStatus(data.stepId, data.status);
      }
    });

    this.hubConnection.on('LogEmitted', (data: { runId: string; level: string; message: string; timestamp: string }) => {
      if (data.runId.toLowerCase() === runId.toLowerCase()) {
        onLog(data.level, data.message, data.timestamp);
      }
    });

    await this.hubConnection.start();
    await this.hubConnection.invoke('SubscribeToRun', runId);
  }

  async getWorkflows(): Promise<WorkflowDefinition[]> {
    const res = await fetch(`${BASE_URL}/workflows`);
    return res.json();
  }

  async getWorkflowById(id: string): Promise<WorkflowDefinition> {
    const res = await fetch(`${BASE_URL}/workflows/${id}`);
    return res.json();
  }

  async getRunsHistory(limit = 20): Promise<RunHistoryItem[]> {
    const res = await fetch(`${BASE_URL}/runs/history?limit=${limit}`);
    return res.json();
  }

  async getRunDetails(runId: string): Promise<WorkflowRunDetail> {
    const res = await fetch(`${BASE_URL}/runs/${runId}`);
    return res.json();
  }

  async triggerRun(workflowId: string, payload: { triggeredBy: string; inputPayload: string }) {
    const res = await fetch(`${BASE_URL}/runs/trigger/${workflowId}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    return res.json();
  }
}

export const flowForgeApi = new FlowForgeApi();
