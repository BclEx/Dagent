using System;
using System.Threading;
namespace Contoso.Bus.NugetMessages
{
    public class DeployReply
    {
        public class WaitState
        {
            internal ManualResetEvent WaitEvent = new ManualResetEvent(false);

            public Action<string> Success { get; set; }
            public Action Failure { get; set; }

            public void DoWait()
            {
                if (!WaitEvent.WaitOne(TimeSpan.FromSeconds(30), false))
                    Failure();
            }
        }

        public string Body { get; set; }
    }
}