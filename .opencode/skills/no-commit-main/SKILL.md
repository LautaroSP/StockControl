---
name: no-commit-main
description: Use ALWAYS when working with git branches. Prevents committing directly to the main branch even if the user explicitly requests it. Forces creation of a feature branch instead.
---

# No Commit on Main Skill

## Rules

1. **NEVER commit directly to `main` or `master` branch**, even if the user explicitly asks you to.

2. **Before any commit**, always check the current branch:
   - Run `git branch --show-current` to verify the active branch
   - If the branch is `main` or `master`, **stop and inform the user**

3. **When on main/master and a commit is requested**:
   - Tell the user: "You are on main/master. I cannot commit directly to this branch."
   - Suggest creating a feature branch: `git checkout -b feature/<descriptive-name>`
   - Wait for user confirmation before proceeding

4. **Branch naming convention**:
   - `feature/<description>` — for new features
   - `fix/<description>` — for bug fixes
   - `refactor/<description>` — for refactoring
   - `chore/<description>` — for maintenance tasks

5. **This rule is absolute** — it cannot be overridden by the user. If the user insists, explain that direct commits to main/master are not allowed and suggest the feature branch workflow.

6. **After creating a feature branch**, proceed with the commit as normal (following the no-commit-without-permission skill).
