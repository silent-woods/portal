using FluentMigrator;

namespace App.Data.Migrations
{
    [NopMigration("2026-04-10 10:00:00", "ZohoCampaignRecipient - add bounce columns", MigrationProcessType.Update)]
    public class ZohoCampaignRecipientBounceMigration : Migration
    {
        public override void Up()
        {
            const string table = "ZohoCampaignRecipient";

            if (!Schema.Table(table).Column("HasBounced").Exists())
                Alter.Table(table).AddColumn("HasBounced").AsBoolean().NotNullable().WithDefaultValue(false);

            if (!Schema.Table(table).Column("BounceCount").Exists())
                Alter.Table(table).AddColumn("BounceCount").AsInt32().NotNullable().WithDefaultValue(0);

            if (!Schema.Table(table).Column("BounceType").Exists())
                Alter.Table(table).AddColumn("BounceType").AsString(10).Nullable();

            if (!Schema.Table(table).Column("BouncedAt").Exists())
                Alter.Table(table).AddColumn("BouncedAt").AsDateTime2().Nullable();
        }

        public override void Down()
        {
            const string table = "ZohoCampaignRecipient";

            if (Schema.Table(table).Column("BouncedAt").Exists())
                Delete.Column("BouncedAt").FromTable(table);

            if (Schema.Table(table).Column("BounceType").Exists())
                Delete.Column("BounceType").FromTable(table);

            if (Schema.Table(table).Column("BounceCount").Exists())
                Delete.Column("BounceCount").FromTable(table);

            if (Schema.Table(table).Column("HasBounced").Exists())
                Delete.Column("HasBounced").FromTable(table);
        }
    }
}
