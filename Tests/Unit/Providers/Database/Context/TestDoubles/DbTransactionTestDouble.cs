using System.Data;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Providers.Database.Context.TestDoubles;

public sealed class DbTransactionFake : DbTransaction
{
    private readonly DbConnection _connection;

    public bool DisposeCalled { get; private set; }
    public override IsolationLevel IsolationLevel { get; }

    public DbTransactionFake(DbConnection connection, IsolationLevel isolationLevel)
    {
        _connection = connection;
        IsolationLevel = isolationLevel;
    }

    protected override DbConnection? DbConnection => _connection;

    public override void Commit() { }
    public override void Rollback() { }

    protected override void Dispose(bool disposing)
    {
        DisposeCalled = true;
        base.Dispose(disposing);
    }
}