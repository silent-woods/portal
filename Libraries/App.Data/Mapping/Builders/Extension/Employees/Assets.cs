using App.Core.Domain.Employees;
using App.Data.Mapping.Builders;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Employees
{
    /// <summary>
    /// Represents a employee entity builder
    /// </summary>
    public partial class AssetsBuilder : NopEntityBuilder<Assets>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Assets.EmployeeID)).AsString(1000).NotNullable();
        }

        #endregion
    }
}