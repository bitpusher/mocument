using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Mocument.DataAccess;
using NUnit.Framework;
using Mocument.IPC;
namespace Mocument.ReverseProxyServer.Tests
{
    [TestFixture]
    public class ReverseProxyServerFixture
    {

        [Test]
        public void ProxyCanRecordAndPlaybackOpen()
        {
            // fire up a proxy server, record a request, check the store and then replay it

            int port = 81;
            string contextName = "Mocument";
            Server server = null;
            try
            {
                server = new Server(contextName, port, false,"Mocument_IPC",IpcChannelSide.Low);
                server.Start();
                const string postData = "bar=foo";
                var recordAddress = "http://localhost:" + port + "/record/sky/httpbin/httpbin.org/post?foo=bar";
                string response1 = new WebClient().UploadString(recordAddress, postData);

                // introduce some delay to let the server store the tape. this is not contrived....
                Thread.Sleep(1000);

                var tapes = Context.ListTapes();
                Assert.AreEqual(1, tapes.Count);
                Assert.AreEqual("sky.httpbin", tapes[0].Id);
                Assert.AreEqual(1, tapes[0].log.entries.Count);

                // #TODO: we need to add something unobtrusive to the replayed response to make it clear it was canned
                var playAddress = "http://localhost:" + port + "/play/sky/httpbin/httpbin.org/post?foo=bar";
                string response2 = new WebClient().UploadString(playAddress, postData);
                Thread.Sleep(1000);
                Assert.AreEqual(response1, response2);

            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
            finally
            {
                if (server != null)
                {
                    server.Stop();
                }
            }


        }

        
    }
}
