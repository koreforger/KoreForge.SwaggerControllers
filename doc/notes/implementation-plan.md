# KoreForge.SwaggerControllers — Implementation Plan

This plan sequences the build-out of [specification.md](../specification.md). Each phase is independently shippable and leaves `main` green.

## Phase 0 — Repo bootstrap

1. Create the GitHub repo `koreforger/KoreForge.SwaggerControllers` (public). Default branch `main`. Open `development` branch.
2. Drop in: `README.md`, `LICENSE.md`, `.gitignore`, `Directory.Build.props`, `Directory.Packages.props`, `NuGet.config`, `KoreForge.SwaggerControllers.slnx`.
3. Copy `scr/koreforge-build.psm1` from a sibling eco-system repo (e.g. `KoreForge.AppLifecycle`) verbatim. Add `build-clean.ps1`, `build-rebuild.ps1`, `build-test.ps1`, `build-pack.ps1`, `build-test-codecoverage.ps1` thin wrappers.
4. Add `.github/workflows/ci.yml` (build + test) and `.github/workflows/publish-nuget.yml` (tag-on-main + pack + push, two tag prefixes).
5. Open PR into `development`, merge, then PR `development -> main`.

## Phase 1 — Abstractions package

1. Scaffold `src/KoreForge.SwaggerControllers.Abstractions/` `.csproj` with `<TargetFramework>net10.0</TargetFramework>`, `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`, NuGet metadata.
2. Add public types **one per file** matching Section 4.1 of the spec.
3. Add `tst/KoreForge.SwaggerControllers.Abstractions.Tests/` with xUnit + FluentAssertions:
   - `OutcomeTests` (Ok/Fail factories, immutability).
   - `OutcomeExtensionsTests` (status code mapping table).
   - `AuthDelegatingHandlerTests` (each `AuthMode` adds the right headers using a fake inner handler).
   - `DefaultPrincipalRoleResolverTests` (multiple Role claims, dedup, trimming).
   - `PublicApiApprovalTests` (PublicApiGenerator + Verify).
4. Pack via `scr/build-pack.ps1`, verify `.nupkg` lands in `artifacts/packages`.
5. Tag `KoreForge.SwaggerControllers.Abstractions/v0.1.0`, publish to NuGet.org via `publish-nuget.yml`.

## Phase 2 — Copilot skill `parse-swagger`

1. Author the skill at workspace `c:\My\KoreForge2\.github\skills\parse-swagger\`:
   - `SKILL.md` — purpose, frontmatter, when-to-use rules.
   - `references/metadata-schema.md` — JSON schema documentation with example.
   - `references/naming-conventions.md` — operationId → C# method name, parameter casing rules.
   - `references/type-mapping.md` — Swagger primitives & `$ref` → C# types (incl. nullable handling).
2. Validate against three real swaggers from `temp/aft-detection-core-external-api/swaggers/` (Party, SuspendAccount V1+V2, UserGroupFederationDetail V1+V2) — confirm metadata.json is structurally identical to the existing vendored output.
3. Pre-commit: a `scripts/sync-skill.ps1` that copies workspace skill into the template package directory and fails CI if drift is detected.
4. Update `c:\My\KoreForge2\docs\ecosystem\documentation-inventory.md` with the new skill.

## Phase 3 — Template scaffold (no generator yet)

1. Build `src/KoreForge.SwaggerControllers.Template/KoreForge.SwaggerControllers.Template.csproj` with `PackageType=Template`, following the `koreforge-data` `.csproj` shape.
2. Author `templates/koreforge-swagger-controllers/.template.config/template.json` per Section 4.2.
3. Scaffold the template content tree per Section 3 of the spec, with `*.ps1.txt` placeholders for scripts (initially just `Write-Host "TODO"` stubs).
4. Add bundled copy of the `parse-swagger` skill under `.github/skills/`.
5. Pack the template, run `dotnet new install <path>`, run `dotnet new koreforge-swagger-controllers -n Probe.External.Apis` into a temp dir, confirm solution loads in `dotnet`. No code generation yet.

## Phase 4 — Stage 2 generator (`generate.ps1`)

1. Port the vendored `temp/aft-detection-core-external-api/scripts/generate.ps1` to PowerShell 7. Keep helper functions (`Write-FileIfChanged`, `Get-NonBodyParams`, `Get-RefitAttribute`, etc.) but replace any PS5-only constructs.
2. Adapt namespace, type names, and the `Add<Api>V<N>Services` extension shape to match `KoreForge.SwaggerControllers.Abstractions` (Outcome<T>, AddSwaggerControllers, etc.).
3. Add a `<RootNamespace>AssemblyMarker.cs` emit (one per generated repo, not per API).
4. Drop the bundled `Sample/v1/swagger.yml` fixture inside the template content under `swaggers/Sample/v1/` (only included when `IncludeSampleSwagger=true`).
5. Snapshot tests in `tst/KoreForge.SwaggerControllers.Template.Tests/`:
   - Instantiate template into temp dir.
   - Run `parse-swagger` for each swagger fixture (or use checked-in `metadata.json` to keep tests offline).
   - Run `scr/generate.ps1`.
   - Run `dotnet build` — assert zero warnings.
   - Compare generated `*.g.cs` against `tst/expected/<ApiName>/` and assert byte-equality.

## Phase 5 — Documentation

1. `doc/user-guide.md` — install → scaffold → drop swagger → invoke skill → run generate → wire into a host.
2. `doc/developer-guide.md` — build/test/pack scripts; how to extend the generator; how to add a new emitted file kind.
3. `doc/structure.md` — folder map, dependency direction, runtime topology of decorator chain.
4. Update `docs/ecosystem/documentation-inventory.md` with all new docs.

## Phase 6 — Release readiness

1. Public API approval test passes.
2. Generator snapshot tests pass on Windows + Linux runners.
3. `publish-nuget.yml` dry-run on `development` (`--dry-run` flag added) passes.
4. Tag and publish v0.1.0 of the template package.

## Estimated dependency graph

```text
Phase 0 ──> Phase 1 ──> Phase 6 (Abstractions release)
            │
            └─> Phase 2 ──> Phase 3 ──> Phase 4 ──> Phase 5 ──> Phase 6 (Template release)
```

Phase 4 cannot start until Phase 1 publishes a real `KoreForge.SwaggerControllers.Abstractions` package to either NuGet.org or the workspace local feed.
