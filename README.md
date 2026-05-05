# KoreForge.SwaggerControllers

A KoreForge .NET project template + companion runtime library for generating opt-in ASP.NET Core controllers from Swagger 2.0 documents.

The template scaffolds a multi-API library that:

- Parses each `swagger.yml` once into a deterministic `metadata.json` (Stage 1, Copilot skill).
- Generates DTOs, a Refit external client, an `I<Api>Service` interface, the leaf `<Api>Service`, a `<Api>LoggingDecorator`, an MVC `<Api>Controller`, and a `Permissions` catalog (Stage 2, PowerShell 7).
- Lets developers customize behavior via two scaffolded-once decorator layers (`<Api>BusinessDecorator`, `<Api>PersistenceDecorator`) that the generator never overwrites.
- Exposes its controllers only when a host explicitly opts in via `IMvcBuilder.AddSwaggerControllers<TMarker>()`.

See [doc/specification.md](doc/specification.md) for the full contract and [doc/notes/implementation-plan.md](doc/notes/implementation-plan.md) for the build-out sequence.

## Quick start

```powershell
dotnet new install KoreForge.SwaggerControllers
dotnet new koreforge-swagger-controllers -n MyCompany.External.Apis
```

Then drop one or more `swagger.yml` files into `swaggers/<ApiName>/v<N>/`, run the parse-swagger Copilot skill, and run `scr/generate.ps1`.
