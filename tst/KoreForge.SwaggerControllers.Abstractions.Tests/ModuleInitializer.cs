using System.Runtime.CompilerServices;

namespace KoreForge.SwaggerControllers.Tests;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init() => VerifierSettings.UseUtf8NoBom();
}
