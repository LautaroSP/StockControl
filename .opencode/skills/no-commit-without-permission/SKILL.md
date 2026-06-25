---
name: no-commit-without-permission
description: Use ALWAYS when working with git. Prevents committing any changes without explicit user permission. Triggers on any git commit, git add, or staging operation. Must be active during all coding sessions.
---

# No Commit Without Permission Skill

## Rules

1. **NEVER run `git commit` unless the user explicitly asks for it.** This includes:
   - `git commit`
   - `git commit -m`
   - `git commit -am`
   - `git add` followed by commit
   - Any other command that creates a commit

2. **NEVER run `git add` to stage files unless the user explicitly asks.** Staging is a precursor to committing and should only be done with permission.

3. **When you finish implementing changes**, simply inform the user what was done. Do NOT suggest committing. Wait for the user to say "commit" or "commit this".

4. **If the user says "commit"**, before committing:
   - Run `git status` and `git diff` to review changes
   - Show the user a summary of what will be committed
   - Write a concise commit message matching the repo style
   - Then commit

5. **If the user says "save my work" or similar**, do NOT interpret this as a commit request. Ask if they want to commit.

6. **Never auto-commit**, even if it seems like the right thing to do (e.g., after completing a task, fixing a bug, or at the end of a session).
