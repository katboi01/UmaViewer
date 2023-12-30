using Cutt;
using System.Collections.Generic;
using System.Text;

internal class LabelToIDDictionary
{
    private Dictionary<int, int> _hashToIDDictionary = new Dictionary<int, int>();

    public LabelToIDDictionary(string header)
    {
        if (header != null && header.Length != 0)
        {
            byte[] array = new byte[1];
            for (int i = 0; i < 26; i++)
            {
                array[0] = (byte)(65 + i);
                string seed = header + Encoding.UTF8.GetString(array);
                _hashToIDDictionary.Add(FNVHash.Generate(seed), i);
            }
        }
    }

    public bool getID(int hash, out int id)
    {
        return _hashToIDDictionary.TryGetValue(hash, out id);
    }
}
