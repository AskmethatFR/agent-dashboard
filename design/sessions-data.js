// Sessions data — 1 session = 1 agent × 1 ticket × 1 pass
// Today is wed may 20, 2026

window.AGENTS = {
  PM: { id: "PM", name: "PM",        glyph: "PM", role: "Project Manager" },
  AR: { id: "AR", name: "Architect", glyph: "Ar", role: "Project Architect" },
  DA: { id: "DA", name: "DevA",      glyph: "Da", role: "Developer A" },
  DB: { id: "DB", name: "DevB",      glyph: "Db", role: "Developer B" },
  QA: { id: "QA", name: "QA",        glyph: "Qa", role: "QA" },
  SE: { id: "SE", name: "Security",  glyph: "Se", role: "Security" },
};

// Outcome legend:
//   done       — terminée propre, transition aval acceptée
//   retry      — n retries < seuil
//   warn       — retry 2/3
//   danger     — retry 3/3
//   escalated  — escalade levée
//   running    — en cours
//   failed     — échec terminal sans escalade (rare)

function s(opts) {
  return {
    id: opts.id,
    agent: opts.agent,
    ticket: opts.ticket,
    title: opts.title,
    role: opts.role || "implement",
    started: opts.started,         // "HH:MM" same day, or "yyyy-mm-dd HH:MM"
    ended: opts.ended || null,
    durationMin: opts.durationMin || null,
    events: opts.events,
    findings: opts.findings || 0,
    outcome: opts.outcome,
    retryN: opts.retryN || 0,
    lastEvent: opts.lastEvent,
    activity: opts.activity || null, // for running
    cost: opts.cost || null,          // { tokens, dollars }
    model: opts.model || "claude-sonnet-4.5",
    day: opts.day, // "today" | "yesterday" | "2days" | dateString
  };
}

window.SESSIONS = [
  // ---- RUNNING ----
  s({ id: "s_19f3", agent: "DA", ticket: 488, title: "parse OAuth state and validate signature",
      role: "implement", started: "17:28", events: 24, outcome: "running", retryN: 0,
      activity: "running unit tests · 14/22 passing", day: "today",
      cost: { tokens: 38420, dollars: 0.31 } }),
  s({ id: "s_1a02", agent: "QA", ticket: 466, title: "passkey enrollment: device-bound credential id",
      role: "validate", started: "17:36", events: 7, outcome: "running", retryN: 0,
      activity: "reviewing scenarios on staging · 2 pending", day: "today",
      cost: { tokens: 12100, dollars: 0.09 } }),

  // ---- TODAY ----
  s({ id: "s_18e1", agent: "DB", ticket: 482, title: "add rate-limit headers to all /api responses",
      role: "implement", started: "17:21", ended: "17:34", durationMin: 13, events: 24,
      outcome: "done", retryN: 0, lastEvent: "cross-review approved by DevA · ready for QA",
      day: "today", cost: { tokens: 41200, dollars: 0.43 } }),

  s({ id: "s_18d4", agent: "DA", ticket: 487, title: "refactor auth middleware: drop legacy cookie path",
      role: "implement", started: "16:48", ended: "17:12", durationMin: 24, events: 31,
      outcome: "retry", retryN: 1, lastEvent: "DevB flagged missing test on csrf header propagation",
      findings: 2, day: "today", cost: { tokens: 58900, dollars: 0.62 } }),

  s({ id: "s_18c9", agent: "PM", ticket: 460, title: "search: support is:open filter on /tasks",
      role: "validate", started: "16:33", ended: "16:45", durationMin: 12, events: 18,
      outcome: "done", retryN: 0, lastEvent: "spec acceptance criteria all green · closed and deployed",
      day: "today", cost: { tokens: 19500, dollars: 0.18 } }),

  s({ id: "s_18b2", agent: "DB", ticket: 471, title: "fix race condition in invite acceptance flow",
      role: "implement", started: "16:02", ended: "16:51", durationMin: 49, events: 44,
      outcome: "warn", retryN: 2, lastEvent: "QA: still reproducible under concurrent load (3 of 50 runs)",
      findings: 4, day: "today", cost: { tokens: 82100, dollars: 0.91 } }),

  s({ id: "s_18a0", agent: "AR", ticket: 455, title: "CSRF on /api/admin actions",
      role: "arbitrate", started: "15:44", ended: "16:11", durationMin: 27, events: 36,
      outcome: "escalated", retryN: 0, lastEvent: "QA/Security disagree on token rotation cadence · escalating to PM",
      findings: 6, day: "today", cost: { tokens: 71400, dollars: 0.78 } }),

  s({ id: "s_188f", agent: "SE", ticket: 473, title: "harden CSP: drop unsafe-inline from styles",
      role: "audit", started: "14:58", ended: "15:09", durationMin: 11, events: 19,
      outcome: "done", retryN: 0, lastEvent: "no high-severity findings · 1 low (deprecated nonce strategy)",
      findings: 1, day: "today", cost: { tokens: 23800, dollars: 0.21 } }),

  s({ id: "s_1879", agent: "PM", ticket: 459, title: "fix N+1 in /v1/projects?expand=members",
      role: "validate", started: "14:31", ended: "14:39", durationMin: 8, events: 14,
      outcome: "done", retryN: 0, lastEvent: "benchmark confirms 14x improvement · closed",
      day: "today", cost: { tokens: 15200, dollars: 0.14 } }),

  s({ id: "s_1864", agent: "QA", ticket: 461, title: "billing portal: cancel-then-restore flow",
      role: "validate", started: "13:55", ended: "14:17", durationMin: 22, events: 27,
      outcome: "retry", retryN: 1, lastEvent: "edge case: yearly plan restore preserves discount?",
      findings: 1, day: "today", cost: { tokens: 34900, dollars: 0.32 } }),

  s({ id: "s_184d", agent: "DA", ticket: 476, title: "extract useDebouncedValue from search page",
      role: "implement", started: "13:18", ended: "13:27", durationMin: 9, events: 16,
      outcome: "done", retryN: 0, lastEvent: "approved on first pass · types inferred from caller",
      day: "today", cost: { tokens: 17800, dollars: 0.16 } }),

  s({ id: "s_1838", agent: "DB", ticket: 476, title: "extract useDebouncedValue from search page",
      role: "review", started: "13:27", ended: "13:33", durationMin: 6, events: 9,
      outcome: "done", retryN: 0, lastEvent: "lgtm · suggested 1 jsdoc comment, applied inline",
      day: "today", cost: { tokens: 9400, dollars: 0.08 } }),

  s({ id: "s_181c", agent: "AR", ticket: 495, title: "add per-org rate limit with redis token bucket",
      role: "spec", started: "11:42", ended: "12:08", durationMin: 26, events: 33,
      outcome: "done", retryN: 0, lastEvent: "tech spec written · 4 unknowns resolved by reading existing code",
      day: "today", cost: { tokens: 64200, dollars: 0.71 } }),

  s({ id: "s_1808", agent: "PM", ticket: 502, title: "[bug] settings page 404 after sso login",
      role: "refine", started: "11:21", ended: "11:25", durationMin: 4, events: 6,
      outcome: "done", retryN: 0, lastEvent: "reproduced · steps documented · ready for spec",
      day: "today", cost: { tokens: 7100, dollars: 0.06 } }),

  s({ id: "s_17f4", agent: "DA", ticket: 478, title: "cache org membership lookup (LRU, 5 min)",
      role: "implement", started: "10:44", ended: "11:18", durationMin: 34, events: 38,
      outcome: "retry", retryN: 1, lastEvent: "DevB: cache invalidation on role-change is racy under multi-region",
      findings: 2, day: "today", cost: { tokens: 51200, dollars: 0.55 } }),

  s({ id: "s_17e1", agent: "SE", ticket: 485, title: "feat(billing): proration on plan downgrade",
      role: "audit", started: "10:02", ended: "10:14", durationMin: 12, events: 17,
      outcome: "done", retryN: 0, lastEvent: "no security implications · pricing inputs are server-side only",
      day: "today", cost: { tokens: 21300, dollars: 0.19 } }),

  s({ id: "s_17c8", agent: "DB", ticket: 485, title: "feat(billing): proration on plan downgrade",
      role: "implement", started: "09:18", ended: "10:02", durationMin: 44, events: 41,
      outcome: "done", retryN: 0, lastEvent: "approved · proration logic mirrors Stripe spec exactly",
      day: "today", cost: { tokens: 79500, dollars: 0.84 } }),

  // ---- YESTERDAY (may 19) ----
  s({ id: "s_17ab", agent: "DB", ticket: 463, title: "fix refresh-token replay vulnerability",
      role: "implement", started: "2026-05-19 23:14", ended: "2026-05-20 00:26", durationMin: 72, events: 78,
      outcome: "escalated", retryN: 3, lastEvent: "max retries reached · DevA review keeps rejecting the rotation strategy",
      findings: 9, day: "yesterday", cost: { tokens: 142800, dollars: 1.54 } }),

  s({ id: "s_178e", agent: "AR", ticket: 491, title: "OAuth state parameter must be HMAC-signed",
      role: "spec", started: "2026-05-19 19:42", ended: "2026-05-19 20:11", durationMin: 29, events: 35,
      outcome: "done", retryN: 0, lastEvent: "spec finalized · 3 acceptance scenarios defined",
      day: "yesterday", cost: { tokens: 67200, dollars: 0.74 } }),

  s({ id: "s_1772", agent: "DA", ticket: 468, title: "normalize 4xx error envelope (RFC 7807)",
      role: "implement", started: "2026-05-19 17:31", ended: "2026-05-19 17:58", durationMin: 27, events: 32,
      outcome: "done", retryN: 0, lastEvent: "approved · all 47 error sites migrated",
      day: "yesterday", cost: { tokens: 52400, dollars: 0.56 } }),

  s({ id: "s_175d", agent: "QA", ticket: 451, title: "feat: invite via email link, 7d ttl",
      role: "validate", started: "2026-05-19 16:08", ended: "2026-05-19 16:31", durationMin: 23, events: 28,
      outcome: "done", retryN: 0, lastEvent: "all scenarios pass · edge case for expired link returns 410",
      day: "yesterday", cost: { tokens: 39800, dollars: 0.41 } }),

  s({ id: "s_173e", agent: "DA", ticket: 463, title: "fix refresh-token replay vulnerability",
      role: "implement", started: "2026-05-19 14:22", ended: "2026-05-19 16:51", durationMin: 149, events: 91,
      outcome: "warn", retryN: 2, lastEvent: "DevB rejected: token rotation should happen pre-response, not post",
      findings: 5, day: "yesterday", cost: { tokens: 168200, dollars: 1.81 } }),

  s({ id: "s_1722", agent: "DB", ticket: 442, title: "fix(ui): dark-mode contrast on disabled buttons",
      role: "implement", started: "2026-05-19 13:08", ended: "2026-05-19 13:14", durationMin: 6, events: 8,
      outcome: "done", retryN: 0, lastEvent: "3-line CSS change · approved without comment",
      day: "yesterday", cost: { tokens: 6200, dollars: 0.05 } }),

  s({ id: "s_1708", agent: "PM", ticket: 448, title: "remove unused legacyAuth feature flag",
      role: "refine", started: "2026-05-19 11:33", ended: "2026-05-19 11:38", durationMin: 5, events: 7,
      outcome: "done", retryN: 0, lastEvent: "scope clarified · 14 callsites identified",
      day: "yesterday", cost: { tokens: 9800, dollars: 0.08 } }),

  s({ id: "s_16f1", agent: "SE", ticket: 456, title: "audit log: redact bearer tokens in dump",
      role: "audit", started: "2026-05-19 10:24", ended: "2026-05-19 10:37", durationMin: 13, events: 19,
      outcome: "done", retryN: 0, lastEvent: "redaction confirmed at 3 layers · added test for nested tokens",
      findings: 0, day: "yesterday", cost: { tokens: 24100, dollars: 0.22 } }),

  s({ id: "s_16d8", agent: "AR", ticket: 489, title: "migrate logger to pino, structured JSON output",
      role: "spec", started: "2026-05-19 09:14", ended: "2026-05-19 09:52", durationMin: 38, events: 42,
      outcome: "done", retryN: 0, lastEvent: "spec done · backwards-compat layer designed for legacy log readers",
      day: "yesterday", cost: { tokens: 84200, dollars: 0.92 } }),

  // ---- 2 DAYS AGO (may 18) ----
  s({ id: "s_16ba", agent: "DB", ticket: 445, title: "docs: agent escalation protocol v2",
      role: "implement", started: "2026-05-18 22:11", ended: "2026-05-18 22:28", durationMin: 17, events: 21,
      outcome: "done", retryN: 0, lastEvent: "merged · diagrams generated via mermaid",
      day: "2days", cost: { tokens: 32400, dollars: 0.31 } }),

  s({ id: "s_169e", agent: "QA", ticket: 441, title: "migrate logger to pino, structured JSON output",
      role: "validate", started: "2026-05-18 19:01", ended: "2026-05-18 19:33", durationMin: 32, events: 36,
      outcome: "retry", retryN: 1, lastEvent: "log format breaks downstream dashboard · DevA addressing",
      findings: 2, day: "2days", cost: { tokens: 54300, dollars: 0.58 } }),

  s({ id: "s_167a", agent: "DA", ticket: 452, title: "settings: org-level SSO enforcement toggle",
      role: "implement", started: "2026-05-18 15:42", ended: "2026-05-18 16:18", durationMin: 36, events: 39,
      outcome: "done", retryN: 0, lastEvent: "approved · feature-flagged behind sso_v2",
      day: "2days", cost: { tokens: 58800, dollars: 0.62 } }),

  s({ id: "s_1652", agent: "AR", ticket: 497, title: "remove deprecated /v1/users/me/teams endpoint",
      role: "spec", started: "2026-05-18 11:28", ended: "2026-05-18 11:44", durationMin: 16, events: 22,
      outcome: "done", retryN: 0, lastEvent: "spec done · deprecation period decided: 30d",
      day: "2days", cost: { tokens: 34900, dollars: 0.32 } }),
];

// Build ticket-grouped index
window.TICKETS_INDEX = (() => {
  const m = {};
  window.SESSIONS.forEach((s) => {
    if (!m[s.ticket]) m[s.ticket] = { ticket: s.ticket, title: s.title, sessions: [] };
    m[s.ticket].sessions.push(s);
  });
  Object.values(m).forEach((t) => t.sessions.sort((a, b) => (a.started > b.started ? 1 : -1)));
  return m;
})();
