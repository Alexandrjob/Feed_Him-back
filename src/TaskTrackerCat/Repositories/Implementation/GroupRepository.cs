using Dapper;
using Microsoft.Data.SqlClient;
using TaskTrackerCat.Infrastructure.Factories.Interfaces;
using TaskTrackerCat.Repositories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Repositories.Implementation;

public class GroupRepository : IGroupRepository
{
    private readonly IDbConnectionFactory<SqlConnection> _dbConnectionFactory;

    public GroupRepository(IDbConnectionFactory<SqlConnection> dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public async Task<GroupDto> AddGroupAsync(ConfigDto config)
    {
        var group = new GroupDto()
        {
            Name = "Покорми его",
            ConfigId = config.Id
        };

        var sql =
            "INSERT INTO groups " +
            "OUTPUT INSERTED.* " +
            "VALUES(@name, @ConfigId)";

        var connection = await _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<GroupDto>(sql, group);

        return result.FirstOrDefault();
    }

    public async Task<GroupDto> UpdateGroupAsync(UserDto user)
    {
        var sql = "UPDATE users " +
                  "SET current_group_id = @CurrentGroupId " +
                  "WHERE Id = @Id";

        var connection = await _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, user);

        return new GroupDto();
    }

    public async Task<List<GroupDto>> GetAllGroupsAsync()
    {
        var sql =
            @"SELECT config_id FROM groups";

        var connection = await _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<GroupDto>(sql);

        return result.ToList();
    }

    public async Task DeleteGroupAsync(GroupDto group)
    {
        var sql =
            "DELETE groups " +
            "WHERE id = @Id";

        var connection = await _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, group);
    }

    public async Task<GroupDto> GetGroupAsync(UserDto user)
    {
        var sql =
            @"SELECT * FROM groups " +
            "WHERE id = @CurrentGroupId";

        var connection = await _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<GroupDto>(sql, user);

        return result.FirstOrDefault();
    }

}