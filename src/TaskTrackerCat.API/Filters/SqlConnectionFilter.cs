using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using TaskTrackerCat.DAL.Factories.Interfaces;

namespace TaskTrackerCat.API.Filters;

public class SqlConnectionFilter : ActionFilterAttribute
{
    private readonly IDbConnectionFactory<SqlConnection> _dbConnectionFactory;

    public SqlConnectionFilter(IDbConnectionFactory<SqlConnection> dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        _dbConnectionFactory.Dispose();
    }
}