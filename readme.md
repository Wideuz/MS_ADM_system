game/
├── sharp/
│   ├── modules/
│   │   ├── AdminCommands/
│   │   │   ├── AdminCommands.dll
│   │   │   ├── AdminCommands.deps.json
│   │   │   └── AdminCommands.pdb
│   │   ├── PlayerManager_Shared/
│   │   │   ├── PlayerManager_Shared.dll
│   │   │   ├── PlayerManager_Shared.deps.json
│   │   │   └── PlayerManager_Shared.pdb
│   │   ├── PlayerManager_Shared.Abstractions/
│   │       ├── PlayerManager_Shared.Abstractions.dll
│   │       ├── PlayerManager_Shared.Abstractions.deps.json
│   │       └── PlayerManager_Shared.Abstractions.pdb
│   │   
│   │   
│   │   
│   │   
│   ├── shared/
│   │   │
│   │   ├── PlayerManager_Shared.Abstractions/
│   │   │   ├── PlayerManager_Shared.Abstractions.dll
│   │   │   ├── PlayerManager_Shared.Abstractions.deps.json
│   │   │   └── PlayerManager_Shared.Abstractions.pdb
│   │   └── PlayerManager_Shared.pdb
├── data/
    └── PlayerPermission.json ← 請自行建立，用來設定玩家權限

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
