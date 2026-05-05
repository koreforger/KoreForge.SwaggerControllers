# KoreForge.SwaggerControllers — Structure

## 1. Repo tree

```text
eco-system/KoreForge.SwaggerControllers/
  README.md
  LICENSE.md
  KoreForge.SwaggerControllers.slnx
  Directory.Build.props
  Directory.Packages.props
  NuGet.config
  doc/
    specification.md
    user-guide.md
    developer-guide.md
    structure.md
    notes/
      implementation-plan.md
  scr/
    build-clean.ps1
    build-rebuild.ps1
    build-test.ps1
    build-test-codecoverage.ps1
    build-pack.ps1
    install-local.ps1
    sync-skill.ps1
    koreforge-build.psm1
  src/
    KoreForge.SwaggerControllers.Abstractions/        # runtime library package
      KoreForge.SwaggerControllers.Abstractions.csproj
      *.cs                                            # Outcome, ErrorInfo, AuthDelegatingHandler, etc.
    KoreForge.SwaggerControllers.Template/            # dotnet new template package
      KoreForge.SwaggerControllers.Template.csproj
      templates/koreforge-swagger-controllers/        # template content tree
        .template.config/template.json
        .github/skills/parse-swagger/                 # bundled Copilot skill
        Directory.Build.props
        Directory.Packages.props
        NuGet.config
        MyCompany.External.Apis.slnx
        src/MyCompany.External.Apis.AssemblyMarker/
        tst/MyCompany.External.Apis.Tests/
        swaggers/
          _readme.txt
          Sample/v1/                                  # excluded unless --IncludeSampleSwagger
        scr/                                          # *.ps1.txt renamed to *.ps1 on scaffold
  tst/
    KoreForge.SwaggerControllers.Abstractions.Tests/  # 32 unit tests
    KoreForge.SwaggerControllers.Template.Tests/      # generator snapshot test
      expected/Sample/v1/                             # snapshot baseline
```

## 2. Two shipping packages

```text
KoreForge.SwaggerControllers.Abstractions  --(PackageReference)-->  every generated repo, every host
KoreForge.SwaggerControllers (template)    --(dotnet new install)--> developer machine -> scaffolds new repo
```

Generated consumer repos depend on Abstractions; they do not depend on the template package at runtime.

## 3. Generated project layout

For each `swaggers/<ApiName>/v<N>/metadata.json` the generator produces:

```text
src/<RootNamespace>.<ApiName>.V<N>/
  <RootNamespace>.<ApiName>.V<N>.csproj      # write-once
  ServiceCollectionExtensions.cs             # write-once
  <ApiName>BusinessDecorator.cs              # write-once (TODO stub)
  <ApiName>PersistenceDecorator.cs           # write-once (TODO stub)
  Generated/
    Dtos/<DtoName>.g.cs                      # regenerated
    I<ApiName>ExternalClient.g.cs            # regenerated
    I<ApiName>Service.g.cs                   # regenerated
    <ApiName>Service.g.cs                    # regenerated
    <ApiName>LoggingDecorator.g.cs           # regenerated
    <ApiName>Controller.g.cs                 # regenerated
    Permissions.g.cs                         # regenerated
```

`Generated/` is wiped and rewritten on every `generate.ps1` run; everything outside `Generated/` is written once and never overwritten so user edits survive.

## 4. Decorator chain

`Add<Api>Services` resolves `I<Api>Service` from the DI container as:

```text
host -> I<Api>Service -> <Api>LoggingDecorator
                       -> <Api>BusinessDecorator   (user-owned, pass-through default)
                       -> <Api>PersistenceDecorator (user-owned, pass-through default)
                       -> <Api>Service             (Refit + Outcome mapping)
                       -> I<Api>ExternalClient     (Refit-generated HTTP client)
                       -> upstream HTTP API
```

Outcomes flow back up unchanged; each decorator may inspect, log, or transform without breaking the chain.

## 5. Two-stage code generation

```text
Stage 1 (Copilot skill, .github/skills/parse-swagger/)
  swagger.yml  -->  metadata.json   (naming + type rules applied here)

Stage 2 (PowerShell, scr/generate.ps1)
  metadata.json  -->  src/<RN>.<Api>.V<N>/Generated/*.g.cs  + write-once scaffolds
```

The skill is bundled inside the template package and kept in sync with the workspace copy by `scr/sync-skill.ps1` (use `-Check` in CI).

## 6. Script discovery

Repo scripts under `scr/` follow the workspace convention so they appear in `builder.ps1`:

```text
build-clean.ps1
build-rebuild.ps1
build-test.ps1
build-test-codecoverage.ps1
build-pack.ps1
install-local.ps1
sync-skill.ps1
```

All build output is routed through the workspace `artifacts/` tree by `scr/koreforge-build.psm1`; nothing is written under the repo root.
