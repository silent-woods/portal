using App.Services.JobPostings;
using App.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Controllers
{
    public partial class CareersController : BasePublicController
    {
        #region Fields
        private readonly IJobPostingService _jobPostingService;
        #endregion

        #region Ctor

        public CareersController(IJobPostingService jobPostingService)
        {
            _jobPostingService = jobPostingService;
        }

        #endregion

        #region Utilities

        #endregion

        #region Actions
        public async Task<IActionResult> Index()
        {
            var jobsPaged = await _jobPostingService.GetAllJobPostingAsync(tittle: null, positionid: 0, pageIndex: 0, pageSize: int.MaxValue);

            var model = jobsPaged.Where(x => x.Publish).ToList();

            return View("~/Themes/DefaultClean/Views/Extension/Careers/Index.cshtml", model);
        }

        #endregion
    }
}