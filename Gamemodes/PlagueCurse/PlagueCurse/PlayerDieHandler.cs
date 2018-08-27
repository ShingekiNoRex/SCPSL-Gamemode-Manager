using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.EventSystem.Events;

namespace Smod.TestPlugin
{
    class RoundStartHandler : IEventHandlerSetSCPConfig, IEventHandlerTeamRespawn, IEventHandlerInitialAssignTeam, IEventHandlerSetRole, IEventHandlerSetRoleMaxHP
    {
        private Plugin plugin;

        public RoundStartHandler(Plugin plugin)
        {
            this.plugin = plugin;
        }

		public void OnSetRoleMaxHP(SetRoleMaxHPEvent ev)
		{
			if (GamemodeManager.GamemodeManager.GetCurrentMode().Equals(plugin))
			{
				if (ev.Role == Role.SCP_049)
				{
					ev.MaxHP = 3000;
				}
			}
		}

        public void OnSetSCPConfig(SetSCPConfigEvent ev)
        {
            if (GamemodeManager.GamemodeManager.GetCurrentMode().Equals(plugin))
            {
				ev.Ban049 = false;
				ev.Ban079 = true;
				ev.Ban096 = true;
				ev.Ban106 = true;
				ev.Ban173 = true;
				ev.Ban939_53 = true;
				ev.Ban939_89 = true;
            }
        }

		public void OnSetRole(PlayerSetRoleEvent ev)
		{
			if (GamemodeManager.GamemodeManager.GetCurrentMode().Equals(plugin))
			{
				ev.Player.SetAmmo(AmmoType.DROPPED_5, 1000);
				ev.Player.SetAmmo(AmmoType.DROPPED_7, 1000);
				ev.Player.SetAmmo(AmmoType.DROPPED_9, 1000);
			}
		}

        public void OnTeamRespawn(TeamRespawnEvent ev)
        {
            if (GamemodeManager.GamemodeManager.GetCurrentMode().Equals(plugin))
            {
				ev.SpawnChaos = false;
            }
        }

		public void OnAssignTeam(PlayerInitialAssignTeamEvent ev)
		{
			if (GamemodeManager.GamemodeManager.GetCurrentMode().Equals(plugin))
			{
				if (ev.Team == Team.CHAOS_INSURGENCY)
				{
					ev.Team = Team.NINETAILFOX;
				}
			}
		}
    }

    class PlayerDieHandler : IEventHandlerPlayerDie
    {
        private Plugin plugin;

        public PlayerDieHandler(Plugin plugin)
        {
            this.plugin = plugin;
        }

        public void OnPlayerDie(PlayerDeathEvent ev)
        {
            if (GamemodeManager.GamemodeManager.GetCurrentMode().Equals(plugin) && !plugin.pluginManager.Server.Map.WarheadDetonated)
            {
                if (ev.Player.TeamRole.Role == Role.SCP_049)
                {
                    if (ev.Killer != null && !ev.Killer.Name.Equals(ev.Player.Name))
                    {
                        ev.Killer.ChangeRole(Role.SCP_049);
                    }
                    else
                    {
                        foreach (Player newplayer in plugin.pluginManager.Server.GetPlayers())
                        {
                            if (newplayer.Equals(ev.Player))
                                continue;

                            if (newplayer.TeamRole.Role != Role.SCP_049_2)
                                continue;

                            newplayer.ChangeRole(Role.SCP_049);
                            break;
                        }
                    }
					plugin.pluginManager.Server.Map.Shake();
                }
            }
        }
    }

    class PlayerHurtHandler : IEventHandlerPlayerHurt
    {
        private Plugin plugin;

        public PlayerHurtHandler(Plugin plugin)
        {
            this.plugin = plugin;
        }

        public void OnPlayerHurt(PlayerHurtEvent ev)
        {
            if (GamemodeManager.GamemodeManager.GetCurrentMode().Equals(plugin) && !plugin.pluginManager.Server.Map.WarheadDetonated)
            {
                if (ev.Attacker != null && ev.Attacker.TeamRole.Role == Role.SCP_049_2)
                {
                    ev.Damage = 0.0f;
					ev.Player.ChangeRole(Role.SCP_049_2);
                }
            }
        }
    }
}