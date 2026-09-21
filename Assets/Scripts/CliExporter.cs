using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using UnityEngine;

// Headless export mode: reuses the in-app exporters to write a PMX (+textures) and
// a VMD for babylon-mmd, driven by command-line args. Enabled by --export.
//
//   UmaViewer -batchmode --export \
//     --data-path /path/to/Persistent --chara 1127 --costume 00 \
//     --anim anm_eve_chr1127_00_idle01_loop --out ./export/1127 \
//     -logFile ./logs/export.log [--region jp|global] [--blink] [--no-mouth]
//
// --pmx-name / --vmd-name override the generated chr<id>_<costume> filenames; the
// extension is optional, and the VMD defaults to the model's stem.
//
// Model and motion are separable: omit --anim to export just the PMX (+textures); pass
// --no-model with --anim to export just the VMD (reusing a model you exported once).
// --blink injects a periodic eye-blink; --no-mouth strips the mouth vowel morphs so a
// viewer can drive lip-sync live (together: a talkable loop). --seconds N records N
// seconds raw (no loop trim) instead of one period.
//
// Run with -batchmode but NOT -nographics: character meshes/textures need a real
// GfxDevice (a Null device loads nothing). macOS batchmode uses an offscreen Metal
// context. Runs in the real player loop so singletons, coroutines, and the recorder work.
public class CliExporter : MonoBehaviour
{
    static string[] Argv => Environment.GetCommandLineArgs();
    static bool Flag(string f) => Argv.Contains(f);
    static string Opt(string name, string def = null)
    {
        var a = Argv;
        for (int i = 0; i < a.Length - 1; i++) if (a[i] == name) return a[i + 1];
        return def;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void PreInit()
    {
        if (!Flag("--export")) return;
        // This runs BeforeSceneLoad, ahead of the Awake that normally creates the Config,
        // so Config.Instance is still null here. Create it now: the overrides below have to
        // be in place before the database loads, and UmaViewerMain.Awake leaves an existing
        // instance alone.
        if (Config.Instance == null) new Config();
        // Point the runtime at the requested data folder before the DB loads in Awake.
        var dp = Opt("--data-path");
        if (!string.IsNullOrEmpty(dp)) Config.Instance.MainPath = dp;
        var region = Opt("--region");
        if (region == "global") Config.Instance.Region = Region.Global;
        else if (region == "jp") Config.Instance.Region = Region.Jp;
        // Force the managed CySpring solver (the C# port) instead of the native plugin.
        if (Flag("--managed-physics")) Gallop.CySpringNative.isNative = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (!Flag("--export")) return;
        var go = new GameObject("CliExporter");
        DontDestroyOnLoad(go);
        go.AddComponent<CliExporter>().StartCoroutine(Run());
    }

    static IEnumerator Run()
    {
        string charaId = Opt("--chara");
        string costume = Opt("--costume", "00");
        string animId = Opt("--anim");
        string outDir = Path.GetFullPath(Opt("--out", "export"));
        // Optional: record this many seconds raw (no loop trimming) instead of one
        // period, e.g. to capture several strides of a run for external loop-finding.
        float.TryParse(Opt("--seconds", "0"), out float recordSeconds);
        bool addBlink = Flag("--blink");    // inject a periodic まばたき (blink) track
        bool dropMouth = Flag("--no-mouth"); // strip mouth vowel morphs (drive them live)
        bool noModel = Flag("--no-model");   // skip the PMX+textures, export only the VMD
        // --bake-physics: record the CySpring-simulated spring bones as VMD tracks and write
        // the PMX without rigid bodies, instead of shipping bodies for a runtime to simulate.
        // CySpring is a Verlet solver with a hard length constraint; PMX physics is rigid
        // bodies and 6DOF springs, so the runtime can only approximate it. Baking ships the
        // solver's own output. The cost is that the cloth no longer reacts at runtime.
        bool bakePhysics = Flag("--bake-physics");

        yield return null; // let scene Awake/Start run

        // Wait for the database (Characters list is built once meta is read).
        float t = 0f;
        while ((UmaViewerMain.Instance == null || UmaViewerMain.Instance.Characters.Count == 0) && t < 60f)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        var main = UmaViewerMain.Instance;
        if (main == null || main.Characters.Count == 0) { Fail("database not ready (check --data-path / --region)"); yield break; }

        var chara = main.Characters.FirstOrDefault(c => c.Id.ToString() == charaId);
        if (chara == null) { Fail($"character id '{charaId}' not found ({main.Characters.Count} known)"); yield break; }

        Directory.CreateDirectory(outDir);

        Exception buildErr = null;
        yield return RunSafe(UmaViewerBuilder.Instance.LoadUma(chara, costume, false, ""), e => buildErr = e);
        if (buildErr != null) { Fail("LoadUma threw: " + buildErr); yield break; }
        var container = UmaViewerBuilder.Instance.CurrentUMAContainer;
        if (container == null) { Fail("character build produced no container"); yield break; }
        yield return null;

        // --physics-ref <file.json>: record the CySpring-simulated Sp_* bone rotations per
        // frame (physics ON) to JSON, for comparing/calibrating against the exported PMX
        // physics or baking the exact motion. Run on Windows for the real CySpring.dll.
        // --spring-dump <file.json>: write the raw CySpring per-bone parameters. The solver
        // scales them (StiffnessForce/1000, DragForce/100, Gravity/10000; CySpringPlugin.dll
        // constants at 0x18008ec8c, 0x18008ec80, 0x18008ec90), so the raw ranges decide how they
        // map onto PMX.
        string dumpPath = Opt("--spring-dump");
        if (!string.IsNullOrEmpty(dumpPath))
        {
            var sbp = new StringBuilder();
            var ciD = CultureInfo.InvariantCulture;
            sbp.Append("[");
            bool first = true;
            foreach (var c in container.cySpringDataContainers)
            {
                if (c == null || c.springParam == null) continue;
                foreach (var e in c.springParam)
                {
                    if (e == null || string.IsNullOrEmpty(e.BoneName)) continue;
                    if (!first) sbp.Append(",");
                    first = false;
                    sbp.AppendFormat(ciD, "{{\"n\":\"{0}\",\"root\":1,\"stiff\":{1},\"drag\":{2},\"grav\":{3},\"rad\":{4},\"lim\":{5}}}",
                        e.BoneName, e.StiffnessForce, e.DragForce, e.Gravity, e.CollisionRadius, e._isLimit ? 1 : 0);
                    if (e._childElements == null) continue;
                    foreach (var ce in e._childElements)
                    {
                        if (ce == null || string.IsNullOrEmpty(ce.Name)) continue;
                        sbp.AppendFormat(ciD, ",{{\"n\":\"{0}\",\"root\":0,\"stiff\":{1},\"drag\":{2},\"grav\":{3},\"rad\":{4},\"lim\":{5}}}",
                            ce.Name, ce.StiffnessForce, ce.DragForce, ce.Gravity, ce.CollisionRadius, ce.IsLimit ? 1 : 0);
                    }
                }
            }
            sbp.Append("]");
            try { File.WriteAllText(dumpPath, sbp.ToString()); }
            catch (Exception ex) { Fail("spring-dump threw: " + ex); yield break; }
            Debug.Log("CLI_EXPORT: wrote spring dump " + dumpPath);
            Quit(0); yield break;
        }

        // --list-anims <substring>: print the motion assets whose name contains the substring.
        // Asset names are not guessable -- a character often has both a generic type00 motion
        // and its own chr<id> variant, and picking the wrong one exports a different animation
        // than the viewer shows.
        string listPat = Opt("--list-anims");
        if (!string.IsNullOrEmpty(listPat))
        {
            var hits = main.AbMotions.Where(e => e.Name.Contains(listPat)).Select(e => e.Name).OrderBy(n => n).ToList();
            Debug.Log($"CLI_EXPORT: {hits.Count} motion(s) containing '{listPat}'");
            foreach (var n in hits) Debug.Log("CLI_ANIM " + n);
            Quit(0); yield break;
        }

        // --solver-diff <file.txt>: single-step differential of CySpringSolver against
        // CySpringPlugin.dll (see CySpringDiff). Windows only; forces the native path.
        string diffPath = Opt("--solver-diff");
        if (!string.IsNullOrEmpty(diffPath))
        {
            if (string.IsNullOrEmpty(animId)) { Fail("--solver-diff needs --anim"); yield break; }
            Gallop.CySpringNative.isNative = true;
            Gallop.CySpringDiff.Reset();
            Gallop.CySpringSolver.ResetCounters();
            float.TryParse(Opt("--solver-diff-collide", "1"), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float collideScale);
            Gallop.CySpringDiff.CollideScale = collideScale;
            Gallop.CySpringDiff.CollideMode = Opt("--solver-diff-collide-mode", "");
            if (collideScale != 1f) Debug.Log($"CLI_EXPORT: solver-diff scaling collider radii by {collideScale}");
            Gallop.CySpringDiff.Armed = true;
            yield return RunSafe(RecordPhysicsRef(container, main, animId, diffPath + ".ref.json"), e => buildErr = e);
            Gallop.CySpringDiff.Armed = false;
            if (buildErr != null) { Fail("solver-diff threw: " + buildErr); yield break; }
            Debug.Log($"CLI_BRANCH clampCalls={Gallop.CySpringSolver.ClampCalls} clampBites={Gallop.CySpringSolver.ClampBites} collCalls={Gallop.CySpringSolver.CollisionCalls} collHits={Gallop.CySpringSolver.CollisionHits} sphere={Gallop.CySpringSolver.HitSphere} capMid={Gallop.CySpringSolver.HitCapsuleMid} capEnd={Gallop.CySpringSolver.HitCapsuleEnd} plane={Gallop.CySpringSolver.HitPlane} inner={Gallop.CySpringSolver.SeenInner} skipChara={Gallop.CySpringSolver.SkipChara} skirtKnee={Gallop.CySpringSolver.SkirtKneeHits}");
            Debug.Log("CLI_COLL types=" + string.Join(",", Gallop.CySpringSolver.TypeSeen) + " disabled=" + Gallop.CySpringSolver.SkipDisabled);
            Gallop.CySpringDiff.Write(diffPath);
            Debug.Log("DIFF" + System.Environment.NewLine + Gallop.CySpringDiff.Report());
            Debug.Log("CLI_EXPORT_DONE " + diffPath);
            Quit(0); yield break;
        }

        // --solver-trace <file.csv>: per-call solver state, inputs and outputs, for either
        // solver (see CySpringTrace).
        string tracePath = Opt("--solver-trace");

        string physRefPath = Opt("--physics-ref");
        if (!string.IsNullOrEmpty(physRefPath))
        {
            if (!string.IsNullOrEmpty(tracePath)) { Gallop.CySpringTrace.Reset(); Gallop.CySpringTrace.Armed = true; }
            yield return RunSafe(RecordPhysicsRef(container, main, animId, physRefPath), e => buildErr = e);
            if (buildErr != null) { Fail("physics-ref threw: " + buildErr); yield break; }
            if (!string.IsNullOrEmpty(tracePath)) { Gallop.CySpringTrace.Armed = false; Gallop.CySpringTrace.Write(tracePath); }
            Debug.Log($"CLI_BRANCH clampCalls={Gallop.CySpringSolver.ClampCalls} clampBites={Gallop.CySpringSolver.ClampBites} collCalls={Gallop.CySpringSolver.CollisionCalls} collHits={Gallop.CySpringSolver.CollisionHits} sphere={Gallop.CySpringSolver.HitSphere} capMid={Gallop.CySpringSolver.HitCapsuleMid} capEnd={Gallop.CySpringSolver.HitCapsuleEnd} plane={Gallop.CySpringSolver.HitPlane} inner={Gallop.CySpringSolver.SeenInner} skipChara={Gallop.CySpringSolver.SkipChara} skirtKnee={Gallop.CySpringSolver.SkirtKneeHits}");
            Debug.Log("CLI_EXPORT_DONE " + physRefPath);
            Quit(0); yield break;
        }

        if (noModel && string.IsNullOrEmpty(animId)) { Fail("--no-model needs --anim (nothing to export)"); yield break; }

        // --- PMX (+ textures) --- unless --no-model (reuse a previously exported model)
        // --pmx-name / --vmd-name override the generated names; the extension is optional.
        string pmxPath = Path.Combine(outDir, WithExtension(Opt("--pmx-name"), $"chr{charaId}_{costume}", ".pmx"));
        if (!noModel)
        {
            ModelExporter.BakeSpringBones = bakePhysics;
            try { ModelExporter.ExportModel(container, pmxPath); }
            catch (Exception ex) { Fail("PMX export threw: " + ex); yield break; }
            Debug.Log("CLI_EXPORT: wrote " + pmxPath);
        }

        // Spring bones and the pose the PMX was written in. ExportModel disables physics and
        // leaves the model at rest, which is exactly the bind pose a VMD rotation is relative
        // to, so capture it here -- by the time the recorder exists the animation has moved on.
        var springBind = new Dictionary<string, Quaternion>();
        var springXf = new Dictionary<string, Transform>();
        if (bakePhysics)
        {
            foreach (var tr in container.GetComponentsInChildren<Transform>(true))
            {
                if (!SpringBoneNames.IsSpringBone(tr.name) || springXf.ContainsKey(tr.name)) continue;
                springXf[tr.name] = tr;
                springBind[tr.name] = tr.localRotation;
            }
            // Physics is off after the PMX export; baking needs it running to record.
            container.EnablePhysics = true;
            container.SetDynamicBoneEnable(true);
            Debug.Log($"CLI_EXPORT: baking {springXf.Count} spring bones (PMX written without rigid bodies)");
        }

        // --- VMD (one animation) ---
        if (!string.IsNullOrEmpty(animId))
        {
            var anim = main.AbMotions.FirstOrDefault(e => e.Name == animId)
                    ?? main.AbMotions.FirstOrDefault(e => e.Name.Contains(animId));
            if (anim != null && anim.Name != animId)
            {
                // A substring match is a guess. Say so loudly: it silently exports a different
                // animation than the one asked for, which looks like a version mismatch later.
                int alts = main.AbMotions.Count(e => e.Name.Contains(animId));
                Debug.LogWarning($"CLI_EXPORT: '{animId}' is not an exact asset name; using '{anim.Name}'"
                               + (alts > 1 ? $" -- {alts} assets match, see --list-anims" : ""));
            }
            if (anim != null) Debug.Log("CLI_EXPORT: motion asset = " + anim.Name);
            if (anim == null) { Fail($"animation '{animId}' not found"); yield break; }

            AnimationClip clip = null;
            try { clip = anim.Get<AnimationClip>(); } catch { }
            container.LoadAnimation(anim);
            // LoadAnimation files _s/_e clips into the transition slots without playing them:
            // in the viewer they only run inside a loop's chain (idle _e -> clip _s -> loop).
            // Recording one on its own plays it directly, below, once the recorder is running.
            bool playDirect = clip != null && (clip.name.EndsWith("_s") || clip.name.EndsWith("_e"));
            // Head/eye look-at (FinalIK) aims at a camera-following target; headless
            // that target is arbitrary and won't loop, so the head pops. Export the
            // raw animation and let the consumer add look-at themselves.
            container.SetHeadTracking(false);
            container.EnableEyeTracking = false;
            float len = (clip != null && clip.length > 0.01f) ? clip.length : 5f;
            bool isLoop = animId.Contains("loop") || (clip != null && clip.name.Contains("loop"));
            Debug.Log($"CLI_CLIP: {anim.Name} length={len:F3}s ({Mathf.RoundToInt(len * 30f)} frames at 30fps) loop={isLoop}");

            // Deterministic capture: lock game time to a fixed 1/30 step (frame count and
            // pose sampling no longer depend on wall-clock speed), and force the job system
            // single-threaded so parallel animation/physics FP reductions are bit-stable
            // (their thread-order otherwise jitters poses ~0.3 deg run to run). Restored after.
            int prevCapture = Time.captureFramerate;
            float prevFixed = Time.fixedDeltaTime;
            int prevWorkers = Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount;
            // CySpring's constants are per-step, so its output depends strongly on the rate
            // it is stepped at: the same run baked at 30 instead of the game's 60 moves the
            // tail bones ~46 deg on average. Render at that rate so FixedUpdate, and with it
            // SaveFrame and the simulation, run at it too, then drop the extra frames when
            // writing (WriteStride) so the file is still a 30fps track. Note the recorder's
            // Initialize() assigns Time.fixedDeltaTime itself, so rec.FixedStep has to carry
            // the rate rather than setting Time.fixedDeltaTime before it.
            int.TryParse(Opt("--physics-fps", "60"), out int recFps);
            if (recFps != 60) recFps = 30;
            Time.captureFramerate = recFps;
            Time.fixedDeltaTime = 1f / recFps;
            var springCtrl = container.GetComponentInChildren<Gallop.CySpringController>(true);
            bool prevSpringMode = springCtrl != null && springCtrl.Is60FpsMode;
            if (springCtrl != null) springCtrl.Is60FpsMode = (recFps == 60);
            Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount = 0;

            // For a loop, let the crossfade finish and the motion settle so frame 0
            // matches where the loop ends (warming whole periods also starts capture
            // at the clip's own phase 0); otherwise the bind->idle blend makes the
            // seam pop. Non-loop clips are recorded from the start.
            if (isLoop)
            {
                // --warmup N: settle for N whole animation periods before capturing. The
                // skeleton is periodic from the first loop, but the spring chains are damped
                // oscillators driven by it and need several periods to fall into the orbit
                // that repeats with the animation. Too few and the recorded cloth does not
                // close: frame 0 and frame P disagree, which shows up once per tile.
                // Default 2 is the long-standing behaviour and measurement says it is enough:
                // sweeping 2/4/8/16/32 periods moved the cloth wrap by under half a degree,
                // so the chains have already settled by two. The knob exists for diagnosis.
                float.TryParse(Opt("--warmup", "2"), out float warmPeriods);
                if (warmPeriods < 1f) warmPeriods = 1f;
                float warm = 0f, warmTarget = Mathf.Max(1.5f, len * warmPeriods);
                Debug.Log($"CLI_EXPORT: warming {warmPeriods:0.#} periods ({warmTarget:0.00}s) before capture");
                while (warm < warmTarget) { warm += Time.deltaTime; yield return null; }

                // Phase alignment. LoadAnimation plays the previous clip's _e and this clip's _s
                // ahead of the loop, which shifts the loop's phase against wall time, so the
                // warm-up alone does not land on phase 0. Wait until the loop itself is playing,
                // then until it is alignLead frames from wrapping: the first kept VMD frame
                // (recorded frame 2, after the transient frame 0 is dropped and 60fps frames
                // are written in pairs) is captured that many frames after this loop ends
                // (measured: CLI_PHASE below reports where it landed).
                var an = container.UmaAnimator;
                int.TryParse(Opt("--align-lead", "3"), out int alignLead);
                float step = Time.deltaTime / len;
                for (int guard = 0; guard < 100000; guard++)
                {
                    var infos = an.GetCurrentAnimatorClipInfo(0);
                    bool inLoop = !an.IsInTransition(0) && infos.Length > 0 && infos[0].clip != null
                                  && infos[0].clip.name.EndsWith(anim.Name.Substring(anim.Name.LastIndexOf('/') + 1));
                    if (inLoop)
                    {
                        float frac = an.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;
                        if (Mathf.Abs(frac + alignLead * step - 1f) <= step * 0.5f + 1e-5f) break;
                    }
                    yield return null;
                }
            }

            var rootbone = container.transform.Find("Position");
            var rec = rootbone.gameObject.AddComponent<UnityHumanoidVMDRecorder>();
            rec.FixedStep = 1f / recFps;     // Initialize() applies it; it hardcodes 1/30 otherwise
            rec.KeyReductionLevel = 1;       // every frame is a real sample; the default 2 keys the body at 15fps
            rec.Initialize();
            rec.WriteStride = recFps / 30;   // 60fps steps -> one 30fps VMD frame per two
            Debug.Log($"CLI_EXPORT: stepping physics at {recFps}fps, writing every {rec.WriteStride} frame(s)");
            if (bakePhysics)
            {
                // Short names so each track fits the VMD's 15-byte field and matches the PMX
                // primary name ModelExporter wrote for the same bone.
                var shortNames = SpringBoneNames.BuildMap(springXf.Keys);
                foreach (var kv in shortNames)
                    rec.AddExtraBone(kv.Value, springXf[kv.Key], springBind[kv.Key]);
                Debug.Log($"CLI_EXPORT: registered {rec.ExtraBoneCount} baked spring tracks");
            }
            yield return null;            // one frame so the animator is posed at t=0
            {
                // Which animator state the capture actually starts in. LoadAnimation chains the
                // previous clip's _e and this clip's _s ahead of the loop, so a time-based
                // warm-up can still be inside those.
                var an = container.UmaAnimator; var st = an.GetCurrentAnimatorStateInfo(0);
                string stName = "?";
                foreach (var cand in new[] { "motion_1", "motion_2", "motion_s", "motion_e", "motion_t", "motion_p" })
                    if (st.IsName(cand)) stName = cand;
                string Clip(string k) { var c = container.OverrideController[k]; return c == null ? "-" : $"{c.name.Substring(c.name.LastIndexOf('/') + 1)}[{c.length:F2}s]"; }
                Debug.Log($"CLI_STATE: capture starts in {stName} t={st.normalizedTime:F2} len={st.length:F2}s transition={an.IsInTransition(0)} clip_1={Clip("clip_1")} clip_2={Clip("clip_2")} clip_s={Clip("clip_s")} clip_e={Clip("clip_e")}");
            }
            rec.StartRecording();
            // Capture a full period plus a small margin so frame P (== phase 0 of the
            // next cycle) is present; the loop trim below keeps exactly one period.
            // --seconds overrides this to record raw for external loop-finding.
            float recLen = recordSeconds > 0f ? recordSeconds : (isLoop ? len + 0.1f : len);
            if (playDirect) recLen += 2f * Time.deltaTime;
            float e2 = 0f;
            for (int i = 0; e2 < recLen; i++)
            {
                // Recorded frame k is captured in the FixedUpdate before iteration k+1 and the
                // first kept VMD frame is recorded frame 2, so a clip started here, at
                // iteration 2, has its frame 0 captured as that first kept frame.
                if (playDirect && i == 2)
                {
                    container.OverrideController["clip_2"] = clip;
                    container.UmaAnimator.Play("motion_2", 0, 0);
                }
                if (i == 3)   // recorded frame 2 (the first kept VMD frame) is captured in the FixedUpdate before this iteration
                    Debug.Log($"CLI_PHASE: first kept frame at clip phase {container.UmaAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f:F4}");
                e2 += Time.deltaTime; yield return null;
            }
            rec.StopRecording();
            Time.captureFramerate = prevCapture;
            Time.fixedDeltaTime = prevFixed;
            if (springCtrl != null) springCtrl.Is60FpsMode = prevSpringMode;
            Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount = prevWorkers;
            // Defaults to the model's stem so a model/motion pair stays matched.
            string vmdPath = Path.Combine(outDir, WithExtension(Opt("--vmd-name"), Path.GetFileNameWithoutExtension(pmxPath), ".vmd"));
            try { rec.SaveVMD(Path.GetFileNameWithoutExtension(pmxPath), vmdPath); }
            catch (Exception ex) { Fail("VMD save threw: " + ex); yield break; }
            // Drop the recorder's transient first frame (a stale pose at capture start).
            // For a loop, keep exactly frames 1..P (P = one period) so the seam is a
            // single step regardless of how fast the motion is; the wrap is phase0<-phaseP-1.
            // With --seconds we keep everything (raw multi-period capture).
            int period = (recordSeconds > 0f || !isLoop) ? int.MaxValue : Mathf.RoundToInt(len * 30f);
            try { TrimLoop(vmdPath, period, dropMouth, addBlink); }
            catch (Exception ex) { Debug.LogWarning("CLI_EXPORT: loop trim skipped: " + ex); }
            Debug.Log("CLI_EXPORT: wrote " + vmdPath);
        }

        Debug.Log("CLI_EXPORT_DONE " + outDir);
        Quit(0);
    }

    // Standard MMD mouth-shape morphs, dropped by --no-mouth so a viewer can drive
    // lip-sync live. Expression morphs (笑い/怒り/まばたき…) are kept.
    static readonly string[] MouthMorphs =
        { "あ", "い", "う", "え", "お", "あ2", "い2", "う2", "え2", "お2", "▲", "□" };

    // Drop the transient frame 0 and keep frames 1..period, renumbered to 0..period-1,
    // for the bone and morph sections; camera/light/shadow/IK sections are copied as-is.
    // A non-loop clip passes period=int.MaxValue (keep every frame after 0).
    // dropMouth removes mouth morphs; addBlink replaces any blink track with one periodic
    // blink over the loop.
    static void TrimLoop(string path, int period, bool dropMouth, bool addBlink)
    {
        var enc = ShiftJisOrUtf8();
        byte[] d = File.ReadAllBytes(path);
        var outp = new List<byte>(d.Length);
        outp.AddRange(new ArraySegment<byte>(d, 0, 50)); // header(30) + model name(20)
        int o = 50;
        int maxFrame = 0;
        o = TrimBones(d, o, period, outp, ref maxFrame);
        o = TrimMorphs(d, o, period, dropMouth, addBlink, maxFrame, enc, outp);
        outp.AddRange(new ArraySegment<byte>(d, o, d.Length - o)); // remaining sections
        File.WriteAllBytes(path, outp.ToArray());
    }

    static System.Text.Encoding ShiftJisOrUtf8()
    {
        try { return System.Text.Encoding.GetEncoding("shift_jis"); }
        catch { return System.Text.Encoding.UTF8; }
    }

    // Bone section (111-byte records, frame at offset 15): keep 1<=frame<=period,
    // renumber frame-1; report the highest kept (renumbered) frame.
    static int TrimBones(byte[] d, int o, int period, List<byte> outp, ref int maxFrame)
    {
        int count = BitConverter.ToInt32(d, o); o += 4;
        var kept = new List<byte[]>(count);
        for (int i = 0; i < count; i++, o += 111)
        {
            uint fr = BitConverter.ToUInt32(d, o + 15);
            if (fr < 1 || fr > (uint)period) continue;
            var r = new byte[111];
            Array.Copy(d, o, r, 0, 111);
            uint nf = fr - 1;
            Array.Copy(BitConverter.GetBytes(nf), 0, r, 15, 4);
            if (nf > (uint)maxFrame) maxFrame = (int)nf;
            kept.Add(r);
        }
        outp.AddRange(BitConverter.GetBytes(kept.Count));
        foreach (var r in kept) outp.AddRange(r);
        return o;
    }

    // Morph section (23-byte records: 15-byte name, 4-byte frame, 4-byte weight). Same
    // keep/renumber as bones, minus mouth morphs (dropMouth) and any existing blink when
    // addBlink is set; a synthesized periodic blink is then appended.
    static int TrimMorphs(byte[] d, int o, int period, bool dropMouth, bool addBlink,
                          int maxFrame, System.Text.Encoding enc, List<byte> outp)
    {
        byte[] blinkName = enc.GetBytes("まばたき");
        int count = BitConverter.ToInt32(d, o); o += 4;
        var kept = new List<byte[]>(count);
        for (int i = 0; i < count; i++, o += 23)
        {
            string name = NameAt(d, o, enc);
            uint fr = BitConverter.ToUInt32(d, o + 15);
            if (fr < 1 || fr > (uint)period) continue;
            if (dropMouth && Array.IndexOf(MouthMorphs, name) >= 0) continue;
            if (addBlink && name == "まばたき") continue;
            var r = new byte[23];
            Array.Copy(d, o, r, 0, 23);
            Array.Copy(BitConverter.GetBytes(fr - 1), 0, r, 15, 4);
            kept.Add(r);
        }
        if (addBlink)
            foreach (var (f, w) in BlinkKeys(maxFrame))
                kept.Add(MorphRecord(blinkName, f, w));
        outp.AddRange(BitConverter.GetBytes(kept.Count));
        foreach (var r in kept) outp.AddRange(r);
        return o;
    }

    static string NameAt(byte[] d, int o, System.Text.Encoding enc)
    {
        int n = 0; while (n < 15 && d[o + n] != 0) n++;
        return enc.GetString(d, o, n);
    }

    // One 0->1->0 blink at the loop midpoint (clamped for very short loops).
    static (int, float)[] BlinkKeys(int maxFrame)
    {
        if (maxFrame < 6) return new[] { (0, 0f), (Math.Max(1, maxFrame / 2), 1f), (maxFrame, 0f) };
        int c = maxFrame / 2;
        return new[] { (0, 0f), (c - 2, 0f), (c, 1f), (c + 3, 0f), (maxFrame, 0f) };
    }

    static byte[] MorphRecord(byte[] name, int frame, float weight)
    {
        var r = new byte[23];
        Array.Copy(name, 0, r, 0, Math.Min(name.Length, 15));
        Array.Copy(BitConverter.GetBytes((uint)frame), 0, r, 15, 4);
        Array.Copy(BitConverter.GetBytes(weight), 0, r, 19, 4);
        return r;
    }

    // Record CySpring-simulated Sp_* bone local rotations per frame to JSON. Captures at
    // end-of-frame (after the container's LateUpdate runs CySpring), at a fixed 1/30 step.
    static IEnumerator RecordPhysicsRef(UmaContainerCharacter container, UmaViewerMain main, string animId, string outPath)
    {
        if (string.IsNullOrEmpty(animId)) { Debug.LogError("CLI_EXPORT_FAIL: --physics-ref needs --anim"); yield break; }
        var anim = main.AbMotions.FirstOrDefault(e => e.Name == animId) ?? main.AbMotions.FirstOrDefault(e => e.Name.Contains(animId));
        if (anim == null) { Debug.LogError("CLI_EXPORT_FAIL: animation not found: " + animId); yield break; }
        AnimationClip clip = null; try { clip = anim.Get<AnimationClip>(); } catch { }
        float len = (clip != null && clip.length > 0.01f) ? clip.length : 5f;

        container.SetDynamicBoneEnable(true);   // ensure CySpring physics is running
        container.LoadAnimation(anim);

        // --physics-fps <30|60>: step rate for the capture. CySpringController hardcodes its
        // fps mode to 60 and never reads the config, so a 30fps capture tells the solver 60
        // while stepping it at 1/30. Keep the two in agreement, and sample every other frame
        // when stepping at 60 so the output stays a 30fps track either way.
        // Default 60: that is what the game runs at, and CySpring's per-step constants make
        // its output strongly rate-dependent -- stepping the same motion at 30 instead moves
        // the tail bones by ~50 degrees on average.
        int.TryParse(Opt("--physics-fps", "60"), out int physFps);
        if (physFps != 60) physFps = 30;
        int stride = physFps / 30;

        int prevCapture = Time.captureFramerate; float prevFixed = Time.fixedDeltaTime;
        Time.captureFramerate = physFps; Time.fixedDeltaTime = 1f / physFps;
        var ctrl = container.GetComponentInChildren<Gallop.CySpringController>(true);
        bool prevMode = ctrl != null && ctrl.Is60FpsMode;
        if (ctrl != null) ctrl.Is60FpsMode = (physFps == 60);
        Debug.Log($"CLI_EXPORT: physics-ref stepping at {physFps}fps, solver 60fps-mode={(ctrl != null ? ctrl.Is60FpsMode.ToString() : "n/a")}");

        // --warmup <periods>: settle time before recording, in clip lengths. 0 records from
        // the bind pose.
        float.TryParse(Opt("--warmup", "2"), out float warmP);
        float warm = 0f, warmTarget = Mathf.Max(warmP <= 0f ? 0f : 1.5f, len * warmP);
        while (warm < warmTarget) { warm += Time.deltaTime; yield return null; }

        var sp = container.GetComponentsInChildren<Transform>(true).Where(t => t.name.StartsWith("Sp_")).ToList();
        int nframes = Mathf.Max(1, Mathf.RoundToInt(len * 30f));
        var ci = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        sb.Append("{\"fps\":30,\"frames\":").Append(nframes).Append(",\"bones\":[");
        sb.Append(string.Join(",", sp.Select(t => "\"" + t.name + "\"")));
        sb.Append("],\"rot\":[");
        for (int f = 0; f < nframes; f++)
        {
            // Next Update: bones hold the previous frame's LateUpdate (CySpring) result.
            // (WaitForEndOfFrame never fires in batchmode, so it can't be used here.)
            for (int s2 = 0; s2 < stride; s2++) yield return null;
            if (f > 0) sb.Append(",");
            sb.Append("[");
            for (int i = 0; i < sp.Count; i++)
            {
                var q = sp[i].localRotation;
                if (i > 0) sb.Append(",");
                sb.Append("[").Append(q.x.ToString(ci)).Append(",").Append(q.y.ToString(ci)).Append(",")
                  .Append(q.z.ToString(ci)).Append(",").Append(q.w.ToString(ci)).Append("]");
            }
            sb.Append("]");
        }
        sb.Append("]}");
        Time.captureFramerate = prevCapture; Time.fixedDeltaTime = prevFixed;
        if (ctrl != null) ctrl.Is60FpsMode = prevMode;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath)));
        File.WriteAllText(outPath, sb.ToString());
        Debug.Log($"CLI_EXPORT: wrote physics ref ({sp.Count} bones x {nframes} frames) " + outPath);
    }

    // Run a coroutine to completion, capturing any exception (so we can fail cleanly).
    static IEnumerator RunSafe(IEnumerator inner, Action<Exception> onError)
    {
        while (true)
        {
            object cur;
            try { if (!inner.MoveNext()) yield break; cur = inner.Current; }
            catch (Exception ex) { onError(ex); yield break; }
            yield return cur;
        }
    }

    // Pick an output filename: the caller's if given, else the generated fallback. The
    // extension is optional ("model" and "model.pmx" both work). Any directory part is
    // dropped so --out stays the only thing deciding where files land.
    static string WithExtension(string name, string fallbackStem, string ext)
    {
        string file = string.IsNullOrWhiteSpace(name) ? fallbackStem : Path.GetFileName(name.Trim());
        if (string.IsNullOrEmpty(file)) file = fallbackStem;
        if (!file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) file += ext;
        return file;
    }

    static void Fail(string msg) { Debug.LogError("CLI_EXPORT_FAIL: " + msg); Quit(1); }

    static void Quit(int code)
    {
        Debug.Log("CLI_EXPORT quitting code=" + code);
        Application.Quit(code);
    }
}
