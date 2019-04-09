using Smod2;
using Smod2.API;
using System.Collections.Generic;
using System.Linq;

namespace GamemodeManager
{
	public abstract class GamemodeManager
	{
		/// <summary>
		/// The gamemode currently in use
		/// </summary>
		public static Plugin CurrentMode;
		/// <summary>
		/// The next gamemode in the queue
		/// </summary>
		public static Plugin NextMode;
		/// <summary>
		/// The current team respawn queue
		/// </summary>
		public static Team[] CurrentQueue;
		/// <summary>
		/// The next team respawn queue
		/// </summary>
		public static Team[] NextQueue;
		/// <summary>
		/// The name of the current gamemode
		/// </summary>
		public static string CurrentName;
		/// <summary>
		/// The name of the next gamemode
		/// </summary>
		public static string NextName;
		/// <summary>
		/// The description of the current gamemode
		/// </summary>
		public static string CurrentDescription;
		/// <summary>
		/// A list of all registered gamemodes
		/// </summary>
		public static List<Plugin> ModeList = new List<Plugin>();
		/// <summary>
		/// 
		/// </summary>
		public static List<Team[]> SpawnQueue = new List<Team[]>();
		/// <summary>
		/// 
		/// </summary>
		public static List<string> Template = new List<string>();
		/// <summary>
		/// A list of the names of all registered gamemodes
		/// </summary>
		public static List<string> ModeName = new List<string>();
		/// <summary>
		/// 
		/// </summary>
		public static List<string> Descriptions = new List<string>();
		/// <summary>
		/// Disable all gamemode actions
		/// </summary>
		public static bool DisableAll;

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

			List<Team> classTeamQueue = spawnqueue.ToCharArray().Select(s => int.TryParse(s.ToString(), out int n) ? n : 4 ).Cast<Team>().ToList();
			/*List<Team> classTeamQueue = new List<Team>();					// old code
			for (int i = 0; i < spawnqueue.Length; i++)
			{
				int item = 4;
				if (!int.TryParse(spawnqueue[i].ToString(), out item))
				{
					item = 4;
				}
				classTeamQueue.Add((Team)item);
			}*/
			CurrentQueue = classTeamQueue.ToArray();
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