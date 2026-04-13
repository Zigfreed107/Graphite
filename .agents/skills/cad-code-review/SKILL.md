---
name: cad-code-review
description: Review the Graphite CAD application code for C#, WPF, Helix Toolkit SharpDX, interaction tools, ToolManager behavior, selection, snapping, rendering, scene management, architecture boundaries, real-time performance, maintainability, and scale risks. Use when Codex is asked to review CAD code, find bugs, assess what will break at scale, or suggest refactors in this repository.
---

# CAD Code Review

Use this skill to review the Graphite CAD codebase with a findings-first, CAD-aware lens.

## Workflow

1. Read `Agents.md` before reviewing so the project constraints and teaching preferences are active.
2. Inspect the relevant source files before making claims. For tool reviews, include `ToolManager`, active tools, UI event wiring, preview rendering, snapping, selection, and document mutation paths.
3. Load `references/cad-review-checklist.md` when the review involves CAD architecture, rendering, tools, selection, snapping, performance, or maintainability tradeoffs.
4. Lead with actionable findings ordered by severity. Prefer concrete bugs, behavioral regressions, scale risks, missing tests, and architecture boundary violations over style notes.
5. Use file and line references for every finding. Keep each line range tight.
6. Explain why each issue matters in a CAD application, then give the smallest clean fix.
7. Avoid broad rewrites unless the evidence shows the current design will block near-term features or cause correctness/performance problems.
8. Preserve the user's preferences: explicit C# types, maintainable comments, simple abstractions, no hard-coded constants, no unnecessary rendering/domain coupling, and no speculative large changes.

## Output Format

Start with findings. If there are no findings, say that clearly and mention remaining risk or test gaps.

For each finding, include:

- Severity or priority
- File and line reference
- What is wrong
- Why it matters for CAD behavior, performance, or maintainability
- The smallest clean fix

After findings, include:

- Open questions or assumptions
- Short architecture notes, only when useful
- Suggested next steps

## Review Standards

Treat real-time input/render paths as performance-sensitive. Look for repeated allocations, repeated full-scene rebuilds, duplicate event subscriptions, stale preview geometry, and state machines where multiple tools can respond to the same input.

Treat CAD domain logic as separate from rendering and UI. Rendering may visualize entities and previews, but durable document behavior, command boundaries, geometry rules, selection state, and snapping policy should not be hidden inside WPF event handlers.

Prefer teachable recommendations. The user is learning CAD architecture, so explain important design reasoning briefly without burying the findings.
