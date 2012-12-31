using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using Mocument.DataAccess.File.IPC;
using Mocument.Model;
using Newtonsoft.Json;

namespace Mocument.DataAccess.File
{
    /// <summary>
    /// client has read-only access to data file. modifications must be sent as IPC requests to the proxy server
    /// </summary>
    public class StoreClient : IDisposable
    {
        private readonly IpcChannel _ipc;
        private readonly string _path;
        private Catalog _catalog;
        private bool _disposed;
        private Mutex _mutex;
        public StoreClient(string path)
        {
            _path = path;
            CreateMutex();
            _catalog = new Catalog();
            _ipc = IpcChannel.Create("MOCUMENT_IPC_" + Store.CalculateMD5Hash(_path.ToLower()), IpcChannelSide.Low);
            Load();
        }

        /// <summary>
        /// Creates a systemwide mutex to enforce one instance and for use in thread access syncronization
        /// </summary>
        private void CreateMutex()
        {
            // http://stackoverflow.com/questions/663332/localized-group-name
            var rule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                                           MutexRights.Modify
                                           | MutexRights.Synchronize
                                           | MutexRights.TakeOwnership
                                           | MutexRights.ReadPermissions,
                                           AccessControlType.Allow);
            var security = new MutexSecurity();
            security.AddAccessRule(rule);
            bool createdNew;
            _mutex = new Mutex(false, "MOCUMENT_MUTEX_" + Store.CalculateMD5Hash(_path.ToLower()), out createdNew, security);
       
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
        }

        #endregion

        public void Delete(string id)
        {
            lock (_catalog)
            {
                _ipc.SendMessage(new IPCMessage
                                     {
                                         Tape = new Tape { Id = id },
                                         Type = IPCMessageType.DeleteTape
                                     });
                Load();
            }
        }

        public void Add(Tape tape)
        {
            lock (_catalog)
            {
                _ipc.SendMessage(new IPCMessage
                                     {
                                         Tape = tape,
                                         Type = IPCMessageType.AddTape
                                     });
                Load();
            }
        }

        public void Update(Tape tape)
        {
            lock (_catalog)
            {
                _ipc.SendMessage(new IPCMessage
                                     {
                                         Tape = tape,
                                         Type = IPCMessageType.UpdateTape
                                     });
                Load();
            }
        }

        public Tape Select(string id)
        {
            Tape existing = _catalog.Tapes.FirstOrDefault(t => t.Id == id);
            return existing;
        }

        public Tape SelectIndex(string id)
        {
            Tape existing = _catalog.Tapes.FirstOrDefault(t => t.Id == id);
            return existing == null
                       ? null
                       : new Tape
                             {
                                 AllowedIpAddress = existing.AllowedIpAddress,
                                 Comment = existing.Comment,
                                 Description = existing.Description,
                                 Id = existing.Id,
                                 OpenForRecording = existing.OpenForRecording
                             };
        }


        public IEnumerable<Tape> SelectIndex()
        {
            return _catalog.Tapes.Select(t => new Tape
                                                  {
                                                      AllowedIpAddress = t.AllowedIpAddress,
                                                      Comment = t.Comment,
                                                      Description = t.Description,
                                                      Id = t.Id,
                                                      OpenForRecording = t.OpenForRecording
                                                  }).ToList();
        }

        public IEnumerable<Tape> Select()
        {
            return _catalog.Tapes.ToList();
        }

        public IEnumerable<Tape> SelectIndex(Func<Tape, bool> criteria)
        {
            return _catalog.Tapes.Where(criteria).Select(t => new Tape
                                                                  {
                                                                      AllowedIpAddress = t.AllowedIpAddress,
                                                                      Comment = t.Comment,
                                                                      Description = t.Description,
                                                                      Id = t.Id,
                                                                      OpenForRecording = t.OpenForRecording
                                                                  }).ToList();
        }

        public IEnumerable<Tape> Select(Func<Tape, bool> criteria)
        {
            return _catalog.Tapes.Where(criteria).ToList();
        }

        public void Load()
        {
            lock (_catalog)
            {
                try
                {
                    _mutex.WaitOne();
                    string json;
                    using (FileStream stream = FileExt.WaitAndOpenFileForReading(_path, TimeSpan.FromMilliseconds(1000)))
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            json = reader.ReadToEnd();
                        }
                    }
                    _catalog = JsonConvert.DeserializeObject<Catalog>(json);

                }
                finally
                {
                    _mutex.ReleaseMutex();
                }
            }
        }
    }
}