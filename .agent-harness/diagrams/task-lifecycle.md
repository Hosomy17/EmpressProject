> **Human-Only Visual Reference**: AI agents skip this file (compact workflows live in [plan-task.md](../workflows/plan-task.md), [execute-task.md](../workflows/execute-task.md), and [review-and-closeout.md](../workflows/review-and-closeout.md)).

# Multi-Agent Task Lifecycle & State Machine

## Hand-off Sequence

```mermaid
sequenceDiagram
    autonumber
    actor User as User (Approval Gates)
    actor Planner as Planning Agent
    actor Executor as Execution Sub-agent
    actor TechLead as Technical Lead

    Note over Planner: 1. Planning Phase
    Planner->>Planner: Validate design against relevant architecture shards
    Planner->>TasksDir: Clone task.template.md into .agent-harness/tasks/TASK-XXX.md
    Planner->>Board: Register task in tasks-board.md as TODO
    Planner->>User: Gate 1 — Pause & request consent to start execution

    User-->>Planner: Consent granted (Start Execution)

    Note over Executor: 2. Execution Phase
    Executor->>Board: Claim task as DOING
    Executor->>Executor: Implement code/assets (no manual .meta or YAML edits)
    Executor->>Executor: Log SoT Conformance Attestation
    Executor->>Executor: Run unity-cli-recompile & unity-test-runner
    Executor->>Board: Update task as REVIEW

    Note over TechLead: 3. Review & Closeout Phase
    TechLead->>TechLead: Audit diff, attestation & Logs/*-Results.xml
    TechLead->>User: Gate 2 — Pause & request review/consent for DONE
    alt User Approves
        User-->>TechLead: Consent granted (Mark DONE)
        TechLead->>TasksDir: Delete .agent-harness/tasks/TASK-XXX.md
        TechLead->>Board: Remove row from tasks-board.md
        TechLead->>TechLead: Update project-topology.md if architecture shifted
    else Revisions Requested
        TechLead->>Board: Revert task to DOING with revision notes
    end
```

## Task State Transitions

```mermaid
stateDiagram-v2
    [*] --> TODO: Planner generates TASK-XXX.md
    TODO --> DOING: Gate 1 consent granted & Sub-agent claims task
    DOING --> REVIEW: Implementation complete & Unity CLI tests green
    REVIEW --> DOING: Tech Lead or User requests revisions
    REVIEW --> DONE: Gate 2 consent granted
    state DONE {
        [*] --> PurgeSpec: Delete .agent-harness/tasks/TASK-XXX.md
        PurgeSpec --> ClearBoard: Remove row from tasks-board.md
        ClearBoard --> [*]
    }
    DONE --> [*]: Lifecycle Complete
```
