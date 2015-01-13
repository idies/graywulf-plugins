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
        public override void Execute(System.IO.TextWriter output)
        {
            if (!KeystoneAuthentication.Configuration.IsEnabled)
            {
                output.WriteLine("Keystone authentication is not enabled");
            }
            else
            {
                output.WriteLine("Testing Keystone configuration...");
                var kclient = new Keystone.KeystoneClient();
                kclient.GetDomain("default");

                output.WriteLine("Keystone configuration appears to be correct.");
            }
        }

        public override IEnumerable<CheckRoutineBase> GetCheckRoutines()
        {
            yield break;
        }
    }
}
