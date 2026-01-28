using System.Threading.Tasks;
using App.Services.ScheduleTasks;

namespace App.Tests.App.Services.Tests.ScheduleTasks
{
    public class TestScheduleTask : IScheduleTask
    {        
        public TestScheduleTask()
        {
            IsInit = true;
        }

        public Task ExecuteAsync()
        {
            throw new System.NotImplementedException();
        }

        public static bool IsInit { get; protected set; }

        public static void ResetInitFlag()
        {
            IsInit = false;
        }
    }
}
