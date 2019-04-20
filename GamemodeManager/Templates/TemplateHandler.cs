using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Smod2;
using GamemodeManager.SmodAPI;

namespace GamemodeManager.Templates
{
	/// <summary>
	/// Handle a template
	/// </summary>
	public class TemplateHandler
	{
		private PluginGamemodeManager _plugin;

		/// <summary>
		/// Initializes a new instance of the <see cref="TemplateHandler"/> class
		/// </summary>
		/// <param name="plugin">Instance of the plugin</param>
		internal TemplateHandler(PluginGamemodeManager plugin) => this._plugin = plugin;

		public Template ReturnTemplate(string path)
		{
			string[] config = File.ReadAllLines(path);
			List<string> sectionBody = new List<string>();

			int index = 0;
			foreach (string line in config)
			{
				if (line.StartsWith("[") && line.EndsWith("]"))
				{
					sectionBody.Clear();
					string pluginId = line.Replace("[", string.Empty).Replace("]", string.Empty);
					Plugin gamemode = this._plugin.PluginManager.GetEnabledPlugin(pluginId);

					if (pluginId.ToUpper().Equals("DEFAULT"))
					{
						gamemode = this._plugin;
					}
					else if (gamemode == null)
					{
						gamemode = this._plugin;
						this._plugin.Warn("Can't find gamemode " + line);
					}

					index++;
				}
				else 
					sectionBody.Add(line);


			}



			Template template = new Template();



			return template;
		}

		/// <summary>
		/// Return a <see cref="IEnumerable{T}"/> of the section under the header
		/// </summary>
		/// <param name="lines">The lines beneath the header</param>
		/// <returns><see cref="IEnumerable{T}"/></returns>
		public IEnumerable<Tuple<string, string>> HandleSection(List<string> lines) =>
			lines.Where(line => line.TrimStart().StartsWith("-") && line.Contains(':'))
				.Select(line => new { line, key = Regex.Split(line, ":")[0].Trim() })
				.Select(t => new { t, value = Regex.Split(t.line, ":")[1].Trim() })
				.Select(t => new Tuple<string, string>(t.t.key, t.value));

		/// <summary>
		/// Return a tuple based off the inputted line
		/// </summary>
		/// <param name="line">Config line input</param>
		/// <returns>KeyValue pair</returns>
		public static Tuple<string, string> GetKeyPair(string line)
		{
			string[] split = Regex.Split(line, ": ");
			return split.Length != 2
					   ? new Tuple<string, string>(line, "invalid")
					   : new Tuple<string, string>(split[0], split[1]);
		}
	}
}
