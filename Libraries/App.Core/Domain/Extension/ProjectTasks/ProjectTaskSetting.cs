using App.Core.Configuration;

namespace App.Core.Domain.Extension.ProjectTasks
{
    public partial class ProjectTaskSetting: ISettings
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        public bool IsShowSelctAllCheckList { get; set; }
    }
}
