using Smod2;
using Smod2.Attributes;
using Smod2.Events;
using Smod2.API;
using Smod.SCP035;
using System;
using System.Collections.Generic;
using Smod2.EventHandlers;
using Smod2.EventSystem.Events;
using Smod2.Lang;

namespace scp035
{
	[PluginDetails(
		author = "ShingekiNoRex",
		name = "scp035",
		description = "",
		id = "rex.gamemode.scp035",
		version = "2.1",
		SmodMajor = 3,
		SmodMinor = 1,
		SmodRevision = 15
	)]
	class SCP035 : Plugin
	{
		public override void OnDisable()
		{
		}

		public override void OnEnable()
		{
			this.Info("Gamemode SCP-035 has loaded :)");
		}

		public override void Register()
		{
			this.AddEventHandlers(new SmodEventHandler(this), Priority.Normal);
			GamemodeManager.GamemodeManager.RegisterMode(this, "43444404444344434444");

			Dictionary<string, string> translations = new Dictionary<string, string>
			{
				{ "MODE035_YOU_ARE", "You are" },
				{ "MODE035_DESCRIPTION", "Description" },
				{ "MODE035_GOAL", "Goal" },

				{ "MODE035_SCP_035", "SCP-035" },
				{ "MODE035_SCP_035_DESC", "Looks like a D-Boy. Spawning with a pistol, a medkit, a zone manager keycard and a flashgrenade." },
				{ "MODE035_SCP_035_GOAL", "Kill everyone else." },

				{ "MODE035_SCP_049", "Doctor" },
				{ "MODE035_SCP_049_DESC", "You cant attack, but is able to revive the dead regardless of time. You cant revive those who commit suicide." },
				{ "MODE035_SCP_049_GOAL", "Help find out and kill SCP-035 with your zombies." },

				{ "MODE035_SCP_106", "Teleporter" },
				{ "MODE035_SCP_106_DESC", "You cant attack directly, but is able to teleport people to the portal you create." },
				{ "MODE035_SCP_106_GOAL", "Help find out and kill SCP-035." },

				{ "MODE035_SCIENTIST", "Detective" },
				{ "MODE035_SCIENTIST_DESC", "You can use disarmer to find out who SCP-035 is." },
				{ "MODE035_SCIENTIST_GOAL", "Kill SCP-035." },

				{ "MODE035_HUNTER", "Hunter" },
				{ "MODE035_HUNTER_DESC", "You have a gun to kill others." },
				{ "MODE035_HUNTER_GOAL", "Kill SCP-035." },

				{ "MODE035_CLASSD", "Civilian" },
				{ "MODE035_CLASSD_DESC", "You are just a civilian with nothing." },
				{ "MODE035_CLASSD_GOAL", "Kill SCP-035." }
			};

			foreach (var translation in translations)
			{
				this.AddTranslation(new LangSetting(translation.Key, translation.Value, "gamemode_scp035"));
			}
		}
	}
}

namespace Smod.SCP035
{
	class SmodEventHandler : IEventHandlerPlayerDie, IEventHandlerRoundStart, IEventHandlerSetRole, IEventHandlerPlayerHurt, IEventHandlerCheckRoundEnd, IEventHandlerSpawnRagdoll, IEventHandlerInfected, IEventHandlerSetSCPConfig, IEventHandlerPocketDimensionEnter
	{ 
		private static List<int> scp_list = new List<int>();
		private Plugin plugin;
		//private static bool roundend;
		//private static bool roundstart;
		private static bool allowkill;
		public SmodEventHandler(Plugin plugin)
		{
			this.plugin = plugin;
		}

		public void OnSetSCPConfig(SetSCPConfigEvent ev)
		{
			ev.Ban049 = false;
			ev.Ban096 = true;
			ev.Ban106 = false;
			ev.Ban173 = true;
			ev.Ban939_53 = true;
			ev.Ban939_89 = true;
		}

		public void OnRoundStart(RoundStartEvent ev)
		{
			if (GamemodeManager.GamemodeManager.CurrentMode == plugin)
			{
				List<Player> PlayerList = new List<Player>();
				List<int> hunter_list = new List<int>();
				scp_list = new List<int>();

				foreach (Player player in ev.Server.GetPlayers())
				{
					if (player.TeamRole.Team == Team.CLASSD)
					{
						PlayerList.Add(player);
					}
				}

				Random rm = new Random();
				Player scp035 = PlayerList[rm.Next(PlayerList.Count)];
				scp_list.Add(scp035.PlayerId);

				scp035.GiveItem(ItemType.COM15);
				scp035.GiveItem(ItemType.MEDKIT);
				scp035.GiveItem(ItemType.ZONE_MANAGER_KEYCARD);
				scp035.GiveItem(ItemType.FLASHBANG);

				PlayerList.Remove(scp035);

				if (PlayerList.Count > 4)
				{
					Player randomplayer = PlayerList[rm.Next(PlayerList.Count)];
					randomplayer.GiveItem(ItemType.MP4);
					PlayerList.Remove(randomplayer);
					hunter_list.Add(randomplayer.PlayerId);

					if (PlayerList.Count > 8)
					{
						randomplayer = PlayerList[rm.Next(PlayerList.Count)];
						randomplayer.GiveItem(ItemType.MP4);
						PlayerList.Remove(randomplayer);
						hunter_list.Add(randomplayer.PlayerId);
					}

					if (PlayerList.Count > 10)
					{
						randomplayer = PlayerList[rm.Next(PlayerList.Count)];
						randomplayer.GiveItem(ItemType.COM15);
						randomplayer.GiveItem(ItemType.MEDKIT);
						randomplayer.GiveItem(ItemType.ZONE_MANAGER_KEYCARD);
						randomplayer.GiveItem(ItemType.FLASHBANG);
						scp_list.Add(randomplayer.PlayerId);
					}
				}

				foreach (Item item in ev.Server.Map.GetItems(ItemType.FRAG_GRENADE, false))
				{
					item.Remove();
				}

				foreach (Player player in ev.Server.GetPlayers())
				{
					if (scp_list.Contains(player.PlayerId))
					{
						player.SendConsoleMessage(Environment.NewLine + plugin.GetTranslation("MODE035_YOU_ARE") + ": " + plugin.GetTranslation("MODE035_SCP_035") + Environment.NewLine + plugin.GetTranslation("MODE035_DESCRIPTION") + ": " + plugin.GetTranslation("MODE035_SCP_035_DESC") + Environment.NewLine + plugin.GetTranslation("MODE035_GOAL") + ": " + plugin.GetTranslation("MODE035_SCP_035_GOAL"), "red");
					}
					else if (hunter_list.Contains(player.PlayerId))
					{
						player.SendConsoleMessage(Environment.NewLine + plugin.GetTranslation("MODE035_YOU_ARE") + ": " + plugin.GetTranslation("MODE035_HUNTER") + Environment.NewLine + plugin.GetTranslation("MODE035_DESCRIPTION") + ": " + plugin.GetTranslation("MODE035_HUNTER_DESC") + Environment.NewLine + plugin.GetTranslation("MODE035_GOAL") + ": " + plugin.GetTranslation("MODE035_HUNTER_GOAL"), "red");
					}
					else if (player.TeamRole.Role == Role.SCIENTIST)
					{
						player.SendConsoleMessage(Environment.NewLine + plugin.GetTranslation("MODE035_YOU_ARE") + ": " + plugin.GetTranslation("MODE035_SCIENTIST") + Environment.NewLine + plugin.GetTranslation("MODE035_DESCRIPTION") + ": " + plugin.GetTranslation("MODE035_SCIENTIST_DESC") + Environment.NewLine + plugin.GetTranslation("MODE035_GOAL") + ": " + plugin.GetTranslation("MODE035_SCIENTIST_GOAL"), "red");
					}
					else if (player.TeamRole.Role == Role.SCP_049)
					{
						player.SendConsoleMessage(Environment.NewLine + plugin.GetTranslation("MODE035_YOU_ARE") + ": " + plugin.GetTranslation("MODE035_SCP_049") + Environment.NewLine + plugin.GetTranslation("MODE035_DESCRIPTION") + ": " + plugin.GetTranslation("MODE035_SCP_049_DESC") + Environment.NewLine + plugin.GetTranslation("MODE035_GOAL") + ": " + plugin.GetTranslation("MODE035_SCP_049_GOAL"), "red");
					}
					else if (player.TeamRole.Role == Role.SCP_106)
					{
						player.SendConsoleMessage(Environment.NewLine + plugin.GetTranslation("MODE035_YOU_ARE") + ": " + plugin.GetTranslation("MODE035_SCP_106") + Environment.NewLine + plugin.GetTranslation("MODE035_DESCRIPTION") + ": " + plugin.GetTranslation("MODE035_SCP_106_DESC") + Environment.NewLine + plugin.GetTranslation("MODE035_GOAL") + ": " + plugin.GetTranslation("MODE035_SCP_106_GOAL"), "red");
					}
					else if (player.TeamRole.Role == Role.CLASSD)
					{
						player.SendConsoleMessage(Environment.NewLine + plugin.GetTranslation("MODE035_YOU_ARE") + ": " + plugin.GetTranslation("MODE035_CLASSD") + Environment.NewLine + plugin.GetTranslation("MODE035_DESCRIPTION") + ": " + plugin.GetTranslation("MODE035_CLASSD_DESC") + Environment.NewLine + plugin.GetTranslation("MODE035_GOAL") + ": " + plugin.GetTranslation("MODE035_CLASSD_GOAL"), "red");
					}
				}

				//roundend = false;
				//roundstart = true;
				allowkill = false;
				System.Timers.Timer t = new System.Timers.Timer();
				t.Interval = 60000;
				t.AutoReset = false;
				t.Enabled = true;
				t.Elapsed += delegate
				{
					allowkill = true;
					t.Enabled = false;
				};
			}
		}

		public void OnPlayerDie(PlayerDeathEvent ev)
		{
			if (GamemodeManager.GamemodeManager.CurrentMode == plugin)
			{
				if (ev.Player.PlayerId != ev.Killer.PlayerId && !scp_list.Contains(ev.Player.PlayerId) && !scp_list.Contains(ev.Killer.PlayerId))
				{
					ev.Killer.Kill();
				}

				if (scp_list.Contains(ev.Player.PlayerId))
				{
					scp_list.Remove(ev.Player.PlayerId);
				}
			}
			/*
			if (ev.Player.PlayerId != ev.Killer.PlayerId)
			{
				if (ev.Player.PlayerId == scp_id)
				{
					if (!roundend)
					{
						plugin.pluginManager.Server.Round.EndRound();
						roundend = true;
						roundstart = false;
					}
				}
				else if (ev.Killer.PlayerId != scp_id && ev.Killer.TeamRole.Team != Team.SCP && ev.Player.TeamRole.Team != Team.SCP)
				{
					ev.Killer.Kill();
					CheckEndCondition();
				}
				else
				{
					CheckEndCondition();
				}
			}
			else
			{
				CheckEndCondition();
			}*/
		}

		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			if (GamemodeManager.GamemodeManager.CurrentMode == plugin)
			{
				if (ev.DamageType != DamageType.FALLDOWN && ev.DamageType != DamageType.SCP_106 && (!scp_list.Contains(ev.Player.PlayerId) || ev.Player.TeamRole.Team == Team.CLASSD))
				{
					if (allowkill)
					{
						ev.Damage = 20000.0F;
					}
					else
					{
						ev.Damage = 0.0F;
					}
				}
				if (scp_list.Contains(ev.Player.PlayerId) && scp_list.Contains(ev.Attacker.PlayerId) && ev.Player.PlayerId != ev.Attacker.PlayerId)
				{
					ev.Damage = 0.0F;
				}
				if ((ev.Attacker.TeamRole.Role == Role.SCP_049 && ev.Player.TeamRole.Role != Role.SCP_049) || (ev.Attacker.TeamRole.Role == Role.SCP_106 && ev.Player.TeamRole.Role != Role.SCP_106))
				{
					ev.Damage = 0.0F;
				}
			}
		}

		public void OnSetRole(PlayerSetRoleEvent ev)
		{
			if (GamemodeManager.GamemodeManager.CurrentMode == plugin)
			{
				foreach (Item item in ev.Player.GetInventory())
				{
					if (item.Equals(ItemType.FRAG_GRENADE))
					{
						item.Remove();
					}
				}

				if (ev.Role == Role.SCIENTIST)
				{
					ev.Player.GiveItem(ItemType.E11_STANDARD_RIFLE);
					ev.Player.GiveItem(ItemType.DISARMER);
					ev.Player.GiveItem(ItemType.RADIO);
				}
				else if (ev.Role == Role.NTF_SCIENTIST)
				{
					ev.Player.GiveItem(ItemType.FLASHBANG);
					ev.Player.GiveItem(ItemType.DISARMER);
				}
			}
			//CheckEndCondition();
		}

		public void OnCheckRoundEnd(CheckRoundEndEvent ev)
		{
			if (GamemodeManager.GamemodeManager.CurrentMode == plugin)
			{
				bool scpalive = false;
				bool humanalive = false;

				foreach (Player player in ev.Server.GetPlayers())
				{
					if (scp_list.Contains(player.PlayerId))
					{
						scpalive = true;
					}
					else if (player.TeamRole.Team != Team.SPECTATOR)
					{
						humanalive = true;
					}
				}

				if (scpalive && !humanalive)
				{
					ev.Status = ROUND_END_STATUS.SCP_VICTORY;
				}
				else if (!scpalive && humanalive)
				{
					ev.Status = ROUND_END_STATUS.MTF_VICTORY;
				}
				else
				{
					ev.Status = ROUND_END_STATUS.ON_GOING;
				}
			}
		}

		public void OnSpawnRagdoll(PlayerSpawnRagdollEvent ev)
		{
			if (GamemodeManager.GamemodeManager.CurrentMode == plugin)
			{
				ev.DamageType = DamageType.SCP_049;
				ev.AllowRecall = true;
				ev.Player.Infect(180f);
			}
		}

		public void OnPlayerInfected(PlayerInfectedEvent ev)
		{
			if (GamemodeManager.GamemodeManager.CurrentMode == plugin)
			{
				ev.InfectTime = 180f;
			}
		}

		public void OnPocketDimensionEnter(PlayerPocketDimensionEnterEvent ev)
		{
			Vector portal = Vector.Zero;
			foreach (Player player in plugin.pluginManager.Server.GetPlayers())
			{
				if (player.TeamRole.Role == Role.SCP_106)
				{
					portal = player.Get106Portal();
				}
			}
			bool portalExist = Math.Abs(portal.x * portal.y * portal.z) < 1f;
			portal = new Vector(portal.x, portal.y + 2f, portal.z);
			ev.TargetPosition = portalExist ? ev.LastPosition : portal;
		}

		/*
		public void OnDisconnect(DisconnectEvent ev)
		{
			CheckEndCondition();
		}
		
		public void OnPlayerJoin(PlayerJoinEvent ev)
		{
			if (roundstart && plugin.pluginManager.Server.NumPlayers <= 2)
			{
				plugin.pluginManager.Server.Round.EndRound();
			}
		}

		public void CheckEndCondition()
		{
			System.Timers.Timer t = new System.Timers.Timer();
			t.Interval = 5000;
			t.AutoReset = false;
			t.Enabled = true;
			t.Elapsed += delegate
			{
				t.Enabled = false;
				if (roundstart)
				{
					bool scpalive = false;
					bool humanalive = false;

					foreach (Player player in plugin.pluginManager.Server.GetPlayers())
					{
						if (player.TeamRole.Team != Team.SPECTATOR)
						{
							if (player.PlayerId == scp_id && !scpalive)
							{
								scpalive = true;
							}
							if (player.PlayerId != scp_id && !humanalive && player.TeamRole.Team != Team.SCP)
							{
								humanalive = true;
							}
						}
					}

					if (!scpalive || (scpalive && !humanalive))
					{
						if (!roundend)
						{
							plugin.pluginManager.Server.Round.EndRound();
							roundend = true;
							roundstart = false;
						}
					}
				}
			};
		}*/
	}
}