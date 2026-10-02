import React from 'react';
import { RunHistoryItem } from '../types/workflow';

interface RunsHistoryTableProps {
  runs: RunHistoryItem[];
  onSelectRun: (runId: string) => void;
  selectedRunId: string | null;
}

export const RunsHistoryTable: React.FC<RunsHistoryTableProps> = ({
  runs,
  onSelectRun,
  selectedRunId
}) => {
  return (
    <div className="history-panel">
      <div className="panel-header">
        <h3>Pipeline Runs History</h3>
        <span className="badge-runs-count">{runs.length} Runs</span>
      </div>

      <div className="table-wrapper">
        <table className="runs-table">
          <thead>
            <tr>
              <th>Status</th>
              <th>Workflow Name</th>
              <th>Triggered By</th>
              <th>Steps Completed</th>
              <th>Duration</th>
              <th>Queued Time</th>
            </tr>
          </thead>
          <tbody>
            {runs.length === 0 ? (
              <tr>
                <td colSpan={6} style={{ textAlign: 'center', padding: '24px' }}>
                  No execution history available.
                </td>
              </tr>
            ) : (
              runs.map((r) => (
                <tr
                  key={r.id}
                  className={`run-row ${selectedRunId === r.id ? 'active' : ''}`}
                  onClick={() => onSelectRun(r.id)}
                >
                  <td>
                    <span className={`run-status-badge status-${r.status.toLowerCase()}`}>
                      {r.status}
                    </span>
                  </td>
                  <td><strong>{r.workflowName}</strong></td>
                  <td>{r.triggeredBy}</td>
                  <td>{r.succeededSteps} / {r.stepsCount} steps</td>
                  <td>{r.durationSeconds > 0 ? `${r.durationSeconds}s` : '---'}</td>
                  <td>{new Date(r.queuedAtUtc).toLocaleString()}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};
