using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Jhu.Graywulf.Schema
{
    public class CasJobsSettings
    {
        #region Private member variables

        #endregion
        #region Properties

        #endregion
        #region Constructors and initializers

        public CasJobsSettings()
        {
            InitializeMembers(new StreamingContext());
        }

        [OnDeserializing]
        private void InitializeMembers(StreamingContext context)
        {
        }

        #endregion
    }
}
