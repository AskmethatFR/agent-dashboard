// Sessions Index — hi-fi React
const { useState, useMemo, useEffect, useRef, useCallback } = React;

// ---- helpers ----
function fmtTime(t) {
  if (!t) return "";
  if (t.includes(" ")) return t.split(" ")[1];
  return t;
}
function fmtDur(min) {
  if (!min) return "";
  if (min < 60) return min + "m";
  const h = Math.floor(min / 60);
  const m = min % 60;
  return h + "h" + (m < 10 ? "0" : "") + m + "m";
}

function OutcomeChip({ s }) {
  switch (s.outcome) {
    case "done":      return <span className="row-outcome outcome-ok">✓ done</span>;
    case "retry":     return <span className="row-outcome outcome-retry">↻ {s.retryN}/3</span>;
    case "warn":      return <span className="row-outcome outcome-warn">▲ {s.retryN}/3</span>;
    case "danger":    return <span className="row-outcome outcome-danger">▲▲ {s.retryN}/3</span>;
    case "escalated": return <span className="row-outcome outcome-esc">◆ esc.</span>;
    case "running":   return <span className="row-outcome outcome-running">◐ running</span>;
    case "failed":    return <span className="row-outcome outcome-failed">× failed</span>;
    default:          return <span className="row-outcome">{s.outcome}</span>;
  }
}

// ---- TopBar ----
function Clock() {
  const [now, setNow] = useState(() => new Date());
  useEffect(() => { const t = setInterval(() => setNow(new Date()), 1000); return () => clearInterval(t); }, []);
  const h = String(now.getUTCHours()).padStart(2, "0");
  const m = String(now.getUTCMinutes()).padStart(2, "0");
  const ss = String(now.getUTCSeconds()).padStart(2, "0");
  return <span>{h}:{m}:{ss} UTC</span>;
}

function TopBar({ techMode, onToggleTech, escCount }) {
  return (
    <header className="topbar">
      <div className="brand">
        <span className="brand-dot"></span>
        <span>team/</span>
      </div>
      <nav className="nav">
        <a className="nav-item" href="Home.html">Home</a>
        <button className="nav-item active">Sessions</button>
        <a className="nav-item" href="Replay.html">Replay</a>
        <a className="nav-item" href="Agent.html">Agents</a>
        <a className="nav-item" href="Flow.html">Flow</a>
        <a className="nav-item" href="Escalations.html">Escalations{escCount ? <span className="badge">{escCount}</span> : null}</a>
      </nav>
      <div className="topbar-right">
        <button
          className={"tech-toggle" + (techMode ? " on" : "")}
          onClick={onToggleTech}
          title="Show cost/tokens/model columns"
        >
          <span className="sw"></span>
          tech
        </button>
        <span className="live-dot"><span className="pulse"></span>live</span>
        <span><Clock /></span>
      </div>
    </header>
  );
}

// ---- Search ----
function SearchBar({ value, onChange, inputRef }) {
  return (
    <div className="search-row">
      <div className="search">
        <span className="glyph">⌕</span>
        <input
          ref={inputRef}
          value={value}
          onChange={(e) => onChange(e.target.value)}
          placeholder="search by ticket #, agent, title, content…"
          autoFocus
          spellCheck={false}
        />
        {value && <button className="clear" onClick={() => onChange("")}>⌫ clear</button>}
        <span className="hint">⌘K</span>
      </div>
      <div className="search-hint">
        try <code>#482</code> · <code>agent:DevA</code> · <code>outcome:escalated</code> · <code>retries:&gt;=2</code> · <code>since:yesterday</code>
      </div>
    </div>
  );
}

// ---- Anchors ----
function Anchors({ counts, runningCount }) {
  const today = "Wed, May 20, 2026";
  return (
    <div className="anchors">
      {runningCount > 0 && (
        <span className="anchor running">
          <span className="running-pulse"></span>
          <strong>{runningCount}</strong> running
        </span>
      )}
      <span className="anchor"><strong>{counts.today}</strong> today</span>
      <span className="anchor"><strong>{counts.yesterday}</strong> yesterday</span>
      <span className="anchor"><strong>{counts["2days"]}</strong> may 18</span>
      <span className="anchor"><strong>{counts.total}</strong> ·  total (7d · 184)</span>
      <div className="anchors-right">
        <button className="date-pill">{today} ▾</button>
      </div>
    </div>
  );
}

// ---- Filters ----
function FilterRow({ filters, onChange }) {
  const set = (k, v) => onChange({ ...filters, [k]: v });
  const isActive = (k, v) => filters[k] === v;
  return (
    <div className="filters">
      <span className="filters-label">filters</span>
      <button className={"filter-btn" + (filters.agent ? " active" : "")}>
        Agent: {filters.agent || "all"} <span className="caret">▾</span>
      </button>
      <button className={"filter-btn" + (filters.outcome ? " active" : "")}>
        Outcome: {filters.outcome || "all"} <span className="caret">▾</span>
      </button>
      <button className={"filter-btn" + (filters.retries ? " active" : "")}>
        Retries: {filters.retries || "≥ 0"} <span className="caret">▾</span>
      </button>
      <button className={"filter-btn" + (filters.duration ? " active" : "")}>
        Duration: {filters.duration || "any"} <span className="caret">▾</span>
      </button>
      <button className={"filter-btn" + (filters.findings ? " active" : "")} onClick={() => set("findings", !filters.findings)}>
        {filters.findings ? "✓ " : ""}Has findings
      </button>
      {(filters.agent || filters.outcome || filters.retries || filters.duration || filters.findings) && (
        <button className="filter-clear" onClick={() => onChange({})}>clear ×</button>
      )}
    </div>
  );
}

// ---- Session row ----
function SessionRow({ s, techMode, focused, onClick, onTicketClick }) {
  const cls = ["row", "standard", techMode && "tech", s.outcome, focused && "focused"]
    .filter(Boolean).join(" ");
  const isCrossDay = s.started && s.started.includes("2026-") && s.ended && s.ended.includes("2026-")
    && s.started.slice(0, 10) !== s.ended.slice(0, 10);
  return (
    <div className={cls} onClick={() => onClick(s)}>
      <span className={"row-time" + (isCrossDay ? " cross-day" : "")}>
        {fmtTime(s.started)}
        <span className="arrow">→</span>
        {fmtTime(s.ended)}
        {isCrossDay && <span className="cross"> +1d</span>}
      </span>
      <span className="row-agent">
        <span className="agent-glyph">{window.AGENTS[s.agent].glyph}</span>
      </span>
      <div className="row-body">
        <div className="row-title">
          <span className="row-ticket" onClick={(e) => { e.stopPropagation(); onTicketClick(s.ticket); }}>
            #{s.ticket}
          </span>
          <span className="row-role">{s.role}</span>
          <span className="row-name">{s.title}</span>
        </div>
        <div className="row-preview">
          <span className="indicator">└</span>
          {s.lastEvent}
          {s.findings > 0 && <span className="findings">· {s.findings} finding{s.findings > 1 ? "s" : ""}</span>}
        </div>
      </div>
      <OutcomeChip s={s} />
      <span className={"row-dur" + (s.durationMin >= 60 ? " long" : "")}>{fmtDur(s.durationMin)}</span>
      <span className="row-evt"><strong>{s.events}</strong>e</span>
      {techMode && (
        <>
          <span className="row-cost">${s.cost ? s.cost.dollars.toFixed(2) : "—"}</span>
          <span className="row-tokens">{s.cost ? (s.cost.tokens / 1000).toFixed(1) + "k" : "—"}</span>
        </>
      )}
      <div className="row-actions">
        <span className="key"><kbd>⏎</kbd> open</span>
        <span className="key"><kbd>⇧⏎</kbd> ticket</span>
        <span className="key"><kbd>y</kbd> copy</span>
      </div>
    </div>
  );
}

// ---- Running row ----
function RunningRow({ s, focused, onClick, onTicketClick }) {
  return (
    <div className={"row running" + (focused ? " focused" : "")} onClick={() => onClick(s)}>
      <span className="row-time">started {fmtTime(s.started)}</span>
      <span className="row-agent">
        <span className="agent-glyph thinking">{window.AGENTS[s.agent].glyph}</span>
      </span>
      <div className="row-body">
        <div className="row-title">
          <span className="row-ticket" onClick={(e) => { e.stopPropagation(); onTicketClick(s.ticket); }}>
            #{s.ticket}
          </span>
          <span className="row-role">{s.role}</span>
          <span className="row-name">{s.title}</span>
        </div>
        <div className="row-preview">
          <span className="activity-dot"></span>
          {s.activity}
        </div>
      </div>
      <span className="row-outcome outcome-running">◐ running</span>
      <RunningElapsed start={s.started} />
      <span className="row-evt"><strong>{s.events}</strong>e</span>
    </div>
  );
}

function RunningElapsed({ start }) {
  // Simulate ticking elapsed; baseline anchored at 17:42 to match story
  const [t, setT] = useState(0);
  useEffect(() => { const i = setInterval(() => setT((x) => x + 1), 1000); return () => clearInterval(i); }, []);
  // hardcoded offsets per started time relative to "now" 17:42
  const baseElapsed = { "17:28": 14, "17:36": 6 }[start] || 0;
  const totalSec = baseElapsed * 60 + t;
  const mm = Math.floor(totalSec / 60);
  const ss = String(totalSec % 60).padStart(2, "0");
  return <span className="row-dur">{mm}m {ss}s</span>;
}

// ---- Day Group ----
function DayGroup({ label, dateLabel, sessions, focusId, techMode, onClick, onTicketClick, collapsed, onToggle }) {
  const ok = sessions.filter((s) => s.outcome === "done").length;
  const warn = sessions.filter((s) => s.outcome === "warn" || s.outcome === "danger").length;
  const esc = sessions.filter((s) => s.outcome === "escalated").length;

  return (
    <>
      <div className="section-head">
        <span className="label">── {label} {dateLabel}</span>
        <span className="meta">
          <strong>{sessions.length}</strong> sessions
          <span> · <span className="ok">{ok} ok</span></span>
          {warn > 0 && <span> · <span className="warn">{warn} ▲</span></span>}
          {esc > 0 && <span> · <span className="esc">{esc} ◆</span></span>}
        </span>
        <button className="hide-btn" onClick={onToggle}>{collapsed ? "expand" : "collapse"}</button>
      </div>
      {!collapsed && (
        <>
          <div className={"col-head standard" + (techMode ? " tech" : "")}>
            <span>started → ended</span>
            <span></span>
            <span>ticket / activity</span>
            <span className="right">outcome</span>
            <span className="right">dur</span>
            <span className="right">evt</span>
            {techMode && <span className="right">cost</span>}
            {techMode && <span className="right">tokens</span>}
          </div>
          {sessions.length === 0 && <div className="empty-day">// no sessions in this window</div>}
          {sessions.map((s) => (
            <SessionRow
              key={s.id}
              s={s}
              techMode={techMode}
              focused={focusId === s.id}
              onClick={onClick}
              onTicketClick={onTicketClick}
            />
          ))}
        </>
      )}
    </>
  );
}

// ---- Running Section ----
function RunningSection({ sessions, focusId, onClick, onTicketClick, collapsed, onToggle }) {
  return (
    <>
      <div className="section-head running">
        <span className="label">◉ RUNNING</span>
        <span className="meta"><strong>{sessions.length}</strong> live</span>
        <button className="hide-btn" onClick={onToggle}>{collapsed ? "show" : "hide"}</button>
      </div>
      {!collapsed && (
        <>
          <div className="col-head running">
            <span>since</span>
            <span></span>
            <span>ticket / activity</span>
            <span className="right">state</span>
            <span className="right">elapsed</span>
            <span className="right">evt</span>
          </div>
          {sessions.map((s) => (
            <RunningRow
              key={s.id}
              s={s}
              focused={focusId === s.id}
              onClick={onClick}
              onTicketClick={onTicketClick}
            />
          ))}
        </>
      )}
    </>
  );
}

// ---- Side Panel — Ticket View ----
function TicketPanel({ ticketId, onClose, onOpenSession }) {
  const t = window.TICKETS_INDEX[ticketId];
  if (!t) return null;
  const totalMin = t.sessions.reduce((a, s) => a + (s.durationMin || 0), 0);
  const escCount = t.sessions.filter((s) => s.outcome === "escalated").length;

  return (
    <>
      <div className="scrim" onClick={onClose}></div>
      <aside className="side">
        <div className="side-head">
          <div className="side-kicker">
            <span>ticket</span>
            <span className="id">#{t.ticket}</span>
            <button className="side-close" onClick={onClose}>×</button>
          </div>
          <h2 className="side-title">{t.title}</h2>
          <div className="side-stats">
            <span><strong>{t.sessions.length}</strong> sessions</span>
            <span><strong>{fmtDur(totalMin)}</strong> total time</span>
            <span className={escCount ? "esc" : ""}>{escCount ? <strong>◆ {escCount}</strong> : <strong>0</strong>} escalations</span>
          </div>
        </div>
        <div className="side-body">
          {t.sessions.map((s, i) => (
            <div
              key={s.id}
              className={"side-step " + s.outcome}
              onClick={() => onOpenSession(s)}
            >
              <span className="stepper">
                <span className="stepper-dot"></span>
              </span>
              <span className="agent-glyph">{window.AGENTS[s.agent].glyph}</span>
              <div className="side-step-body">
                <div className="side-step-head">
                  <span className="role">{s.role}</span>
                  <span>·</span>
                  <span>{window.AGENTS[s.agent].name}</span>
                  <span className="ts">{fmtTime(s.started)} · {s.day === "today" ? "today" : s.day === "yesterday" ? "may 19" : "may 18"}</span>
                </div>
                <div className="side-step-preview">{s.lastEvent || s.activity}</div>
              </div>
              <span className="side-step-outcome">
                <OutcomeChip s={s} />
                <span style={{ color: "var(--tx-3)", marginLeft: 8 }}>{fmtDur(s.durationMin) || "—"}</span>
              </span>
            </div>
          ))}
        </div>
        <div className="side-footer">
          <button className="btn">open in github ↗</button>
          <button className="btn">copy permalink</button>
          <span className="total">{t.sessions.length} sessions · {fmtDur(totalMin)} · ${t.sessions.reduce((a, s) => a + (s.cost ? s.cost.dollars : 0), 0).toFixed(2)}</span>
        </div>
      </aside>
    </>
  );
}

// ---- Cmd+K stub ----
function CmdK({ onClose, onJumpTicket }) {
  const [q, setQ] = useState("");
  const ref = useRef();
  useEffect(() => { ref.current && ref.current.focus(); }, []);
  const items = [
    { kind: "Jump to ticket", label: "#482 add rate-limit headers", action: () => onJumpTicket(482) },
    { kind: "Jump to ticket", label: "#487 refactor auth middleware", action: () => onJumpTicket(487) },
    { kind: "Jump to ticket", label: "#455 CSRF on /api/admin", action: () => onJumpTicket(455) },
    { kind: "Filter", label: "Only running sessions", action: () => {} },
    { kind: "Filter", label: "Outcome: escalated", action: () => {} },
    { kind: "Go to", label: "Escalations Inbox", action: () => {} },
    { kind: "Go to", label: "Flow Analytics", action: () => {} },
  ];
  return (
    <div className="cmdk-stub" onClick={onClose}>
      <div className="cmdk-panel" onClick={(e) => e.stopPropagation()}>
        <input ref={ref} placeholder="jump to ticket, agent, view… (esc to close)" value={q} onChange={(e) => setQ(e.target.value)} />
        <div className="cmdk-list">
          <div className="cmdk-section">Jump to ticket</div>
          {items.filter((i) => i.kind === "Jump to ticket").map((i, k) => (
            <div key={k} className="cmdk-item" onClick={() => { i.action(); onClose(); }}>
              <span>#</span>{i.label}<span className="key">⏎</span>
            </div>
          ))}
          <div className="cmdk-section">Filter sessions</div>
          {items.filter((i) => i.kind === "Filter").map((i, k) => (
            <div key={k} className="cmdk-item">⊕ {i.label}</div>
          ))}
          <div className="cmdk-section">Navigate</div>
          {items.filter((i) => i.kind === "Go to").map((i, k) => (
            <div key={k} className="cmdk-item">→ {i.label}</div>
          ))}
        </div>
      </div>
    </div>
  );
}

// ---- Kbar (keyboard hint bar) ----
function Kbar({ techMode, query }) {
  return (
    <div className="kbar">
      <span className="hint"><kbd>/</kbd> search</span>
      <span className="hint"><kbd>⌘K</kbd> palette</span>
      <span className="hint"><kbd>j</kbd> <kbd>k</kbd> navigate</span>
      <span className="hint"><kbd>⏎</kbd> open replay</span>
      <span className="hint"><kbd>⇧⏎</kbd> open ticket view</span>
      <span className="hint"><kbd>e</kbd> escalations</span>
      <span className="hint right">
        {query ? <>searching <code style={{ color: "var(--tx-1)" }}>"{query}"</code></> : "184 sessions · 7d"}
      </span>
    </div>
  );
}

// ---- Search parser ----
function parseQuery(q) {
  const tokens = q.toLowerCase().split(/\s+/).filter(Boolean);
  const filters = {};
  const free = [];
  for (const t of tokens) {
    const m = t.match(/^(agent|outcome|retries|since|range):(.+)$/);
    if (m) filters[m[1]] = m[2];
    else if (t.startsWith("#")) filters.ticket = t.slice(1);
    else free.push(t);
  }
  filters._free = free.join(" ");
  return filters;
}

function matchesSession(s, parsed, panelFilters) {
  if (parsed.ticket && String(s.ticket) !== parsed.ticket) return false;
  if (parsed.agent && window.AGENTS[s.agent].name.toLowerCase() !== parsed.agent && s.agent.toLowerCase() !== parsed.agent) return false;
  if (parsed.outcome && s.outcome !== parsed.outcome) return false;
  if (parsed.retries) {
    const m = parsed.retries.match(/^(>=|>|<=|<|=)?(\d+)$/);
    if (m) {
      const op = m[1] || "=", n = parseInt(m[2]);
      const r = s.retryN || 0;
      const ok = op === "=" ? r === n : op === ">" ? r > n : op === ">=" ? r >= n : op === "<" ? r < n : r <= n;
      if (!ok) return false;
    }
  }
  if (parsed._free) {
    const hay = (s.title + " " + (s.lastEvent || "") + " " + (s.activity || "") + " #" + s.ticket).toLowerCase();
    for (const w of parsed._free.split(" ")) if (!hay.includes(w)) return false;
  }
  if (panelFilters.findings && (!s.findings || s.findings <= 0)) return false;
  if (panelFilters.agent && s.agent !== panelFilters.agent) return false;
  if (panelFilters.outcome && s.outcome !== panelFilters.outcome) return false;
  return true;
}

// ---- Root ----
function SessionsApp() {
  const [query, setQuery] = useState("");
  const [panelFilters, setPanelFilters] = useState({});
  const [techMode, setTechMode] = useState(false);
  const [runningCollapsed, setRunningCollapsed] = useState(false);
  const [collapsedDays, setCollapsedDays] = useState({});
  const [ticketPanelId, setTicketPanelId] = useState(null);
  const [cmdkOpen, setCmdkOpen] = useState(false);
  const [focusIdx, setFocusIdx] = useState(-1);
  const searchRef = useRef();

  const parsed = useMemo(() => parseQuery(query), [query]);

  const filtered = useMemo(() =>
    window.SESSIONS.filter((s) => matchesSession(s, parsed, panelFilters))
  , [parsed, panelFilters]);

  const running = filtered.filter((s) => s.outcome === "running");
  const finished = filtered.filter((s) => s.outcome !== "running");
  const today = finished.filter((s) => s.day === "today");
  const yesterday = finished.filter((s) => s.day === "yesterday");
  const twoDays = finished.filter((s) => s.day === "2days");

  const counts = {
    today: window.SESSIONS.filter((s) => s.day === "today" && s.outcome !== "running").length,
    yesterday: window.SESSIONS.filter((s) => s.day === "yesterday").length,
    "2days": window.SESSIONS.filter((s) => s.day === "2days").length,
    total: window.SESSIONS.length,
  };

  const escCount = window.SESSIONS.filter((s) => s.outcome === "escalated").length;

  // visible rows in order — for j/k nav
  const visibleRows = useMemo(() => {
    const rows = [];
    if (!runningCollapsed) running.forEach((s) => rows.push(s));
    if (!collapsedDays.today) today.forEach((s) => rows.push(s));
    if (!collapsedDays.yesterday) yesterday.forEach((s) => rows.push(s));
    if (!collapsedDays["2days"]) twoDays.forEach((s) => rows.push(s));
    return rows;
  }, [running, today, yesterday, twoDays, runningCollapsed, collapsedDays]);

  const onOpenSession = useCallback((s) => {
    window.location.href = "Replay.html";
  }, []);

  const onTicketClick = useCallback((t) => setTicketPanelId(t), []);

  // global keys
  useEffect(() => {
    const fn = (e) => {
      if (cmdkOpen && e.key === "Escape") { setCmdkOpen(false); return; }
      if ((e.metaKey || e.ctrlKey) && e.key === "k") { e.preventDefault(); setCmdkOpen(true); return; }
      if (e.target.tagName === "INPUT") return;
      if (e.key === "/") { e.preventDefault(); searchRef.current && searchRef.current.focus(); return; }
      if (e.key === "j") { setFocusIdx((i) => Math.min(visibleRows.length - 1, i + 1)); return; }
      if (e.key === "k") { setFocusIdx((i) => Math.max(0, i - 1)); return; }
      if (e.key === "Enter") {
        const s = visibleRows[Math.max(0, focusIdx)];
        if (s) {
          if (e.shiftKey) setTicketPanelId(s.ticket);
          else onOpenSession(s);
        }
        return;
      }
      if (e.key === "Escape") { setTicketPanelId(null); return; }
    };
    window.addEventListener("keydown", fn);
    return () => window.removeEventListener("keydown", fn);
  }, [cmdkOpen, visibleRows, focusIdx, onOpenSession]);

  const hasResults = visibleRows.length > 0;

  return (
    <div className="app">
      <TopBar techMode={techMode} onToggleTech={() => setTechMode((v) => !v)} escCount={escCount} />
      {escCount > 0 && <div className="alarm-strip"></div>}

      <div className="main">
        <SearchBar value={query} onChange={setQuery} inputRef={searchRef} />
        <Anchors counts={counts} runningCount={running.length} />
        <FilterRow filters={panelFilters} onChange={setPanelFilters} />

        <div className="sessions">
          {running.length > 0 && (
            <RunningSection
              sessions={running}
              focusId={visibleRows[focusIdx]?.id}
              onClick={onOpenSession}
              onTicketClick={onTicketClick}
              collapsed={runningCollapsed}
              onToggle={() => setRunningCollapsed((v) => !v)}
            />
          )}

          {hasResults || running.length > 0 ? (
            <>
              {today.length > 0 && (
                <DayGroup
                  label="TODAY"
                  dateLabel="· WED MAY 20"
                  sessions={today}
                  focusId={visibleRows[focusIdx]?.id}
                  techMode={techMode}
                  onClick={onOpenSession}
                  onTicketClick={onTicketClick}
                  collapsed={collapsedDays.today}
                  onToggle={() => setCollapsedDays((d) => ({ ...d, today: !d.today }))}
                />
              )}
              {yesterday.length > 0 && (
                <DayGroup
                  label="YESTERDAY"
                  dateLabel="· TUE MAY 19"
                  sessions={yesterday}
                  focusId={visibleRows[focusIdx]?.id}
                  techMode={techMode}
                  onClick={onOpenSession}
                  onTicketClick={onTicketClick}
                  collapsed={collapsedDays.yesterday}
                  onToggle={() => setCollapsedDays((d) => ({ ...d, yesterday: !d.yesterday }))}
                />
              )}
              {twoDays.length > 0 && (
                <DayGroup
                  label="MON MAY 18"
                  dateLabel=""
                  sessions={twoDays}
                  focusId={visibleRows[focusIdx]?.id}
                  techMode={techMode}
                  onClick={onOpenSession}
                  onTicketClick={onTicketClick}
                  collapsed={collapsedDays["2days"]}
                  onToggle={() => setCollapsedDays((d) => ({ ...d, "2days": !d["2days"] }))}
                />
              )}
            </>
          ) : (
            <div className="no-results">
              <div className="big">// no match for "{query}"</div>
              <div className="small">try broader terms, or check syntax: <code style={{ color: "var(--tx-1)" }}>agent:DevA</code></div>
              <button onClick={() => { setQuery(""); setPanelFilters({}); }}>reset query</button>
            </div>
          )}
        </div>
      </div>

      {ticketPanelId && (
        <TicketPanel
          ticketId={ticketPanelId}
          onClose={() => setTicketPanelId(null)}
          onOpenSession={onOpenSession}
        />
      )}
      {cmdkOpen && (
        <CmdK
          onClose={() => setCmdkOpen(false)}
          onJumpTicket={(t) => { setTicketPanelId(t); setCmdkOpen(false); }}
        />
      )}

      <Kbar techMode={techMode} query={query} />
    </div>
  );
}

ReactDOM.createRoot(document.getElementById("root")).render(<SessionsApp />);
