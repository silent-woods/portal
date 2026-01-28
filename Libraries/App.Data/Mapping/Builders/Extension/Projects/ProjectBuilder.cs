using System.Data;
using FluentMigrator.Builders.Create.Table;
using App.Data.Extensions;
using App.Core.Domain.Employees;
using App.Core.Domain.Projects;
using App.Core.Domain.Leaves;

namespace App.Data.Mapping.Builders
{
    /// <summary>
    /// Represents a Projects entity builder
    /// </summary>
    public partial class ProjectBuilder : NopEntityBuilder<Project>
    {
        #region Methods
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table 
            .WithColumn(nameof(Project.CreateOnUtc)).AsDateTime2().Nullable();
        }
        #endregion
    }
}