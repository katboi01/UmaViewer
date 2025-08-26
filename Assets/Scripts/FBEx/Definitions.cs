
using System.Collections.Generic;
using System.Linq;

namespace FBEx
{
	public class Definitions : FbxObject
	{
		public Definitions()
		{
			Version = 100;
		}

		public string Get(List<FbxObject> Objects)
		{
			List<string> strings = new List<string>();

			strings.Add("Definitions:  {");
			strings.Add($"{Indent(1)}Version: {Version}");
			strings.Add($"{Indent(1)}Count: {Objects.Count}");

			var uniqueInstances = Objects
				.GroupBy(o => o.GetType())
				.Select(g => g.First())
				.ToList();

			foreach (var item in uniqueInstances)
			{
				strings.Add($"{Indent(1)}ObjectType: \"{item.GetType().Name}\" {{");
				strings.Add($"{Indent(2)}Count: {Objects.Where(o => o.GetType() == item.GetType()).Count()}");
				if (item.PropertyTemplate != "")
				{
					strings.Add($"{item.PropertyTemplate}");
				}
				strings.Add($"{Indent(1)}}}");
			}
			strings.Add("}");

			return string.Join("\n", strings);
		}
	}
}