using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.EventSystem.Events;

namespace GamemodeManager.SmodAPI
{
	internal class SmodEventHandler : IEventHandlerWaitingForPlayers, IEventHandlerRoundRestart, IEventHandlerDecideTeamRespawnQueue, IEventHandlerSetServerName, IEventHandlerPlayerJoin
	{
		private static bool FirstRoundComplete;

		private static int ModeCount;

		private readonly PluginGamemodeManager plugin;

		public SmodEventHandler(PluginGamemodeManager plugin)
		{
			this.plugin = plugin;
		}

		public void OnDecideTeamRespawnQueue(DecideRespawnQueueEvent ev)
		{
			if (GamemodeManager.ModeList.Count > 0 && !GamemodeManager.DisableAll)
			{
				string result = string.Empty;
				foreach (Team team in ev.Teams) result = result + (int)team;
				ev.Teams = GamemodeManager.CurrentQueue;
			}
		}

		public void OnPlayerJoin(PlayerJoinEvent ev)
		{
			ev.Player.SendConsoleMessage(
				Environment.NewLine + this.plugin.GetTranslation("GM_CURRENT_MODE") + ": " + GamemodeManager.CurrentName
				+ Environment.NewLine + this.plugin.GetTranslation("GM_DESCRIPTION") + ": " + Environment.NewLine
				+ GamemodeManager.CurrentDescription.Replace("<n>", Environment.NewLine),
				"red");
		}

		public void OnRoundRestart(RoundRestartEvent ev)
		{
			if (GamemodeManager.DisableAll && GamemodeManager.EnabledRounds == 0)
			{
				GamemodeManager.CurrentMode = null;
				GamemodeManager.CurrentName = "Default";
			}
			else
			{
				if (GamemodeManager.NextMode != null)
				{
					GamemodeManager.CurrentMode = GamemodeManager.NextMode;
					GamemodeManager.CurrentName = GamemodeManager.NextName;
					GamemodeManager.CurrentQueue = GamemodeManager.NextQueue;
					GamemodeManager.CurrentDescription = GamemodeManager.CurrentMode.Equals(this.plugin)
															 ? "Standard Mode"
															 : GamemodeManager.CurrentMode.Details.description;
					GamemodeManager.NextMode = null;
					GamemodeManager.NextName = null;
					GamemodeManager.NextQueue = null;
					this.plugin.Info(
						"Changing mode to ["
						+ (GamemodeManager.CurrentMode.Equals(this.plugin)
							   ? "Default"
							   : GamemodeManager.CurrentMode.ToString()) + "] (" + GamemodeManager.CurrentName + ") ("
						+ GamemodeManager.QueueToString(GamemodeManager.CurrentQueue) + ")");
				}
				else if (GamemodeManager.ModeList.Count > 0)
				{
					List<string> templates = new List<string>(
						Smod2.ConfigManager.Manager.Config.GetListValue("gm_round_sequence", true));

					// debug
					for (int i = 0; i < templates.Count; i++) this.plugin.Info($"DEBUG: templates[{i}]={templates[i]}");

					if (templates.Count > 0 && !string.IsNullOrEmpty(templates[ModeCount - 1])
											&& GamemodeManager.Templates.Contains(templates[ModeCount - 1]))
					{
						int queue = GamemodeManager.Templates.FindIndex(x => x.Equals(templates[ModeCount - 1]));
						GamemodeManager.CurrentMode = GamemodeManager.ModeList[queue];
						GamemodeManager.CurrentName = GamemodeManager.ModeName[queue];
						GamemodeManager.CurrentQueue = GamemodeManager.SpawnQueue[queue];
						GamemodeManager.CurrentDescription = GamemodeManager.Descriptions[queue];
						this.plugin.Info(
							"Changing mode to [" + templates[ModeCount - 1] + "] ["
							+ (GamemodeManager.CurrentMode.Equals(this.plugin)
								   ? "Default"
								   : GamemodeManager.CurrentMode.ToString()) + "] (" + GamemodeManager.CurrentName
							+ ") (" + GamemodeManager.QueueToString(GamemodeManager.CurrentQueue) + ")");
						ModeCount++;
						if (ModeCount >= templates.Count) ModeCount = 0;
					}
					else
					{
						GamemodeManager.CurrentMode = GamemodeManager.ModeList[ModeCount];
						GamemodeManager.CurrentName = GamemodeManager.ModeName[ModeCount];
						GamemodeManager.CurrentQueue = GamemodeManager.SpawnQueue[ModeCount];
						GamemodeManager.CurrentDescription = GamemodeManager.Descriptions[ModeCount++];
						this.plugin.Info(
							"Changing mode to ["
							+ (GamemodeManager.CurrentMode.Equals(this.plugin)
								   ? "Default"
								   : GamemodeManager.CurrentMode.ToString()) + "] (" + GamemodeManager.CurrentName
							+ ") (" + GamemodeManager.QueueToString(GamemodeManager.CurrentQueue) + ")");

						if (ModeCount >= GamemodeManager.ModeList.Count) ModeCount = 0;
					}
				}

				GamemodeManager.EnabledRounds--;
			}
		}

		public void OnSetServerName(SetServerNameEvent ev)
		{
			if (ev.ServerName.Contains("$gamemode"))
				ev.ServerName = ev.ServerName.Replace("$gamemode", GamemodeManager.CurrentName);
		}

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			// debug
			// 	this.plugin.Info("DEBUG: gm_enable=" + this.plugin.GetConfigBool("gm_enable"));
			// 	this.plugin.Info("DEBUG: GamemodeManager.GamemodeManager.EnabledRounds=" + GamemodeManager.GamemodeManager.EnabledRounds);
			// end debug
			if (!this.plugin.GetConfigBool("gm_enable") && GamemodeManager.EnabledRounds == 0)
				GamemodeManager.DisableAll = true;
			if (FirstRoundComplete) return;

			string path = ConfigManager.Manager.Config.GetConfigPath()
				.Replace("config_gameplay.txt", "sm_config_gamemode.txt");
			if (File.Exists(path))
			{
				int line = -1;
				List<Plugin> modeList = new List<Plugin>();
				List<string> template = new List<string>();
				List<string> name = new List<string>();
				List<int> rounds = new List<int>();
				List<string> spawnqueue = new List<string>();
				List<string> description = new List<string>();
				GamemodeManager.ModeList.Clear();
				GamemodeManager.Templates.Clear();
				GamemodeManager.ModeName.Clear();
				GamemodeManager.SpawnQueue.Clear();
				GamemodeManager.Descriptions.Clear();

				string[] config = File.ReadAllLines(path);
				foreach (string configLine in config)
					if (configLine.Contains("[") && configLine.Contains("]"))
					{
						string pluginId = configLine.Replace("[", string.Empty).Replace("]", string.Empty);
						Plugin gamemode = this.plugin.PluginManager.GetEnabledPlugin(pluginId);
						if (pluginId.ToUpper().Equals("DEFAULT"))
						{
							gamemode = this.plugin;
						}
						else if (gamemode == null)
						{
							gamemode = this.plugin;
							this.plugin.Warn("Can't find gamemode " + configLine);
						}

						line++;
						modeList.Add(gamemode);
						template.Add(string.Empty);
						name.Add(gamemode.Equals(this.plugin) ? "Default" : gamemode.Details.name);
						rounds.Add(0);
						spawnqueue.Add("40143140314414041340");
						description.Add(gamemode.Equals(this.plugin) ? "Standard Mode" : gamemode.Details.description);
					}
					else if (line > -1 && configLine.Contains("-"))
					{
						string[] keyValue = Regex.Split(configLine, ": ");
						keyValue[0] = keyValue[0].Replace("-", string.Empty).Trim();
						switch (keyValue[0].ToUpper())
						{
							case "TEMPLATENAME":
								{
									template[line] = keyValue[1];
									break;
								}

							case "NAME":
								{
									name[line] = keyValue[1];
									break;
								}

							case "ROUNDS":
								{
									rounds[line] = int.TryParse(keyValue[1], out int temp) ? temp : 1;
									break;
								}

							case "SPAWNQUEUE":
								{
									spawnqueue[line] = keyValue[1];
									break;
								}

							case "DESCRIPTION":
								{
									description[line] = keyValue[1];
									break;
								}
						}
					}

				// debug
				this.plugin.Info("DEBUG: config read successfully");

				// end debug
				for (int i = 0; i <= line; i++)
				{
					for (int j = 0; j < rounds[i]; j++)
					{
						GamemodeManager.ModeList.Add(modeList[i]);
						GamemodeManager.Templates.Add(template[i]);
						GamemodeManager.ModeName.Add(name[i]);
						GamemodeManager.Descriptions.Add(description[i]);
						List<Team> classTeamQueue = new List<Team>();
						for (int k = 0; k < spawnqueue[i].Length; k++)
						{
							int item = 4;
							if (!int.TryParse(spawnqueue[i][k].ToString(), out item)) item = 4;

							classTeamQueue.Add((Team)item);
						}

						GamemodeManager.SpawnQueue.Add(classTeamQueue.ToArray());
					}
				}
			}

			int queue2 = 0;
			List<string> templates =
				new List<string>(ConfigManager.Manager.Config.GetListValue("gm_round_sequence", true));
			if (templates.Count > 0 && !string.IsNullOrEmpty(templates[0])
									&& GamemodeManager.Templates.Contains(templates[0]))
				queue2 = GamemodeManager.Templates.FindIndex(x => x.Equals(templates[0]));

			GamemodeManager.CurrentMode = GamemodeManager.ModeList[queue2];
			GamemodeManager.CurrentName = GamemodeManager.ModeName[queue2];
			GamemodeManager.CurrentQueue = GamemodeManager.SpawnQueue[queue2];
			GamemodeManager.CurrentDescription = GamemodeManager.Descriptions[queue2];
			ModeCount++;
			FirstRoundComplete = true;
		}
	}
}