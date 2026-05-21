// TODO(EPIC-2-binding): replace with MediatR query when EPIC-2 data binding lands

namespace AgentDashboard.Web.Components.Pages.TeamBoard;

public sealed record AgentInfo(string Id, string Name, string Glyph, string Role);

public sealed record ColumnDefinition(string Id, string Label);

public sealed record StubTicket(
    int Id,
    string ColumnId,
    string Title,
    string AgentId,
    string? CoAgentId,
    bool CrossReview,
    int Retry,
    string Age,
    bool Thinking,
    bool Fresh,
    bool Stale,
    bool Escalated,
    string? EscalationTarget);

public static class StubTickets
{
    public static readonly IReadOnlyDictionary<string, AgentInfo> Agents =
        new Dictionary<string, AgentInfo>
        {
            ["PM"] = new("PM", "PM",        "PM", "Project Manager"),
            ["AR"] = new("AR", "Architect",  "Ar", "Project Architect"),
            ["DA"] = new("DA", "DevA",       "Da", "Developer A"),
            ["DB"] = new("DB", "DevB",       "Db", "Developer B"),
            ["QA"] = new("QA", "QA",         "Qa", "QA"),
            ["SE"] = new("SE", "Security",   "Se", "Security"),
        };

    public static readonly IReadOnlyList<ColumnDefinition> Columns =
    [
        new("CREATED",        "Created"),
        new("SPECIFIED",      "Specified"),
        new("IN_DEVELOPMENT", "In development"),
        new("IN_REVIEW",      "In review"),
        new("IN_QA",          "In QA"),
        new("AWAITING_VAL",   "Awaiting validation"),
        new("DONE",           "Done · today"),
    ];

    public static readonly IReadOnlyList<StubTicket> Tickets =
    [
        // CREATED
        new(502, "CREATED",        "[bug] settings page 404 after sso login",                    "PM", null, false, 0, "3m",    Thinking: true,  Fresh: true,  Stale: false, Escalated: false, EscalationTarget: null),
        new(501, "CREATED",        "[feat] export tasks as CSV from /workspaces",                 "PM", null, false, 0, "11m",   Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),
        new(499, "CREATED",        "rename `org.admin` to `org.owner` across api",               "PM", null, false, 0, "26m",   Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),
        new(497, "CREATED",        "remove deprecated `/v1/users/me/teams` endpoint",             "PM", null, false, 0, "1h 02m",Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),

        // SPECIFIED
        new(495, "SPECIFIED",      "add per-org rate limit with redis token bucket",              "AR", null, false, 0, "8m",    Thinking: true,  Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),
        new(491, "SPECIFIED",      "OAuth state parameter must be HMAC-signed",                   "AR", null, false, 0, "34m",   Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),
        new(489, "SPECIFIED",      "migrate logger to pino, structured JSON output",              "AR", null, false, 0, "1h 47m",Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),

        // IN_DEVELOPMENT
        new(488, "IN_DEVELOPMENT", "parse OAuth state and validate signature",                    "DA", "DB", true,  0, "22m",   Thinking: true,  Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),
        new(487, "IN_DEVELOPMENT", "refactor auth middleware: drop legacy cookie path",           "DA", null, false, 1, "1h 14m",Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),
        new(485, "IN_DEVELOPMENT", "feat(billing): proration on plan downgrade",                  "DB", null, false, 0, "2h 03m",Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),
        new(482, "IN_DEVELOPMENT", "add rate-limit headers to all /api responses",               "DB", null, false, 0, "44m",   Thinking: false, Fresh: true,  Stale: false, Escalated: false, EscalationTarget: null),
        new(478, "IN_DEVELOPMENT", "cache org membership lookup (LRU, 5 min)",                   "DA", null, false, 1, "2h 38m",Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),
        new(471, "IN_DEVELOPMENT", "fix race condition in invite acceptance flow",               "DB", null, false, 2, "3h 09m",Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),

        // IN_REVIEW
        new(477, "IN_REVIEW",      "fix flaky test on signup with social provider",              "DB", "DA", true,  2, "1h 02m",Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),
        new(476, "IN_REVIEW",      "extract `useDebouncedValue` from search page",               "DA", "DB", true,  0, "31m",   Thinking: true,  Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),
        new(473, "IN_REVIEW",      "harden CSP: drop `unsafe-inline` from styles",               "DA", "DB", true,  1, "1h 51m",Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),
        new(468, "IN_REVIEW",      "normalize 4xx error envelope (RFC 7807)",                    "DB", "DA", true,  0, "2h 22m",Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),
        new(463, "IN_REVIEW",      "fix refresh-token replay vulnerability",                     "DB", "DA", true,  3, "47m",   Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),

        // IN_QA
        new(466, "IN_QA",          "passkey enrollment: device-bound credential id",             "QA", null, false, 0, "18m",   Thinking: true,  Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),
        new(461, "IN_QA",          "billing portal: cancel-then-restore flow",                   "QA", null, false, 1, "1h 27m",Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),
        new(441, "IN_QA",          "migrate logger to pino, structured JSON output",             "QA", null, false, 1, "6h 12m",Thinking: false, Fresh: false, Stale: true,  Escalated: false, EscalationTarget: null),

        // AWAITING_VAL
        new(455, "AWAITING_VAL",   "CSRF on /api/admin actions — proposed fix conflicts QA scenarios", "AR", null, false, 0, "3h 22m",Thinking: false, Fresh: false, Stale: false, Escalated: true, EscalationTarget: "PM"),
        new(452, "AWAITING_VAL",   "settings: org-level SSO enforcement toggle",                "PM", null, false, 0, "1h 41m",Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),

        // DONE
        new(460, "DONE",           "search: support `is:open` filter on /tasks",                 "PM", null, false, 0, "12m",   Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),
        new(459, "DONE",           "fix N+1 in /v1/projects?expand=members",                    "PM", null, false, 0, "47m",   Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),
        new(456, "DONE",           "audit log: redact bearer tokens in dump",                    "PM", null, false, 0, "1h 14m",Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),
        new(451, "DONE",           "feat: invite via email link, 7d ttl",                        "PM", null, false, 0, "2h 06m",Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),
        new(448, "DONE",           "remove unused `legacyAuth` feature flag",                   "PM", null, false, 0, "3h 18m",Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),
        new(445, "DONE",           "docs: agent escalation protocol v2",                         "PM", null, false, 0, "4h 02m",Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),
        new(442, "DONE",           "fix(ui): dark-mode contrast on disabled buttons",            "PM", null, false, 0, "5h 21m",Thinking: false, Fresh: false, Stale: false, Escalated: false, EscalationTarget: null),
    ];
}
