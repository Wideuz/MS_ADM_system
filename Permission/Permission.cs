using Microsoft.Extensions.Logging;
using Permission.DatabaseMain;
using Permission.checkpermission;
using Sharp.Shared;
using static Sharp.Shared.Definition.ChatColor;
using Sharp.Shared.Enums;
using Sharp.Shared.Listeners;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using System;
using System.Security;
using Permission_Shared;


namespace Permission
{
    public sealed class Permission : IModSharpModule, IEventListener
    {

        public int ListenerPriority => 0;
        public int ListenerVersion => 1;
        public string DisplayName => "Permission";
        public string DisplayAuthor => "Widez";

        private readonly ILogger<Permission> _logger;
        private readonly ISharedSystem _sharedSystem;
        private readonly DatabaseMainHandler _database;
        private readonly ICPermission _cpermission; // ✅ 新增 JSON 身分管理


        public Permission(ISharedSystem sharedSystem,
            string? dllPath = null,
            string? sharpPath = null,
            Version? version = null,
            Microsoft.Extensions.Configuration.IConfiguration? coreConfiguration = null,
            bool hotReload = false)
        {
            _sharedSystem = sharedSystem ?? throw new ArgumentNullException(nameof(sharedSystem));
            _logger = _sharedSystem.GetLoggerFactory().CreateLogger<Permission>();

            // 初始化資料庫（不需要 moduleDirectory）
            _database = new DatabaseMainHandler(_logger);

            _cpermission = new CPermission(); // 初始化 JSON 身分管理
        }

        public bool Init()
        {
            _database.DatabaseOnLoad().GetAwaiter().GetResult();
            _logger.LogInformation("Permission initializing");
            return true;
        }

        public void PostInit()
        {
            _logger.LogInformation("Permission post-initialized");

            var eventMgr = _sharedSystem.GetEventManager();
            eventMgr.InstallEventListener(this);
            eventMgr.HookEvent("player_connect");

            
            // ✅ 改成註冊 .who 指令
            _sharedSystem.GetClientManager().InstallCommandCallback("who", OnWhoCommand);
            _logger.LogInformation("指令 .who 已註冊成功");

            _sharedSystem.GetSharpModuleManager()
                .RegisterSharpModuleInterface<ICPermission>(
                    this,
                    ICPermission.ProjectIdentity,   // identity，其他模組用這個名稱來取得
                    _cpermission
                );


        }

        public void Shutdown()
        {
            _database.DatabaseOnUnload();
            _logger.LogInformation("Permission shutting down");
        }

        // --- IEventListener 實作 ---
        public void FireGameEvent(IGameEvent ev)
        {
            if (ev.Name == "player_connect")
            {
                string name = ev.GetString("name");
                ulong xuid = ev.GetUInt64("xuid"); // SteamID64
                string steamId64 = xuid.ToString();
                bool isBot = ev.GetBool("bot");

                if (!isBot)
                {
                    var modSharp = _sharedSystem.GetModSharp();
                    string identity = _cpermission.GetIdentity(steamId64);
                    string message = $" {DarkBlue}玩家連線: {Lime}{name} {DarkBlue}({steamId64} 身分: {Lime}{identity})";
                    modSharp.PrintToChatAll(message);
                    _ = _database.InsertOrUpdatePlayerAsync(name, steamId64);
                }
            }
        }
        private ECommandAction OnWhoCommand(IGameClient client, StringCommand command)
        {
            var modSharp = _sharedSystem.GetModSharp();

            var steamId = client.SteamId.ToString();
            string identity = _cpermission.GetIdentity(steamId);

            string message = $" {DarkBlue}你的身分是: {Lime}{identity}";

            client.ConsolePrint(message);

            var filter = new RecipientFilter(client.Slot);
            modSharp.PrintChannelFilter(HudPrintChannel.Chat, message, filter);

            return ECommandAction.Stopped;
        }
    }
}
