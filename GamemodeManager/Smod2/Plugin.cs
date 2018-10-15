using Smod2.Attributes;
using Smod2.Handler;

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