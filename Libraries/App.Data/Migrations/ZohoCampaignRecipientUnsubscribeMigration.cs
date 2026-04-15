using FluentMigrator;

namespace App.Data.Migrations
{
    [NopMigration("2026-04-13 10:00:00", "ZohoCampaignRecipient - add unsubscribe columns", MigrationProcessType.Update)]
    public class ZohoCampaignRecipientUnsubscribeMigration : Migration
    {
        public override void Up()
        {
            const string table = "ZohoCampaignRecipient";

            if (!Schema.Table(table).Column("HasUnsubscribed").Exists())
                Alter.Table(table).AddColumn("HasUnsubscribed").AsBoolean().NotNullable().WithDefaultValue(false);

            if (!Schema.Table(table).Column("UnsubscribedAt").Exists())
                Alter.Table(table).AddColumn("UnsubscribedAt").AsDateTime2().Nullable();
        }

        public override void Down()
        {
            const string table = "ZohoCampaignRecipient";

            if (Schema.Table(table).Column("UnsubscribedAt").Exists())
                Delete.Column("UnsubscribedAt").FromTable(table);

            if (Schema.Table(table).Column("HasUnsubscribed").Exists())
                Delete.Column("HasUnsubscribed").FromTable(table);
        }
    }
}
