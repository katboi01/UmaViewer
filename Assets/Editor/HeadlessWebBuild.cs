using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class HeadlessWebBuild
{
    // WebGL only supports IL2CPP, so there's no backend to switch (unlike the mac/win Mono
    // builds). Outputs a hostable folder: Build/Web/index.html + Build/Web/Build/*.
    // Pass -umaRelease (tools/build-web.sh --release) for a lean prod build; the default is a
    // debug-friendly build with full stack traces and no data caching.
    public static void Build()
    {
        bool release = Environment.GetCommandLineArgs().Contains("-umaRelease");

        PlayerSettings.WebGL.template = "PROJECT:UmaViewer";
        // Decompress gzipped build files in the loader so any static host works, even one
        // that doesn't send Content-Encoding: gzip (e.g. python3 -m http.server).
        PlayerSettings.WebGL.decompressionFallback = true;
        // Debug: no data caching so a rebuilt Build/Web.data doesn't serve stale over the same
        // URL. Release: cache it so returning visitors don't re-download ~46MB every load.
        PlayerSettings.WebGL.dataCaching = release;
        // Debug: full managed stack traces so caught exceptions can be located. Release: the lean
        // default (explicitly-thrown only), smaller and faster.
        PlayerSettings.WebGL.exceptionSupport = release
            ? WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly
            : WebGLExceptionSupport.FullWithStacktrace;

        Debug.Log($"[HeadlessWebBuild] {(release ? "RELEASE" : "DEBUG")} build");

        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Version2.unity", "Assets/Scenes/LiveScene.unity" },
            locationPathName = "Build/Web",
            target = BuildTarget.WebGL,
            // Release player, not Development: the Development flag hid the in-game top menu bar
            // on WebGL. We don't need it anyway, since ReleaseLogGate keeps WebGL logging on and
            // exceptionSupport above keeps full managed stack traces.
            options = BuildOptions.None,
        };
        var report = BuildPipeline.BuildPlayer(options);
        var s = report.summary;
        if (s.result != BuildResult.Succeeded)
        {
            Debug.LogError($"BUILD_FAILED result={s.result} errors={s.totalErrors}");
            EditorApplication.Exit(1);
            return;
        }
        Debug.Log($"BUILD_OK path={s.outputPath} sizeBytes={s.totalSize}");
        EditorApplication.Exit(0);
    }
}
