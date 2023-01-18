using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TaskTrackerCat.DAL.Factories.Interfaces;

namespace TaskTrackerCat.DAL.Factories;

public class MsConnectionFactory : IDbConnectionFactory<SqlConnection>
{
    private readonly string _connectionString;
    private SqlConnection _connection;
    private readonly object _lock = new object();
    
    public MsConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetSection("ConnectionStringMSSQL").Value;
    }

    public SqlConnection CreateConnection()
    {
        lock (_lock)
        {
            if (_connection != null) return _connection;

            _connection = new SqlConnection(_connectionString);
            _connection.Open();
            return _connection;
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}