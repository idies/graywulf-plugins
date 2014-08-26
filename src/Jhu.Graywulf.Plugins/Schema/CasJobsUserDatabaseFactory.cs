using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jhu.Graywulf.Registry;
using Jhu.Graywulf.Schema;

namespace Jhu.Graywulf.Schema
{
    public class CasJobsUserDatabaseFactory : UserDatabaseFactory
    {
        #region Private member variables

        private CasJobsSettings settings;

        #endregion
        #region Properties

        public CasJobsSettings Settings
        {
            get { return settings; }
        }

        #endregion
        #region Constructors and initializers

        public CasJobsUserDatabaseFactory(Federation federation)
            :base(federation)
        {
            InitializeMembers();

            settings = (CasJobsSettings)federation.Settings["CasJobsSettings"].Value;
        }

        private void InitializeMembers()
        {
            this.settings = new CasJobsSettings();
        }

        #endregion

        public override void EnsureUserDatabaseExists(User user)
        {
            throw new NotImplementedException();
        }

        public override DatasetBase GetUserDatabase(User user)
        {
            throw new NotImplementedException();
        }
    }
}
