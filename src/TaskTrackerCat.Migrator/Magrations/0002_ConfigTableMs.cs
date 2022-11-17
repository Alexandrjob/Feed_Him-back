using FluentMigrator;

namespace TaskTrackerCat.Migrator.Magrations;

[Migration(2)]
public class ConfigTableMs : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            IF NOT EXISTS (
                SELECT * FROM sys.tables t 
                JOIN sys.schemas s ON (t.schema_id = s.schema_id) 
                WHERE s.name = 'configs') 	
                CREATE TABLE configs(
                    id INT PRIMARY KEY IDENTITY,
                    number_meals_per_day INT,
                    start_feeding time,
                    end_feeding time
                    );");
    }

    public override void Down()
    {
        Execute.Sql("IF EXISTS (" +
                    "DROP TABLE dbo.config" +
                    ");");
    }
}