using App.Core.Domain.TaskAlerts;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Builders.TaskAlerts;

public partial class TaskAlertReasonBuilder : NopEntityBuilder<TaskAlertReason>
{
    #region Methods

    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table.WithColumn(nameof(TaskAlertReason.Name)).AsString(int.MaxValue).NotNullable();
    }

    #endregion
}
