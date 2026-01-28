using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using App.Core;
using App.Core.Domain;
using App.Core.Domain.Blogs;
using App.Core.Domain.Catalog;
using App.Core.Domain.Customers;
using App.Core.Domain.Directory;
using App.Core.Domain.Forums;
using App.Core.Domain.Messages;
using App.Core.Domain.News;
using App.Core.Domain.Stores;
using App.Core.Events;
using App.Core.Infrastructure;
using App.Services.Blogs;
using App.Services.Common;
using App.Services.Customers;
using App.Services.Directory;
using App.Services.Forums;
using App.Services.Helpers;
using App.Services.Html;
using App.Services.Localization;
using App.Services.News;
using App.Services.Seo;
using App.Services.Stores;
using App.Services.Events;
using App.Core.Domain.ManageResumes;
using Nop.Core.Domain.Customers;
using App.Core.Domain.result;
using App.Core.Domain.Extension.Candidate;
using App.Core.Domain.Extension.WeeklyQuestions;
using App.Services.Employees;
using DocumentFormat.OpenXml.EMMA;
using Newtonsoft.Json;
using App.Core.Domain.WeeklyQuestion;

namespace App.Services.Messages
{
    /// <summary>
    /// Message token provider
    /// </summary>
    public partial class MessageTokenProvider : IMessageTokenProvider
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IAddressService _addressService;
        private readonly IBlogService _blogService;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerAttributeFormatter _customerAttributeFormatter;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHtmlFormatter _htmlFormatter;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly INewsService _newsService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly MessageTemplatesSettings _templatesSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly IEmployeeService _employeeService;
        private Dictionary<string, IEnumerable<string>> _allowedTokens;

        #endregion

        #region Ctor

        public MessageTokenProvider(CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            IActionContextAccessor actionContextAccessor,
            IAddressAttributeFormatter addressAttributeFormatter,
            IAddressService addressService,
            IBlogService blogService,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerAttributeFormatter customerAttributeFormatter,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            IHtmlFormatter htmlFormatter,
            ILanguageService languageService,
            ILocalizationService localizationService,
            INewsService newsService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStoreService storeService,
            IUrlHelperFactory urlHelperFactory,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            MessageTemplatesSettings templatesSettings,
            StoreInformationSettings storeInformationSettings,
            IEmployeeService employeeService)
        {
            _catalogSettings = catalogSettings;
            _currencySettings = currencySettings;
            _actionContextAccessor = actionContextAccessor;
            _addressAttributeFormatter = addressAttributeFormatter;
            _addressService = addressService;
            _blogService = blogService;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerAttributeFormatter = customerAttributeFormatter;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
            _htmlFormatter = htmlFormatter;
            _languageService = languageService;
            _localizationService = localizationService;
            _newsService = newsService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _storeService = storeService;
            _urlHelperFactory = urlHelperFactory;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _templatesSettings = templatesSettings;
            _storeInformationSettings = storeInformationSettings;
            _employeeService = employeeService;
        }

        #endregion

        #region Allowed tokens

        /// <summary>
        /// Get all available tokens by token groups
        /// </summary>
        protected Dictionary<string, IEnumerable<string>> AllowedTokens
        {
            get
            {
                if (_allowedTokens != null)
                    return _allowedTokens;

                _allowedTokens = new Dictionary<string, IEnumerable<string>>();

                //store tokens
                _allowedTokens.Add(TokenGroupNames.StoreTokens, new[]
                {
                    "%Store.Name%",
                    "%Store.URL%",
                    "%Store.Email%",
                    "%Store.CompanyName%",
                    "%Store.CompanyAddress%",
                    "%Store.CompanyPhoneNumber%",
                    "%Store.CompanyVat%",
                    "%Facebook.URL%",
                    "%Twitter.URL%",
                    "%YouTube.URL%",
                    "%Instagram.URL%"
                });

                //customer tokens
                _allowedTokens.Add(TokenGroupNames.CustomerTokens, new[]
                {
                    "%Customer.Email%",
                    "%Customer.Username%",
                    "%Customer.FullName%",
                    "%Customer.FirstName%",
                    "%Customer.LastName%",
                    "%Customer.VatNumber%",
                    "%Customer.VatNumberStatus%",
                    "%Customer.CustomAttributes%",
                    "%Customer.PasswordRecoveryURL%",
                    "%Customer.AccountActivationURL%",
                    "%Customer.EmailRevalidationURL%",
                    "%Wishlist.URLForCustomer%"
                });

                //order tokens
                _allowedTokens.Add(TokenGroupNames.OrderTokens, new[]
                {
                    "%Order.OrderNumber%",
                    "%Order.CustomerFullName%",
                    "%Order.CustomerEmail%",
                    "%Order.BillingFirstName%",
                    "%Order.BillingLastName%",
                    "%Order.BillingPhoneNumber%",
                    "%Order.BillingEmail%",
                    "%Order.BillingFaxNumber%",
                    "%Order.BillingCompany%",
                    "%Order.BillingAddress1%",
                    "%Order.BillingAddress2%",
                    "%Order.BillingCity%",
                    "%Order.BillingCounty%",
                    "%Order.BillingStateProvince%",
                    "%Order.BillingZipPostalCode%",
                    "%Order.BillingCountry%",
                    "%Order.BillingCustomAttributes%",
                    "%Order.Shippable%",
                    "%Order.ShippingMethod%",
                    "%Order.ShippingFirstName%",
                    "%Order.ShippingLastName%",
                    "%Order.ShippingPhoneNumber%",
                    "%Order.ShippingEmail%",
                    "%Order.ShippingFaxNumber%",
                    "%Order.ShippingCompany%",
                    "%Order.ShippingAddress1%",
                    "%Order.ShippingAddress2%",
                    "%Order.ShippingCity%",
                    "%Order.ShippingCounty%",
                    "%Order.ShippingStateProvince%",
                    "%Order.ShippingZipPostalCode%",
                    "%Order.ShippingCountry%",
                    "%Order.ShippingCustomAttributes%",
                    "%Order.PaymentMethod%",
                    "%Order.VatNumber%",
                    "%Order.CustomValues%",
                    "%Order.Product(s)%",
                    "%Order.CreatedOn%",
                    "%Order.OrderURLForCustomer%",
                    "%Order.PickupInStore%",
                    "%Order.OrderId%",
                    "%Order.IsCompletelyShipped%",
                    "%Order.IsCompletelyReadyForPickup%",
                    "%Order.IsCompletelyDelivered%"
                });

                //shipment tokens
                _allowedTokens.Add(TokenGroupNames.ShipmentTokens, new[]
                {
                    "%Shipment.ShipmentNumber%",
                    "%Shipment.TrackingNumber%",
                    "%Shipment.TrackingNumberURL%",
                    "%Shipment.Product(s)%",
                    "%Shipment.URLForCustomer%"
                });

                //refunded order tokens
                _allowedTokens.Add(TokenGroupNames.RefundedOrderTokens, new[]
                {
                    "%Order.AmountRefunded%"
                });

                //order note tokens
                _allowedTokens.Add(TokenGroupNames.OrderNoteTokens, new[]
                {
                    "%Order.NewNoteText%",
                    "%Order.OrderNoteAttachmentUrl%"
                });

                //recurring payment tokens
                _allowedTokens.Add(TokenGroupNames.RecurringPaymentTokens, new[]
                {
                    "%RecurringPayment.ID%",
                    "%RecurringPayment.CancelAfterFailedPayment%",
                    "%RecurringPayment.RecurringPaymentType%"
                });

                //newsletter subscription tokens
                _allowedTokens.Add(TokenGroupNames.SubscriptionTokens, new[]
                {
                    "%NewsLetterSubscription.Email%",
                    "%NewsLetterSubscription.ActivationUrl%",
                    "%NewsLetterSubscription.DeactivationUrl%"
                });

                //product tokens
                _allowedTokens.Add(TokenGroupNames.ProductTokens, new[]
                {
                    "%Product.ID%",
                    "%Product.Name%",
                    "%Product.ShortDescription%",
                    "%Product.ProductURLForCustomer%",
                    "%Product.SKU%",
                    "%Product.StockQuantity%"
                });

                //return request tokens
                _allowedTokens.Add(TokenGroupNames.ReturnRequestTokens, new[]
                {
                    "%ReturnRequest.CustomNumber%",
                    "%ReturnRequest.OrderId%",
                    "%ReturnRequest.Product.Quantity%",
                    "%ReturnRequest.Product.Name%",
                    "%ReturnRequest.Reason%",
                    "%ReturnRequest.RequestedAction%",
                    "%ReturnRequest.CustomerComment%",
                    "%ReturnRequest.StaffNotes%",
                    "%ReturnRequest.Status%"
                });

                //forum tokens
                _allowedTokens.Add(TokenGroupNames.ForumTokens, new[]
                {
                    "%Forums.ForumURL%",
                    "%Forums.ForumName%"
                });

                //forum topic tokens
                _allowedTokens.Add(TokenGroupNames.ForumTopicTokens, new[]
                {
                    "%Forums.TopicURL%",
                    "%Forums.TopicName%"
                });

                //forum post tokens
                _allowedTokens.Add(TokenGroupNames.ForumPostTokens, new[]
                {
                    "%Forums.PostAuthor%",
                    "%Forums.PostBody%"
                });

                //private message tokens
                _allowedTokens.Add(TokenGroupNames.PrivateMessageTokens, new[]
                {
                    "%PrivateMessage.Subject%",
                    "%PrivateMessage.Text%"
                });

                //vendor tokens
                _allowedTokens.Add(TokenGroupNames.VendorTokens, new[]
                {
                    "%Vendor.Name%",
                    "%Vendor.Email%",
                    "%Vendor.VendorAttributes%"
                });

                //gift card tokens
                _allowedTokens.Add(TokenGroupNames.GiftCardTokens, new[]
                {
                    "%GiftCard.SenderName%",
                    "%GiftCard.SenderEmail%",
                    "%GiftCard.RecipientName%",
                    "%GiftCard.RecipientEmail%",
                    "%GiftCard.Amount%",
                    "%GiftCard.CouponCode%",
                    "%GiftCard.Message%"
                });

                //product review tokens
                _allowedTokens.Add(TokenGroupNames.ProductReviewTokens, new[]
                {
                    "%ProductReview.ProductName%",
                    "%ProductReview.Title%",
                    "%ProductReview.IsApproved%",
                    "%ProductReview.ReviewText%",
                    "%ProductReview.ReplyText%"
                });

                //attribute combination tokens
                _allowedTokens.Add(TokenGroupNames.AttributeCombinationTokens, new[]
                {
                    "%AttributeCombination.Formatted%",
                    "%AttributeCombination.SKU%",
                    "%AttributeCombination.StockQuantity%"
                });

                //blog comment tokens
                _allowedTokens.Add(TokenGroupNames.BlogCommentTokens, new[]
                {
                    "%BlogComment.BlogPostTitle%"
                });

                //news comment tokens
                _allowedTokens.Add(TokenGroupNames.NewsCommentTokens, new[]
                {
                    "%NewsComment.NewsTitle%"
                });

                //product back in stock tokens
                _allowedTokens.Add(TokenGroupNames.ProductBackInStockTokens, new[]
                {
                    "%BackInStockSubscription.ProductName%",
                    "%BackInStockSubscription.ProductUrl%"
                });

                //email a friend tokens
                _allowedTokens.Add(TokenGroupNames.EmailAFriendTokens, new[]
                {
                    "%EmailAFriend.PersonalMessage%",
                    "%EmailAFriend.Email%"
                });

                //wishlist to friend tokens
                _allowedTokens.Add(TokenGroupNames.WishlistToFriendTokens, new[]
                {
                    "%Wishlist.PersonalMessage%",
                    "%Wishlist.Email%"
                });

                //VAT validation tokens
                _allowedTokens.Add(TokenGroupNames.VatValidation, new[]
                {
                    "%VatValidationResult.Name%",
                    "%VatValidationResult.Address%"
                });

                //contact us tokens
                _allowedTokens.Add(TokenGroupNames.ContactUs, new[]
                {
                    "%ContactUs.SenderEmail%",
                    "%ContactUs.SenderName%",
                    "%ContactUs.Body%"
                });

                //contact vendor tokens
                _allowedTokens.Add(TokenGroupNames.ContactVendor, new[]
                {
                    "%ContactUs.SenderEmail%",
                    "%ContactUs.SenderName%",
                    "%ContactUs.Body%"
                });

                return _allowedTokens;
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Generates an absolute URL for the specified store, routeName and route values
        /// </summary>
        /// <param name="storeId">Store identifier; Pass 0 to load URL of the current store</param>
        /// <param name="routeName">The name of the route that is used to generate URL</param>
        /// <param name="routeValues">An object that contains route values</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the generated URL
        /// </returns>
        protected virtual async Task<string> RouteUrlAsync(int storeId = 0, string routeName = null, object routeValues = null)
        {
            //try to get a store by the passed identifier
            var store = await _storeService.GetStoreByIdAsync(storeId) ?? await _storeContext.GetCurrentStoreAsync()
                ?? throw new Exception("No store could be loaded");

            //ensure that the store URL is specified
            if (string.IsNullOrEmpty(store.Url))
                throw new Exception("URL cannot be null");

            //generate the relative URL
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var url = urlHelper.RouteUrl(routeName, routeValues);

            //compose the result
            return new Uri(new Uri(store.Url), url).AbsoluteUri;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add store tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="store">Store</param>
        /// <param name="emailAccount">Email account</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddStoreTokensAsync(IList<Token> tokens, Store store, EmailAccount emailAccount)
        {
            if (emailAccount == null)
                throw new ArgumentNullException(nameof(emailAccount));

            tokens.Add(new Token("Store.Name", await _localizationService.GetLocalizedAsync(store, x => x.Name)));
            tokens.Add(new Token("Store.URL", store.Url, true));
            tokens.Add(new Token("Store.Email", emailAccount.Email));
            tokens.Add(new Token("Store.CompanyName", store.CompanyName));
            tokens.Add(new Token("Store.CompanyAddress", store.CompanyAddress));
            tokens.Add(new Token("Store.CompanyPhoneNumber", store.CompanyPhoneNumber));
            tokens.Add(new Token("Store.CompanyVat", store.CompanyVat));

            tokens.Add(new Token("Facebook.URL", _storeInformationSettings.FacebookLink));
            tokens.Add(new Token("Twitter.URL", _storeInformationSettings.TwitterLink));
            tokens.Add(new Token("YouTube.URL", _storeInformationSettings.YoutubeLink));
            tokens.Add(new Token("Instagram.URL", _storeInformationSettings.InstagramLink));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(store, tokens);
        }

        /// <summary>
        /// Add customer tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddCustomerTokensAsync(IList<Token> tokens, int customerId)
        {
            if (customerId <= 0)
                throw new ArgumentOutOfRangeException(nameof(customerId));

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            await AddCustomerTokensAsync(tokens, customer);
        }

        /// <summary>
        /// Add customer tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddCustomerTokensAsync(IList<Token> tokens, Customer customer)
        {
            tokens.Add(new Token("Customer.Email", customer.Email));
            tokens.Add(new Token("Customer.Username", customer.Username));
            tokens.Add(new Token("Customer.FullName", await _customerService.GetCustomerFullNameAsync(customer)));
            tokens.Add(new Token("Customer.FirstName", customer.FirstName));
            tokens.Add(new Token("Customer.LastName", customer.LastName));
            tokens.Add(new Token("Customer.VatNumber", customer.VatNumber));

            var customAttributesXml = customer.CustomCustomerAttributesXML;
            tokens.Add(new Token("Customer.CustomAttributes", await _customerAttributeFormatter.FormatAttributesAsync(customAttributesXml), true));

            //note: we do not use SEO friendly URLS for these links because we can get errors caused by having .(dot) in the URL (from the email address)
            var passwordRecoveryUrl  = await RouteUrlAsync(routeName: "PasswordRecoveryConfirm", routeValues: new { token = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute), guid = customer.CustomerGuid });
            var accountActivationUrl  = await RouteUrlAsync(routeName: "AccountActivation", routeValues: new { token = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.AccountActivationTokenAttribute), guid = customer.CustomerGuid });
            var emailRevalidationUrl  = await RouteUrlAsync(routeName: "EmailRevalidation", routeValues: new { token = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.EmailRevalidationTokenAttribute), guid = customer.CustomerGuid });
            var wishlistUrl  = await RouteUrlAsync(routeName: "Wishlist", routeValues: new { customerGuid = customer.CustomerGuid });
            tokens.Add(new Token("Customer.PasswordRecoveryURL", passwordRecoveryUrl, true));
            tokens.Add(new Token("Customer.AccountActivationURL", accountActivationUrl, true));
            tokens.Add(new Token("Customer.EmailRevalidationURL", emailRevalidationUrl, true));
            tokens.Add(new Token("Wishlist.URLForCustomer", wishlistUrl, true));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(customer, tokens);
        }
        /// <summary>
        /// Add Interviewer tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="customer">Interviewer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddInterviewerTokensAsync(IList<Token> tokens, CandidatesResumes trainee)
        {
            tokens.Add(new Token("Trainee.EmployeeId", trainee.EmployeeId));
            tokens.Add(new Token("Trainee.AppliedForId", trainee.AppliedForId));
            tokens.Add(new Token("Trainee.PracticalRoundDate", trainee.PracticalRoundDate));
            tokens.Add(new Token("Trainee.TechnicalRoundDate", trainee.TechnicalRoundDate));
            tokens.Add(new Token("Trainee.StatusId", trainee.StatusId));
           
            //event notification
           await _eventPublisher.EntityTokensAddedAsync(trainee, tokens);
         }
        //public virtual async Task AddTraineeTokensAsync(IList<Token> tokens, Candidate trainee)
        //{
        //    tokens.Add(new Token("Trainee.Email", trainee.Email));
        //    tokens.Add(new Token("Trainee.FirstName", trainee.FirstName));
        //    tokens.Add(new Token("Trainee.LastName", trainee.LastName));
        //    tokens.Add(new Token("Trainee.CollegeName", trainee.CollegeName));
        //    tokens.Add(new Token("Trainee.MobileNumber", trainee.MobileNo));
        //    tokens.Add(new Token("Trainee.Percentage", trainee.Percentage));
        //    tokens.Add(new Token("Trainee.Address1", trainee.Address1));
        //    tokens.Add(new Token("Trainee.Address2", trainee.Address2));
        //    tokens.Add(new Token("Trainee.City", trainee.City));
        //    tokens.Add(new Token("Trainee.DateForInterview", trainee.DateForInterview.ToString()));
        //    tokens.Add(new Token("Trainee.PersonInCharge", trainee.PersonInCharge));
        //    tokens.Add(new Token("Trainee.Venue", trainee.Venue));
        //    tokens.Add(new Token("Trainee.Resume", trainee.ResumeUrl));
        //    tokens.Add(new Token("Trainee.Id", trainee.Id));
        //    //event notification
        //    await _eventPublisher.EntityTokensAddedAsync(trainee, tokens);
        //}
    /// <summary>
    /// Add newsletter subscription tokens
    /// </summary>
    /// <param name="tokens">List of already added tokens</param>
    /// <param name="subscription">Newsletter subscription</param>
    /// <returns>A task that represents the asynchronous operation</returns>
     public virtual async Task AddInterviewerToHrTokensAsync(IList<Token> tokens, CandidatesResult trainee)
        {
            var selectedAvailableDaysOption = trainee.ResultStatusId;
            tokens.Add(new Token("Trainee.ResultStatusId", ((ResultStatusEnum)selectedAvailableDaysOption).ToString()));
            tokens.Add(new Token("Trainee.Feedback", trainee.Feedback));
            tokens.Add(new Token("Trainee.partially", trainee.partially));
            tokens.Add(new Token("Trainee.Incorrect", trainee.Incorrect));
            tokens.Add(new Token("Trainee.correct", trainee.correct));
           
            //event notification
           await _eventPublisher.EntityTokensAddedAsync(trainee, tokens);
    }
        public virtual async Task AddWeeklyupdateToHrTokensAsync(IList<Token> tokens, WeeklyReports trainee)
        {
            var Eid = trainee.EmployeeId;
            var jsonString = trainee.Qdata;
            var employee = await _employeeService.GetEmployeeByIdAsync(Eid);
            var marks = JsonConvert.DeserializeObject<List<string>>(jsonString);
            foreach (var mark in marks)
            {
              var  model = JsonConvert.DeserializeObject<List<WeeklyQuestions>>(mark);
                foreach (var item in model)
                { 
                    tokens.Add(new Token($"item.QuestionText", item.QuestionText));
                    tokens.Add(new Token($"item.ControlValue", item.ControlValue));
                }

            }
            tokens.Add(new Token("trainee.Employee", employee.FirstName + employee.LastName));

           

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(trainee, tokens);
        }
        public virtual async Task AddNewsLetterSubscriptionTokensAsync(IList<Token> tokens, NewsLetterSubscription subscription)
        {
            tokens.Add(new Token("NewsLetterSubscription.Email", subscription.Email));

            var activationUrl = await RouteUrlAsync(routeName: "NewsletterActivation", routeValues: new { token = subscription.NewsLetterSubscriptionGuid, active = "true" });
            tokens.Add(new Token("NewsLetterSubscription.ActivationUrl", activationUrl, true));

            var deactivationUrl = await RouteUrlAsync(routeName: "NewsletterActivation", routeValues: new { token = subscription.NewsLetterSubscriptionGuid, active = "false" });
            tokens.Add(new Token("NewsLetterSubscription.DeactivationUrl", deactivationUrl, true));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(subscription, tokens);
        }

        /// <summary>
        /// Add blog comment tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="blogComment">Blog post comment</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddBlogCommentTokensAsync(IList<Token> tokens, BlogComment blogComment)
        {
            var blogPost = await _blogService.GetBlogPostByIdAsync(blogComment.BlogPostId);

            tokens.Add(new Token("BlogComment.BlogPostTitle", blogPost.Title));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(blogComment, tokens);
        }

        /// <summary>
        /// Add news comment tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="newsComment">News comment</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddNewsCommentTokensAsync(IList<Token> tokens, NewsComment newsComment)
        {
            var newsItem = await _newsService.GetNewsByIdAsync(newsComment.NewsItemId);

            tokens.Add(new Token("NewsComment.NewsTitle", newsItem.Title));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(newsComment, tokens);
        }

        /// <summary>
        /// Add forum topic tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="forumTopic">Forum topic</param>
        /// <param name="friendlyForumTopicPageIndex">Friendly (starts with 1) forum topic page to use for URL generation</param>
        /// <param name="appendedPostIdentifierAnchor">Forum post identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddForumTopicTokensAsync(IList<Token> tokens, ForumTopic forumTopic,
            int? friendlyForumTopicPageIndex = null, int? appendedPostIdentifierAnchor = null)
        {
            //attributes
            //we cannot inject IForumService into constructor because it'll cause circular references.
            //that's why we resolve it here this way
            var forumService = EngineContext.Current.Resolve<IForumService>();

            string topicUrl;
            if (friendlyForumTopicPageIndex.HasValue && friendlyForumTopicPageIndex.Value > 1)
                topicUrl = await RouteUrlAsync(routeName: "TopicSlugPaged", routeValues: new { id = forumTopic.Id, slug = await forumService.GetTopicSeNameAsync(forumTopic), pageNumber = friendlyForumTopicPageIndex.Value });
            else
                topicUrl = await RouteUrlAsync(routeName: "TopicSlug", routeValues: new { id = forumTopic.Id, slug = await forumService.GetTopicSeNameAsync(forumTopic) });
            if (appendedPostIdentifierAnchor.HasValue && appendedPostIdentifierAnchor.Value > 0)
                topicUrl = $"{topicUrl}#{appendedPostIdentifierAnchor.Value}";
            tokens.Add(new Token("Forums.TopicURL", topicUrl, true));
            tokens.Add(new Token("Forums.TopicName", forumTopic.Subject));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(forumTopic, tokens);
        }

        /// <summary>
        /// Add forum post tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="forumPost">Forum post</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddForumPostTokensAsync(IList<Token> tokens, ForumPost forumPost)
        {
            //attributes
            //we cannot inject IForumService into constructor because it'll cause circular references.
            //that's why we resolve it here this way
            var forumService = EngineContext.Current.Resolve<IForumService>();

            var customer = await _customerService.GetCustomerByIdAsync(forumPost.CustomerId);

            tokens.Add(new Token("Forums.PostAuthor", await _customerService.FormatUsernameAsync(customer)));
            tokens.Add(new Token("Forums.PostBody", forumService.FormatPostText(forumPost), true));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(forumPost, tokens);
        }

        /// <summary>
        /// Add forum tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="forum">Forum</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddForumTokensAsync(IList<Token> tokens, Forum forum)
        {
            //attributes
            //we cannot inject IForumService into constructor because it'll cause circular references.
            //that's why we resolve it here this way
            var forumService = EngineContext.Current.Resolve<IForumService>();

            var forumUrl = await RouteUrlAsync(routeName: "ForumSlug", routeValues: new { id = forum.Id, slug = await forumService.GetForumSeNameAsync(forum) });
            tokens.Add(new Token("Forums.ForumURL", forumUrl, true));
            tokens.Add(new Token("Forums.ForumName", forum.Name));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(forum, tokens);
        }

        /// <summary>
        /// Add private message tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="privateMessage">Private message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddPrivateMessageTokensAsync(IList<Token> tokens, PrivateMessage privateMessage)
        {
            //attributes
            //we cannot inject IForumService into constructor because it'll cause circular references.
            //that's why we resolve it here this way
            var forumService = EngineContext.Current.Resolve<IForumService>();

            tokens.Add(new Token("PrivateMessage.Subject", privateMessage.Subject));
            tokens.Add(new Token("PrivateMessage.Text", forumService.FormatPrivateMessageText(privateMessage), true));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(privateMessage, tokens);
        }

        /// <summary>
        /// Get collection of allowed (supported) message tokens for campaigns
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the collection of allowed (supported) message tokens for campaigns
        /// </returns>
        public virtual async Task<IEnumerable<string>> GetListOfCampaignAllowedTokensAsync()
        {
            var additionalTokens = new CampaignAdditionalTokensAddedEvent();
            await _eventPublisher.PublishAsync(additionalTokens);

            var allowedTokens = (await GetListOfAllowedTokensAsync(new[] { TokenGroupNames.StoreTokens, TokenGroupNames.SubscriptionTokens })).ToList();
            allowedTokens.AddRange(additionalTokens.AdditionalTokens);

            return allowedTokens.Distinct();
        }

        /// <summary>
        /// Get collection of allowed (supported) message tokens
        /// </summary>
        /// <param name="tokenGroups">Collection of token groups; pass null to get all available tokens</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the collection of allowed message tokens
        /// </returns>
        public virtual async Task<IEnumerable<string>> GetListOfAllowedTokensAsync(IEnumerable<string> tokenGroups = null)
        {
            var additionalTokens = new AdditionalTokensAddedEvent
            {
                TokenGroups = tokenGroups
            };
            await _eventPublisher.PublishAsync(additionalTokens);

            var allowedTokens = AllowedTokens.Where(x => tokenGroups == null || tokenGroups.Contains(x.Key))
                .SelectMany(x => x.Value).ToList();

            allowedTokens.AddRange(additionalTokens.AdditionalTokens);

            return allowedTokens.Distinct();
        }

        /// <summary>
        /// Get token groups of message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <returns>Collection of token group names</returns>
        public virtual IEnumerable<string> GetTokenGroups(MessageTemplate messageTemplate)
        {
            //groups depend on which tokens are added at the appropriate methods in IWorkflowMessageService
            return messageTemplate.Name switch
            {
                MessageTemplateSystemNames.CustomerRegisteredStoreOwnerNotification or 
                MessageTemplateSystemNames.CustomerWelcomeMessage or 
                MessageTemplateSystemNames.CustomerEmailValidationMessage or 
                MessageTemplateSystemNames.CustomerEmailRevalidationMessage or 
                MessageTemplateSystemNames.CustomerPasswordRecoveryMessage => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.CustomerTokens },

                MessageTemplateSystemNames.NewsletterSubscriptionActivationMessage or 
                MessageTemplateSystemNames.NewsletterSubscriptionDeactivationMessage => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.SubscriptionTokens },

                MessageTemplateSystemNames.EmailAFriendMessage => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.CustomerTokens, TokenGroupNames.ProductTokens, TokenGroupNames.EmailAFriendTokens },
                MessageTemplateSystemNames.WishlistToFriendMessage => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.CustomerTokens, TokenGroupNames.WishlistToFriendTokens },

                MessageTemplateSystemNames.NewForumTopicMessage => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ForumTopicTokens, TokenGroupNames.ForumTokens, TokenGroupNames.CustomerTokens },
                MessageTemplateSystemNames.NewForumPostMessage => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ForumPostTokens, TokenGroupNames.ForumTopicTokens, TokenGroupNames.ForumTokens, TokenGroupNames.CustomerTokens },
                MessageTemplateSystemNames.PrivateMessageNotification => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.PrivateMessageTokens, TokenGroupNames.CustomerTokens },
                MessageTemplateSystemNames.BlogCommentStoreOwnerNotification => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.BlogCommentTokens, TokenGroupNames.CustomerTokens },
                MessageTemplateSystemNames.NewsCommentStoreOwnerNotification => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.NewsCommentTokens, TokenGroupNames.CustomerTokens },
                MessageTemplateSystemNames.ContactUsMessage => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ContactUs },
                _ => Array.Empty<string>(),
            };
        }

        #endregion
    }
}