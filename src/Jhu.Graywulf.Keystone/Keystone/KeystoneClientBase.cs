using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Jhu.Graywulf.Components;
using Jhu.Graywulf.SimpleRestClient;

namespace Jhu.Graywulf.Keystone
{
    /// <summary>
    /// Implements core function to be used with services interacting
    /// with Keystone or any other service supporting Keystone.
    /// </summary>
    public abstract class KeystoneClientBase : RestClient
    {
        #region Constructors and initializers

        protected KeystoneClientBase()
        {
            InitializeMembers();
        }

        protected KeystoneClientBase(Uri baseUri)
            : base(baseUri)
        {
            InitializeMembers();
        }

        private void InitializeMembers()
        {
        }

        #endregion
        #region Specialized REST functions for Keystone

        protected RestHeaderCollection PreprocessHeaders(RestHeaderCollection headers, Token authToken)
        {
            if (headers == null)
            {
                headers = new RestHeaderCollection();
            }

            // Add authentication token
            headers.Set(new RestHeader(Constants.KeystoneXAuthTokenHeader, authToken.ID));

            return headers;
        }

        protected RestMessage<T> PreprocessHeaders<T>(RestMessage<T> message, Token authToken)
        {
            message.Headers = PreprocessHeaders(message.Headers, authToken);

            return message;
        }

        protected RestHeaderCollection SendRequest(HttpMethod method, string path, Token authToken)
        {
            return SendRequest(method, path, (RestHeaderCollection)null, authToken);
        }

        protected RestHeaderCollection SendRequest(HttpMethod method, string path, RestHeaderCollection headers, Token authToken)
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

        protected RestMessage<R> SendRequest<R>(HttpMethod method, string path, Token authToken)
        {
            return SendRequest<R>(method, path, (RestHeaderCollection)null, authToken);
        }

        protected RestMessage<R> SendRequest<R>(HttpMethod method, string path, RestHeaderCollection headers, Token authToken)
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

        protected RestHeaderCollection SendRequest<T>(HttpMethod method, string path, RestMessage<T> message, Token authToken)
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

        protected RestMessage<U> SendRequest<T, U>(HttpMethod method, string path, RestMessage<T> message, Token authToken)
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

        #endregion
    }
}
