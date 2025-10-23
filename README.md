# XP-Plugin
XP system plugin for scp:sl for Exiled. 

## Configuration
```cs
public bool IsEnabled { get; set; } = true;
public bool Debug { get; set; } = false;
public string ConnectionString { get; set; } = "Server=ip_of_your_database;Port=your_database_port;Database=name_of_your_database;Uid=database_user;Pwd=database_password";
public Dictionary<string, decimal> PremiumRoleMultipliers { get; set; } = new Dictionary<string, decimal>
{
    { "example_role_in_sl", 1.1m },
    { "example_role_in_sl", 1.2m },
    { "example_role_in_sl", 1.225m },
    { "example_role_in_sl", 1.275m }
};
public string On_Level_Up { get; set; } = "Congratulations! You reached {level}";
public decimal on_kill_xp { get; set; } = 1m;
public string on_kill_message { get; set; } = "You killed {player} and got {xp} XP!\n {CurrnetXP}/{RequestXPToNextLevel}";
public decimal on_escape_xp { get; set; } = 5m;
public string on_escape_message { get; set; } = "You escaped and got {xp} XP!\n {CurrnetXP}/{RequestXPToNextLevel}";
public decimal on_scp_kill_xp { get; set; } = 15m;
public string on_scp_kill_message { get; set; } = "You killed an SCP ({player}) and got {xp} XP!\n {CurrnetXP}/{RequestXPToNextLevel}";
public decimal on_player_escort_xp { get; set; } = 20m;
public string on_player_escort_message { get; set; } = "You escorted a player and got {xp} XP!\n {CurrnetXP}/{RequestXPToNextLevel}";
public decimal on_scp_item_use_xp { get; set; } = 2m;
public string on_scp_item_use_message { get; set; } = "You used an SCP item and got {xp} XP!\n {CurrnetXP}/{RequestXPToNextLevel}";
public decimal on_warhead_panel_unlock_xp { get; set; } = 4m;
public string on_warhead_panel_unlock_message { get; set; } = "You unlocked the warhead panel and got {xp} XP!\n {CurrnetXP}/{RequestXPToNextLevel}";
public decimal on_warhead_detonation_xp { get; set; } = 10m;
public string on_warhead_detonation_message { get; set; } = "You detonated the warhead and got {xp} XP!\n {CurrnetXP}/{RequestXPToNextLevel}";
```

