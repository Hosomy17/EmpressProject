> **Human-Only Visual Reference**: AI agents skip this file (equivalent dependency matrix lives in [assembly-boundaries.md](../architecture/assembly-boundaries.md)).

# Assembly Definitions (`.asmdef`) Dependency Graph

```mermaid
flowchart TD
    subgraph FrameworkLayer["Framework Layer"]
        FWEvents["Com.Voobox.Framework.Events"]
        FWPool["Com.Voobox.Framework.ObjectPooling"]
    end

    subgraph FoundationLayer["Foundation & Input"]
        ProjUtils["Com.Voobox.Project.Utils"]
        ProjInput["Com.Voobox.Project.InputSystem"]
    end

    subgraph EventsLayer["Events Layer"]
        BossEvts["Com.Voobox.Project.Events.BossEvents"]
        HeroEvts["Com.Voobox.Project.Events.HeroEvents"]
        SteamEvts["Com.Voobox.Project.Events.SteamworksEvents"]
    end

    subgraph MidLayer["Shared Utilities, Data, AI & Components"]
        ProjOthers["Com.Voobox.Project.Others"]
        ProjData["Com.Voobox.Project.Data"]
        ProjAI["Com.Voobox.Project.AI"]
        ProjComp["Com.Voobox.Project.Component"]
    end

    subgraph OrchestrationLayer["Entities, Systems & Scene Scripts"]
        ProjEntity["Com.Voobox.Project.Entity"]
        ProjSystems["Com.Voobox.Project.Systems"]
        ProjScripts["Com.Voobox.Project.Scripts"]
    end

    subgraph TestLayer["Tests"]
        EditTests["Com.Voobox.Project.Tests.EditMode"]
    end

    BossEvts --> FWEvents
    HeroEvts --> FWEvents
    SteamEvts --> FWEvents

    ProjOthers --> FWEvents
    ProjOthers --> FWPool
    ProjOthers --> SteamEvts

    ProjData --> ProjOthers

    ProjAI --> ProjData
    ProjAI --> ProjUtils

    ProjComp --> ProjData
    ProjComp --> ProjOthers
    ProjComp --> ProjUtils
    ProjComp --> ProjInput

    ProjEntity --> ProjAI
    ProjEntity --> ProjComp
    ProjEntity --> ProjData
    ProjEntity --> ProjOthers
    ProjEntity --> ProjUtils
    ProjEntity --> ProjInput
    ProjEntity --> FWEvents
    ProjEntity --> BossEvts
    ProjEntity --> HeroEvts

    ProjSystems --> FWEvents
    ProjSystems --> HeroEvts
    ProjSystems --> BossEvts

    ProjScripts --> FWEvents
    ProjScripts --> BossEvts
    ProjScripts --> SteamEvts

    EditTests -.->|"tests all runtime assemblies"| OrchestrationLayer
```
