using FluentMigrator;

namespace TaskTrackerCat.Migrator.Magrations;

[Migration(4)]
public class UsersTableMs : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            IF NOT EXISTS (
                SELECT * FROM sys.tables t 
                JOIN sys.schemas s ON (t.schema_id = s.schema_id) 
                WHERE s.name = 'users') 	
                CREATE TABLE users(
                    id INT PRIMARY KEY IDENTITY,
                    name nvarchar(50),
                    group_id INT
                    );");
    }

    public override void Down()
    {
        throw new NotImplementedException();
    }
}