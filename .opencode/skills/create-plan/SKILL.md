---
name: create-plan
description: Use ONLY when the user asks to create a development plan, task plan, implementation plan, or project roadmap. Creates the plan as a markdown file in .opencode/plans/ directory. Do not use for general file creation or documentation outside of planning.
---

# Create Plan Skill

When the user requests a development plan, implementation plan, or any kind of project roadmap:

## Rules

1. **Always create plans in `.opencode/plans/`** — never in the project root or other directories.
2. **File naming** — Use a descriptive kebab-case name: `.opencode/plans/<descriptive-name>.md`
3. **Format** — Use the following structure:

```markdown
# Plan Title

**Date:** DD/MM/YYYY
**Status:** Pending / In Progress / Completed

---

## Summary

Brief overview of what needs to be done.

---

## Task 1 — Task Name

**Priority:** HIGH / MEDIUM / LOW
**Estimate:** X hours

### Problem / Need
What is the issue or requirement.

### Files to modify
- `path/to/file.cs`

### Changes
1. Step-by-step description of changes.

---

## Execution Order

| # | Task | Est. Time |
|---|------|-----------|
| 1 | Task name | X min/hours |
```

4. **If `.opencode/plans/` does not exist**, create it first.
5. **Do not create duplicate plans** — check if a similar plan already exists in the folder before creating a new one.
6. **Keep plans concise** — focus on actionable items, files to modify, and clear steps.
