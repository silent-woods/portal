using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Core;
using App.Core.Domain.Polls;
using App.Services.Customers;
using App.Services.Localization;
using App.Services.Polls;
using App.Services.Stores;
using App.Web.Factories;

namespace App.Web.Controllers
{
    [AutoValidateAntiforgeryToken]
    public partial class PollController : BasePublicController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IPollModelFactory _pollModelFactory;
        private readonly IPollService _pollService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public PollController(ICustomerService customerService, 
            ILocalizationService localizationService, 
            IPollModelFactory pollModelFactory,
            IPollService pollService,
            IStoreMappingService storeMappingService,
            IWorkContext workContext)
        {
            _customerService = customerService;
            _localizationService = localizationService;
            _pollModelFactory = pollModelFactory;
            _pollService = pollService;
            _storeMappingService = storeMappingService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        [HttpPost]
        public virtual async Task<IActionResult> Vote(int pollAnswerId)
        {
            var pollAnswer = await _pollService.GetPollAnswerByIdAsync(pollAnswerId);
            if (pollAnswer == null)
                return Json(new { error = "No poll answer found with the specified id" });

            var poll = await _pollService.GetPollByIdAsync(pollAnswer.PollId);

            if (!poll.Published || !await _storeMappingService.AuthorizeAsync(poll))
                return Json(new { error = "Poll is not available" });

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (await _customerService.IsGuestAsync(customer) && !poll.AllowGuestsToVote)
                return Json(new { error = await _localizationService.GetResourceAsync("Polls.OnlyRegisteredUsersVote") });

            var alreadyVoted = await _pollService.AlreadyVotedAsync(poll.Id, customer.Id);
            if (!alreadyVoted)
            {
                //vote
                await _pollService.InsertPollVotingRecordAsync(new PollVotingRecord
                {
                    PollAnswerId = pollAnswer.Id,
                    CustomerId = customer.Id,
                    CreatedOnUtc = DateTime.UtcNow
                });

                //update totals
                pollAnswer.NumberOfVotes = (await _pollService.GetPollVotingRecordsByPollAnswerAsync(pollAnswer.Id)).Count;
                await _pollService.UpdatePollAnswerAsync(pollAnswer);
                await _pollService.UpdatePollAsync(poll);
            }

            return Json(new
            {
                html = await RenderPartialViewToStringAsync("_Poll", await _pollModelFactory.PreparePollModelAsync(poll, true)),
            });
        }

        #endregion
    }
}