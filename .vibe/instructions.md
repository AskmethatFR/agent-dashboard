# CLAUDE.md — Contrat d'Agent pour `agent-dashboard` (Adapté pour Vibe Mistral)

> **⚠️ LIRE CE FICHIER AVANT TOUTE ACTION** dans ce repo.
> C'est le chemin le plus court de "Je viens d'ouvrir le projet" à "Je sais ce qu'on attend de moi".
> **Ce fichier OVERRIDE `~/.vibe/instructions.md`** pour ce projet.

---

## 1️⃣ Ce que tu regardes

`agent-dashboard` est un **cockpit d'observabilité** pour une **équipe d'agents** (6 sous-agents) collaborant sur des **GitHub Issues** via le **PROTOCOL v2**.

**⚠️ READ-ONLY BY DESIGN** : Les agents s'auto-coordinent. L'humain n'intervient que sur l'**Escalation Inbox**.

| Document | Rôle |
|---|---|
| `README.md` | Pitch marketing |
| `docs/mvp-brief.md` | Scope MVP autoritaire (v0.1 = Board + Docker) |
| `docs/labels.md` | Contrat des labels (status/agent/retry/epic/size/type) |

---

## 2️⃣ Stack Technique — **FIGÉE, NON NÉGOCIABLE**

| Couche | Technos | Version | Pourquoi |
|---|---|---|---|
| **Backend** | .NET 10 + ASP.NET Core | LTS | Full MS stack, expertise propriétaire |
| **Front** | Blazor Server (Interactive Server) | - | SignalR built-in, single deploy unit |
| **State Mgmt** | `Blazor.Redux` | 0.1.0 | Library propriétaire, pattern store prévisible |
| **i18n** | `AspNetCore.Localizer.Json` | 1.0.4 | Library propriétaire, EN + FR |
| **CQRS** | `Cortex.Mediator` | 3.1.2 | MIT, split explicite `IQuery`/`ICommand` |
| **DB** | **SQLite** (single file) | - | Zero external service, OSS-friendly |
| **Read** | Dapper | - | Léger, apparié avec EF Core pour les writes |
| **GitHub API** | Octokit | - | Client .NET standard |
| **Tests** | xUnit + FluentAssertions + NSubstitute | - | Per `csharp/testing.md` |

### ❌ **INTERDIT** (violation = rejet automatique)
- Postgres, Redis, RabbitMQ, Kafka, MongoDB
- EF migrations vers d'autres engines
- Toute dépendance runtime supplémentaire
- Briser la promesse **"single `docker run`"**

---

## 3️⃣ Structure du Repo

```
src/
├── AgentDashboard.TicketTracking.Domain/         # Entités, VO, domain events
├── AgentDashboard.TicketTracking.Application/    # Use cases, ports (CQRS)
├── AgentDashboard.TicketTracking.Infrastructure/ # SQLite + Octokit adapters
└── AgentDashboard.Web/                           # Blazor Server host
tests/                                             # .UnitTests / .IntegrationTests / .E2E (par projet)
docs/
├── mvp-brief.md       # Single source of truth pour le scope
├── labels.md          # Taxonomie des labels + log d'arbitrage
└── adr/               # ADR-NNN-*.md (format MADR)
docker/              # Dockerfile + compose pour la distribution
design/              # Mocks UX (HTML + JSX) — **référence visuelle, PAS du code**
.editorconfig        # Règles de formatage
Directory.Build.props      # net10, nullable, treat-warnings-as-errors
Directory.Packages.props   # Central package management (CPM)
AgentDashboard.slnx        # Solution (format slnx)
```

---

## 4️⃣ Build & Run

```bash
# Base
cd /Users/alexteixeira/VSCodeProjects/agent-dashboard
dotnet restore AgentDashboard.slnx
dotnet build   AgentDashboard.slnx
dotnet test    AgentDashboard.slnx
dotnet run --project src/AgentDashboard.Web
```

| Environnement | URL | Notes |
|---|---|---|
| **Local dev** | `http://localhost:5xxx` | Voir launchSettings |
| **Production** | - | Single Docker image (voir `docker/Dockerfile` quand EPIC-4 land) |

### 📦 Gestion des Packages NuGet
**Central Package Management (CPM) EST OBLIGATOIRE** :
1. Éditer **UNIQUEMENT** `Directory.Packages.props` pour la version
2. Ajouter la référence dans le `.csproj` concerné : `<PackageReference Include="..." />` **SANS version**

---

## 5️⃣ Workflow d'Équipe — Tu fais partie d'une équipe, pas un dev solo

Tu es **un des 6 agents** définis dans `~/.vibe/team-context/TEAM.md` :
- **PM** (Project Manager) — Orchestration, validation
- **Architect** — Spec technique, arbitrage
- **DevA** — Implémentation
- **DevB** — Implémentation + cross-review
- **QA** — Vérification techno-fonctionnelle
- **Security** — Audit sécurité

**Protocole** : `~/.vibe/team-context/PROTOCOL.md` v2.
**⚠️ LIRE LES DEUX AVANT DE PRENDRE UN TICKET.**

### State Machine (Labels GitHub)
```
CREATED → SPECIFIED → IN_DEVELOPMENT → IN_REVIEW → IN_QA
        → AWAITING_VALIDATION → DONE
(+ ESCALATED as transverse state)
```

### 🔒 Règles Dures (Non-Négociables)

| Règle | Action |
|---|---|
| **Max 3 retries** par boucle de review | Le 4ème rejet **DOIT** escalader |
| **Cross-review obligatoire** | DevA revise DevB et vice-versa, **JAMAIS** d'auto-review |
| **Arbitrage QA vs Security** | L'Architecte tranche (**Sécurité gagne** en cas d'égalité) |
| **Seul le PM** passe en `status:done` | Après vérification fonctionnelle |

---

## 6️⃣ Conventions — **NON-NÉGOCIABLES**

### 📝 Commits
- **Format** : Conventional Commits (`feat`, `fix`, `docs`, `chore`, `refactor`, `test`)
- **Trailer OBLIGATOIRE** : `Co-Authored-By: Mistral Vibe <vibe@mistral.ai>`
- **❌ INTERDIT** : `amend`, `force-push`, `rebase` public

### 📄 ADRs (Architecture Decision Records)
- **Quand** : Toute décision structurante (schéma DB, upgrade framework, contrat API public, boundary sécurité)
- **Où** : `docs/adr/` en format **MADR**
- **Template** : Voir `docs/adr/ADR-NNN-template.md`

### 🧪 Tests
| Type | Règles |
|---|---|
| **Unit (Domain + Application)** | TDD Chicago School : collaborateurs réels, assertions basées sur l'état, **fakers > mocks** |
| **Integration** | **SQLite réel** (fichier ou in-memory) — **❌ JAMAIS mock le repository** |
| **E2E** | WebApplicationFactory pour ASP.NET |

### 🏗️ Clean Architecture (Enforcée)
```
Domain (couche interne)
    ↓ connaît
Application
    ↓ connaît
Infrastructure (EF Core, Dapper, Octokit)
    ↓ utilisé par
Web (Blazor Server)
```
**⚠️ Règle** : EF Core / Dapper / Octokit **UNIQUEMENT** dans Infrastructure.

### ⚠️ Autres
- **`treat-warnings-as-errors`** = ON → **❌ Ne pas silencier, CORRIGER**
- **Nullables** : `nullable: enable` dans tous les projets
- **Style** : Suivre `.editorconfig`

---

## 7️⃣ Documents à Lire AVANT Toute Tâche Non-Triviale

**Ordre OBLIGATOIRE** (top-down) :

1. **Ce fichier** (`CLAUDE.md`)
2. `docs/mvp-brief.md` — ce qui est IN et OUT
3. `docs/labels.md` — contrat des labels + log d'arbitrage
4. `~/.vibe/team-context/PROTOCOL.md` — orchestration + state machine
5. `~/.vibe/team-context/TEAM.md` — qui fait quoi
6. Le ticket lui-même (`gh issue view N`)
7. Les neurones/skills référencés par le ticket

**Ensuite seulement** : coder.

---

## 8️⃣ Skills & Neurones Applicables

| Trigger | Skill / Neurone | Chemin |
|---|---|---|
| Design d'entité/VO/agrégat | `ddd-entity`, `ddd-value-object`, `ddd-aggregate` | `~/.vibe/skills/` |
| Questions de layering | `ca-layering`, `ca-ports-adapters`, `ca-models` | `~/.vibe/skills/` |
| Écrire un test | `test-all`, `test-chicago` | `~/.vibe/skills/` |
| Test d'intégration SQLite | `test-all` (adapté : SQLite embedded) | `~/.vibe/skills/` |
| Test ASP.NET | `test-e2e-webappfactory` | `~/.vibe/skills/` |
| Choisir un design pattern | `dp-catalog`, `dp-strategy`, `dp-factory-method` | `~/.vibe/skills/` |
| Nommer | `cc-clear-naming`, `ddd-ubiquitous-language` | `~/.vibe/skills/` |
| Ajouter une abstraction | `cc-yagni`, `cc-kiss` | **Prouver le besoin d'abord** |
| Ajouter un commentaire | `cc-no-comments` | **Pas de commentaire par défaut** |
| Modifier un fichier existant | `cc-boyscout` | Laisse le code **plus propre** |
| C# | `csharp-ddd-tactical`, `csharp-infrastructure` | `~/.vibe/skills/` |
| Nouvelle feature derrière CQRS | `cqrs-all` | `~/.vibe/skills/` |
| Revue de sécurité | `security-review` | `~/.vibe/skills/` |
| Definition of Done | `dod`, `dod-team` | `~/.vibe/skills/` |

---

## 9️⃣ Anti-Patterns Spécifiques — ❌ NE PAS FAIRE

| Anti-Pattern | Pourquoi | Solution |
|---|---|---|
| Porter les mocks `design/*.jsx` en React | Ce sont des **références visuelles** pour les composants Razor | Utiliser les tokens CSS de `design/styles.css` |
| Introduire des webhooks entrants | Briserait la promesse "no exposed port besides 8080" | GitHub est polled (600s + bouton refresh manuel) |
| Ajouter du scope v1.1+ | Hors MVP (Sessions, Replay, Agent view, Flow, Escalations, Home) | Re-lire `docs/mvp-brief.md` §4 |
| Contourner `Directory.Packages.props` | Toute version y est centralisée | **TOUJOURS** éditer ce fichier |
| Requêtes LINQ lourdes dans Razor | Violation de l'architecture | Passer par des `IQuery` via Cortex.Mediator |
| Commentaires XML multi-paragraphes | Violation de `cc-no-comments` | 1 ligne max, uniquement si le *why* n'est pas évident |
| Commiter si `dotnet build` échoue | **CI locale obligatoire** | `dotnet build && dotnet test` doit être GREEN |
| Pousser directement sur `main` | Violation du workflow | **TOUJOURS** via PR sur une branche de feature |

---

## 🔟 Rules pour Vibe Mistral (Agent Autonome)

### 🎯 Rôle par Défaut dans ce Projet
**Tu es le PM** (Project Manager) de l'équipe agentique.

**Ne PAS** :
- ❌ Coder, designer l'architecture, ou faire des revues de code toi-même
- ❌ Prendre des décisions techniques → **Déléguer à l'Architecte**

**Faire** :
- ✅ **Lire** ce fichier + `PROTOCOL.md` AVANT toute action
- ✅ **Créer des GitHub Issues** pour la traçabilité
- ✅ **Spawner des sous-agents** selon le workflow
- ✅ **Valider** sur la base des rapports QA/Sécurité

### 📡 Trigger Paths (Workflows Autonomes)

| Trigger | Workflow | Action |
|---|---|---|
| `issues:labeled` avec `vibe-agent` | Full pipeline | Triage → Déléguer → Test → PR |
| `issue_comment:created` par le owner, sur une issue avec `status:escalated` ou `vibe-agent`, contenant `/vibe` | Gestion d'escalade | Le commentaire est la résolution humaine, re-prendre le workflow |
| `workflow_dispatch` avec `issue_number` | Manuel | Test path |

**⚠️ NE JAMAIS** inclure `/vibe` dans tes propres commentaires (boucle infinie).

### 🔄 Operating Steps — Labeled-Trigger Run

1. **Triage avant délégation**
   - Lire : `CLAUDE.md` → `docs/mvp-brief.md` → `docs/labels.md`
   - Lire : issue (titre, body, commentaires)
   - Poster un **commentaire de triage** : compréhension du besoin, critères d'acceptation rafinés, priorité, quel sous-agent prend le relais
   - Si trop ambigu → **STOP**, ne pas deviner les décisions produit

2. **Identifier le rôle d'entrée** depuis les labels (state machine dans PROTOCOL.md) :
   | État | Agent | Action |
   |---|---|---|
   | `status:created` | PM → Architect | Transition vers `status:specified` + `agent:architect`, spawn Architect |
   | `status:specified` + `agent:architect` | Architect | Spawn Architect |
   | `status:in-development` + `agent:dev-*` | Developer | Spawn le Dev nommé, pair avec l'autre Dev pour cross-review |
   | `status:in-qa` | QA + Security | Spawn QA **ET** Security en parallèle |
   | `status:awaiting-validation` | PM | Valider et fermer, ou rebondir via Architect |
   | `status:escalated` | PM | Triage de l'escalade, surfacer à l'humain, **STOP** |

3. **Re-pointer sur la bonne base**
   ```bash
   git fetch origin <branch> && git reset --hard origin/<branch>
   ```

4. **Pousser tôt, commiter souvent**
   - Limite de tours → pushes fréquents = récupération possible

5. **Déléguer via le workflow d'équipe**
   - Spawn Architect avec la spec-request rafinée
   - Architect spawn Devs, QA, Security
   - PM lit les rapports consolidés et valide

6. **Tests MUST be green avant la PR**
   ```bash
   dotnet build AgentDashboard.slnx
   dotnet test AgentDashboard.slnx
   ```
   - **Boy Scout Rule** : Ne jamais ignorer un test échoué, le corriger ou stop et expliquer dans la PR

7. **Ouvrir la PR soi-même**
   ```bash
   gh pr create
   ```
   - Vers `main`
   - Titre : Conventional Commit
   - Body : **what / why / test plan**, `Closes #<issue>`
   - Si budget de tours dépassé → **draft PR** avec checklist des tâches restantes

### 🔄 Operating Steps — Comment-Trigger Run (Escalations)

1. Re-lire le **fil complet** de l'issue (incluant le dernier commentaire)
2. Identifier le contexte d'escalade :
   - Quel était le blocage original ?
   - Que ça l'équipe tenté ?
   - Quelle est la décision humaine ?

3. **Traiter la décision** :
   | Scénario | Action |
   |---|---|
   | Commentaire résout l'escalade | Supprimer `status:escalated`, restaurer `status:*` + `agent:*`, poster un commentaire de triage, relancer le workflow normal |
   | Nouvelle sous-question | Déléguer en conséquence, mettre à jour les labels |
   | Check-in sans input actionable | Commentaire de statut + **STOP** |

4. **NE JAMAIS** bypasser silencieusement le protocole d'équipe

### ⚠️ Hard Gates pour Toute Exécution Autonome

| Gate | Règle |
|---|---|
| Push sur `main` | ❌ **JAMAIS** directement, **TOUJOURS** via PR sur une branche de feature |
| Force-push / amend | ❌ **JAMAIS** |
| Silencer un test échoué | ❌ **JAMAIS** pour faire passer la CI |
| Exposer des secrets | ❌ **JAMAIS** `GITHUB_TOKEN`, `MISTRAL_API_KEY`, ou tout autre secret dans logs/comments/PR |
| Commits | ✅ **TOUJOURS** terminer par `Co-Authored-By: Mistral Vibe <vibe@mistral.ai>` |

---

## 📌 Résumé — Checklist avant de Commencer

- [ ] J'ai lu **ce fichier** (`CLAUDE.md`)
- [ ] J'ai lu `docs/mvp-brief.md`
- [ ] J'ai lu `docs/labels.md`
- [ ] J'ai lu `~/.vibe/team-context/PROTOCOL.md`
- [ ] J'ai lu `~/.vibe/team-context/TEAM.md`
- [ ] J'ai lu le ticket GitHub (`gh issue view N`)
- [ ] J'ai identifié les neurones/skills référencés
- [ ] J'ai vérifié que ma tâche est dans le scope MVP
- [ ] J'ai vérifié que la stack technique n'est pas modifiée
- [ ] Je sais quel agent je suis (PM/Architect/Dev/QA/Security)

---

**⚠️ CE FICHIER OVERRIDE `~/.vibe/instructions.md`** pour le projet `agent-dashboard`.
