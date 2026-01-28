using App.Data.Extensions;
using App.Data.Migrations;
using FluentMigrator;
using Satyanam.Plugin.Misc.AzureIntegration.Domain;

namespace Satyanam.Plugin.Misc.AzureIntegration.Data;

[NopMigration("2025/06/30 11:33:17:6455422", "Misc.AzureIntegration base schema", MigrationProcessType.Installation)]
public partial class AzureIntegrationSchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<AzureSyncLog>();
    }
}
