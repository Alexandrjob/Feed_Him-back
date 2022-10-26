using System.Data;
using Chat.Api.Infrastructure.Factories.Interfaces;
using Npgsql;

namespace Chat.Api.Infrastructure.Factories;

public class NpgsqlConnectionFactory : IDbConnectionFactory<NpgsqlConnection>
{
    private readonly string _connectionString;
    private NpgsqlConnection _connection;

    public NpgsqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetSection("ConnectionString").Value;
    }

    public async Task<NpgsqlConnection> CreateConnection()
    {
        if (_connection != null)
        {
            return _connection;
        }

        _connection = new NpgsqlConnection(_connectionString);
        await _connection.OpenAsync();
        _connection.StateChange += (o, e) =>
        {
            if (e.CurrentState == ConnectionState.Closed)
            {
                _connection = null;
            }
        };
        return _connection;
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}