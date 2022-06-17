using Dapper;
using Microsoft.Data.Sqlite;
using storage.Data;
using storage.Database;
using ILogger = Serilog.ILogger;

namespace storage.Repositories;

public class ImageRepository
{
    private readonly DatabaseConfig _databaseConfig;

    public ImageRepository(DatabaseConfig databaseConfig, ILogger logger)
    {
        _databaseConfig = databaseConfig;
    }

    public async Task<bool> Create(Image image)
    {
        await using var connection = new SqliteConnection(_databaseConfig.Dsn);

        var result = await connection.ExecuteAsync(
            "insert into image (uuid, size, hash, relative_path)" +
            "VALUES (@Uuid, @Size, @Hash, @RelativePath);",
            image
        );

        return result != 0;
    }

    public async Task<Image?> Get(Guid uuid)
    {
        return await GetImageByField(nameof(uuid), new { uuid });
    }

    public async Task<Image?> Get(string hash)
    {
        return await GetImageByField(nameof(hash), new { hash });
    }

    private async Task<Image?> GetImageByField(string paramName, object paramConfig)
    {
        await using var connection = new SqliteConnection(_databaseConfig.Dsn);

        return await connection.QueryFirstOrDefaultAsync<Image?>(
            $"SELECT uuid as Uuid, size as Size, hash as Hash, relative_path as RelativePath FROM image where {paramName} = @{paramName};",
            paramConfig);
    }

    public async Task<bool> CreateModified(ModifiedImage modifiedImage)
    {
        await using var connection = new SqliteConnection(_databaseConfig.Dsn);

        var result = await connection.ExecuteAsync(
            "insert into modified_image (uuid, hash, relative_path)" +
            "VALUES (@Uuid, @Hash, @RelativePath);",
            modifiedImage
        );

        return result != 0;
    }

    public async Task<ModifiedImage?> GetModified(string hash)
    {
        await using var connection = new SqliteConnection(_databaseConfig.Dsn);

        return await connection.QueryFirstOrDefaultAsync<ModifiedImage?>(
            $"SELECT uuid as Uuid, hash as Hash, relative_path as RelativePath FROM modified_image where hash = @hash;",
            new { hash });
    }
}