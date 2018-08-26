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
		version = "1.6",
		SmodMajor = 3,
		SmodMinor = 1,
		SmodRevision = 14
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
			this.AddCommand("gamemode", new CommandHandler(this));
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
		//public static Dictionary<List<Plugin>, List<Teams[]>> ModeList = new Dictionary<List<Plugin>, List<Teams[]>>();
		public static List<Plugin> ModeList = new List<Plugin>();
		public static List<Team[]> SpawnQueue = new List<Team[]>();
		public static bool DisableAll;

		public static void RegisterMode(Plugin gamemode, string spawnqueue = "-1")
		{
			CurrentMode = gamemode;
			ModeList.Add(gamemode);
			gamemode.Info("[GamemodeManager] " + gamemode.ToString() + " has been registered.");

			if (spawnqueue.Equals("-1"))
			{
				spawnqueue = gamemode.GetConfigString("team_respawn_queue");
			}
			gamemode.pluginManager.DisablePlugin(gamemode);

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

		public static void SetCurrentMode(Plugin gamemode)
		{
			CurrentMode = gamemode;
			CurrentQueue = SpawnQueue[ModeList.FindIndex(x => x.Equals(gamemode))];
		}
			
		public static Plugin GetCurrentMode()
		{
			return CurrentMode;
		}

		public static Team[] GetCurrentQueue()
		{
			return CurrentQueue;
		}

		public static Team[] GetModeQueue(Plugin gamemode)
		{
			return SpawnQueue[ModeList.FindIndex(x => x.Equals(gamemode))];
		}

		public static List<Plugin> GetModeList()
		{
			return ModeList;
		}
	}
}

namespace Smod2.Handler
{ 
	class SmodEventHandler : IEventHandlerWaitingForPlayers, IEventHandlerRoundRestart, IEventHandlerDecideTeamRespawnQueue, IEventHandlerSetServerName
	{
		private Plugin plugin;
		public SmodEventHandler(Plugin plugin)
		{
			this.plugin = plugin;
		}

		private static int ModeCount = 0;

		private static bool FirstRound;

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			if (!FirstRound)
			{
				string path = ConfigManager.Manager.Config.GetConfigPath().Replace("config.txt", "sm_config_gamemode.txt");
				if (File.Exists(path))
				{
					int queue = -1;
					List<Plugin> modeList = new List<Plugin>();
					List<int> rounds = new List<int>();
					List<string> spawnqueue = new List<string>();
					GamemodeManager.GamemodeManager.ModeList.Clear();
					string[] config = File.ReadAllLines(path);
					foreach (string line in config)
					{
						if (line.Contains("[") && line.Contains("]"))
						{
							string pluginid = line.Replace("[", string.Empty).Replace("]", string.Empty);
							Plugin gamemode = plugin.pluginManager.GetDisabledPlugin(pluginid);
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
							rounds.Add(0);
							spawnqueue.Add("-1");
						}
						else if (queue > -1 && line.Contains("-"))
						{
							string[] keyvalue = System.Text.RegularExpressions.Regex.Split(line, ": ");
							keyvalue[0] = keyvalue[0].Replace("-", string.Empty).Trim();
							switch(keyvalue[0].ToUpper())
							{
								case "NAME":
									{
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
							}
						}
					}
					for (int i = 0; i <= queue; i++)
					{
						for (int j = 0; j < rounds[i]; j ++)
						{
							GamemodeManager.GamemodeManager.ModeList.Add(modeList[i]);

							List<Team> classTeamQueue = new List<Team>();
							for (int k = 0; k < spawnqueue[i].Length; i++)
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

				GamemodeManager.GamemodeManager.CurrentMode = GamemodeManager.GamemodeManager.ModeList[0];

				if (!GamemodeManager.GamemodeManager.CurrentMode.Equals(this.plugin))
					plugin.pluginManager.EnablePlugin(GamemodeManager.GamemodeManager.CurrentMode);

				FirstRound = true;
			}
		}

		public void OnRoundRestart(RoundRestartEvent ev)
		{
			if (GamemodeManager.GamemodeManager.DisableAll)
			{
				foreach (Plugin gamemode in GamemodeManager.GamemodeManager.ModeList)
				{
					if (!gamemode.Equals(this.plugin))
						plugin.pluginManager.DisablePlugin(gamemode);
				}
			}
			else
			{
				if (GamemodeManager.GamemodeManager.NextMode != null)
				{
					if (!GamemodeManager.GamemodeManager.CurrentMode.Equals(this.plugin))
						plugin.pluginManager.DisablePlugin(GamemodeManager.GamemodeManager.CurrentMode);
					if (!GamemodeManager.GamemodeManager.NextMode.Equals(this.plugin))
						plugin.pluginManager.EnablePlugin(GamemodeManager.GamemodeManager.NextMode);

					GamemodeManager.GamemodeManager.CurrentMode = GamemodeManager.GamemodeManager.NextMode;
					GamemodeManager.GamemodeManager.CurrentQueue = GamemodeManager.GamemodeManager.SpawnQueue[GamemodeManager.GamemodeManager.ModeList.FindIndex(x => x.Equals(GamemodeManager.GamemodeManager.CurrentMode))];
					GamemodeManager.GamemodeManager.NextMode = null;
					plugin.Info("Changing mode to [" + (GamemodeManager.GamemodeManager.CurrentMode.Equals(this.plugin) ? "default" : GamemodeManager.GamemodeManager.CurrentMode.ToString()) + "] (" + GamemodeManager.GamemodeManager.CurrentQueue + ")");
				}
				else if (GamemodeManager.GamemodeManager.ModeList.Count > 0)
				{
					if (!GamemodeManager.GamemodeManager.CurrentMode.Equals(this.plugin))
						plugin.pluginManager.DisablePlugin(GamemodeManager.GamemodeManager.CurrentMode);
					if (!GamemodeManager.GamemodeManager.ModeList[ModeCount].Equals(this.plugin))
						plugin.pluginManager.EnablePlugin(GamemodeManager.GamemodeManager.ModeList[ModeCount]);

					GamemodeManager.GamemodeManager.CurrentMode = GamemodeManager.GamemodeManager.ModeList[ModeCount];
					GamemodeManager.GamemodeManager.CurrentQueue = GamemodeManager.GamemodeManager.SpawnQueue[ModeCount++];
					plugin.Info("Changing mode to [" + (GamemodeManager.GamemodeManager.CurrentMode.Equals(this.plugin) ? "default" : GamemodeManager.GamemodeManager.CurrentMode.ToString()) + "] (" + GamemodeManager.GamemodeManager.CurrentQueue + ")");

					if (ModeCount >= GamemodeManager.GamemodeManager.ModeList.Count)
					{
						ModeCount = 0;
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
				ev.ServerName = ev.ServerName.Replace("$gamemode", GamemodeManager.GamemodeManager.CurrentMode.Equals(this.plugin) ? "default" : GamemodeManager.GamemodeManager.CurrentMode.Details.name);
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
			return "Show current gamemode [Show a list of gamemodes/Set the mode of next round]";
		}

		public string GetUsage()
		{
			return "GAMEMODE [LIST/SETNEXTMODE]";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (GamemodeManager.GamemodeManager.ModeList.Count > 0)
			{
				if (args.Length > 0 && args[0].ToUpper().Equals("LIST"))
				{
					string myList = string.Empty;
					int i = 0;
					foreach (Plugin modeplugin in GamemodeManager.GamemodeManager.ModeList)
					{
						string queue = "";
						foreach(Team team in GamemodeManager.GamemodeManager.SpawnQueue[i++])
						{
							queue = queue + (int)team;
						}
						myList += modeplugin.ToString() + " Queue:" + queue + "\n";
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
				else if (args.Length == 2 && args[0].ToUpper().Equals("SETNEXTMODE"))
				{
					if (args[1].Equals("default"))
					{
						GamemodeManager.GamemodeManager.NextMode = this.plugin;
						return new string[] { "Next mode will be default"};
					}
					else
					{
						Plugin nextmode = PluginManager.Manager.GetEnabledPlugin(args[1]);
						if (nextmode != null && nextmode.Details.id.Contains("gamemode") && !nextmode.Equals(plugin))
						{
							GamemodeManager.GamemodeManager.NextMode = nextmode;
							return new string[] { "Next mode will be " + nextmode.ToString() };
						}
						else
							return new string[] { "Can't find gamemode: " + args[1] };
					}
				}
				else
				{
					string queue = "";
					foreach (Team team in GamemodeManager.GamemodeManager.CurrentQueue)
					{
						queue = queue + (int)team;
					}
					return new string[] { GamemodeManager.GamemodeManager.CurrentMode.ToString() + " Queue:" + queue };
				}
			}
			else
				return new string[] { "Can't find any gamemodes." };
		}
	}
}
