# KoreForge.SwaggerControllers — Developer Guide

This guide is for contributors changing the Abstractions library, the template, or the generator.

## 1. Repo layout

See [structure.md](structure.md) for the full tree. The two shipping artifacts are:

- `src/KoreForge.SwaggerControllers.Abstractions/` — runtime library.
- `src/KoreForge.SwaggerControllers.Template/` — `dotnet new` template package whose content lives under `templates/koreforge-swagger-controllers/`.

The template content tree includes the bundled `parse-swagger` Copilot skill, the Stage-2 generator (`scr/generate.ps1.txt`), and a `Sample` swagger fixture used by the snapshot tests.

## 2. Build and test

All commands run from the repo root.

```pwsh
pwsh scr/build-clean.ps1
pwsh scr/build-rebuild.ps1
pwsh scr/build-test.ps1
pwsh scr/build-test-codecoverage.ps1
```

`scr/build-pack.ps1 -Configuration Release` produces `.nupkg` files under `artifacts/repos/KoreForge.SwaggerControllers/packages/`.

The full test suite covers:

- `KoreForge.SwaggerControllers.Abstractions.Tests` — 32 unit tests on `Outcome<T>`, `OutcomeExtensions`, `AuthDelegatingHandler`, etc.
- `KoreForge.SwaggerControllers.Template.Tests` — snapshot test that runs `generate.ps1` against the bundled `Sample` fixture and byte-compares output against `expected/Sample/v1/`.

## 3. Local package development

The workspace NuGet feed is at `artifacts/packages/`. To validate template changes end-to-end:

```pwsh
pwsh scr/build-pack.ps1 -Configuration Release
Copy-Item artifacts\repos\KoreForge.SwaggerControllers\packages\KoreForge.SwaggerControllers.<v>.nupkg ..\..\artifacts\packages\
dotnet new uninstall KoreForge.SwaggerControllers
dotnet new install ..\..\artifacts\packages\KoreForge.SwaggerControllers.<v>.nupkg --force
```

Then scaffold a probe under `eco-system/`:

```pwsh
cd ..\..\eco-system
dotnet new koreforge-swagger-controllers -n Probe.External.Apis --RootNamespace Probe.External.Apis --IncludeSampleSwagger true
cd Probe.External.Apis
pwsh scr/generate.ps1
dotnet build src/Probe.External.Apis.Sample.V1/Probe.External.Apis.Sample.V1.csproj
```

`dotnet build` must report `0 Warning(s)` and `0 Error(s)`.

## 4. Generator extension points

The generator is a single PowerShell 7 script (`scr/generate.ps1.txt`). Extending it usually means editing one emitter helper:

| Helper                          | Produces                                                      |
| ------------------------------- | ------------------------------------------------------------- |
| `Emit-Dto`                      | DTO records with `[JsonPropertyName]`.                        |
| `Emit-RefitClient`              | Refit interface (`[Get]`/`[Post]` + `[AliasAs]`/`[Body]`).    |
| `Emit-ServiceInterface`         | Domain interface returning `Task<Outcome<T>>`.                |
| `Emit-ServiceImpl`              | Refit-to-Outcome mapping with `JsonSerializer`.               |
| `Emit-LoggingDecorator`         | Start/success/failure logging.                                |
| `Emit-Controller`               | `[ApiController]` + `[Authorize]` per action.                 |
| `Emit-Permissions`              | `const string` permission names.                              |
| `Emit-Csproj`                   | One-shot project file.                                        |
| `Emit-ServiceCollectionExtensions` | One-shot DI wiring.                                        |
| `Emit-ScaffoldDecorator`        | One-shot business and persistence decorator stubs.            |

After changing any emitter, regenerate the snapshot baseline and re-run the test suite:

```pwsh
# Regenerate baseline into the snapshot folder, then run tests
pwsh -NoProfile -File src/KoreForge.SwaggerControllers.Template/templates/koreforge-swagger-controllers/scr/generate.ps1.txt -RepoRoot <scratch>
# Copy <scratch>/src/<rn>.Sample.V1/* to tst/KoreForge.SwaggerControllers.Template.Tests/expected/Sample/v1/
dotnet test tst/KoreForge.SwaggerControllers.Template.Tests/KoreForge.SwaggerControllers.Template.Tests.csproj
```

## 5. Bundled skill

The `parse-swagger` Copilot skill is shipped with the template and **must stay byte-identical** to the workspace skill at `.github/skills/parse-swagger/`. The mirror is enforced by:

```pwsh
pwsh scr/sync-skill.ps1            # copies workspace -> template
pwsh scr/sync-skill.ps1 -Check     # exits non-zero if drift exists
```

Run `-Check` in CI to gate releases.

## 6. Release

1. Ensure `git status` is clean on `development`.
2. `pwsh scr/build-pack.ps1 -Configuration Release` and copy the two `.nupkg` files into `artifacts/packages/`.
3. Smoke-test the template via Section 3.
4. Open a PR `development → main`; tag with `v<X.Y.Z>` after merge.
5. Publish from `main` using the workspace release scripts; never publish from `development`.

## 7. Conventions

- One top-level type per file. File name matches the type.
- `TreatWarningsAsErrors=true`. `NoWarn` includes `CS1591`.
- No `ProjectReference` in production code; only `PackageReference`.
- Branches: `development` for active work, `main` for releases.
