using System.Data;
using System.Data.Common;

namespace Dfe.Data.Common.Infrastructure.Persistence.Sql.Dapper.Providers.Database.Context.TestDoubles;

public sealed class DbConnectionFake : DbConnection
{
    private ConnectionState _state = ConnectionState.Closed;

    public bool OpenCalled { get; private set; }
    public bool DisposeCalled { get; private set; }
    public bool BeginTransactionCalled { get; private set; }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        BeginTransactionCalled = true;
        return new DbTransactionFake(this, isolationLevel);
    }

    public override Task OpenAsync(CancellationToken cancellationToken)
    {
        OpenCalled = true;
        _state = ConnectionState.Open;
        return Task.CompletedTask;
    }

    public override void Close()
    {
        _state = ConnectionState.Closed;
    }

    public override string ConnectionString { get; set; } = string.Empty;
    public override string Database => "FakeDb";
    public override string DataSource => "FakeSource";
    public override string ServerVersion => "1.0";
    public override ConnectionState State => _state;

    protected override DbCommand CreateDbCommand()
        => throw new NotImplementedException();

    protected override void Dispose(bool disposing)
    {
        DisposeCalled = true;
        base.Dispose(disposing);
    }

    public override void ChangeDatabase(string databaseName)
    {
        throw new NotImplementedException();
    }

    public override void Open()
    {
        throw new NotImplementedException();
    }
}