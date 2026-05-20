// Escalations Inbox — actionable
const { useState, useMemo, useEffect } = React;

function EscClock() {
  const [now, setNow] = useState(() => new Date());
  useEffect(() => { const t = setInterval(() => setNow(new Date()), 1000); return () => clearInterval(t); }, []);
  const h = String(now.getUTCHours()).padStart(2, "0");
  const m = String(now.getUTCMinutes()).padStart(2, "0");
  const s = String(now.getUTCSeconds()).padStart(2, "0");
  return <span>{h}:{m}:{s} UTC</span>;
}

function EscTopBar({ escCount }) {
  return (
    <header className="topbar">
      <div className="brand">
        <span className="brand-dot"></span>
        <span>team/</span>
      </div>
      <nav className="nav">
        <a className="nav-item" href="Home.html">Home</a>
        <a className="nav-item" href="Sessions.html">Sessions</a>
        <a className="nav-item" href="Replay.html">Replay</a>
        <a className="nav-item" href="Agent.html">Agents</a>
        <a className="nav-item" href="Flow.html">Flow</a>
        <button className="nav-item active">Escalations{escCount ? <span className="badge">{escCount}</span> : null}</button>
      </nav>
      <div className="topbar-right">
        <span className="live-dot"><span className="pulse"></span>live</span>
        <span><EscClock /></span>
      </div>
    </header>
  );
}

function fmtWait(min) {
  if (min < 60) return min + "m";
  if (min < 60 * 24) return Math.floor(min / 60) + "h " + (min % 60) + "m";
  return Math.floor(min / (60 * 24)) + "d " + Math.floor((min % (60 * 24)) / 60) + "h";
}

function Chain({ chain }) {
  // chain = ["DB→AR", "AR→PM", "PM→human"]
  const labels = ["DevA","DevB","DA","DB","PM","AR","SE","QA","Architect","Security","human"];
  const niceMap = { DA: "DevA", DB: "DevB", PM: "PM", AR: "Architect", SE: "Security", QA: "QA", human: "you" };
  const items = chain.map((step) => step.split("→").map((s) => niceMap[s] || s));
  // Flatten to unique nodes maintaining order
  const nodes = [];
  items.forEach((pair) => pair.forEach((n) => { if (!nodes.length || nodes[nodes.length - 1] !== n) nodes.push(n); }));
  return (
    <div className="esc-chain">
      {nodes.map((n, i) => (
        <React.Fragment key={i}>
          <span className={"step" + (n === "you" ? " human" : "")}>{n}</span>
          {i < nodes.length - 1 && <span className="arr">→</span>}
        </React.Fragment>
      ))}
    </div>
  );
}

function EscOption({ opt, selected, onSelect }) {
  return (
    <div className={"esc-option" + (selected ? " selected" : "")} onClick={() => onSelect(opt.id)}>
      <div className="badge">{opt.id}</div>
      <div className="opt-body">
        <div className="opt-title">{opt.title}</div>
        <div className="opt-pros">
          {opt.pros.map((p, i) => (
            <div key={i} className="opt-row pro">
              <span className="label">pro</span>
              <span className="v">{p}</span>
            </div>
          ))}
        </div>
        <div className="opt-cons" style={{marginTop: 6}}>
          {opt.cons.map((p, i) => (
            <div key={i} className="opt-row con">
              <span className="label">con</span>
              <span className="v">{p}</span>
            </div>
          ))}
        </div>
      </div>
      <div className="opt-meta">
        <div>proposed by</div>
        <div className="by">{opt.proposedBy}</div>
      </div>
    </div>
  );
}

function EscCase({ esc }) {
  const [selected, setSelected] = useState(null);
  const [dismissed, setDismissed] = useState(false);
  if (dismissed) return null;
  const long = esc.waitingSinceMin > 60 * 2;
  return (
    <article className={"esc-case severity-" + esc.severity}>
      <header className="esc-case-head">
        <div className="top">
          <span className="ticket-link">#{esc.ticket}</span>
          <span className={"sev " + esc.severity}>{esc.severity}</span>
          <span className="category">{esc.category}</span>
          <span className={"waiting" + (long ? " long" : "")}>
            <span className="pulse"></span>
            waiting {fmtWait(esc.waitingSinceMin)}
          </span>
        </div>
        <h2>{esc.ticketTitle}</h2>
        <p className="summary">{esc.summary}</p>
      </header>

      <div className="esc-case-body">
        <div className="esc-case-col">
          <h4>Context</h4>
          <ul className="esc-context">
            {esc.context.map((c, i) => <li key={i}>{c}</li>)}
          </ul>
          <h4 style={{marginTop: 14}}>Escalation chain</h4>
          <Chain chain={esc.chainOfEscalation} />
        </div>
        <div className="esc-case-col">
          <h4>How we got here</h4>
          <div className="esc-timeline">
            {esc.timeline.map((t, i) => (
              <div key={i} className="esc-timeline-item">
                <span className="at">{t.at}</span>
                <span className="who">{t.who}</span>
                <span className="txt">{t.txt}</span>
              </div>
            ))}
          </div>
        </div>
      </div>

      <div className="esc-options">
        <div className="esc-options-head">
          <h4>Proposed options · pick one</h4>
          <span className="wfor">{esc.waitingFor}</span>
        </div>
        {esc.options.map((o) => (
          <EscOption key={o.id} opt={o} selected={selected === o.id} onSelect={setSelected} />
        ))}
      </div>

      <div className="esc-actions">
        <button className="btn primary" disabled={!selected} onClick={() => alert("Resolve with option " + selected)}>
          {selected ? `Resolve with option ${selected}` : "Select an option to resolve"}
        </button>
        <button className="btn" onClick={() => alert("Open replay of session " + esc.sessionId)}>
          open session replay ↗
        </button>
        <button className="btn" onClick={() => alert("Comment on #" + esc.ticket + " in GitHub")}>
          comment on github
        </button>
        <button className="btn" onClick={() => setDismissed(true)}>
          defer · ↓
        </button>
        <div className="right">
          <span>or compose a new constraint:</span>
          <span className="link" onClick={() => alert("Open free-form response")}>+ write back to architect</span>
        </div>
      </div>
    </article>
  );
}

function EscApp() {
  const escalations = window.ESCALATIONS;
  const totalWait = escalations.reduce((a, e) => a + e.waitingSinceMin, 0);
  const longest = Math.max(...escalations.map((e) => e.waitingSinceMin));

  return (
    <div className="esc-app">
      <EscTopBar escCount={escalations.length} />
      <div className="alarm-strip"></div>

      <header className="esc-banner">
        <div className="icon">◆</div>
        <div>
          <h1>{escalations.length} escalations waiting</h1>
          <div className="sub">
            you are the <span className="esc">final arbiter</span> — these all reached the top of the chain.
            <span> · last raised <span className="warn">{fmtWait(Math.min(...escalations.map((e) => e.waitingSinceMin)))}</span> ago</span>
          </div>
        </div>
        <div className="stats">
          <div className="stat">
            <span className="k">open</span>
            <span className="v esc">{escalations.length}</span>
          </div>
          <div className="stat">
            <span className="k">longest wait</span>
            <span className="v warn">{fmtWait(longest)}</span>
          </div>
          <div className="stat">
            <span className="k">avg time-to-resolve · 7d</span>
            <span className="v">4h 12m</span>
          </div>
          <div className="stat">
            <span className="k">resolved · 7d</span>
            <span className="v">11</span>
          </div>
        </div>
      </header>

      <main className="esc-body">
        <div className="esc-section-head">
          <span>open</span>
          <strong>{escalations.length}</strong>
          <span style={{color: "var(--tx-3)"}}>· sorted by wait time, longest first</span>
        </div>

        {[...escalations]
          .sort((a, b) => b.waitingSinceMin - a.waitingSinceMin)
          .map((e) => <EscCase key={e.id} esc={e} />)}

        <div className="esc-history">
          <div className="esc-section-head">
            <span>recently resolved</span>
            <strong>{window.ESCALATION_AUDIT.length}</strong>
            <span style={{color: "var(--tx-3)"}}>· audit trail · last 7d</span>
          </div>
          {window.ESCALATION_AUDIT.map((h) => (
            <div key={h.id} className="esc-history-item">
              <span className="at">{h.at}</span>
              <span className="ticket">#{h.ticket}</span>
              <span style={{color: "var(--tx-1)"}}>{h.summary}</span>
              <span className={"outcome " + h.outcome}>{h.outcome}</span>
            </div>
          ))}
        </div>
      </main>
    </div>
  );
}

ReactDOM.createRoot(document.getElementById("root")).render(<EscApp />);
