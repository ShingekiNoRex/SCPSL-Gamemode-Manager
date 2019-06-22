using Smod2;
using Smod2.API;
using System.Collections.Generic;
using System.Linq;

namespace GamemodeManager
{
	public static class GamemodeManager
	{
		#region Properties
		/// <summary>
		/// The gamemode currently in use
		/// </summary>
		public static Plugin CurrentMode { get; set; }
		/// <summary>
		/// The next gamemode in the queue
		/// </summary>
		public static Plugin NextMode { get; set; }
		/// <summary>
		/// The current team respawn queue
		/// </summary>
		public static Team[] CurrentQueue { get; set; }
		/// <summary>
		/// The next team respawn queue
		/// </summary>
		public static Team[] NextQueue { get; set; }
		/// <summary>
		/// The name of the current gamemode
		/// </summary>
		public static string CurrentName { get; set; }
		/// <summary>
		/// The name of the next gamemode
		/// </summary>
		public static string NextName { get; set; }
		/// <summary>
		/// The description of the current gamemode
		/// </summary>
		public static string CurrentDescription { get; set; }
		/// <summary>
		/// Disable all gamemode actions
		/// </summary>
		public static bool DisableAll { get; set; }
		#endregion

		#region Fields
		/// <summary>
		/// A list of all registered gamemodes
		/// </summary>
		internal static List<Plugin> ModeList = new List<Plugin>();
		/// <summary>
		/// 
		/// </summary>
		internal static List<Team[]> SpawnQueue = new List<Team[]>();
		/// <summary>
		/// 
		/// </summary>
		internal static List<string> Templates = new List<string>();
		/// <summary>
		/// A list of the names of all registered gamemodes
		/// </summary>
		internal static List<string> ModeName = new List<string>();
		/// <summary>
		/// 
		/// </summary>
		internal static List<string> Descriptions = new List<string>();

		/// <summary>
		/// The amount of rounds gamemodes will be enabled for
		/// </summary>
		internal static uint EnabledRounds = 0;
		#endregion

		/// <summary>
		/// Register a gamemode
		/// </summary>
		/// <param name="gamemode">The gamemode to be registered</param>
		/// <param name="spawnqueue">The spawn queue to be associated with the registered mode</param>
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

			CurrentQueue = spawnqueue.ToCharArray().Select(s => int.TryParse(s.ToString(), out int n) ? n : 4).Cast<Team>().ToArray();
			SpawnQueue.Add(CurrentQueue);
		}

		/// <summary>
		/// Method to set the next gamemode
		/// </summary>
		/// <param name="gamemode">The plugin to be used as the next gamemode</param>
		/// <param name="spawnqueue">The spawnqueue to be associated with the gamemode</param>
		/// <param name="name">The name of the gamemode</param>
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
				NextQueue = spawnqueue.ToCharArray().Select(s => int.TryParse(s.ToString(), out int n) ? n : 4).Cast<Team>().ToArray();
			}
		}

		public static Plugin[] GetModes()
		{
			Plugin[] modes = new Plugin[ModeList.Count];
			for (int i = 0; i < ModeList.Count; i++)
				modes[i] = ModeList[i];
			return modes;
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
				result += (int)team;
			}

			return result;
		}
	}
}