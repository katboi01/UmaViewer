using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Cutt
{
    public static class CuttControlCommon
    {
        public static bool ResaizeArray<T>(ref T[] Data, int newCount) where T : new()
        {
            if (Data.Length < newCount)
            {
                T target = (Data.Length <= 0) ? new T() : Data[Data.Length - 1];
                int num = Data.Length;
                Array.Resize(ref Data, newCount);
                for (int i = num; i < Data.Length; i++)
                {
                    Data[i] = AllCopy(target);
                }
            }
            else if (Data.Length > newCount)
            {
                Array.Resize(ref Data, newCount);
            }
            return true;
        }

        public static object AllCopy(this object target)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                binaryFormatter.Serialize(memoryStream, target);
                memoryStream.Position = 0L;
                return binaryFormatter.Deserialize(memoryStream);
            }
            finally
            {
                memoryStream.Close();
            }
        }

        public static T AllCopy<T>(T target)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                binaryFormatter.Serialize(memoryStream, target);
                memoryStream.Position = 0L;
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
            finally
            {
                memoryStream.Close();
            }
        }
    }
}
