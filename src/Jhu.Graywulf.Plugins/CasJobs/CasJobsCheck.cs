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
        public CasJobsCheck()
        {
        }

        public override void Execute(System.IO.TextWriter output)
        {
            output.WriteLine("Testing CasJobs configuration...");
            
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
        }

        public override IEnumerable<CheckRoutineBase> GetCheckRoutines()
        {
            yield return new DatabaseCheck(CasJobsClient.Configuration.BatchAdminConnectionString);
        }
    }
}
