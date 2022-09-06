using System;

namespace ClHcaSharp
{
    internal static class JaggedArray
    {
        public static T CreateJaggedArray<T>(params int[] lengths)
        {
            return (T)InitJaggedArray(typeof(T).GetElementType(), lengths, 0);
        }

        private static object InitJaggedArray(Type type, int[] lengths, int arrayIndex)
        {
            Array array = Array.CreateInstance(type, lengths[arrayIndex]);
            Type subElementType = type.GetElementType();

            if (type.HasElementType)
            {
                for (int i = 0; i < lengths[arrayIndex]; i++)
                {
                    array.SetValue(InitJaggedArray(subElementType, lengths, arrayIndex + 1), i);
                }
            }

            return array;
        }
    }
}