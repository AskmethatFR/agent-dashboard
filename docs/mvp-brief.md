# MVP Brief — agent-dashboard v0.1

> Document d'entrée pour le PM agent. À refiner en tickets GitHub selon
> le PROTOCOL.md v2 (state machine : CREATED → SPECIFIED → ...).

## 1. Vision

Permettre à n'importe quel utilisateur de **lancer une image Docker
en une commande** et de voir le **Team Board** de son équipe agentique
alimenté en temps quasi réel depuis ses **GitHub Issues**.

> *"docker run, je vois mon board, je suis content."*

C'est le **plus petit MVP utilisable** : pas de Sessions, pas de Replay,
pas d'Escalations. On valide la chaîne **ingestion → persistence →
rendu → distribution** sur le cas le plus simple.

## 2. Persona & promesse

**Persona :** un tech lead / engineering manager qui pilote une équipe
de sub-agents Claude Code coordonnés via le PROTOCOL.md v2 sur un repo
GitHub donné.

**Promesse v0.1 :** "En 2 minutes, tu vois ton board agentique dans ton
navigateur, sans installer .NET, sans configurer une base, sans déployer
quoi que ce soit ailleurs que sur ta machine."

## 3. Scope IN — ce qui doit être livré

> **v1.0 = dogfooding strict.** Le repo cible est codé en dur sur
> `AskmethatFR/agent-dashboard` (le dashboard observe la team agentique
> qui le développe). La paramétrisation du repo cible — et donc l'usage
> OSS générique — est explicitement reportée post-v1 (voir issue #29
> sur le contrat multi-repo).

- **Polling GitHub Issues** du repo dogfooding (hardcodé v1.0), toutes
  les N secondes (défaut **600s / 10 min**), pour récupérer les tickets
  actifs et leur état
- **Bouton de refresh manuel** dans la topbar, toujours dispo,
  qui force un poll immédiat hors planning
- **Persistance SQLite** (single file, dans un volume monté)
- **Page Team Board** servie en Blazor Server, affichant les tickets
  groupés par statut (7 colonnes selon la state machine)
- **Configuration data-driven** (YAML monté + variables d'env)
- **Image Docker unique** publiée sur GHCR, lançable par n'importe qui
- **Design system minimal** (IBM Plex + tokens dark) — fidèle au mock
  `design/Team Board.html`

## 4. Scope OUT — explicitement reporté

> Si le PM agent voit une ambiguïté, ces points doivent rester **OUT** du MVP.

| Hors scope | Reporté à |
|---|---|
| Sessions index, Session Replay | v1.1 |
| Agent profile view | v1.1 |
| Flow Analytics | v1.2 |
| Home dashboard | v1.0 final |
| Escalations Inbox | v1.1 |
| Transcript ingestion (`.jsonl`) | v1.1 (avec Sessions) |
| Webhook GitHub entrant | jamais (polling = choix archi) |
| Authentification utilisateur | v2.0 si besoin |
| Multi-repo / multi-tenant | post v1 (issue #29) |
| Paramétrisation du repo cible (OSS générique, tout repo) | post v1 (issue #29) |
| Export, recherche, filtres avancés | v1.0 final |
| Notifications (mail, push) | v2.0 |
| Persistance Postgres | non — SQLite est le choix MVP |

## 5. Epics fonctionnelles à découper

> Le PM agent doit transformer chaque epic en N tickets (1 vertical
> slice = 1 ticket), conformément au PROTOCOL.

### EPIC-1 — Ingestion GitHub des tickets

**But :** Pouvoir refléter en SQLite l'état réel des Issues du repo configuré.

**Acceptance criteria à refiner :**
- Un worker en arrière-plan poll l'API GitHub à un intervalle configurable
  (**défaut 600s / 10 min, min 300s / 5 min**). À cette fréquence le quota
  GitHub n'est plus une contrainte (~6 req/h sur 5000/h disponibles).
- Le worker peut aussi être déclenché à la demande (refresh manuel
  depuis l'UI) — un poll out-of-band ne réinitialise PAS la planification
- Chaque Issue ouverte du repo configuré devient un `Ticket` côté SQLite
- L'état du ticket dérive du label GitHub `status:*` (mapping configurable)
- Le compteur de retry dérive d'un label `retry:N` ou d'un champ équivalent
  (à décider en spec)
- L'agent attribué au ticket dérive d'un label `agent:*` ou des assignees
  (à décider en spec)
- Les tickets fermés disparaissent du board (sauf statut Done récent)
- Le poller respecte le rate-limit GitHub (ETag conditional requests)
- Échec API → retry exponentiel, pas de crash du worker

**Contraintes techniques :**
- `Octokit` (déjà déclaré dans `Directory.Packages.props`)
- Schéma SQLite write model + read model projection pour le board
- Worker = `BackgroundService` ASP.NET Core

**Hors scope de l'epic :** ingestion des comments, attachements,
historique au-delà du current state.

---

### EPIC-2 — Page Team Board

**But :** Afficher visuellement les tickets en kanban, fidèle au mock.

**Acceptance criteria à refiner :**
- Route `/` rend la page Team Board (page d'accueil pour le MVP)
- 7 colonnes selon `TicketStatus` : Created, Specified, In Development,
  In Review, In Qa, Awaiting Validation, Done (today)
- Chaque ticket = une carte avec : `#id`, titre, agent (glyph + name),
  retry counter (`⟳ N/3`), badge âge, badges visuels (fresh / stale /
  escalated / cross-review / thinking)
- Code couleur cohérent avec le mock :
  - Cross-review actif : badge `⇄`
  - Retry 2/3 : warn (orange)
  - Retry 3/3 : danger (rouge)
  - Stale (> seuil configurable, défaut 3h) : indicateur `zZz`
  - Escalated : badge `◆ esc → <agent cible>`
- Top bar avec brand `team/`, navigation (autres écrans = liens vers
  pages placeholder "coming soon" pour v1.1+), horloge UTC live,
  **et un bouton de refresh manuel** qui déclenche un poll immédiat
  via l'ingestion (EPIC-1) et rafraîchit le board
- Design system : IBM Plex Sans + Mono, dark mode, tokens portés depuis
  `design/styles.css`
- Mise à jour live : quand l'ingestion détecte un changement, la page
  est rafraîchie côté client (mécanisme à choisir en spec : circuit
  Blazor Server natif via état partagé + `StateHasChanged`, ou
  SignalR hub dédié)

**Contraintes techniques :**
- Composants Razor à créer (au minimum) :
  `TopBar`, `LiveClock`, `BoardColumn`, `TicketCard`, `AgentChip`,
  `RetryCounter`, `AgeBadge`
- Référence visuelle stricte : `design/Team Board.html` + `design/TeamBoard.jsx`
  + `design/styles.css`

**Hors scope de l'epic :** hover preview, drag & drop, filtres, recherche,
détail ticket (la carte n'est pas cliquable au-delà d'un lien vers
l'Issue GitHub correspondante).

---

### EPIC-3 — Configuration data-driven

**But :** L'image Docker doit pouvoir être lancée par n'importe qui
sans recompilation.

**Acceptance criteria à refiner :**
- Fichier `agent-roster.yml` monté dans le container définit :
  - Liste des agents (id, name, glyph, role) — défaut = 6 agents PROTOCOL
  - Mapping `label GitHub → TicketStatus`
  - Mapping `label GitHub → AgentId` (qui possède le ticket)
  - Seuil "stale" en heures
- Variables d'environnement :
  - `GITHUB_TOKEN` (obligatoire, fail-fast si absent)
  - `POLL_INTERVAL_SECONDS` (optionnel, défaut 600, min 300)
  - `DASHBOARD_PORT` (optionnel, défaut 8080)
  - `DATA_PATH` (optionnel, défaut `/data`)
- Le repo cible est **hardcodé** sur `AskmethatFR/agent-dashboard`
  pour v1.0 (dogfooding strict, cf §3). Aucun env var `GITHUB_REPO`
  n'est lu ni accepté. La paramétrisation est reportée post-v1 (#29).
- Validation des configs au démarrage avec messages d'erreur clairs
- Exemple `agent-roster.example.yml` commit au repo
- I18n EN + FR dès le MVP via `AspNetCore.Localizer.Json` (au minimum
  les labels du board)

**Contraintes techniques :**
- `Microsoft.Extensions.Options` pour les configs typées
- `IValidateOptions<T>` pour validation au démarrage
- `AspNetCore.Localizer.Json` (déjà câblé)

**Hors scope :** hot reload de la config, UI de config, multi-repo,
paramétrisation du repo cible (hardcodé v1.0, voir #29).

---

### EPIC-4 — Distribution Docker

**But :** `docker run` et voir le board.

**Acceptance criteria à refiner :**
- Un `Dockerfile` multi-stage (build → runtime)
- Base runtime : `mcr.microsoft.com/dotnet/aspnet:10.0-alpine`
- Image finale < 250 MB (Blazor Server impose le runtime ASP.NET)
- Support `linux/amd64` ET `linux/arm64` (build multi-arch)
- Workflow GitHub Actions publie sur GHCR à chaque tag `v*`
- README contient la commande exacte de lancement :
  ```bash
  docker run -p 8080:8080 \
    -v $(pwd)/agent-roster.yml:/config/agent-roster.yml:ro \
    -v $(pwd)/data:/data \
    -e GITHUB_TOKEN=ghp_xxx \
    ghcr.io/askmethatfr/agent-dashboard:0.1
  ```
  Le repo cible est hardcodé sur `AskmethatFR/agent-dashboard` pour
  v1.0 (dogfooding). La paramétrisation arrive post-v1 (issue #29).
- `docker-compose.yml` d'exemple à la racine
- Healthcheck HTTP intégré à l'image

**Contraintes techniques :**
- `.dockerignore` pour build rapide
- Pas de secrets dans l'image
- User non-root dans le container

**Hors scope :** Helm chart, déploiement Kubernetes, signature des images,
SBOM (peut être ajouté en v1.0 final).

## 6. Non-functional requirements

| Catégorie | Exigence |
|---|---|
| **Démarrage** | Container "ready" en < 5s sur machine moyenne |
| **Empreinte** | < 200 MB RAM en idle |
| **Latence** | First render Team Board < 500ms après nav |
| **Robustesse** | Polling GitHub doit tolérer 24h sans crash |
| **Sécurité** | `GITHUB_TOKEN` jamais loggé, jamais retourné par l'UI |
| **Accessibilité** | Lisible en zoom 150%, contrastes WCAG AA |
| **Observabilité** | Logs structurés (JSON via `Serilog` ou `ILogger` natif) |

## 7. Contraintes techniques transverses

- Stack figée : **.NET 10, Blazor Server (Interactive Server), SQLite**
- **Pas de Postgres**, pas de Redis, pas de RabbitMQ
- Respect des layers Clean Architecture (structure déjà en place)
- TDD attendu côté Domain + Application (Chicago school, cf
  `tests/general.md` + `tests/chicago-school.md`)
- Tests d'intégration sur SQLite réelle (pas de mock de la base)
- Commits en format Conventional Commits
- ADR à produire pour les décisions structurantes (cf
  `documentation/adr.md`)

## 8. Definition of Done (rappel)

Chaque ticket suit `skills/dod-team.md` :
- Code review croisée passée (DevA ↔ DevB)
- QA review passée
- Security review passée (même si rapide pour MVP)
- Tests verts en CI
- ADR si décision structurante
- Documentation user (README) à jour si surface changée

## 9. Questions ouvertes (à arbitrer en refinement)

> Le PM doit lever ces ambiguïtés AVANT d'envoyer le spec à l'Architect.

1. **Convention de labels GitHub** : on impose un format (`status:in-dev`,
   `retry:2`, `agent:devA`) ou on documente plusieurs conventions
   acceptables ?
2. **Détection cross-review** : label `cross-review:active` ? Assignee
   multiple ? Pattern dans le titre ?
3. **Tickets "Done today"** : on filtre par date de fermeture (≤ 24h) ?
   On affiche les N derniers fermés ?
4. **Mécanisme de live refresh** : push serveur natif Blazor (état
   partagé + `StateHasChanged`) ou hub SignalR dédié — choix Architect.
5. **GitHub rate-limit** : 5000 req/h authentifié → suffisant pour
   polling 60s. Quel comportement si on est limité par instance/org ?
6. **Première fenêtre temporelle d'historique** : on remonte combien de
   tickets fermés au démarrage ? (proposition : 7 jours, configurable)
7. **Format du retry counter dans GitHub** : label `retry:2/3` ?
   commentaire structuré ? checkbox dans le body ?

## 9bis. Label convention

The full GitHub label taxonomy used by the agentic team (status, agent, retry,
epic, size, type) and the PM's arbitration of the open questions above are
documented in [`docs/labels.md`](./labels.md). That document is the contract the
EPIC-1 poller and the EPIC-2 board both rely on — change it via ADR only.

## 10. Acceptance globale du MVP

Le MVP est **DONE** quand :
- [ ] L'auteur lance le bloc `docker run` du README (en remplaçant
      `ghp_xxx` par son PAT), et voit le board du repo dogfooding
      `AskmethatFR/agent-dashboard` sous 2 minutes
      (l'usage par un tiers sur son propre repo arrive post-v1, #29)
- [ ] Le board reflète l'état réel et se met à jour seul
- [ ] L'image est publiée sur `ghcr.io/askmethatfr/agent-dashboard:0.1`
- [ ] Un contributeur OSS peut cloner, `dotnet run`, et travailler en local
- [ ] La CI passe sur main (build + tests)
- [ ] Le README explique : ce que c'est, comment lancer, comment
      contribuer, status (early WIP / MVP)
