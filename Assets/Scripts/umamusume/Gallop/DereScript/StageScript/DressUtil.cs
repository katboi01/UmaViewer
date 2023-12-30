using System.Text.RegularExpressions;

namespace Stage
{
    public class DressUtil
    {
        public enum eAttributeType
        {
            Cute = 1,
            Cool,
            Passion,
            All,
            Max
        }

        public enum eDressAttribute
        {
            Cute = 1,
            Cool = 2,
            Passion = 4,
            All = 7
        }

        public enum eDressOpenType
        {
            Common = 0,
            SSR = 1,
            Buy = 2,
            AfterBuy = 3,
            Eisenofen = 4,
            GameCenter = 5,
            Community = 6,
            CantPossess = 99
        }

        public enum eDressType
        {
            Common,
            SSR,
            Unit,
            Attribute
        }

        public enum eDressCategoryType
        {
            Only3d,
            Both,
            ColorChange3d,
            Only2d,
            DressParts
        }

        public enum eDressPartsCategory
        {
            None,
            Tops,
            Bottoms,
            Max
        }

        public enum eDressPartsPriority
        {
            Main,
            Sub,
            Max
        }

        public const int UNIT_MOD_NUM = 1000;

        protected const string PETIT_TEXTURE_PATH = "Card/{0:D6}/live/live_{0:D6}_01";

        protected const string PETIT_COMMON_TEXTURE_PATH = "Live/DressPetit/{0:D7}/{1:D3}/live_{0:D7}_{1:D3}_01";

        protected const string PETIT_SERIES_TEXTURE_PATH = "Card/{0:D6}/petit/petit_{0:D6}";

        private static readonly Regex _regex = new Regex("［(?<value>.*?)］");

        private static readonly int NAME_INDEX = 1;

        public static int DRESS_CLOSET_HIDE_MASK = 16711680;

        /// <summary>
        /// ドレスIDから種類を取得する
        /// </summary>
        public static eDressType GetDressType(int dress_id)
        {
            if (IsAttributeDress(dress_id))
            {
                return eDressType.Attribute;
            }
            if (IsUnitDressId(dress_id))
            {
                return eDressType.Unit;
            }
            if (IsCommonDressId(dress_id))
            {
                return eDressType.Common;
            }
            return eDressType.SSR;
        }

        /// <summary>
        /// ドレスIDから共通衣装かどうかを取得する
        /// </summary>
        public static bool IsCommonDressId(int dress_id)
        {
            if (dress_id < 100)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// ドレスIDからユニット衣装(ショップ衣装)かどうかを取得する
        /// </summary>
        public static bool IsUnitDressId(int dress_id)
        {
            if (dress_id >= 7000000)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 属性衣装？未実装
        /// </summary>
        public static bool IsAttributeDress(int dress_id)
        {
            return false;
        }

        /// <summary>
        /// ドレスIDからパーツ衣装(体操服)かどうかを取得する
        /// </summary>
        public static bool IsPartsDress(int dressId)
        {
            MasterDressData.DressData dressData = MasterDBManager.instance.masterDressData.Get(dressId);
            if (dressData != null && dressData.dressType == 4)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// ドレスIDから旧SSR衣装かどうかを取得する
        /// </summary>
        public static int IsOldSSRDress(int dress_id)
        {
            if (dress_id >= 100 && dress_id < 200)
            {
                return dress_id % 100;
            }
            return -1;
        }

        public static int IsSSRDressGroup(int dress_id)
        {
            if (dress_id >= 100 && dress_id < 110)
            {
                return dress_id % 10;
            }
            return -1;
        }

        /// <summary>
        /// 衣装カード？が実装されたらここに入るっぽい
        /// </summary>
        public static bool IsDressCardId(int dressId)
        {
            //MVは999999のIDが降られるらしい
            if (dressId == 999999 || dressId == 999998)
            {
                return false;
            }
            if (dressId > 100000 && dressId < 7000000)
            {
                return true;
            }
            return false;
        }

        public static bool IsDressExtraId(int dressId)
        {
            if (dressId == 204)
            {
                return true;
            }
            return false;
        }


        public static int GetCurrentDressId(int dress_id)
        {
            if (IsUnitDressId(dress_id))
            {
                dress_id -= dress_id % 1000;
                return dress_id;
            }
            if (dress_id == 99)
            {
                return 99;
            }
            if (dress_id == 204)
            {
                return 204;
            }
            if (dress_id == 999998)
            {
                return 98;
            }
            if (dress_id == 999999)
            {
                return 97;
            }
            if (dress_id > 100)
            {
                if (dress_id > 100000)
                {
                    MasterCardData.CardData cardData = MasterDBManager.instance.masterCardData.Get(dress_id);
                    if (cardData != null)
                    {
                        if (MasterDBManager.instance.masterCharaData.CheckExcludeChara(null, GetCharaIdFromCardId(cardData.id)))
                        {
                            return 201;
                        }
                        int simpleRarityFromRarity = StageUtil.GetSimpleRarityFromRarity(cardData.rarity);
                        return 100 * (1 + (4 - simpleRarityFromRarity));
                    }
                }
                else if (dress_id % 10 != 0)
                {
                    return dress_id - dress_id % 10;
                }
            }
            if (dress_id == 0)
            {
                return 1;
            }
            return dress_id;
        }

        public static int GetCharaIdFromCardId(int cardId)
        {
            MasterCardData.CardData cardData = MasterDBManager.instance.masterCardData.Get(cardId);
            if (cardData != null)
            {
                return cardData.charaId;
            }
            return 0;
        }

        public static int GetCurrentDressId(int dress_id, int model_id)
        {
            if (dress_id == 0)
            {
                if (StageUtil.IsModelCommonDressId(model_id))
                {
                    return 1;
                }
                return 100;
            }
            return GetCurrentDressId(dress_id);
        }
        /*
        public static int ConvertRealCostumeId(int card_id, int dress_id)
        {
            int modelDressId = StageUtil.GetModelDressId(card_id);
            return GetCurrentDressId(dress_id, modelDressId);
        }
        */
        public static int ConvertUnitDressModelId(int dress_id, int chara_id, eAttributeType attribute)
        {
            if (!IsUnitDressId(dress_id))
            {
                return dress_id;
            }
            /*
            int currentDressId = GetCurrentDressId(dress_id);
            if (!IsEquipableDress(dress_id, chara_id, attribute))
            {
                return dress_id;
            }
            return currentDressId + chara_id;
            */
            return dress_id;
        }

        public static bool IsTargetAttribute(int attribute_bits, eAttributeType check_attribute)
        {
            int num = 1 << (int)(check_attribute - 1);
            int num2 = attribute_bits & num;
            if (num2 > 0)
            {
                return true;
            }
            return false;
        }

        public static int GetDressOpenType(int dressId)
        {
            if (MasterDBManager.instance == null)
            {
                return 0;
            }
            MasterDressData masterDressData = MasterDBManager.instance.masterDressData;
            if (masterDressData == null)
            {
                return 0;
            }
            MasterDressData.DressData dressData = masterDressData.Get(dressId);
            if (dressData != null)
            {
                return dressData.openType;
            }
            return 0;
        }

        /*
        public static bool IsEquipableCharaDress(int dress_id, int chara_id)
        {
            TempData.DressTempData dressTempData = SingletonMonoBehaviour<TempData>.instance.dressTempData;
            if (!dressTempData.IsExistCharaDressList(chara_id))
            {
                return false;
            }
            List<int> charaDressList = dressTempData.GetCharaDressList(chara_id);
            dress_id = GetCurrentDressId(dress_id);
            if (!charaDressList.Contains(dress_id))
            {
                return false;
            }
            return true;
        }

        public static bool IsEquipableAttributeDress(int dress_id, eAttributeType attribute)
        {
            TempData.DressTempData dressTempData = SingletonMonoBehaviour<TempData>.instance.dressTempData;
            if (!dressTempData.IsExistAttributeDressList(attribute))
            {
                return false;
            }
            List<int> attributeDressList = dressTempData.GetAttributeDressList(attribute);
            dress_id = GetCurrentDressId(dress_id);
            if (!attributeDressList.Contains(dress_id))
            {
                return false;
            }
            return true;
        }

        public static bool IsEquipableDress(int dress_id, int chara_id, eAttributeType attribute)
        {
            if (IsCommonDressId(dress_id))
            {
                return true;
            }
            if (IsEquipableCharaDress(dress_id, chara_id))
            {
                return true;
            }
            if (IsEquipableAttributeDress(dress_id, attribute))
            {
                return true;
            }
            return false;
        }

        public static bool IsUsableDress(MasterDressData.DressData dress_data, WorkDressData work_dress_data)
        {
            if ((int)dress_data.openType != 2)
            {
                return true;
            }
            if (work_dress_data.IsOwnDress(dress_data.id))
            {
                return true;
            }
            return false;
        }
        */
    }
}
