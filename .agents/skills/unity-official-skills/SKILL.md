---
name: unity-official-skills
description: >-
  Query, install, and run official Unity-Technologies/skills automation packs via
  npx skills. Use before writing custom Unity tooling or when a task maps to an
  official Unity skill.
---

# Official Unity Agent Skills

Prefer official `Unity-Technologies/skills` automation over reinventing custom scripts.

1. **List Loaded Skills**:
   ```bash
   npx skills list
   ```
2. **Register / Update Official Pack** (if missing):
   ```bash
   npx skills add Unity-Technologies/skills
   ```
3. **Execute a Skill**:
   ```bash
   npx skills run <skill-name>
   ```
4. **Verify**: Inspect command output and follow up with `unity-cli-recompile` and `unity-test-runner` if project assets or C# files were mutated.
