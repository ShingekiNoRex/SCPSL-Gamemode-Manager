using System;

namespace GamemodeManager.Templates
{
	public class Template
	{
		public Template()
		{
			this.AssignProperties();
		}

		public Template(string pluginId, Tuple<string, string>[] rawValues)
		{
			this.PluginId = pluginId;
			this.RawValues = rawValues;

			this.AssignProperties();
		}

		public string PluginId { get; set; }

		public Tuple<string, string>[] RawValues { get; set; }

		public string TemplateName { get; private set; }

		public string Name { get; set; }

		public int Rounds { get; set; }

		public string SpawnQueue { get; set; }

		public string Description { get; set; }

		private void AssignProperties()
		{
			foreach ((string key, string value) in this.RawValues)
			{
				switch (key.ToUpper())
				{
					case "TEMPLATENAME":
						this.TemplateName = value;
						break;
					case "NAME":
						this.Name = value;
						break;
					case "ROUNDS":
						this.Rounds = int.TryParse(value, out int r) ? r : 1;
						break;
					case "SPAWNQUEUE":
						this.SpawnQueue = int.TryParse(string.Join(string.Empty, value.Split().Range(value.Length - 2)), out int f)
											  ? value
											  : "40143140314414041340";
						break;
					case "DESCRIPTION":
						this.Description = value;
						break;
					default:
						break;
				}
			}
		}
	}
}
