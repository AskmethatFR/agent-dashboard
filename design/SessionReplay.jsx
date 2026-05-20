// Session Replay — hi-fi React component
const { useState, useMemo, useEffect, useRef, useCallback } = React;

function ClockR() {
  const [now, setNow] = useState(() => new Date());
  useEffect(() => { const t = setInterval(() => setNow(new Date()), 1000); return () => clearInterval(t); }, []);
  const h = String(now.getUTCHours()).padStart(2, "0");
  const m = String(now.getUTCMinutes()).padStart(2, "0");
  const s = String(now.getUTCSeconds()).padStart(2, "0");
  return <span>{h}:{m}:{s} UTC</span>;
}

function ReplayTopBar({ escCount }) {
  return (
    <header className="topbar">
      <div className="brand">
        <span className="brand-dot"></span>
        <span>team/</span>
      </div>
      <nav className="nav">
        <a className="nav-item" href="Home.html">Home</a>
        <a className="nav-item" href="Sessions.html">Sessions</a>
        <button className="nav-item active">Replay</button>
        <a className="nav-item" href="Agent.html">Agents</a>
        <a className="nav-item" href="Flow.html">Flow</a>
        <a className="nav-item" href="Escalations.html">Escalations{escCount ? <span className="badge">{escCount}</span> : null}</a>
      </nav>
      <div className="topbar-right">
        <span className="cmdk" style={{display: "inline-flex", alignItems: "center", gap: 6, padding: "3px 7px 3px 8px", border: "1px solid var(--bd-2)", borderRadius: 3, color: "var(--tx-1)", fontFamily: "var(--ff-mono)", fontSize: 11, background: "var(--bg-1)"}}>
          ⌘K <span style={{fontFamily: "var(--ff-mono)", fontSize: 10, color: "var(--tx-3)", padding: "1px 4px", border: "1px solid var(--bd-2)", borderRadius: 2, background: "var(--bg-2)"}}>jump</span>
        </span>
        <span className="live-dot"><span className="pulse"></span>live</span>
        <span><ClockR /></span>
      </div>
    </header>
  );
}

function OutcomeBadge({ outcome, retryN }) {
  const map = {
    done: { cls: "done", text: "✓ done" },
    retry: { cls: "retry", text: "↻ retry " + retryN + "/3" },
    warn: { cls: "warn", text: "▲ retry " + retryN + "/3" },
    danger: { cls: "danger", text: "▲▲ retry " + retryN + "/3" },
    escalated: { cls: "escalated", text: "◆ escalated" },
  };
  const v = map[outcome] || { cls: "", text: outcome };
  return <span className={"outcome-big " + v.cls}>{v.text}</span>;
}

function fmtDur(min) {
  if (!min) return "—";
  if (min < 60) return min + "m";
  return Math.floor(min / 60) + "h" + String(min % 60).padStart(2, "0") + "m";
}

function SessionHead({ sd }) {
  const agent = window.AGENTS[sd.agent];
  return (
    <section className="session-head">
      <div className="agent-bigglyph">{agent.glyph}</div>
      <div className="head-body">
        <div className="head-kicker">
          <span className="pill">{sd.role}</span>
          <span>·</span>
          <span>{agent.name}</span>
          <span className="ticket" onClick={() => alert("Would open ticket panel #" + sd.ticket)}>
            #{sd.ticket}
          </span>
        </div>
        <h1>{sd.title}</h1>
        <div className="stats">
          <span className="item"><span className="k">started</span><span className="v">{sd.started.split(" ")[1]}</span></span>
          <span className="item"><span className="k">ended</span><span className="v">{sd.ended.split(" ")[1]}</span></span>
          <span className="item"><span className="k">duration</span><span className="v">{fmtDur(sd.durationMin)}</span></span>
          <span className="item"><span className="k">events</span><span className="v">{sd.events}</span></span>
          <span className="item"><span className="k">findings</span><span className={"v " + (sd.findings ? "warn" : "")}>{sd.findings}</span></span>
          <span className="item"><span className="k">tokens</span><span className="v">{(sd.cost.tokens / 1000).toFixed(1)}k</span></span>
          <span className="item"><span className="k">cost</span><span className="v">${sd.cost.dollars.toFixed(2)}</span></span>
          <span className="item"><span className="k">model</span><span className="v">{sd.model}</span></span>
          <span className="item"><span className="k">branch</span><span className="v cyan">{sd.branch}</span></span>
        </div>
      </div>
      <div className="actions">
        <OutcomeBadge outcome={sd.outcome} retryN={sd.retryN} />
        <button className="crumb-btn">open in github ↗</button>
        <button className="crumb-btn">copy permalink</button>
      </div>
    </section>
  );
}

function Crumb({ sd }) {
  return (
    <div className="crumb">
      <a href="Sessions.html">← back to sessions</a>
      <span className="sep">/</span>
      <span className="link" onClick={() => alert("Would open ticket #" + sd.ticket)}>ticket #{sd.ticket}</span>
      <span className="sep">/</span>
      <span className="id">session {sd.id}</span>
      <div className="crumb-right">
        <span className="crumb-btn" style={{cursor: "default"}}>{sd.commit}</span>
        <div className="crumb-arrows">
          <button className="arr" title={"prev on ticket — " + sd.prevOnTicket.role}>← prev</button>
          <button className="arr" title={"next on ticket — " + sd.nextOnTicket.role}>next →</button>
        </div>
      </div>
    </div>
  );
}

const KIND_LABEL = {
  prompt: "prompt",
  thought: "thinking",
  tool: "tool",
  peer_msg: "peer msg",
  finding: "finding",
  state_change: "state",
  retry: "retry",
  escalation: "escalation",
  session_end: "end",
};

const KIND_GLYPH = {
  prompt: "▸",
  thought: "◌",
  tool: "⚒",
  peer_msg: "↔",
  finding: "◇",
  state_change: "↦",
  retry: "↻",
  escalation: "◆",
  session_end: "◐",
};

function EventItem({ e, expandedThoughts, onToggleExpand, manuallyExpanded, focused, onClick }) {
  const isExpandable = e.body && (e.kind === "prompt" || e.kind === "thought" || e.kind === "finding" || e.kind === "peer_msg" || (e.kind === "tool" && e.body));
  const longBody = e.body && (e.bodyLength > 200 || e.body.length > 200);

  // Auto-expand for findings, prompts (short), peer_msg always.
  // Thoughts collapsed unless expandedThoughts on.
  // Tool body shown if short, hidden by default if "body".
  const shouldShow = (() => {
    if (e.kind === "thought") return expandedThoughts || manuallyExpanded;
    if (e.kind === "prompt" && longBody) return manuallyExpanded;
    if (e.kind === "tool" && e.body) return manuallyExpanded;
    if (e.kind === "finding") return true;
    if (e.kind === "peer_msg") return manuallyExpanded || (e.body && e.body.length < 140);
    if (e.kind === "state_change") return true;
    if (e.kind === "retry") return manuallyExpanded;
    if (e.kind === "session_end") return true;
    return false;
  })();

  const cls = "event " + e.kind + (e.severity === "high" ? " high" : "") + (focused ? " focused" : "");

  return (
    <div className={cls} id={"e" + e.step} onClick={onClick}>
      <div className="rail">
        <span className="t">{e.t.slice(0, 5)}</span>
        <span className="stepnum">#{String(e.step).padStart(2, "0")}</span>
        <span className="elapsed">{e.elapsed}</span>
      </div>
      <div className="glyph-col">
        <span className="glyph">{KIND_GLYPH[e.kind]}</span>
      </div>
      <div className="body">
        <div className="head">
          <span className="kind">{KIND_LABEL[e.kind]}</span>
          {e.severity && e.severity !== "expected" && e.severity !== "ok" && (
            <span className={"severity " + e.severity}>{e.severity}</span>
          )}
          {e.location && (
            <span className="location" onClick={(ev) => { ev.stopPropagation(); }}>{e.location}</span>
          )}
          <span className="title">{e.summary}</span>
          {e.meta && <span className="meta">· {e.meta}</span>}
          {e.diff && (
            <span className="diff">
              {e.diff.add > 0 && <span className="add">+{e.diff.add}</span>}
              {e.diff.del > 0 && <span className="del">−{e.diff.del}</span>}
            </span>
          )}
          {e.duration && <span className="meta">· {e.duration}s</span>}
        </div>
        {shouldShow && e.body && <div className="body-content">{e.body}</div>}
        {e.skills && (
          <div className="skills">
            {e.skills.map((s) => <span className="skill" key={s}>{s}</span>)}
          </div>
        )}
        {isExpandable && !shouldShow && (
          <button className="expand-toggle" onClick={(ev) => { ev.stopPropagation(); onToggleExpand(e.step); }}>
            {e.bodyLength ? `▾ expand (${e.bodyLength} chars)` : "▾ expand"}
          </button>
        )}
        {isExpandable && shouldShow && !(e.kind === "finding" || e.kind === "state_change" || e.kind === "session_end") && (
          <button className="expand-toggle" onClick={(ev) => { ev.stopPropagation(); onToggleExpand(e.step); }}>
            ▴ collapse
          </button>
        )}
      </div>
    </div>
  );
}

function Outline({ sd, activeStep, onJump }) {
  const eventStats = useMemo(() => {
    const events = window.SESSION_EVENTS;
    const byKind = {};
    events.forEach((e) => { byKind[e.kind] = (byKind[e.kind] || 0) + 1; });
    return byKind;
  }, []);
  return (
    <aside className="outline">
      <h4>Session at a glance</h4>
      <div className="outline-summary">
        <div className="row"><span className="k">prompts</span><span className="v">{eventStats.prompt || 0}</span></div>
        <div className="row"><span className="k">tool calls</span><span className="v">{eventStats.tool || 0}</span></div>
        <div className="row"><span className="k">peer messages</span><span className="v">{eventStats.peer_msg || 0}</span></div>
        <div className="row"><span className="k">findings</span><span className="v warn">{eventStats.finding || 0}</span></div>
        <div className="row"><span className="k">state changes</span><span className="v">{eventStats.state_change || 0}</span></div>
        <div className="row"><span className="k">retries</span><span className="v warn">{eventStats.retry || 0}</span></div>
      </div>
      <h4>Outline</h4>
      <div className="outline-section">
        {window.SESSION_OUTLINE.map((o, i) => {
          const active = activeStep >= o.step && (i === window.SESSION_OUTLINE.length - 1 || activeStep < window.SESSION_OUTLINE[i + 1].step);
          return (
            <div key={o.step} className={"outline-item" + (active ? " active" : "")} onClick={() => onJump(o.step)}>
              <span className="icon">{o.icon}</span>
              <span className="stepnum">#{String(o.step).padStart(2, "0")}</span>
              <span>{o.label}</span>
            </div>
          );
        })}
      </div>
      <h4 style={{marginTop: 20}}>Related</h4>
      <div className="outline-section">
        <div className="outline-item" onClick={() => alert("prev session on ticket")}>
          <span className="icon">◂</span>
          <span style={{color: "var(--tx-2)"}}>prev · {sd.prevOnTicket.role}</span>
        </div>
        <div className="outline-item" onClick={() => alert("next session on ticket")}>
          <span className="icon">▸</span>
          <span style={{color: "var(--tx-2)"}}>next · {sd.nextOnTicket.role}</span>
        </div>
      </div>
    </aside>
  );
}

function ReplayApp() {
  const sd = window.SESSION_DETAIL;
  const allEvents = window.SESSION_EVENTS;
  const [filter, setFilter] = useState("all");
  const [expandedThoughts, setExpandedThoughts] = useState(false);
  const [manual, setManual] = useState({});
  const [activeStep, setActiveStep] = useState(1);
  const timelineRef = useRef();

  const filtered = useMemo(() => {
    if (filter === "all") return allEvents;
    if (filter === "prompts") return allEvents.filter((e) => e.kind === "prompt" || e.kind === "peer_msg");
    if (filter === "tools") return allEvents.filter((e) => e.kind === "tool");
    if (filter === "peer") return allEvents.filter((e) => e.kind === "peer_msg");
    if (filter === "findings") return allEvents.filter((e) => e.kind === "finding" || e.kind === "retry" || e.kind === "escalation");
    if (filter === "changes") return allEvents.filter((e) => e.kind === "state_change" || e.kind === "retry" || e.kind === "escalation" || e.kind === "session_end");
    return allEvents;
  }, [filter, allEvents]);

  const toggleExpand = (step) => setManual((m) => ({ ...m, [step]: !m[step] }));

  const jumpToStep = (step) => {
    setActiveStep(step);
    const el = document.getElementById("e" + step);
    if (el && timelineRef.current) {
      const top = el.offsetTop - 60;
      timelineRef.current.scrollTo({ top, behavior: "smooth" });
    }
  };

  useEffect(() => {
    const el = timelineRef.current;
    if (!el) return;
    const onScroll = () => {
      const top = el.scrollTop + 80;
      let cur = 1;
      for (const e of filtered) {
        const node = document.getElementById("e" + e.step);
        if (node && node.offsetTop <= top) cur = e.step;
      }
      setActiveStep(cur);
    };
    el.addEventListener("scroll", onScroll);
    return () => el.removeEventListener("scroll", onScroll);
  }, [filtered]);

  return (
    <div className="replay-app">
      <ReplayTopBar escCount={1} />
      <div>
        <Crumb sd={sd} />
        <SessionHead sd={sd} />
      </div>
      <div className="replay-grid">
        <div className="timeline" ref={timelineRef}>
          <div className="replay-filter">
            <span className="label">show</span>
            {[
              ["all", "all"], ["prompts", "prompts"], ["tools", "tools"],
              ["peer", "peer msgs"], ["findings", "findings"], ["changes", "state changes"],
            ].map(([k, l]) => (
              <button key={k} className={"chip" + (filter === k ? " active" : "")} onClick={() => setFilter(k)}>{l}</button>
            ))}
            <div className="right">
              <button className="toggle" onClick={() => setExpandedThoughts((v) => !v)}>
                {expandedThoughts ? "▴ hide thoughts" : "▾ show thoughts"}
              </button>
              <button className="toggle" onClick={() => {
                if (Object.keys(manual).length === filtered.length) setManual({});
                else { const m = {}; filtered.forEach((e) => m[e.step] = true); setManual(m); }
              }}>⇕ expand all</button>
            </div>
          </div>
          <div className="events">
            {filtered.map((e) => (
              <EventItem
                key={e.step}
                e={e}
                expandedThoughts={expandedThoughts}
                manuallyExpanded={!!manual[e.step]}
                onToggleExpand={toggleExpand}
                focused={false}
                onClick={() => setActiveStep(e.step)}
              />
            ))}
          </div>
        </div>
        <Outline sd={sd} activeStep={activeStep} onJump={jumpToStep} />
      </div>
    </div>
  );
}

ReactDOM.createRoot(document.getElementById("root")).render(<ReplayApp />);
