using System;
using System.Collections.Generic;

namespace GamemodeManager.TemplateSystem
{
	public class Template
	{
		public Template()
		{
			this.AssignProperties();
		}

		public Template(string pluginId, string[] lines)
		{
			this.PluginId = pluginId;
			this.Lines = lines;

			this.AssignProperties();
		}

		public string PluginId { get; set; }

		public string[] Lines { get; set; }

		public string TemplateName { get; private set; }

		public string Name { get; set; }

		public int Rounds { get; set; }

		public string SpawnQueue { get; set; }

		public string Description { get; set; }

		public List<Node> Nodes { get; set; }

		

		private void AssignProperties()
		{
			foreach (Node node in this.Nodes)
			{
				switch (node.Key.ToUpper())
				{
					case "TEMPLATENAME":
						this.TemplateName = node.Value;
						break;
					case "NAME":
						this.Name = node.Value;
						break;
					case "ROUNDS":
						this.Rounds = int.TryParse(node.Value, out int r) ? r : 1;
						break;
					case "SPAWNQUEUE":
						this.SpawnQueue = int.TryParse(string.Join(string.Empty, node.Value.Split().Range(node.Value.Length - 2)), out int f)
											  ? node.Value
											  : "40143140314414041340";
						break;
					case "DESCRIPTION":
						this.Description = node.Value;
						break;
					default:
						break;
				}
			}
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return base.ToString();
		}
	}
}
