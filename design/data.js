// Mock data for Team Board

window.AGENTS = {
  PM: { id: "PM", name: "PM",       glyph: "PM", role: "Project Manager" },
  AR: { id: "AR", name: "Architect", glyph: "Ar", role: "Project Architect" },
  DA: { id: "DA", name: "DevA",     glyph: "Da", role: "Developer A" },
  DB: { id: "DB", name: "DevB",     glyph: "Db", role: "Developer B" },
  QA: { id: "QA", name: "QA",       glyph: "Qa", role: "QA" },
  SE: { id: "SE", name: "Security", glyph: "Se", role: "Security" },
};

window.COLUMNS = [
  { id: "CREATED",       label: "Created" },
  { id: "SPECIFIED",     label: "Specified" },
  { id: "IN_DEVELOPMENT",label: "In development" },
  { id: "IN_REVIEW",     label: "In review" },
  { id: "IN_QA",         label: "In QA" },
  { id: "AWAITING_VAL",  label: "Awaiting validation" },
  { id: "DONE",          label: "Done · today" },
];

window.TICKETS = [
  // CREATED
  { id: 502, col: "CREATED", title: "[bug] settings page 404 after sso login", agent: "PM",  retry: 0, age: "3m",   thinking: true,  fresh: true },
  { id: 501, col: "CREATED", title: "[feat] export tasks as CSV from /workspaces", agent: "PM", retry: 0, age: "11m" },
  { id: 499, col: "CREATED", title: "rename `org.admin` to `org.owner` across api", agent: "PM", retry: 0, age: "26m" },
  { id: 497, col: "CREATED", title: "remove deprecated `/v1/users/me/teams` endpoint", agent: "PM", retry: 0, age: "1h 02m" },

  // SPECIFIED
  { id: 495, col: "SPECIFIED", title: "add per-org rate limit with redis token bucket", agent: "AR", retry: 0, age: "8m", thinking: true },
  { id: 491, col: "SPECIFIED", title: "OAuth state parameter must be HMAC-signed", agent: "AR", retry: 0, age: "34m" },
  { id: 489, col: "SPECIFIED", title: "migrate logger to pino, structured JSON output", agent: "AR", retry: 0, age: "1h 47m" },

  // IN_DEVELOPMENT
  { id: 488, col: "IN_DEVELOPMENT", title: "parse OAuth state and validate signature", agent: "DA", coAgent: "DB", crossReview: true, retry: 0, age: "22m", thinking: true },
  { id: 487, col: "IN_DEVELOPMENT", title: "refactor auth middleware: drop legacy cookie path", agent: "DA", retry: 1, age: "1h 14m" },
  { id: 485, col: "IN_DEVELOPMENT", title: "feat(billing): proration on plan downgrade", agent: "DB", retry: 0, age: "2h 03m" },
  { id: 482, col: "IN_DEVELOPMENT", title: "add rate-limit headers to all /api responses", agent: "DB", retry: 0, age: "44m", fresh: true },
  { id: 478, col: "IN_DEVELOPMENT", title: "cache org membership lookup (LRU, 5 min)", agent: "DA", retry: 1, age: "2h 38m" },
  { id: 471, col: "IN_DEVELOPMENT", title: "fix race condition in invite acceptance flow", agent: "DB", retry: 2, age: "3h 09m" },

  // IN_REVIEW
  { id: 477, col: "IN_REVIEW", title: "fix flaky test on signup with social provider", agent: "DB", coAgent: "DA", crossReview: true, retry: 2, age: "1h 02m" },
  { id: 476, col: "IN_REVIEW", title: "extract `useDebouncedValue` from search page", agent: "DA", coAgent: "DB", crossReview: true, retry: 0, age: "31m", thinking: true },
  { id: 473, col: "IN_REVIEW", title: "harden CSP: drop `unsafe-inline` from styles", agent: "DA", coAgent: "DB", crossReview: true, retry: 1, age: "1h 51m" },
  { id: 468, col: "IN_REVIEW", title: "normalize 4xx error envelope (RFC 7807)", agent: "DB", coAgent: "DA", crossReview: true, retry: 0, age: "2h 22m" },
  { id: 463, col: "IN_REVIEW", title: "fix refresh-token replay vulnerability", agent: "DB", coAgent: "DA", crossReview: true, retry: 3, age: "47m" },

  // IN_QA
  { id: 466, col: "IN_QA", title: "passkey enrollment: device-bound credential id", agent: "QA", retry: 0, age: "18m", thinking: true },
  { id: 461, col: "IN_QA", title: "billing portal: cancel-then-restore flow", agent: "QA", retry: 1, age: "1h 27m" },
  { id: 441, col: "IN_QA", title: "migrate logger to pino, structured JSON output", agent: "QA", retry: 1, age: "6h 12m", stale: true },

  // AWAITING_VAL
  { id: 455, col: "AWAITING_VAL", title: "CSRF on /api/admin actions — proposed fix conflicts QA scenarios", agent: "AR", retry: 0, age: "3h 22m", escalated: true, escTo: "PM" },
  { id: 452, col: "AWAITING_VAL", title: "settings: org-level SSO enforcement toggle", agent: "PM", retry: 0, age: "1h 41m" },

  // DONE
  { id: 460, col: "DONE", title: "search: support `is:open` filter on /tasks", agent: "PM", retry: 0, age: "12m" },
  { id: 459, col: "DONE", title: "fix N+1 in /v1/projects?expand=members", agent: "PM", retry: 0, age: "47m" },
  { id: 456, col: "DONE", title: "audit log: redact bearer tokens in dump", agent: "PM", retry: 0, age: "1h 14m" },
  { id: 451, col: "DONE", title: "feat: invite via email link, 7d ttl", agent: "PM", retry: 0, age: "2h 06m" },
  { id: 448, col: "DONE", title: "remove unused `legacyAuth` feature flag", agent: "PM", retry: 0, age: "3h 18m" },
  { id: 445, col: "DONE", title: "docs: agent escalation protocol v2", agent: "PM", retry: 0, age: "4h 02m" },
  { id: 442, col: "DONE", title: "fix(ui): dark-mode contrast on disabled buttons", agent: "PM", retry: 0, age: "5h 21m" },
];

window.TICKER_EVENTS = [
  { ts: "17:42:08", kind: "ok",   text: "DevA → DevB · cross-review requested on #482" },
  { ts: "17:42:01", kind: "info", text: "Architect · spec finalized on #495, moved to IN_DEVELOPMENT" },
  { ts: "17:41:48", kind: "warn", text: "DevB · retry 2/3 on #471 — invite race still failing under load" },
  { ts: "17:41:33", kind: "ok",   text: "QA · #466 passkey enrollment — 14 scenarios passing, 2 pending" },
  { ts: "17:41:09", kind: "info", text: "Security · #473 CSP audit started" },
  { ts: "17:40:55", kind: "ok",   text: "PM · #460 closed, deployed to prod (commit a91f4c)" },
  { ts: "17:40:30", kind: "warn", text: "Architect · #455 conflict QA↔Security — escalation prepared for PM" },
];
