// Home Dashboard
const { useState, useMemo, useEffect } = React;

function HClock() {
  const [now, setNow] = useState(() => new Date());
  useEffect(() => { const t = setInterval(() => setNow(new Date()), 1000); return () => clearInterval(t); }, []);
  return <span>{String(now.getUTCHours()).padStart(2,"0")}:{String(now.getUTCMinutes()).padStart(2,"0")}:{String(now.getUTCSeconds()).padStart(2,"0")} UTC</span>;
}

function HomeTopBar({ escCount }) {
  return (
    <header className="topbar">
      <div className="brand"><span className="brand-dot"></span><span>team/</span></div>
      <nav className="nav">
        <button className="nav-item active">Home</button>
        <a className="nav-item" href="Sessions.html">Sessions</a>
        <a className="nav-item" href="Replay.html">Replay</a>
        <a className="nav-item" href="Agent.html">Agents</a>
        <a className="nav-item" href="Flow.html">Flow</a>
        <a className="nav-item" href="Escalations.html">Escalations{escCount ? <span className="badge">{escCount}</span> : null}</a>
      </nav>
      <div className="topbar-right">
        <span className="live-dot"><span className="pulse"></span>live</span>
        <span><HClock /></span>
      </div>
    </header>
  );
}

function fmtWait(min) {
  if (min < 60) return min + "m";
  if (min < 60 * 24) return Math.floor(min / 60) + "h " + (min % 60) + "m";
  return Math.floor(min / (60 * 24)) + "d " + Math.floor((min % (60 * 24)) / 60) + "h";
}

function RunningTile({ s }) {
  const a = window.AGENTS[s.agent];
  return (
    <a className="now-tile" href="Replay.html">
      <div className="agent-glyph">{a.glyph}</div>
      <div className="info">
        <div className="top">
          <span className="ticket">#{s.ticket}</span>
          {s.title}
        </div>
        <div className="activity">
          <span className="pulse"></span>
          {s.activity}
        </div>
      </div>
      <div className="elapsed">started {s.started.includes(" ") ? s.started.split(" ")[1] : s.started}</div>
    </a>
  );
}

function LoadRow({ agentId, total, max, running }) {
  const a = window.AGENTS[agentId];
  const pct = (total / max) * 100;
  const tone = total >= max * 0.8 ? "warn" : "";
  return (
    <a className="load-row" href="Agent.html">
      <div className="agent-glyph">{a.glyph}</div>
      <div>
        <div className="name">{a.name}<span className="role">{a.role}</span></div>
      </div>
      <div className="bar"><div className={"fill " + tone} style={{width: pct + "%"}}></div></div>
      <span className="ratio"><strong>{total}</strong> · 7d</span>
    </a>
  );
}

function StreamRow({ s }) {
  const a = window.AGENTS[s.agent];
  const v = (() => {
    if (s.outcome === "done")      return <span className="verdict ok">✓ done</span>;
    if (s.outcome === "retry")     return <span className="verdict retry">↻ {s.retryN}/3</span>;
    if (s.outcome === "warn")      return <span className="verdict warn">▲ {s.retryN}/3</span>;
    if (s.outcome === "danger")    return <span className="verdict danger">▲▲ {s.retryN}/3</span>;
    if (s.outcome === "escalated") return <span className="verdict esc">◆ esc.</span>;
    if (s.outcome === "running")   return <span className="verdict fresh"><span className="pulse"></span>running</span>;
    return <span className="verdict">{s.outcome}</span>;
  })();
  return (
    <a className="stream-row" href="Replay.html">
      <span className="ts">{s.outcome === "running" ? s.started : (s.ended || s.started).split(" ").pop()}</span>
      <span className="agent-glyph">{a.glyph}</span>
      <span className="who">{a.name}</span>
      <span className="desc"><span className="ticket">#{s.ticket}</span>{s.title}</span>
      {v}
    </a>
  );
}

function SparkRow({ label, sub, value, delta, deltaDir, data }) {
  const max = Math.max(...data);
  const min = Math.min(...data);
  return (
    <div className="spark-row">
      <div className="lab">
        <span>{label}</span>
        <span className="k"> · {sub}</span>
      </div>
      <svg viewBox="0 0 120 22" width="120" height="22" preserveAspectRatio="none">
        <polyline
          fill="none"
          stroke="var(--st-fresh)"
          strokeWidth="1.5"
          points={data.map((v, i) => {
            const x = (i / (data.length - 1)) * 118 + 1;
            const y = 21 - ((v - min) / (max - min || 1)) * 20;
            return `${x},${y}`;
          }).join(" ")}
        />
      </svg>
      <span className="v">{value}</span>
    </div>
  );
}

function HomeApp() {
  const sessions = window.SESSIONS;
  const running = sessions.filter((s) => s.outcome === "running");
  const today = sessions.filter((s) => s.day === "today");
  const todayFinished = today.filter((s) => s.outcome !== "running");
  const todayDone = todayFinished.filter((s) => s.outcome === "done").length;
  const todayRetry = todayFinished.filter((s) => s.outcome === "retry" || s.outcome === "warn" || s.outcome === "danger").length;
  const todayEsc = sessions.filter((s) => s.outcome === "escalated" && s.day === "today").length;
  const escCount = window.ESCALATIONS.length;

  // team load — sessions per agent, 7d
  const loadByAgent = useMemo(() => {
    const m = {};
    sessions.forEach((s) => { m[s.agent] = (m[s.agent] || 0) + 1; });
    return m;
  }, [sessions]);
  const maxLoad = Math.max(...Object.values(loadByAgent));

  // recent stream — last 8 events (most recent first)
  const stream = useMemo(() => {
    const ordered = [...sessions]
      .filter((s) => s.day === "today" || s.outcome === "running")
      .sort((a, b) => {
        const aT = a.ended || a.started;
        const bT = b.ended || b.started;
        return aT > bT ? -1 : 1;
      });
    return ordered.slice(0, 9);
  }, [sessions]);

  // longest waiting escalation
  const longestEsc = Math.max(...window.ESCALATIONS.map((e) => e.waitingSinceMin));

  return (
    <div className="home-app">
      <HomeTopBar escCount={escCount} />
      <div className="alarm-strip"></div>
      <main className="home-main">

        <div className="home-greet">
          <div>
            <h1>Good afternoon.</h1>
            <div className="date">Wed, May 20, 2026 · 17:42 UTC</div>
          </div>
          <span className="verdict attention">◆ {escCount} escalations · attention required</span>
        </div>

        {/* alert banner */}
        <div className="home-alert">
          <div className="icon">◆</div>
          <div>
            <div className="top">
              <strong>{escCount} escalations</strong> waiting on you · the team can't move forward without a call
            </div>
            <div className="sub">
              longest waiting <strong>{fmtWait(longestEsc)}</strong> · last raised 31m ago
            </div>
          </div>
          <a className="cta" href="Escalations.html">go to inbox →</a>
        </div>

        {/* quick search */}
        <a className="quick-search" href="Sessions.html">
          <span className="glyph">⌕</span>
          <span className="label">search sessions by ticket, agent, content…</span>
          <span className="kbd"><kbd>⌘</kbd><kbd>K</kbd></span>
        </a>

        <div className="home-grid">
          {/* LEFT COL */}
          <div className="home-col">

            {/* Now */}
            <div className="card">
              <div className="card-head">
                <h2>Now</h2>
                <span className="sub">live sessions in progress</span>
                <span className="right"><a href="Sessions.html">all sessions →</a></span>
              </div>
              <div className="card-body">
                {running.length === 0 && (
                  <div style={{fontFamily: "var(--ff-mono)", fontSize: 11.5, color: "var(--tx-3)", padding: "12px 4px"}}>
                    // team is idle right now
                  </div>
                )}
                {running.map((s) => <RunningTile key={s.id} s={s} />)}
              </div>
            </div>

            {/* Today */}
            <div className="card">
              <div className="card-head">
                <h2>Today</h2>
                <span className="sub">Wed, May 20 · started 09:18 UTC</span>
                <span className="right"><a href="Flow.html">trends →</a></span>
              </div>
              <div className="today-grid">
                <div className="today-stat">
                  <div className="v">{today.length}</div>
                  <div className="k">sessions</div>
                </div>
                <div className="today-stat">
                  <div className="v ok">{todayDone}</div>
                  <div className="k">done</div>
                </div>
                <div className="today-stat">
                  <div className="v warn">{todayRetry}</div>
                  <div className="k">retries</div>
                </div>
                <div className="today-stat">
                  <div className="v esc">{todayEsc}</div>
                  <div className="k">escalated</div>
                </div>
              </div>
            </div>

            {/* Stream */}
            <div className="card">
              <div className="card-head">
                <h2>Activity stream</h2>
                <span className="sub">most recent first · today</span>
                <span className="right"><a href="Sessions.html">all sessions →</a></span>
              </div>
              <div className="card-body">
                {stream.map((s) => <StreamRow key={s.id} s={s} />)}
              </div>
            </div>

          </div>

          {/* RIGHT COL */}
          <div className="home-col">

            {/* Team load */}
            <div className="card">
              <div className="card-head">
                <h2>Team load</h2>
                <span className="sub">sessions · last 7d</span>
                <span className="right"><a href="Agent.html">profiles →</a></span>
              </div>
              <div className="card-body">
                {["PM", "AR", "DA", "DB", "QA", "SE"].map((id) => (
                  <LoadRow
                    key={id}
                    agentId={id}
                    total={loadByAgent[id] || 0}
                    max={maxLoad}
                    running={running.some((s) => s.agent === id)}
                  />
                ))}
              </div>
            </div>

            {/* Top of mind */}
            <div className="card">
              <div className="card-head">
                <h2>Top of mind</h2>
                <span className="sub">things worth a look</span>
              </div>
              <div className="card-body">
                <a className="tom-item danger" href="Escalations.html" style={{textDecoration: "none", display: "block"}}>
                  <div className="tag">◆ blocking · esc #463</div>
                  <div className="title">refresh-token replay fix has been waiting <strong>17h</strong> for your call — blocks release v2.4</div>
                  <div className="meta"><span className="arrow">→</span> open inbox</div>
                </a>
                <a className="tom-item" href="Flow.html" style={{textDecoration: "none", display: "block"}}>
                  <div className="tag">⚠ bottleneck · IN_REVIEW</div>
                  <div className="title">cross-review wait time grew <strong>+38%</strong> week-over-week — DevA & DevB regularly blocked on each other</div>
                  <div className="meta"><span className="arrow">→</span> see flow analytics</div>
                </a>
                <a className="tom-item" href="Sessions.html" style={{textDecoration: "none", display: "block"}}>
                  <div className="tag">⚠ stale · #441</div>
                  <div className="title">pino logger migration · stuck in QA <strong>6h 12m</strong> — pending dev addressing</div>
                  <div className="meta"><span className="arrow">→</span> view session</div>
                </a>
              </div>
            </div>

            {/* Pulse (7d sparklines) */}
            <div className="card">
              <div className="card-head">
                <h2>Pulse · 7d</h2>
                <span className="sub">vital signs</span>
                <span className="right"><a href="Flow.html">full →</a></span>
              </div>
              <div className="card-body">
                <SparkRow label="throughput" sub="sessions/day" value="23.0" data={[24,27,8,12,26,31,23]} />
                <SparkRow label="first-pass yield" sub="%" value="68%" data={[71,72,70,68,69,68,68]} />
                <SparkRow label="lead time" sub="median" value="3h 14m" data={[4.1,3.8,4.0,3.6,3.4,3.3,3.2]} />
                <SparkRow label="retries" sub="avg/session" value="0.42" data={[0.38,0.4,0.44,0.41,0.43,0.44,0.42]} />
                <SparkRow label="escalation rate" sub="%" value="3.3%" data={[2.1,2.0,2.3,2.5,2.8,3.0,3.3]} />
              </div>
            </div>

          </div>
        </div>

      </main>
    </div>
  );
}

ReactDOM.createRoot(document.getElementById("root")).render(<HomeApp />);
