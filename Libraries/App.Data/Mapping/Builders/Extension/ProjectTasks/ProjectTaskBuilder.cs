using App.Core.Domain.Employees;
using FluentMigrator.Builders.Create.Table;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.Projects;
using App.Data.Extensions;


namespace App.Data.Mapping.Builders.Extension.ProjectTasks
{
    public partial class ProjectTaskBuilder : NopEntityBuilder<ProjectTask>
    {
        #region Methods
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ProjectTask.ProjectId)).AsInt32().ForeignKey<Project>(onDelete: Rule.SetNull).Nullable();
        }
        #endregion
    }
}
