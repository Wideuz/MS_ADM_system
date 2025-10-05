using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Permission_Shared;
using PlayerManager_Shared.Abstractions;
using Sharp.Shared;
using Sharp.Shared.Definition;
using Sharp.Shared.Enums;
using Sharp.Shared.Listeners;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using BC = AdminCommands.BaseCommands.BaseCommands;
using static CMsgGCCStrike15_v2_Account_RequestCoPlays.Types;
using static Sharp.Shared.Definition.ChatColor;

namespace AdminCommands
{
    public sealed class AdminCommands : IModSharpModule, IClientListener
    {
        public string DisplayName => "AdminCommands";
        public string DisplayAuthor => "si";

        public int ListenerVersion => 1;
        public int ListenerPriority => 100;

        private readonly ILogger<AdminCommands> _logger;
        private readonly ISharedSystem _sharedSystem;
        private IPlayerManager? _playerManager;
        private ICPermission? _permission;

        public AdminCommands(
            ISharedSystem sharedSystem,
            string? dllPath,
            string? sharpPath,
            Version? version,
            IConfiguration? coreConfiguration,
            bool hotReload)
        {
            _sharedSystem = sharedSystem ?? throw new ArgumentNullException(nameof(sharedSystem));
            _logger = _sharedSystem.GetLoggerFactory().CreateLogger<AdminCommands>();
        }

        public bool Init()
        {
            
            return true;
        }

        public void PostInit()
        {
            
        }

        public void Shutdown()
        {
            _logger.LogInformation("AdminCommands shutting down");
        }

        // ✅ 當 PlayerManager 註冊完成後，框架會呼叫這裡

        public void OnLibraryDisconnect(string name)
        {
            if (name == "PlayerManager")
            {
                _logger.LogWarning("PlayerManager 已卸載，AdminCommands 功能停用");
                _playerManager = null;
            }
        }
        public void OnAllModulesLoaded()
        {
            var wrapper = _sharedSystem.GetSharpModuleManager()
                .GetRequiredSharpModuleInterface<IPlayerManager>(IPlayerManager.Identity);
            _playerManager = wrapper.Instance
                             ?? throw new InvalidOperationException("PlayerManager_Shared 介面不可為 null");

            var permWrapper = _sharedSystem.GetSharpModuleManager()
                .GetRequiredSharpModuleInterface<ICPermission>(ICPermission.ProjectIdentity);
            _permission = permWrapper.Instance
                          ?? throw new InvalidOperationException("Permission_Shared 介面不可為 null");

            _logger.LogInformation("AdminCommands 啟動!");
            foreach (var cmd in BC.All)
            {
                _sharedSystem.GetClientManager().InstallCommandCallback(cmd.Name,
                    (client, command) => ForClientCommand(client, command, cmd.Action));

                _sharedSystem.GetConVarManager().CreateServerCommand(
                    cmd.ConsoleName,
                    command => ForServerCommand(command, cmd.Action),
                    cmd.Description,
                    ConVarFlags.Release);
            }
        }

        private ECommandAction ForClientCommand(
            IGameClient client,
            StringCommand command,
            Action<ISharedSystem, IGamePlayer, IGameClient?> action)
        {
            if (!HasPermission(client)) return ECommandAction.Handled;

            return HandlePlayerTargets(client, command, target =>
            {
                action(_sharedSystem, target, client);
            });
        }

        private ECommandAction ForServerCommand(
            StringCommand command,
            Action<ISharedSystem, IGamePlayer, IGameClient?> action)
        {
            if (_playerManager is null)
            {
                Console.WriteLine("PlayerManager 尚未準備好");
                return ECommandAction.Handled;
            }

            return HandlePlayerTargets(null, command, target =>
            {
                action(_sharedSystem, target, null);
            });
        }

        private ECommandAction HandlePlayerTargets(
            IGameClient? client,
            StringCommand command,
            Action<IGamePlayer> action)
        {
            string selector = "@me";
            if (command.ArgCount > 0 && !string.IsNullOrWhiteSpace(command.ArgString))
                selector = command.ArgString.Trim();

            var allPlayers = _playerManager!.GetPlayers();

            var targets = selector switch
            {
                "@me" when client != null => allPlayers.Where(p => p.Client.Equals(client)).ToArray(),
                "@ct" => allPlayers.Where(p =>
                {
                    var controller = _sharedSystem.GetEntityManager().FindPlayerControllerBySlot(p.Client.Slot);
                    return controller?.Team == CStrikeTeam.CT;
                }).ToArray(),
                "@t" => allPlayers.Where(p =>
                {
                    var controller = _sharedSystem.GetEntityManager().FindPlayerControllerBySlot(p.Client.Slot);
                    return controller?.Team == CStrikeTeam.TE;
                }).ToArray(),
                _ => allPlayers.Where(p =>
                    !string.IsNullOrWhiteSpace(p.Name) &&
                    (p.Name.Equals(selector, StringComparison.OrdinalIgnoreCase) ||
                     p.Name.StartsWith(selector, StringComparison.OrdinalIgnoreCase))
                ).ToArray()
            };

            if (targets.Length == 0)
            {
                if (client != null)
                    client.SayChatMessage(false, $"{ChatColor.Red}[ADMCommands]{ChatColor.White} 找不到符合條件的玩家：{selector}");
                else
                    Console.WriteLine($"找不到符合條件的玩家：{selector}");
                return ECommandAction.Handled;
            }

            foreach (var target in targets)
            {
                action(target);
            }

            return ECommandAction.Handled;
        }
        private bool HasPermission(IGameClient client)
        {
            if (_permission is null) return false;

            var steamId = client.SteamId.ToString();
            var identity = _permission.GetIdentity(steamId);

            return identity.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                   || identity.Equals("Manager", StringComparison.OrdinalIgnoreCase)
                   || identity.Equals("Owner", StringComparison.OrdinalIgnoreCase);
        }
    }
}
    