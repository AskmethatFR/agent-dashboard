// Team Board — hi-fi React component
const { useState, useMemo, useEffect, useRef } = React;

// ---- AgentChip ----
function AgentChip({ id, thinking, dense }) {
  const a = window.AGENTS[id];
  if (!a) return null;
  return (
    <span className={"agent" + (thinking ? " thinking" : "")} title={a.role + " — " + a.name}>
      <span className="agent-glyph">{a.glyph}</span>
      {!dense && <span className="agent-name">{a.name}</span>}
    </span>
  );
}

// ---- RetryCounter ----
function RetryCounter({ n }) {
  const cls = n >= 3 ? "danger" : n === 2 ? "warn" : "ok";
  const tri = n === 0 ? "" : n === 1 ? "△" : n === 2 ? "▲" : "▲▲";
  return (
    <span className={"retry " + cls}>
      <span className="glyph">⟳</span>
      <span>{n}/3</span>
      {tri && <span className="glyph">{tri}</span>}
    </span>
  );
}

// ---- Ticket Card ----
function TicketCard({ t, onHover, onLeave, onClick }) {
  const cls = [
    "card",
    t.escalated && "escalated",
    !t.escalated && t.retry >= 3 && "danger",
    !t.escalated && t.retry === 2 && "warn",
    !t.escalated && t.fresh && "fresh",
    !t.escalated && t.stale && "stale",
  ].filter(Boolean).join(" ");

  const ageWarn = t.stale || /^[3-9]h/.test(t.age) || /^\d{2,}h/.test(t.age);

  return (
    <div
      className={cls}
      onMouseEnter={(e) => onHover(t, e)}
      onMouseMove={(e) => onHover(t, e)}
      onMouseLeave={onLeave}
      onClick={() => onClick && onClick(t)}
    >
      {t.escalated && (
        <span className="esc-badge">
          <span className="siren">◆</span>
          esc → {t.escTo}
        </span>
      )}
      <div className="card-head">
        <span className="card-id mono">#{t.id}</span>
        <span className="card-title">{t.title}</span>
      </div>
      <div className="card-meta">
        {t.crossReview ? (
          <span className="cross">
            <AgentChip id={t.agent} thinking={t.thinking} dense />
            <span className="x">⇄</span>
            <AgentChip id={t.coAgent} dense />
          </span>
        ) : (
          <AgentChip id={t.agent} thinking={t.thinking} />
        )}
        <RetryCounter n={t.retry} />
        <span className={"age" + (ageWarn ? " warn" : "")}>
          {t.fresh ? <span className="sparkle">✦ </span> : null}
          ⏱ {t.age}
          {t.stale && <span className="stale-marker"> · zZz</span>}
        </span>
      </div>
      {t.crossReview && !t.escalated && (
        <div className="card-extra">
          <span style={{ color: "var(--tx-3)" }}>cross-review</span>
          <span style={{ color: "var(--tx-2)" }}>
            <AgentChip id={t.agent} dense /> implementing · <AgentChip id={t.coAgent} dense /> reviewing
          </span>
        </div>
      )}
    </div>
  );
}

// ---- Column ----
function Column({ col, tickets, onHover, onLeave, onClick }) {
  const liveCount = tickets.filter((t) => t.thinking).length;
  const attentionCount = tickets.filter((t) => t.retry >= 2 || t.stale || t.escalated).length;
  const needsHuman = col.id === "AWAITING_VAL" && tickets.length > 0;

  let signal = null;
  if (needsHuman) signal = "attention";
  else if (liveCount > 0) signal = "live";
  else if (attentionCount > 0 && col.id === "IN_REVIEW") signal = "info";

  return (
    <div className="column">
      <div className="column-header">
        <span className="column-title">{col.label}</span>
        <span className="column-meta">
          <span>{tickets.length}</span>
          {signal && <span className={"signal " + signal}></span>}
        </span>
      </div>
      <div className="cards">
        {tickets.map((t) => (
          <TicketCard key={t.id} t={t} onHover={onHover} onLeave={onLeave} onClick={onClick} />
        ))}
      </div>
    </div>
  );
}

// ---- Live Ticker ----
function LiveTicker({ events, collapsed, onToggle }) {
  const [idx, setIdx] = useState(0);
  useEffect(() => {
    const t = setInterval(() => setIdx((i) => (i + 1) % events.length), 3200);
    return () => clearInterval(t);
  }, [events.length]);
  const e = events[idx];
  return (
    <div className={"ticker" + (collapsed ? " collapsed" : "")}>
      <span className="ticker-label">
        <span className="dot"></span>
        Stream
      </span>
      <div className="ticker-feed">
        <span key={idx} className="ticker-feed-row" style={{ display: "inline-flex", gap: 8, alignItems: "center" }}>
          <span className="ts">{e.ts}</span>
          <span className="arrow">▸</span>
          <span className={e.kind}>{e.text}</span>
        </span>
      </div>
      <button className="ticker-toggle" onClick={onToggle}>
        {collapsed ? "expand" : "collapse"} <span className="ticker-kbd">t</span>
      </button>
    </div>
  );
}

// ---- Tooltip ----
function Tooltip({ t, x, y }) {
  if (!t) return null;
  const offset = 14;
  const a = window.AGENTS[t.agent];
  return (
    <div className="tooltip" style={{ left: x + offset, top: y + offset }}>
      <div className="t-row"><span className="t-k">id</span><span className="t-v">#{t.id}</span></div>
      <div className="t-row"><span className="t-k">state</span><span className="t-v">{t.col}</span></div>
      <div className="t-row"><span className="t-k">agent</span><span className="t-v">{a.name} · {a.role}</span></div>
      {t.coAgent && <div className="t-row"><span className="t-k">co</span><span className="t-v">{window.AGENTS[t.coAgent].name} (review)</span></div>}
      <div className="t-row"><span className="t-k">retry</span><span className="t-v">{t.retry}/3</span></div>
      <div className="t-row"><span className="t-k">in column</span><span className="t-v">{t.age}</span></div>
      <div className="t-row"><span className="t-k">last action</span><span className="t-v">prompt #14 · skill <span style={{color:"var(--st-fresh)"}}>edit_file</span></span></div>
      {t.escalated && <div className="t-row"><span className="t-k">escalated</span><span className="t-v" style={{color:"var(--st-esc)"}}>→ {t.escTo}</span></div>}
      <div style={{marginTop: 6, paddingTop: 6, borderTop: "1px dashed var(--bd-2)", color: "var(--tx-3)", fontSize: 10}}>
        click to open timeline · right-click for actions
      </div>
    </div>
  );
}

// ---- Legend (collapsible) ----
function Legend({ onClose }) {
  return (
    <div className="legend">
      <button className="legend-close" onClick={onClose}>×</button>
      <h4>Card statuses</h4>
      <ul>
        <li><span className="swatch"></span> normal</li>
        <li><span className="swatch fresh"></span> fresh / updated</li>
        <li><span className="swatch warn"></span> retry 2/3</li>
        <li><span className="swatch danger"></span> retry 3/3</li>
        <li><span className="swatch esc"></span> escalated → human</li>
        <li><span className="swatch stale"></span> stale &gt; 6h</li>
      </ul>
      <h4 style={{ marginTop: 12 }}>Glyphs</h4>
      <ul>
        <li><span style={{color: "var(--tx-1)", width: 14, textAlign: "center"}}>⟳</span> retry counter</li>
        <li><span style={{color: "var(--tx-1)", width: 14, textAlign: "center"}}>⇄</span> cross-review</li>
        <li><span style={{color: "var(--st-fresh)", width: 14, textAlign: "center"}}>✦</span> just updated</li>
        <li><span style={{color: "var(--st-warn)", width: 14, textAlign: "center"}}>zZz</span> stale</li>
        <li><span style={{color: "var(--st-esc)", width: 14, textAlign: "center"}}>◆</span> escalation</li>
      </ul>
    </div>
  );
}

// ---- Clock ----
function Clock() {
  const [now, setNow] = useState(() => new Date());
  useEffect(() => {
    const t = setInterval(() => setNow(new Date()), 1000);
    return () => clearInterval(t);
  }, []);
  const hh = String(now.getUTCHours()).padStart(2, "0");
  const mm = String(now.getUTCMinutes()).padStart(2, "0");
  const ss = String(now.getUTCSeconds()).padStart(2, "0");
  return <span>{hh}:{mm}:{ss} UTC</span>;
}

// ---- Team Board ----
function TeamBoard() {
  const [tickerCollapsed, setTickerCollapsed] = useState(false);
  const [legendOpen, setLegendOpen] = useState(true);
  const [hover, setHover] = useState({ t: null, x: 0, y: 0 });

  const byCol = useMemo(() => {
    const m = {};
    window.COLUMNS.forEach((c) => (m[c.id] = []));
    window.TICKETS.forEach((t) => m[t.col].push(t));
    return m;
  }, []);

  const total = window.TICKETS.length;
  const open = window.TICKETS.filter((t) => t.col !== "DONE").length;
  const escCount = window.TICKETS.filter((t) => t.escalated).length;
  const staleCount = window.TICKETS.filter((t) => t.stale).length;

  const handleHover = (t, e) => setHover({ t, x: e.clientX, y: e.clientY });
  const handleLeave = () => setHover({ t: null, x: 0, y: 0 });

  useEffect(() => {
    const fn = (e) => {
      if (e.key === "t" && !e.metaKey && !e.ctrlKey && document.activeElement.tagName !== "INPUT") {
        setTickerCollapsed((c) => !c);
      }
    };
    window.addEventListener("keydown", fn);
    return () => window.removeEventListener("keydown", fn);
  }, []);

  return (
    <div className="app">
      {/* topbar */}
      <header className="topbar">
        <div className="brand">
          <span className="brand-dot"></span>
          <span>team/</span>
        </div>
        <nav className="nav">
          <button className="nav-item active">Board</button>
          <button className="nav-item">Timeline</button>
          <button className="nav-item">Agents</button>
          <button className="nav-item">Flow</button>
          <button className="nav-item">
            Escalations
            <span className="badge">{escCount}</span>
          </button>
        </nav>
        <div className="topbar-right">
          <span className="cmdk">⌘K <kbd>jump</kbd></span>
          <span className="live-dot"><span className="pulse"></span>live</span>
          <span><Clock /></span>
        </div>
      </header>

      {escCount > 0 && <div className="alarm-strip"></div>}

      {/* board */}
      <div className="board-wrap">
        <div className="board-header">
          <div className="counts">
            <span><strong>{open}</strong> active</span>
            <span className="esc"><strong>{escCount}</strong> escalated</span>
            <span className="stale"><strong>{staleCount}</strong> stale</span>
            <span style={{color:"var(--tx-3)"}}>· {total} total today</span>
          </div>
          <div className="filters">
            <button className="filter-btn">Filter <span className="caret">▾</span></button>
            <button className="filter-btn">Agent: all <span className="caret">▾</span></button>
            <button className="filter-btn">Sort: updated <span className="caret">▾</span></button>
            <button className="filter-btn" onClick={() => setLegendOpen((v) => !v)}>
              {legendOpen ? "hide" : "show"} legend
            </button>
          </div>
        </div>

        <div className="columns">
          {window.COLUMNS.map((c) => (
            <Column
              key={c.id}
              col={c}
              tickets={byCol[c.id] || []}
              onHover={handleHover}
              onLeave={handleLeave}
            />
          ))}
        </div>
      </div>

      <LiveTicker
        events={window.TICKER_EVENTS}
        collapsed={tickerCollapsed}
        onToggle={() => setTickerCollapsed((c) => !c)}
      />

      <Tooltip t={hover.t} x={hover.x} y={hover.y} />
      {legendOpen && <Legend onClose={() => setLegendOpen(false)} />}
    </div>
  );
}

ReactDOM.createRoot(document.getElementById("root")).render(<TeamBoard />);
