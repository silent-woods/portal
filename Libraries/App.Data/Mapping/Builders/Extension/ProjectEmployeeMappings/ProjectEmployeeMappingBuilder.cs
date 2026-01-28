using System.Data;
using FluentMigrator.Builders.Create.Table;
using App.Data.Extensions;
using App.Core.Domain.Employees;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectEmployeeMappings;

namespace App.Data.Mapping.Builders
{
    /// <summary>
    /// Represents a ProjectEmployeeMapping entity builder
    /// </summary>
    public partial class ProjectEmployeeMappingBuilder : NopEntityBuilder<ProjectEmployeeMapping>
    {
        #region Methods
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ProjectEmployeeMapping.ProjectId)).AsInt32().ForeignKey<Project>(onDelete: Rule.SetNull).Nullable()
                .WithColumn(nameof(ProjectEmployeeMapping.EmployeeId)).AsInt32().ForeignKey<Employee>(onDelete: Rule.SetNull).Nullable();
        }
        #endregion
    }
}