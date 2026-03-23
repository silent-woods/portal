using App.Data.Migrations;
using FluentMigrator;

namespace Satyanam.Plugin.Misc.AccountManagement.Data;

[NopMigration("2026/03/09 16:00:00:0000001", "Misc.AccountManagement add LeaveEncashmentDays to EmployeeMonthlySalary", MigrationProcessType.Installation)]
public partial class LeaveEncashmentDaysMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (Schema.Table("EmployeeMonthlySalary").Exists() &&
            !Schema.Table("EmployeeMonthlySalary").Column("LeaveEncashmentDays").Exists())
        {
            Alter.Table("EmployeeMonthlySalary")
                .AddColumn("LeaveEncashmentDays").AsDecimal(18, 4).NotNullable().WithDefaultValue(0);
        }
    }

    #endregion
}
