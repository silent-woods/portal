using App.Core.Domain.ProjectIntegrations;
using App.Data.Extensions;
using App.Data.Mapping;
using FluentMigrator;

namespace App.Data.Migrations.UpgradeTo460.Extension;

[NopMigration("2025-07-07 03:03:32", "ProjectIntegrationSettingsMigration for 4.60.0", MigrationProcessType.Update)]
public partial class ProjectIntegrationSettingsMigration : Migration
{
    #region Fields

    protected readonly INopDataProvider _nopDataProvider;

    #endregion

    #region Ctor

    public ProjectIntegrationSettingsMigration(INopDataProvider nopDataProvider)
    {
        _nopDataProvider = nopDataProvider;
    }

    #endregion

    #region Methods

    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ProjectIntegrationSettings))).Exists())
            Create.TableFor<ProjectIntegrationSettings>();
    }

    public override void Down()
    {
    }

    #endregion
}
