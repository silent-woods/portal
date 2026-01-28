using System.Data;
using FluentMigrator.Builders.Create.Table;
using App.Data.Extensions;
using App.Core.Domain.PerformanceMeasurements;
using App.Core.Domain.Employees;

namespace App.Data.Mapping.Builders
{
    /// <summary>
    /// Represents a TeamPerformanceMeasurement entity builder
    /// </summary>
    public partial class TeamPerformanceMeasurementBuilder : NopEntityBuilder<TeamPerformanceMeasurement>
    {
        #region Methods
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(TeamPerformanceMeasurement.EmployeeId)).AsInt32().ForeignKey<Employee>(onDelete: Rule.SetNull).Nullable();
        }
        #endregion
    }
}