using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Ozge.Data.Storage;

public sealed class SqlitePragmaConnectionInterceptor : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        if (connection is not SqliteConnection sqliteConnection)
        {
            return;
        }

        using var command = sqliteConnection.CreateCommand();
        command.CommandText = @"
PRAGMA journal_mode=WAL;
PRAGMA synchronous=NORMAL;
PRAGMA cache_size=-20000;
PRAGMA busy_timeout=5000;";
        command.ExecuteNonQuery();
    }
}
