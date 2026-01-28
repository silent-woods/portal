using App.Core.Domain.TaskAlerts;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Builders.TaskAlerts;

public partial class TaskAlertConfigurationBuilder : NopEntityBuilder<TaskAlertConfiguration>
{
    #region Methods

    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table.WithColumn(nameof(TaskAlertConfiguration.Message)).AsString(int.MaxValue).NotNullable();
    }

    #endregion
}
