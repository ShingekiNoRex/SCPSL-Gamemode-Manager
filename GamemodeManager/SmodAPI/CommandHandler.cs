using System;
using System.Collections.Generic;
using Smod2.Commands;
using Smod2;

namespace GamemodeManager.SmodAPI
{

	internal class CommandHandler : ICommandHandler
	{
		private readonly PluginGamemodeManager _plugin;

		public CommandHandler(PluginGamemodeManager plugin)
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


		public static string[] HelpMessage() =>
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

		public static string[] HelpMessage(string cmd)
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
			string input = args.Length == 0 ? " " : args[0];

			switch (input.ToUpper())
			{
				case "HELP":
					{
						return args.Length == 1 ? HelpMessage(args[0]) : HelpMessage();
					}
				case "LIST":
				{
					List<string> gamemodeList = new List<string> {"Gamemodes List"};
					for (int i = 0; i < GamemodeManager.ModeList.Count; i++)
					{
						string queue =
							GamemodeManager.QueueToString(GamemodeManager
								.SpawnQueue[i]);
						gamemodeList.Add("[" + (i + 1) + "]" +
											(GamemodeManager.ModeList[i]
												.Equals(this._plugin)
												? "Default"
												: GamemodeManager.ModeList[i].ToString()
											)
											+ " Name:" + GamemodeManager.ModeName[i] + " Queue:" + queue);
					}

					return gamemodeList.ToArray();
				}
				case "ENABLE":
				{
					GamemodeManager.DisableAll = false;
					if (args.Length < 2)
						return new[]
					{
						"Gamemodes will be enabled in the following rounds.",
						"NOTE: This will be overriden by the config option on each round start"
					};
					else
					{
						if (int.TryParse(args[1], out int enabledRounds))
						{
							GamemodeManager.EnabledRounds = (uint)enabledRounds;
							return new[] { $"Gamemodes will be enabled for the following rounds {enabledRounds}" };
						}

						else
						{
							GamemodeManager.EnabledRounds = 1;
							return new[] { $"{args[1]} is not an integer", "Gamemodes will be enabled in this round" };
						}
					}
				}
				case "DISABLE":
				{
					GamemodeManager.DisableAll = true;
					GamemodeManager.EnabledRounds = 0;
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
						GamemodeManager.SetNextMode(this._plugin,
							_plugin.GetConfigString("team_respawn_queue"), "Default");
						return new string[] {"Next mode will be Default"};
					}

					if (!args[1].Contains(".")) return HelpMessage("setnextmode");

					Plugin nextMode = PluginManager.Manager.GetEnabledPlugin(args[1]);
					if (nextMode == null)
					    return new[] {"Can't find gamemode: " + args[1], "REASON: " + nameof(nextMode) + " is null"};
					if (!nextMode.Details.id.Contains("gamemode"))
					    return new[] {"Can't find gamemode: " + args[1], "REASON: " + nameof(nextMode) + " does not contain 'gamemode'"};
					if (nextMode.Equals(_plugin))
						return new string[] {"Can't find gamemode: " + args[1], "REASON: " + nameof(nextMode) + " equals DEFAULT"};
					string queue = "-1";
					if (args.Length >= 3)
						queue = args[2];
					string name = null;
					if (args.Length >= 4)
						name = args[3];
					GamemodeManager.SetNextMode(nextMode, queue, name);
					return new string[] {"Next mode will be " + nextMode.ToString()};

				}

				default:
				{
			return new[]
					{
						(GamemodeManager.CurrentMode.Equals(this._plugin) ? "Default" : GamemodeManager.CurrentMode.ToString()) 
						+ " Name:"
						+ GamemodeManager.CurrentName 
						+ " Queue:"
						+ (GamemodeManager.CurrentMode.Equals(this._plugin)
							? ConfigManager.Manager.Config.GetStringValue("team_respawn_queue", "40143140314414041340")
							: GamemodeManager.QueueToString(GamemodeManager.CurrentQueue))
					};
				}
			}
		}
	}
}
