using FluentMigrator;

namespace TaskTrackerCat.Migrator.Magrations;

[Migration(1)]
public class DietsTable : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            CREATE TABLE if not exists diets(
                id BIGSERIAL PRIMARY KEY,
                serving_number INT,
                waiter_name TEXT,
                date timestamp,
                status boolean,
                estimated_date_feeding date
                )");
    }

    public override void Down()
    {
        Execute.Sql("DROP TABLE if exists diets;");
    }
}