using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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


		/// <summary>
		/// Get the templates in the given path
		/// </summary>
		/// <param name="path">The path to the file of the given file</param>
		/// <returns><see cref="Templates"/></returns>
		public Templates GetTemplates(string path)
		{
			string[] config = File.ReadAllLines(path);
			List<string> sectionBody = new List<string>();
			Templates collection = new Templates();
			List<string> pluginIds = new List<string>();
			List<Template> templates = new List<Template>();

			int templateCount = 0;
			foreach (string line in config)
			{
				if (line.StartsWith("[") && line.EndsWith("]"))
				{
					if (!pluginIds.Any())
					{
						pluginIds.Add(line.Replace("[", string.Empty).Replace("]", string.Empty));
						continue;
					}

					IEnumerable<Tuple<string, string>> handled = this.HandleSection(sectionBody);
					Template template = new Template
						{
							PluginId = pluginIds[templateCount],
							RawValues = handled.ToArray()
						};
					templates.Add(template);
					sectionBody.Clear();
					templateCount++;
				}
				else 
					sectionBody.Add(line);
			}

			using (List<string>.Enumerator idsEnumerator = pluginIds.GetEnumerator())
			{
				using (List<Template>.Enumerator templatesEnumerator = templates.GetEnumerator())
				{
					while (idsEnumerator.MoveNext() && templatesEnumerator.MoveNext())
					{
						collection.Add(idsEnumerator.Current ?? string.Empty, templatesEnumerator.Current);
					}
				}
			}
			return collection;
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
	}
}
