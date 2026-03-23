using App.Data.Migrations;
using FluentMigrator;

namespace Satyanam.Plugin.Misc.AccountManagement.Data;

[NopMigration("2026/03/09 14:00:00:0000001", "Misc.AccountManagement add IsRemainder to SalaryComponentConfig", MigrationProcessType.Update)]
public partial class SalaryComponentIsRemainderMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (Schema.Table("SalaryComponentConfig").Exists() &&
            !Schema.Table("SalaryComponentConfig").Column("IsRemainder").Exists())
        {
            Alter.Table("SalaryComponentConfig")
                .AddColumn("IsRemainder").AsBoolean().NotNullable().WithDefaultValue(false);
        }
    }

    #endregion
}
