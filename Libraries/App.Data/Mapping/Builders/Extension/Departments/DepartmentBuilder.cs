using App.Core.Domain.Departments;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Builders.Departments
{
    /// <summary>
    /// Represents a department entity builder
    /// </summary>
    public partial class DepartmentBuilder : NopEntityBuilder<Department>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Department.Name)).AsString(1000).NotNullable();
        }

        #endregion
    }
}