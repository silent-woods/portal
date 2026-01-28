using App.Data;
using App.Services.ScheduleTasks;
using Satyanam.Nop.Core.Domains;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{

    public partial class GenerateUpdateTemplatePeriodsTask : IScheduleTask
    {
        #region Fields
        private readonly IRepository<UpdateTemplate> _templateRepository;
        private readonly IRepository<UpdateTemplatePeriod> _periodRepository;

        #endregion

        #region Ctor

        public GenerateUpdateTemplatePeriodsTask(IRepository<UpdateTemplate> templateRepository, IRepository<UpdateTemplatePeriod> periodRepository)
        {
            _templateRepository = templateRepository;
            _periodRepository = periodRepository;
        }

        #endregion

        #region Methods

        #region UpdateSubmissionService
        //public async Task ExecuteAsync()
        //{
        //    var templates = await _templateRepository.GetAllAsync(q => q);

        //    foreach (var template in templates)
        //    {
        //        // Get the latest period for this template
        //        var periods = await _periodRepository.GetAllAsync(query =>
        //            query.Where(x => x.UpdateTemplateId == template.Id)
        //                 .OrderByDescending(x => x.PeriodEnd)
        //                 .Take(1)
        //        );

        //        // If no period, start from template created date
        //        var lastPeriodEnd = periods.FirstOrDefault()?.PeriodEnd.Date ?? template.CreatedOnUTC.Date;

        //        var now = DateTime.UtcNow.Date;
        //        var repeatEvery = template.RepeatEvery > 0 ? template.RepeatEvery : 1;

        //        // ✅ FIX: Use <= to ensure today’s period is included
        //        while (lastPeriodEnd <= now)
        //        {
        //            var nextStart = lastPeriodEnd;
        //            DateTime nextEnd;

        //            switch (template.FrequencyId)
        //            {
        //                case 1: // OneTime (treat like Daily)
        //                    nextEnd = nextStart.AddDays(1); break;
        //                case 2: // Daily
        //                    nextEnd = nextStart.AddDays(repeatEvery); break;
        //                case 3: // Weekly
        //                    nextEnd = nextStart.AddDays(7 * repeatEvery); break;
        //                case 4: // Monthly
        //                    nextEnd = nextStart.AddMonths(repeatEvery); break;
        //                default:
        //                    nextEnd = nextStart.AddDays(1); break;
        //            }

        //            // ✅ Ensure period is valid (safety)
        //            if (nextEnd <= nextStart)
        //                nextEnd = nextStart.AddDays(1);

        //            var newPeriod = new UpdateTemplatePeriod
        //            {
        //                UpdateTemplateId = template.Id,
        //                PeriodStart = nextStart,
        //                PeriodEnd = nextEnd,
        //                CreatedOnUtc = DateTime.UtcNow
        //            };

        //            await _periodRepository.InsertAsync(newPeriod);

        //            // Prepare for next loop
        //            lastPeriodEnd = nextEnd;
        //        }
        //    }
        //}

        public async Task ExecuteAsync()
        {
            try
            {
                // Use this if you have IsActive property, otherwise remove the Where clause
                var templates = await _templateRepository.GetAllAsync(q =>
                    q.Where(t => t.IsActive == true)); // Remove this Where if no IsActive property

                foreach (var template in templates)
                {
                    try
                    {
                        await GeneratePeriodsForTemplate(template);
                    }
                    catch (Exception ex)
                    {
                        // Log error for individual template but continue with others
                        // You can add logging here if needed
                        // _logger.Error($"Error generating periods for template {template.Id}: {ex.Message}", ex);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log main error
                // _logger.Error($"Error in GenerateUpdateTemplatePeriodsTask: {ex.Message}", ex);
                throw; // Re-throw so NopCommerce knows the task failed
            }
        }

        private async Task GeneratePeriodsForTemplate(UpdateTemplate template)
        {
            if (template == null) return;

            var currentDate = DateTime.UtcNow.Date;
            var repeatEvery = template.RepeatEvery > 0 ? template.RepeatEvery : 1;

            // Get the latest period for this template
            var lastPeriods = await _periodRepository.GetAllAsync(query =>
                query.Where(x => x.UpdateTemplateId == template.Id)
                     .OrderByDescending(x => x.PeriodStart)
                     .Take(1)
            );

            DateTime nextPeriodStart;

            if (lastPeriods.Any())
            {
                nextPeriodStart = lastPeriods.First().PeriodEnd.Date;
            }
            else
            {
                nextPeriodStart = template.CreatedOnUTC.Date;
            }

            // Safety check - don't go too far back
            var minStartDate = currentDate.AddDays(-90); // Don't generate periods older than 90 days
            if (nextPeriodStart < minStartDate)
            {
                nextPeriodStart = minStartDate;
            }

            // Generate periods up to current date
            var maxIterations = 100; // Safety limit to prevent infinite loops
            var iterations = 0;

            while (nextPeriodStart <= currentDate && iterations < maxIterations)
            {
                iterations++;

                var nextPeriodEnd = CalculatePeriodEnd(nextPeriodStart, template.FrequencyId, repeatEvery);

                // Safety check for period end
                if (nextPeriodEnd <= nextPeriodStart)
                {
                    nextPeriodEnd = nextPeriodStart.AddDays(1);
                }

                // Check if this period already exists
                var existingPeriods = await _periodRepository.GetAllAsync(query =>
                    query.Where(x => x.UpdateTemplateId == template.Id &&
                               x.PeriodStart == nextPeriodStart &&
                               x.PeriodEnd == nextPeriodEnd)
                );

                if (!existingPeriods.Any())
                {
                    var newPeriod = new UpdateTemplatePeriod
                    {
                        UpdateTemplateId = template.Id,
                        PeriodStart = nextPeriodStart,
                        PeriodEnd = nextPeriodEnd,
                        CreatedOnUtc = DateTime.UtcNow
                    };

                    await _periodRepository.InsertAsync(newPeriod);
                }

                nextPeriodStart = nextPeriodEnd;
            }
        }

        private DateTime CalculatePeriodEnd(DateTime startDate, int frequencyId, int repeatEvery)
        {
            try
            {
                switch (frequencyId)
                {
                    case (int)UpdatedFrequency.OneTime: // 1
                        return startDate.AddDays(1);

                    case (int)UpdatedFrequency.Daily: // 2
                        return startDate.AddDays(repeatEvery);

                    case (int)UpdatedFrequency.Weekly: // 3
                        return startDate.AddDays(7 * repeatEvery);

                    case (int)UpdatedFrequency.Monthly: // 4
                        return startDate.AddMonths(repeatEvery);

                    default:
                        return startDate.AddDays(1);
                }
            }
            catch
            {
                // Fallback to daily if any calculation fails
                return startDate.AddDays(1);
            }
        }

        #endregion

        #endregion
    }
}