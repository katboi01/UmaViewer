using System;
using System.Collections.Generic;
using UnityEngine;

namespace LibMMD.Model
{
    public class Bone
    {
        public class IkLink
        {
            public int LinkIndex { get; set; }
            public bool HasLimit { get; set; }
            public Vector3 LoLimit { get; set; }
            public Vector3 HiLimit { get; set; }

            public static IkLink CopyOf(IkLink ikLink)
            {
                return new IkLink
                {
                    LinkIndex = ikLink.LinkIndex,
                    HasLimit = ikLink.HasLimit,
                    LoLimit = ikLink.LoLimit,
                    HiLimit = ikLink.HiLimit
                };
            }
        }

        public class ChildBone
        {
            public bool ChildUseId { get; set; }
            public Vector3 Offset { get; set; }
            public int Index { get; set; }

            public static ChildBone CopyOf(ChildBone childBone)
            {
                return new ChildBone
                {
                    ChildUseId = childBone.ChildUseId,
                    Offset = childBone.Offset,
                    Index = childBone.Index
                };
            }
        }


        public class AppendBone
        {
            public int Index { get; set; }
            public float Ratio { get; set; }

            public static AppendBone CopyOf(AppendBone appendBone)
            {
                return new AppendBone
                {
                    Index = appendBone.Index,
                    Ratio = appendBone.Ratio
                };
            }
        }

        public class IkInfo
        {
            public int IkTargetIndex { get; set; }
            public int CcdIterateLimit { get; set; }
            public float CcdAngleLimit { get; set; }
            public IkLink[] IkLinks { get; set; }

            public static IkInfo CopyOf(IkInfo ikInfo)
            {
                var ikLinksCopy = new IkLink[ikInfo.IkLinks.Length];
                ikInfo.IkLinks.CopyTo(ikLinksCopy, 0);
                return new IkInfo
                {
                    IkTargetIndex = ikInfo.IkTargetIndex,
                    CcdIterateLimit = ikInfo.CcdIterateLimit,
                    CcdAngleLimit = ikInfo.CcdAngleLimit,
                    IkLinks = ikLinksCopy
                };
            }
        }

        public class LocalAxis
        {
            public Vector3 AxisX { get; set; }
            public Vector3 AxisY { get; set; }
            public Vector3 AxisZ { get; set; }

            public static LocalAxis CopyOf(LocalAxis localAxis)
            {
                return new LocalAxis
                {
                    AxisX = localAxis.AxisX,
                    AxisY = localAxis.AxisY,
                    AxisZ = localAxis.AxisZ
                };
            }
        }

        public Bone()
        {
            ChildBoneVal = new ChildBone();
            AppendBoneVal = new AppendBone();
            LocalAxisVal = new LocalAxis();
        }

        public string Name { get; set; }
        public string NameEn { get; set; }
        public Vector3 Position { get; set; }
        public int ParentIndex { get; set; }
        public int TransformLevel { get; set; }
        public bool Rotatable { get; set; }
        public bool Movable { get; set; }
        public bool Visible { get; set; }
        public bool Controllable { get; set; }
        public bool HasIk { get; set; }
        public bool AppendRotate { get; set; }
        public bool AppendTranslate { get; set; }
        public bool RotAxisFixed { get; set; }
        public bool UseLocalAxis { get; set; }
        public bool PostPhysics { get; set; }
        public bool ReceiveTransform { get; set; }
        public ChildBone ChildBoneVal { get; set; }
        public AppendBone AppendBoneVal { get; set; }
        public Vector3 RotAxis { get; set; }
        public LocalAxis LocalAxisVal { get; set; }
        public int ExportKey { get; set; }
        public IkInfo IkInfoVal { get; set; }

        public static Bone CopyOf(Bone bone)
        {
            return new Bone
            {
                Name = bone.Name,
                NameEn = bone.NameEn,
                Position = bone.Position,
                ParentIndex = bone.ParentIndex,
                TransformLevel = bone.TransformLevel,
                Rotatable = bone.Rotatable,
                Movable = bone.Movable,
                Visible = bone.Visible,
                Controllable = bone.Controllable,
                HasIk = bone.HasIk,
                AppendRotate = bone.AppendRotate,
                AppendTranslate = bone.AppendTranslate,
                RotAxisFixed =  bone.RotAxisFixed,
                UseLocalAxis = bone.UseLocalAxis,
                PostPhysics = bone.PostPhysics,
                ReceiveTransform = bone.ReceiveTransform,
                ChildBoneVal = ChildBone.CopyOf(bone.ChildBoneVal),
                AppendBoneVal = AppendBone.CopyOf(bone.AppendBoneVal),
                RotAxis = bone.RotAxis,
                LocalAxisVal = LocalAxis.CopyOf(bone.LocalAxisVal),
                ExportKey = bone.ExportKey,
                IkInfoVal = IkInfo.CopyOf(bone.IkInfoVal),
            };
        }
        
        
    }
}