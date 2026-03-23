using App.Data.Migrations;
using FluentMigrator;

namespace Satyanam.Plugin.Misc.AccountManagement.Data;

[NopMigration("2026/03/09 15:00:00:0000001", "Misc.AccountManagement add LeaveEncashmentAmount to EmployeeMonthlySalary", MigrationProcessType.Installation)]
public partial class LeaveEncashmentMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (Schema.Table("EmployeeMonthlySalary").Exists() &&
            !Schema.Table("EmployeeMonthlySalary").Column("LeaveEncashmentAmount").Exists())
        {
            Alter.Table("EmployeeMonthlySalary")
                .AddColumn("LeaveEncashmentAmount").AsDecimal(18, 4).NotNullable().WithDefaultValue(0);
        }
    }

    #endregion
}
