# SolidWorksAIBridge session instructions

## Purpose and documentation

The project aims to reconstruct architectural buildings/facades from photographs and generate parametric SolidWorks models. Today it is a C# console application that directly creates a single wall and architectural openings through SolidWorks COM. The AI pipeline and Architectural LEGO library are approved designs, not implemented subsystems.

At the start of each session:

1. Read this file, [ARCHITECTURE.md](ARCHITECTURE.md), [ROADMAP.md](ROADMAP.md), and [DECISIONS.md](DECISIONS.md).
2. Read relevant sections of [ARCHITECTURAL_MODELING_STANDARD_V1.md](ARCHITECTURAL_MODELING_STANDARD_V1.md) and [ARCHITECTURAL_LEGO_V1.md](ARCHITECTURAL_LEGO_V1.md).
3. Inspect the relevant source and project configuration. Documentation never replaces code verification.
4. Identify the development stage and distinguish implemented, partial, experimental, planned, and UNKNOWN status. Do not infer tests passed from a method name, binary, comment, or design document.
5. Implement only the requested scope; preserve unrelated work. Do not treat a roadmap as authorization to implement it all.
6. Run available relevant checks, subject to the session's permission to execute CAD. Report unavailable checks and reasons accurately.
7. Update relevant documentation when architecture, implementation status, or major decisions change.
8. End with a concise summary: changes, validation, limitations, and next step.

## Existing code conventions

- Follow the current SolidWorksAIBridge namespace, four-space indentation, braces on separate lines, PascalCase types/methods, and camelCase locals/private fields.
- Target .NET Framework 4.8 and the existing explicit Compile entries in the non-SDK csproj. Do not introduce a framework migration or dependencies without task justification.
- Preserve the STA entry point for COM. Keep connection logic in SolidWorksConnector, geometry in SolidWorksModeler, and length conversion in UnitConverter or appropriate future equivalents.
- Public existing geometry lengths/positions are miniature millimeters; internal COM lengths are meters. Preserve explicit conversion and finite/positive/nonnegative validation. Future Building JSON architectural lengths are real-world millimeters and must be resolved separately before calling these APIs.
- Use descriptive feature/dimension names and contextual exceptions. Preserve invariant numeric formatting in diagnostics.
- Restore modified sketch settings and application preferences in finally blocks. Explicitly select sketches/entities; do not rely on incidental selection state or undocumented segment ordering.

## SolidWorks automation and compatibility

- The connector attaches to an already running SolidWorks instance; do not silently add auto-launch behavior.
- Running the application mutates CAD documents. Do not run it as a harmless smoke test during documentation/read-only tasks. Perform CAD execution only when authorized by the current task.
- Existing behavior creates unsaved parts, leaves failures open for inspection, and does not save/close user documents. Preserve that behavior unless explicitly requested otherwise.
- A configured default Part template and a plane named Front Plane are current prerequisites. COM configuration targets version 32 (SolidWorks 2024); compatibility on another installation is unverified until checked.
- Preserve existing public signatures, centered-wall coordinates, feature naming, ground-door semantics, cut tolerance behavior, and shared-sketch row/grid generation.
- Pattern/Mirror guidance applies to future features; it does not authorize replacing working shared sketches. Prefer narrow adapters/additive paths over broad refactors.
- Current overlap tracking is 2D and one-wall-specific. Do not assume arbitrary orientations, full-building topology, or safe multi-body cut scope are already supported.
- For new multi-body features, explicitly identify target bodies and merge policy. Do not cut unrelated bodies or merge by geometric contact alone.

## Testing and lifecycle

The initial documentation audit found no tests. A regression baseline now exists under [tests/README.md](tests/README.md): Run-RegressionTests.ps1 builds/runs only non-COM checks; ManualValidation.csproj is a separate explicit opt-in CAD harness, including slab cases. The first slab implementation has typed preflight tests; read the latest tests/results report before claiming coverage. Do not execute the manual harness as part of automated tests. No CI or completed recorded CAD suite exists yet.

- For code changes, build with the available compatible Windows MSBuild/COM toolchain when permitted; record the exact command/result. A build is not a CAD integration test.
- For geometry changes, verify relevant dimensions, direction/depth, placement, feature/body scope, rebuilds, parameter edits, and legacy behavior when CAD execution is authorized.
- Prefer non-COM checks for schema/parameter/dependency logic before CAD mutation. Add meaningful regression fixtures as these layers are implemented.
- Follow the Golden Dataset strategy in ARCHITECTURAL_LEGO_V1.md; do not claim the dataset exists until artifacts are present.
- Reusable definitions follow Draft -> Testing -> Validated. AI must not register untested geometry as Validated. Required human visual approval supplements automated evidence.

## Maintaining portable memory

Keep repository-relative links and record reproducible prerequisites rather than machine-specific paths. Do not store secrets, local IDE caches, or build output as project memory. Update ARCHITECTURE.md for actual boundaries, ROADMAP.md for evidence/status/next steps, DECISIONS.md for choices and unresolved questions, and the detailed standards only when their contracts change within the authorized scope. Preserve useful prior documentation and decision history. Do not commit or push automatically.

## Mandatory session-closing documentation

At the end of every development session:

1. Update ROADMAP.md with completed work, tests actually verified, unresolved issues, and the exact next development step.
2. Update DECISIONS.md with any new architectural or implementation decisions and their rationale. If there are no new decisions, do not invent entries.
3. Update ARCHITECTURE.md only when the actual system architecture changes; a plan or test run alone does not establish an architecture change.
4. Clearly distinguish planned, implemented, tested, and validated functionality. Implemented source is not proof of successful execution; passing focused tests is not full component validation.
5. Never report tests as passed unless they actually ran successfully. Record failed, blocked, and not-run checks explicitly, including relevant limitations.
6. Preserve a concise handoff summary in ROADMAP.md for the next Codex session: what changed, verification results, remaining issues, and the exact next action. Include a concise end-of-session summary in the user-facing response.

Respect the current task's file-change scope; if a required documentation update is outside that scope, report it as pending rather than modifying unauthorized files. Do not perform Git operations automatically. Session closing does not authorize production code changes, CAD execution, commits, or pushes.
