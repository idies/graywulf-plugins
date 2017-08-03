using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Jhu.Graywulf.SimpleRestClient
{
    [Serializable]
    public class RestException : Exception
    {
        private string method;
        private Uri uri;
        private WebHeaderCollection headers;
        private string requestBody;
        private string responseBody;

        public string RequestMethod
        {
            get { return method; }
            internal set { method = value; }
        }

        public Uri RequestUri
        {
            get { return uri; }
            internal set { uri = value; }
        }

        public WebHeaderCollection RequestHeaders
        {
            get { return headers; }
            internal set { headers = value; }
        }

        public string RequestBody
        {
            get { return requestBody; }
            internal set { requestBody = value; }
        }

        public string ResponseBody
        {
            get { return responseBody; }
            internal set { responseBody = value; }
        }
        

        public string CurlCommand
        {
            get
            {
                var cmd = new StringBuilder();

                cmd.AppendFormat("curl -X {0}", method.ToUpper());
                cmd.Append(" \"" + uri.ToString() + "\"");

                for (int i = 0; i < headers.Count; ++i)
                {
                    string header = headers.GetKey(i);
                    foreach (string value in headers.GetValues(i))
                    {
                        cmd.Append(" -H \"" + header + ": " + value + "\"");
                    }
                }

                if (!String.IsNullOrWhiteSpace(requestBody))
                {
                    cmd.Append(" -d '");
                    cmd.Append(requestBody);
                    cmd.Append("'");
                }

                cmd.Replace('\r', ' ');
                cmd.Replace('\n', '\\');

                return cmd.ToString();
            }
        }

        public RestException()
        {
        }

        public RestException(string message)
            :base(message)
        {
        }

        public RestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
