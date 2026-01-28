using App.Core.Domain.ProjectIntegrations;
using App.Data.Extensions;
using App.Data.Mapping;
using FluentMigrator;

namespace App.Data.Migrations.UpgradeTo460.Extension;

[NopMigration("2025-07-07 02:02:21", "ProjectIntegrationMappingsMigration for 4.60.0", MigrationProcessType.Update)]
public partial class ProjectIntegrationMappingsMigration : Migration
{
    #region Fields

    protected readonly INopDataProvider _nopDataProvider;

    #endregion

    #region Ctor

    public ProjectIntegrationMappingsMigration(INopDataProvider nopDataProvider)
    {
        _nopDataProvider = nopDataProvider;
    }

    #endregion

    #region Methods

    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ProjectIntegrationMappings))).Exists())
            Create.TableFor<ProjectIntegrationMappings>();
    }

    public override void Down()
    {
    }

    #endregion
}
