using System.Data;
using FluentMigrator.Builders.Create.Table;
using App.Data.Extensions;
using App.Core.Domain.Employees;
using App.Core.Domain.Projects;
using App.Core.Domain.TimeSheets;
using App.Core.Domain.PerformanceMeasurements;
using App.Core.Domain.Designations;

namespace App.Data.Mapping.Builders
{
    /// <summary>
    /// Represents a KPIWeightage entity builder
    /// </summary>
    public partial class KPIWeightageBuilder : NopEntityBuilder<KPIWeightage>
    {
        #region Methods
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(KPIWeightage.KPIMasterId)).AsInt32().ForeignKey<KPIMaster>(onDelete: Rule.SetNull).Nullable()
                .WithColumn(nameof(KPIWeightage.DesignationId)).AsInt32().ForeignKey<Designation>(onDelete: Rule.SetNull).Nullable();
        }
        #endregion
    }
}