using System;
using Exiled.API.Features;
using MySqlConnector;

namespace xp_plugin
{
    public class xp_plugin : Plugin<Config>
    {
        public override string Name => "xp_plugin";
        public override string Author => "Naleśnior";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredExiledVersion => new Version(9, 6, 1);

        public static xp_plugin Instance { get; private set; }
        private EventHandlers handlers;

        public override void OnEnabled()
        {
            base.OnEnabled();
            Instance = this;
            CreateDatabase();

            handlers = new EventHandlers(this);
            Exiled.Events.Handlers.Player.Verified += handlers.OnPlayerVerified;
            Exiled.Events.Handlers.Player.Dying += handlers.OnDying;
            Exiled.Events.Handlers.Player.Escaping += handlers.OnEscaping;
            Exiled.Events.Handlers.Player.Handcuffing += handlers.OnHandcuffing;
            Exiled.Events.Handlers.Player.UsedItem += handlers.OnUsedItem;
            Exiled.Events.Handlers.Player.ActivatingWarheadPanel += handlers.OnActivatingWarheadPanel;
            Exiled.Events.Handlers.Warhead.Starting += handlers.OnWarheadStarting;
            Exiled.Events.Handlers.Warhead.Detonated += handlers.OnWarheadDetonated;

            Log.Info("XP Plugin has been enabled.");
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.Verified -= handlers.OnPlayerVerified;
            Exiled.Events.Handlers.Player.Dying -= handlers.OnDying;
            Exiled.Events.Handlers.Player.Escaping -= handlers.OnEscaping;
            Exiled.Events.Handlers.Player.Handcuffing -= handlers.OnHandcuffing;
            Exiled.Events.Handlers.Player.UsedItem -= handlers.OnUsedItem;
            Exiled.Events.Handlers.Player.ActivatingWarheadPanel -= handlers.OnActivatingWarheadPanel;
            Exiled.Events.Handlers.Warhead.Starting -= handlers.OnWarheadStarting;
            Exiled.Events.Handlers.Warhead.Detonated -= handlers.OnWarheadDetonated;

            handlers = null;
            Instance = null;
            base.OnDisabled();
            Log.Info("XP Plugin has been disabled.");
        }

        private void CreateDatabase()
        {
            try
            {
                using (var connection = GetDatabaseConnection())
                {
                    connection.Open();
                    string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS `xp_data` (
                            `UserId` VARCHAR(255) NOT NULL PRIMARY KEY,
                            `XP` DOUBLE NOT NULL DEFAULT 0,
                            `Level` INT NOT NULL DEFAULT 1,
                            `Nickname` VARCHAR(64) NOT NULL DEFAULT ''
                        );";

                    using (var command = new MySqlCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                Log.Error($"Error creating database: {ex.Message}");
            }
        }

        public MySqlConnection GetDatabaseConnection()
        {
            return new MySqlConnection(Config.ConnectionString);
        }

        public decimal CalculateLevel(decimal xp)
        {
            decimal level = 1;
            while (xp >= GetXpForNextLevel(level))
            {
                level++;
            }
            return level;
        }

        public decimal GetXpForNextLevel(decimal level)
        {
            return (decimal)Math.Pow((double)level, 2);
        }

        public async void AddXp(Player player, decimal amount, string message, Player target = null)
        {
            if (player == null || amount <= 0) return;

            decimal finalXp = amount;
            decimal multiplier = 1.0m;

            if (Config.PremiumRoleMultipliers != null && Config.PremiumRoleMultipliers.TryGetValue(player.GroupName, out decimal roleMultiplier))
            {
                multiplier = roleMultiplier;
            }
            finalXp *= multiplier;

            decimal currentXp = 0;
            decimal currentLevel = 1;
            decimal newLevel = 1;

            try
            {
                using (var connection = GetDatabaseConnection())
                {
                    await connection.OpenAsync();

                    string selectQuery = "SELECT `XP`, `Level` FROM `xp_data` WHERE `UserId` = @userId;";
                    using (var selectCmd = new MySqlCommand(selectQuery, connection))
                    {
                        selectCmd.Parameters.AddWithValue("@userId", player.UserId);
                        using (var reader = await selectCmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                currentXp = Convert.ToDecimal(reader["XP"]);
                                currentLevel = Convert.ToDecimal(reader["Level"]);
                            }
                        }
                    }

                    decimal newXp = currentXp + finalXp;
                    newLevel = currentLevel;
                    while (newXp >= GetXpForNextLevel(newLevel))
                    {
                        newXp -= GetXpForNextLevel(newLevel);
                        newLevel++;
                    }

                    decimal xpToNextLevel = GetXpForNextLevel(newLevel);

                    string formattedMessage = message
                        .Replace("{xp}", finalXp.ToString("0.##"))
                        .Replace("{player}", target?.Nickname ?? "someone")
                        .Replace("{CurrnetXP}", newXp.ToString("0.##"))
                        .Replace("{RequestXPToNextLevel}", xpToNextLevel.ToString("0.##"));

                    player.ShowHint(formattedMessage, 5f);

                    string updateQuery = "INSERT INTO `xp_data` (`UserId`, `XP`, `Level`, `Nickname`) VALUES (@userId, @newXp, @newLevel, @nickname) ON DUPLICATE KEY UPDATE `XP` = @newXp, `Level` = @newLevel, `Nickname` = @nickname;";
                    using (var updateCmd = new MySqlCommand(updateQuery, connection))
                    {
                        updateCmd.Parameters.AddWithValue("@userId", player.UserId);
                        updateCmd.Parameters.AddWithValue("@newXp", newXp);
                        updateCmd.Parameters.AddWithValue("@newLevel", (int)newLevel);
                        updateCmd.Parameters.AddWithValue("@nickname", player.Nickname);
                        await updateCmd.ExecuteNonQueryAsync();
                    }
                }

                if (newLevel > currentLevel)
                {
                    await System.Threading.Tasks.Task.Delay(500);
                    string levelUpMessage = Config.On_Level_Up.Replace("{level}", ((int)newLevel).ToString());
                    player.ShowHint(levelUpMessage, 10f);
                }
            }
            catch (MySqlException ex)
            {
                Log.Error($"Database error while adding XP: {ex.Message}");
            }
        }
    }
}