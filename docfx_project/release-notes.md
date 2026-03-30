# Release Notes

## 10.0.0

Targets **.NET 10**. Contains breaking changes.

### Breaking Changes

#### Albatross.EFCore.ChangeReporting

`ChangeReportDbEventHandler<T>` has been redesigned. The mutable `ChangeReportingOptions` class has been removed. All configuration is now done via `required init` properties directly on the handler, built through `ChangeReportBuilder<T>`.

The following `ChangeReportBuilder<T>` extension methods have been removed:
- `ChangeType`, `FixedHeaders`, `IgnoreProperties`, `OnReportGenerated`
- `Prefix`, `Postfix`
- `Formatter`, `AsyncFormatter`, `Format`, `FormatFixedHeader`
- `NumericFormat`, `DateFormat`, `TimeFormat`
- `Deleted`, `Modified`, `Added`, `AllChangeTypes`

The remaining extension methods are `ExcludeAuditProperties` and `ExcludeTemporalProperties`.

`IChangeReport` and `ChangeReportingOptions` have been removed.

Text generation is now synchronous — `BuildText` returns `string` instead of `Task<string>`.

#### Albatross.EFCore.CodeGen

The source generator has been rewritten as an incremental generator (`IIncrementalGenerator`). The previous `EntityModelBuilderClassCodeGen` and `EntityModelClassWalker` classes have been replaced by `EntityModelBuilderClassCodeGenerator`. Generated output and behavior are unchanged.

#### Albatross.EFCore.Testing (removed)

The `Albatross.Testing.EFCore` package has been removed.

---

### New Features

#### Albatross.EFCore — Repository Pattern

A new `IRepository` interface and `Repository<T>` abstract base class have been added.

- `IRepository` exposes `Add<T>`, `Delete<T>`, and `SaveChangesAsync`
- `SaveChangesAsync` returns a `SaveResults` record with `Success`, `NameConflict`, `ForeignKeyConflict`, and `Error` — no exception is thrown on constraint violations
- `Repository<T>` requires subclasses to implement `IsUniqueConstraintViolation` and `IsForeignKeyConstraintViolation`

#### Albatross.EFCore.SqlServer — Constraint Violation Helpers

Two extension methods on `Exception` have been added:
- `IsUniqueConstraintViolation()` — detects SQL errors 2601 and 2627
- `IsForeignKeyConstraintViolation()` — detects SQL error 547

#### Albatross.EFCore.PostgreSQL — Constraint Violation Helpers

Two extension methods on `Exception` have been added:
- `IsUniqueConstraintViolation()` — detects `SqlState` `23505`
- `IsForeignKeyConstraintViolation()` — detects `SqlState` `23503`

---

## 8.1.2

- Documentation updates
- Minor internal fixes
