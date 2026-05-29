-- SQLite schema for TicketTracking bounded context
-- This is a hand-rolled schema as per ADR-009

CREATE TABLE IF NOT EXISTS tickets (
    repo TEXT NOT NULL,
    github_issue_number INTEGER NOT NULL,
    title TEXT NOT NULL,
    status TEXT NOT NULL,
    agent TEXT,
    retry_count INTEGER NOT NULL,
    github_url TEXT NOT NULL,
    created_at_utc TEXT NOT NULL,
    updated_at_utc TEXT NOT NULL,
    closed_at_utc TEXT,
    PRIMARY KEY (repo, github_issue_number)
);

CREATE INDEX IF NOT EXISTS idx_tickets_repo ON tickets(repo);
CREATE INDEX IF NOT EXISTS idx_tickets_status ON tickets(status);
CREATE INDEX IF NOT EXISTS idx_tickets_agent ON tickets(agent);
