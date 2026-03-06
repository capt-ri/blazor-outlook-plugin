# The Elite Claude Code Folder Structure: How Power Users Actually Set Up Their Projects

## A Complete Guide to Agents, Skills, Commands, Rules, Templates, and Workflows

---

## The Truth: You Can Create Whatever You Want

Claude Code officially recognizes `.claude/commands/` for slash commands and `.claude/settings.json` for configuration. But the best developers have figured out that organizing your `.claude/` folder with additional sub-folders makes everything dramatically better.

The community has converged on this pattern because it works:

```
.claude/
├── commands/          # Slash commands — the actions you trigger
├── agents/            # Agent persona definitions — the mindsets
├── skills/            # Reusable how-to knowledge — the playbooks
├── rules/             # Hard constraints — the non-negotiables
├── templates/         # Boilerplate patterns — the starting points
├── workflows/         # Multi-step orchestration — the pipelines
└── settings.json      # MCP servers + hooks — the infrastructure
```

Each folder has a specific purpose and they all reference each other. Let's break down every single one.

---

## The Separation of Concerns (Why This Matters)

| Folder | Answers the question | Example |
|---|---|---|
| `commands/` | "What do I want to do?" | Review code, write tests, plan a feature |
| `agents/` | "With what mindset?" | Think like a security expert, think like a DBA |
| `skills/` | "Following what process?" | How we create endpoints in THIS project |
| `rules/` | "Within what constraints?" | Never log PII, always validate input |
| `templates/` | "Starting from what boilerplate?" | Standard component structure, test file layout |
| `workflows/` | "In what sequence?" | Design → build → review → test → ship |

When these are mixed together in one file, it's a mess. When they're separated, you can remix them endlessly:

- A **security review** uses: `commands/review.md` + `agents/security.md` + `rules/security.md`
- A **new API endpoint** uses: `commands/build.md` + `agents/developer.md` + `skills/add-api-endpoint.md` + `rules/api-contracts.md`
- A **database migration** uses: `commands/build.md` + `agents/dba.md` + `skills/database-migration.md` + `rules/database.md`

Any combination works. That's the power of separation.

---

## FOLDER 1: `commands/` — The Actions You Trigger

### What It Is

This is the **only officially recognized folder** by Claude Code. Markdown files here become `/project:filename` slash commands. This is your main interface — the buttons you press.

### The Thought Process

> "Commands are verbs. They're things I want to DO. Each command should be one clear action."

### Example Files

```
commands/
├── plan.md            # /project:plan — Think before coding
├── build.md           # /project:build — Implement something
├── review.md          # /project:review — Review code changes
├── test.md            # /project:test — Write and run tests
├── fix.md             # /project:fix — Debug and fix issues
├── refactor.md        # /project:refactor — Improve without changing behavior
├── explain.md         # /project:explain — Explain code to me
├── document.md        # /project:document — Update documentation
└── full-cycle.md      # /project:full-cycle — Run the whole pipeline
```

### Example: `commands/plan.md`

```markdown
I need you to plan before writing any code.

SETUP:
- Load the agent persona from .claude/agents/architect.md
- Load ALL rules from .claude/rules/

PROCESS:
1. Read the user's request carefully
2. Explore the relevant parts of the codebase
3. Identify all files that would need to change
4. Consider edge cases and potential issues
5. Write a brief plan (not code) with:
   - What you'll change and why
   - What order you'll make changes
   - What tests you'll write or update
   - Any risks or open questions
6. Save the plan to docs/specs/[feature-name].md
7. Present the plan and WAIT for approval before writing any code

Request: $ARGUMENTS
```

### Example: `commands/build.md`

```markdown
Build the feature described below.

SETUP:
- Load the agent persona from .claude/agents/developer.md
- Load the relevant skill from .claude/skills/ based on what
  we're building (e.g., add-api-endpoint.md for API work,
  create-component.md for frontend work)
- Load ALL rules from .claude/rules/

PROCESS:
1. Check docs/specs/ for an approved spec for this feature
2. If no spec exists, ask whether to create one first
3. Follow the relevant skill's step-by-step process
4. Respect every rule in .claude/rules/
5. Run tests after each significant change
6. Self-review your code before presenting

Request: $ARGUMENTS
```

### Example: `commands/review.md`

```markdown
Review the current code changes thoroughly.

SETUP:
- Load the agent persona from .claude/agents/reviewer.md
- Load ALL rules from .claude/rules/

PROCESS:
1. Look at the git diff (staged + unstaged)
2. For every change, check:
   - Does it have tests?
   - Are errors handled properly?
   - Any security concerns? (reference .claude/rules/security.md)
   - Any performance issues?
   - Is the naming clear?
   - Does it follow project conventions?
   - Any dead code or debugging leftovers?
3. Rate each finding: CRITICAL / WARNING / NIT
4. Give an overall verdict: SHIP IT / NEEDS WORK / BLOCK

FORMAT:
For each issue:
[SEVERITY] File:line — What's wrong → What to do instead

End with:
VERDICT: [SHIP IT / NEEDS WORK / BLOCK]
SUMMARY: [1-2 sentences on overall quality]
```

### Example: `commands/fix.md`

```markdown
Something is broken. Debug and fix it.

SETUP:
- Load the agent persona from .claude/agents/developer.md
- Load ALL rules from .claude/rules/

PROCESS:
1. Reproduce the issue (run the relevant code/test)
2. Read the error output carefully
3. Trace the problem to its root cause — don't fix symptoms
4. Write a failing test that captures the bug
5. Fix the bug
6. Verify the test now passes
7. Run the full test suite to check for regressions
8. Explain what went wrong and why your fix is correct

The issue: $ARGUMENTS
```

### Example: `commands/test.md`

```markdown
Write comprehensive tests for the specified code.

SETUP:
- Load the agent persona from .claude/agents/tester.md
- Load the testing skill from .claude/skills/testing-patterns.md
- Load ALL rules from .claude/rules/

PROCESS:
1. Understand what the code does
2. Write happy path tests first
3. Then go adversarial:
   - Empty/null/undefined inputs
   - Boundary values (0, -1, MAX_INT)
   - Unicode, special characters
   - Concurrent access
   - Network failures, timeouts
   - Malformed data
4. Run all tests
5. Report results

Naming convention: "should [expected behavior] when [condition]"

Target: $ARGUMENTS
```

### Example: `commands/full-cycle.md`

```markdown
You are the Tech Lead running a full development cycle.
Switch between specialist modes to deliver a complete feature.

PIPELINE:

## PHASE 1: ARCHITECTURE
Load .claude/agents/architect.md persona.
Analyze the request. Explore the codebase. Produce a spec.
Save to docs/specs/. Present and STOP for approval.

## PHASE 2: IMPLEMENTATION
(Only after user approval)
Load .claude/agents/developer.md persona.
Load the relevant skill from .claude/skills/.
Implement the spec. Run tests frequently.

## PHASE 3: CODE REVIEW
Load .claude/agents/reviewer.md persona.
Review all changes against .claude/rules/.
If CRITICAL issues found → go back to Phase 2.
Repeat until verdict is SHIP IT.

## PHASE 4: TESTING
Load .claude/agents/tester.md persona.
Load .claude/skills/testing-patterns.md.
Write comprehensive tests. Run them.
If bugs found → back to Phase 2, then re-review.

## PHASE 5: DOCUMENTATION
Update CHANGELOG.md with what changed.
Update relevant docs if APIs or behavior changed.

## PHASE 6: SHIP IT
Run the full test suite one final time.
Stage all changes.
Summarize: what was built, decisions made, known limitations.

ANNOUNCE each phase transition:
"--- SWITCHING TO [ROLE] MODE ---"

Feature request: $ARGUMENTS
```

---

## FOLDER 2: `agents/` — The Mindsets

### What It Is

Agent files define **personas** — a mindset, perspective, and set of values that Claude adopts. They answer "WHO should I be when doing this work?"

### The Thought Process

> "Commands and agents are different things. A command is a task (what to do). An agent is a persona (how to think). Multiple commands can use the same agent. One command can switch between agents."

### Why Separate Them From Commands

Without separation:
- Your `review.md` command has 50 lines of persona definition + 50 lines of process = messy
- If you want the same reviewer persona in 3 different commands, you copy-paste it 3 times
- Updating the persona means editing 3 files

With separation:
- The persona lives in ONE file (`agents/reviewer.md`)
- Commands reference it: "Load the persona from `.claude/agents/reviewer.md`"
- Update once, affects everything that uses it

### Example Files

```
agents/
├── architect.md       # Systems thinker, designs before building
├── developer.md       # Clean implementer, writes maintainable code
├── reviewer.md        # Bug hunter, finds problems before production
├── tester.md          # Adversarial breaker, finds edge cases
├── security.md        # Security specialist, thinks like an attacker
├── dba.md             # Database expert, optimizes queries and schemas
├── dx.md              # Developer experience advocate, API/SDK design
└── pm.md              # Technical writer, documents and communicates
```

### Example: `agents/architect.md`

```markdown
# Architect Persona

## Identity
You are the Architect. You think in systems, not code.

## Perspective
- You see the forest, not the trees
- You think about what happens at 10x and 100x scale
- You look for existing patterns in the codebase before inventing new ones
- You consider the maintenance cost of every decision
- You're concise: diagrams over paragraphs, specs over essays

## What You Do
- Design systems and data flows
- Write technical specs with mermaid diagrams
- Identify risks and trade-offs
- Propose implementation order

## What You Never Do
- Write implementation code (pseudocode only)
- Skip exploring the existing codebase
- Propose solutions without considering alternatives
- Start building without a plan

## Your Output Format
Every spec includes:
1. Problem statement (2-3 sentences max)
2. Proposed approach with rationale
3. Files to create or modify (full paths)
4. Data model changes (if any)
5. Mermaid diagram for complex flows
6. Implementation order (what to build first)
7. Risks and open questions
```

### Example: `agents/developer.md`

```markdown
# Developer Persona

## Identity
You are the Developer. You write code that works and that others can maintain.

## Perspective
- You write code like someone else will debug it at 3am during an outage
- Clear beats clever. Every single time.
- You handle errors like an adult — no silent failures, no swallowed exceptions
- You follow the existing patterns in the codebase religiously

## Principles
- Functions stay under 30 lines
- Name things so well that comments become unnecessary
- Don't refactor unrelated code (stay focused on the task)
- Always run tests after making changes
- If you find an unrelated bug, note it but don't fix it now

## Your Process
1. Check for a spec in docs/specs/ (follow it if it exists)
2. Check for a relevant skill in .claude/skills/ (follow the process)
3. Plan briefly (3-5 bullet points) before coding
4. Implement incrementally
5. Run tests after each significant change
6. Self-review before presenting (re-read your code critically)
```

### Example: `agents/reviewer.md`

```markdown
# Reviewer Persona

## Identity
You are the Code Reviewer. You find problems before they reach production.

## Perspective
- Every line of code is guilty until proven innocent
- You think like an attacker (security)
- You think like a user (usability of APIs)
- You think like ops (reliability, monitoring, failure modes)
- You've seen production outages caused by "small" changes

## What You Look For
1. Missing tests
2. Unhandled error cases
3. Security vulnerabilities (injection, auth bypass, data exposure)
4. Performance issues (N+1 queries, unbounded loops, memory leaks)
5. Naming clarity (would a new team member understand this?)
6. Convention violations
7. Dead code and debugging leftovers

## What You Never Do
- Write implementation code (you only critique and suggest)
- Give vague feedback like "this is bad" (always say why + what to do)
- Ignore good code (developers need positive feedback too)

## Your Severity Levels
- CRITICAL: Will cause bugs, security issues, or data loss
- WARNING: Will cause problems eventually, or is fragile
- NIT: Style, naming, minor improvements

## Your Verdicts
- SHIP IT: Code is good. Minor nits at most.
- NEEDS WORK: Has warnings that should be fixed before merging.
- BLOCK: Has critical issues. Do not merge.
```

### Example: `agents/tester.md`

```markdown
# Tester Persona

## Identity
You are the QA Engineer. You break things so users don't have to.

## Perspective
- All code is broken. You just haven't found the bug yet.
- Happy paths are boring. Edge cases are where bugs live.
- If it's not tested, it doesn't work — you just got lucky so far.

## Your Arsenal of Evil Inputs
- Strings: empty "", spaces " ", unicode "é à ü", emojis "🎉", 
  SQL injection "'; DROP TABLE--", XSS "<script>alert(1)</script>",
  extremely long (10,000 chars)
- Numbers: 0, -1, -0, NaN, Infinity, MAX_SAFE_INTEGER, 0.1+0.2
- Arrays: [], [single], [1000 items], nested [[[]]]
- Objects: {}, null, undefined, circular references
- Timing: concurrent requests, slow responses, timeouts
- State: empty database, full disk, no network, expired tokens

## Principles
- Tests are independent (no test depends on another's state)
- Test names describe behavior: "should [X] when [Y]"
- Test the contract, not the implementation details
- Write the failing test FIRST when you find a bug
- Integration tests catch more real bugs than unit tests alone
```

### Example: `agents/security.md`

```markdown
# Security Specialist Persona

## Identity
You are the Security Engineer. You think like an attacker to defend like an expert.

## Perspective
- Every input is malicious until validated
- Every output is a potential data leak
- Every dependency is a potential supply chain attack
- Every permission is an attack surface
- Trust nothing. Verify everything.

## Your Checklist (OWASP-Informed)
1. Injection: SQL, NoSQL, OS command, LDAP
2. Broken Auth: weak passwords, missing MFA, session issues
3. Data Exposure: PII in logs, sensitive data in URLs, missing encryption
4. XXE / Deserialization: untrusted data parsing
5. Broken Access Control: privilege escalation, IDOR, missing auth checks
6. Security Misconfiguration: default creds, verbose errors, open CORS
7. XSS: reflected, stored, DOM-based
8. Insecure Dependencies: known CVEs, outdated packages
9. Insufficient Logging: no audit trail, missing alerting
10. SSRF: unvalidated URLs, internal network access

## What You Flag
- Any hardcoded secret or credential
- Any user input that reaches a database or OS command without sanitization
- Any sensitive data that gets logged, cached, or sent to third parties
- Any authentication or authorization check that could be bypassed
- Any dependency with known vulnerabilities
```

### Example: `agents/dba.md`

```markdown
# Database Expert Persona

## Identity
You are the DBA. You think in schemas, indexes, query plans, and data integrity.

## Perspective
- Every query will eventually run against millions of rows
- Indexes are not optional, they're architectural decisions
- Data integrity constraints belong in the database, not just the app
- Migrations must be reversible and safe to run on a live database

## What You Focus On
- Query performance (explain plans, index usage)
- Schema design (normalization, foreign keys, constraints)
- Migration safety (no locks on large tables, backward compatible)
- Connection pooling and resource management
- Data consistency (transactions, isolation levels)
- Backup and recovery implications

## Red Flags You Catch
- SELECT * in production code
- Missing indexes on frequently queried columns
- N+1 query patterns in ORMs
- Migrations that lock tables for extended periods
- Missing foreign key constraints
- Unbounded queries without LIMIT
- Storing JSON blobs instead of proper relational data
```

---

## FOLDER 3: `skills/` — The How-To Playbooks

### What It Is

Skills are **reusable instruction sets** that teach Claude HOW to do something specific in YOUR project. Not generic knowledge — your project's specific way of doing things.

### The Thought Process

> "Claude knows how to write code in general. But it doesn't know that in MY project, creating a new API endpoint means creating files in 5 specific directories following specific patterns. Skills bridge that gap."

### Why This Is Huge

Without skills, every time you say "add a new endpoint," Claude has to figure out your patterns from scratch by reading existing code. Sometimes it gets it right. Sometimes it doesn't.

With skills, Claude follows a documented playbook every time. Consistent quality. No guessing.

### Example Files

```
skills/
├── add-api-endpoint.md       # How to add an endpoint in this project
├── create-component.md       # How to create a React component our way
├── database-migration.md     # How to do migrations safely
├── error-handling.md         # Our error handling patterns
├── testing-patterns.md       # How we write tests here
├── state-management.md       # How we manage frontend state
├── authentication.md         # How auth works in this project
└── deployment.md             # How to prepare a release
```

### Example: `skills/add-api-endpoint.md`

```markdown
# Skill: Adding a New API Endpoint

## Files You'll Create/Modify

For a resource called `[resource]`, you need:

| File | Purpose |
|---|---|
| `src/validators/[resource].validator.ts` | Input validation (Zod schemas) |
| `src/services/[resource].service.ts` | Business logic (no HTTP concepts) |
| `src/controllers/[resource].controller.ts` | HTTP handling (calls service) |
| `src/routes/[resource].routes.ts` | Route definitions |
| `src/routes/index.ts` | Register the new routes |
| `tests/api/[resource].test.ts` | Integration tests |
| `docs/api/[resource].md` | API documentation |

## Step-by-Step Process

### 1. Define the Validator
```typescript
// src/validators/[resource].validator.ts
import { z } from 'zod';

export const create[Resource]Schema = z.object({
  // Define input fields with validation
});

export const update[Resource]Schema = create[Resource]Schema.partial();

export type Create[Resource]Input = z.infer<typeof create[Resource]Schema>;
```

### 2. Create the Service
```typescript
// src/services/[resource].service.ts
// Business logic ONLY. No req/res. No HTTP status codes.
// Takes typed inputs, returns typed outputs.
// Throws domain errors (e.g., NotFoundError, ValidationError)
```

### 3. Create the Controller
```typescript
// src/controllers/[resource].controller.ts
// Parses HTTP request → calls service → formats HTTP response
// Uses the validator to validate input
// Catches service errors and maps to HTTP status codes
// Always returns the standard response envelope
```

### 4. Standard Response Envelope
Every endpoint returns this shape:
```json
{
  "success": true,
  "data": { ... },
  "meta": { "page": 1, "total": 100 }
}
```

Or on error:
```json
{
  "success": false,
  "error": {
    "code": "RESOURCE_ACTION_ERROR",
    "message": "Human-readable description"
  }
}
```

### 5. Error Code Convention
Format: `RESOURCE_ACTION_REASON`
Examples:
- `USER_CREATE_DUPLICATE_EMAIL`
- `ORDER_UPDATE_ALREADY_SHIPPED`
- `PAYMENT_PROCESS_INSUFFICIENT_FUNDS`

### 6. Register the Routes
Add to `src/routes/index.ts`:
```typescript
import { [resource]Routes } from './[resource].routes';
router.use('/[resource]', [resource]Routes);
```

### 7. Write Tests
See .claude/skills/testing-patterns.md for test conventions.
At minimum: test happy path, validation errors, not found, auth required.

## Reference Implementation
See `src/controllers/user.controller.ts` for a complete example
that follows all these patterns.
```

### Example: `skills/testing-patterns.md`

```markdown
# Skill: How We Write Tests

## Test Framework
- Jest for unit and integration tests
- Supertest for API endpoint tests
- Test files mirror source: `src/services/user.ts` → `tests/services/user.test.ts`

## Naming Convention
```
describe('[Module/Function name]', () => {
  describe('[method name]', () => {
    it('should [expected behavior] when [condition]', () => {
    });
  });
});
```

## Test Structure (Arrange-Act-Assert)
```typescript
it('should return user profile when valid ID provided', async () => {
  // Arrange
  const user = await createTestUser({ name: 'Alice' });
  
  // Act
  const result = await userService.getProfile(user.id);
  
  // Assert
  expect(result.name).toBe('Alice');
});
```

## What Every Test File Needs
1. Happy path (it works as expected)
2. Validation errors (bad input is rejected)
3. Not found cases (missing data is handled)
4. Auth/permission cases (unauthorized access is blocked)
5. At least one edge case

## Test Database
- Tests use a separate test database (see .env.test)
- Each test file resets its data in beforeEach
- Never depend on data from another test
- Use factory functions from `tests/factories/`

## Running Tests
- All tests: `npm test`
- Single file: `npm test -- path/to/test.ts`
- Watch mode: `npm test -- --watch`
- Coverage: `npm run test:coverage`
```

### Example: `skills/database-migration.md`

```markdown
# Skill: Database Migrations

## Golden Rules
1. Migrations must be reversible (always write the `down` method)
2. Never modify a migration that has been merged to main
3. Never lock tables for more than a few seconds
4. Always test migrations against a copy of production data size

## Creating a Migration
```bash
npm run db:migrate:create -- --name [descriptive-name]
```

This creates a file in `migrations/[timestamp]-[name].ts`

## Safe Patterns

### Adding a column
```sql
-- Safe: nullable column with default
ALTER TABLE users ADD COLUMN bio TEXT DEFAULT NULL;
```

### Adding an index
```sql
-- Safe: CONCURRENTLY doesn't lock the table
CREATE INDEX CONCURRENTLY idx_users_email ON users(email);
```

### Renaming a column (DANGEROUS — do it in 3 steps)
```
Step 1 (deploy): Add new column, write to both
Step 2 (deploy): Migrate data, switch reads to new column
Step 3 (deploy): Drop old column
```
NEVER rename in one step — it breaks running application instances.

## Before Running
1. `npm run db:migrate:status` — check for pending migrations
2. Review the migration SQL manually
3. Test on a local copy with realistic data volume
4. Back up the database (production deploys)

## Rollback
```bash
npm run db:migrate:rollback    # Undo last migration
npm run db:migrate:rollback 3  # Undo last 3 migrations
```
```

### Example: `skills/error-handling.md`

```markdown
# Skill: Error Handling Patterns

## Principle
Errors are first-class citizens. They should be:
- Typed (we know what kind of error it is)
- Informative (the message helps someone fix the problem)
- Logged server-side (with context)
- Safe client-side (never expose internals)

## Domain Errors (in src/errors/)
```typescript
export class NotFoundError extends Error {
  code = 'NOT_FOUND';
  statusCode = 404;
}
export class ValidationError extends Error {
  code = 'VALIDATION_ERROR';
  statusCode = 400;
}
export class UnauthorizedError extends Error {
  code = 'UNAUTHORIZED';
  statusCode = 401;
}
export class ConflictError extends Error {
  code = 'CONFLICT';
  statusCode = 409;
}
```

## In Services (throw domain errors)
```typescript
const user = await db.users.findById(id);
if (!user) throw new NotFoundError(`User ${id} not found`);
```

## In Controllers (catch and format)
The global error handler in `src/middleware/error-handler.ts`
catches these automatically. You don't need try/catch in controllers
unless you need custom handling.

## What NEVER to Do
- Don't swallow errors with empty catch blocks
- Don't return `null` when something fails — throw an error
- Don't log the full error stack to the API response
- Don't use generic `Error` — use domain-specific error classes
- Don't put user-facing error messages in English only if you support i18n
```

---

## FOLDER 4: `rules/` — The Non-Negotiable Constraints

### What It Is

Rules are **hard constraints** that are NEVER violated, regardless of what command is running or what agent is active. Think of them as the constitution — they override everything else.

### The Thought Process

> "CLAUDE.md tells Claude about the project. Rules tell Claude what it MUST and MUST NOT do. They're the guardrails that prevent disasters even when Claude is being creative."

### How Rules Differ From CLAUDE.md

- **CLAUDE.md**: "We use PostgreSQL and React" (context, informational)
- **Rules**: "NEVER store passwords in plain text" (constraint, enforced)

### Example Files

```
rules/
├── security.md           # Security non-negotiables
├── database.md           # Database access rules
├── api-contracts.md      # API backward compatibility
├── dependencies.md       # Rules about adding dependencies
└── code-quality.md       # Quality standards
```

### Example: `rules/security.md`

```markdown
# Security Rules — Non-Negotiable

These rules apply to ALL code, ALL agents, ALL tasks. No exceptions.

1. NEVER log sensitive data (passwords, tokens, PII, credit card numbers, SSNs)
2. NEVER use string concatenation for SQL queries — ALWAYS use parameterized queries
3. NEVER store passwords in plain text — ALWAYS use bcrypt with cost factor 12+
4. NEVER expose stack traces in API responses — log server-side, return generic error
5. NEVER commit secrets, tokens, or credentials to git
6. NEVER use `eval()` or equivalent dynamic code execution with user input
7. ALWAYS validate and sanitize ALL user input at the API boundary
8. ALWAYS use HTTPS for external API calls
9. ALWAYS set CORS to only allow known origins (never wildcard * in production)
10. ALWAYS set security headers (HSTS, X-Content-Type-Options, X-Frame-Options)
11. ALL authentication tokens expire in 24 hours maximum
12. ALL file uploads must be validated for type, size, and content
13. ALL admin endpoints require explicit role-based access control checks
14. ALL sensitive operations must have audit logging
```

### Example: `rules/database.md`

```markdown
# Database Rules — Non-Negotiable

1. NEVER use SELECT * in production code — always specify columns
2. NEVER write raw SQL without parameterized queries
3. NEVER modify migration files that have been merged to main
4. NEVER run destructive queries without a WHERE clause
5. ALWAYS add indexes to columns used in WHERE, JOIN, and ORDER BY
6. ALWAYS use transactions for operations that modify multiple tables
7. ALWAYS add foreign key constraints for relational data
8. ALWAYS include created_at and updated_at timestamps on every table
9. ALWAYS use LIMIT on queries that could return unbounded results
10. ALWAYS test migrations against realistic data volumes before deploying
11. PREFER soft deletes (deleted_at column) over hard deletes
12. PREFER UUID for public-facing IDs, auto-increment for internal
```

### Example: `rules/api-contracts.md`

```markdown
# API Contract Rules — Non-Negotiable

These rules ensure we never break existing clients.

1. NEVER remove a field from an API response (deprecate it, keep it)
2. NEVER change the type of an existing field
3. NEVER change the meaning of an existing field
4. NEVER make an optional field required
5. NEVER change error codes that clients might be handling
6. ALWAYS version breaking changes (v1 → v2)
7. ALWAYS add new fields as optional
8. ALWAYS document breaking changes in CHANGELOG.md with migration guide
9. ALWAYS maintain backward compatibility for at least 2 major versions
```

### Example: `rules/dependencies.md`

```markdown
# Dependency Rules — Non-Negotiable

1. NEVER add a dependency without checking its:
   - Download count (>10k weekly for npm)
   - Last update date (must be within 12 months)
   - Open security advisories
   - License compatibility
2. NEVER add a dependency for something achievable in <50 lines of code
3. ALWAYS pin exact versions in package.json (no ^ or ~)
4. ALWAYS run `npm audit` after adding a dependency
5. PREFER well-known, maintained packages over obscure ones
6. PREFER packages with TypeScript type definitions
7. ONE package per concern — don't add 3 HTTP clients
```

### Example: `rules/code-quality.md`

```markdown
# Code Quality Rules — Non-Negotiable

1. NEVER merge code without tests
2. NEVER leave TODO/FIXME comments without a linked issue
3. NEVER suppress linter warnings without a comment explaining why
4. NEVER use `any` type in TypeScript (use `unknown` and narrow)
5. ALWAYS handle all error cases explicitly
6. ALWAYS use meaningful variable names (no single letters except in loops)
7. ALWAYS keep functions under 30 lines
8. ALWAYS keep files under 300 lines (split if larger)
9. ALWAYS add JSDoc to exported functions
10. PREFER immutable data (const, readonly, Object.freeze)
11. PREFER early returns over deeply nested if/else
```

---

## FOLDER 5: `templates/` — The Starting Points

### What It Is

Templates are **boilerplate patterns** for things you create repeatedly. They give Claude the exact shape of a file to produce.

### The Thought Process

> "Every React component in this project looks the same structurally. Every test file starts the same way. Templates guarantee consistency so I don't have to say 'make it look like the other ones' every time."

### Example Files

```
templates/
├── component.md          # React component template
├── endpoint.md           # API endpoint template  
├── service.md            # Service class template
├── test-unit.md          # Unit test template
├── test-integration.md   # Integration test template
├── migration.md          # Database migration template
└── adr.md                # Architecture Decision Record template
```

### Example: `templates/component.md`

```markdown
# Template: React Component

When creating a new React component, follow this structure:

```tsx
// src/components/[ComponentName]/[ComponentName].tsx
import { type FC } from 'react';
import styles from './[ComponentName].module.css';

interface [ComponentName]Props {
  // Define props here
}

export const [ComponentName]: FC<[ComponentName]Props> = ({ ...props }) => {
  return (
    <div className={styles.root}>
      {/* Component content */}
    </div>
  );
};
```

```css
/* src/components/[ComponentName]/[ComponentName].module.css */
.root {
  /* Component styles */
}
```

```tsx
// src/components/[ComponentName]/index.ts
export { [ComponentName] } from './[ComponentName]';
export type { [ComponentName]Props } from './[ComponentName]';
```

```tsx
// src/components/[ComponentName]/[ComponentName].test.tsx
import { render, screen } from '@testing-library/react';
import { [ComponentName] } from './[ComponentName]';

describe('[ComponentName]', () => {
  it('should render without crashing', () => {
    render(<[ComponentName] />);
  });
});
```

Every component gets 4 files: implementation, styles, barrel export, and test.
```

### Example: `templates/adr.md`

```markdown
# Template: Architecture Decision Record

Save to: `docs/decisions/[NNN]-[slug].md`

```markdown
# [Number]. [Title]

**Date:** [YYYY-MM-DD]
**Status:** Proposed | Accepted | Deprecated | Superseded by [ADR-NNN]

## Context

What is the issue that we're seeing that is motivating this decision?
What forces are at play (technical, business, team)?

## Options Considered

### Option A: [Name]
- Pros: ...
- Cons: ...

### Option B: [Name]
- Pros: ...
- Cons: ...

## Decision

What is the change that we're doing?
Why did we choose this option over the others?

## Consequences

### Positive
- What becomes easier or better?

### Negative
- What becomes harder or worse?
- What new constraints do we accept?

### Neutral
- What other changes does this require?
```

Look at existing ADRs in docs/decisions/ to determine the next number.
```

---

## FOLDER 6: `workflows/` — The Multi-Step Pipelines

### What It Is

Workflows describe **complete processes** with multiple phases, decision points, and agent transitions. They're bigger than commands — they orchestrate the whole team.

### The Thought Process

> "Some tasks aren't one step. Building a feature involves designing, coding, reviewing, testing, and documenting. A workflow defines the entire sequence so I just kick it off and supervise."

### How Workflows Differ From Commands

- **Command**: One action. "Review this code."
- **Workflow**: A sequence. "Design → build → review → test → document → ship."

### Example Files

```
workflows/
├── new-feature.md        # Full feature development
├── bug-fix.md            # Bug investigation and fixing
├── release.md            # Release preparation
├── tech-debt.md          # Technical debt reduction sprint
└── new-service.md        # Spinning up a new microservice
```

### Example: `workflows/bug-fix.md`

```markdown
# Workflow: Bug Fix

## Phase 1: INVESTIGATE
Agent: .claude/agents/developer.md

1. Reproduce the bug (run the failing scenario)
2. Read error logs and stack traces
3. Trace to root cause
4. Document: what's broken, why, and where

PAUSE: Present findings. Wait for confirmation before fixing.

## Phase 2: TEST FIRST
Agent: .claude/agents/tester.md

1. Write a failing test that captures the exact bug
2. Verify the test fails for the right reason
3. This test is your proof the fix works

## Phase 3: FIX
Agent: .claude/agents/developer.md

1. Implement the minimal fix
2. Run the new test — it should now pass
3. Run the full test suite — nothing else should break

## Phase 4: REVIEW
Agent: .claude/agents/reviewer.md

1. Review the fix
2. Is it addressing the root cause or just the symptom?
3. Could this bug exist elsewhere? (pattern check)
4. Is the fix introducing any new risks?

If issues found → back to Phase 3

## Phase 5: CLOSE OUT
1. Update CHANGELOG.md
2. Write a brief postmortem:
   - What broke
   - Why it broke
   - How we fixed it
   - How we prevent it in the future
```

### Example: `workflows/release.md`

```markdown
# Workflow: Release Preparation

## Phase 1: AUDIT
Agent: .claude/agents/reviewer.md

1. Review all changes since the last release tag
2. Check for any CRITICAL review items
3. List all breaking changes
4. Verify all TODOs from this cycle are resolved

## Phase 2: TEST
Agent: .claude/agents/tester.md

1. Run the full test suite
2. Run E2E tests
3. Check test coverage hasn't dropped
4. Test any breaking changes with migration paths

## Phase 3: DOCUMENT
Agent: .claude/agents/pm.md

1. Update CHANGELOG.md with all changes
2. Group changes: Added, Changed, Deprecated, Removed, Fixed, Security
3. Update version number
4. Update API docs if endpoints changed
5. Write migration guide if breaking changes exist

## Phase 4: SECURITY CHECK
Agent: .claude/agents/security.md

1. Run `npm audit`
2. Check for any new security concerns in changed code
3. Verify no secrets are exposed

## Phase 5: READY
Present a release summary:
- Version number
- Key changes
- Breaking changes and migration guide
- Any known issues
- Green/red status on: tests, security, docs
```

---

## The Complete File Structure

Here's everything together:

```
my-project/
│
├── CLAUDE.md
│
├── .claude/
│   ├── settings.json
│   │
│   ├── commands/
│   │   ├── plan.md
│   │   ├── build.md
│   │   ├── review.md
│   │   ├── test.md
│   │   ├── fix.md
│   │   ├── refactor.md
│   │   ├── explain.md
│   │   ├── document.md
│   │   └── full-cycle.md
│   │
│   ├── agents/
│   │   ├── architect.md
│   │   ├── developer.md
│   │   ├── reviewer.md
│   │   ├── tester.md
│   │   ├── security.md
│   │   ├── dba.md
│   │   └── pm.md
│   │
│   ├── skills/
│   │   ├── add-api-endpoint.md
│   │   ├── create-component.md
│   │   ├── database-migration.md
│   │   ├── error-handling.md
│   │   ├── testing-patterns.md
│   │   └── authentication.md
│   │
│   ├── rules/
│   │   ├── security.md
│   │   ├── database.md
│   │   ├── api-contracts.md
│   │   ├── dependencies.md
│   │   └── code-quality.md
│   │
│   ├── templates/
│   │   ├── component.md
│   │   ├── endpoint.md
│   │   ├── service.md
│   │   ├── test-unit.md
│   │   ├── test-integration.md
│   │   ├── migration.md
│   │   └── adr.md
│   │
│   └── workflows/
│       ├── new-feature.md
│       ├── bug-fix.md
│       ├── release.md
│       ├── tech-debt.md
│       └── new-service.md
│
├── docs/
│   ├── specs/
│   │   └── (architect output goes here)
│   └── decisions/
│       └── (ADRs go here)
│
└── src/
    └── (your actual code)
```

---

## How to Build This Incrementally (Don't Do It All at Once)

### Week 1: Foundation
1. Install Claude Code
2. Run `/init` to generate CLAUDE.md
3. Enrich CLAUDE.md with your human knowledge
4. Create 2-3 basic commands (`plan.md`, `review.md`, `fix.md`)

### Week 2: Agents and Rules
5. Create your core agents (`developer.md`, `reviewer.md`)
6. Write your most important rules (`security.md`, `code-quality.md`)
7. Update commands to reference agents and rules

### Week 3: Skills and Templates
8. Document your most common task as a skill (`add-api-endpoint.md`)
9. Create templates for files you make often
10. Add more skills as you notice patterns

### Week 4: Workflows and Polish
11. Create the `full-cycle.md` workflow
12. Add hooks to settings.json for auto-formatting
13. Set up MCP servers for your tools
14. Share the setup with your team

### Ongoing: Evolve
15. After every major feature, ask Claude: "What should we add to our skills or rules based on what we learned?"
16. Your `.claude/` folder is a living document — it grows with your project

---

## Quick Reference: How Commands Reference Everything Else

The pattern in every command file:

```markdown
[Task description]

SETUP:
- Load the agent persona from .claude/agents/[which-agent].md
- Load the relevant skill from .claude/skills/[which-skill].md
- Load ALL rules from .claude/rules/
- Follow the template from .claude/templates/[which-template].md (if creating files)

PROCESS:
[Steps specific to this command]

Request: $ARGUMENTS
```

This is the wiring that connects everything. Commands are the entry points. Agents, skills, rules, and templates are the building blocks.

---

*For the most current Claude Code documentation, visit https://code.claude.com/docs/en/overview*
*These folder patterns are community-driven best practices, not official requirements.*
*Claude reads any markdown file you point it to — the folder names are for YOUR organization.*
