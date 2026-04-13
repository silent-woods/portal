using App.Data.Extensions;
using App.Data.Mapping;
using App.Data.Migrations;
using FluentMigrator;
using Satyanam.Nop.Core.Domains;

namespace Satyanam.Nop.Core.Data
{
    [NopMigration("2026/04/03 10:00:00", "ZohoCampaignStat schema", MigrationProcessType.Update)]
    public class ZohoCampaignSchemaMigration : AutoReversingMigration
    {
        #region Methods

        public override void Up()
        {
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ZohoCampaignStat))).Exists())
                Create.TableFor<ZohoCampaignStat>();

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ZohoCampaignLocationStat))).Exists())
                Create.TableFor<ZohoCampaignLocationStat>();
        }

        #endregion
    }
}
