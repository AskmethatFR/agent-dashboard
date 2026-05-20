// Flow Analytics
const { useState, useMemo, useEffect } = React;

function FlowClock() {
  const [now, setNow] = useState(() => new Date());
  useEffect(() => { const t = setInterval(() => setNow(new Date()), 1000); return () => clearInterval(t); }, []);
  return <span>{String(now.getUTCHours()).padStart(2,"0")}:{String(now.getUTCMinutes()).padStart(2,"0")}:{String(now.getUTCSeconds()).padStart(2,"0")} UTC</span>;
}

function FlowTopBar({ escCount }) {
  return (
    <header className="topbar">
      <div className="brand"><span className="brand-dot"></span><span>team/</span></div>
      <nav className="nav">
        <a className="nav-item" href="Home.html">Home</a>
        <a className="nav-item" href="Sessions.html">Sessions</a>
        <a className="nav-item" href="Replay.html">Replay</a>
        <a className="nav-item" href="Agent.html">Agents</a>
        <button className="nav-item active">Flow</button>
        <a className="nav-item" href="Escalations.html">Escalations{escCount ? <span className="badge">{escCount}</span> : null}</a>
      </nav>
      <div className="topbar-right">
        <span className="live-dot"><span className="pulse"></span>live</span>
        <span><FlowClock /></span>
      </div>
    </header>
  );
}

// --- mock flow data (would be computed from session/state-change events) ---
const FLOW = {
  leadTime:    { median: "3h 14m", p95: "1d 4h", trend: { dir: "down", text: "−14% vs prev 7d" } },
  cycleByState: [
    { state: "CREATED",         median: 14,  p95: 42,   color: "ok" },
    { state: "SPECIFIED",       median: 22,  p95: 68,   color: "ok" },
    { state: "IN_DEVELOPMENT",  median: 38,  p95: 124,  color: "ok" },
    { state: "IN_REVIEW",       median: 96,  p95: 280,  color: "bottleneck" }, // bottleneck
    { state: "IN_QA",           median: 24,  p95: 51,   color: "ok" },
    { state: "AWAITING_VAL",    median: 41,  p95: 188,  color: "ok" },
  ],
  firstPassYield: { pct: 68, target: 75, trend: { dir: "flat", text: "stable" } },
  retryDistribution: [
    { bucket: "0", n: 124, pct: 0.67, tone: "" },
    { bucket: "1", n: 38,  pct: 0.21, tone: "" },
    { bucket: "2", n: 16,  pct: 0.09, tone: "warn" },
    { bucket: "3", n: 6,   pct: 0.03, tone: "danger" },
  ],
  escalationRate: { pct: 3.3, abs: 6, total: 184, trend: { dir: "up", text: "↑ from 2.1% last week" } },
  throughput: [
    { day: "Thu 14", n: 24 },
    { day: "Fri 15", n: 27 },
    { day: "Sat 16", n: 8 },
    { day: "Sun 17", n: 12 },
    { day: "Mon 18", n: 26 },
    { day: "Tue 19", n: 31 },
    { day: "Wed 20", n: 23, today: true },
  ],
  // Cumulative flow diagram — stacked series, 7 days
  cfd: {
    days: ["14", "15", "16", "17", "18", "19", "20"],
    series: [
      { name: "CREATED",        color: "#3a4250", values: [4, 5, 2, 3, 6, 5, 4] },
      { name: "SPECIFIED",      color: "#4d5d7a", values: [3, 4, 2, 4, 4, 3, 3] },
      { name: "IN_DEVELOPMENT", color: "#5c7aa8", values: [5, 6, 4, 5, 7, 6, 6] },
      { name: "IN_REVIEW",      color: "#e6a33a", values: [3, 4, 5, 6, 7, 6, 5] }, // amber, the bottleneck
      { name: "IN_QA",          color: "#7c8cf8", values: [2, 3, 2, 2, 3, 4, 3] },
      { name: "AWAITING_VAL",   color: "#4ad6e0", values: [1, 1, 1, 2, 1, 2, 2] },
      { name: "DONE",           color: "#3bc97a", values: [12, 18, 20, 22, 30, 38, 45] },
    ],
  },
  // bottlenecks
  bottlenecks: [
    { state: "IN_REVIEW", reason: "cross-review wait time grew 38% — DevA & DevB often blocked on each other", action: "consider parallelizing review with QA on low-risk tickets" },
    { state: "AWAITING_VAL", reason: "your own queue — 2 escalations open > 1d", action: "tackle longest-waiting first" },
  ],
};

function Tile({ label, value, tone, trend, hint }) {
  return (
    <div className="flow-panel">
      <div style={{display: "flex", justifyContent: "space-between", alignItems: "baseline"}}>
        <h3 style={{margin: 0}}>{label}</h3>
        {trend && (
          <span className="trend" style={{fontFamily: "var(--ff-mono)", fontSize: 10.5, color: "var(--tx-2)"}}>
            {trend.dir === "up" && <span style={{color: "var(--st-danger)"}}>↑</span>}
            {trend.dir === "down" && <span style={{color: "var(--st-ok)"}}>↓</span>}
            {trend.dir === "flat" && <span style={{color: "var(--tx-3)"}}>—</span>}
            <span style={{marginLeft: 4}}>{trend.text}</span>
          </span>
        )}
      </div>
      <div style={{fontFamily: "var(--ff-mono)", fontSize: 28, color: "var(--tx-0)", fontWeight: 500, marginTop: 8, letterSpacing: "-0.01em"}}>
        <span className={tone === "warn" ? "" : ""} style={tone === "warn" ? {color: "var(--st-warn)"} : tone === "danger" ? {color: "var(--st-danger)"} : tone === "ok" ? {color: "var(--st-ok)"} : {}}>{value}</span>
      </div>
      {hint && <div style={{fontFamily: "var(--ff-mono)", fontSize: 10.5, color: "var(--tx-3)", marginTop: 6}}>{hint}</div>}
    </div>
  );
}

function CycleTime() {
  const maxP95 = Math.max(...FLOW.cycleByState.map((c) => c.p95));
  return (
    <div className="flow-panel">
      <h3>Cycle time by state</h3>
      <div className="sub">median bars · p95 in dim · bottleneck highlighted</div>
      <div className="cycle-rows">
        {FLOW.cycleByState.map((c) => (
          <div key={c.state} className="cycle-row">
            <span className="state">{c.state}</span>
            <div className="bar-area">
              <div className={"bar" + (c.color === "bottleneck" ? " bottleneck" : "")} style={{width: (c.median / maxP95 * 100) + "%"}}></div>
            </div>
            <span className="median">{c.median}m</span>
            <span className="p95">p95 {c.p95}m</span>
          </div>
        ))}
      </div>
    </div>
  );
}

function RetryHistogram() {
  const max = Math.max(...FLOW.retryDistribution.map((r) => r.n));
  return (
    <div className="flow-panel">
      <h3>Retry distribution</h3>
      <div className="sub">how many review loops before merge · last 7d · n=184</div>
      <div className="histogram">
        {FLOW.retryDistribution.map((r) => (
          <div key={r.bucket} className="histo-col" style={{height: "100%"}}>
            <span className="v">{r.n}</span>
            <div className={"fill " + r.tone} style={{height: (r.n / max * 100) + "%"}}></div>
          </div>
        ))}
      </div>
      <div className="histo-labels">
        {FLOW.retryDistribution.map((r) => <span key={r.bucket}>{r.bucket} retries</span>)}
      </div>
    </div>
  );
}

function Throughput() {
  const max = Math.max(...FLOW.throughput.map((t) => t.n));
  return (
    <div className="flow-panel">
      <h3>Throughput · sessions / day</h3>
      <div className="sub">last 7 days · today partial</div>
      <div className="throughput">
        {FLOW.throughput.map((t) => (
          <div key={t.day} className="thr-col" title={t.day + " · " + t.n + " sessions"} style={{height: "100%"}}>
            <span className="v">{t.n}</span>
            <div className="fill" style={{height: (t.n / max * 100) + "%", background: t.today ? "var(--st-fresh)" : "#5c7aa8"}}></div>
          </div>
        ))}
      </div>
      <div className="thr-labels">
        {FLOW.throughput.map((t) => <span key={t.day} className={t.today ? "today" : ""}>{t.day}</span>)}
      </div>
    </div>
  );
}

// Stacked area CFD using SVG
function CumulativeFlow() {
  const cfd = FLOW.cfd;
  const W = 560, H = 220, padX = 36, padY = 16;
  const days = cfd.days.length;
  const xStep = (W - padX * 2) / (days - 1);

  // build cumulative arrays
  const cumByDay = cfd.days.map((_, di) =>
    cfd.series.reduce((acc, s, si) => {
      const prev = si === 0 ? 0 : acc[si - 1];
      acc.push(prev + s.values[di]);
      return acc;
    }, [])
  );

  const maxTotal = Math.max(...cumByDay.map((d) => d[d.length - 1]));
  const yScale = (v) => H - padY - (v / maxTotal) * (H - padY * 2);
  const xScale = (i) => padX + i * xStep;

  // Build polygons per series
  const polys = cfd.series.map((s, si) => {
    const points = [];
    cfd.days.forEach((_, di) => {
      const upper = cumByDay[di][si];
      points.push([xScale(di), yScale(upper)]);
    });
    // back along lower line (which is the previous series, or 0)
    for (let di = days - 1; di >= 0; di--) {
      const lower = si === 0 ? 0 : cumByDay[di][si - 1];
      points.push([xScale(di), yScale(lower)]);
    }
    return { name: s.name, color: s.color, points };
  });

  return (
    <div className="flow-panel">
      <h3>Cumulative flow · 7 days</h3>
      <div className="sub">WIP per state stacked over time · widening bands = bottleneck</div>
      <div className="cfd-wrap">
        <svg width="100%" viewBox={`0 0 ${W} ${H}`} style={{display: "block"}}>
          {/* grid */}
          {[0, 0.25, 0.5, 0.75, 1].map((g, i) => (
            <line key={i}
              x1={padX} x2={W - padX}
              y1={padY + g * (H - padY * 2)} y2={padY + g * (H - padY * 2)}
              stroke="var(--bd-1)" strokeWidth="1" />
          ))}
          {/* y-axis labels */}
          <text x={4} y={padY + 4} fill="var(--tx-3)" fontFamily="var(--ff-mono)" fontSize="9">{maxTotal}</text>
          <text x={4} y={H - padY + 4} fill="var(--tx-3)" fontFamily="var(--ff-mono)" fontSize="9">0</text>
          {/* polygons (reverse so DONE is rendered last / on top) */}
          {polys.map((p, i) => (
            <polygon key={i} points={p.points.map((pt) => pt.join(",")).join(" ")}
              fill={p.color} fillOpacity="0.85" stroke={p.color} strokeOpacity="0.4" strokeWidth="0.5" />
          ))}
          {/* x-axis labels */}
          {cfd.days.map((d, i) => (
            <text key={i} x={xScale(i)} y={H - 2} fill="var(--tx-3)" fontFamily="var(--ff-mono)" fontSize="9" textAnchor="middle">{d}</text>
          ))}
        </svg>
        <div className="cfd-legend">
          {cfd.series.map((s) => (
            <span key={s.name} className="item">
              <span className="swatch" style={{background: s.color}}></span>
              <span>{s.name}</span>
            </span>
          ))}
        </div>
      </div>
    </div>
  );
}

function Bottlenecks() {
  return (
    <div className="flow-panel">
      <h3>Bottlenecks · 7d</h3>
      <div className="sub">automated detection · widening band or growing wait time</div>
      <div style={{display: "flex", flexDirection: "column", gap: 10, marginTop: 4}}>
        {FLOW.bottlenecks.map((b, i) => (
          <div key={i} style={{padding: 10, background: "var(--bg-2)", border: "1px solid var(--bd-2)", borderLeft: "2px solid var(--st-warn)", borderRadius: "var(--r-2)"}}>
            <div style={{fontFamily: "var(--ff-mono)", fontSize: 10.5, color: "var(--st-warn)", textTransform: "uppercase", letterSpacing: "0.06em", marginBottom: 4}}>
              {b.state}
            </div>
            <div style={{fontSize: 12.5, color: "var(--tx-1)", lineHeight: 1.5}}>{b.reason}</div>
            <div style={{fontFamily: "var(--ff-mono)", fontSize: 11, color: "var(--tx-2)", marginTop: 6}}>→ {b.action}</div>
          </div>
        ))}
      </div>
    </div>
  );
}

function FlowApp() {
  const [range, setRange] = useState("7d");
  const escCount = window.SESSIONS.filter((s) => s.outcome === "escalated").length;

  return (
    <div className="flow-app">
      <FlowTopBar escCount={escCount} />
      <main className="flow-main">
        <div className="flow-head">
          <h1>Flow analytics</h1>
          <span className="sub">retrospective — what the team's flow looks like over time</span>
          <div className="range">
            {["24h", "7d", "30d", "90d"].map((r) => (
              <button key={r} className={range === r ? "active" : ""} onClick={() => setRange(r)}>{r}</button>
            ))}
          </div>
        </div>

        <div className="flow-tiles" style={{marginTop: 18}}>
          <Tile
            label="Lead time"
            value={FLOW.leadTime.median}
            trend={FLOW.leadTime.trend}
            hint={"p95 · " + FLOW.leadTime.p95}
          />
          <Tile
            label="First-pass yield"
            value={FLOW.firstPassYield.pct + "%"}
            tone={FLOW.firstPassYield.pct >= FLOW.firstPassYield.target ? "ok" : "warn"}
            trend={FLOW.firstPassYield.trend}
            hint={"target · " + FLOW.firstPassYield.target + "%"}
          />
          <Tile
            label="Escalation rate"
            value={FLOW.escalationRate.pct + "%"}
            tone="warn"
            trend={FLOW.escalationRate.trend}
            hint={FLOW.escalationRate.abs + " of " + FLOW.escalationRate.total + " sessions"}
          />
          <Tile
            label="Throughput"
            value="23.0 / day"
            trend={{ dir: "up", text: "+12% vs prev 7d" }}
            hint="7-day rolling avg"
          />
        </div>

        <div className="flow-grid">
          <CycleTime />
          <RetryHistogram />
          <CumulativeFlow />
          <Throughput />
        </div>

        <div className="flow-grid" style={{gridTemplateColumns: "1fr"}}>
          <Bottlenecks />
        </div>
      </main>
    </div>
  );
}

ReactDOM.createRoot(document.getElementById("root")).render(<FlowApp />);
