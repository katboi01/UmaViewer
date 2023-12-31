using System;
using System.Collections.Generic;
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
    }
}
