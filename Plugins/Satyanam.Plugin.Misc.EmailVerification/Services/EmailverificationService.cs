using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.EmailVerification.Services
{
    /// <summary>
    /// Represents service shipping by weight service implementation
    /// </summary>
    public class EmailverificationService : IEmailverificationService
    {
        #region Constants



        #endregion

        #region Fields
        private readonly EmailVerificationSettings _emailVerificationSettings;

        #endregion

        #region Ctor

        public EmailverificationService(EmailVerificationSettings emailVerificationSettings)
        {
            _emailVerificationSettings = emailVerificationSettings;
        }

        #endregion

        #region Methods

        public async Task<string> VerifyEmailApi(string replyToEmailAddress)
        {
            string responseString = string.Empty;

            try
            {
                string apiKey = _emailVerificationSettings.ApiKey;
                string apiUrl = $"https://api.quickemailverification.com/v1/verify?email={replyToEmailAddress}&apikey={apiKey}";
                WebRequest request = WebRequest.Create(apiUrl);
                request.Method = "GET";

                using (WebResponse response = await request.GetResponseAsync())
                using (StreamReader ostream = new StreamReader(response.GetResponseStream()))
                {
                    responseString = await ostream.ReadToEndAsync();
                }
            }
            catch (WebException webEx)
            {
                if (webEx.Response is HttpWebResponse httpResponse && (int)httpResponse.StatusCode == 402)
                {
                    // Session expired or limit exceeded
                    return "__SESSION_EXPIRED__";
                }

                return null; // Generic failure
            }
            catch
            {
                return null;
            }

            return responseString;
        }



        #endregion
    }
}
