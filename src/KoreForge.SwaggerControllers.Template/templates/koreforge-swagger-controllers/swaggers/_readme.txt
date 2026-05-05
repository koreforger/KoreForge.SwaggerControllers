Drop Swagger 2.0 input files here under <ApiName>/v<N>/swagger.{yml,yaml,json}.

Each file produces one project under src/<RootNamespace>.<ApiName>.V<N>/ via
the two-stage code generator (parse-swagger Copilot skill -> metadata.json,
then scr/generate.ps1).

This file (_readme.txt) is preserved by the template; safe to delete after
adding your first swagger.
