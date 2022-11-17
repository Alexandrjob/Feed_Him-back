using Dapper;
using Microsoft.Data.SqlClient;
using TaskTrackerCat.Infrastructure.Factories.Interfaces;
using TaskTrackerCat.Repositories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Repositories.Implementation;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory<SqlConnection> _dbConnectionFactory;

    public UserRepository(IDbConnectionFactory<SqlConnection> dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public async Task<UserDto> AddUserAsync(UserDto user)
    {
        var sql =
            "INSERT INTO users " +
            "OUTPUT INSERTED.* " +
            "VALUES(@Email,@Name, @Password, @GroupId)";

        var connection = await _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<UserDto>(sql, user);

        return result.FirstOrDefault();
    }

    public async Task<UserDto> GetUserAsync(UserDto user)
    {
        var sql =
            @"SELECT * FROM users " +
            "WHERE email = @Email";

        var connection = await _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<UserDto>(sql, user);

        return result.FirstOrDefault();
    }

    public Task UpdateUserAsync(UserDto user)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteUserAsync(UserDto user)
    {
        var sql =
            "DELETE users " +
            "WHERE id = @Id";

        var connection = await _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, user);
    }
}