using App.Data.Extensions;
using App.Data.Mapping;
using App.Data.Migrations;
using FluentMigrator;
using Satyanam.Nop.Core.Domains;

namespace Satyanam.Nop.Core.Data
{
    [NopMigration("2026/04/06 10:00:00", "ZohoCampaignDailyStat and ZohoCampaignRecipient schema", MigrationProcessType.Update)]
    public class ZohoCampaignTimelineMigration : AutoReversingMigration
    {
        #region Methods

        public override void Up()
        {
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ZohoCampaignDailyStat))).Exists())
                Create.TableFor<ZohoCampaignDailyStat>();

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ZohoCampaignRecipient))).Exists())
                Create.TableFor<ZohoCampaignRecipient>();
        }

        #endregion
    }
}
