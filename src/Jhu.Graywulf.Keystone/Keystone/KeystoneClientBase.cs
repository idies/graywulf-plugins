using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Jhu.Graywulf.SimpleRestClient;

namespace Jhu.Graywulf.Keystone
{
    public abstract class KeystoneClientBase : RestClient
    {
        #region Private member variables

        private string adminAuthToken;
        private string userAuthToken;

        #endregion
        #region Properties

        public string AdminAuthToken
        {
            get { return adminAuthToken; }
            set { adminAuthToken = value; }
        }

        public string UserAuthToken
        {
            get { return userAuthToken; }
            set { userAuthToken = value; }
        }

        #endregion
        #region Constructors and initializers

        protected KeystoneClientBase(Uri baseUri)
            : base(baseUri)
        {
        }

        #endregion

        protected RestHeaderCollection PreprocessHeaders(RestHeaderCollection headers, string authToken)
        {
            if (headers == null)
            {
                headers = new RestHeaderCollection();
            }

            // Add authentication token
            headers.Set(new RestHeader(Constants.KeystoneXAuthTokenHeader, authToken));

            return headers;
        }

        protected RestMessage<T> PreprocessHeaders<T>(RestMessage<T> message, string authToken)
        {
            message.Headers = PreprocessHeaders(message.Headers, authToken);

            return message;
        }

        protected RestHeaderCollection SendRequest(HttpMethod method, string path, string authToken)
        {
            return SendRequest(method, path, (RestHeaderCollection)null, authToken);
        }

        protected RestHeaderCollection SendRequest(HttpMethod method, string path, RestHeaderCollection headers, string authToken)
        {
            try
            {
                return base.SendRequest(method, path, PreprocessHeaders(headers, authToken));
            }
            catch (RestException ex)
            {
                throw CreateException(ex);
            }
        }

        protected RestMessage<R> SendRequest<R>(HttpMethod method, string path, string authToken)
        {
            return SendRequest<R>(method, path, (RestHeaderCollection)null, authToken);
        }

        protected RestMessage<R> SendRequest<R>(HttpMethod method, string path, RestHeaderCollection headers, string authToken)
        {
            try
            {
                return base.SendRequest<R>(method, path, PreprocessHeaders(headers, authToken));
            }
            catch (RestException ex)
            {
                throw CreateException(ex);
            }
        }

        protected RestHeaderCollection SendRequest<T>(HttpMethod method, string path, RestMessage<T> message, string authToken)
        {
            try
            {
                return base.SendRequest<T>(method, path, PreprocessHeaders(message, authToken));
            }
            catch (RestException ex)
            {
                throw CreateException(ex);
            }
        }

        protected RestMessage<U> SendRequest<T, U>(HttpMethod method, string path, RestMessage<T> message, string authToken)
        {
            try
            {
                return base.SendRequest<T, U>(method, path, PreprocessHeaders(message, authToken));
            }
            catch (RestException ex)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif

                throw CreateException(ex);
            }
        }

        protected abstract Exception CreateException(RestException ex);
    }
}
