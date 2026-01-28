using System.Data;
using FluentMigrator.Builders.Create.Table;
using App.Data.Extensions;
using App.Core.Domain.Employees;
using App.Core.Domain.EmployeeAttendances;

namespace App.Data.Mapping.Builders
{
    /// <summary>
    /// Represents a EmployeeAttendance entity builder
    /// </summary>
    public partial class EmployeeAttendanceBuilder : NopEntityBuilder<EmployeeAttendance>
    {
        #region Methods
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(EmployeeAttendance.EmployeeId)).AsInt32().ForeignKey<Employee>(onDelete: Rule.SetNull).Nullable();
                
        }
        #endregion
    }
}