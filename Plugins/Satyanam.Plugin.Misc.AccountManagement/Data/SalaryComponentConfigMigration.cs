using App.Data.Migrations;
using FluentMigrator;

namespace Satyanam.Plugin.Misc.AccountManagement.Data;

[NopMigration("2026/03/09 12:00:00:0000001", "Misc.AccountManagement add SalaryComponentConfig table", MigrationProcessType.Installation)]
public partial class SalaryComponentConfigMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (!Schema.Table("SalaryComponentConfig").Exists())
        {
            Create.Table("SalaryComponentConfig")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable()
                .WithColumn("Name").AsString(200).NotNullable()
                .WithColumn("ComponentTypeId").AsInt32().NotNullable()
                .WithColumn("IsPercentage").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("Value").AsDecimal(18, 4).NotNullable().WithDefaultValue(0)
                .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("DisplayOrder").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("CreatedOnUtc").AsDateTime2().NotNullable()
                .WithColumn("UpdatedOnUtc").AsDateTime2().NotNullable();
        }
    }

    #endregion
}
