using Smod2;
using Smod2.Attributes;
using Smod2.Config;
using Smod2.Lang;

namespace GamemodeManager.SmodAPI
{
	[PluginDetails(
		author = "ShingekiNoRex | lordofkhaos",
		name = "GamemodeManager",
		description = "",
		id = "rex.gamemode.manager",
		version = "2.3",
		SmodMajor = 3,
		SmodMinor = 4,
		SmodRevision = 0
	)]
	internal class PluginGamemodeManager : Plugin
	{
		public override void OnDisable()
		{
		}

		public override void OnEnable()
		{
			Info("Gamemode Manager has been enabled :)");
		}

		public override void Register()
		{
			// Register Events
			AddEventHandlers(new SmodEventHandler(this));
			AddConfig(new ConfigSetting("gm_enable", true, true, "enables GameModes for the current server")); // bool
			AddConfig(new ConfigSetting("gm_round_sequence", new [] { string.Empty }, true, string.Empty)); // list
			AddCommand("gamemode", new CommandHandler(this));
			AddCommand("gmm", new CommandHandler(this));
			AddTranslation(new LangSetting("GM_CURRENT_MODE", "Current Mode"));
			AddTranslation(new LangSetting("GM_DESCRIPTION", "Description"));
		}
	}
}