using FluentMigrator;

namespace TaskTrackerCat.Migrator.Magrations;

[Migration(1)]
public class DietsTableMS : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            IF NOT EXISTS (
                SELECT * FROM sys.tables t 
                JOIN sys.schemas s ON (t.schema_id = s.schema_id) 
                WHERE s.name = 'diets') 	
                CREATE TABLE diets(
                    id INT PRIMARY KEY IDENTITY,
                    serving_number INT,
                    waiter_name nvarchar(50),
                    date datetime,
                    status BIT,
                    estimated_date_feeding datetime,
                    group_id INT,
                    );");
    }

    public override void Down()
    {
        Execute.Sql("IF EXISTS (" +
                    "DROP TABLE dbo.diets" +
                    ");");
    }
}