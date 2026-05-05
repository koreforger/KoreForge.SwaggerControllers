using System.Diagnostics;
using FluentAssertions;
using Xunit;

namespace KoreForge.SwaggerControllers.Template.Tests;

public sealed class GeneratorSnapshotTests
{
    [Fact]
    public void Generator_produces_expected_files_for_Sample_fixture()
    {
        var repoRoot = LocateRepoRoot();
        var templateRoot = Path.Combine(
            repoRoot,
            "src",
            "KoreForge.SwaggerControllers.Template",
            "templates",
            "koreforge-swagger-controllers");
        Directory.Exists(templateRoot).Should().BeTrue($"template content tree must exist at {templateRoot}");

        var scratch = Path.Combine(Path.GetTempPath(), "kfsc-snapshot-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(scratch);
            Directory.CreateDirectory(Path.Combine(scratch, "scr"));
            Directory.CreateDirectory(Path.Combine(scratch, "swaggers", "Sample", "v1"));
            File.WriteAllText(Path.Combine(scratch, "Sample.Test.slnx"), string.Empty);

            File.Copy(
                Path.Combine(templateRoot, "swaggers", "Sample", "v1", "metadata.json"),
                Path.Combine(scratch, "swaggers", "Sample", "v1", "metadata.json"));
            File.Copy(
                Path.Combine(templateRoot, "scr", "generate.ps1.txt"),
                Path.Combine(scratch, "scr", "generate.ps1"));

            RunPwsh(scratch, Path.Combine(scratch, "scr", "generate.ps1"));

            var generatedRoot = Path.Combine(scratch, "src", "Sample.Test.Sample.V1");
            Directory.Exists(generatedRoot).Should().BeTrue("generator must emit project directory");

            var expectedRoot = Path.Combine(AppContext.BaseDirectory, "expected", "Sample", "v1");
            Directory.Exists(expectedRoot).Should().BeTrue($"expected baseline must be copied to {expectedRoot}");

            var expectedFiles = Directory.GetFiles(expectedRoot, "*", SearchOption.AllDirectories);
            expectedFiles.Should().NotBeEmpty();

            foreach (var expectedFile in expectedFiles)
            {
                var rel = Path.GetRelativePath(expectedRoot, expectedFile);
                var actualFile = Path.Combine(generatedRoot, rel);
                File.Exists(actualFile).Should().BeTrue($"generator must emit {rel}");
                var expected = File.ReadAllText(expectedFile);
                var actual = File.ReadAllText(actualFile);
                actual.Should().Be(expected, $"generator output for {rel} must match snapshot");
            }
        }
        finally
        {
            try { Directory.Delete(scratch, recursive: true); } catch { /* best effort */ }
        }
    }

    private static string LocateRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (Directory.GetFiles(dir.FullName, "*.slnx").Length > 0) return dir.FullName;
            dir = dir.Parent;
        }
        throw new InvalidOperationException("Could not locate repo root (no *.slnx found walking up from test output).");
    }

    private static void RunPwsh(string workingDirectory, string scriptPath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "pwsh",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        psi.ArgumentList.Add("-NoProfile");
        psi.ArgumentList.Add("-File");
        psi.ArgumentList.Add(scriptPath);

        using var process = Process.Start(psi)!;
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        process.ExitCode.Should().Be(0, $"pwsh generate.ps1 must succeed.\nSTDOUT:\n{stdout}\nSTDERR:\n{stderr}");
    }
}
