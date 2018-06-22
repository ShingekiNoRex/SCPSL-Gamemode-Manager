# SCPSL Gamemode Manager
A framework that supports different gamemodes in one single server.

#Command
*gamemode* - Show the current gamemode.
*gamemode list* - Show a list of gamemodes.
*gamemode setnextmode <plugin id>* - Set the mode of next round.

#API
void RegisterMode(Plugin gamemode, string spawnqueue = "-1");
If spawnqueue is "-1", the gamemode will run with default config(team_respawn_queue)

void SetCurrentMode(Plugin gamemode);

Teams[] GetCurrentQueue();

Plugin GetCurrentMode();

Teams[] GetModeQueue(Plugin gamemode);

List<Plugin> GetModeList();