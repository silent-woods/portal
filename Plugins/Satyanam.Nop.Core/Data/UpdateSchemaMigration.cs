using App.Data.Extensions;
using App.Data.Mapping;
using App.Data.Migrations;
using FluentMigrator;
using Satyanam.Nop.Core.Domains;

namespace Satyanam.Nop.Core.Data
{
    [NopMigration("2026/01/05 12:30:00", "Misc.SatyanamNopCore schema", MigrationProcessType.Update)]
    public class UpdateSchemaMigration : AutoReversingMigration
    {
        #region Methods

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Title))).Exists())
            {
                Create.TableFor<Title>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Tags))).Exists())
            {
                Create.TableFor<Tags>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(LeadStatus))).Exists())
            {
                Create.TableFor<LeadStatus>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(LeadSource))).Exists())
            {
                Create.TableFor<LeadSource>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Industry))).Exists())
            {
                Create.TableFor<Industry>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Categorys))).Exists())
            {
                Create.TableFor<Categorys>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Lead))).Exists())
            {
                Create.TableFor<Lead>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Lead))).Exists())
            {
                Create.TableFor<Lead>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(LeadTags))).Exists())
            {
                Create.TableFor<LeadTags>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Campaings))).Exists())
            {
                Create.TableFor<Campaings>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Contacts))).Exists())
            {
                Create.TableFor<Contacts>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ContactsTags))).Exists())
            {
                Create.TableFor<ContactsTags>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(AccountType))).Exists())
            {
                Create.TableFor<AccountType>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Company))).Exists())
            {
                Create.TableFor<Company>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Deals))).Exists())
            {
                Create.TableFor<Deals>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(LeadAPILog))).Exists())
            {
                Create.TableFor<LeadAPILog>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(CampaingsEmailLogs))).Exists())
            {
                Create.TableFor<CampaingsEmailLogs>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(UpdateTemplate))).Exists())
            {
                Create.TableFor<UpdateTemplate>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(UpdateTemplate))).Column(nameof(UpdateTemplate.DueTime)).Exists())
            {
                Alter.Table(NameCompatibilityManager.GetTableName(typeof(UpdateTemplate)))
                    .AddColumn(nameof(UpdateTemplate.DueTime)).AsTime().Nullable();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(UpdateTemplate))).Column(nameof(UpdateTemplate.LastReminderSentUtc)).Exists())
            {
                Alter.Table(NameCompatibilityManager.GetTableName(typeof(UpdateTemplate)))
                    .AddColumn(nameof(UpdateTemplate.LastReminderSentUtc)).AsDateTime().Nullable();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(UpdateTemplateQuestion))).Exists())
            {
                Create.TableFor<UpdateTemplateQuestion>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(UpdateQuestionOption))).Exists())
            {
                Create.TableFor<UpdateQuestionOption>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(UpdateQuestionOption))).Column(nameof(UpdateQuestionOption.IsRequired)).Exists())
            {
                Alter.Table(NameCompatibilityManager.GetTableName(typeof(UpdateQuestionOption)))
                    .AddColumn(nameof(UpdateQuestionOption.IsRequired)).AsBoolean().WithDefaultValue(false);
            }
            // Create if not exists
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(UpdateSubmission))).Exists())
            {
                Create.TableFor<UpdateSubmission>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(UpdateSubmissionAnswer))).Exists())
            {
                Create.TableFor<UpdateSubmissionAnswer>();
            }
            else if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(UpdateSubmissionAnswer))).Column("FilePath").Exists())
            {
                Alter.Table(NameCompatibilityManager.GetTableName(typeof(UpdateSubmissionAnswer)))
                    .AddColumn("FilePath").AsString(500).Nullable();
            }

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Lead))).Column(nameof(Lead.IsSyncedToReply)).Exists())
            {
                Alter.Table(NameCompatibilityManager.GetTableName(typeof(Lead)))
                    .AddColumn(nameof(Lead.IsSyncedToReply)).AsBoolean().WithDefaultValue(false);
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(UpdateTemplatePeriod))).Exists())
            {
                Create.TableFor<UpdateTemplatePeriod>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(UpdateSubmissionReviewer))).Exists())
            {
                Create.TableFor<UpdateSubmissionReviewer>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(UpdateSubmissionComment))).Exists())
            {
                Create.TableFor<UpdateSubmissionComment>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(LinkedInFollowups))).Exists())
            {
                Create.TableFor<LinkedInFollowups>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ConnectionRequest))).Exists())
            {
                Create.TableFor<ConnectionRequest>();
            }


            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(TaskChangeLog))).Exists())
            {
                Create.TableFor<TaskChangeLog>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(TaskComments))).Exists())
            {
                Create.TableFor<TaskComments>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ProcessWorkflow))).Exists())
            {
                Create.TableFor<ProcessWorkflow>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(WorkflowStatus))).Exists())
            {
                Create.TableFor<WorkflowStatus>();
            }

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ProcessRules))).Exists())
            {
                Create.TableFor<ProcessRules>();
            }

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Announcement))).Exists())
            {
                Create.TableFor<Announcement>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(TaskCategory))).Exists())
            {
                Create.TableFor<TaskCategory>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ProjectTaskCategoryMapping))).Exists())
            {
                Create.TableFor<ProjectTaskCategoryMapping>();
            }

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(CheckListMaster))).Exists())
            {
                Create.TableFor<CheckListMaster>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(CheckListMapping))).Exists())
            {
                Create.TableFor<CheckListMapping>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(TaskCheckListEntry))).Exists())
            {
                Create.TableFor<TaskCheckListEntry>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Inquiry))).Exists())
            {
                Create.TableFor<Inquiry>();
            }
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(FollowUpTask))).Exists())
            {
                Create.TableFor<FollowUpTask>();
            }
        }

        #endregion
    }
}