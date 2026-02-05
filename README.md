# Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper

A strongly typed, dependency-injected Dapper persistence framework designed for multi-database applications.

This library provides:

- Strongly typed database identifiers (`IDbName`)
- Runtime-generated database name types
- Scoped database context providers
- Connection and transaction lifecycle management
- SQL query/command handlers and orchestrators
- Parameter redaction for secure logging
- DI extension methods for easy registration

It is built to be **testable**, **extensible**, and **safe by default**.

---

## Features

### Strongly typed database names

Each database is represented by a type implementing `IDbName`.

You can:

- Define your own marker type, or
- Register a database using a runtime-generated type via a string name.

### Scoped database context provider

`DbContextProvider<TName>` manages:

- Opening connections  
- Beginning transactions  
- Ensuring disposal  
- Injecting query/command handlers  
- Logging lifecycle events  

Each scope gets a consistent connection + transaction context.

### Connection factory abstraction

Connections are created via:

```csharp
DbConnectionFactory<TName>
```
This allows:

- Per‑database connection logic  
- Dependency‑injection‑friendly construction  
- Easy mocking and substitution in unit tests  

### SQL execution pipeline

The library includes a full execution pipeline built around Dapper:

- `ISqlQueryHandler`
- `ISqlCommandHandler`
- `ISqlExecutionOrchestrator`
- `ICommandDefinitionFactory`

These components wrap raw Dapper calls with:

- Structured logging  
- Parameter redaction  
- Cancellation token support  
- Consistent command definition creation  
- Optional transaction participation  

### Secure parameter redaction

`SqlParameterRedactor` ensures sensitive values never appear in logs.

It supports:

- Full masking (e.g., `"******"`)
- Partial masking (e.g., `"12****89"`)
- Custom placeholder values
- Recursive traversal of objects and dictionaries

This makes it safe to enable verbose SQL logging in production environments.

### Dependency injection extensions

The `CompositionRoot` class provides extension methods for registering all required services:

```csharp
services.AddDatabase<TName>(...)
services.AddDatabaseAsDefault<TName>(...)
services.AddDatabase("name", ...)
services.AddDatabaseAsDefault("name", ...)
```
These register:

- Connection factories  
- Database options (`DatabaseOptions` and `IOptions<DatabaseOptions>`)  
- SQL command and query handlers  
- The SQL execution orchestrator  
- The parameter redactor  
- The strongly typed `DbContextProvider<TName>`  
- The default `IDbContextProvider` implementation  

### Runtime type generation

When registering a database by string name:

```csharp
services.AddDatabase("MyDb", ...)
```
the library generates a runtime type implementing `IDbName`.

To prevent duplicate type name exceptions, these generated types are cached:

```csharp
private static readonly Dictionary<string, Type> RuntimeTypes = new();
```
This ensures:

- Deterministic type reuse  
- Safe parallel test execution  
- No duplicate type collisions in the dynamic assembly  

---

## Getting started

### Register a strongly typed database

```csharp
services.AddDatabase<MyDbName>(sp =>
    new SqlConnection(configuration.GetConnectionString("MyDb")));
```
### Register a database by string name

```csharp
services.AddDatabase("ReportingDb", sp =>
    new SqlConnection(configuration.GetConnectionString("ReportingDb")));
```
### Register as the default database

```csharp
services.AddDatabaseAsDefault<MyDbName>(sp =>
    new SqlConnection(configuration.GetConnectionString("MyDb")));
```
### Using the DbContextProvider

```csharp
public class MyService
{
    private readonly IDbContextProvider _db;

    public MyService(IDbContextProvider db) => _db = db;

    public async Task<IEnumerable<User>> GetUsersAsync(CancellationToken ct)
    {
        // Ensures the connection is opened and the context is ready
        await _db.ConnectAsync(ct);

        // Execute a query using the registered SQL query handler
        return await _db.SqlQueryHandler.QueryAsync<User>(
            "SELECT * FROM Users",
            new { },
            ct);
    }
}
```
## Testing

The library is designed for testability:

- Fake `DbConnection` and `DbTransaction` implementations  
- DI‑driven construction  
- Minimal static state (only cached runtime types)  
- Redaction logic fully unit tested  
- DI registration validated via `ServiceCollection`  

Example:

```csharp
var services = new ServiceCollection();
services.AddDatabase<TestDbName>(_ => new FakeDbConnection());

var provider = services.BuildServiceProvider();

var ctx = provider.GetRequiredService<DbContextProvider<TestDbName>>();
```
## Security

- Sensitive SQL parameters are redacted before logging  
- Supports full and partial masking  
- Custom placeholder support  
- Null keys safely skipped  
- Nested objects and dictionaries handled  

This allows detailed SQL logging without exposing secrets in production environments.

---

## Architecture overview

```text
CompositionRoot
 ├─ Registers all services
 ├─ Generates runtime IDbName types
 └─ Provides AddDatabase / AddDatabaseAsDefault

DbContextProvider<TName>
 ├─ Manages connection lifecycle
 ├─ Manages transactions
 ├─ Provides query + command handlers
 └─ Ensures disposal (sync + async)

Handlers
 ├─ SqlQueryHandler
 ├─ SqlCommandHandler
 ├─ SqlExecutionOrchestrator
 └─ CommandDefinitionFactory

Logging
 └─ SqlParameterRedactor (full/partial masking)

Options
 └─ DatabaseOptions (isolation level, connection-open callbacks)
```
 ## DatabaseOptions

```csharp
public class DatabaseOptions
{
    public IsolationLevel DefaultIsolationLevel { get; set; }
    public Func<DbConnection, CancellationToken, Task>? OnConnectionOpen { get; set; }
}
```
`DefaultIsolationLevel`  
Defines the default transaction isolation level used when a transaction is started without explicitly specifying one. This allows you to enforce consistent transactional behavior across all database operations unless overridden.

`OnConnectionOpen`  
Provides a hook that runs immediately after a database connection is opened. This is useful for executing session‑level configuration such as:

- `SET NOCOUNT ON`
- `SET TRANSACTION ISOLATION LEVEL ...`
- Enabling or disabling ANSI settings
- Applying tenant or context metadata

---

## Extensibility

You can extend or replace:

- **Redaction behavior** (`ISqlParameterRedactor`)
- **Execution orchestration** (`ISqlExecutionOrchestrator`)
- **Command/query handlers**
- **Connection factories** (`DbConnectionFactory<TName>`)
- **Logging configuration** (`ILogger<T>`)

Because everything is wired through dependency injection, customization is straightforward and non‑intrusive.

---



