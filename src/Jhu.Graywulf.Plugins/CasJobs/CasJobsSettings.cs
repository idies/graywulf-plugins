using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Jhu.Graywulf.CasJobs
{
    public class CasJobsSettings
    {
        #region Private member variables

        private Uri restServiceBaseUri;
        private string sqlUsername;
        private string sqlPassword;

        #endregion
        #region Properties

        public Uri RestServiceBaseUri
        {
            get { return restServiceBaseUri; }
            set { restServiceBaseUri = value; }
        }

        public string SqlUserName
        {
            get { return sqlUsername; }
            set { sqlUsername = value; }
        }

        public string SqlPassword
        {
            get { return sqlPassword; }
            set { sqlPassword = value; }
        }

        #endregion
        #region Constructors and initializers

        public CasJobsSettings()
        {
            InitializeMembers(new StreamingContext());
        }

        [OnDeserializing]
        private void InitializeMembers(StreamingContext context)
        {
            this.restServiceBaseUri = null;
            this.sqlUsername = null;
            this.sqlPassword = null;
        }

        #endregion
    }
}
