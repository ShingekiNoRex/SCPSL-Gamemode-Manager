using Smod.TestPlugin;
using Smod2;
using Smod2.Attributes;
using Smod2.EventHandlers;
using Smod2.Events;

namespace SetNickname
{
    [PluginDetails(
        author = "ShingekiNoRex",
        name = "Plague Curse",
        description = "Curse of the Plague(Based on Gamemode Manager)",
        id = "rex.gamemode.plaguecurse",
        version = "1.2",
        SmodMajor = 3,
        SmodMinor = 1,
        SmodRevision = 4
        )]
    class GiveItem : Plugin
    {
        public override void OnDisable()
        {
        }

        public override void OnEnable()
        {
            this.Info("PlagueCurse has loaded :)");
        }

        public override void Register()
        {
            // Register Events
            this.AddEventHandlers(new RoundStartHandler(this), Priority.Highest);
			this.AddEventHandlers(new PlayerDieHandler(this), Priority.Highest);
			this.AddEventHandlers(new PlayerHurtHandler(this), Priority.Highest);
			GamemodeManager.GamemodeManager.RegisterMode(this, "01111111111111111111");
        }
    }
}
