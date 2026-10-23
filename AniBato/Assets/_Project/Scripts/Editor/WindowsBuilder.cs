using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace AniBato.EditorTools
{
    public static class WindowsBuilder
    {
        public const BuildOptions Options = BuildOptions.CompressWithLz4HC | BuildOptions.DetailedBuildReport;
        public static void Build()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
                throw new InvalidOperationException("Windows 64bit target is required.");
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-anibatoWindowsOutput");
            if (index < 0 || index + 1 >= args.Length) throw new ArgumentException("-anibatoWindowsOutput is required.");
            string output = Path.GetFullPath(args[index + 1]);
            string allowed = Path.GetFullPath("../Build") + Path.DirectorySeparatorChar;
            if (!output.StartsWith(allowed,StringComparison.OrdinalIgnoreCase) || Path.GetFileName(output) != "AniBato.exe")
                throw new ArgumentException("Output must be an AniBato.exe below Build.");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = output, target = BuildTarget.StandaloneWindows64, options = Options
            });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Windows build failed: " + report.summary.result);
        }
    }
}
