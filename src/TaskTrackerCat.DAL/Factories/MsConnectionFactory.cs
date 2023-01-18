using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TaskTrackerCat.DAL.Factories.Interfaces;

namespace TaskTrackerCat.DAL.Factories;

public class MsConnectionFactory : IDbConnectionFactory<SqlConnection>
{
    private readonly string _connectionString;
    private SqlConnection _connection;

    public MsConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetSection("ConnectionStringMSSQL").Value;
    }

    public SqlConnection CreateConnection()
    {
        if (_connection != null) return _connection;

        _connection = new SqlConnection(_connectionString);
        _connection.Open();
        if (_connection.State == ConnectionState.Closed) _connection = null;

        return _connection;
    }

    public void Dispose()
    {
        _connection?.Dispose();
        _connection = null;
    }
}