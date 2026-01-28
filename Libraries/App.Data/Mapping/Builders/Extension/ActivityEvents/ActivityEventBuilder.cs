using System.Data;
using FluentMigrator.Builders.Create.Table;
using App.Data.Extensions;
using App.Core.Domain.Employees;
using App.Core.Domain.Projects;
using App.Core.Domain.TimeSheets;
using App.Core.Domain.Activities;
using App.Core.Domain.ActivityEvents;

namespace App.Data.Mapping.Builders
{
    /// <summary>
    /// Represents a TimeSheet entity builder
    /// </summary>
    public partial class ActivityEventBuilder : NopEntityBuilder<ActivityEvent>
    {
        #region Methods
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
 .WithColumn(nameof(Activity.CreateOnUtc)).AsDateTime2().Nullable();
        }
        #endregion
    }
}