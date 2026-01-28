using FluentMigrator;
using App.Core.Domain.Affiliates;
using App.Core.Domain.Blogs;
using App.Core.Domain.Common;
using App.Core.Domain.Configuration;
using App.Core.Domain.Customers;
using App.Core.Domain.Directory;
using App.Core.Domain.Forums;
using App.Core.Domain.Gdpr;
using App.Core.Domain.Localization;
using App.Core.Domain.Logging;
using App.Core.Domain.Media;
using App.Core.Domain.Messages;
using App.Core.Domain.News;
using App.Core.Domain.Polls;
using App.Core.Domain.ScheduleTasks;
using App.Core.Domain.Security;
using App.Core.Domain.Seo;
using App.Core.Domain.Stores;
using App.Core.Domain.Topics;
using App.Data.Extensions;

namespace App.Data.Migrations.Installation
{
    [NopMigration("2020/01/31 11:24:16:2551771", "App.Data base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        /// <summary>
        /// Collect the UP migration expressions
        /// <remarks>
        /// We use an explicit table creation order instead of an automatic one
        /// due to problems creating relationships between tables
        /// </remarks>
        /// </summary>
        public override void Up()
        {
            Create.TableFor<AddressAttribute>();
            Create.TableFor<AddressAttributeValue>();
            Create.TableFor<GenericAttribute>();
            Create.TableFor<SearchTerm>();
            Create.TableFor<Country>();
            Create.TableFor<Currency>();
            Create.TableFor<MeasureDimension>();
            Create.TableFor<MeasureWeight>();
            Create.TableFor<StateProvince>();
            Create.TableFor<Address>();
            Create.TableFor<Affiliate>();
            Create.TableFor<Language>();
            Create.TableFor<CustomerAttribute>();
            Create.TableFor<CustomerAttributeValue>();
            Create.TableFor<Customer>();
            Create.TableFor<CustomerPassword>();
            Create.TableFor<CustomerAddressMapping>();
            Create.TableFor<CustomerRole>();
            Create.TableFor<CustomerCustomerRoleMapping>();
            Create.TableFor<ExternalAuthenticationRecord>();
            Create.TableFor<RewardPointsHistory>();
            Create.TableFor<Store>();
            Create.TableFor<StoreMapping>();
            Create.TableFor<LocaleStringResource>();
            Create.TableFor<LocalizedProperty>();
            Create.TableFor<BlogPost>();
            Create.TableFor<BlogComment>();
            Create.TableFor<Download>();
            Create.TableFor<Picture>();
            Create.TableFor<PictureBinary>();
            Create.TableFor<Video>();
            Create.TableFor<Setting>();
            Create.TableFor<PrivateMessage>();
            Create.TableFor<ForumGroup>();
            Create.TableFor<Forum>();
            Create.TableFor<ForumTopic>();
            Create.TableFor<ForumPost>();
            Create.TableFor<ForumPostVote>();
            Create.TableFor<ForumSubscription>();
            Create.TableFor<GdprConsent>();
            Create.TableFor<GdprLog>();
            Create.TableFor<ActivityLogType>();
            Create.TableFor<ActivityLog>();
            Create.TableFor<Log>();
            Create.TableFor<Campaign>();
            Create.TableFor<EmailAccount>();
            Create.TableFor<MessageTemplate>();
            Create.TableFor<NewsLetterSubscription>();
            Create.TableFor<QueuedEmail>();
            Create.TableFor<NewsItem>();
            Create.TableFor<NewsComment>();
            Create.TableFor<Poll>();
            Create.TableFor<PollAnswer>();
            Create.TableFor<PollVotingRecord>();
            Create.TableFor<AclRecord>();
            Create.TableFor<PermissionRecord>();
            Create.TableFor<PermissionRecordCustomerRoleMapping>();
            Create.TableFor<UrlRecord>();
            Create.TableFor<ScheduleTask>();
            Create.TableFor<TopicTemplate>();
            Create.TableFor<Topic>();
        }
    }
}
