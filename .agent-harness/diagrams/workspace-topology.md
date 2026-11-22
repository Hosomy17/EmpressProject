> **Human-Only Visual Reference**: AI agents skip this file (equivalent tables live in [project-topology.md](../architecture/project-topology.md)).

# Workspace Isolation & CLI Topology

## Workspace Isolation Map

```mermaid
flowchart LR
    subgraph HarnessDomain[".agent-harness/ and .agents/ (Agent Domain)"]
        direction TB
        A0["AGENTS.md (Always-On Router)"]
        A1["architecture/ (Domain Shards)"]
        A2["rules/ (Role-Scoped Rules)"]
        A3["workflows/ (Phase Procedures)"]
        A4["tasks-board.md and tasks/"]
        A5["scripts/ (invoke-unity.ps1, bootstrap)"]
        A6[".agents/skills/ (On-Demand Runbooks)"]
    end

    subgraph UnityCore["Unity Project Core (Engine Domain)"]
        direction TB
        U1["Assets/_Project/ (Scripts, Scenes, Prefabs, Data)"]
        U2["Assets/Editor/AgentAutomation/"]
        U3["Assets/Tests/EditMode/"]
        U4["Packages/ and ProjectSettings/"]
        U5["Logs/ (CLI Logs and Test XML)"]
    end

    HarnessDomain -.->|"Orchestrates via Unity CLI and Skills"| UnityCore
```

## Agent-to-AssetDatabase Execution Flow

```mermaid
flowchart TD
    Agent["AI Agent"] -->|"Loads relevant shards"| Harness["Agent Harness (.agent-harness/)"]
    Agent -->|"Activates on demand"| Skills[".agents/skills/ and npx skills"]
    Agent -->|"Invokes invoke-unity.ps1"| UnityCLI["Unity 6000.3.10f1 (-batchmode -nographics)"]
    Skills -->|"Dispatches CLI / Batch"| UnityCLI
    UnityCLI -->|"Recompiles & Imports"| AssetDB["Unity AssetDatabase (GUID & .meta)"]
    AssetDB -->|"Syncs"| SourceAssets["Assets/_Project/"]
    UnityCLI -->|"Writes"| LogsFolder["Logs/ (*.log and *-Results.xml)"]
```

## Unity Executable Resolution Order

```mermaid
flowchart LR
    A["Check UNITY_EDITOR_PATH env var"] -->|"Found"| D["Use Resolved Unity.exe"]
    A -->|"Not Found"| B["Check Exact 6000.3.10f1 Hub Path"]
    B -->|"Found"| D
    B -->|"Not Found"| C["Scan Hub Editor Directories"]
    C -->|"Found"| D
    C -->|"Not Found"| E["Exit 1: Unity Installation Unresolved"]
```
