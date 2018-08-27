# SCPSL Gamemode Manager
A framework that supports different gamemodes in one single server.

Put sm_config_gamemode.txt in the same folder with config_gameplay.txt to modify gamemodes.

# Command
**gamemode** - Show the current gamemode.

**gamemode help** - Show the usage of gamemode command.

**gamemode list** - Show the list of gamemodes.

**gamemode setnextmode [plugin id] [spawn queue] [name]** - Set the mode of next round.

**gamemode enable** - Enable gamemodes.

**gamemode disable** - Disable all gamemodes.

# API
`void` RegisterMode(Plugin gamemode, string spawnqueue = "-1");
If spawnqueue is "-1", the gamemode will run with default config(team_respawn_queue)

`void` SetNextMode(Plugin gamemode, string spawnqueue = "-1", string name = null);

`Teams[]` GetCurrentQueue();

`Plugin` GetCurrentMode();

`string` GetCurrentName();

`List<Plugin>` GetModeList();
