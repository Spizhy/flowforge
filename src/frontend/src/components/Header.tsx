import React from 'react';

interface HeaderProps {
  activeView: 'editor' | 'runs';
  onViewChange: (view: 'editor' | 'runs') => void;
  onOpenTriggerModal: () => void;
}

export const Header: React.FC<HeaderProps> = ({
  activeView,
  onViewChange,
  onOpenTriggerModal
}) => {
  return (
    <header className="flowforge-header">
      <div className="header-brand">
        <div className="brand-icon">
          <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
            <circle cx="6" cy="6" r="3"></circle>
            <circle cx="6" cy="18" r="3"></circle>
            <circle cx="18" cy="12" r="3"></circle>
            <line x1="8.5" y1="7.5" x2="15.5" y2="10.5"></line>
            <line x1="8.5" y1="16.5" x2="15.5" y2="13.5"></line>
          </svg>
        </div>
        <span className="brand-title">FlowForge</span>
        <span className="brand-tag">DAG ORCHESTRATOR</span>
      </div>

      <nav className="header-nav">
        <button
          className={`nav-tab ${activeView === 'editor' ? 'active' : ''}`}
          onClick={() => onViewChange('editor')}
        >
          Visual DAG Canvas
        </button>
        <button
          className={`nav-tab ${activeView === 'runs' ? 'active' : ''}`}
          onClick={() => onViewChange('runs')}
        >
          Execution History
        </button>
      </nav>

      <div className="header-actions">
        <button className="btn-trigger" onClick={onOpenTriggerModal}>
          ▶ Trigger Pipeline Run
        </button>
      </div>
    </header>
  );
};
