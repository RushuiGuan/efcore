---
_layout: landing
---

# Albatross.EFCore

A set of libraries to simplify EFCore setup for SQL Server and PostgreSQL.

## Libraries

| Package | Description |
|---|---|
| `Albatross.EFCore` | Base library: `DbSession`, `EntityMap<T>`, repository pattern, event system |
| `Albatross.EFCore.CodeGen` | Roslyn source generator — auto-registers all `EntityMap<T>` classes at compile time |
| `Albatross.EFCore.SqlServer` | SQL Server DI registration and constraint violation helpers |
| `Albatross.EFCore.PostgreSQL` | PostgreSQL DI registration, lowercase naming convention, constraint violation helpers |
| `Albatross.EFCore.Admin` | CLI admin utilities: migrate, run deployment scripts, generate SQL |
| `Albatross.EFCore.Audit` | Audit event handler that captures entity changes |
| `Albatross.EFCore.AutoCacheEviction` | Auto-evicts cache entries on entity saves |
| `Albatross.EFCore.ChangeReporting` | Produces a formatted change report for entity types |

## Latest Release

See the [Release Notes](release-notes.md) for what changed in **10.0.0**, including breaking changes to `ChangeReporting` and the new repository pattern.
