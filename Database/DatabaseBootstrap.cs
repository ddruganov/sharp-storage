using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace storage.Database;

public class DatabaseBootstrap
{
    private const string ImageTableName = "image";
    private const string ModifiedImageTableName = "modified_image";

    private readonly DatabaseConfig _databaseConfig;

    public DatabaseBootstrap(DatabaseConfig databaseConfig)
    {
        _databaseConfig = databaseConfig;
    }

    public void Setup()
    {
        using var connection = new SqliteConnection(_databaseConfig.Dsn);

        SetupImageTable(connection);
        SetupModifiedImageTable(connection);
    }

    private static void SetupImageTable(IDbConnection connection)
    {
        var table = connection.Query<string>(
            $"select name from sqlite_master where type='table' and name = '{ImageTableName}';"
        );
        if (table.Any())
        {
            return;
        }

        connection.Execute($"create table {ImageTableName} (" +
                           "uuid varchar(36) NOT NULL UNIQUE," +
                           "size integer NOT NULL," +
                           "hash varchar(40) NOT NULL UNIQUE," +
                           "relative_path varchar(100) NOT NULL UNIQUE);"
        );
    }

    private static void SetupModifiedImageTable(IDbConnection connection)
    {
        var table = connection.Query<string>(
            $"select name from sqlite_master where type='table' and name = '{ModifiedImageTableName}';"
        );
        if (table.Any())
        {
            return;
        }

        connection.Execute($"create table {ModifiedImageTableName} (" +
                           "id integer PRIMARY KEY," +
                           "uuid varchar(36) NOT NULL," +
                           "hash varchar(40) NOT NULL UNIQUE ," +
                           "relative_path varchar(100) NOT NULL UNIQUE," +
                           $"foreign key (uuid) REFERENCES {ImageTableName}(uuid)" +
                           ")"
        );
    }
}