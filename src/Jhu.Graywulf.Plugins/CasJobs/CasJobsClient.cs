using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Configuration;
using Jhu.Graywulf.SimpleRestClient;
using Jhu.Graywulf.Keystone;

namespace Jhu.Graywulf.CasJobs
{
    public class CasJobsClient : KeystoneClientBase
    {
        #region Static members
        public static CasJobsClientConfiguration Configuration
        {
            get
            {
                return (CasJobsClientConfiguration)ConfigurationManager.GetSection("Jhu.Graywulf/casJobs");
            }
        }

        #endregion
        #region Private member variables

        private KeystoneClient keystoneClient;

        #endregion
        #region Properties

        #region Properties

        public KeystoneCredentials AdminCredentials
        {
            get { return keystoneClient.AdminCredentials; }
            set { keystoneClient.AdminCredentials = value; }
        }

        /// <summary>
        /// Gets or sets the token used for user authentication
        /// </summary>
        public KeystoneCredentials UserCredentials
        {
            get { return keystoneClient.UserCredentials; }
            set { keystoneClient.UserCredentials = value; }
        }

        #endregion

        #endregion
        #region Constructors and initializers

        public CasJobsClient(KeystoneClient keystoneClient)
            :base(Configuration.BaseUri)
        {
            this.keystoneClient = keystoneClient;
        }

        #endregion
        #region Client functions

        public User GetUser(string userID)
        {
            var res = SendRequest<User>(
                HttpMethod.Get, String.Format("/users/{0}", userID), keystoneClient.GetAdminToken());

            return res.Body;
        }

        public void Create(string keystoneUserID, User user)
        {
            var req = new RestMessage<User>(user);
            var res = SendRequest<User>(
                HttpMethod.Put, String.Format("/users/{0}", keystoneUserID), req, keystoneClient.GetAdminToken());
        }

        public void Submit(string context, Query query)
        {
            var req = new RestMessage<Query>(query);
            var res = SendRequest<Query>(
                HttpMethod.Post, String.Format("/contexts/{0}/query", context), req, keystoneClient.GetUserToken());
        }

        #endregion
        #region Utility functions

        protected override Exception CreateException(RestException ex)
        {
            CasJobsException cjex = null;
            var error = DeserializeJson<ErrorResponse>(ex.Body);

            if (error != null)
            {
                cjex = new CasJobsException(error.Message, ex)
                {
                    StatusCode = (HttpStatusCode)error.StatusCode,
                    ErrorType = error.ErrorType,
                    LogMessageID = error.LogMessageID
                };
            }
            else if (ex.InnerException is WebException)
            {
                cjex = new CasJobsException(ex.Message, ex)
                {
                    StatusCode = ((HttpWebResponse)((WebException)ex.InnerException).Response).StatusCode
                };
            }
            else
            {
                cjex = new CasJobsException(ex.Message, ex);
            }

            return cjex;
        }

        #endregion
    }
}
