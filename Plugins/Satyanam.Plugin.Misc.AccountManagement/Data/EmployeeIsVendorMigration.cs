using App.Data.Migrations;
using FluentMigrator;

namespace Satyanam.Plugin.Misc.AccountManagement.Data;

[NopMigration("2026/03/13 11:00:00:0000001", "Employee add IsVendor column", MigrationProcessType.Update)]
public partial class EmployeeIsVendorMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (!Schema.Table("Employee").Column("IsVendor").Exists())
            Alter.Table("Employee").AddColumn("IsVendor").AsBoolean().NotNullable().WithDefaultValue(false);
    }

    #endregion
}
