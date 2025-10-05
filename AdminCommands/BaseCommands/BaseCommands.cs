using PlayerManager_Shared.Abstractions;
using Sharp.Shared;
using Sharp.Shared.Definition;
using Sharp.Shared.Enums;
using Sharp.Shared.Objects;

namespace AdminCommands.BaseCommands
{
    public class CommandDefinition
    {
        public string Name { get; }
        public string ConsoleName { get; }
        public string Description { get; }
        public Action<ISharedSystem, IGamePlayer, IGameClient?> Action { get; }

        public CommandDefinition(string name, string consoleName, string description,
            Action<ISharedSystem, IGamePlayer, IGameClient?> action)
        {
            Name = name;
            ConsoleName = consoleName;
            Description = description;
            Action = action;
        }
    }

    public static class BaseCommands
    {
        public static readonly List<CommandDefinition> All = new()
        {
            new CommandDefinition(
                "noclip",
                "ms_noclip",
                "切換玩家 noclip",
                (shared, target, caller) =>
                {
                    var pawn = shared.GetEntityManager().FindPlayerPawnBySlot(target.Client.Slot);
                    if (pawn == null || !pawn.IsAlive) return;

                    bool enable = pawn.MoveType != MoveType.NoClip;
                    pawn.SetMoveType(enable ? MoveType.NoClip : MoveType.Walk);

                    if (caller != null)
                        target.SendMessage($"{ChatColor.Red}[ADMCommands]{ChatColor.White} NoClip {(enable ? "enabled" : "disabled")}");
                    else
                        Console.WriteLine($"已切換 {target.Name} 的 NoClip 為 {(enable ? "開啟" : "關閉")}");
                }),

            new CommandDefinition(
                "slay",
                "ms_slay",
                "處決玩家",
                (shared, target, caller) =>
                {
                    var pawn = shared.GetEntityManager().FindPlayerPawnBySlot(target.Client.Slot);
                    if (pawn == null || !pawn.IsAlive) return;

                    pawn.Slay();

                    if (caller != null)
                        target.SendMessage($"{ChatColor.Red}[ADMCommands]{ChatColor.White} 你被處決了");
                    else
                        Console.WriteLine($"已處決 {target.Name}");
                }),

            new CommandDefinition(
                "freeze",
                "ms_freeze",
                "凍結玩家",
                (shared, target, caller) =>
                {
                    var pawn = shared.GetEntityManager().FindPlayerPawnBySlot(target.Client.Slot);
                    if (pawn == null || !pawn.IsAlive) return;

                    pawn.SetMoveType(MoveType.None); // ✅ 直接禁止移動

                    if (caller != null)
                        target.SendMessage($"{ChatColor.Red}[ADMCommands]{ChatColor.White} 你已被凍結");
                    else
                        Console.WriteLine($"已凍結 {target.Name}");
                }),

            new CommandDefinition(
                "unfreeze",
                "ms_unfreeze",
                "解除凍結玩家",
                (shared, target, caller) =>
                {
                    var pawn = shared.GetEntityManager().FindPlayerPawnBySlot(target.Client.Slot);
                    if (pawn == null || !pawn.IsAlive) return;

                    pawn.SetMoveType(MoveType.Walk); // ✅ 恢復正常移動

                    if (caller != null)
                        target.SendMessage($"{ChatColor.Red}[ADMCommands]{ChatColor.White} 你已被解除凍結");
                    else
                        Console.WriteLine($"已解除凍結 {target.Name}");
                })
        };
    }
}
