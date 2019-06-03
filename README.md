# SCPSL Gamemode Manager
A framework that supports different gamemodes in one single server.

Put sm_config_gamemode.txt in the same folder with config_gameplay.txt to modify gamemodes.

# Config
`gm_round_sequence: {15%template1|20%template2|template3}, template4`

# Commands

## Main Commands
() Denotes optional argument.  
<> Denotes required argument.  
| Means either one or the other is needed.  
& Means both are needed.  

Command | Alias | Description
--- | :---: | ---
gamemode | gmm | When used without arguments, shows the current gamemode.

## Subcommands
These are each called on top of a main command

### GMM:
Command | Alias | Arguments | Description
--- | :---: | :---: | ---
help | -h | (command) | Returns the usage info of the command.
current | -c | | Shows the current gamemode (same as main command without arguments)
setnextmode | -s | <template name \| plugin id \| spawn queue \| name> | Set the next gamemode
enable | -e | | Enable gamemodes
disable | -d | | Disable all gamemodes
version | -v | | Print the current version to the console

# API
`void` RegisterMode(Plugin gamemode, string spawnqueue = "-1");
If spawnqueue is "-1", the gamemode will run with default config(team_respawn_queue)

`void` SetNextMode(Plugin gamemode, string spawnqueue = "-1", string name = null);

`Teams[]` GetCurrentQueue();

`Plugin` GetCurrentMode();

`string` GetCurrentName();

`List<Plugin>` GetModeList();
