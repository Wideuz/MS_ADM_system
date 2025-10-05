using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Permission.DatabaseMain
{
    public sealed class DatabaseMainHandler
    {
        private SqliteConnection? _connection;
        private readonly string _dbPath;
        private readonly ILogger _logger;

        public DatabaseMainHandler(ILogger logger)
        {
            SQLitePCL.Batteries_V2.Init();

            var baseDir = AppContext.BaseDirectory; // e.g. ...\game\sharp\core\
            var sharpRoot = Path.GetFullPath(Path.Combine(baseDir, "..")); // 回到 ...\game\sharp\
            var dbFolder = Path.Combine(sharpRoot, "modules", "Permission", "Database");

            // ✅ 只在 Database 資料夾不存在時才建立
            if (!Directory.Exists(dbFolder))
            {
                Directory.CreateDirectory(dbFolder);
            }

            _dbPath = Path.Combine(dbFolder, "Permission_players.db");

            _logger = logger;
        }

        /// <summary>
        /// 初始化資料庫，建立資料表
        /// </summary>
        public async Task DatabaseOnLoad()
        {
            _connection = new SqliteConnection($"Data Source={_dbPath}");
            _connection.Open();

            _logger.LogInformation("[DatabaseOnLoad] Database ready at {Path}", _dbPath);

            var sql = @"
                CREATE TABLE IF NOT EXISTS players (
                    steamid TEXT PRIMARY KEY,
                    username TEXT NOT NULL,
                    connected_at TEXT NOT NULL
                );
            ";

            await _connection.ExecuteAsync(sql);
        }

        /// <summary>
        /// 關閉資料庫連線
        /// </summary>
        public void DatabaseOnUnload()
        {
            _connection?.Close();
            _logger.LogInformation("[DatabaseOnUnload] Database connection closed.");
        }

        /// <summary>
        /// 插入或更新玩家資料
        /// </summary>
        public async Task InsertOrUpdatePlayerAsync(string username, string steamId)
        {
            if (_connection == null)
            {
                _logger.LogError("[InsertOrUpdatePlayerAsync] SqlConnection is null!");
                return;
            }

            var sql = @"
                INSERT INTO players (steamid, username, connected_at)
                VALUES (@SteamId, @Username, @ConnectedAt)
                ON CONFLICT(steamid) DO UPDATE SET
                    username = excluded.username,
                    connected_at = excluded.connected_at;
            ";

            await _connection.ExecuteAsync(sql, new
            {
                SteamId = steamId,
                Username = username,
                ConnectedAt = DateTime.UtcNow.ToString("o")
            });

            _logger.LogInformation("[InsertOrUpdatePlayerAsync] Saved player {Name} ({SteamId})", username, steamId);
        }

        /// <summary>
        /// 查詢玩家
        /// </summary>
        public async Task<string?> GetPlayerNameAsync(string steamId)
        {
            if (_connection == null)
            {
                _logger.LogError("[GetPlayerNameAsync] SqlConnection is null!");
                return null;
            }

            var sql = @"SELECT username FROM players WHERE steamid = @SteamId;";
            return await _connection.ExecuteScalarAsync<string?>(sql, new { SteamId = steamId });
        }
    }
}
