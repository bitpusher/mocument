using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using CIAPI.Rpc;
using Mocument.DataAccess;
using Mocument.ReverseProxyServer;
using NUnit.Framework;

namespace CIAPI.RecordedTests.Infrastructure
{
    /// <summary>
    /// We want to be able to switch between record and playback with a config switch
    /// </summary>
    [TestFixture]
    public abstract class CIAPIRecordingFixtureBase
    {
        private MocumentMode _mocumentMode;
        private string _userName;
        private string _password;
        private string _mocumentKey;
        private string _apiUrl;
        private string _apiKey;
        private string _streamingUrl;
        private Server _server;
        private IStore _store;
        private string _mocumentDataPath;
        protected internal string UserName
        {
            get { return _userName; }
        }

        protected internal string Password
        {
            get { return _password; }
        }

        [TestFixtureTearDown]
        protected void FixtureTeardown()
        {
            if (null != _server)
            {
                _server.Stop();
            }
        }

        [TestFixtureSetUp]
        protected void FixtureSetup()
        {
            object[] att = GetType().GetCustomAttributes(typeof(MocumentModeOverrideAttribute), true);

            _mocumentMode = att.Length > 0
                                ? ((MocumentModeOverrideAttribute)att[0]).Mode
                                : ((MocumentMode)Enum.Parse(typeof(MocumentMode), ConfigurationManager.AppSettings["mocumentMode"], true));

            switch (_mocumentMode)
            {
                case MocumentMode.Record:
                case MocumentMode.Play:
                    break;
                default:
                    throw new ConfigurationErrorsException("invalid Mocument.Mode. expect record or play");
            }

            _mocumentDataPath = ConfigurationManager.AppSettings["mocumentDataPath"];
            _mocumentDataPath = Path.GetFullPath(_mocumentDataPath);
            _mocumentKey = ConfigurationManager.AppSettings["mocumentKey"];
            _userName = ConfigurationManager.AppSettings["apiUserName"];
            _password = ConfigurationManager.AppSettings["apiPassword"];
            _apiUrl = ConfigurationManager.AppSettings["apiRpcUrl"];
            _streamingUrl = ConfigurationManager.AppSettings["apiStreamingUrl"];
            _apiKey = ConfigurationManager.AppSettings["apiKey"];
            _server = CreateServer();
            _server.Start();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private Uri BuildUri()
        {
            string tapeId = new StackFrame(2, true).GetMethod().Name;
            string api = _apiUrl.Substring(new Uri(_apiUrl).Scheme.Length + 3);
            string url = "http://localhost.:" + _server.Port + "/" + _mocumentMode.ToString().ToLower() + "/" + _mocumentKey + "/" + tapeId +
                         "/" + api;
            return new Uri(url);
        }

        private IStore CreateStore()
        {
            if (!Directory.Exists(_mocumentDataPath))
            {
                Directory.CreateDirectory(_mocumentDataPath);
            }
            string typename = GetType().Name;


            string path = Path.Combine(_mocumentDataPath, typename + ".mit");
            switch (_mocumentMode)
            {
                case MocumentMode.Record:
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    break;
                case MocumentMode.Play:
                    if (!File.Exists(path))
                    {
                        throw new FileNotFoundException("could not open data file for fixture", path);
                    }
                    break;
                default:
                    throw new ConfigurationErrorsException("invalid Mocument.Mode. expect record or play");
            }
            _store = new JsonFileStore(path);
            return _store;
        }


        private Server CreateServer()
        {
            int port = Server.GetAvailablePort(32000, 33000, IPAddress.Loopback, false);

            IStore store = CreateStore();
            var server = new Server(port, false, store);
            return server;
        }

        protected Client BuildRpcClient()
        {
            // WARNING: do not nest or otherwise refactor this method
            // buildUri is looking back 2 stack frames to get the method that called this


            var rpcClient = new Client(BuildUri(), new Uri(_streamingUrl), _apiKey);
            rpcClient.LogIn(_userName, _password);
            return rpcClient;
        }
        protected Client BuildUnauthenticatedRpcClient()
        {
            // WARNING: do not nest or otherwise refactor this method
            // buildUri is looking back 2 stack frames to get the method that called this

            var rpcClient = new Client(BuildUri(), new Uri(_streamingUrl), _apiKey);
            return rpcClient;
        }
    }
}