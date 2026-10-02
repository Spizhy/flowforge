import React, { useState } from 'react';
import { WorkflowDefinition } from '../types/workflow';

interface WorkflowTriggerModalProps {
  isOpen: boolean;
  onClose: () => void;
  workflows: WorkflowDefinition[];
  onTrigger: (workflowId: string, payload: { triggeredBy: string; inputPayload: string }) => Promise<void>;
}

export const WorkflowTriggerModal: React.FC<WorkflowTriggerModalProps> = ({
  isOpen,
  onClose,
  workflows,
  onTrigger
}) => {
  const [selectedWorkflowId, setSelectedWorkflowId] = useState(workflows[0]?.id || '');
  const [triggeredBy, setTriggeredBy] = useState('SeniorDataEngineer');
  const [inputPayload, setInputPayload] = useState('{\n  "batchSize": 50000,\n  "targetEnv": "production"\n}');
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedWorkflowId) return;

    try {
      setIsSubmitting(true);
      await onTrigger(selectedWorkflowId, { triggeredBy, inputPayload });
      onClose();
    } catch (err: any) {
      alert(err.message || 'Failed to trigger run');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-dialog" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3>Trigger Pipeline Execution</h3>
          <button className="btn-modal-close" onClick={onClose}>&times;</button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-group">
            <label>Select Target Workflow</label>
            <select
              value={selectedWorkflowId}
              onChange={(e) => setSelectedWorkflowId(e.target.value)}
              required
            >
              {workflows.map((w) => (
                <option key={w.id} value={w.id}>
                  {w.name} ({w.category})
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label>Triggered By Identity</label>
            <input
              type="text"
              required
              value={triggeredBy}
              onChange={(e) => setTriggeredBy(e.target.value)}
            />
          </div>

          <div className="form-group">
            <label>Execution JSON Parameters</label>
            <textarea
              rows={4}
              value={inputPayload}
              onChange={(e) => setInputPayload(e.target.value)}
            />
          </div>

          <div className="modal-buttons">
            <button type="button" className="btn-cancel" onClick={onClose} disabled={isSubmitting}>
              Cancel
            </button>
            <button type="submit" className="btn-confirm" disabled={isSubmitting}>
              {isSubmitting ? 'Queuing Run...' : 'Queue Pipeline Run'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
