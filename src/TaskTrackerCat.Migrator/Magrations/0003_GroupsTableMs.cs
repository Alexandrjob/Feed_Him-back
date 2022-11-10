using FluentMigrator;

namespace TaskTrackerCat.Migrator.Magrations;

[Migration(3)]
public class GroupsTableMs : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            IF NOT EXISTS (
                SELECT * FROM sys.tables t 
                JOIN sys.schemas s ON (t.schema_id = s.schema_id) 
                WHERE s.name = 'groups') 	
                CREATE TABLE groups(
                    id INT PRIMARY KEY IDENTITY,
                    name nvarchar(50),
                    config_id INT               
                    );");
    }

    public override void Down()
    {
        throw new NotImplementedException();
    }
}