using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Mocument.IPC.Tests
{
    [TestFixture]
    public class IpcChannelFixture
    {
        [Test]
        public void CanChannelsTalk()
        {
            var high = IpcChannel.Create("foo", IpcChannelSide.High);
            var low = IpcChannel.Create("foo", IpcChannelSide.Low);
            try
            {
                var gateHigh = new AutoResetEvent(false);
                var gateLow = new AutoResetEvent(false);
                string highGot = null;
                string lowGot = null;
                const string highmessage = "hey low";
                const string lowmessage = "hey high";
                high.DataReceived += (sender, e) =>
                                         {
                                             highGot = e.Message;
                                             Console.WriteLine(highGot);
                                             high.SendMessage(highmessage);
                                             gateHigh.Set();
                                         };
                low.DataReceived += (sender, e) =>
                                        {
                                            lowGot = e.Message;
                                            Console.WriteLine(lowGot);
                                            gateLow.Set();
                                        };

                // kick it off
                low.SendMessage(lowmessage);

                gateHigh.WaitOne();
                gateLow.WaitOne();
                Assert.AreEqual(lowmessage,highGot);
                Assert.AreEqual(highmessage,lowGot);
            }
            finally
            {
                low.Dispose();
                high.Dispose();
            }
            
        }

        
    }
}
