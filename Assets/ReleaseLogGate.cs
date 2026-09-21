using System;
using System.Linq;
using UnityEngine;

public static class ReleaseLogGate
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Apply()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.unityLogger.logEnabled = true;
        Debug.unityLogger.filterLogType = LogType.Log;
#else
        // Release players stay quiet, with one exception: the headless CLI modes, where the
        // log is the only output channel. Muting those makes a failure indistinguishable
        // from a hang -- --data-path being silently ignored read as a 60s stall with an
        // empty log and no error.
        //
        // This lives here rather than in CliExporter because BeforeSceneLoad hooks run in an
        // unspecified order, so a decision made elsewhere could be overwritten by this one.
        var argv = Environment.GetCommandLineArgs();
        bool headless = argv.Contains("--export") || argv.Contains("--verbose");
        // The WebGL player has no player.log; the browser console is its only output channel,
        // so keep logging on there (matches the headless rationale above).
        bool web = Application.platform == RuntimePlatform.WebGLPlayer;
        Debug.unityLogger.logEnabled = headless || web;
        if (headless || web) Debug.unityLogger.filterLogType = LogType.Log;
#endif
    }
}
