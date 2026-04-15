using FluentMigrator;

namespace App.Data.Migrations
{
    [NopMigration("2026-04-10 11:00:00", "Lead - add interest score columns", MigrationProcessType.Update)]
    public class LeadInterestScoreMigration : Migration
    {
        public override void Up()
        {
            const string table = "Lead";

            if (!Schema.Table(table).Column("InterestScore").Exists())
                Alter.Table(table).AddColumn("InterestScore").AsInt32().Nullable();

            if (!Schema.Table(table).Column("InterestScoreUpdatedUtc").Exists())
                Alter.Table(table).AddColumn("InterestScoreUpdatedUtc").AsDateTime2().Nullable();
        }

        public override void Down()
        {
            const string table = "Lead";

            if (Schema.Table(table).Column("InterestScoreUpdatedUtc").Exists())
                Delete.Column("InterestScoreUpdatedUtc").FromTable(table);

            if (Schema.Table(table).Column("InterestScore").Exists())
                Delete.Column("InterestScore").FromTable(table);
        }
    }
}
