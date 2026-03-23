using App.Data.Migrations;
using FluentMigrator;

namespace Satyanam.Plugin.Misc.AccountManagement.Data;

[NopMigration("2026/03/09 10:00:00:0000001", "Misc.AccountManagement salary audit log schema", MigrationProcessType.Installation)]
public partial class SalaryAuditLogMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (!Schema.Table("EmployeeSalaryAuditLog").Exists())
        {
            Create.Table("EmployeeSalaryAuditLog")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable()
                .WithColumn("EmployeeSalaryRecordId").AsInt32().NotNullable()
                .WithColumn("FieldName").AsString(200).NotNullable()
                .WithColumn("OldValue").AsString(int.MaxValue).Nullable()
                .WithColumn("NewValue").AsString(int.MaxValue).Nullable()
                .WithColumn("ChangedByEmployeeId").AsInt32().NotNullable()
                .WithColumn("ChangedOnUtc").AsDateTime2().NotNullable();
        }
    }

    #endregion
}
