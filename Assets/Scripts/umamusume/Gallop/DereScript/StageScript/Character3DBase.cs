using UnityEngine;

public class Character3DBase : MonoBehaviour
{
    public abstract class Parts
    {
        public enum eCategory
        {
            Head = 0,
            Body = 1,
            Accessory = 2,
            Unknown = 3,
            MAX = 4,
            Invalid = 4
        }

        public static bool IsValidPartsCode(string partsCode)
        {
            bool result = false;
            int result2 = 0;
            if (!string.IsNullOrEmpty(partsCode) && partsCode.Length == 4 && int.TryParse(partsCode, out result2))
            {
                result = result2 >= 0;
            }
            return result;
        }
    }
}