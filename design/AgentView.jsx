// Agent View
const { useState, useMemo, useEffect } = React;

function AgentClock() {
  const [now, setNow] = useState(() => new Date());
  useEffect(() => { const t = setInterval(() => setNow(new Date()), 1000); return () => clearInterval(t); }, []);
  return <span>{String(now.getUTCHours()).padStart(2,"0")}:{String(now.getUTCMinutes()).padStart(2,"0")}:{String(now.getUTCSeconds()).padStart(2,"0")} UTC</span>;
}

function AgentTopBar({ escCount }) {
  return (
    <header className="topbar">
      <div className="brand"><span className="brand-dot"></span><span>team/</span></div>
      <nav className="nav">
        <a className="nav-item" href="Home.html">Home</a>
        <a className="nav-item" href="Sessions.html">Sessions</a>
        <a className="nav-item" href="Replay.html">Replay</a>
        <button className="nav-item active">Agents</button>
        <a className="nav-item" href="Flow.html">Flow</a>
        <a className="nav-item" href="Escalations.html">Escalations{escCount ? <span className="badge">{escCount}</span> : null}</a>
      </nav>
      <div className="topbar-right">
        <span className="live-dot"><span className="pulse"></span>live</span>
        <span><AgentClock /></span>
      </div>
    </header>
  );
}

// per-agent computed metrics from SESSIONS
function computeMetrics(agentId) {
  const sessions = window.SESSIONS.filter((s) => s.agent === agentId);
  const done = sessions.filter((s) => s.outcome === "done").length;
  const retry = sessions.filter((s) => s.outcome === "retry" || s.outcome === "warn" || s.outcome === "danger").length;
  const escalated = sessions.filter((s) => s.outcome === "escalated").length;
  const running = sessions.filter((s) => s.outcome === "running").length;
  const total = sessions.length;
  const firstPassYield = total ? Math.round((done / (total - running)) * 100) : 0;
  const totalMin = sessions.reduce((a, s) => a + (s.durationMin || 0), 0);
  const totalTokens = sessions.reduce((a, s) => a + (s.cost ? s.cost.tokens : 0), 0);
  const totalCost = sessions.reduce((a, s) => a + (s.cost ? s.cost.dollars : 0), 0);
  const avgMin = total ? Math.round(totalMin / total) : 0;
  return { total, done, retry, escalated, running, firstPassYield, totalMin, totalTokens, totalCost, avgMin };
}

// per-agent skills (manually curated by role)
const AGENT_SKILLS = {
  DA: [
    { name: "edit_file", count: 142, pct: 1.0 },
    { name: "read_file", count: 118, pct: 0.83 },
    { name: "run_tests", count: 88, pct: 0.62 },
    { name: "grep", count: 67, pct: 0.47 },
    { name: "git", count: 54, pct: 0.38 },
    { name: "tdd_loop", count: 41, pct: 0.29 },
  ],
  DB: [
    { name: "edit_file", count: 128, pct: 1.0 },
    { name: "read_file", count: 109, pct: 0.85 },
    { name: "run_tests", count: 76, pct: 0.59 },
    { name: "grep", count: 58, pct: 0.45 },
    { name: "git", count: 49, pct: 0.38 },
    { name: "cross_review", count: 34, pct: 0.27 },
  ],
  PM: [
    { name: "github_issues", count: 92, pct: 1.0 },
    { name: "read_file", count: 41, pct: 0.45 },
    { name: "validate_acceptance", count: 38, pct: 0.41 },
  ],
  AR: [
    { name: "tech_spec", count: 73, pct: 1.0 },
    { name: "read_file", count: 87, pct: 1.19 },
    { name: "grep", count: 51, pct: 0.7 },
    { name: "arbitrate", count: 18, pct: 0.25 },
  ],
  QA: [
    { name: "run_e2e", count: 64, pct: 1.0 },
    { name: "read_file", count: 38, pct: 0.59 },
    { name: "scenario_design", count: 41, pct: 0.64 },
  ],
  SE: [
    { name: "security_audit", count: 48, pct: 1.0 },
    { name: "grep", count: 39, pct: 0.81 },
    { name: "read_file", count: 32, pct: 0.67 },
    { name: "threat_model", count: 21, pct: 0.44 },
  ],
};

const AGENT_FEEDBACK = {
  DA: [
    { from: "DB", ticket: 487, severity: "warn", at: "today 17:03", quote: "missing test for csrf header propagation when cookie path is empty — please add coverage." },
    { from: "DB", ticket: 478, severity: "warn", at: "today 11:18", quote: "cache invalidation on role-change is racy under multi-region. consider a versioned key strategy." },
    { from: "DB", ticket: 476, severity: "ok", at: "today 13:33", quote: "lgtm · clean extraction, types inferred from caller — nice." },
    { from: "AR", ticket: 463, severity: "danger", at: "may 19 22:48", quote: "3rd attempt still doesn't address the lifecycle question — escalating arbitration." },
  ],
  DB: [
    { from: "DA", ticket: 482, severity: "ok", at: "today 17:34", quote: "approved on first pass, headers consistently attached across all 4 routes." },
    { from: "QA", ticket: 471, severity: "warn", at: "today 16:51", quote: "still reproducible under concurrent load (3 of 50 runs) — consider a mutex." },
  ],
  PM: [
    { from: "AR", ticket: 502, severity: "ok", at: "today 11:25", quote: "repro is unambiguous, acceptance criteria are explicit. easy to spec." },
    { from: "QA", ticket: 451, severity: "ok", at: "may 19 16:31", quote: "scenarios were comprehensive — caught the 410 edge case before me." },
  ],
  AR: [
    { from: "DA", ticket: 495, severity: "ok", at: "today 12:08", quote: "tech spec resolved all 4 ambiguities. ready to implement." },
    { from: "QA", ticket: 455, severity: "warn", at: "today 16:11", quote: "arbitration didn't converge — but the framing of options was clear." },
  ],
  QA: [
    { from: "PM", ticket: 466, severity: "ok", at: "today 17:36", quote: "scenarios cover both happy path and 2 device-edge cases. ship-ready." },
    { from: "DA", ticket: 461, severity: "warn", at: "today 14:17", quote: "edge case yearly-plan-restore wasn't in spec — added retroactively. could be flagged earlier." },
  ],
  SE: [
    { from: "AR", ticket: 473, severity: "ok", at: "today 15:09", quote: "audit was tight: 1 low finding, well-justified. spec compliant." },
    { from: "DA", ticket: 455, severity: "warn", at: "today 16:04", quote: "objection on token rotation cadence triggered escalation — surfaced correctly." },
  ],
};

function Metric({ label, value, unit, tone, trend, sparkline }) {
  return (
    <div className="metric-card">
      <div className="label">{label}</div>
      <div className={"value" + (tone ? " " + tone : "")}>{value}{unit && <span className="unit">{unit}</span>}</div>
      {trend && (
        <div className="trend">
          {trend.dir === "up" && <span className="up">↑</span>}
          {trend.dir === "down" && <span className="down">↓</span>}
          {trend.dir === "flat" && <span className="flat">—</span>}
          <span>{trend.text}</span>
        </div>
      )}
      {sparkline && (
        <svg className="sparkline" viewBox="0 0 120 28" preserveAspectRatio="none">
          <polyline
            fill="none"
            stroke="var(--st-fresh)"
            strokeWidth="1.5"
            points={sparkline.map((v, i) => `${(i / (sparkline.length - 1)) * 120},${28 - (v * 24) - 2}`).join(" ")}
          />
        </svg>
      )}
    </div>
  );
}

function BarRow({ label, n, max, tone }) {
  return (
    <div className="bar-row">
      <div className="lab">
        <span style={{minWidth: 110, display: "inline-block"}}>{label}</span>
        <div className="bar"><div className={"fill " + (tone || "")} style={{width: (n / max * 100) + "%"}}></div></div>
      </div>
      <span className="n">{n}</span>
    </div>
  );
}

function FeedbackList({ items }) {
  return (
    <div className="feedback">
      {items.map((f, i) => (
        <div key={i} className={"feedback-item " + (f.severity || "")}>
          <div className="top">
            <span className="from">{window.AGENTS[f.from] ? window.AGENTS[f.from].name : f.from}</span>
            <span>· on</span>
            <span className="ticket">#{f.ticket}</span>
            <span style={{marginLeft: "auto"}}>{f.at}</span>
          </div>
          <div className="quote">"{f.quote}"</div>
        </div>
      ))}
    </div>
  );
}

function MiniSessions({ agentId }) {
  const sessions = window.SESSIONS
    .filter((s) => s.agent === agentId)
    .slice(0, 8);
  return (
    <div>
      {sessions.map((s) => (
        <div key={s.id} className="mini-sessions-row" onClick={() => window.location.href = "Replay.html"}>
          <span className="at">{s.started.includes(" ") ? s.started.split(" ")[1] : s.started}</span>
          <span className="title-c"><span className="ticket">#{s.ticket}</span>{s.title}</span>
          <span className="outc">
            {s.outcome === "done" && <span style={{color: "var(--st-ok)"}}>✓ done</span>}
            {s.outcome === "retry" && <span>↻ {s.retryN}/3</span>}
            {s.outcome === "warn" && <span style={{color: "var(--st-warn)"}}>▲ {s.retryN}/3</span>}
            {s.outcome === "danger" && <span style={{color: "var(--st-danger)"}}>▲▲ {s.retryN}/3</span>}
            {s.outcome === "escalated" && <span style={{color: "var(--st-esc)"}}>◆ esc.</span>}
            {s.outcome === "running" && <span style={{color: "var(--st-fresh)"}}>◐ live</span>}
          </span>
          <span className="dur">{s.durationMin ? s.durationMin + "m" : "—"}</span>
        </div>
      ))}
    </div>
  );
}

function AgentProfile({ agentId }) {
  const a = window.AGENTS[agentId];
  const m = computeMetrics(agentId);
  const skills = AGENT_SKILLS[agentId] || [];
  const feedback = AGENT_FEEDBACK[agentId] || [];
  const maxSkill = Math.max(1, ...skills.map((s) => s.count));

  // synthesized sparklines (sessions/day for last 7d)
  const spark = {
    DA: [0.4, 0.55, 0.5, 0.7, 0.6, 0.8, 0.75],
    DB: [0.3, 0.45, 0.6, 0.5, 0.65, 0.7, 0.8],
    PM: [0.3, 0.3, 0.4, 0.55, 0.6, 0.45, 0.5],
    AR: [0.2, 0.3, 0.4, 0.3, 0.5, 0.55, 0.45],
    QA: [0.4, 0.35, 0.5, 0.5, 0.55, 0.6, 0.55],
    SE: [0.2, 0.15, 0.3, 0.3, 0.35, 0.4, 0.35],
  }[agentId] || [0.3,0.3,0.4,0.4,0.5,0.5,0.6];

  const isLive = m.running > 0;

  return (
    <div className="agent-detail">
      <header className="agent-profile-head">
        <div className="agent-glyph">{a.glyph}</div>
        <div>
          <h1>{a.name}</h1>
          <div className="sub">
            <span>{a.role}</span>
            <span>·</span>
            <span>{m.total} sessions · 7d</span>
            {isLive && (
              <span className="live">
                <span className="pulse"></span>
                running {m.running} now
              </span>
            )}
          </div>
        </div>
        <div className="actions">
          <a className="crumb-btn" href="Sessions.html">view all sessions ↗</a>
        </div>
      </header>

      <div className="agent-metrics">
        <Metric
          label="Sessions · 7d"
          value={m.total}
          trend={{ dir: "up", text: "+18% vs prev 7d" }}
          sparkline={spark}
        />
        <Metric
          label="First-pass yield"
          value={m.firstPassYield}
          unit="%"
          tone={m.firstPassYield >= 70 ? "ok" : m.firstPassYield >= 50 ? "" : "warn"}
          trend={{ dir: m.firstPassYield >= 70 ? "flat" : "down", text: m.firstPassYield >= 70 ? "steady" : "below team avg (72%)" }}
        />
        <Metric
          label="Avg session"
          value={m.avgMin}
          unit="m"
          trend={{ dir: "down", text: "−12% vs prev 7d" }}
        />
        <Metric
          label="Escalations · 7d"
          value={m.escalated}
          tone={m.escalated > 0 ? "warn" : "ok"}
          trend={{ dir: m.escalated > 0 ? "up" : "flat", text: m.escalated > 0 ? "above baseline" : "none" }}
        />
      </div>

      <section className="agent-section" style={{display: "grid", gridTemplateColumns: "1fr 1fr", gap: 16}}>
        <div>
          <h2>Top skills · last 7d</h2>
          <div className="panel">
            <div className="bar-list">
              {skills.map((s) => (
                <BarRow key={s.name} label={s.name} n={s.count} max={maxSkill} />
              ))}
            </div>
          </div>
        </div>
        <div>
          <h2>Outcome distribution · 7d</h2>
          <div className="panel">
            <div className="bar-list">
              <BarRow label="done" n={m.done} max={m.total || 1} tone="" />
              <BarRow label="retry" n={m.retry} max={m.total || 1} tone="warn" />
              <BarRow label="escalated" n={m.escalated} max={m.total || 1} tone="warn" />
              <BarRow label="running" n={m.running} max={m.total || 1} tone="muted" />
            </div>
            <div style={{marginTop: 14, paddingTop: 12, borderTop: "1px solid var(--bd-2)", display: "flex", justifyContent: "space-between", fontFamily: "var(--ff-mono)", fontSize: 11.5, color: "var(--tx-2)"}}>
              <span>total spent · {Math.round(m.totalMin / 60 * 10) / 10}h</span>
              <span>tokens · {(m.totalTokens / 1000).toFixed(1)}k</span>
              <span style={{color: "var(--tx-0)"}}>cost · ${m.totalCost.toFixed(2)}</span>
            </div>
          </div>
        </div>
      </section>

      <section className="agent-section">
        <h2>Recent feedback from peers</h2>
        <div className="panel">
          <FeedbackList items={feedback} />
        </div>
      </section>

      <section className="agent-section">
        <h2>Recent sessions</h2>
        <div className="panel" style={{padding: 0}}>
          <MiniSessions agentId={agentId} />
        </div>
      </section>
    </div>
  );
}

function AgentApp() {
  const [active, setActive] = useState("DA");
  const order = ["PM", "AR", "DA", "DB", "QA", "SE"];
  const escCount = window.SESSIONS.filter((s) => s.outcome === "escalated").length;

  return (
    <div className="agent-app">
      <AgentTopBar escCount={escCount} />
      <div className="agent-main">
        <aside className="agent-sidebar">
          <div className="agent-side-head">Team · 6 agents</div>
          {order.map((id) => {
            const a = window.AGENTS[id];
            const m = computeMetrics(id);
            return (
              <div
                key={id}
                className={"agent-list-item" + (active === id ? " active" : "")}
                onClick={() => setActive(id)}
              >
                <div className="agent-glyph">{a.glyph}</div>
                <div>
                  <div className="name">{a.name}</div>
                  <div className="role">{a.role}</div>
                </div>
                <div className={"status" + (m.running > 0 ? " live" : "")}>
                  {m.running > 0 ? "◐ live" : m.total + "s"}
                </div>
              </div>
            );
          })}
        </aside>
        <AgentProfile agentId={active} />
      </div>
    </div>
  );
}

ReactDOM.createRoot(document.getElementById("root")).render(<AgentApp />);
