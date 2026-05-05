# KoreForge.SwaggerControllers — User Guide

This guide shows how to consume the two packages shipped from this repo:

- `KoreForge.SwaggerControllers.Abstractions` — runtime library used by every generated repo and every host.
- `KoreForge.SwaggerControllers` — `dotnet new` template (`koreforge-swagger-controllers`) that scaffolds a multi-API library repo plus a two-stage code generator.

## 1. Install the template

```pwsh
dotnet new install KoreForge.SwaggerControllers
```

During local development the workspace local feed at `artifacts/packages` is searched first (see `NuGet.config`).

## 2. Scaffold a new external-APIs repo

Scaffold under your workspace's eco-system folder so the template's relative `NuGet.config` resolves the local feed correctly:

```pwsh
cd eco-system
dotnet new koreforge-swagger-controllers `
    -n MyCompany.External.Apis `
    --RootNamespace MyCompany.External.Apis
```

Optional flags:

| Flag                      | Default | Notes                                                                 |
| ------------------------- | ------- | --------------------------------------------------------------------- |
| `--RootNamespace`         | project | Root namespace for every generated project (`<RN>.<Api>.V<N>`).        |
| `--IncludeSampleSwagger`  | `false` | When `true`, drops a `swaggers/Sample/v1/` example so `generate.ps1` can be run immediately. |

The scaffold contains `Directory.Build.props`, `Directory.Packages.props`, an empty `swaggers/` folder, the bundled parse-swagger Copilot skill at `.github/skills/parse-swagger/`, and the Stage-2 generator at `scr/generate.ps1`.

## 3. Add an API definition

Drop a Swagger 2.0 file at `swaggers/<ApiName>/v<N>/swagger.yml` and, in Copilot Chat, run the bundled `parse-swagger` skill to produce the matching `metadata.json`. The skill applies the project naming and type-mapping conventions; do not hand-edit `metadata.json` once it is generated.

## 4. Generate the per-API project

```pwsh
pwsh scr/generate.ps1
```

For each `swaggers/<ApiName>/v<N>/metadata.json` the generator emits `src/<RootNamespace>.<ApiName>.V<N>/`:

| File                                  | Regenerated each run | Description                                                  |
| ------------------------------------- | -------------------- | ------------------------------------------------------------ |
| `Generated/Dtos/*.g.cs`               | yes                  | DTOs with `[JsonPropertyName]` and nullable handling.         |
| `Generated/I<Api>ExternalClient.g.cs` | yes                  | Refit interface used to call the upstream service.           |
| `Generated/I<Api>Service.g.cs`        | yes                  | Domain interface returning `Task<Outcome<T>>`.                |
| `Generated/<Api>Service.g.cs`         | yes                  | Leaf implementation that calls Refit and maps to `Outcome`.   |
| `Generated/<Api>LoggingDecorator.g.cs`| yes                  | Logs start/success/failure per call.                          |
| `Generated/<Api>Controller.g.cs`      | yes                  | ASP.NET Core controller (`[ApiController]`, `[Authorize]`).   |
| `Generated/Permissions.g.cs`          | yes                  | `const string` permission names.                              |
| `<projectName>.csproj`                | once                 | `net10.0` + `FrameworkReference` + Refit + Abstractions.      |
| `ServiceCollectionExtensions.cs`      | once                 | `Add<Api>Services` wires the decorator chain.                 |
| `<Api>BusinessDecorator.cs`           | once                 | Hand-written hook for business rules. Pass-through default.   |
| `<Api>PersistenceDecorator.cs`        | once                 | Hand-written hook for audit/persistence. Pass-through default.|

Re-running `generate.ps1` is idempotent: `Generated/` is wiped and rewritten; the four scaffold files are never overwritten.

## 5. Wire the host

In your host project:

```csharp
using KoreForge.SwaggerControllers;

builder.Services.AddSwaggerControllers<MyCompany.External.Apis.AssemblyMarker>();

builder.Services.AddSampleServices(http =>
{
    http.ConfigureHttpClient(c => c.BaseAddress = new Uri("https://upstream.example.com"));
    // attach AuthDelegatingHandler etc. here as needed
});
```

`AddSwaggerControllers<TMarker>()` registers MVC and discovers controllers from the assembly that owns `TMarker`. `Add<Api>Services` registers the Refit client and the decorator chain (Logging → Business → Persistence → Service).

## 6. Authorization

Each generated controller action is decorated with `[Authorize]` and the `Permissions.<OperationName>` constant gives you a stable name for the action. Bind those names to your authorization story in the host (KoreForge.Web rules, ASP.NET policies, etc.). The package itself does not pick an authorization library.

## 7. Troubleshooting

- **`dotnet new install` fails with "No projects to restore"** — the workspace `NuGet.config` enables `packageSourceMapping`. Ensure the repo's `NuGet.config` maps `KoreForge.*` to the local feed (the template ships this mapping by default).
- **Generator complains the variable `$type?` is not set** — you are running an older `generate.ps1`; pull the latest template.
- **Refit `Query.Name` not found** — Refit 8 dropped `Query(Name = ...)`; the generator emits `[Query, AliasAs("...")]` instead.
- **`error NU1510` for `Microsoft.Extensions.Logging.Abstractions`** — that package is part of `Microsoft.AspNetCore.App`; remove it from the per-API csproj.
