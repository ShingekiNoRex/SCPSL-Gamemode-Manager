﻿using System;
using System.Collections.Generic;
using Smod2.Commands;

namespace Smod2.Handler
{
	internal class CommandHandler : ICommandHandler
	{
		private readonly Plugin _plugin;

		public CommandHandler(Plugin plugin)
		{
			this._plugin = plugin;
		}

		public string GetCommandDescription()
		{
			return "Show the usage of gamemode command";
		}

		public string GetUsage()
		{
			return "GAMEMODE HELP";
		}


		public string[] HelpMessage() =>
			new[]
			{
				"Gamemode Command List",
				"gamemode - Show the current gamemode.",
				"gamemode help - Show the usage of gamemode command.",
				"gamemode list - Show the list of gamemodes.",
				"gamemode setnextmode {<template name>|<plugin id> <spawn queue> <name>} - Set the gamemode of next round.",
				"gamemode enable - Enable gamemodes.",
				"gamemode disable - Disable all gamemodes."
			};

		public string[] HelpMessage(string cmd)
		{
			switch (cmd.ToUpper())
			{
				case "LIST":
					return new[]
					{
						"LIST",
						"Lists all of the gamemodes available. No arguments."
					};
				case "ENABLE":
					return new[]
					{
						"ENABLE",
						"Enables gamemodes for the current session",
						"N.B.: Overriden by config \"gm_enable\" at round start."
					};
				case "DISABLE":
					return new[]
					{
						"DISABLE",
						"Disables gamemodes for the current session",
						"N.B.: Overriden by config \"gm_enable\" at round start."
					};
				case "SETNEXTMODE":
					return new[]
					{
						"SETNEXTMODE (TEMPLATE NAME | PLUGIN ID) <QUEUE> <NAME>",
						"Set the gamemode of the next round.",
						"() Denotes a required parameter."
					};
			}

			return new string[] { };
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (args.Length <= 0 || args == null) return HelpMessage();

			switch (args[0].ToUpper())
			{
				case "HELP":
				{
					if (args.Length == 1) return HelpMessage();
					return HelpMessage();

				}
				case "LIST":
				{
					List<string> gamemodeList = new List<string> {"Gamemodes List"};
					for (int i = 0; i < GamemodeManager.GamemodeManager.ModeList.Count; i++)
					{
						string queue =
							GamemodeManager.GamemodeManager.QueueToString(GamemodeManager.GamemodeManager
								.SpawnQueue[i]);
						gamemodeList.Add("[" + (i + 1) + "]" +
											(GamemodeManager.GamemodeManager.ModeList[i]
												.Equals(this._plugin)
												? "Default"
												: GamemodeManager.GamemodeManager.ModeList[i].ToString()
											)
											+ " Name:" + GamemodeManager.GamemodeManager.ModeName[i] + " Queue:" + queue);
					}

					return gamemodeList.ToArray();
				}
				case "ENABLE":
				{
					GamemodeManager.GamemodeManager.DisableAll = false;
					return new[]
					{
						"Gamemodes will be enabled in the following rounds.",
						"NOTE: This will be overriden by the config option on each round start"
					};
				}
				case "DISABLE":
				{
					GamemodeManager.GamemodeManager.DisableAll = true;
					return new[]
					{
						"Gamemodes will be disabled in the following rounds.",
						"NOTE: This will be overriden by the config option on each round start"
					};
				}
				case "SETNEXTMODE":
				{
					if (args.Length < 2)
						return HelpMessage("setnextmode");

					if (args[1].ToUpper().Equals("DEFAULT"))
					{
						GamemodeManager.GamemodeManager.SetNextMode(this._plugin,
							_plugin.GetConfigString("team_respawn_queue"), "Default");
						return new string[] {"Next mode will be Default"};
					}

					if (!args[1].Contains(".")) return HelpMessage("setnextmode");

					Plugin nextMode = PluginManager.Manager.GetEnabledPlugin(args[1]);
					if (nextMode == null || !nextMode.Details.id.Contains("gamemode") ||
					    nextMode.Equals(_plugin))
						return new string[] {"Can't find gamemode: " + args[1]};
					string queue = "-1";
					if (args.Length >= 3)
						queue = args[2];
					string name = null;
					if (args.Length >= 4)
						name = args[3];
					GamemodeManager.GamemodeManager.SetNextMode(nextMode, queue, name);
					return new string[] {"Next mode will be " + nextMode.ToString()};

				}
				default:
				{
					string queue =
						GamemodeManager.GamemodeManager.QueueToString(GamemodeManager.GamemodeManager.CurrentQueue);
					return new[]
					{
						(GamemodeManager.GamemodeManager.CurrentMode.Equals(this._plugin)
							? "Default"
							: GamemodeManager.GamemodeManager.CurrentMode.ToString()) +
						" Name:" +
						GamemodeManager.GamemodeManager.CurrentName +
						" Queue:" +
						queue
					};
				}
			}
		}
	}
}
