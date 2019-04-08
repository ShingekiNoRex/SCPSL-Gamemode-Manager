using System;
using Smod2.Commands;

namespace Smod2.Handler
{
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
				return new string[]
				{
					"Gamemode Command List",
					"gamemode - Show the current gamemode.",
					"gamemode help - Show the usage of gamemode command.",
					"gamemode list - Show the list of gamemodes.",
					"gamemode setnextmode {<template name>|<plugin id> <spawn queue> <name>} - Set the gamemode of next round.",
					"gamemode enable - Enable gamemodes.",
					"gamemode disable - Disable all gamemodes."
				};
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
