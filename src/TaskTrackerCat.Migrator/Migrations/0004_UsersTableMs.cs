using FluentMigrator;

namespace TaskTrackerCat.Migrator.Migrations;

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
                    email nvarchar(50), 
                    name nvarchar(50),
                    password nvarchar(50),
                    native_group_id INT,
                    current_group_id INT,
                    );");
    }

    public override void Down()
    {
        throw new NotImplementedException();
    }
}