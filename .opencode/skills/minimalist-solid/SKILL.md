---
name: minimalist-solid
description: Use ALWAYS when creating new files, writing code, or implementing features. Ensures minimalist file creation and adherence to SOLID principles. Triggers on any file creation, code writing, or feature implementation task.
---

# Minimalist + SOLID Skill

## Minimalism Rules

1. **NEVER create files that are not strictly necessary.** Before creating any file, ask: "Is this file absolutely required for the feature to work?"

2. **Do not create**:
   - README files unless explicitly requested
   - Documentation files (*.md) unless explicitly requested
   - Test files unless explicitly requested or part of the task
   - Config files unless the feature requires them
   - Empty or placeholder files
   - Duplicate files with slight variations

3. **Prefer editing existing files** over creating new ones. Look for existing utilities, helpers, or components that can be extended.

4. **One file, one responsibility.** Do not split a single concern across multiple files unnecessarily.

5. **No over-engineering.** Implement only what is needed now. Do not add features "just in case" or "for future use."

## SOLID Principles

### Single Responsibility Principle (SRP)
- Each class should have one reason to change
- Each file should contain one class or one clear responsibility
- Do not mix data access, business logic, and UI in the same class

### Open/Closed Principle (OCP)
- Open for extension, closed for modification
- Use interfaces and abstractions to allow extending behavior without changing existing code
- Prefer composition over inheritance

### Liskov Substitution Principle (LSP)
- Subtypes must be substitutable for their base types
- Do not create subclasses that break the contract of the parent class

### Interface Segregation Principle (ISP)
- Many small, specific interfaces are better than one general-purpose interface
- Do not force classes to implement methods they do not use

### Dependency Inversion Principle (DIP)
- Depend on abstractions, not concretions
- High-level modules should not depend on low-level modules; both should depend on abstractions
- Use dependency injection when possible

## Code Style

1. **Follow existing conventions** in the codebase — naming, structure, patterns.
2. **No unnecessary comments.** Code should be self-explanatory.
3. **Keep methods short** — if a method exceeds 30 lines, consider refactoring.
4. **Use existing libraries and patterns** — do not reinvent what already exists in the project.
5. **Minimal imports** — only import what is needed.
