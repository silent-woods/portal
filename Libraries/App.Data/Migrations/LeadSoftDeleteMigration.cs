using FluentMigrator;

namespace App.Data.Migrations
{
    [NopMigration("2026-04-09 10:00:00", "Lead - add soft delete columns", MigrationProcessType.Update)]
    public class LeadSoftDeleteMigration : Migration
    {
        public override void Up()
        {
            const string table = "Lead";

            if (!Schema.Table(table).Column("IsDeleted").Exists())
                Alter.Table(table).AddColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false);

            if (!Schema.Table(table).Column("DeletedOnUtc").Exists())
                Alter.Table(table).AddColumn("DeletedOnUtc").AsDateTime2().Nullable();
        }

        public override void Down()
        {
            const string table = "Lead";

            if (Schema.Table(table).Column("DeletedOnUtc").Exists())
                Delete.Column("DeletedOnUtc").FromTable(table);

            if (Schema.Table(table).Column("IsDeleted").Exists())
                Delete.Column("IsDeleted").FromTable(table);
        }
    }
}
