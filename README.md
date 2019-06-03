# SCPSL Gamemode Manager
A framework that supports different gamemodes in one single server.

Put sm_config_gamemode.txt in the same folder with config_gameplay.txt to modify gamemodes.

# Config
`gm_round_sequence: {15%template1|20%template2|template3}, template4`

# Command
**gamemode** | **gmm** - Show the current gamemode.

## Subcommands
**current** | **-c** - Show the current gamemode.

**help** | **-h** - Show the usage of gamemode command.

**list** | **-l** - Show the list of gamemodes.

**setnextmode { [template name] | [plugin id] [spawn queue] [name] }** | **-s { [template name] | [plugin id] [spawn queue] [name] }**- Set the mode of next round.

**enable** | **-e** - Enable gamemodes.

**disable* | **-d** - Disable all gamemodes.

**version** | **-v** - Print the current version to the console.

# API
`void` RegisterMode(Plugin gamemode, string spawnqueue = "-1");
If spawnqueue is "-1", the gamemode will run with default config(team_respawn_queue)

`void` SetNextMode(Plugin gamemode, string spawnqueue = "-1", string name = null);

`Teams[]` GetCurrentQueue();

`Plugin` GetCurrentMode();

`string` GetCurrentName();

`List<Plugin>` GetModeList();
