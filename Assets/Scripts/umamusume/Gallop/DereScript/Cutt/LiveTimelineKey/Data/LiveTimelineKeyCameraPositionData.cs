using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyCameraPositionData : LiveTimelineKeyWithInterpolate
    {
        public enum CullingLayer
        {
            TransparentFX = 1,
            Background3D_NotReflect = 2,
            Background3d = 4,
            Character3d = 8,
            Character3d_0 = 0x10,
            Character3d_1 = 0x20,
            Character3d_2 = 0x40,
            Character3d_3 = 0x80,
            Character3d_4 = 0x100,
            Character3D_NotReflect = 0x200,
            Background3D_Other = 0x400
        }

        public enum CharacterLOD
        {
            Outline_0 = 1,
            Outline_1 = 2,
            Outline_2 = 4,
            Outline_3 = 8,
            Outline_4 = 0x10,
            Outline_5 = 0x20,
            Outline_6 = 0x40,
            Outline_7 = 0x80,
            Outline_8 = 0x100,
            Outline_9 = 0x200,
            Shader_0 = 0x8000,
            Shader_1 = 0x10000,
            Shader_2 = 0x20000,
            Shader_3 = 0x40000,
            Shader_4 = 0x80000,
            Shader_5 = 0x100000,
            Shader_6 = 0x200000,
            Shader_7 = 0x400000,
            Shader_8 = 0x800000,
            Shader_9 = 0x1000000
        }

        public static readonly float defaultNearClip = 1f;

        public static readonly float defaultFarClip = 100f;

        public static readonly float minClipValue = 0.001f;

        protected const CullingLayer allCullingLayer = (CullingLayer)0x7FF;

        protected const CullingLayer defCameraCullingLayer = (CullingLayer)0x7FE;

        protected const CharacterLOD outlineLODMask = (CharacterLOD)0x3FF;

        protected const CharacterLOD shaderLODMask = (CharacterLOD)0x1FF8000;

        public LiveCameraPositionType setType = LiveCameraPositionType.Direct;

        public Vector3 posDirect = Vector3.zero;

        [NonSerialized]
        public Transform posTransform;

        public string posTransformName = "";

        public Vector3 offset = Vector3.zero;

        public Vector3[] bezierPoints;

        public LiveCharaPositionFlag charaRelativeBase = LiveCharaPositionFlag.Center;

        public LiveCameraLookAtCharaParts charaRelativeParts = LiveCameraLookAtCharaParts.ConstHeightFace;

        public float traceSpeed = 0.1f;

        public float nearClip = defaultNearClip;

        public float farClip = defaultFarClip;

        public float outlineZOffset = 1f;

        public CullingLayer cullingMask = defCameraCullingLayer;

        public CharacterLOD characterLODMask = (CharacterLOD)((uint)outlineLODMask + (uint)shaderLODMask);

        public bool newBezierCalcMethod;

        public bool necessaryToUseNewBezierCalcMethod
        {
            get
            {
                if (!newBezierCalcMethod)
                {
                    return GetBezierPointCount() > 3;
                }
                return true;
            }
        }
        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.CameraPos;

        protected static int GetCullingMask(CullingLayer layer)
        {
            int num = 257;
            if ((layer & CullingLayer.TransparentFX) != 0)
            {
                num |= 2;
            }
            if ((layer & CullingLayer.Background3D_NotReflect) != 0)
            {
                num |= 0x80000;
            }
            if ((layer & CullingLayer.Background3d) != 0)
            {
                num |= 0x100000;
            }
            if ((layer & CullingLayer.Character3d) != 0)
            {
                num |= 0x200000;
            }
            if ((layer & CullingLayer.Character3d_0) != 0)
            {
                num |= 0x400000;
            }
            if ((layer & CullingLayer.Character3d_1) != 0)
            {
                num |= 0x800000;
            }
            if ((layer & CullingLayer.Character3d_2) != 0)
            {
                num |= 0x1000000;
            }
            if ((layer & CullingLayer.Character3d_3) != 0)
            {
                num |= 0x2000000;
            }
            if ((layer & CullingLayer.Character3d_4) != 0)
            {
                num |= 0x4000000;
            }
            if ((layer & CullingLayer.Character3D_NotReflect) != 0)
            {
                num |= 0x8000000;
            }
            if ((layer & CullingLayer.Background3D_Other) != 0)
            {
                num |= 0x40000;
            }
            return num;

            /*
            uint bittest = (uint)layer;

            uint result = (uint)LayerMask.GetMask("Default");

            if ((bittest & (uint)CullingLayer.TransparentFX) > 0)
                result |= (uint)LayerMask.GetMask("TransparentFX");
            if ((bittest & (uint)CullingLayer.Background3D_NotReflect) > 0)
                result |= (uint)LayerMask.GetMask("background_NotReflect");
            if ((bittest & (uint)CullingLayer.Background3d) > 0)
                result |= (uint)LayerMask.GetMask("background");
            if ((bittest & (uint)CullingLayer.Character3d) > 0)
                result |= (uint)LayerMask.GetMask("charas");
            if ((bittest & (uint)CullingLayer.Character3d_0) > 0)
                result |= (uint)LayerMask.GetMask("chara1");
            if ((bittest & (uint)CullingLayer.Character3d_1) > 0)
                result |= (uint)LayerMask.GetMask("chara2");
            if ((bittest & (uint)CullingLayer.Character3d_2) > 0)
                result |= (uint)LayerMask.GetMask("chara3");
            if ((bittest & (uint)CullingLayer.Character3d_3) > 0)
                result |= (uint)LayerMask.GetMask("chara4");
            if ((bittest & (uint)CullingLayer.Character3d_4) > 0)
                result |= (uint)LayerMask.GetMask("chara5");
            if ((bittest & (uint)CullingLayer.Character3D_NotReflect) > 0)
                result |= (uint)LayerMask.GetMask("otherChara");
            if ((bittest & (uint)CullingLayer.Background3D_Other) > 0)
                result |= (uint)LayerMask.GetMask("background_Other");
            return (int)result;
            */
        }

        public static int GetDefaultCullingMask()
        {
            return GetCullingMask(defCameraCullingLayer);
        }

        public static int GetAllCullingMask()
        {
            return GetCullingMask(allCullingLayer);
        }

        public int GetCullingMask()
        {
            return GetCullingMask(cullingMask);
        }

        public override void OnLoad(LiveTimelineControl timelineControl)
        {
            base.OnLoad(timelineControl);
            if (setType == LiveCameraPositionType.Locator && !string.IsNullOrEmpty(posTransformName))
            {
                posTransform = timelineControl.FindPositionLocator(posTransformName);
            }
        }

        public bool IsDelay()
        {
            return attribute.hasFlag(LiveTimelineKeyAttribute.CameraDelayEnable);
        }

        public bool IsDelayContinuous()
        {
            return attribute.hasFlag(LiveTimelineKeyAttribute.CameraDelayInherit);
        }

        public bool HasBezier()
        {
            if (bezierPoints != null)
            {
                return bezierPoints.Length != 0;
            }
            return false;
        }

        public int GetBezierPointCount()
        {
            if (!HasBezier())
            {
                return 0;
            }
            return bezierPoints.Length;
        }

        public Vector3 GetBezierPoint(int index, LiveTimelineControl timelineControl)
        {
            if (HasBezier() && index < bezierPoints.Length)
            {
                return GetValue(timelineControl) + bezierPoints[index];
            }
            return GetValue(timelineControl) + Vector3.zero;
        }

        public void GetBezierPoints(LiveTimelineControl timelineControl, Vector3[] outPoints, int startIndex)
        {
            if (HasBezier())
            {
                for (int i = 0; i < bezierPoints.Length; i++)
                {
                    outPoints[startIndex + i] = GetValue(timelineControl) + bezierPoints[i];
                }
            }
        }

        public virtual Vector3 GetValue(LiveTimelineControl timelineControl)
        {
            return GetValue(timelineControl, setType, containOffset: true);
        }

        protected virtual Vector3 GetValue(LiveTimelineControl timelineControl, LiveCameraPositionType type, bool containOffset)
        {
            Vector3 vector = Vector3.zero;
            switch (type)
            {
                case LiveCameraPositionType.Direct:
                    vector = posDirect;
                    break;
                case LiveCameraPositionType.Locator:
                    if (posTransform != null)
                    {
                        vector = posTransform.position;
                    }
                    break;
                case LiveCameraPositionType.Character:
                    vector = timelineControl.GetPositionWithCharacters(charaRelativeBase, charaRelativeParts);
                    break;
            }
            if (!containOffset)
            {
                return vector;
            }
            return vector + offset;
        }
    }
}
