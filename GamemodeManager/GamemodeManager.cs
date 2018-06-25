using Smod2;
using Smod2.Attributes;
using Smod2.API;
using Smod2.Commands;
using Smod2.Handler;
using System.Collections.Generic;
using Smod2.EventHandlers;
using Smod2.EventSystem.Events;
using Smod2.Events;

namespace Smod2.Plugins
{
    [PluginDetails(
        author = "ShingekiNoRex",
        name = "GamemodeManager",
        description = "",
        id = "rex.gamemode.manager",
        version = "1.5",
        SmodMajor = 3,
        SmodMinor = 1,
        SmodRevision = 5
    )]
    class PluginGamemodeManager : Plugin
    {
        public override void OnDisable()
        {
        }

        public override void OnEnable()
        {
            this.Info("Gamemode Manager has loaded :)");
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

        public static void RegisterMode(Plugin gamemode, string spawnqueue = "-1")
        {
            CurrentMode = gamemode;
            ModeList.Add(gamemode);
            gamemode.Info("[GamemodeManager] " + gamemode.ToString() + " has been added to list.");

            if (spawnqueue.Equals("-1"))
            {
                spawnqueue = gamemode.GetConfigString("team_respawn_queue");
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
    class SmodEventHandler : IEventHandlerRoundRestart, IEventHandlerDecideTeamRespawnQueue
    {
        private Plugin plugin;
        public SmodEventHandler(Plugin plugin)
        {
            this.plugin = plugin;
        }

        private static int ModeCount = 0;
        public void OnRoundRestart(RoundRestartEvent ev)
        {
			if (GamemodeManager.GamemodeManager.NextMode != null)
			{
				string servername = plugin.pluginManager.Server.Name;
				servername = servername.Replace(GamemodeManager.GamemodeManager.CurrentMode.Details.name, "$gamemode");
				servername = servername.Replace("$gamemode", GamemodeManager.GamemodeManager.NextMode.Details.name);
				/*
				servername = servername.Replace("Plague Curse", "瘟疫诅咒");
				servername = servername.Replace("Escort VIP", "护送VIP");
				servername = servername.Replace("Infinite Escaping", "无限逃生");
				servername = servername.Replace("Standard Mode", "标准模式");
				*/
				plugin.pluginManager.Server.Name = servername;

				plugin.Info("Change mode to [" + GamemodeManager.GamemodeManager.NextMode.ToString() + "]");
				GamemodeManager.GamemodeManager.CurrentMode = GamemodeManager.GamemodeManager.NextMode;
				GamemodeManager.GamemodeManager.CurrentQueue = GamemodeManager.GamemodeManager.SpawnQueue[GamemodeManager.GamemodeManager.ModeList.FindIndex(x => x.Equals(GamemodeManager.GamemodeManager.CurrentMode))];
				GamemodeManager.GamemodeManager.NextMode = null;
			}
			else if (GamemodeManager.GamemodeManager.ModeList.Count > 0)
			{
				string servername = plugin.pluginManager.Server.Name;
				servername = servername.Replace(GamemodeManager.GamemodeManager.CurrentMode.Details.name, "$gamemode");
				servername = servername.Replace("$gamemode", GamemodeManager.GamemodeManager.ModeList[ModeCount].Details.name);
				/*
				servername = servername.Replace("Plague Curse", "瘟疫诅咒");
				servername = servername.Replace("Escort VIP", "护送VIP");
				servername = servername.Replace("Infinite Escaping", "无限逃生");
				servername = servername.Replace("Standard Mode", "标准模式");
				*/
				plugin.pluginManager.Server.Name = servername;

				plugin.Info("Change mode to [" + GamemodeManager.GamemodeManager.ModeList[ModeCount].ToString() + "]");
				GamemodeManager.GamemodeManager.CurrentMode = GamemodeManager.GamemodeManager.ModeList[ModeCount];
				GamemodeManager.GamemodeManager.CurrentQueue = GamemodeManager.GamemodeManager.SpawnQueue[ModeCount++];
				if (ModeCount >= GamemodeManager.GamemodeManager.ModeList.Count)
				{
					ModeCount = 0;
				}
			}
        }

        public void OnDecideTeamRespawnQueue(DecideRespawnQueueEvent ev)
        {
            if (GamemodeManager.GamemodeManager.ModeList.Count > 0)
            {
				string result = "";
				foreach(Team team in ev.Teams)
				{
					result = result + (int)team;
				}
                ev.Teams = GamemodeManager.GamemodeManager.CurrentQueue;
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
                if (args.Length == 0 || args[0] == null || args == null)
                {
                    string queue = "";
                    foreach (Team team in GamemodeManager.GamemodeManager.CurrentQueue)
                    {
                        queue = queue + (int)team;
                    }
					plugin.Info(GamemodeManager.GamemodeManager.CurrentMode.ToString() + " Queue:" + queue);
                }
                else if (args[0].ToUpper().Equals("LIST"))
                {
                    int i = 0;
                    foreach (Plugin modeplugin in GamemodeManager.GamemodeManager.ModeList)
                    {
                        string queue = "";
                        foreach(Team team in GamemodeManager.GamemodeManager.SpawnQueue[i++])
                        {
                            queue = queue + (int)team;
                        }
						plugin.Info(modeplugin.ToString() + " Queue:" + queue);
                    }
                }
                else if (args[0].ToUpper().Equals("SETNEXTMODE") && args.Length == 2)
                {
					Plugin nextmode = PluginManager.Manager.GetEnabledPlugin(args[1]);
					if (nextmode != null && nextmode.Details.id.Contains("gamemode") && !nextmode.Equals(plugin))
					{
						GamemodeManager.GamemodeManager.NextMode = nextmode;
						plugin.Info("Next mode will be " + nextmode.ToString());
					}
					else
					{
						plugin.Info("Can't find gamemode: " + args[1]);
					}
                }
            }
            else
            {
				plugin.Info("Can't find any gamemode.");
            }

			return new string[] { "" };
        }
    }
}