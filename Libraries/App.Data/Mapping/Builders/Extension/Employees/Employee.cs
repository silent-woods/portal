using App.Core.Domain.Employees;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Builders.Employees
{
    /// <summary>
    /// Represents a employee entity builder
    /// </summary>
    public partial class EmployeeBuilder : NopEntityBuilder<Employee>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Employee.FirstName)).AsString(1000).NotNullable();
        }

        #endregion
    }
}