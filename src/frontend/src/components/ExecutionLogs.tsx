import React, { useEffect, useRef } from 'react';

export interface LogLine {
  level: string;
  message: string;
  timestamp: string;
}

interface ExecutionLogsProps {
  logs: LogLine[];
  activeStepName?: string;
  onClear: () => void;
}

export const ExecutionLogs: React.FC<ExecutionLogsProps> = ({
  logs,
  activeStepName,
  onClear
}) => {
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [logs]);

  return (
    <div className="logs-panel">
      <div className="logs-header">
        <div className="logs-title-row">
          <span className="terminal-prompt">$</span>
          <h3>Live Execution Stream {activeStepName ? `(${activeStepName})` : ''}</h3>
        </div>
        <div className="logs-actions">
          <span className="logs-count">{logs.length} lines</span>
          <button className="btn-clear-logs" onClick={onClear}>Clear</button>
        </div>
      </div>

      <div className="logs-console">
        {logs.length === 0 ? (
          <div className="logs-empty">No streaming execution logs yet. Trigger a run to observe pipeline workers.</div>
        ) : (
          logs.map((log, index) => (
            <div key={index} className="log-row">
              <span className="log-time">
                {new Date(log.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' })}
              </span>
              <span className={`log-level level-${log.level.toLowerCase()}`}>
                [{log.level}]
              </span>
              <span className="log-msg">{log.message}</span>
            </div>
          ))
        )}
        <div ref={bottomRef} />
      </div>
    </div>
  );
};
