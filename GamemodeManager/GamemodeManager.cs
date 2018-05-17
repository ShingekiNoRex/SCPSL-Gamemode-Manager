using Smod2;
using Smod2.Attributes;
using Smod2.Events;
using Smod2.API;
using Smod2.Commands;
using Smod2.Handler;
using System.Collections.Generic;

namespace Smod2.Plugins
{
    [PluginDetails(
        author = "ShingekiNoRex",
        name = "GamemodeManager",
        description = "",
        id = "rex.gamemode.manager",
        version = "1.1",
        SmodMajor = 2,
        SmodMinor = 0,
        SmodRevision = 0
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
            AddEventHandler(typeof(IEventRoundStart), new SmodEventHandler(this), Priority.Highest);
            AddEventHandler(typeof(IEventDecideTeamRespawnQueue), new SmodEventHandler(this), Priority.Highest);
            AddCommand("gamemode", new CommandHandler(this));
        }
    }
}

namespace GamemodeManager
{
    public abstract class GamemodeManager
    {
        public static Plugin CurrentMode;
        public static Teams[] CurrentQueue;
        public static List<Plugin> ModeList = new List<Plugin>();
        public static List<Teams[]> SpawnQueue = new List<Teams[]>();

        public static void RegisterMode(Plugin gamemode, string spawnqueue = "-1")
        {
            CurrentMode = gamemode;
            ModeList.Add(gamemode);
            gamemode.Info("[GamemodeManager] " + gamemode.ToString() + " has been added to list.");

            if (spawnqueue.Equals("-1"))
            {
                spawnqueue = gamemode.GetConfigString("team_respawn_queue");
            }

            List<Teams> classTeamQueue = new List<Teams>();
            for (int i = 0; i < spawnqueue.Length; i++)
            {
                int item = 4;
                if (!int.TryParse(spawnqueue[i].ToString(), out item))
                {
                    item = 4;
                }
                classTeamQueue.Add((Teams)item);
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

        public static Teams[] GetCurrentQueue()
        {
            return CurrentQueue;
        }

        public static Teams[] GetModeQueue(Plugin gamemode)
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
    class SmodEventHandler : IEventRoundStart, IEventDecideTeamRespawnQueue
    {
        private Plugin plugin;
        public SmodEventHandler(Plugin plugin)
        {
            this.plugin = plugin;
        }

        private int ModeCount = 0;
        public void OnRoundStart(Server server)
        {
            if (GamemodeManager.GamemodeManager.ModeList.Count > 0)
            {
                GamemodeManager.GamemodeManager.CurrentMode = GamemodeManager.GamemodeManager.ModeList[ModeCount];
                GamemodeManager.GamemodeManager.CurrentQueue = GamemodeManager.GamemodeManager.SpawnQueue[ModeCount++];
                if (ModeCount >= GamemodeManager.GamemodeManager.ModeList.Count)
                {
                    ModeCount = 0;
                }
            }
        }

        public void OnDecideTeamRespawnQueue(Teams[] teams, out Teams[] teamsOutput)
        {
            if (GamemodeManager.GamemodeManager.ModeList.Count > 0)
            {
                teamsOutput = GamemodeManager.GamemodeManager.CurrentQueue;
            }
            else
            {
                teamsOutput = teams;
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
            return "Show current gamemode [Show a list of gamemodes]";
        }

        public string GetUsage()
        {
            return "GAMEMODE [LIST]";
        }

        public void OnCall(ICommandManager manger, string[] args)
        {
            if (GamemodeManager.GamemodeManager.ModeList.Count > 0)
            {
                if (args.Length == 0 || args[0] == null || args == null)
                {
                    plugin.Info(GamemodeManager.GamemodeManager.CurrentMode.ToString() + "(Queue:" + GamemodeManager.GamemodeManager.CurrentQueue.ToString() + ")");
                }
                else if (args[0].ToUpper().Equals("LIST"))
                {
                    int i = 0;
                    foreach (Plugin modeplugin in GamemodeManager.GamemodeManager.ModeList)
                    {
                        plugin.Info(modeplugin.ToString() + "(Queue:" + GamemodeManager.GamemodeManager.SpawnQueue[i++].ToString() + ")");
                    }
                }
            }
            else
            {
                plugin.Info("Can't find any gamemode.");
            }
        }
    }
}