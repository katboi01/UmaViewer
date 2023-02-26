using System.Collections.Generic;

namespace Stage
{
    public class ConditionVmStack
    {
        public List<int> buffer = new List<int>();

        public int Pop()
        {
            int result = buffer[0];
            buffer.RemoveAt(0);
            return result;
        }

        public void Push(int item)
        {
            buffer.Insert(0, item);
        }

        public int[] Buffer()
        {
            return buffer.ToArray();
        }
    }
}
