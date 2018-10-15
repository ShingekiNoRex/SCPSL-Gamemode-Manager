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
		author = "ShingekiNoRex | lordofkhaos",
		name = "GamemodeManager",
		description = "",
		id = "rex.gamemode.manager",
		version = "2.2",
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
			this.AddConfig(new Config.ConfigSetting("gm_enable", true, Config.SettingType.BOOL, true, "enables GameModes for the current server"));
			this.AddConfig(new Config.ConfigSetting("gm_round_sequence", new string[] { "" }, Config.SettingType.LIST, true, ""));
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

