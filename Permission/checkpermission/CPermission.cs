using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Permission_Shared;

namespace Permission.checkpermission
{
    public class CPermission : ICPermission
    {
        private static readonly Lazy<Dictionary<string, string>> _jsonPlayers = new(LoadIdentities);
        private static string _defaultIdentity = "Player";

        // ✅ Thread-Safe 快取 (SteamID → 身分)
        private readonly ConcurrentDictionary<string, string> _cache = new();
        private readonly SqliteConnection? _connection;

        public CPermission(SqliteConnection? connection = null)
        {
            _connection = connection;
        }

        private static Dictionary<string, string> LoadIdentities()
        {
            var dict = new Dictionary<string, string>();
            try
            {
                string basePath = AppContext.BaseDirectory;
                string sharpRoot = Path.GetFullPath(Path.Combine(basePath, ".."));
                string dataPath = Path.Combine(sharpRoot, "data", "PlayerPermission.json");

                if (!File.Exists(dataPath))
                {
                    Console.WriteLine($"[Permission] PlayerPermission.json not found at {dataPath}");
                    return dict;
                }

                string json = File.ReadAllText(dataPath);
                var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("players", out var playersElement))
                {
                    foreach (var player in playersElement.EnumerateObject())
                    {
                        dict[player.Name] = player.Value.GetString() ?? "Player";
                    }
                }

                if (doc.RootElement.TryGetProperty("settings", out var settingsElement) &&
                    settingsElement.TryGetProperty("defaultIdentity", out var defaultIdentityElement))
                {
                    _defaultIdentity = defaultIdentityElement.GetString() ?? "Player";
                }

                Console.WriteLine($"[Permission] Loaded PlayerPermission.json with {dict.Count} entries.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Permission] Failed to load PlayerPermission.json: {ex.Message}");
            }

            return dict;
        }

        public string GetIdentity(string steamId64)
        {
            // 先查快取
            if (_cache.TryGetValue(steamId64, out var cached))
                return cached;

            // 再查 JSON
            if (_jsonPlayers.Value.TryGetValue(steamId64, out var identity))
            {
                _cache[steamId64] = identity;
                return identity;
            }

            // 沒有 → 用 default
            _cache[steamId64] = _defaultIdentity;
            return _defaultIdentity;
        }

        public async Task<string> GetIdentityFromDatabaseAsync(string steamId64)
        {
            // 先查快取
            if (_cache.TryGetValue(steamId64, out var cached))
                return cached;

            if (_connection == null)
                return GetIdentity(steamId64);

            var sql = "SELECT steamid FROM players WHERE steamid = @SteamId LIMIT 1;";
            var dbSteamId = await _connection.ExecuteScalarAsync<string?>(sql, new { SteamId = steamId64 });

            if (dbSteamId == null)
            {
                _cache[steamId64] = _defaultIdentity;
                return _defaultIdentity;
            }

            var identity = GetIdentity(dbSteamId);
            _cache[steamId64] = identity;
            return identity;
        }

        public string GetDefaultIdentity() => _defaultIdentity;
    }
}
