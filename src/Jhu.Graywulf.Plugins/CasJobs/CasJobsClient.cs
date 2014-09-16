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
            /*KeystoneException kex = null;
            var error = DeserializeJson<ErrorResponse>(ex.Body);

            if (error != null)
            {
                kex = new KeystoneException(error.Error.Message, ex)
                {
                    Title = error.Error.Title,
                    StatusCode = (HttpStatusCode)error.Error.Code
                };
            }
            else if (ex.InnerException is WebException)
            {
                kex = new KeystoneException(ex.Message, ex)
                {
                    StatusCode = ((HttpWebResponse)((WebException)ex.InnerException).Response).StatusCode
                };
            }
            else
            {
                kex = new KeystoneException(ex.Message, ex);
            }

            return kex;*/

            throw new NotImplementedException();
        }

        #endregion
    }
}
