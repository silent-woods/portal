using FluentMigrator;
using FluentMigrator.SqlServer;
using App.Core.Domain.Common;
using App.Core.Domain.Customers;
using App.Core.Domain.Directory;
using App.Core.Domain.Forums;
using App.Core.Domain.Localization;
using App.Core.Domain.Logging;
using App.Core.Domain.Messages;
using App.Core.Domain.Security;
using App.Core.Domain.Seo;
using App.Core.Domain.Stores;
using App.Data.Mapping;

namespace App.Data.Migrations.Installation
{
    [NopMigration("2020/03/13 09:36:08:9037677", "App.Data base indexes", MigrationProcessType.Installation)]
    public class Indexes : AutoReversingMigration
    {
        #region Methods

        public override void Up()
        {
            Create.Index("IX_UrlRecord_Slug")
                .OnTable(nameof(UrlRecord))
                .OnColumn(nameof(UrlRecord.Slug))
                .Ascending()
                .WithOptions()
                .NonClustered();

            Create.Index("IX_UrlRecord_Custom_1").OnTable(nameof(UrlRecord))
                .OnColumn(nameof(UrlRecord.EntityId)).Ascending()
                .OnColumn(nameof(UrlRecord.EntityName)).Ascending()
                .OnColumn(nameof(UrlRecord.LanguageId)).Ascending()
                .OnColumn(nameof(UrlRecord.IsActive)).Ascending()
                .WithOptions().NonClustered();

            Create.Index("IX_StoreMapping_EntityId_EntityName").OnTable(nameof(StoreMapping))
                .OnColumn(nameof(StoreMapping.EntityId)).Ascending()
                .OnColumn(nameof(StoreMapping.EntityName)).Ascending()
                .WithOptions().NonClustered();

            Create.Index("IX_QueuedEmail_SentOnUtc_DontSendBeforeDateUtc_Extended").OnTable(nameof(QueuedEmail))
                .OnColumn(nameof(QueuedEmail.SentOnUtc)).Ascending()
                .OnColumn(nameof(QueuedEmail.DontSendBeforeDateUtc)).Ascending()
                .WithOptions().NonClustered()
                .Include(nameof(QueuedEmail.SentTries));

            Create.Index("IX_QueuedEmail_CreatedOnUtc").OnTable(nameof(QueuedEmail))
                .OnColumn(nameof(QueuedEmail.CreatedOnUtc)).Descending()
                .WithOptions().NonClustered();

            Create.Index("IX_NewsletterSubscription_Email_StoreId").OnTable(nameof(NewsLetterSubscription))
                .OnColumn(nameof(NewsLetterSubscription.Email)).Ascending()
                .OnColumn(nameof(NewsLetterSubscription.StoreId)).Ascending()
                .WithOptions().NonClustered();

            Create.Index("IX_Log_CreatedOnUtc").OnTable(nameof(Log))
                .OnColumn(nameof(Log.CreatedOnUtc)).Descending()
                .WithOptions().NonClustered();

            Create.Index("IX_LocaleStringResource").OnTable(nameof(LocaleStringResource))
                .OnColumn(nameof(LocaleStringResource.ResourceName)).Ascending()
                .OnColumn(nameof(LocaleStringResource.LanguageId)).Ascending()
                .WithOptions().NonClustered();

            Create.Index("IX_Language_DisplayOrder").OnTable(nameof(Language))
                .OnColumn(nameof(Language.DisplayOrder)).Ascending()
                .WithOptions().NonClustered();

            Create.Index("IX_GenericAttribute_EntityId_and_KeyGroup").OnTable(nameof(GenericAttribute))
                .OnColumn(nameof(GenericAttribute.EntityId)).Ascending()
                .OnColumn(nameof(GenericAttribute.KeyGroup)).Ascending()
                .WithOptions().NonClustered();

            Create.Index("IX_Forums_Subscription_TopicId").OnTable(NameCompatibilityManager.GetTableName(typeof(ForumSubscription)))
                .OnColumn(nameof(ForumSubscription.TopicId)).Ascending()
                .WithOptions().NonClustered();

            Create.Index("IX_Forums_Subscription_ForumId").OnTable(NameCompatibilityManager.GetTableName(typeof(ForumSubscription)))
                .OnColumn(nameof(ForumSubscription.ForumId)).Ascending()
                .WithOptions().NonClustered();

            Create.Index("IX_Forums_Group_DisplayOrder").OnTable(NameCompatibilityManager.GetTableName(typeof(ForumGroup)))
                .OnColumn(nameof(ForumGroup.DisplayOrder)).Ascending()
                .WithOptions().NonClustered();

            Create.Index("IX_Forums_Forum_DisplayOrder").OnTable(NameCompatibilityManager.GetTableName(typeof(Forum)))
                .OnColumn(nameof(Forum.DisplayOrder)).Ascending()
                .WithOptions().NonClustered();

            Create.Index("IX_Customer_Username").OnTable(nameof(Customer))
                .OnColumn(nameof(Customer.Username)).Ascending()
                .WithOptions().NonClustered();

            Create.Index("IX_Customer_SystemName").OnTable(nameof(Customer))
                .OnColumn(nameof(Customer.SystemName)).Ascending()
                .WithOptions().NonClustered();

            Create.Index("IX_Customer_Email").OnTable(nameof(Customer))
                .OnColumn(nameof(Customer.Email)).Ascending()
                .WithOptions().NonClustered();

            Create.Index("IX_Customer_CustomerGuid").OnTable(nameof(Customer))
                .OnColumn(nameof(Customer.CustomerGuid)).Ascending()
                .WithOptions().NonClustered();

            Create.Index("IX_Customer_CreatedOnUtc").OnTable(nameof(Customer))
                .OnColumn(nameof(Customer.CreatedOnUtc)).Descending()
                .WithOptions().NonClustered();

            Create.Index("IX_Currency_DisplayOrder").OnTable(nameof(Currency))
                .OnColumn(nameof(Currency.DisplayOrder)).Ascending()
                .WithOptions().NonClustered();

            Create.Index("IX_Country_DisplayOrder").OnTable(nameof(Country))
                .OnColumn(nameof(Country.DisplayOrder)).Ascending()
                .WithOptions().NonClustered();

            Create.Index("IX_ActivityLog_CreatedOnUtc").OnTable(nameof(ActivityLog))
                .OnColumn(nameof(ActivityLog.CreatedOnUtc)).Descending()
                .WithOptions().NonClustered();

            Create.Index("IX_AclRecord_EntityId_EntityName").OnTable(nameof(AclRecord))
                .OnColumn(nameof(AclRecord.EntityId)).Ascending()
                .OnColumn(nameof(AclRecord.EntityName)).Ascending()
                .WithOptions().NonClustered();
        }

        #endregion
    }
}
