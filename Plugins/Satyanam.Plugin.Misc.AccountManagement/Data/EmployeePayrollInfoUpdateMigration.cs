using App.Data.Migrations;
using FluentMigrator;
using static LinqToDB.Reflection.Methods.LinqToDB;

namespace Satyanam.Plugin.Misc.AccountManagement.Data;

[NopMigration("2026/03/13 10:00:00:0000001", "Misc.AccountManagement add CTC, IFSCCode, BeneficiaryName to EmployeePayrollInfo; remove CTC from Employee", MigrationProcessType.Update)]
public partial class EmployeePayrollInfoUpdateMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (!Schema.Table("EmployeePayrollInfo").Column("CTC").Exists())
            Alter.Table("EmployeePayrollInfo").AddColumn("CTC").AsString(100).Nullable();

        if (!Schema.Table("EmployeePayrollInfo").Column("IFSCCode").Exists())
            Alter.Table("EmployeePayrollInfo").AddColumn("IFSCCode").AsString(20).Nullable();

        if (!Schema.Table("EmployeePayrollInfo").Column("BeneficiaryName").Exists())
            Alter.Table("EmployeePayrollInfo").AddColumn("BeneficiaryName").AsString(200).Nullable();
    }
    #endregion
}
