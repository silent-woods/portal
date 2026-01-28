using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.WeeklyQuestions
{
    /// <summary>
    /// Represents a WeeklyQuestions list model
    /// </summary>
    public partial record WeeklyQuestionsListModel : BasePagedListModel<WeeklyQuestionsModel>
    {
    }
}