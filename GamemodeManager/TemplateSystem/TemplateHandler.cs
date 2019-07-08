using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GamemodeManager.SmodAPI;

namespace GamemodeManager.TemplateSystem
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

					Template template = new Template
					{
						PluginId = pluginIds[templateCount],
						Lines = config
					};

					AssignNodes(template, sectionBody.ToArray());

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

		private void AssignNodes(Template template, string[] lines)
		{
			int nest = 0;
			foreach (string line in lines)
			{
				if (!line.StartsWith("-"))
					continue;

				Node n = new Node(line.Split(':')[0]);
				if (line.Trim().EndsWith("{") && !line.Trim().EndsWith(@"\{"))
					nest++;

				if (nest == 0)
				{
					n.IsNested = false;
					n.Value = line.Split(':')[1];
					return;
				}

				while (nest > 0)
				{
					n.IsNested = true;
					bool reachedCurrent = false;

					using (IEnumerator<string> e = ((IEnumerable<string>)lines).GetEnumerator())
					{
						while (e.MoveNext())
						{
							if (e.Current != line && !reachedCurrent)
								continue;

							if (e.Current == line)
							{
								reachedCurrent = true;
								continue;
							}

							if (reachedCurrent)
							{
								if (line.Contains(":"))
									n.Values.Add(new Tuple<string, string>(e.Current.Split(':')[0], e.Current.Split(':')[1]));
								else
									n.Values.Add(new Tuple<string, string>(e.Current, null));

								if (line.Trim().EndsWith("}") && !line.Trim().EndsWith(@"\}"))
								{
									nest--;
									break;
								}
							}
						}
					}
				}

				template.Nodes.Add(n); 
			}
		}
	}
}
