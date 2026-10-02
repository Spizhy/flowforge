import React from 'react';
import { DagStep, StepStatus } from '../types/workflow';

interface WorkflowCanvasProps {
  steps: DagStep[];
  stepStatuses: Record<string, StepStatus>;
  selectedStepId: string | null;
  onSelectStep: (stepId: string) => void;
}

export const WorkflowCanvas: React.FC<WorkflowCanvasProps> = ({
  steps,
  stepStatuses,
  selectedStepId,
  onSelectStep
}) => {
  // Compute step positions for horizontal layout
  const nodeWidth = 220;
  const nodeHeight = 84;
  const horizontalGap = 80;

  const stepPositions = steps.map((s, idx) => ({
    ...s,
    x: 40 + idx * (nodeWidth + horizontalGap),
    y: 90
  }));

  const totalWidth = Math.max(900, steps.length * (nodeWidth + horizontalGap) + 100);

  return (
    <div className="canvas-container">
      <div className="canvas-toolbar">
        <span className="canvas-title">DAG Topological Execution Graph</span>
        <div className="canvas-legend">
          <span className="legend-badge legend-pending">Pending</span>
          <span className="legend-badge legend-running">Running</span>
          <span className="legend-badge legend-succeeded">Succeeded</span>
        </div>
      </div>

      <div className="canvas-scroll-area">
        <svg className="canvas-svg" width={totalWidth} height={260}>
          {/* Render Connecting Curved Bezier Lines */}
          {stepPositions.map((step, idx) => {
            if (idx === 0) return null;
            const prev = stepPositions[idx - 1];
            const startX = prev.x + nodeWidth;
            const startY = prev.y + nodeHeight / 2;
            const endX = step.x;
            const endY = step.y + nodeHeight / 2;
            const c1X = startX + horizontalGap / 2;
            const c2X = endX - horizontalGap / 2;

            const isDone = stepStatuses[prev.stepId] === 'Succeeded';

            return (
              <path
                key={`line-${prev.stepId}-${step.stepId}`}
                d={`M ${startX} ${startY} C ${c1X} ${startY}, ${c2X} ${endY}, ${endX} ${endY}`}
                fill="none"
                stroke={isDone ? '#10b981' : '#374151'}
                strokeWidth={isDone ? 2.5 : 2}
                strokeDasharray={isDone ? 'none' : '4 4'}
              />
            );
          })}

          {/* Render Step Nodes as SVG foreignObject */}
          {stepPositions.map((step) => {
            const status = stepStatuses[step.stepId] || 'Pending';
            const isSelected = selectedStepId === step.stepId;

            return (
              <foreignObject
                key={step.stepId}
                x={step.x}
                y={step.y}
                width={nodeWidth}
                height={nodeHeight}
                style={{ overflow: 'visible' }}
              >
                <div
                  className={`dag-node node-${status.toLowerCase()} ${isSelected ? 'selected' : ''}`}
                  onClick={() => onSelectStep(step.stepId)}
                >
                  <div className="node-header">
                    <span className="node-type">{step.type}</span>
                    <span className={`node-status-dot dot-${status.toLowerCase()}`}></span>
                  </div>
                  <div className="node-name">{step.name}</div>
                  <div className="node-footer">
                    <span className="node-id">{step.stepId}</span>
                    <span className="node-status-label">{status}</span>
                  </div>
                </div>
              </foreignObject>
            );
          })}
        </svg>
      </div>
    </div>
  );
};
