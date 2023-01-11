using System.Collections.Generic;

public class CySpringDataBucket<T> where T : class
{
	private Dictionary<string, List<T>> _mapDataList = new Dictionary<string, List<T>>();

	public Dictionary<string, List<T>> mapDataList => _mapDataList;

	public List<T> GetDataList(string partsCode)
	{
		List<T> value = null;
		if (_mapDataList != null && !_mapDataList.TryGetValue(partsCode, out value))
		{
			value = new List<T>();
			_mapDataList.Add(partsCode, value);
		}
		return value;
	}
}
