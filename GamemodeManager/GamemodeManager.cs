using Smod2;
using Smod2.Attributes;
using Smod2.API;
using Smod2.Commands;
using Smod2.Handler;
using System.Collections.Generic;
using Smod2.EventHandlers;
using Smod2.EventSystem.Events;
using Smod2.Events;
using System.IO;

namespace Smod2.Plugins
{
	[PluginDetails(
		author = "ShingekiNoRex",
		name = "GamemodeManager",
		description = "",
		id = "rex.gamemode.manager",
		version = "2.1",
		SmodMajor = 3,
		SmodMinor = 1,
		SmodRevision = 15
	)]
	class PluginGamemodeManager : Plugin
	{
		public override void OnDisable()
		{
		}

		public override void OnEnable()
		{
			this.Info("Gamemode Manager has been enabled :)");
		}

		public override void Register()
		{
			// Register Events
			this.AddEventHandlers(new SmodEventHandler(this));
			this.AddConfig(new Config.ConfigSetting("gm_round_sequence", "", Config.SettingType.LIST, true, ""));
			this.AddCommand("gamemode", new CommandHandler(this));
			this.AddTranslation(new Lang.LangSetting("GM_CURRENT_MODE", "Current Mode"));
			this.AddTranslation(new Lang.LangSetting("GM_DESCRIPTION", "Description"));
		}
	}
}

namespace GamemodeManager
{
	public abstract class GamemodeManager
	{
		public static Plugin CurrentMode;
		public static Plugin NextMode;
		public static Team[] CurrentQueue;
		public static Team[] NextQueue;
		public static string CurrentName;
		public static string NextName;
		public static string CurrentDescription;
		public static List<Plugin> ModeList = new List<Plugin>();
		public static List<Team[]> SpawnQueue = new List<Team[]>();
		public static List<string> Template = new List<string>();
		public static List<string> ModeName = new List<string>();
		public static List<string> Descriptions = new List<string>();
		public static bool DisableAll;

		public static void RegisterMode(Plugin gamemode, string spawnqueue = "-1")
		{
			CurrentMode = gamemode;
			CurrentName = gamemode.Details.name;
			CurrentDescription = gamemode.Details.description;
			ModeList.Add(gamemode);
			ModeName.Add(CurrentName);
			Descriptions.Add(CurrentDescription);
			gamemode.Info("[GamemodeManager] " + gamemode.ToString() + " has been registered.");

			if (spawnqueue.Equals("-1"))
			{
				spawnqueue = ConfigManager.Manager.Config.GetStringValue("team_respawn_queue", "40143140314414041340");
			}

			List<Team> classTeamQueue = new List<Team>();
			for (int i = 0; i < spawnqueue.Length; i++)
			{
				int item = 4;
				if (!int.TryParse(spawnqueue[i].ToString(), out item))
				{
					item = 4;
				}
				classTeamQueue.Add((Team)item);
			}
			CurrentQueue = classTeamQueue.ToArray();
			SpawnQueue.Add(CurrentQueue);
		}

		public static void SetNextMode(Plugin gamemode, string spawnqueue = "-1", string name = null)
		{
			NextMode = gamemode;
			NextName = string.IsNullOrEmpty(name) ? gamemode.Details.name : name;

			if (spawnqueue.Equals("-1"))
			{
				NextQueue = SpawnQueue[ModeList.FindIndex(x => x.Equals(gamemode))];
			}
			else
			{
				List<Team> classTeamQueue = new List<Team>();
				for (int i = 0; i < spawnqueue.Length; i++)
				{
					int item = 4;
					if (!int.TryParse(spawnqueue[i].ToString(), out item))
					{
						item = 4;
					}
					classTeamQueue.Add((Team)item);
				}
				NextQueue = classTeamQueue.ToArray();
			}
		}

		public static Plugin GetCurrentMode()
		{
			return CurrentMode;
		}

		public static Team[] GetCurrentQueue()
		{
			return CurrentQueue;
		}

		public static string GetCurrentName()
		{
			return CurrentName;
		}

		/*
		public static Team[] GetModeQueue(Plugin gamemode)
		{
			return SpawnQueue[ModeList.FindIndex(x => x.Equals(gamemode))];
		}

		public static string GetModeName(Plugin gamemode)
		{
			return ModeName[ModeList.FindIndex(x => x.Equals(gamemode))];
		}
		*/

		public static List<Plugin> GetModeList()
		{
			return ModeList;
		}

		public static string QueueToString(Team[] queue)
		{
			string result = "";
			foreach (Team team in queue)
			{
				result = result + (int)team;
			}

			return result;
		}
	}
}

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

		private static bool FirstRound;

		public void OnPlayerJoin(PlayerJoinEvent ev)
		{
			ev.Player.SendConsoleMessage(System.Environment.NewLine + plugin.GetTranslation("GM_CURRENT_MODE") + ": " + GamemodeManager.GamemodeManager.CurrentName + System.Environment.NewLine + plugin.GetTranslation("GM_DESCRIPTION") + ": " + System.Environment.NewLine + GamemodeManager.GamemodeManager.CurrentDescription.Replace("<n>", System.Environment.NewLine), "red");
		}

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			if (!FirstRound)
			{
				string path = ConfigManager.Manager.Config.GetConfigPath().Replace("config_gameplay.txt", "sm_config_gamemode.txt");
				if (File.Exists(path))
				{
					int queue = -1;
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
					foreach (string line in config)
					{
						if (line.Contains("[") && line.Contains("]"))
						{
							string pluginid = line.Replace("[", string.Empty).Replace("]", string.Empty);
							Plugin gamemode = plugin.pluginManager.GetEnabledPlugin(pluginid);
							if (pluginid.ToUpper().Equals("DEFAULT"))
							{
								gamemode = this.plugin;
							}
							else if (gamemode == null)
							{
								gamemode = this.plugin;
								plugin.Warn("Can't find gamemode " + line);
							}
							queue++;
							modeList.Add(gamemode);
							template.Add("");
							name.Add(gamemode.Equals(this.plugin) ? "Default" : gamemode.Details.name);
							rounds.Add(0);
							spawnqueue.Add("40143140314414041340");
							description.Add(gamemode.Equals(this.plugin) ? "Standard Mode" : gamemode.Details.description);
						}
						else if (queue > -1 && line.Contains("-"))
						{
							string[] keyvalue = System.Text.RegularExpressions.Regex.Split(line, ": ");
							keyvalue[0] = keyvalue[0].Replace("-", string.Empty).Trim();
							switch(keyvalue[0].ToUpper())
							{
								case "TEMPLATENAME":
									{
										template[queue] = keyvalue[1];
										break;
									}
								case "NAME":
									{
										name[queue] = keyvalue[1];
										break;
									}
								case "ROUNDS":
									{
										rounds[queue] = System.Convert.ToInt32(keyvalue[1]);
										break;
									}
								case "SPAWNQUEUE":
									{
										spawnqueue[queue] = keyvalue[1];
										break;
									}
								case "DESCRIPTION":
									{
										description[queue] = keyvalue[1];
										break;
									}
							}
						}
					}
					for (int i = 0; i <= queue; i++)
					{
						for (int j = 0; j < rounds[i]; j ++)
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
				FirstRound = true;
			}
		}

		public void OnRoundRestart(RoundRestartEvent ev)
		{
			if (GamemodeManager.GamemodeManager.DisableAll)
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
					plugin.Info("Changing mode to [" + (GamemodeManager.GamemodeManager.CurrentMode.Equals(this.plugin) ? "Default" : GamemodeManager.GamemodeManager.CurrentMode.ToString()) + "] (" + GamemodeManager.GamemodeManager.CurrentName + ") (" + GamemodeManager.GamemodeManager.QueueToString(GamemodeManager.GamemodeManager.CurrentQueue) + ")");
				}
				else if (GamemodeManager.GamemodeManager.ModeList.Count > 0)
				{
					List<string> templates = new List<string>(ConfigManager.Manager.Config.GetListValue("gm_round_sequence", true));
					if (templates.Count > 0 && !string.IsNullOrEmpty(templates[ModeCount]) && GamemodeManager.GamemodeManager.Template.Contains(templates[ModeCount]))
					{
						int queue = GamemodeManager.GamemodeManager.Template.FindIndex(x => x.Equals(templates[ModeCount]));
						GamemodeManager.GamemodeManager.CurrentMode = GamemodeManager.GamemodeManager.ModeList[queue];
						GamemodeManager.GamemodeManager.CurrentName = GamemodeManager.GamemodeManager.ModeName[queue];
						GamemodeManager.GamemodeManager.CurrentQueue = GamemodeManager.GamemodeManager.SpawnQueue[queue];
						GamemodeManager.GamemodeManager.CurrentDescription = GamemodeManager.GamemodeManager.Descriptions[queue];
						plugin.Info("Changing mode to [" + templates[ModeCount] + "] [" + (GamemodeManager.GamemodeManager.CurrentMode.Equals(this.plugin) ? "Default" : GamemodeManager.GamemodeManager.CurrentMode.ToString()) + "] (" + GamemodeManager.GamemodeManager.CurrentName + ") (" + GamemodeManager.GamemodeManager.QueueToString(GamemodeManager.GamemodeManager.CurrentQueue) + ")");
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
			}
		}

		public void OnDecideTeamRespawnQueue(DecideRespawnQueueEvent ev)
		{
			if (GamemodeManager.GamemodeManager.ModeList.Count > 0 && !GamemodeManager.GamemodeManager.DisableAll)
			{
				string result = "";
				foreach(Team team in ev.Teams)
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

	class CommandHandler : ICommandHandler
	{
		private Plugin plugin;
		public CommandHandler(Plugin plugin)
		{
			this.plugin = plugin;
		}

		public string GetCommandDescription()
		{
			return "Show the usage of gamemode command";
		}

		public string GetUsage()
		{
			return "GAMEMODE HELP";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (args.Length == 1 && args[0].ToUpper().Equals("HELP"))
			{
				return new string[] { "Gamemode Command List" + "\n" + "gamemode - Show the current gamemode." + "\n" + "gamemode help - Show the usage of gamemode command." + "\n" + "gamemode list - Show the list of gamemodes." + "\n" + "gamemode setnextmode {<template name>|<plugin id> <spawn queue> <name>} - Set the gamemode of next round." + "\n" + "gamemode enable - Enable gamemodes." + "\n" + "gamemode disable - Disable all gamemodes." };
			}
			else
			{
				if (GamemodeManager.GamemodeManager.ModeList.Count > 0)
				{
					if (args.Length > 0 && args[0].ToUpper().Equals("LIST"))
					{
						string myList = "Gamemodes List" + "\n";
						for (int i = 0; i < GamemodeManager.GamemodeManager.ModeList.Count; i++)
						{
							string queue = GamemodeManager.GamemodeManager.QueueToString(GamemodeManager.GamemodeManager.SpawnQueue[i]);
							myList += "[" + (i + 1) + "]" + (GamemodeManager.GamemodeManager.ModeList[i].Equals(this.plugin) ? "Default" : GamemodeManager.GamemodeManager.ModeList[i].ToString()) + " Name:" + GamemodeManager.GamemodeManager.ModeName[i] + " Queue:" + queue + "\n";
						}
						return new string[] { myList };
					}
					else if (args.Length > 0 && args[0].ToUpper().Equals("ENABLE"))
					{
						GamemodeManager.GamemodeManager.DisableAll = false;
						return new string[] { "Gamemodes will be enabled in the following rounds." };
					}
					else if (args.Length > 0 && args[0].ToUpper().Equals("DISABLE"))
					{
						GamemodeManager.GamemodeManager.DisableAll = true;
						return new string[] { "Gamemodes will be disabled in the following rounds." };
					}
					else if (args.Length >= 2 && args[0].ToUpper().Equals("SETNEXTMODE"))
					{
						if (args[1].Contains("."))
						{
							if (args[1].ToUpper().Equals("DEFAULT"))
							{
								GamemodeManager.GamemodeManager.SetNextMode(this.plugin, plugin.GetConfigString("team_respawn_queue"), "Default");
								return new string[] { "Next mode will be Default" };
							}
							else
							{
								Plugin nextmode = PluginManager.Manager.GetEnabledPlugin(args[1]);
								if (nextmode != null && nextmode.Details.id.Contains("gamemode") && !nextmode.Equals(plugin))
								{
									string queue = "-1";
									if (args.Length >= 3)
										queue = args[2];

									string name = null;
									if (args.Length >= 4)
										name = args[3];

									GamemodeManager.GamemodeManager.SetNextMode(nextmode, queue, name);
									return new string[] { "Next mode will be " + nextmode.ToString() };
								}
								else
									return new string[] { "Can't find gamemode: " + args[1] };
							}
						}
						else
						{
							if (GamemodeManager.GamemodeManager.Template.Contains(args[1]))
							{
								int queue = GamemodeManager.GamemodeManager.Template.FindIndex(x => x.Equals(args[1]));
								GamemodeManager.GamemodeManager.SetNextMode(GamemodeManager.GamemodeManager.ModeList[queue], GamemodeManager.GamemodeManager.QueueToString(GamemodeManager.GamemodeManager.SpawnQueue[queue]), GamemodeManager.GamemodeManager.ModeName[queue]);
								return new string[] { "Next mode will be " + args[1] };
							}
							else
								return new string[] { "Can't find template: " + args[1] };
						}
					}
					else
					{
						string queue = GamemodeManager.GamemodeManager.QueueToString(GamemodeManager.GamemodeManager.CurrentQueue);
						return new string[] { (GamemodeManager.GamemodeManager.CurrentMode.Equals(this.plugin) ? "Default" : GamemodeManager.GamemodeManager.CurrentMode.ToString()) + " Name:" + GamemodeManager.GamemodeManager.CurrentName + " Queue:" + queue };
					}
				}
				else
					return new string[] { "Can't find any gamemodes." };
			}
		}
	}
}
