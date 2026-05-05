# MyCompany.External.Apis

A KoreForge.SwaggerControllers consumer repo. Drop Swagger 2.0 files under
`swaggers/<ApiName>/v<N>/swagger.yml`, run the bundled `parse-swagger` skill to
produce `metadata.json`, then run `pwsh scr/generate.ps1` to (re)emit the
strongly-typed controller, service, and DTO projects under `src/`.

## Quick start

```pwsh
# 1. Drop swagger
mkdir -p swaggers/Party/v1
copy <upstream>/party-v1.yml swaggers/Party/v1/swagger.yml

# 2. Stage 1: parse via the bundled Copilot skill
#    (Use the `parse-swagger` skill in your editor on the swagger file.)

# 3. Stage 2: generate
pwsh scr/generate.ps1

# 4. Build, test, pack
pwsh scr/build-rebuild.ps1
pwsh scr/build-test.ps1
pwsh scr/build-pack.ps1
```

## Layout

- `swaggers/` — input Swagger 2.0 files. Not committed by upstream policy; check the
  repo's `.gitignore` if you want to ship them.
- `src/<RootNamespace>.AssemblyMarker/` — single empty marker assembly. Hosts
  register the controllers via `AddSwaggerControllers<AssemblyMarker>()`.
- `src/<RootNamespace>.<ApiName>.V<N>/Generated/` — wiped + rewritten by
  `generate.ps1`. Never edit by hand.
- `src/<RootNamespace>.<ApiName>.V<N>/*.cs` (non-`Generated/`) — scaffolded once
  by `generate.ps1`, safe to edit (decorators, DI extensions).
- `tst/MyCompany.External.Apis.Tests/` — unit tests for hand-written code.

## Authorization

Every generated controller action carries `[Authorize]`. The host wires per-method
rules using `KoreForge.Web` `AddDynamicMethodAuthorization` and the generated
`Permissions.<Operation>` constants.
