using Smod2.API;
using System.Collections.Generic;
using Smod2.EventHandlers;
using Smod2.EventSystem.Events;
using Smod2.Events;
using System.IO;

namespace Smod2.Handler
{
	class SmodEventHandler : IEventHandlerWaitingForPlayers, IEventHandlerRoundRestart, IEventHandlerDecideTeamRespawnQueue, IEventHandlerSetServerName, IEventHandlerPlayerJoin
	{
		private Plugin plugin;
		public SmodEventHandler(Plugin plugin)
		{
			this.plugin = plugin;
		}

		private static int ModeCount = 0;

		private static bool FirstRoundComplete;

		public void OnPlayerJoin(PlayerJoinEvent ev)
		{
			ev.Player.SendConsoleMessage(System.Environment.NewLine + plugin.GetTranslation("GM_CURRENT_MODE") + ": " + GamemodeManager.GamemodeManager.CurrentName + System.Environment.NewLine + plugin.GetTranslation("GM_DESCRIPTION") + ": " + System.Environment.NewLine + GamemodeManager.GamemodeManager.CurrentDescription.Replace("<n>", System.Environment.NewLine), "red");
		}

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			// debug
		//	this.plugin.Info("DEBUG: gm_enable=" + this.plugin.GetConfigBool("gm_enable"));
		//	this.plugin.Info("DEBUG: GamemodeManager.GamemodeManager.EnabledRounds=" + GamemodeManager.GamemodeManager.EnabledRounds);
			// end debug

			if (!this.plugin.GetConfigBool("gm_enable") && GamemodeManager.GamemodeManager.EnabledRounds == 0) GamemodeManager.GamemodeManager.DisableAll = true;
			if (FirstRoundComplete) return;

			string path = ConfigManager.Manager.Config.GetConfigPath().Replace("config_gameplay.txt", "sm_config_gamemode.txt");
			if (File.Exists(path))
			{
				int line = -1;
				List<Plugin> modeList = new List<Plugin>();
				List<string> template = new List<string>();
				List<string> name = new List<string>();
				List<int> rounds = new List<int>();
				List<string> spawnqueue = new List<string>();
				List<string> description = new List<string>();
				GamemodeManager.GamemodeManager.ModeList.Clear();
				GamemodeManager.GamemodeManager.Template.Clear();
				GamemodeManager.GamemodeManager.ModeName.Clear();
				GamemodeManager.GamemodeManager.SpawnQueue.Clear();
				GamemodeManager.GamemodeManager.Descriptions.Clear();

				string[] config = File.ReadAllLines(path);
				foreach (string configLine in config)
				{
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
						template.Add("");
						name.Add(gamemode.Equals(this.plugin) ? "Default" : gamemode.Details.name);
						rounds.Add(0);
						spawnqueue.Add("40143140314414041340");
						description.Add(gamemode.Equals(this.plugin) ? "Standard Mode" : gamemode.Details.description);
					}
					else if (line > -1 && configLine.Contains("-"))
					{
						string[] keyValue = System.Text.RegularExpressions.Regex.Split(configLine, ": ");
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
				}
				// debug
				this.plugin.Info("DEBUG: config read successfully");
				// end debug

				for (int i = 0; i <= line; i++)
				{
					for (int j = 0; j < rounds[i]; j++)
					{
						GamemodeManager.GamemodeManager.ModeList.Add(modeList[i]);
						GamemodeManager.GamemodeManager.Template.Add(template[i]);
						GamemodeManager.GamemodeManager.ModeName.Add(name[i]);
						GamemodeManager.GamemodeManager.Descriptions.Add(description[i]);
						List<Team> classTeamQueue = new List<Team>();
						for (int k = 0; k < spawnqueue[i].Length; k++)
						{
							int item = 4;
							if (!int.TryParse(spawnqueue[i][k].ToString(), out item))
							{
								item = 4;
							}
							classTeamQueue.Add((Team)item);
						}
						GamemodeManager.GamemodeManager.SpawnQueue.Add(classTeamQueue.ToArray());
					}
				}
			}

			int queue2 = 0;
			List<string> templates = new List<string>(ConfigManager.Manager.Config.GetListValue("gm_round_sequence", true));
			if (templates.Count > 0 && !string.IsNullOrEmpty(templates[0]) && GamemodeManager.GamemodeManager.Template.Contains(templates[0]))
			{
				queue2 = GamemodeManager.GamemodeManager.Template.FindIndex(x => x.Equals(templates[0]));
			}

			GamemodeManager.GamemodeManager.CurrentMode = GamemodeManager.GamemodeManager.ModeList[queue2];
			GamemodeManager.GamemodeManager.CurrentName = GamemodeManager.GamemodeManager.ModeName[queue2];
			GamemodeManager.GamemodeManager.CurrentQueue = GamemodeManager.GamemodeManager.SpawnQueue[queue2];
			GamemodeManager.GamemodeManager.CurrentDescription = GamemodeManager.GamemodeManager.Descriptions[queue2];
			ModeCount++;
			FirstRoundComplete = true;
		}

		public void OnRoundRestart(RoundRestartEvent ev)
		{
			if (GamemodeManager.GamemodeManager.DisableAll && GamemodeManager.GamemodeManager.EnabledRounds == 0)
			{
				GamemodeManager.GamemodeManager.CurrentMode = null;
				GamemodeManager.GamemodeManager.CurrentName = "Default";
			}
			else
			{
				if (GamemodeManager.GamemodeManager.NextMode != null)
				{
					GamemodeManager.GamemodeManager.CurrentMode = GamemodeManager.GamemodeManager.NextMode;
					GamemodeManager.GamemodeManager.CurrentName = GamemodeManager.GamemodeManager.NextName;
					GamemodeManager.GamemodeManager.CurrentQueue = GamemodeManager.GamemodeManager.NextQueue;
					GamemodeManager.GamemodeManager.CurrentDescription = GamemodeManager.GamemodeManager.CurrentMode.Equals(this.plugin) ? "Standard Mode" : GamemodeManager.GamemodeManager.CurrentMode.Details.description;
					GamemodeManager.GamemodeManager.NextMode = null;
					GamemodeManager.GamemodeManager.NextName = null;
					GamemodeManager.GamemodeManager.NextQueue = null;
					this.plugin.Info("Changing mode to [" + (GamemodeManager.GamemodeManager.CurrentMode.Equals(this.plugin) ? "Default" : GamemodeManager.GamemodeManager.CurrentMode.ToString()) + "] (" + GamemodeManager.GamemodeManager.CurrentName + ") (" + GamemodeManager.GamemodeManager.QueueToString(GamemodeManager.GamemodeManager.CurrentQueue) + ")");
				}
				else if (GamemodeManager.GamemodeManager.ModeList.Count > 0)
				{
					List<string> templates = new List<string>(ConfigManager.Manager.Config.GetListValue("gm_round_sequence", true));

					if (templates.Count > 0 && !string.IsNullOrEmpty(templates[ModeCount - 1]) && GamemodeManager.GamemodeManager.Template.Contains(templates[ModeCount - 1]))
					{
						int queue = GamemodeManager.GamemodeManager.Template.FindIndex(x => x.Equals(templates[ModeCount - 1]));
						GamemodeManager.GamemodeManager.CurrentMode = GamemodeManager.GamemodeManager.ModeList[queue];
						GamemodeManager.GamemodeManager.CurrentName = GamemodeManager.GamemodeManager.ModeName[queue];
						GamemodeManager.GamemodeManager.CurrentQueue = GamemodeManager.GamemodeManager.SpawnQueue[queue];
						GamemodeManager.GamemodeManager.CurrentDescription = GamemodeManager.GamemodeManager.Descriptions[queue];
						this.plugin.Info("Changing mode to [" + templates[ModeCount - 1] + "] [" + (GamemodeManager.GamemodeManager.CurrentMode.Equals(this.plugin) ? "Default" : GamemodeManager.GamemodeManager.CurrentMode.ToString()) + "] (" + GamemodeManager.GamemodeManager.CurrentName + ") (" + GamemodeManager.GamemodeManager.QueueToString(GamemodeManager.GamemodeManager.CurrentQueue) + ")");
						ModeCount++;
						if (ModeCount >= templates.Count)
						{
							ModeCount = 0;
						}
					}
					else
					{
						GamemodeManager.GamemodeManager.CurrentMode = GamemodeManager.GamemodeManager.ModeList[ModeCount];
						GamemodeManager.GamemodeManager.CurrentName = GamemodeManager.GamemodeManager.ModeName[ModeCount];
						GamemodeManager.GamemodeManager.CurrentQueue = GamemodeManager.GamemodeManager.SpawnQueue[ModeCount];
						GamemodeManager.GamemodeManager.CurrentDescription = GamemodeManager.GamemodeManager.Descriptions[ModeCount++];
						plugin.Info("Changing mode to [" + (GamemodeManager.GamemodeManager.CurrentMode.Equals(this.plugin) ? "Default" : GamemodeManager.GamemodeManager.CurrentMode.ToString()) + "] (" + GamemodeManager.GamemodeManager.CurrentName + ") (" + GamemodeManager.GamemodeManager.QueueToString(GamemodeManager.GamemodeManager.CurrentQueue) + ")");

						if (ModeCount >= GamemodeManager.GamemodeManager.ModeList.Count)
						{
							ModeCount = 0;
						}
					}
				}

				GamemodeManager.GamemodeManager.EnabledRounds--;
			}
		}

		public void OnDecideTeamRespawnQueue(DecideRespawnQueueEvent ev)
		{
			if (GamemodeManager.GamemodeManager.ModeList.Count > 0 && !GamemodeManager.GamemodeManager.DisableAll)
			{
				string result = "";
				foreach (Team team in ev.Teams)
				{
					result = result + (int)team;
				}
				ev.Teams = GamemodeManager.GamemodeManager.CurrentQueue;
			}
		}

		public void OnSetServerName(SetServerNameEvent ev)
		{
			if (ev.ServerName.Contains("$gamemode"))
			{
				ev.ServerName = ev.ServerName.Replace("$gamemode", GamemodeManager.GamemodeManager.CurrentName);
			}
		}
	}
}