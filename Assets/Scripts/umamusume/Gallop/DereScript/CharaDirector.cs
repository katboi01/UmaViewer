using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Stage;
using System;

public class CharaDirector
{
    public class Asset
    {
        public static string GetAccessoryModelPath(int accId, int accIndex, Character3DBase.eResourceQuality resourceQuality, Character3DBase.AccessoryParts.eType type)
        {
            accId = ((accId == 0) ? 1 : accId);
            string empty = string.Empty;
            if (StageUtil.IsModelCommonAccId(accId))
            {
                return GetCommonAccessoryModelPath(accId, accIndex, resourceQuality, type);
            }
            return GetOwnAccessoryModelPath(accId, accIndex, resourceQuality, type);
        }

        private static string GetCommonAccessoryModelPath(int accId, int accIndex, Character3DBase.eResourceQuality resourceQuality, Character3DBase.AccessoryParts.eType type)
        {
            string arg = string.Empty;
            switch (type)
            {
                case Character3DBase.AccessoryParts.eType.Main:
                    arg = ((accIndex > 0) ? $"_{accIndex:00}" : string.Empty);
                    break;
                case Character3DBase.AccessoryParts.eType.Sub:
                    arg = $"_sub{accIndex:00}";
                    break;
            }
            string arg2 = ((resourceQuality == Character3DBase.eResourceQuality.Rich) ? "_hq" : string.Empty);
            return string.Format("3D/Chara/Accessory/acc{0:0000}/Meshes/md_acc{0:0000}{1}{2}", accId, arg, arg2);
        }

        private static string GetOwnAccessoryModelPath(int accId, int index, Character3DBase.eResourceQuality resourceQuality, Character3DBase.AccessoryParts.eType type)
        {
            return string.Empty;
        }

        public static string GetAccessoryTexturePath(int accId, int colorId, bool accTexDiversity, int accTexIndex, Character3DBase.eResourceQuality resourceQuality, Character3DBase.MultiTextures.eMap map)
        {
            string empty = string.Empty;
            accId = ((accId == 0) ? 1 : accId);
            string accessoryTextureSuffix = GetAccessoryTextureSuffix(resourceQuality, map);
            if (StageUtil.IsModelCommonAccId(accId))
            {
                return GetCommonAccessoryTexturePath(accId, colorId, accTexDiversity, accTexIndex, accessoryTextureSuffix);
            }
            return GetOwnAccessoryTexturePath(accId, colorId, accTexDiversity, accTexIndex, accessoryTextureSuffix);
        }

        private static string GetAccessoryTextureSuffix(Character3DBase.eResourceQuality resourceQuality, Character3DBase.MultiTextures.eMap map)
        {
            string result = string.Empty;
            switch (map)
            {
                case Character3DBase.MultiTextures.eMap.Diffuse:
                    if (resourceQuality == Character3DBase.eResourceQuality.Rich)
                    {
                        result = "_hq";
                    }
                    break;
                case Character3DBase.MultiTextures.eMap.Specular:
                    result = "_spec";
                    break;
                case Character3DBase.MultiTextures.eMap.Control:
                    result = "_multi";
                    break;
            }
            return result;
        }

        private static string GetCommonAccessoryTexturePath(int accId, int colorId, bool accTexDiversity, int accTexIndex, string suffix)
        {
            string text = string.Empty;
            if (accTexDiversity)
            {
                text = $"_{accTexIndex:0000}";
            }
            return string.Format("3D/Chara/Accessory/acc{0:0000}/Textures/tx_acc{0:0000}_{1}{2}{3}", accId, colorId, text, suffix);
        }

        private static string GetOwnAccessoryTexturePath(int accId, int colorId, bool accTexDiversity, int accTexIndex, string suffix)
        {
            return string.Empty;
        }

        public static string GetAccessoryParameterPath(int accId)
        {
            _ = string.Empty;
            accId = ((accId == 0) ? 1 : accId);
            if (!StageUtil.IsModelCommonAccId(accId))
            {
                return string.Empty;
            }
            return string.Format("3D/Chara/Accessory/acc{0:0000}/Parameter/prm_acc{0:0000}", accId);
        }

        public static string GetAccessoryClothPath(int accId, int accIndex, Character3DBase.AccessoryParts.eType type)
        {
            accId = ((accId == 0) ? 1 : accId);
            return GetAccessoryClothCollisionPath(accId, accIndex, "_cloth", type);
        }

        public static string GetAccessoryCollisionPath(int accId, int accIndex, Character3DBase.AccessoryParts.eType type)
        {
            accId = ((accId == 0) ? 1 : accId);
            return GetAccessoryClothCollisionPath(accId, accIndex, "_collision", type);
        }

        private static string GetAccessoryClothCollisionPath(int accId, int accIndex, string suffix, Character3DBase.AccessoryParts.eType type)
        {
            string arg = string.Empty;
            switch (type)
            {
                case Character3DBase.AccessoryParts.eType.Main:
                    arg = ((accIndex != 0) ? $"_{accIndex:00}" : string.Empty);
                    break;
                case Character3DBase.AccessoryParts.eType.Sub:
                    arg = ((accIndex != 0) ? $"_sub{accIndex:00}" : string.Empty);
                    break;
            }
            return string.Format("3D/Chara/Accessory/acc{0:0000}/Clothes/md_acc{0:0000}{1}{2}", accId, arg, suffix);
        }

        public static string GetAccessoryFlareColliderPath(int accId, int accIndex, Character3DBase.AccessoryParts.eType type)
        {
            string arg = string.Empty;
            switch (type)
            {
                case Character3DBase.AccessoryParts.eType.Main:
                    arg = ((accIndex != 0) ? $"_{accIndex:00}" : string.Empty);
                    break;
                case Character3DBase.AccessoryParts.eType.Sub:
                    arg = ((accIndex != 0) ? $"_sub{accIndex:00}" : string.Empty);
                    break;
            }
            return string.Format("3D/Chara/Accessory/acc{0:0000}/Flares/md_acc{0:0000}{1}_flare", accId, arg);
        }

        public static string GetBodyModelPath(int dressId, int heightId, int weightId, int bustId, int bodyIndex, string partsCode, Character3DBase.eResourceQuality resourceQuality)
        {
            dressId = ((dressId == 0) ? 1 : dressId);
            string empty = string.Empty;
            if (StageUtil.IsModelCommonDressId(dressId))
            {
                return GetCommonBodyModelPath(dressId, heightId, weightId, bustId, bodyIndex, partsCode, resourceQuality);
            }
            return GetOwnBodyModelPath(dressId, resourceQuality);
        }

        private static string GetCommonBodyModelPath(int dressId, int heightId, int weightId, int bustId, int bodyIndex, string partsCode, Character3DBase.eResourceQuality resourceQuality)
        {
            string text = $"3D/Chara/Body/body{dressId:0000}/Meshes/";
            string empty = string.Empty;
            string empty2 = string.Empty;
            if (Character3DBase.Parts.IsValidPartsCode(partsCode))
            {
                empty = "md_body{0:0000}_{1}_{2}_{3}_{4}";
            }
            else
            {
                empty = "md_body{0:0000}_{2}_{3}_{4}";
                partsCode = string.Empty;
            }
            if (bodyIndex > 0)
            {
                empty += "_{5}";
            }
            empty2 = string.Format(text + empty, dressId, partsCode, heightId, weightId, bustId, bodyIndex);
            if (resourceQuality == Character3DBase.eResourceQuality.Rich)
            {
                empty2 += "_hq";
            }
            return empty2;
        }

        private static string GetOwnBodyModelPath(int dressId, Character3DBase.eResourceQuality resourceQuality)
        {
            string text = string.Format("3D/Chara/Body/body{0:0000}/Meshes/md_body{0:0000}", dressId);
            if (resourceQuality == Character3DBase.eResourceQuality.Rich)
            {
                text += "_hq";
            }
            return text;
        }

        public static string GetBodyTexturePath(int charaId, int dressId, int skinId, int bustId, int colorId, int bodyIndex, string partCode, Character3DBase.eResourceQuality resourcesQuality, Character3DBase.MultiTextures.eMap map = Character3DBase.MultiTextures.eMap.Diffuse)
        {
            dressId = ((dressId == 0) ? 1 : dressId);
            string empty = string.Empty;
            if (resourcesQuality != Character3DBase.eResourceQuality.Rich)
            {
                map = Character3DBase.MultiTextures.eMap.Diffuse;
            }
            if (StageUtil.IsModelCommonDressId(dressId))
            {
                return GetCommonBodyTexturePath(charaId, dressId, skinId, bustId, colorId, bodyIndex, partCode, resourcesQuality, map);
            }
            return GetOwnBodyTexturePath(dressId, resourcesQuality, map);
        }

        private static string GetCommonBodyTexturePath(int charaId, int dressId, int skinId, int bustId, int colorId, int bodyIndex, string partsCode, Character3DBase.eResourceQuality resourcesQuality, Character3DBase.MultiTextures.eMap map)
        {
            string path = $"3D/Chara/Body/body{dressId:0000}/Textures/";
            string fileMask = string.Empty;
            string empty = string.Empty;
            if (Character3DBase.Parts.IsValidPartsCode(partsCode))
            {
                fileMask = "tx_body{0:0000}_{1}_{2}_{3}_{4}";
            }
            else
            {
                fileMask = "tx_body{0:0000}_{2}_{3}_{4}";
                partsCode = string.Empty;
            }
            if (bodyIndex > 0)
            {
                fileMask += "_{5}";
            }
            Func<string> func = delegate
            {
                string text2 = string.Format(path + fileMask, dressId, partsCode, skinId, bustId, colorId, bodyIndex);
                if (resourcesQuality == Character3DBase.eResourceQuality.Rich)
                {
                    text2 += "_hq";
                }
                return text2;
            };
            Func<int, int, string, string> func2 = delegate (int defBustId, int defColorId, string suffix)
            {
                fileMask += suffix;
                string text = string.Format(fileMask, dressId, partsCode, skinId, bustId, colorId, bodyIndex);
                if (!Character3DBase.ExistTexture(path, text))
                {
                    text = string.Format(fileMask, dressId, partsCode, skinId, defBustId, defColorId, bodyIndex);
                }
                return path + text;
            };
            return map switch
            {
                Character3DBase.MultiTextures.eMap.Diffuse => func(),
                Character3DBase.MultiTextures.eMap.Specular => func2(1, colorId, "_spec"),
                Character3DBase.MultiTextures.eMap.Control => func2(1, 0, "_multi"),
                _ => string.Empty,
            };
        }

        private static string GetOwnBodyTexturePath(int dressId, Character3DBase.eResourceQuality resourceQuality, Character3DBase.MultiTextures.eMap map = Character3DBase.MultiTextures.eMap.Diffuse)
        {
            string text = string.Format("3D/Chara/Body/body{0:0000}/Textures/tx_body{0:0000}", dressId);
            switch (map)
            {
                case Character3DBase.MultiTextures.eMap.Diffuse:
                    if (resourceQuality == Character3DBase.eResourceQuality.Rich)
                    {
                        text += "_hq";
                    }
                    break;
                case Character3DBase.MultiTextures.eMap.Specular:
                    text += "_spec";
                    break;
                case Character3DBase.MultiTextures.eMap.Control:
                    text += "_multi";
                    break;
            }
            return text;
        }

        public static string GetBodyClothPath(int dressId, int bodyIndex)
        {
            dressId = ((dressId == 0) ? 1 : dressId);
            return GetBodyClothCollisionPath(dressId, bodyIndex, "_cloth");
        }

        public static string GetBodyCollisionPath(int dressId, int bodyIndex)
        {
            dressId = ((dressId == 0) ? 1 : dressId);
            return GetBodyClothCollisionPath(dressId, bodyIndex, "_collision");
        }

        private static string GetBodyClothCollisionPath(int dressId, int bodyIndex, string suffix)
        {
            _ = string.Empty;
            return string.Format("3D/Chara/Body/body{0:0000}/Clothes/md_body{0:0000}" + ((bodyIndex > 0) ? $"_{bodyIndex}{suffix}" : suffix), dressId, bodyIndex);
        }

        public static string GetBodyFlareColliderPath(int dressId, int heightId, int weightId, int bustId, int bodyIndex)
        {
            dressId = ((dressId == 0) ? 1 : dressId);
            string empty = string.Empty;
            if (StageUtil.IsModelCommonDressId(dressId))
            {
                return GetCommonBodyFlareColliderPath(dressId, heightId, weightId, bustId, bodyIndex);
            }
            return GetOwnBodyFlareCollider(dressId);
        }

        private static string GetCommonBodyFlareColliderPath(int dressId, int heightId, int weightId, int bustId, int bodyIndex)
        {
            _ = string.Empty;
            return string.Format("3D/Chara/Body/body{0:0000}/Flares/md_body{0:0000}_{1}_{2}_{3}" + ((bodyIndex > 0) ? "_{4}_flare" : "_flare"), dressId, heightId, weightId, bustId, bodyIndex);
        }

        private static string GetOwnBodyFlareCollider(int dressId)
        {
            return string.Format("3D/Chara/Body/body{0:0000}/Flares/md_body{0:0000}_flare", dressId);
        }

        public static string GetHeadModelPath(int charaId, int dressId, int headIndex, bool isSubHeadIndex, Character3DBase.eResourceQuality resourceQuality)
        {
            dressId = ((dressId == 0) ? 1 : dressId);
            string empty = string.Empty;
            if (StageUtil.IsModelCommonDressId(dressId))
            {
                return GetCommonHeadModelPath(charaId, headIndex, isSubHeadIndex, resourceQuality);
            }
            return GetOwnHeadModelPath(charaId, dressId, headIndex, resourceQuality);
        }

        private static string GetCommonHeadModelPath(int charaId, int headIndex, bool isSubHeadIndex, Character3DBase.eResourceQuality resourceQuality)
        {
            _ = string.Empty;
            string empty = string.Empty;
            string arg = string.Empty;
            if (headIndex != 0)
            {
                arg = (isSubHeadIndex ? "_sub{1:000}" : "_{1:0000}");
            }
            empty = string.Format("3D/Chara/Head/chr{0}{1}/Meshes/md_chr{0}{1}", "{0:0000}", arg);
            if (resourceQuality == Character3DBase.eResourceQuality.Rich)
            {
                empty += "_hq";
            }
            return string.Format(empty, charaId, headIndex);
        }

        private static string GetOwnHeadModelPath(int charaId, int dressId, int headIndex, Character3DBase.eResourceQuality resourceQuality)
        {
            _ = string.Empty;
            string empty = string.Empty;
            string arg = ((headIndex != 0) ? "_sub{2:000}" : string.Empty);
            empty = string.Format("3D/Chara/Head/chr{0}{1}{2}/Meshes/md_chr{0}{1}{2}", "{0:0000}", "_{1:0000}", arg);
            if (resourceQuality == Character3DBase.eResourceQuality.Rich)
            {
                empty += "_hq";
            }
            return string.Format(empty, charaId, dressId, headIndex);
        }

        public static string GetSweatLocatorPath(int charaId, int dressId, int headIndex, bool isSubHeadIndex)
        {
            dressId = ((dressId == 0) ? 1 : dressId);
            string empty = string.Empty;
            if (StageUtil.IsModelCommonDressId(dressId))
            {
                return GetCommonDressSweatLocatorPath(charaId, headIndex, isSubHeadIndex);
            }
            return GetOwnDressSweatLocatorPath(charaId, dressId, headIndex);
        }

        private static string GetCommonDressSweatLocatorPath(int charaId, int headIndex, bool isSubHeadIndex)
        {
            _ = string.Empty;
            _ = string.Empty;
            string arg = string.Empty;
            if (headIndex != 0)
            {
                arg = (isSubHeadIndex ? "_sub{1:000}" : "_{1:0000}");
            }
            return string.Format(string.Format("3D/Chara/Head/chr{0}{1}/Prefab/pf_chr{0}{1}_sweatlocator_hq", "{0:0000}", arg), charaId, headIndex);
        }

        private static string GetOwnDressSweatLocatorPath(int charaId, int dressId, int headIndex)
        {
            _ = string.Empty;
            _ = string.Empty;
            string arg = ((headIndex != 0) ? "_sub{2:000}" : string.Empty);
            return string.Format(string.Format("3D/Chara/Head/chr{0}{1}{2}/Prefab/pf_chr{0}{1}{2}_sweatlocator_hq", "{0:0000}", "_{1:0000}", arg), charaId, dressId, headIndex);
        }

        public static string GetFaceTexturePath(int charaId, int dressId, int headIndex, int headTextureIndex, bool isSubHeadIndex, Character3DBase.MultiTextures.eMap map, Character3DBase.eResourceQuality resourceQuality)
        {
            dressId = ((dressId == 0) ? 1 : dressId);
            switch (map)
            {
                case Character3DBase.MultiTextures.eMap.Specular:
                    resourceQuality = Character3DBase.eResourceQuality.Rich;
                    break;
                case Character3DBase.MultiTextures.eMap.Control:
                    resourceQuality = Character3DBase.eResourceQuality.Rich;
                    headTextureIndex = 0;
                    break;
            }
            string headTextureSuffix = GetHeadTextureSuffix(map, Character3DBase.MaterialPack.eMaterialCategory.Face, resourceQuality);
            return GetHeadTexturePath(charaId, dressId, headIndex, headTextureIndex, headTextureSuffix, isSubHeadIndex);
        }

        public static string GetObjectTexturePath(int charaId, int dressId, int headIndex, int headTextureIndex, bool isSubHeadIndex, Character3DBase.MultiTextures.eMap map, Character3DBase.eResourceQuality resourceQuality)
        {
            dressId = ((dressId == 0) ? 1 : dressId);
            switch (map)
            {
                case Character3DBase.MultiTextures.eMap.Specular:
                    resourceQuality = Character3DBase.eResourceQuality.Rich;
                    break;
                case Character3DBase.MultiTextures.eMap.Control:
                    resourceQuality = Character3DBase.eResourceQuality.Rich;
                    headTextureIndex = 0;
                    break;
            }
            string headTextureSuffix = GetHeadTextureSuffix(map, Character3DBase.MaterialPack.eMaterialCategory.Object, resourceQuality);
            return GetHeadTexturePath(charaId, dressId, headIndex, headTextureIndex, headTextureSuffix, isSubHeadIndex);
        }

        public static string GetCheekTexturePath(int charaId, int dressId, int headIndex, bool isSubHeadIndex, Character3DBase.MultiTextures.eMap map, Character3DBase.eResourceQuality resourceQuality)
        {
            dressId = ((dressId == 0) ? 1 : dressId);
            string text = string.Empty;
            switch (map)
            {
                case Character3DBase.MultiTextures.eMap.Diffuse:
                case Character3DBase.MultiTextures.eMap.Specular:
                case Character3DBase.MultiTextures.eMap.Control:
                    map = Character3DBase.MultiTextures.eMap.Diffuse;
                    text = "_C";
                    break;
                case Character3DBase.MultiTextures.eMap.AlphaMask:
                    text = "_A";
                    break;
            }
            string headTextureSuffix = GetHeadTextureSuffix(Character3DBase.MultiTextures.eMap.Diffuse, Character3DBase.MaterialPack.eMaterialCategory.Cheek, resourceQuality);
            headTextureSuffix += text;
            return GetHeadTexturePath(charaId, dressId, headIndex, 0, headTextureSuffix, isSubHeadIndex);
        }

        private static string GetHeadTextureSuffix(Character3DBase.MultiTextures.eMap map, Character3DBase.MaterialPack.eMaterialCategory materialCategory, Character3DBase.eResourceQuality resourceQuality)
        {
            string empty = string.Empty;
            Func<string> func = delegate
            {
                string empty2 = string.Empty;
                return map switch
                {
                    Character3DBase.MultiTextures.eMap.Diffuse => (resourceQuality == Character3DBase.eResourceQuality.Rich) ? "_hq" : string.Empty,
                    Character3DBase.MultiTextures.eMap.Specular => "_spec",
                    Character3DBase.MultiTextures.eMap.Control => "_multi",
                    _ => string.Empty,
                };
            };
            return materialCategory switch
            {
                Character3DBase.MaterialPack.eMaterialCategory.Cheek => "_cheek",
                Character3DBase.MaterialPack.eMaterialCategory.Object => "_obj" + func(),
                _ => func(),
            };
        }

        private static string GetHeadTexturePath(int charaId, int dressId, int headIndex, int headTextureIndex, string suffix, bool isSubHeadIndex)
        {
            string empty = string.Empty;
            if (StageUtil.IsModelCommonDressId(dressId))
            {
                return GetCommonHeadTexturePath(charaId, headIndex, headTextureIndex, suffix, isSubHeadIndex);
            }
            return GetOwnHeadTexturePath(charaId, dressId, headIndex, suffix);
        }

        private static string GetCommonHeadTexturePath(int charaId, int headIndex, int headTextureIndex, string suffix, bool isSubHeadIndex)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            _ = string.Empty;
            if (isSubHeadIndex)
            {
                empty = ((headIndex != 0) ? $"_sub{headIndex:000}" : string.Empty);
                empty2 = ((headTextureIndex != 0) ? $"_col{headTextureIndex:000}" : string.Empty);
            }
            else
            {
                empty = ((headIndex != 0) ? $"_{headIndex:0000}" : string.Empty);
                empty2 = string.Empty;
            }
            return string.Format("3D/Chara/Head/chr{0:0000}{1}/Textures/tx_chr{0:0000}{1}{2}{3}", charaId, empty, empty2, suffix);
        }

        private static string GetOwnHeadTexturePath(int charaId, int dressId, int headIndex, string suffix)
        {
            string text = string.Empty;
            _ = string.Empty;
            if (headIndex != 0)
            {
                text = $"_sub{headIndex:000}";
            }
            return string.Format("3D/Chara/Head/chr{0:0000}_{1:0000}{2}/Textures/tx_chr{0:0000}_{1:0000}{2}{3}", charaId, dressId, text, suffix);
        }

        public static string GetHeadClothPath(int charaId, int dressId, int headIndex, bool isSubHeadIndex)
        {
            dressId = ((dressId == 0) ? 1 : dressId);
            return GetHeadClothCollisionPath(charaId, dressId, headIndex, "_cloth", isSubHeadIndex);
        }

        public static string GetHeadCollisionPath(int charaId, int dressId, int headIndex, bool isSubHeadIndex)
        {
            dressId = ((dressId == 0) ? 1 : dressId);
            return GetHeadClothCollisionPath(charaId, dressId, headIndex, "_collision", isSubHeadIndex);
        }

        private static string GetHeadClothCollisionPath(int charaId, int dressId, int headIndex, string suffix, bool isSubHeadIndex)
        {
            string empty = string.Empty;
            empty = ((!StageUtil.IsModelCommonDressId(dressId)) ? GetOwnHeadClothCollisionPath(charaId, dressId, headIndex) : GetCommonHeadClothCollisionPath(charaId, headIndex, isSubHeadIndex));
            if (!string.IsNullOrEmpty(suffix))
            {
                empty += suffix;
            }
            return empty;
        }

        private static string GetCommonHeadClothCollisionPath(int charaId, int headIndex, bool isSubHeadIndex)
        {
            string arg = string.Empty;
            if (headIndex != 0)
            {
                arg = (isSubHeadIndex ? $"_sub{headIndex:000}" : $"_{headIndex:0000}");
            }
            return string.Format("3D/Chara/Head/chr{0:0000}{1}/Clothes/md_chr{0:0000}{1}", charaId, arg);
        }

        private static string GetOwnHeadClothCollisionPath(int charaId, int dressId, int headIndex)
        {
            string arg = ((headIndex != 0) ? $"_sub{headIndex:000}" : string.Empty);
            return string.Format("3D/Chara/Head/chr{0:0000}_{1:0000}{2}/Clothes/md_chr{0:0000}_{1:0000}{2}", charaId, dressId, arg);
        }

        public static string GetHeadFlareColliderPath(int charaId, int dressId, int headIndex, bool isSubHeadIndex)
        {
            dressId = ((dressId == 0) ? 1 : dressId);
            string empty = string.Empty;
            if (StageUtil.IsModelCommonDressId(dressId))
            {
                return GetCommonHeadFlareColliderPath(charaId, headIndex, isSubHeadIndex);
            }
            return GetOwnHeadFlareColliderPath(charaId, dressId, headIndex);
        }

        private static string GetCommonHeadFlareColliderPath(int charaId, int headIndex, bool isSubHeadIndex)
        {
            string arg = string.Empty;
            if (headIndex != 0)
            {
                arg = (isSubHeadIndex ? $"_sub{headIndex:000}" : $"_{headIndex:0000}");
            }
            return string.Format("3D/Chara/Head/chr{0:0000}{1}/Flares/md_chr{0:0000}{1}_flare", charaId, arg);
        }

        private static string GetOwnHeadFlareColliderPath(int charaId, int dressId, int headIndex)
        {
            string arg = ((headIndex != 0) ? $"_sub{headIndex:000}" : string.Empty);
            return string.Format("3D/Chara/Head/chr{0:0000}_{1:0000}{2}/Flares/md_chr{0:0000}_{1:0000}{2}_flare", charaId, dressId, arg);
        }

        public static string GetFaceAnimatorPath(int charaId, int dressId, int headIndex, bool isSubHeadIndex)
        {
            dressId = ((dressId == 0) ? 1 : dressId);
            string empty = string.Empty;
            if (StageUtil.IsModelCommonDressId(dressId))
            {
                return GetCommonFaceAnimatorPath(charaId, headIndex, isSubHeadIndex);
            }
            return GetOwnFaceAnimatorPath(charaId, dressId, headIndex);
        }

        private static string GetCommonFaceAnimatorPath(int charaId, int headIndex, bool isSubHeadIndex)
        {
            string arg = string.Empty;
            if (headIndex != 0)
            {
                arg = (isSubHeadIndex ? $"_sub{headIndex:000}" : $"_{headIndex:0000}");
            }
            return string.Format("3D/Chara/Head/chr{0:0000}{1}/Motions/ac_chr{0:0000}{1}", charaId, arg);
        }

        private static string GetOwnFaceAnimatorPath(int charaId, int dressId, int headIndex)
        {
            string arg = ((headIndex != 0) ? $"_sub{headIndex:000}" : string.Empty);
            return string.Format("3D/Chara/Head/chr{0:0000}_{1:0000}{2}/Motions/ac_chr{0:0000}_{1:0000}{2}", charaId, dressId, arg);
        }

        public static string GetOptionalDataPath(int optionName)
        {
            return string.Format("3D/Chara/Option/opt{0:D4}/Prefab/pf_opt{0:D4}", optionName);
        }
    }

    public class AssetBundle
    {
        public static string GetAccessoryModelName(int accId, int accIndex, Character3DBase.AccessoryParts.eType type, Character3DBase.eResourceQuality resourceQuality)
        {
            accId = ((accId == 0) ? 1 : accId);
            string empty = string.Empty;
            if (StageUtil.IsModelCommonAccId(accId))
            {
                return GetCommonAccessoryModelName(accId, accIndex, type, resourceQuality);
            }
            return GetOwnAccessoryModelName(accId, accIndex, type, resourceQuality);
        }

        private static string GetCommonAccessoryModelName(int accId, int accIndex, Character3DBase.AccessoryParts.eType type, Character3DBase.eResourceQuality resourceQuality)
        {
            string text = string.Empty;
            switch (type)
            {
                case Character3DBase.AccessoryParts.eType.Main:
                    text = ((accIndex > 0) ? $"_{accIndex:00}" : string.Empty);
                    break;
                case Character3DBase.AccessoryParts.eType.Sub:
                    text = $"_sub{accIndex:00}";
                    break;
            }
            string text2 = ((resourceQuality == Character3DBase.eResourceQuality.Rich) ? "_hq.unity3d" : ".unity3d");
            return string.Format("{0}{1:0000}{2}{3}", "3d_md_acc", accId, text, text2);
        }

        private static string GetOwnAccessoryModelName(int accId, int accIndex, Character3DBase.AccessoryParts.eType type, Character3DBase.eResourceQuality resourceQuality)
        {
            return string.Empty;
        }

        public static string GetAccessoryTextureName(int accId, int colorId, bool accTexDiversity, int accTexIndex, Character3DBase.eResourceQuality resourceQuality, Character3DBase.MultiTextures.eMap map)
        {
            accId = ((accId == 0) ? 1 : accId);
            string empty = string.Empty;
            string accessoryTextureSuffix = GetAccessoryTextureSuffix(resourceQuality, map);
            if (StageUtil.IsModelCommonAccId(accId))
            {
                return GetCommonAccessoryTextureName(accId, colorId, accTexDiversity, accTexIndex, accessoryTextureSuffix);
            }
            return GetOwnAccessoryTextureName(accId, colorId, accTexDiversity, accTexIndex, accessoryTextureSuffix);
        }

        private static string GetAccessoryTextureSuffix(Character3DBase.eResourceQuality resourceQuality, Character3DBase.MultiTextures.eMap map)
        {
            string empty = string.Empty;
            return map switch
            {
                Character3DBase.MultiTextures.eMap.Diffuse => (resourceQuality == Character3DBase.eResourceQuality.Rich) ? "_hq.unity3d" : ".unity3d",
                Character3DBase.MultiTextures.eMap.Specular => "_spec.unity3d",
                Character3DBase.MultiTextures.eMap.Control => "_multi.unity3d",
                _ => ".unity3d",
            };
        }

        private static string GetCommonAccessoryTextureName(int accId, int colorId, bool accTexDiversity, int accTexIndex, string suffix)
        {
            string text = string.Empty;
            if (accTexDiversity)
            {
                text = $"_{accTexIndex:0000}";
            }
            return string.Format("{0}{1:0000}_{2}{3}{4}", "3d_tx_acc", accId, colorId, text, suffix);
        }

        private static string GetOwnAccessoryTextureName(int accId, int colorId, bool accTexDiversity, int accTexIndex, string suffix)
        {
            return string.Empty;
        }

        public static string GetAccessoryParameterName(int accId)
        {
            return string.Format("{0}{1:0000}.unity3d", "3d_prm_acc", accId);
        }

        public static string GetAccessoryFlareColliderName(int accId, int accIndex, Character3DBase.AccessoryParts.eType type)
        {
            accId = ((accId == 0) ? 1 : accId);
            string empty = string.Empty;
            if (StageUtil.IsModelCommonAccId(accId))
            {
                return GetCommonAccessoryFlareColliderName(accId, accIndex, type);
            }
            return GetOwnAccessoryFlareColliderName(accId, accIndex, type);
        }

        private static string GetCommonAccessoryFlareColliderName(int accId, int accIndex, Character3DBase.AccessoryParts.eType type)
        {
            string text = string.Empty;
            switch (type)
            {
                case Character3DBase.AccessoryParts.eType.Main:
                    text = ((accIndex > 0) ? $"_{accIndex:00}" : string.Empty);
                    break;
                case Character3DBase.AccessoryParts.eType.Sub:
                    text = $"_sub{accIndex:00}";
                    break;
            }
            return string.Format("{0}{1:0000}{2}{3}.unity3d", "3d_md_acc", accId, text, "_flare");
        }

        private static string GetOwnAccessoryFlareColliderName(int accId, int accIndex, Character3DBase.AccessoryParts.eType type)
        {
            return string.Empty;
        }

        public static string GetBodyModelName(int dressId, int heightId, int weightId, int bustId, int bodyIndex, string partsCode, Character3DBase.eResourceQuality resourceQuality)
        {
            dressId = ((dressId == 0) ? 1 : dressId);
            string empty = string.Empty;
            if (StageUtil.IsModelCommonDressId(dressId))
            {
                return GetCommonBodyModelName(dressId, heightId, weightId, bustId, bodyIndex, partsCode, resourceQuality);
            }
            return GetOwnBodyModelName(dressId, resourceQuality);
        }

        private static string GetCommonBodyModelName(int dressId, int heightId, int weightId, int bustId, int bodyIndex, string partsCode, Character3DBase.eResourceQuality resourceQuality)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            empty = ((!Character3DBase.Parts.IsValidPartsCode(partsCode)) ? "{0}{1:0000}_{3}_{4}_{5}" : "{0}{1:0000}_{2}_{3}_{4}_{5}");
            if (bodyIndex > 0)
            {
                empty += "_{6}";
            }
            empty2 = string.Format(empty, "3d_md_body", dressId, partsCode, heightId, weightId, bustId, bodyIndex);
            if ((uint)resourceQuality > 1u && resourceQuality == Character3DBase.eResourceQuality.Rich)
            {
                return empty2 + "_hq.unity3d";
            }
            return empty2 + ".unity3d";
        }

        private static string GetOwnBodyModelName(int dressId, Character3DBase.eResourceQuality resourceQuality)
        {
            string empty = string.Empty;
            empty = (((uint)resourceQuality <= 1u || resourceQuality != Character3DBase.eResourceQuality.Rich) ? "{0}{1:0000}.unity3d" : "{0}{1:0000}_hq.unity3d");
            return string.Format(empty, "3d_md_body", dressId);
        }

        public static string GetBodyTextureName(int charaId, int dressId, int skinId, int bustId, int colorId, int bodyIndex, Character3DBase.eResourceQuality resourceQuality, Character3DBase.MultiTextures.eMap map = Character3DBase.MultiTextures.eMap.Diffuse)
        {
            dressId = ((dressId == 0) ? 1 : dressId);
            string empty = string.Empty;
            if (StageUtil.IsModelCommonDressId(dressId))
            {
                return GetCommonBodyTextureName(charaId, dressId, skinId, bustId, colorId, bodyIndex, resourceQuality, map);
            }
            return GetOwnBodyTextureName(dressId, resourceQuality, map);
        }

        private static string GetCommonBodyTextureName(int charaId, int dressId, int skinId, int bustId, int colorId, int bodyIndex, Character3DBase.eResourceQuality resourceQuality, Character3DBase.MultiTextures.eMap map = Character3DBase.MultiTextures.eMap.Diffuse)
        {
            string pathMask = "3D/Chara/Body/body{0:0000}/Textures/";
            string fileMask = "3d_tx_body{0:0000}_{1}_{2}_{3}";
            string empty = string.Empty;
            if (bodyIndex > 0)
            {
                fileMask += "_{4}";
            }
            Func<string> func = delegate
            {
                string text2 = string.Format(fileMask, dressId, skinId, bustId, colorId, bodyIndex);
                if (resourceQuality == Character3DBase.eResourceQuality.Rich)
                {
                    text2 += "_hq";
                }
                return text2;
            };
            Func<int, int, string, string> func2 = delegate (int defBustId, int defColorId, string suffix)
            {
                fileMask += suffix;
                string path = string.Format(pathMask, dressId);
                string text = string.Format(fileMask, dressId, skinId, bustId, colorId, bodyIndex);
                if (!Character3DBase.ExistTexture(path, text, "{0}.unity3d"))
                {
                    text = string.Format(fileMask, dressId, skinId, defBustId, defColorId, bodyIndex);
                }
                return text;
            };
            if (resourceQuality != Character3DBase.eResourceQuality.Rich)
            {
                map = Character3DBase.MultiTextures.eMap.Diffuse;
            }
            return map switch
            {
                Character3DBase.MultiTextures.eMap.Diffuse => func(),
                Character3DBase.MultiTextures.eMap.Specular => func2(1, colorId, "_spec"),
                Character3DBase.MultiTextures.eMap.Control => func2(1, 0, "_multi"),
                _ => string.Empty,
            } + ".unity3d";
        }

        private static string GetOwnBodyTextureName(int dressId, Character3DBase.eResourceQuality resourceQuality, Character3DBase.MultiTextures.eMap map = Character3DBase.MultiTextures.eMap.Diffuse)
        {
            string arg = "3d_tx_body";
            string arg2 = string.Empty;
            string empty = string.Empty;
            _ = string.Empty;
            if (resourceQuality != Character3DBase.eResourceQuality.Rich)
            {
                map = Character3DBase.MultiTextures.eMap.Diffuse;
            }
            switch (map)
            {
                case Character3DBase.MultiTextures.eMap.Diffuse:
                    if (resourceQuality == Character3DBase.eResourceQuality.Rich)
                    {
                        empty = "{0}{1:000}{2}.unity3d";
                        arg2 = "_hq";
                    }
                    else
                    {
                        empty = "{0}{1:000}.unity3d";
                    }
                    break;
                case Character3DBase.MultiTextures.eMap.Specular:
                    arg2 = "_spec";
                    empty = "{0}{1:000}{2}.unity3d";
                    break;
                case Character3DBase.MultiTextures.eMap.Control:
                    arg2 = "_multi";
                    empty = "{0}{1:000}{2}.unity3d";
                    break;
                default:
                    empty = "{0}{1:000}.unity3d";
                    arg2 = string.Empty;
                    break;
            }
            return string.Format(empty, arg, dressId, arg2);
        }

        public static string GetBodyClothName(int dressId, int bodyIndex)
        {
            dressId = ((dressId == 0) ? 1 : dressId);
            _ = string.Empty;
            return string.Format("{0}{1:0000}" + ((bodyIndex > 0) ? "_{2}.unity3d" : ".unity3d"), "3d_chara_body_", dressId, bodyIndex);
        }

        public static string GetBodyFlareColliderName(int dressId, int heightId, int weightId, int bustId, int bodyIndex)
        {
            dressId = ((dressId == 0) ? 1 : dressId);
            string empty = string.Empty;
            if (StageUtil.IsModelCommonDressId(dressId))
            {
                return GetCommonBodyFlareColliderName(dressId, heightId, weightId, bustId, bodyIndex);
            }
            return GetOwnBodyFlareColliderName(dressId);
        }

        private static string GetCommonBodyFlareColliderName(int dressId, int heightId, int weightId, int bustId, int bodyIndex)
        {
            _ = string.Empty;
            return string.Format("{0}{1:0000}_{2}_{3}_{4}" + ((bodyIndex > 0) ? "_{5}_flare.unity3d" : "_flare.unity3d"), "3d_md_body", dressId, heightId, weightId, bustId, bodyIndex);
        }

        private static string GetOwnBodyFlareColliderName(int dressId)
        {
            return string.Format("{0}{1:0000}_flare.unity3d", "3d_md_body", dressId);
        }

        public static string GetHeadModelName(int charaId, int dressId, int headIndex, bool isSubHeadIndex, Character3DBase.eResourceQuality resourceQuality)
        {
            dressId = ((dressId == 0) ? 1 : dressId);
            string empty = string.Empty;
            if (StageUtil.IsModelCommonDressId(dressId))
            {
                return GetCommonHeadModelName(charaId, headIndex, isSubHeadIndex, resourceQuality);
            }
            return GetOwnHeadModelName(charaId, dressId, headIndex, resourceQuality);
        }

        private static string GetCommonHeadModelName(int charaId, int headIndex, bool isSubHeadIndex, Character3DBase.eResourceQuality resourceQuality)
        {
            string text = string.Empty;
            if (headIndex != 0)
            {
                text = (isSubHeadIndex ? $"_sub{headIndex:000}" : $"_{headIndex:0000}");
            }
            string text2 = ((resourceQuality == Character3DBase.eResourceQuality.Rich) ? "_hq.unity3d" : ".unity3d");
            return string.Format("{0}{1:0000}{2}{3}", "3d_chara_head_", charaId, text, text2);
        }

        private static string GetOwnHeadModelName(int charaId, int dressId, int headIndex, Character3DBase.eResourceQuality resourceQuality)
        {
            string text = ((headIndex != 0) ? $"_sub{headIndex:000}" : string.Empty);
            string text2 = ((resourceQuality == Character3DBase.eResourceQuality.Rich) ? "_hq.unity3d" : ".unity3d");
            return string.Format("{0}{1:0000}_{2:0000}{3}{4}", "3d_chara_head_", charaId, dressId, text, text2);
        }

        public static string GetHeadTextureName(int charaId, int dressId, int headIndex, int headTextureIndex, bool isSubHeadIndex, Character3DBase.eResourceQuality resourceQuality)
        {
            dressId = ((dressId == 0) ? 1 : dressId);
            string result = string.Empty;
            if (StageUtil.IsModelCommonDressId(dressId) && isSubHeadIndex)
            {
                string text = ((resourceQuality == Character3DBase.eResourceQuality.Rich) ? "_hq.unity3d" : ".unity3d");
                string text2 = ((headIndex != 0) ? $"_sub{headIndex:000}" : string.Empty);
                result = string.Format("{0}{1:0000}{2}_col{3:000}{4}", "3d_tx_head", charaId, text2, headTextureIndex, text);
            }
            return result;
        }

        public static string GetHeadFlareColliderName(int charaId, int dressId, int headIndex, bool isSubHeadIndex)
        {
            dressId = ((dressId == 0) ? 1 : dressId);
            string empty = string.Empty;
            if (StageUtil.IsModelCommonDressId(dressId))
            {
                return GetCommonHeadFlareColliderName(charaId, headIndex, isSubHeadIndex);
            }
            return GetOwnHeadFlareColliderName(charaId, dressId, headIndex);
        }

        private static string GetCommonHeadFlareColliderName(int charaId, int headIndex, bool isSubHeadIndex)
        {
            string arg = string.Empty;
            if (headIndex != 0)
            {
                arg = (isSubHeadIndex ? $"_sub{headIndex:000}" : $"_{headIndex:0000}");
            }
            return string.Format("{0}{1:0000}{2}_flare.unity3d", "3d_chara_head_", charaId, arg);
        }

        private static string GetOwnHeadFlareColliderName(int charaId, int dressId, int headIndex)
        {
            string text = ((headIndex != 0) ? $"_sub{headIndex:000}" : string.Empty);
            return string.Format("{0}{1:0000}_{2:0000}{3}_flare.unity3d", "3d_chara_head_", charaId, dressId, text);
        }

        public static string GetOptionalDataName(int id)
        {
            return string.Format("{0}{1:D4}.unity3d", "3d_chara_option_", id);
        }
    }

    /// <summary>
    /// ディレクタで使用するデータをまとめたもの
    /// </summary>
    public class CharaDirectorData
    {
        public CharaDirectorData(SQCharaData _charaID, SQDressData _dressID, int musicID, int position)
        {
            Initialize(_charaID.charaID, _dressID.activeDressID, musicID, position);
        }

        public CharaDirectorData(int _charaID, int _dressID, int _musicID, int _position)
        {
            Initialize(_charaID, _dressID, _musicID, _position);
        }

        public void Initialize(int _charaID, int _dressID, int _musicID, int _position)
        {
            _characterData = Character3DBase.CreateCharacterData(_charaID, _dressID, _dressID);

            //もりくぼ用。目線の変化のためにカードIDが必要
            int cardID = Character3DBase.CharacterData.ConvertModelIdSSRDress(_dressID, _charaID);
            if (cardID != 0)
            {
                _characterData.cardId = cardID;
            }

            ConvertCharadata(_musicID, _position);

            Character3DBase.eHeadLoadType headLoadType = Character3DBase.eHeadLoadType.Default;
            headIndex = StageUtil.GetHeadIndexWithHeadSelector(_characterData, MasterDBManager.instance.master3DCharaData, _musicID, _position == 0, out headLoadType);
            headTextureIndex = StageUtil.GetHeadTextureIndex(_charaID, _dressID);
            isSubHeadIndex = headLoadType == Character3DBase.eHeadLoadType.Default;

            charaId = _charaID;
            activeDressId = _characterData.activeDressId;
            activeAccessoryId = _characterData.activeAccessoryId;
            heightId = _characterData.heightId;
            weightId = _characterData.weightId;
            skinId = _characterData.skinId;
            colorId = _characterData.colorId;
            bustId = _characterData.bustId;

            _dressCabinetCache = new DressCabinet.Cache();
            _dressCabinetCache.Initialize(activeDressId, charaId);

            bodyIndex = _dressCabinetCache.bodyIndex;

            if (_dressCabinetCache.headTexIndex != 0)
            {
                headTextureIndex = _dressCabinetCache.headTexIndex;
            }

            accTexDiversity = false;
            accTexIndex = 0;
            accIndex = 0;
            subAccIndex = 0;
            if (_dressCabinetCache.validAcc)
            {
                accTexDiversity = _dressCabinetCache.accTexDiversity;
                accTexIndex = _dressCabinetCache.accTexIndex;
                accIndex = _dressCabinetCache.mainAccIndex;
                subAccIndex = _dressCabinetCache.subAccIndex;
            }
        }

        private void ConvertCharadata(int musicID, int position)
        {

            //きらりんロボをほかのライブで使用するには身長を変更する必要がある
            if (_characterData.charaId == 83)
            {
                //きらりの身長へ変更
                _characterData.height = 182;
                _characterData.heightWithoutHeel = 190;
            }

            //きらりんロボのテーマの時はでかくしてあげないと見えなくなる
            if (musicID == 1903)
            {
                if (position == 0 || position == 1)
                {
                    _characterData.height = 1800;
                    _characterData.heightWithoutHeel = 1808;
                }
            }

            //びよんどすたーらいとキャラはHQモデルが存在しないため、ハイポリに変更
            if (_characterData.charaId == 48 || _characterData.charaId == 49 || _characterData.charaId == 50)
            {
                resourceQuality = Character3DBase.eResourceQuality.HighPolygon;
            }
            //VR用キャラはHQモデルが存在しない
            if (_characterData.charaId == 701 || _characterData.charaId == 725 || _characterData.charaId == 726 || _characterData.charaId == 672 || _characterData.charaId == 682)
            {
                resourceQuality = Character3DBase.eResourceQuality.HighPolygon;
            }
        }

        /// <summary>
        /// キャラクターデータ
        /// </summary>
        public SQCharaData charaData;

        /// <summary>
        /// ドレスデータ
        /// 共通衣装/ショップ衣装の時だけ入る
        /// SSR衣装はid == 100になっている
        /// </summary>
        public SQDressData dressData;

        public Character3DBase.CharacterData _characterData;

        public DressCabinet.Cache _dressCabinetCache;

        public Character3DBase.eResourceQuality resourceQuality = Character3DBase.eResourceQuality.Rich;

        public bool isSubHeadIndex;

        public int charaId;

        public int bodyIndex;

        public int headIndex;

        public int accIndex;

        public int subAccIndex;

        public int headTextureIndex;

        public int activeDressId;

        public int activeAccessoryId;

        public bool accTexDiversity;

        public int accTexIndex;

        public int heightId;

        public int weightId;

        public int skinId;

        public int colorId;

        public int bustId;
    }

    private CharaDirectorData charaDirectorData = null;

    private CharaDirectorData appendCharaDirectorData = null;

    private readonly float[] bodyNodeNeckSize = 
    {
        1.140078f,
        1.314968f,
        1.428056f,
        1.571387f
    };

    private readonly float[] bodyRootSize = 
    {
        0.8831882f,
        1.018672f,
        1.106277f,
        1.217312f
    };

    public float originalBodyNodeNeckSize
    {
        get
        {
            return bodyNodeNeckSize[charaDirectorData.heightId];
        }
    }

    public float appendBodyNodeNeckSize
    {
        get
        {
            return bodyNodeNeckSize[appendCharaDirectorData.heightId];
        }
    }

    public float originalBodyRootSize
    {
        get
        {
            return bodyRootSize[charaDirectorData.heightId];
        }
    }

    public float appendBodyRootSize
    {
        get
        {
            return bodyRootSize[appendCharaDirectorData.heightId];
        }
    }

    /// <summary>
    /// 楽曲データ
    /// </summary>
    //private SQMusicData musicData;

    /// <summary>
    /// 首のすげ替え
    /// </summary>
    public bool isAppendDress
    {
        get
        {
            if (appendCharaDirectorData != null)
            {
                return true;
            }
            return false;
        }
    }


    /// <summary>
    /// ファイルとりまとめ
    /// </summary>
    public CharaDirector(SQCharaData _charaID, SQDressData _dressID, SQMusicData _musicID, int position)
    {
        //musicData = _musicID;
        charaDirectorData = new CharaDirectorData(_charaID, _dressID, _musicID.music_data_id, position);
    }

    /// <summary>
    /// ファイルとりまとめ
    /// </summary>
    public CharaDirector(SQCharaData _charaID, SQDressData _dressID, SQCharaData _appendCharaID, SQDressData _appendDressID, SQMusicData _musicID, int position)
    {
        //musicData = _musicID;
        charaDirectorData = new CharaDirectorData(_charaID, _dressID, _musicID.music_data_id, position);
        appendCharaDirectorData = new CharaDirectorData(_appendCharaID, _appendDressID, _musicID.music_data_id, position);
    }

    public CharaDirector(int _charaID, int _dressID, int _musicID, int position)
    {
        charaDirectorData = new CharaDirectorData(_charaID, _dressID, _musicID, position);
    }

    public string[] GetAssetFiles()
    {
        List<string> tmp = new List<string>();
        return tmp.ToArray();
    }


    public string GetAccessoryModelPath(Character3DBase.AccessoryParts.eType type, int accIndex)
    {
        if (accIndex == -1)
        {
            accIndex = charaDirectorData.accIndex;
        }
        return Asset.GetAccessoryModelPath(charaDirectorData.activeAccessoryId, accIndex, charaDirectorData.resourceQuality, type);
    }

    public string GetAccessoryTexturePath(Character3DBase.MultiTextures.eMap map)
    {
        return Asset.GetAccessoryTexturePath(charaDirectorData.activeAccessoryId, charaDirectorData.colorId, charaDirectorData.accTexDiversity, charaDirectorData.accTexIndex, charaDirectorData.resourceQuality, map);
    }

    public string GetAccessoryParameterPath()
    {
        return Asset.GetAccessoryParameterPath(charaDirectorData.activeAccessoryId);
    }

    public string GetAccessoryClothPath(int accIndex, Character3DBase.AccessoryParts.eType type)
    {
        return Asset.GetAccessoryClothPath(charaDirectorData.activeAccessoryId, accIndex, type);
    }

    public string GetAccessoryCollisionPath(int accIndex, Character3DBase.AccessoryParts.eType type)
    {
        return Asset.GetAccessoryCollisionPath(charaDirectorData.activeAccessoryId, accIndex, type);
    }

    public string GetAccessoryFlareColliderPath(int accIndex, Character3DBase.AccessoryParts.eType type)
    {
        return Asset.GetAccessoryFlareColliderPath(charaDirectorData.activeAccessoryId, accIndex, type);
    }

    public string GetBodyModelPath(string partsCode)
    {
        return Asset.GetBodyModelPath(charaDirectorData.activeDressId, charaDirectorData.heightId, charaDirectorData.weightId, charaDirectorData.bustId, charaDirectorData.bodyIndex, partsCode, charaDirectorData.resourceQuality);
    }

    /// <summary>
    /// アペンドモデルのボディモデルパスを取得
    /// </summary>
    public string GetAppendBodyModelPath(string partsCode)
    {
        return Asset.GetBodyModelPath(appendCharaDirectorData.activeDressId, appendCharaDirectorData.heightId, appendCharaDirectorData.weightId, appendCharaDirectorData.bustId, appendCharaDirectorData.bodyIndex, partsCode, appendCharaDirectorData.resourceQuality);
    }

    public string GetBodyTexturePath(string partCode = "", Character3DBase.MultiTextures.eMap map = Character3DBase.MultiTextures.eMap.Diffuse)
    {
        return Asset.GetBodyTexturePath(charaDirectorData.charaId, charaDirectorData.activeDressId, charaDirectorData.skinId, charaDirectorData.bustId, charaDirectorData.colorId, charaDirectorData.bodyIndex, partCode, charaDirectorData.resourceQuality, map);
    }

    /// <summary>
    /// アペンドモデルのボディテクスチャを取得
    /// </summary>
    public string GetAppendBodyTexturePath(string partCode = "", Character3DBase.MultiTextures.eMap map = Character3DBase.MultiTextures.eMap.Diffuse)
    {
        return Asset.GetBodyTexturePath(appendCharaDirectorData.charaId, appendCharaDirectorData.activeDressId, appendCharaDirectorData.skinId, appendCharaDirectorData.bustId, appendCharaDirectorData.colorId, appendCharaDirectorData.bodyIndex, partCode, appendCharaDirectorData.resourceQuality, map);
    }

    public string GetBodyClothPath()
    {
        return Asset.GetBodyClothPath(charaDirectorData.activeDressId, charaDirectorData.bodyIndex);
    }

    /// <summary>
    /// アペンドモデルのクロスを取得
    /// </summary>
    public string GetAppendBodyClothPath()
    {
        return Asset.GetBodyClothPath(appendCharaDirectorData.activeDressId, appendCharaDirectorData.bodyIndex);
    }

    public string GetBodyCollisionPath()
    {
        return Asset.GetBodyCollisionPath(charaDirectorData.activeDressId, charaDirectorData.bodyIndex);
    }

    /// <summary>
    /// アペンドモデルのコリジョンを取得
    /// </summary>
    public string GetAppendBodyCollisionPath()
    {
        return Asset.GetBodyCollisionPath(appendCharaDirectorData.activeDressId, appendCharaDirectorData.bodyIndex);
    }

    public string GetBodyFlareColliderPath()
    {
        return Asset.GetBodyFlareColliderPath(charaDirectorData.activeDressId, charaDirectorData.heightId, charaDirectorData.weightId, charaDirectorData.bustId, charaDirectorData.bodyIndex);
    }

    /// <summary>
    /// アペンドモデルのフレアを取得
    /// </summary>
    /// <returns></returns>
    public string GetAppendBodyFlareColliderPath()
    {
        return Asset.GetBodyFlareColliderPath(appendCharaDirectorData.activeDressId, appendCharaDirectorData.heightId, appendCharaDirectorData.weightId, appendCharaDirectorData.bustId, appendCharaDirectorData.bodyIndex);
    }

    public string GetHeadModelPath()
    {
        return Asset.GetHeadModelPath(charaDirectorData.charaId, charaDirectorData.activeDressId, charaDirectorData.headIndex, charaDirectorData.isSubHeadIndex, charaDirectorData.resourceQuality);
    }

    public string GetSweatLocatorPath()
    {
        return Asset.GetSweatLocatorPath(charaDirectorData.charaId, charaDirectorData.activeDressId, charaDirectorData.headIndex, charaDirectorData.isSubHeadIndex);
    }

    public string GetFaceTexturePath(Character3DBase.MultiTextures.eMap map)
    {
        return Asset.GetFaceTexturePath(charaDirectorData.charaId, charaDirectorData.activeDressId, charaDirectorData.headIndex, charaDirectorData.headTextureIndex, charaDirectorData.isSubHeadIndex, map, charaDirectorData.resourceQuality);
    }

    public string GetObjectTexturePath(Character3DBase.MultiTextures.eMap map)
    {
        return Asset.GetObjectTexturePath(charaDirectorData.charaId, charaDirectorData.activeDressId, charaDirectorData.headIndex, charaDirectorData.headTextureIndex, charaDirectorData.isSubHeadIndex, map, charaDirectorData.resourceQuality);
    }

    public string GetCheekTexturePath(Character3DBase.MultiTextures.eMap map)
    {
        return Asset.GetCheekTexturePath(charaDirectorData.charaId, charaDirectorData.activeDressId, charaDirectorData.headIndex, charaDirectorData.isSubHeadIndex, map, charaDirectorData.resourceQuality);
    }

    public string GetHeadClothPath()
    {
        return Asset.GetHeadClothPath(charaDirectorData.charaId, charaDirectorData.activeDressId, charaDirectorData.headIndex, charaDirectorData.isSubHeadIndex);
    }

    public string GetHeadCollisionPath()
    {
        return Asset.GetHeadCollisionPath(charaDirectorData.charaId, charaDirectorData.activeDressId, charaDirectorData.headIndex, charaDirectorData.isSubHeadIndex);
    }

    public string GetHeadFlareColliderPath()
    {
        return Asset.GetHeadFlareColliderPath(charaDirectorData.charaId, charaDirectorData.activeDressId, charaDirectorData.headIndex, charaDirectorData.isSubHeadIndex);
    }

    public string GetFaceAnimatorPath()
    {
        return Asset.GetFaceAnimatorPath(charaDirectorData.charaId, charaDirectorData.activeDressId, charaDirectorData.headIndex, charaDirectorData.isSubHeadIndex);
    }

    public string GetOptionalDataPath(int optionName)
    {
        return Asset.GetOptionalDataPath(optionName);
    }

    public string GetAccessoryModelName(Character3DBase.AccessoryParts.eType type)
    {
        return AssetBundle.GetAccessoryModelName(charaDirectorData.activeAccessoryId, charaDirectorData.accIndex, type, charaDirectorData.resourceQuality);
    }

    public string GetAccessoryTextureName(Character3DBase.MultiTextures.eMap map)
    {
        return AssetBundle.GetAccessoryTextureName(charaDirectorData.activeAccessoryId, charaDirectorData.colorId, charaDirectorData.accTexDiversity, charaDirectorData.accTexIndex, charaDirectorData.resourceQuality, map);
    }

    public string GetAccessoryParameterName()
    {
        return AssetBundle.GetAccessoryParameterName(charaDirectorData.activeAccessoryId);
    }

    public string GetAccessoryFlareColliderName(Character3DBase.AccessoryParts.eType type)
    {
        return AssetBundle.GetAccessoryFlareColliderName(charaDirectorData.activeAccessoryId, charaDirectorData.accIndex, type);
    }

    public string GetBodyModelName(string partsCode)
    {
        return AssetBundle.GetBodyModelName(charaDirectorData.activeDressId, charaDirectorData.heightId, charaDirectorData.weightId, charaDirectorData.bustId, charaDirectorData.bodyIndex, partsCode, charaDirectorData.resourceQuality);
    }

    public string GetBodyTextureName(Character3DBase.MultiTextures.eMap map = Character3DBase.MultiTextures.eMap.Diffuse)
    {
        return AssetBundle.GetBodyTextureName(charaDirectorData.charaId, charaDirectorData.activeDressId, charaDirectorData.skinId, charaDirectorData.bustId, charaDirectorData.colorId, charaDirectorData.bodyIndex, charaDirectorData.resourceQuality, map);
    }

    public string GetBodyClothName()
    {
        return AssetBundle.GetBodyClothName(charaDirectorData.activeDressId, charaDirectorData.bodyIndex);
    }

    public string GetBodyFlareColliderName()
    {
        return AssetBundle.GetBodyFlareColliderName(charaDirectorData.activeDressId, charaDirectorData.heightId, charaDirectorData.weightId, charaDirectorData.bustId, charaDirectorData.bodyIndex);
    }

    public string GetHeadModelName()
    {
        return AssetBundle.GetHeadModelName(charaDirectorData.charaId, charaDirectorData.activeDressId, charaDirectorData.headIndex, charaDirectorData.isSubHeadIndex, charaDirectorData.resourceQuality);
    }

    public string GetHeadTextureName()
    {
        return AssetBundle.GetHeadTextureName(charaDirectorData.charaId, charaDirectorData.activeDressId, charaDirectorData.headIndex, charaDirectorData.headTextureIndex, charaDirectorData.isSubHeadIndex, charaDirectorData.resourceQuality);
    }

    public string GetHeadFlareColliderName()
    {
        return AssetBundle.GetHeadFlareColliderName(charaDirectorData.charaId, charaDirectorData.activeDressId, charaDirectorData.headIndex, charaDirectorData.isSubHeadIndex);
    }

    public string GetOptionalDataName(int optionName)
    {
        return AssetBundle.GetOptionalDataName(optionName);
    }

    public Character3DBase.CharacterData characterData
    {
        get
        {
            return charaDirectorData._characterData;
        }
    }

    public Character3DBase.CharacterData appendCharacterData
    {
        get
        {
            if (appendCharaDirectorData == null)
            {
                return null;
            }
            else
            {
                return appendCharaDirectorData._characterData;
            }
        }
    }
}
