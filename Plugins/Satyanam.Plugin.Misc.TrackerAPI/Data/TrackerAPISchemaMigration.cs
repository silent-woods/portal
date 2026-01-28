using App.Data.Extensions;
using App.Data.Migrations;
using FluentMigrator;
using Satyanam.Plugin.Misc.TrackerAPI.Domain;

namespace Satyanam.Plugin.Misc.TrackerAPI.Data;

[NopMigration("2025/01/15 09:30:17:6455422", "Misc.TrackerAPI base schema", MigrationProcessType.Installation)]
public partial class TrackerAPISchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<TrackerAPILog>();
    }
}
