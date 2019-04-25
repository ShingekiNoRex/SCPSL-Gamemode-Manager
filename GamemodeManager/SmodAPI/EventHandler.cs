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
	using System.Linq;

	using global::GamemodeManager.Templates;

	internal class SmodEventHandler : IEventHandlerWaitingForPlayers, IEventHandlerRoundRestart, IEventHandlerDecideTeamRespawnQueue, IEventHandlerSetServerName, IEventHandlerPlayerJoin
	{
		private static bool FirstRoundComplete;

		private static int ModeCount;

		private readonly PluginGamemodeManager _plugin;

		public SmodEventHandler(PluginGamemodeManager plugin)
		{
			this._plugin = plugin;
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
				Environment.NewLine + this._plugin.GetTranslation("GM_CURRENT_MODE") + ": " + GamemodeManager.CurrentName
				+ Environment.NewLine + this._plugin.GetTranslation("GM_DESCRIPTION") + ": " + Environment.NewLine
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
					GamemodeManager.CurrentDescription = GamemodeManager.CurrentMode.Equals(this._plugin)
															 ? "Standard Mode"
															 : GamemodeManager.CurrentMode.Details.description;
					GamemodeManager.NextMode = null;
					GamemodeManager.NextName = null;
					GamemodeManager.NextQueue = null;
					this._plugin.Info(
						"Changing mode to ["
						+ (GamemodeManager.CurrentMode.Equals(this._plugin)
							   ? "Default"
							   : GamemodeManager.CurrentMode.ToString()) + "] (" + GamemodeManager.CurrentName + ") ("
						+ GamemodeManager.QueueToString(GamemodeManager.CurrentQueue) + ")");
				}
				else if (GamemodeManager.ModeList.Count > 0)
				{
					List<string> templates = new List<string>(
						ConfigManager.Manager.Config.GetListValue("gm_round_sequence", true));

					// debug
					for (int i = 0; i < templates.Count; i++)
						this._plugin.Info($"DEBUG: templates[{i}]={templates[i]}");

					this._plugin.Info($"DEBUG: ModeCount-1={ModeCount - 1}");
					
					if (templates.Count > 0 && !string.IsNullOrEmpty(templates[ModeCount - 1])
											&& GamemodeManager.Templates.Contains(templates[ModeCount - 1]))
					{
						
						this._plugin.Info("DEBUG: entered [true]");
						
						int queue = GamemodeManager.Templates.FindIndex(x => x.Equals(templates[ModeCount - 1]));
						GamemodeManager.CurrentMode = GamemodeManager.ModeList[queue];
						GamemodeManager.CurrentName = GamemodeManager.ModeName[queue];
						GamemodeManager.CurrentQueue = GamemodeManager.SpawnQueue[queue];
						GamemodeManager.CurrentDescription = GamemodeManager.Descriptions[queue];
						this._plugin.Info(
							"Changing mode to [" + templates[ModeCount - 1] + "] ["
							+ (GamemodeManager.CurrentMode.Equals(this._plugin)
								   ? "Default"
								   : GamemodeManager.CurrentMode.ToString()) + "] (" + GamemodeManager.CurrentName
							+ ") (" + GamemodeManager.QueueToString(GamemodeManager.CurrentQueue) + ")");
						ModeCount++;
						if (ModeCount >= templates.Count) ModeCount = 0;
					}
					else
					{
						this._plugin.Info("DEBUG: entered [false]");
						GamemodeManager.CurrentMode = GamemodeManager.ModeList[ModeCount];
						GamemodeManager.CurrentName = GamemodeManager.ModeName[ModeCount];
						GamemodeManager.CurrentQueue = GamemodeManager.SpawnQueue[ModeCount];
						GamemodeManager.CurrentDescription = GamemodeManager.Descriptions[ModeCount++];
						this._plugin.Info(
							"Changing mode to ["
							+ (GamemodeManager.CurrentMode.Equals(this._plugin)
								   ? "Default"
								   : GamemodeManager.CurrentMode.ToString()) + "] (" + GamemodeManager.CurrentName
							+ ") (" + GamemodeManager.QueueToString(GamemodeManager.CurrentQueue) + ")");

						if (ModeCount >= GamemodeManager.ModeList.Count || ModeCount >= templates.Count) ModeCount = 0;
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
			if (!this._plugin.GetConfigBool("gm_enable") && GamemodeManager.EnabledRounds == 0)
				GamemodeManager.DisableAll = true;
			if (FirstRoundComplete) return;

			string path = ConfigManager.Manager.Config.GetConfigPath()
				.Replace("config_gameplay.txt", "sm_config_gamemode.txt");
			if (File.Exists(path))
			{
				// new template system
				TemplateHandler th = new TemplateHandler(this._plugin);
				Templates ts = th.GetTemplates(path);

				GamemodeManager.ModeList.Clear();
				GamemodeManager.Templates.Clear();
				GamemodeManager.ModeName.Clear();
				GamemodeManager.SpawnQueue.Clear();
				GamemodeManager.Descriptions.Clear();

				foreach (KeyValuePair<string, Template> t in ts)
				{
					Plugin gamemode = this._plugin.PluginManager.GetEnabledPlugin(t.Key);
					if (t.Key.ToUpper().Equals("DEFAULT"))
					{
						this._plugin.Info("DEBUG: adding plugin " + gamemode.Details.name +
						                  " to GamemodeManager.GamemodeManager.ModeList" + Environment.NewLine);
						gamemode = this._plugin;
					}
					else if (gamemode == null)
					{
						gamemode = this._plugin;
						this._plugin.Warn($"Can't find gamemode: {t.Key}");
					}

					GamemodeManager.ModeList.Add(gamemode);
					GamemodeManager.ModeName.Add(gamemode.Equals(this._plugin) ? "Default" : gamemode.Details.name);
					GamemodeManager.SpawnQueue.Add(t.Value.SpawnQueue.ToCharArray().Select(s => int.TryParse(s.ToString(), out int n) ? n : 4).Cast<Team>().ToArray());
					GamemodeManager.Descriptions.Add(t.Value.Description);
				}

				// debug
				this._plugin.Info("DEBUG: config read successfully");

				// end debug
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