using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jhu.Graywulf.Check;
using Jhu.Graywulf.Web.Check;

namespace Jhu.Graywulf.CasJobs
{
    public class CasJobsCheck : CheckRoutineBase
    {
        public override CheckCategory Category
        {
            get { return CheckCategory.Plugin; }
        }

        public CasJobsCheck()
        {
        }

        protected override IEnumerable<CheckRoutineStatus> OnExecute()
        {
            yield return ReportInfo("Testing CasJobs configuration...");

            // TODO what to do here? Would be great to ping service
            // we just look for an exception then

            try
            {
                var ksclient = new Keystone.KeystoneClient();
                var csclient = new CasJobsClient(ksclient);

                csclient.GetUser("0");
            }
            catch (CasJobsException)
            {
                // Eat only casjobs exception, everything else means wrong config
            }

            yield return ReportSuccess("OK");
        }

        protected override IEnumerable<CheckRoutineBase> OnGetCheckRoutines()
        {
            yield return new DatabaseCheck(CasJobsClient.Configuration.BatchAdminConnectionString);
        }
    }
}
