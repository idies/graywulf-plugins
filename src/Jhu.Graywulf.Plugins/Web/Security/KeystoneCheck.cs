using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jhu.Graywulf.Check;
using Jhu.Graywulf.Web.Check;

namespace Jhu.Graywulf.Web.Security
{
    public class KeystoneCheck : CheckRoutineBase
    {
        public override CheckCategory Category
        {
            get { return CheckCategory.Plugin; }
        }

        protected override IEnumerable<CheckRoutineStatus> OnExecute()
        {
            if (!KeystoneAuthentication.Configuration.IsEnabled)
            {
                yield return ReportSuccess("Keystone authentication is not enabled");
            }
            else
            {
                yield return ReportInfo("Testing Keystone configuration...");
                var kclient = new Keystone.KeystoneClient();
                kclient.GetDomain("default");

                yield return ReportSuccess("Keystone configuration appears to be correct.");
            }
        }

        protected override IEnumerable<CheckRoutineBase> OnGetCheckRoutines()
        {
            yield break;
        }
    }
}
