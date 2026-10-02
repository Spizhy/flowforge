import React, { useState, useEffect } from 'react';
import { Header } from './components/Header';
import { WorkflowCanvas } from './components/WorkflowCanvas';
import { ExecutionLogs, LogLine } from './components/ExecutionLogs';
import { RunsHistoryTable } from './components/RunsHistoryTable';
import { WorkflowTriggerModal } from './components/WorkflowTriggerModal';
import { flowForgeApi } from './api/flowForgeApi';
import { DagStep, RunHistoryItem, StepStatus, WorkflowDefinition } from './types/workflow';

export const App: React.FC = () => {
  const [activeView, setActiveView] = useState<'editor' | 'runs'>('editor');
  const [workflows, setWorkflows] = useState<WorkflowDefinition[]>([]);
  const [selectedWorkflow, setSelectedWorkflow] = useState<WorkflowDefinition | null>(null);
  const [runs, setRuns] = useState<RunHistoryItem[]>([]);
  const [selectedRunId, setSelectedRunId] = useState<string | null>(null);
  const [activeRunStatus, setActiveRunStatus] = useState<string>('Idle');

  // Active DAG Canvas State
  const [dagSteps, setDagSteps] = useState<DagStep[]>([]);
  const [stepStatuses, setStepStatuses] = useState<Record<string, StepStatus>>({});
  const [selectedStepId, setSelectedStepId] = useState<string | null>(null);
  const [logs, setLogs] = useState<LogLine[]>([]);

  const [isTriggerModalOpen, setIsTriggerModalOpen] = useState(false);

  // Load initial workflows and history
  useEffect(() => {
    flowForgeApi.getWorkflows().then((wfList) => {
      setWorkflows(wfList);
      if (wfList.length > 0) {
        setSelectedWorkflow(wfList[0]);
        try {
          const parsed = JSON.parse(wfList[0].stepsJson);
          setDagSteps(parsed);
          const initialStatuses: Record<string, StepStatus> = {};
          parsed.forEach((s: DagStep) => {
            initialStatuses[s.stepId] = 'Pending';
          });
          setStepStatuses(initialStatuses);
        } catch (e) {
          console.error(e);
        }
      }
    }).catch(console.error);

    loadHistory();
  }, []);

  const loadHistory = () => {
    flowForgeApi.getRunsHistory(15).then(setRuns).catch(console.error);
  };

  const handleTriggerRun = async (workflowId: string, payload: { triggeredBy: string; inputPayload: string }) => {
    const result = await flowForgeApi.triggerRun(workflowId, payload);
    const runId = result.runId;
    setSelectedRunId(runId);
    setActiveRunStatus('Running');
    setActiveView('editor');

    // Reset step statuses to pending
    const resetStatuses: Record<string, StepStatus> = {};
    dagSteps.forEach((s) => { resetStatuses[s.stepId] = 'Pending'; });
    setStepStatuses(resetStatuses);

    setLogs((prev) => [
      {
        level: 'INFO',
        message: `Pipeline run ${runId} queued. Initializing real-time SignalR telemetry stream...`,
        timestamp: new Date().toISOString()
      },
      ...prev
    ]);

    // Connect SignalR WebSocket to this specific execution run
    await flowForgeApi.initHub(
      runId,
      (stepId, status) => {
        setStepStatuses((prev) => ({
          ...prev,
          [stepId]: status as StepStatus
        }));
      },
      (level, message, timestamp) => {
        setLogs((prev) => [
          ...prev,
          { level, message, timestamp }
        ]);
        loadHistory();
      }
    );
  };

  const handleSelectRunFromHistory = async (runId: string) => {
    setSelectedRunId(runId);
    setActiveView('editor');
    try {
      const details = await flowForgeApi.getRunDetails(runId);
      setActiveRunStatus(details.status);
      const statuses: Record<string, StepStatus> = {};
      const runLogs: LogLine[] = [];

      details.steps.forEach((s) => {
        statuses[s.stepId] = s.status;
        s.logs.forEach((l) => {
          runLogs.push({
            level: l.logLevel,
            message: l.message,
            timestamp: l.timestampUtc
          });
        });
      });

      setStepStatuses(statuses);
      setLogs(runLogs);
    } catch (err) {
      console.error(err);
    }
  };

  return (
    <div className="flowforge-app">
      <Header
        activeView={activeView}
        onViewChange={setActiveView}
        onOpenTriggerModal={() => setIsTriggerModalOpen(true)}
      />

      <main className="main-content">
        {selectedWorkflow && (
          <div className="workflow-meta-card">
            <div>
              <h2 className="meta-title">{selectedWorkflow.name}</h2>
              <p className="meta-desc">{selectedWorkflow.description}</p>
            </div>
            <div className="meta-run-status">
              Current Run: <strong style={{ color: '#8b5cf6' }}>{selectedRunId ? selectedRunId.slice(0, 8) : 'None'}</strong> | Status: <strong>{activeRunStatus}</strong>
            </div>
          </div>
        )}

        {activeView === 'editor' ? (
          <>
            <WorkflowCanvas
              steps={dagSteps}
              stepStatuses={stepStatuses}
              selectedStepId={selectedStepId}
              onSelectStep={setSelectedStepId}
            />

            <ExecutionLogs
              logs={logs}
              activeStepName={dagSteps.find((s) => s.stepId === selectedStepId)?.name}
              onClear={() => setLogs([])}
            />
          </>
        ) : (
          <RunsHistoryTable
            runs={runs}
            onSelectRun={handleSelectRunFromHistory}
            selectedRunId={selectedRunId}
          />
        )}
      </main>

      <WorkflowTriggerModal
        isOpen={isTriggerModalOpen}
        onClose={() => setIsTriggerModalOpen(false)}
        workflows={workflows}
        onTrigger={handleTriggerRun}
      />
    </div>
  );
};
