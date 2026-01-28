using System.Data;
using FluentMigrator.Builders.Create.Table;
using App.Data.Extensions;
using App.Core.Domain.Leaves;
using App.Core.Domain.Employees;
using App.Core.Domain.Customers;

namespace App.Data.Mapping.Builders
{
    /// <summary>
    /// Represents a LeaveManagement entity builder
    /// </summary>
    public partial class LeaveManagementBuilder : NopEntityBuilder<LeaveManagement>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(LeaveManagement.EmployeeId)).AsInt32().ForeignKey<Employee>(onDelete: Rule.SetNull).Nullable()
                .WithColumn(nameof(LeaveManagement.ApprovedOnUTC)).AsDateTime2().Nullable();
        }

        #endregion
    }
}