using System;
using System.Collections.Generic;

namespace GamemodeManager.TemplateSystem
{
	public class Node
	{
		public Node(string key, bool isNested)
		{
			this.Key = key;
			this.IsNested = isNested;
		}

		public Node(string key)
		{
			this.Key = key;
		}

		public string Key { get; set; }

		public string Value { get; set; }

		public List<Tuple<string, string>> Values { get; set; }

		public bool IsNested { get; set; }

		public override bool Equals(object obj)
		{
			return obj is Node node &&
				   this.Key == node.Key &&
				   this.Value == node.Value &&
				   EqualityComparer<List<Tuple<string, string>>>.Default.Equals(this.Values, node.Values) &&
				   this.IsNested == node.IsNested;
		}

		public override int GetHashCode()
		{
			int hashCode = -1553581789;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Key);

			if (this.IsNested)
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Value);
			else
				hashCode = hashCode * -1521134295 + EqualityComparer<List<Tuple<string, string>>>.Default.GetHashCode(this.Values);

			hashCode = hashCode * -1521134295 + this.IsNested.GetHashCode();
			return hashCode;
		}
	}
}
