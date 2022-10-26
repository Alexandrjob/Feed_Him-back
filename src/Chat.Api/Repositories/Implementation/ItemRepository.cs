using Chat.Api.Infrastructure.Factories.Interfaces;
using Chat.Api.Repositories.Interfaces;
using Chat.Api.Repositories.Models;
using Dapper;
using Npgsql;

namespace Chat.Api.Repositories.Implementation;

public class ItemRepository : IItemRepository
{
    private readonly IDbConnectionFactory<NpgsqlConnection> _dbConnectionFactory;

    public ItemRepository(IDbConnectionFactory<NpgsqlConnection> dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<ItemDto> GetItemAsync(int id)
    {
        var sql = @"SELECT * FROM items WHERE Id = @id";

        var connection = await _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<ItemDto>(sql, new {id});

        return result.FirstOrDefault();
    }
}