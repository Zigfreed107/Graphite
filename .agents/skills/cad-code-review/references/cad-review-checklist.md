# CAD Review Checklist

Use this checklist when reviewing Graphite CAD code. Prefer concrete evidence from the code over generic advice.

## Tool Lifecycle

- Ensure exactly one interaction tool owns pointer/keyboard input at a time.
- Check activation and deactivation paths for cleanup of preview geometry, snap markers, transient selection, and event subscriptions.
- Look for stale tool state when switching tools, canceling operations, completing commands, or losing viewport focus.
- Confirm tool state transitions are explicit enough for future tools such as rectangle select, move, trim, pan, orbit, and dimension tools.
- Watch for duplicate event handling where UI code directly calls multiple tools instead of routing through a single manager.

## Architecture Boundaries

- Keep durable CAD data, command history, selection state, snapping policy, and geometry rules in `CadApp.Core` or `CadApp.Geometry`.
- Keep Helix Toolkit objects, viewport math, preview visuals, and scene synchronization in `CadApp.Rendering`.
- Keep WPF event wiring, commands, and view concerns in `CadApp.UI` or `CadApp.ViewModels`.
- Flag coupling where a core entity depends on rendering types or where UI handlers mutate document state without a tool/command boundary.
- Prefer simple interfaces only when they protect a real boundary or remove duplicated logic.

## Real-Time Performance

- Avoid allocations inside mouse move, render, hit-test, and preview-update paths.
- Avoid rebuilding the whole scene when a small entity or preview changed.
- Avoid per-frame LINQ, reflection, string formatting, logging, or repeated collection copies.
- Prefer cached render resources and incremental updates for stable scene elements.
- Flag expensive snapping or selection scans that will degrade as entity count grows.

## CAD Behavior

- Check coordinate conversion between screen space, viewport rays, workplanes, and world coordinates.
- Confirm tolerances are named constants or settings, not magic numbers.
- Verify selection and snapping are deterministic when entities overlap or the cursor is near multiple candidates.
- Confirm preview entities are transient and cannot accidentally enter the document model.
- Look for clear command boundaries for undo/redo, especially when tools create or mutate entities.
- Check cancel, escape, right-click, and incomplete-command behavior.

## Maintainability

- Prefer explicit C# types for clarity.
- Require comments where state machines, coordinate math, or rendering lifecycle logic would be hard to infer.
- Do not request comments that merely restate obvious code.
- Flag hard-coded colors, tolerances, dimensions, or input bindings that should become settings or named constants.
- Favor small, understandable changes suitable for a single-developer project.

## Testing

- Recommend focused unit tests for tool state transitions, selection behavior, snapping rules, geometry operations, and command history.
- Recommend lightweight integration checks for UI-to-tool routing when regressions are likely.
- Do not demand brittle rendering snapshot tests unless the visual contract is stable and worth the maintenance cost.
