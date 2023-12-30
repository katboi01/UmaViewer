// StageA2UManager
using Cutt;
using System.Collections.Generic;

public class StageA2UManager : A2UManager
{
    public struct InitContext
    {
        public LiveTimelineControl timelineControl;

        public int rtWidth;

        public int rtHeight;
    }

    private LiveTimelineControl _timelineControl;

    private A2UController.UpdateContext _context;

    private static readonly string AssetPath = "3D/A2U/a2u_";

    private static readonly string MultiSprite_Prefix = "msp_";

    private bool isEnabledCamera
    {
        get
        {
            return _a2uCamera.enabled;
        }
        set
        {
            _a2uCamera.enabled = value;
        }
    }

    public static string MakeA2UTextureAssetPath(int assetNo, string filename)
    {
        return $"{AssetPath}{assetNo:0000}/{filename}";
    }

    public static string MakeA2UMultiSpriteAssetPath(int assetNo, string filename)
    {
        return $"{AssetPath}{assetNo:0000}/{MultiSprite_Prefix}{filename}";
    }

    public void Init(ref InitContext initContext)
    {
        _timelineControl = initContext.timelineControl;
        _timelineControl.OnUpdateA2UConfig += DoUpdateConfigFromTimeline;
        _timelineControl.OnUpdateA2U += DoUpdateFromTimeline;
        LiveTimelineA2USettings a2uSettings = _timelineControl.data.a2uSettings;
        A2UController.InitContext context = default(A2UController.InitContext);
        context.flickRandomSeed = a2uSettings.flickRandomSeed;
        context.flickCount = a2uSettings.flickCount;
        context.flickStepSec = a2uSettings.flickStepSec;
        context.flickMin = a2uSettings.flickMin;
        context.flickMax = a2uSettings.flickMax;
        context.texturePathList = new string[a2uSettings.sprites.Length];
        context.multiSpritePathList = new string[a2uSettings.sprites.Length];
        for (int i = 0; i < a2uSettings.sprites.Length; i++)
        {
            context.texturePathList[i] = MakeA2UTextureAssetPath(a2uSettings.assetNo, a2uSettings.sprites[i]);
            context.multiSpritePathList[i] = MakeA2UMultiSpriteAssetPath(a2uSettings.assetNo, a2uSettings.sprites[i]);
        }
        context.prefabs = new A2UController.PrefabDesc[a2uSettings.prefabs.Length];
        int j = 0;
        for (int num = a2uSettings.prefabs.Length; j < num; j++)
        {
            context.prefabs[j].name = a2uSettings.prefabs[j].name;
            context.prefabs[j].path = MakeA2UTextureAssetPath(a2uSettings.assetNo, a2uSettings.prefabs[j].path);
        }
        int num2 = context.prefabs.Length;
        int count = _timelineControl.data.GetWorkSheetList().Count;
        List<A2UController.GameObjectDesc> list = new List<A2UController.GameObjectDesc>();
        for (int k = 0; k < count; k++)
        {
            LiveTimelineWorkSheet workSheet = _timelineControl.data.GetWorkSheet(k);
            for (int l = 0; l < workSheet.a2uList.Count; l++)
            {
                bool flag = false;
                A2UController.GameObjectDesc item = default(A2UController.GameObjectDesc);
                item.name = workSheet.a2uList[l].name;
                for (int m = 0; m < num2; m++)
                {
                    if (item.name.Equals(context.prefabs[m].name))
                    {
                        item.prefabPath = context.prefabs[m].path;
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    list.Add(item);
                }
            }
        }
        context.gameObjecs = list.ToArray();
        InitController(ref context);
        InitCamera(initContext.rtWidth, initContext.rtHeight);
        isEnabledCamera = false;
        GameObjectUtility.SetLayer(29, base.gameObject.transform);
    }

    public override void Final()
    {
        if (null != _timelineControl)
        {
            _timelineControl.OnUpdateA2UConfig -= DoUpdateConfigFromTimeline;
            _timelineControl.OnUpdateA2U -= DoUpdateFromTimeline;
        }
        base.Final();
    }

    private void DoUpdateConfigFromTimeline(ref A2UConfigUpdateInfo updateInfo)
    {
        if (isEnabledCamera != updateInfo.enable)
        {
            isEnabledCamera = updateInfo.enable;
        }
        if (isEnabledCamera)
        {
            _a2uCamera.SetBlendMode(updateInfo.blend);
            _a2uCamera.SetRenderingOrder(updateInfo.order);
        }
    }

    private void DoUpdateFromTimeline(ref A2UUpdateInfo updateInfo)
    {
        _context.spriteColor = updateInfo.spriteColor;
        _context.position = updateInfo.position;
        _context.scale = updateInfo.scale;
        _context.rotationZ = updateInfo.rotationZ;
        _context.textureIndex = updateInfo.textureIndex;
        _context.appearanceRandomSeed = updateInfo.appearanceRandomSeed;
        _context.spriteAppearance = updateInfo.spriteAppearance;
        _context.slopeRandomSeed = updateInfo.slopeRandomSeed;
        _context.spriteMinSlope = updateInfo.spriteMinSlope;
        _context.spriteMaxSlope = updateInfo.spriteMaxSlope;
        _context.spriteScale = updateInfo.spriteScale;
        _context.spriteOpacity = updateInfo.spriteOpacity;
        _context.startSec = updateInfo.startSec;
        _context.speed = updateInfo.speed;
        _context.isFlick = updateInfo.isFlick;
        _context.enable = updateInfo.enable;
        _a2uController.UpdateComposition(updateInfo.data.nameHash, ref _context);
    }
}
