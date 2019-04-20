using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamemodeManager.Templates
{
	public class Template
	{
		public Template()
		{

		}

		public Template(string name, Tuple<string, string>[] values)
		{
			this.Name = name;
			this.Values = values;
		}

		public string Name { get; set; }

		public Tuple<string, string>[] Values { get; set; }
	}
}
