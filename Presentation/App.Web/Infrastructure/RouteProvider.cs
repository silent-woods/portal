using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using App.Services.Installation;
using App.Web.Framework.Mvc.Routing;

namespace App.Web.Infrastructure
{
    /// <summary>
    /// Represents provider that provided basic routes
    /// </summary>
    public partial class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        #region Methods

        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            //get language pattern
            //it's not needed to use language pattern in AJAX requests and for actions returning the result directly (e.g. file to download),
            //use it only for URLs of pages that the user can go to
            var lang = GetLanguageRoutePattern();

            //areas
            endpointRouteBuilder.MapControllerRoute(name: "areaRoute",
                pattern: $"{{area:exists}}/{{controller=Home}}/{{action=Index}}/{{id?}}");

            //download tracker
            endpointRouteBuilder.MapControllerRoute(name: "DownloadTracker",
                pattern: $"{lang}/download-tracker",
                defaults: new { controller = "Tracker", action = "DownloadTracker" });

            //home page
            endpointRouteBuilder.MapControllerRoute(name: "Homepage",
                pattern: $"{lang}",
                defaults: new { controller = "Employee", action = "Info" });

            //login
            endpointRouteBuilder.MapControllerRoute(name: "Login",
                pattern: $"{lang}/login/",
                defaults: new { controller = "Customer", action = "Login" });

            // multi-factor verification digit code page
            endpointRouteBuilder.MapControllerRoute(name: "MultiFactorVerification",
                pattern: $"{lang}/multi-factor-verification/",
                defaults: new { controller = "Customer", action = "MultiFactorVerification" });

            //register
            endpointRouteBuilder.MapControllerRoute(name: "Register",
                pattern: $"{lang}/register/",
                defaults: new { controller = "Customer", action = "Register" });

            //logout
            endpointRouteBuilder.MapControllerRoute(name: "Logout",
                pattern: $"{lang}/logout/",
                defaults: new { controller = "Customer", action = "Logout" });

            //customer account links
            endpointRouteBuilder.MapControllerRoute(name: "CustomerInfo",
                pattern: $"{lang}/customer/info",
                defaults: new { controller = "Customer", action = "Info" });

             //employee info
            endpointRouteBuilder.MapControllerRoute(name: "EmployeeInfo",
                pattern: $"{lang}/employee/info",
                defaults: new { controller = "Employee", action = "Info" });
            
            //employee addresses
            endpointRouteBuilder.MapControllerRoute(name: "EmployeeAddresses",
                pattern: $"{lang}/employee/addresses",
                defaults: new { controller = "Employee", action = "Addresses" });
            
            //employee educations
            endpointRouteBuilder.MapControllerRoute(name: "EmployeeEducations",
                pattern: $"{lang}/employee/educations",
                defaults: new { controller = "Employee", action = "Education" });

            //employee experiences
            endpointRouteBuilder.MapControllerRoute(name: "EmployeeExperiences",
                pattern: $"{lang}/employee/experiences",
                defaults: new { controller = "Employee", action = "Experience" });
            
            //employee assets
            endpointRouteBuilder.MapControllerRoute(name: "EmployeeAssets",
                pattern: $"{lang}/employee/assets",
                defaults: new { controller = "Employee", action = "Asset" });

            //monthly review perfomance report
            endpointRouteBuilder.MapControllerRoute(name: "MonthlyReview",
                pattern: $"{lang}/employee/monthlyreview",
                defaults: new { controller = "Performance", action = "MonthlyReview" });

            //yearly review perfomance report
            endpointRouteBuilder.MapControllerRoute(name: "YearlyReview",
                pattern: $"{lang}/employee/yearlyreview",
                defaults: new { controller = "Performance", action = "YearlyReview" });

            endpointRouteBuilder.MapControllerRoute(name: "CustomerAddresses",
                pattern: $"{lang}/customer/addresses",
                defaults: new { controller = "Customer", action = "Addresses" });

            //contact us
            endpointRouteBuilder.MapControllerRoute(name: "ContactUs",
                pattern: $"{lang}/contactus",
                defaults: new { controller = "Common", action = "ContactUs" });

            //change currency
            endpointRouteBuilder.MapControllerRoute(name: "ChangeCurrency",
                pattern: $"{lang}/changecurrency/{{customercurrency:min(0)}}",
                defaults: new { controller = "Common", action = "SetCurrency" });

            //change language
            endpointRouteBuilder.MapControllerRoute(name: "ChangeLanguage",
                pattern: $"{lang}/changelanguage/{{langid:min(0)}}",
                defaults: new { controller = "Common", action = "SetLanguage" });

            //blog
            endpointRouteBuilder.MapControllerRoute(name: "Blog",
                pattern: $"{lang}/blog",
                defaults: new { controller = "Blog", action = "List" });

            //news
            endpointRouteBuilder.MapControllerRoute(name: "NewsArchive",
                pattern: $"{lang}/news",
                defaults: new { controller = "News", action = "List" });

            //forum
            endpointRouteBuilder.MapControllerRoute(name: "Boards",
                pattern: $"{lang}/boards",
                defaults: new { controller = "Boards", action = "Index" });

            //downloads (file result)
            endpointRouteBuilder.MapControllerRoute(name: "GetSampleDownload",
                pattern: $"download/sample/{{productid:min(0)}}",
                defaults: new { controller = "Download", action = "Sample" });

            //subscribe newsletters (AJAX)
            endpointRouteBuilder.MapControllerRoute(name: "SubscribeNewsletter",
                pattern: $"subscribenewsletter",
                defaults: new { controller = "Newsletter", action = "SubscribeNewsletter" });

            //register result page
            endpointRouteBuilder.MapControllerRoute(name: "RegisterResult",
                pattern: $"{lang}/registerresult/{{resultId:min(0)}}",
                defaults: new { controller = "Customer", action = "RegisterResult" });

            //check username availability (AJAX)
            endpointRouteBuilder.MapControllerRoute(name: "CheckUsernameAvailability",
                pattern: $"customer/checkusernameavailability",
                defaults: new { controller = "Customer", action = "CheckUsernameAvailability" });

            //passwordrecovery
            endpointRouteBuilder.MapControllerRoute(name: "PasswordRecovery",
                pattern: $"{lang}/passwordrecovery",
                defaults: new { controller = "Customer", action = "PasswordRecovery" });

            //password recovery confirmation
            endpointRouteBuilder.MapControllerRoute(name: "PasswordRecoveryConfirm",
                pattern: $"{lang}/passwordrecovery/confirm",
                defaults: new { controller = "Customer", action = "PasswordRecoveryConfirm" });

            //topics (AJAX)
            endpointRouteBuilder.MapControllerRoute(name: "TopicPopup",
                pattern: $"t-popup/{{SystemName}}",
                defaults: new { controller = "Topic", action = "TopicDetailsPopup" });

            //blog
            endpointRouteBuilder.MapControllerRoute(name: "BlogByTag",
                pattern: $"{lang}/blog/tag/{{tag}}",
                defaults: new { controller = "Blog", action = "BlogByTag" });

            endpointRouteBuilder.MapControllerRoute(name: "BlogByMonth",
                pattern: $"{lang}/blog/month/{{month}}",
                defaults: new { controller = "Blog", action = "BlogByMonth" });

            //blog RSS (file result)
            endpointRouteBuilder.MapControllerRoute(name: "BlogRSS",
                pattern: $"blog/rss/{{languageId:min(0)}}",
                defaults: new { controller = "Blog", action = "ListRss" });

            //news RSS (file result)
            endpointRouteBuilder.MapControllerRoute(name: "NewsRSS",
                pattern: $"news/rss/{{languageId:min(0)}}",
                defaults: new { controller = "News", action = "ListRss" });

            endpointRouteBuilder.MapControllerRoute(name: "CustomerChangePassword",
                pattern: $"{lang}/customer/changepassword",
                defaults: new { controller = "Customer", action = "ChangePassword" });

            endpointRouteBuilder.MapControllerRoute(name: "CustomerAvatar",
                pattern: $"{lang}/customer/avatar",
                defaults: new { controller = "Customer", action = "Avatar" });

            endpointRouteBuilder.MapControllerRoute(name: "AccountActivation",
                pattern: $"{lang}/customer/activation",
                defaults: new { controller = "Customer", action = "AccountActivation" });

            endpointRouteBuilder.MapControllerRoute(name: "EmailRevalidation",
                pattern: $"{lang}/customer/revalidateemail",
                defaults: new { controller = "Customer", action = "EmailRevalidation" });

            endpointRouteBuilder.MapControllerRoute(name: "CustomerForumSubscriptions",
                pattern: $"{lang}/boards/forumsubscriptions/{{pageNumber:int?}}",
                defaults: new { controller = "Boards", action = "CustomerForumSubscriptions" });

            endpointRouteBuilder.MapControllerRoute(name: "CustomerAddressEdit",
                pattern: $"{lang}/customer/addressedit/{{addressId:min(0)}}",
                defaults: new { controller = "Customer", action = "AddressEdit" });

            endpointRouteBuilder.MapControllerRoute(name: "CustomerAddressAdd",
                pattern: $"{lang}/customer/addressadd",
                defaults: new { controller = "Customer", action = "AddressAdd" });

            endpointRouteBuilder.MapControllerRoute(name: "CustomerMultiFactorAuthenticationProviderConfig",
                pattern: $"{lang}/customer/providerconfig",
                defaults: new { controller = "Customer", action = "ConfigureMultiFactorAuthenticationProvider" });

            //customer profile page
            endpointRouteBuilder.MapControllerRoute(name: "CustomerProfile",
                pattern: $"{lang}/profile/{{id:min(0)}}",
                defaults: new { controller = "Profile", action = "Index" });

            endpointRouteBuilder.MapControllerRoute(name: "CustomerProfilePaged",
                pattern: $"{lang}/profile/{{id:min(0)}}/page/{{pageNumber:min(0)}}",
                defaults: new { controller = "Profile", action = "Index" });

            //order downloads (file result)
            endpointRouteBuilder.MapControllerRoute(name: "GetDownload",
                pattern: $"download/getdownload/{{orderItemId:guid}}/{{agree?}}",
                defaults: new { controller = "Download", action = "GetDownload" });

            endpointRouteBuilder.MapControllerRoute(name: "GetLicense",
                pattern: $"download/getlicense/{{orderItemId:guid}}/",
                defaults: new { controller = "Download", action = "GetLicense" });

            endpointRouteBuilder.MapControllerRoute(name: "DownloadUserAgreement",
                pattern: $"customer/useragreement/{{orderItemId:guid}}",
                defaults: new { controller = "Customer", action = "UserAgreement" });

            //customer GDPR
            endpointRouteBuilder.MapControllerRoute(name: "GdprTools",
                pattern: $"{lang}/customer/gdpr",
                defaults: new { controller = "Customer", action = "GdprTools" });

            //customer multi-factor authentication settings 
            endpointRouteBuilder.MapControllerRoute(name: "MultiFactorAuthenticationSettings",
                pattern: $"{lang}/customer/multifactorauthentication",
                defaults: new { controller = "Customer", action = "MultiFactorAuthentication" });

            //poll vote (AJAX)
            endpointRouteBuilder.MapControllerRoute(name: "PollVote",
                pattern: $"poll/vote",
                defaults: new { controller = "Poll", action = "Vote" });

            //new RSS (file result)
            endpointRouteBuilder.MapControllerRoute(name: "NewProductsRSS",
                pattern: $"newproducts/rss",
                defaults: new { controller = "Catalog", action = "NewProductsRss" });

            //get state list by country ID (AJAX)
            endpointRouteBuilder.MapControllerRoute(name: "GetStatesByCountryId",
                pattern: $"country/getstatesbycountryid/",
                defaults: new { controller = "Country", action = "GetStatesByCountryId" });

            //EU Cookie law accept button handler (AJAX)
            endpointRouteBuilder.MapControllerRoute(name: "EuCookieLawAccept",
                pattern: $"eucookielawaccept",
                defaults: new { controller = "Common", action = "EuCookieLawAccept" });

            //authenticate topic (AJAX)
            endpointRouteBuilder.MapControllerRoute(name: "TopicAuthenticate",
                pattern: $"topic/authenticate",
                defaults: new { controller = "Topic", action = "Authenticate" });

            //forums
            endpointRouteBuilder.MapControllerRoute(name: "ActiveDiscussions",
                pattern: $"{lang}/boards/activediscussions",
                defaults: new { controller = "Boards", action = "ActiveDiscussions" });

            endpointRouteBuilder.MapControllerRoute(name: "ActiveDiscussionsPaged",
                pattern: $"{lang}/boards/activediscussions/page/{{pageNumber:int}}",
                defaults: new { controller = "Boards", action = "ActiveDiscussions" });

            //forums RSS (file result)
            endpointRouteBuilder.MapControllerRoute(name: "ActiveDiscussionsRSS",
                pattern: $"boards/activediscussionsrss",
                defaults: new { controller = "Boards", action = "ActiveDiscussionsRSS" });

            endpointRouteBuilder.MapControllerRoute(name: "PostEdit",
                pattern: $"{lang}/boards/postedit/{{id:min(0)}}",
                defaults: new { controller = "Boards", action = "PostEdit" });

            endpointRouteBuilder.MapControllerRoute(name: "PostDelete",
                pattern: $"{lang}/boards/postdelete/{{id:min(0)}}",
                defaults: new { controller = "Boards", action = "PostDelete" });

            endpointRouteBuilder.MapControllerRoute(name: "PostCreate",
                pattern: $"{lang}/boards/postcreate/{{id:min(0)}}",
                defaults: new { controller = "Boards", action = "PostCreate" });

            endpointRouteBuilder.MapControllerRoute(name: "PostCreateQuote",
                pattern: $"{lang}/boards/postcreate/{{id:min(0)}}/{{quote:min(0)}}",
                defaults: new { controller = "Boards", action = "PostCreate" });

            endpointRouteBuilder.MapControllerRoute(name: "TopicEdit",
                pattern: $"{lang}/boards/topicedit/{{id:min(0)}}",
                defaults: new { controller = "Boards", action = "TopicEdit" });

            endpointRouteBuilder.MapControllerRoute(name: "TopicDelete",
                pattern: $"{lang}/boards/topicdelete/{{id:min(0)}}",
                defaults: new { controller = "Boards", action = "TopicDelete" });

            endpointRouteBuilder.MapControllerRoute(name: "TopicCreate",
                pattern: $"{lang}/boards/topiccreate/{{id:min(0)}}",
                defaults: new { controller = "Boards", action = "TopicCreate" });

            endpointRouteBuilder.MapControllerRoute(name: "TopicMove",
                pattern: $"{lang}/boards/topicmove/{{id:min(0)}}",
                defaults: new { controller = "Boards", action = "TopicMove" });

            //topic watch (AJAX)
            endpointRouteBuilder.MapControllerRoute(name: "TopicWatch",
                pattern: $"boards/topicwatch/{{id:min(0)}}",
                defaults: new { controller = "Boards", action = "TopicWatch" });

            endpointRouteBuilder.MapControllerRoute(name: "TopicSlug",
                pattern: $"{lang}/boards/topic/{{id:min(0)}}/{{slug?}}",
                defaults: new { controller = "Boards", action = "Topic" });

            endpointRouteBuilder.MapControllerRoute(name: "TopicSlugPaged",
                pattern: $"{lang}/boards/topic/{{id:min(0)}}/{{slug?}}/page/{{pageNumber:int}}",
                defaults: new { controller = "Boards", action = "Topic" });

            //forum watch (AJAX)
            endpointRouteBuilder.MapControllerRoute(name: "ForumWatch",
                pattern: $"boards/forumwatch/{{id:min(0)}}",
                defaults: new { controller = "Boards", action = "ForumWatch" });

            //forums RSS (file result)
            endpointRouteBuilder.MapControllerRoute(name: "ForumRSS",
                pattern: $"boards/forumrss/{{id:min(0)}}",
                defaults: new { controller = "Boards", action = "ForumRSS" });

            endpointRouteBuilder.MapControllerRoute(name: "ForumSlug",
                pattern: $"{lang}/boards/forum/{{id:min(0)}}/{{slug?}}",
                defaults: new { controller = "Boards", action = "Forum" });

            endpointRouteBuilder.MapControllerRoute(name: "ForumSlugPaged",
                pattern: $"{lang}/boards/forum/{{id:min(0)}}/{{slug?}}/page/{{pageNumber:int}}",
                defaults: new { controller = "Boards", action = "Forum" });

            endpointRouteBuilder.MapControllerRoute(name: "ForumGroupSlug",
                pattern: $"{lang}/boards/forumgroup/{{id:min(0)}}/{{slug?}}",
                defaults: new { controller = "Boards", action = "ForumGroup" });

            endpointRouteBuilder.MapControllerRoute(name: "Search",
                pattern: $"{lang}/boards/search",
                defaults: new { controller = "Boards", action = "Search" });

            //private messages
            endpointRouteBuilder.MapControllerRoute(name: "PrivateMessages",
                pattern: $"{lang}/privatemessages/{{tab?}}",
                defaults: new { controller = "PrivateMessages", action = "Index" });

            endpointRouteBuilder.MapControllerRoute(name: "PrivateMessagesPaged",
                pattern: $"{lang}/privatemessages/{{tab?}}/page/{{pageNumber:min(0)}}",
                defaults: new { controller = "PrivateMessages", action = "Index" });

            endpointRouteBuilder.MapControllerRoute(name: "PrivateMessagesInbox",
                pattern: $"{lang}/inboxupdate",
                defaults: new { controller = "PrivateMessages", action = "InboxUpdate" });

            endpointRouteBuilder.MapControllerRoute(name: "PrivateMessagesSent",
                pattern: $"{lang}/sentupdate",
                defaults: new { controller = "PrivateMessages", action = "SentUpdate" });

            endpointRouteBuilder.MapControllerRoute(name: "SendPM",
                pattern: $"{lang}/sendpm/{{toCustomerId:min(0)}}",
                defaults: new { controller = "PrivateMessages", action = "SendPM" });

            endpointRouteBuilder.MapControllerRoute(name: "SendPMReply",
                pattern: $"{lang}/sendpm/{{toCustomerId:min(0)}}/{{replyToMessageId:min(0)}}",
                defaults: new { controller = "PrivateMessages", action = "SendPM" });

            endpointRouteBuilder.MapControllerRoute(name: "ViewPM",
                pattern: $"{lang}/viewpm/{{privateMessageId:min(0)}}",
                defaults: new { controller = "PrivateMessages", action = "ViewPM" });

            endpointRouteBuilder.MapControllerRoute(name: "DeletePM",
                pattern: $"{lang}/deletepm/{{privateMessageId:min(0)}}",
                defaults: new { controller = "PrivateMessages", action = "DeletePM" });

            //activate newsletters
            endpointRouteBuilder.MapControllerRoute(name: "NewsletterActivation",
                pattern: $"{lang}/newsletter/subscriptionactivation/{{token:guid}}/{{active}}",
                defaults: new { controller = "Newsletter", action = "SubscriptionActivation" });

            //robots.txt (file result)
            endpointRouteBuilder.MapControllerRoute(name: "robots.txt",
                pattern: $"robots.txt",
                defaults: new { controller = "Common", action = "RobotsTextFile" });

            //sitemap
            endpointRouteBuilder.MapControllerRoute(name: "Sitemap",
                pattern: $"{lang}/sitemap",
                defaults: new { controller = "Common", action = "Sitemap" });

            //sitemap.xml (file result)
            endpointRouteBuilder.MapControllerRoute(name: "sitemap.xml",
                pattern: $"sitemap.xml",
                defaults: new { controller = "Common", action = "SitemapXml" });

            endpointRouteBuilder.MapControllerRoute(name: "sitemap-indexed.xml",
                pattern: $"sitemap-{{Id:min(0)}}.xml",
                defaults: new { controller = "Common", action = "SitemapXml" });

            //store closed
            endpointRouteBuilder.MapControllerRoute(name: "StoreClosed",
                pattern: $"{lang}/storeclosed",
                defaults: new { controller = "Common", action = "StoreClosed" });

            //install
            endpointRouteBuilder.MapControllerRoute(name: "Installation",
                pattern: $"{NopInstallationDefaults.InstallPath}",
                defaults: new { controller = "Install", action = "Index" });

            //error page
            endpointRouteBuilder.MapControllerRoute(name: "Error",
                pattern: $"error",
                defaults: new { controller = "Common", action = "Error" });

            //page not found
            endpointRouteBuilder.MapControllerRoute(name: "PageNotFound",
                pattern: $"{lang}/page-not-found",
                defaults: new { controller = "Common", action = "PageNotFound" });

            //WeeklyReport
            endpointRouteBuilder.MapControllerRoute(name: "WeeklyreportCreate",
               pattern: $"{lang}/WeeklyReporting/WeeklyreportCreate",
               defaults: new { controller = "WeeklyReporting", action = "WeeklyreportCreate" });
           
            endpointRouteBuilder.MapControllerRoute(name: "List",
              pattern: $"{lang}/WeeklyReporting/List",
              defaults: new { controller = "WeeklyReporting", action = "List" });

            // Leave Management
            endpointRouteBuilder.MapControllerRoute(name: "SearchLeave",
               pattern: $"{lang}/LeaveManagement/SearchLeave",
               defaults: new { controller = "LeaveManagement", action = "SearchLeave" });

            // TimeSummaryReport
            endpointRouteBuilder.MapControllerRoute(name: "TimeSummaryReport",
               pattern: $"{lang}/Reports/TimeSummaryReport",
               defaults: new { controller = "Reports", action = "TimeSummaryReport" });

            endpointRouteBuilder.MapControllerRoute(name: "AddRatings",
             pattern: $"{lang}/Performance/AddRatings",
             defaults: new { controller = "Performance", action = "AddRatings" });

            endpointRouteBuilder.MapControllerRoute(name: "TaskList",
               pattern: $"{lang}/ProjectTask/List",
               defaults: new { controller = "List", action = "ProjectTask" });

            endpointRouteBuilder.MapControllerRoute(name: "ProjectManagement",
               pattern: $"{lang}/ProjectTask/ProjectManagement",
               defaults: new { controller = "ProjectTask", action = "ProjectManagement" });
            

            endpointRouteBuilder.MapControllerRoute(name: "EmployeePerformanceReport",
              pattern: $"{lang}/Reports/EmployeePerformanceReport",
              defaults: new { controller = "Reports", action = "EmployeePerformanceReport" });


            endpointRouteBuilder.MapControllerRoute(name: "TimeSheetList",
              pattern: $"{lang}/TimeSheet/List",
              defaults: new { controller = "TimeSheet", action = "List" });
            
            endpointRouteBuilder.MapControllerRoute(name: "ViewUpdateList",
              pattern: $"{lang}/UpdateSubmission/SubmissionList",
              defaults: new { controller = "UpdateSubmission", action = "SubmissionList" });

            endpointRouteBuilder.MapControllerRoute(name: "UpdateTimeSheet",
              pattern: $"{lang}/TimeSheet/UpdateTimeSheet",
              defaults: new { controller = "TimeSheet", action = "UpdateTimeSheet" });

            endpointRouteBuilder.MapControllerRoute(name: "EmployeeAttendanceReport",
             pattern: $"{lang}/Reports/AttendanceReport",
             defaults: new { controller = "Reports", action = "AttendanceReport" });

            endpointRouteBuilder.MapControllerRoute(name: "ProjectLeaderReview",
             pattern: $"{lang}/Performance/ProjectLeaderReview",
             defaults: new { controller = "Performance", action = "ProjectLeaderReview" });

            
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 0;

        #endregion
    }
}