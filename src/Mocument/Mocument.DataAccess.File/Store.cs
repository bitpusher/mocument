using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Mocument.DataAccess.File.IPC;
using Mocument.Model;
using Newtonsoft.Json;
using Salient.HTTPArchiveModel;

namespace Mocument.DataAccess.File
{
    public class Store : IDisposable
    {
        private readonly IpcChannel _ipc;
        private readonly string _path;
        private Catalog _catalog;
        private bool _disposed;
        private Mutex _mutex;

        public Store(string path)
        {
            _path = path;
            _catalog = new Catalog();
            CreateMutex();
            _ipc = IpcChannel.Create("MOCUMENT_IPC_" + CalculateMD5Hash(_path.ToLower()), IpcChannelSide.High);
            _ipc.DataReceived += IPCDataReceived;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _mutex.Dispose();
            _mutex = null;
        }

        #endregion

        public static IpcChannel CreateIpcChannel(string path)
        {
            IpcChannel channel = IpcChannel.Create("MOCUMENT_IPC_" + path.ToLower(), IpcChannelSide.Low);
            return channel;
        }


        /// <summary>
        /// The only reason we get an IPC message is because the admin UI wants to modify the
        /// tape store. Add, Delete or Modify a tape. The catalog is locked for the duration of this
        /// event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void IPCDataReceived(object sender, IpcDataRecievedEventArgs e)
        {
            lock (_catalog)
            {
                Tape tape = e.Message.Tape;
                switch (e.Message.Type)
                {
                    case IPCMessageType.UpdateTape:
                        Update(tape);
                        break;
                    case IPCMessageType.AddTape:
                        Add(tape);
                        break;
                    case IPCMessageType.DeleteTape:
                        Delete(tape.Id);
                        break;
                    case IPCMessageType.Reload:
                        Load();
                        break;
                }
                e.Message.Accepted = true;
            }
        }

        public void Delete(string id)
        {
            lock (_catalog)
            {
                Tape existing = _catalog.Tapes.FirstOrDefault(t => t.Id == id);
                if (existing == null)
                {
                    throw new Exception("tape " + id + " not found");
                }
                _catalog.Tapes.Remove(existing);
                Save();
            }
        }

        public void Add(Tape tape)
        {
            lock (_catalog)
            {
                Tape existing = _catalog.Tapes.FirstOrDefault(t => t.Id == tape.Id);
                if (existing != null)
                {
                    throw new Exception("tape " + tape.Id + " already exists");
                }
                _catalog.Tapes.Add(tape);
                Save();
            }
        }

        public void Update(Tape tape)
        {
            lock (_catalog)
            {
                Tape existing = _catalog.Tapes.FirstOrDefault(t => t.Id == tape.Id);
                if (existing == null)
                {
                    throw new Exception("tape " + tape.Id + " not found");
                }
                _catalog.Tapes.Remove(existing);
                _catalog.Tapes.Add(tape);
                Save();
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
                string json;
                if (!System.IO.File.Exists(_path))
                {
                    System.IO.FileStream afile = System.IO.File.Create(_path);
                    afile.Close();
                    afile = null;
                    Thread.Sleep(500);
                    Save();
                }
             
                using (FileStream stream = FileExt.WaitAndOpenFileForReading(_path, TimeSpan.FromMilliseconds(10000)))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        json = reader.ReadToEnd();
                        reader.Close();
                        reader.Dispose();
                    }
                }
                _catalog = JsonConvert.DeserializeObject<Catalog>(json);
            }
        }

        public void Save()
        {
            lock (_catalog)
            {
                string json = JsonConvert.SerializeObject(_catalog,Formatting.Indented);
                using (FileStream stream = FileExt.WaitAndOpenFileForWriting(_path, TimeSpan.FromMilliseconds(10000)))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(json);
                        writer.Flush();
                        writer.Close();
                        writer.Dispose();
                    }
                }
            }
        }
        public Entry MatchEntry(string tapeId, Entry entryToMatch, IEntryComparer[] comparers = null)
        {

            lock (_catalog)
            {


                Tape tape = Select(tapeId);

                // provide a default comparer
                if (comparers == null || comparers.Length == 0)
                {
                    comparers = new IEntryComparer[] { new DefaultEntryComparer() };
                }

                var potentialMatches = tape.log.entries;
                return (
                           from entryComparer in comparers
                           select entryComparer.FindMatch(potentialMatches, entryToMatch)
                               into result
                               where result.Match != null
                               select result.Match)
                    .FirstOrDefault();
            }

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
            _mutex = new Mutex(false, "MOCUMENT_MUTEX_" + CalculateMD5Hash(_path.ToLower()), out createdNew, security);
     
        }


        public static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}