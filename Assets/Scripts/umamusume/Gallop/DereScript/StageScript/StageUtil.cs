using Cute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Stage
{
    public class StageUtil
    {
        public static Vector2 GetRenderTargetResolution()
        {
            return new Vector2(Screen.width, Screen.height);
        }


        public static Vector2 GetQuarterResolution()
        {
            Vector2 renderTargetResolution = GetRenderTargetResolution();
            float x = renderTargetResolution.x * 0.25f;
            float y = renderTargetResolution.y * 0.25f;
            return new Vector2(x, y);
        }

        public static Vector2 CalcRTResolution(float fWidth)
        {
            Vector2 renderTargetResolution = GetRenderTargetResolution();
            float num = renderTargetResolution.x / renderTargetResolution.y;
            float y = fWidth / num;
            return new Vector2(fWidth, y);
        }

        public static int LayerToCullingMask(StageLayers layer)
        {
            return 1 << (int)layer;
        }


        public enum StageLayers
        {
            Default = 0,
            TransparentFX = 1,
            IgnoreRaycast = 2,
            Dummy1 = 3,
            Water = 4,
            UI = 5,
            Dummy2 = 6,
            Dummy3 = 7,
            Background = 8,
            BG = 9,
            Dummy4 = 10,
            Room = 11,
            RoomUI = 12,
            MiniGame = 13,
            AR = 14,
            DebugUI = 0xF,
            Dummy6 = 0x10,
            Dummy7 = 17,
            Background3D_Other = 18,
            Background3D_NotReflect = 19,
            Background3d = 20,
            Character3d = 21,
            Character3d_0 = 22,
            Character3d_1 = 23,
            Character3d_2 = 24,
            Character3d_3 = 25,
            Character3d_4 = 26,
            Character3D_NotReflect = 27,
            MultiCameraMask = 28,
            A2U = 29,
            LiveNGUI = 30,
            Loading = 0x1F,
            CharacterRealtimeShadow = 10
        }

        public static int CharaAllLayers()
        {
            int layers = 0;
            layers |= 1 << (int)StageLayers.Character3d;
            layers |= 1 << (int)StageLayers.Character3d_0;
            layers |= 1 << (int)StageLayers.Character3d_1;
            layers |= 1 << (int)StageLayers.Character3d_2;
            layers |= 1 << (int)StageLayers.Character3d_3;
            layers |= 1 << (int)StageLayers.Character3d_4;
            layers |= 1 << (int)StageLayers.Character3D_NotReflect;

            return layers;
            //return 266338304;
        }

        public static int Background3dAllLayers()
        {
            int layers = 0;
            layers |= 1 << (int)StageLayers.Background3d;
            layers |= 1 << (int)StageLayers.Background3D_NotReflect;
            layers |= 1 << (int)StageLayers.Background3D_Other;

            return layers;
            //return 1835008;
        }

        public static int ArLayers()
        {
            int layers = 0;
            layers |= 1 << (int)StageLayers.AR;
            
            return layers;
            //return 16384;
        }

        public static int GetSimpleRarityFromRarity(int rarity, bool isAllowOver = false)
        {
            int result = 1;
            switch (rarity)
            {
                case 1:
                case 2:
                    result = 1;
                    break;
                case 3:
                case 4:
                    result = 2;
                    break;
                case 5:
                case 6:
                    result = 3;
                    break;
                case 7:
                case 8:
                    result = 4;
                    break;
                default:
                    if (isAllowOver)
                    {
                        result = Mathf.CeilToInt((float)rarity / 2f);
                    }
                    break;
            }
            return result;
        }

        public static bool IsModelCommonDressId(int dressId)
        {
            if (dressId / 1000 != 0)
            {
                return false;
            }
            return true;
        }

        public static bool IsModelCommonAccId(int accId)
        {
            if (accId / 1000 != 0)
            {
                return false;
            }
            return true;
        }

        public static int GetHeadIndexWithHeadSelector(Character3DBase.CharacterData charaData, Master3DCharaData master3DCharaData, int songId, bool isCenter, out Character3DBase.eHeadLoadType headLoadType)
        {
            int charaId = charaData.charaId;
            int activeDressId = charaData.activeDressId;
            int num = 0;
            int headIndex = GetHeadIndex(charaId, activeDressId, master3DCharaData, songId, isCenter, out headLoadType);
            int num2 = headIndex;
            Master3DCharaData.HeadSelector headSelector = master3DCharaData.GetHeadSelector(charaId);
            if (headSelector != null)
            {
                Master3DCharaData.HeadSelector.ePriority priority = Master3DCharaData.HeadSelector.ePriority.None;
                num = headSelector.GetSubId(charaData, out priority);
                if (priority == Master3DCharaData.HeadSelector.ePriority.LoadSongFace && headIndex != 0)
                {
                    return headIndex;
                }
            }
            if (num == 0 && num2 != 0)
            {
                num = num2;
            }
            return num;
        }

        /// <summary>
        /// SubID‚ðŽæ“¾‚·‚é
        /// </summary>
        public static int GetHeadIndex(int charaId, int dressId, Master3DCharaData master3DCharaData, int songId, bool isCenter, out Character3DBase.eHeadLoadType headLoadType)
        {
            headLoadType = Character3DBase.eHeadLoadType.Default;
            if (master3DCharaData == null)
            {
                return 0;
            }
            int dressId2 = dressId;
            dressId = ((dressId == 0) ? 1 : dressId);
            if (IsModelCommonDressId(dressId))
            {
                dressId = 0;
            }
            Master3DCharaData.LoadSettingData loadSettingData = default(Master3DCharaData.LoadSettingData);
            Func<int> func = delegate
            {
                if (!loadSettingData.isCenterOnly)
                {
                    return loadSettingData.subId;
                }
                return isCenter ? loadSettingData.subId : 0;
            };
            if (master3DCharaData.GetLoadCharaSongSetting(ref loadSettingData, charaId, dressId2, -1, isCenter))
            {
                headLoadType = Character3DBase.eHeadLoadType.DressModel;
                return func();
            }
            if (master3DCharaData.GetLoadCharaSongSetting(ref loadSettingData, charaId, dressId, songId, isCenter))
            {
                return func();
            }
            return 0;
        }

        public static int GetHeadTextureIndex(int charaId, int dressId, MasterDressColorData masterDressColorData = null)
        {
            int result = 0;
            if (masterDressColorData == null)
            {
                masterDressColorData = GetMasterDressColorData();
            }
            List<MasterDressColorData.DressColorData> listWithCharaIdAndModelTypeOrderByDressIdAsc = masterDressColorData.GetListWithCharaIdAndModelTypeOrderByDressIdAsc(charaId, 1);
            for (int i = 0; i < listWithCharaIdAndModelTypeOrderByDressIdAsc.Count; i++)
            {
                if (dressId == (int)listWithCharaIdAndModelTypeOrderByDressIdAsc[i].dressId)
                {
                    result = listWithCharaIdAndModelTypeOrderByDressIdAsc[i].colorId;
                    break;
                }
            }
            return result;
        }

        private static MasterDressColorData GetMasterDressColorData()
        {
            MasterDressColorData result = null;
            if (MasterDBManager.instance != null && MasterDBManager.instance.masterDressColorData != null)
            {
                result = MasterDBManager.instance.masterDressColorData;
            }
            return result;
        }
    }
}
