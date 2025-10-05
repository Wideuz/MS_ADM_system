The struct for files

game/
└── sharp/
    ├── modules/
    │   ├── AdminCommands/
    │   │   ├── AdminCommands.dll
    │   │   ├── AdminCommands.deps.json
    │   │   └── AdminCommands.pdb   ← 可選
    │   │
    │   ├── Permission/
    │   │   ├── Dapper.dll
    │   │   ├── e_sqlite3.dll
    │   │   ├── Microsoft.Data.Sqlite.dll
    │   │   ├── Permission.design.cs
    │   │   ├── Permission.dll
    │   │   ├── Permission.pdb
    │   │   ├── SQLitePCLRaw.batteries_v2.dll
    │   │   ├── SQLitePCLRaw.core.dll
    │   │   └── SQLitePCLRaw.provider.e_sqlite3.dll
    │   │
    │   └── PlayerManager_Shared/
    │       ├── PlayerManager_Shared.dll
    │       ├── PlayerManager_Shared.deps.json
    │       ├── PlayerManager_Shared.runtimeconfig.json
    │       └── PlayerManager_Shared.pdb
    │
    ├── shared/
    │   ├── PlayerManager_Shared/
    │   │   ├── PlayerManager_Shared.dll
    │   │   └── PlayerManager_Shared.pdb
    │   │
    │   └── PlayerManager_Shared.Abstractions/
    │       ├── PlayerManager_Shared.Abstractions.deps.json
    │       ├── PlayerManager_Shared.Abstractions.dll
    │       └── PlayerManager_Shared.Abstractions.pdb
    │
    └── data/
        └── PlayerPermission.json(Create to put your permission that you want)

    PlayerPermission.json (Example)
    {
        "players": {
        "76561198212232364": "Admin",
        "76561198000000000": "Player",
        "76561198011111111": "Manager",
        "76561198022222222": "Owner"
    },
    "settings": {
        "defaultIdentity": "Player"
    }
}
        I am newbie  don't ask too many hard questions :3
