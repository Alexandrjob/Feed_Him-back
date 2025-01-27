﻿using Dapper;
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
            "VALUES(@Email, @Name, @Password, @CurrentGroupId, @NativeGroupId)";

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

    public async Task<List<UserDto>> GetUsersGroupAsync(GroupDto group)
    {
        var sql =
            @"SELECT name, email FROM users " +
            "WHERE current_group_id = @Id";

        var connection = await _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<UserDto>(sql, group);

        return result.ToList();
    }

    public async Task UpdateEmailNameAsync(UserDto user)
    {
        var sql = "UPDATE users " +
                  "SET name = @Name, " +
                  "email = @Email " +
                  "WHERE Id = @Id";

        var connection = await _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, user);
    }

    public async Task UpdatePasswordAsync(UserDto user)
    {
        var sql = "UPDATE users " +
                  "SET password = @Password " +
                  "WHERE email = @Email";

        var connection = await _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, user);
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