using FluentMigrator;
using App.Core.Domain.Blogs;
using App.Core.Domain.Common;
using App.Core.Domain.Customers;
using App.Core.Domain.Directory;
using App.Core.Domain.Forums;
using App.Core.Domain.Gdpr;
using App.Core.Domain.Logging;
using App.Core.Domain.Messages;
using App.Core.Domain.News;
using App.Core.Domain.Polls;
using App.Core.Domain.ScheduleTasks;
using App.Data.Mapping;

namespace App.Data.Migrations.UpgradeTo460
{
    [NopMigration("2023-07-28 08:00:00", "Update datetime type precision", MigrationProcessType.Update)]
    public class MySqlDateTimeWithPrecisionMigration : Migration
    {
        public override void Up()
        {
            var dataSettings = DataSettingsManager.LoadSettings();

            //update the types only in MySql 
            if (dataSettings.DataProvider != DataProviderType.MySql)
                return;

            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ActivityLog)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(ActivityLog), nameof(ActivityLog.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(Address)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(Address), nameof(Address.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(BlogComment)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(BlogComment), nameof(BlogComment.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(BlogPost)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(BlogPost), nameof(BlogPost.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(BlogPost)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(BlogPost), nameof(BlogPost.EndDateUtc)))
                 .AsCustom("datetime(6)")
                 .Nullable();
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(BlogPost)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(BlogPost), nameof(BlogPost.StartDateUtc)))
                 .AsCustom("datetime(6)")
                 .Nullable();
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(Campaign)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(Campaign), nameof(Campaign.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(Campaign)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(Campaign), nameof(Campaign.DontSendBeforeDateUtc)))
                 .AsCustom("datetime(6)")
                 .Nullable();
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(Currency)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(Currency), nameof(Currency.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(Currency)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(Currency), nameof(Currency.UpdatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(Customer)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(Customer), nameof(Customer.CannotLoginUntilDateUtc)))
                 .AsCustom("datetime(6)")
                 .Nullable();
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(Customer)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(Customer), nameof(Customer.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(Customer)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(Customer), nameof(Customer.DateOfBirth)))
                 .AsCustom("datetime(6)")
                 .Nullable();
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(Customer)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(Customer), nameof(Customer.LastActivityDateUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(Customer)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(Customer), nameof(Customer.LastLoginDateUtc)))
                 .AsCustom("datetime(6)")
                 .Nullable();
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(CustomerPassword)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(CustomerPassword), nameof(CustomerPassword.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(Forum)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(Forum), nameof(Forum.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(Forum)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(Forum), nameof(Forum.LastPostTime)))
                 .AsCustom("datetime(6)")
                 .Nullable();
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(Forum)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(Forum), nameof(Forum.UpdatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ForumGroup)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(ForumGroup), nameof(ForumGroup.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ForumGroup)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(ForumGroup), nameof(ForumGroup.UpdatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ForumPost)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(ForumPost), nameof(ForumPost.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ForumPost)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(ForumPost), nameof(ForumPost.UpdatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ForumPostVote)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(ForumPostVote), nameof(ForumPostVote.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(PrivateMessage)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(PrivateMessage), nameof(PrivateMessage.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ForumSubscription)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(ForumSubscription), nameof(ForumSubscription.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ForumTopic)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(ForumTopic), nameof(ForumTopic.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ForumTopic)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(ForumTopic), nameof(ForumTopic.LastPostTime)))
                 .AsCustom("datetime(6)")
                 .Nullable();
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ForumTopic)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(ForumTopic), nameof(ForumTopic.UpdatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(GdprLog)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(GdprLog), nameof(GdprLog.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(GenericAttribute)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(GenericAttribute), nameof(GenericAttribute.CreatedOrUpdatedDateUTC)))
                 .AsCustom("datetime(6)")
                 .Nullable();
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(Log)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(Log), nameof(Log.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(MigrationVersionInfo)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(MigrationVersionInfo), nameof(MigrationVersionInfo.AppliedOn)))
                 .AsCustom("datetime(6)")
                 .Nullable();
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(NewsItem)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(NewsItem), nameof(NewsItem.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(NewsItem)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(NewsItem), nameof(NewsItem.EndDateUtc)))
                 .AsCustom("datetime(6)")
                 .Nullable();
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(NewsItem)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(NewsItem), nameof(NewsItem.StartDateUtc)))
                 .AsCustom("datetime(6)")
                 .Nullable();
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(NewsComment)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(NewsComment), nameof(NewsComment.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(NewsLetterSubscription)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(NewsLetterSubscription), nameof(NewsLetterSubscription.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(Poll)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(Poll), nameof(Poll.EndDateUtc)))
                 .AsCustom("datetime(6)")
                 .Nullable();
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(Poll)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(Poll), nameof(Poll.StartDateUtc)))
                 .AsCustom("datetime(6)")
                 .Nullable();
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(PollVotingRecord)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(PollVotingRecord), nameof(PollVotingRecord.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(QueuedEmail)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(QueuedEmail), nameof(QueuedEmail.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(QueuedEmail)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(QueuedEmail), nameof(QueuedEmail.DontSendBeforeDateUtc)))
                 .AsCustom("datetime(6)")
                 .Nullable();
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(QueuedEmail)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(QueuedEmail), nameof(QueuedEmail.SentOnUtc)))
                 .AsCustom("datetime(6)")
                 .Nullable();
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(RewardPointsHistory)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(RewardPointsHistory), nameof(RewardPointsHistory.CreatedOnUtc)))
                 .AsCustom("datetime(6)");
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(RewardPointsHistory)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(RewardPointsHistory), nameof(RewardPointsHistory.EndDateUtc)))
                 .AsCustom("datetime(6)")
                 .Nullable();
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ScheduleTask)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(ScheduleTask), nameof(ScheduleTask.LastEnabledUtc)))
                 .AsCustom("datetime(6)")
                 .Nullable();
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ScheduleTask)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(ScheduleTask), nameof(ScheduleTask.LastEndUtc)))
                 .AsCustom("datetime(6)")
                 .Nullable();
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ScheduleTask)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(ScheduleTask), nameof(ScheduleTask.LastStartUtc)))
                 .AsCustom("datetime(6)")
                 .Nullable();
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ScheduleTask)))
                 .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(ScheduleTask), nameof(ScheduleTask.LastSuccessUtc)))
                 .AsCustom("datetime(6)")
                 .Nullable();
        }

        public override void Down()
        {
            //add the downgrade Logic if necessary 
        }
    }
}