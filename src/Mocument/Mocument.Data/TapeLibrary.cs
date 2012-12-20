using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using Fiddler;
using Mocument.Data.Transcoders;
using Newtonsoft.Json;

namespace Mocument.Data
{
    public class TapeLibrary
    {
        private static readonly object LockTarget = new object();

        private readonly ObjectCache _cache = MemoryCache.Default;

        public TapeLibrary(string path)
        {
            Path = path;
            Tapes = new List<Tape>();
        }

        [JsonProperty]
        protected List<Tape> Tapes { get; set; }

        [JsonIgnore]
        public string Path { get; private set; }

        public int Count
        {
            get { return Tapes.Count; }
        }

        public void Load()
        {
            if (File.Exists(Path))
            {
                string json = File.ReadAllText(Path);
                var c = JsonConvert.DeserializeObject<List<Tape>>(json);
                Tapes = c;
            }
        }

        public void Save()
        {
            lock (LockTarget)
            {
                string json = JsonConvert.SerializeObject(Tapes, Formatting.Indented);
                File.WriteAllText(Path, json);
            }
        }

        public Tape GetOrAddTape(string tapeId, string description = "", bool openForRecording = false, string allowedIpAddress = "")
        {
            lock (LockTarget)
            {
                Tape tape2 = FindTape(tapeId);
                if (tape2 == null)
                {
                    tape2 = new Tape { Id = tapeId, Description = description, AllowedIpAddress = allowedIpAddress, OpenForRecording = openForRecording };
                    Tapes.Add(tape2);
                    Save();
                }
                return tape2;
            }
        }
        public Tape FindTape(string tapeId)
        {
            lock (LockTarget)
            {

                return Tapes.FirstOrDefault(t => string.Compare(tapeId, t.Id, StringComparison.OrdinalIgnoreCase) == 0);

            }
        }
        public Tape GetTape(string tapeId)
        {
            lock (LockTarget)
            {
                Tape tape = FindTape(tapeId);
                if (tape == null )
                {
                    throw new Exception("Tape not found: " + tapeId);
                }
                return tape;
            }
        }

        public void DeleteTape(string tapeId)
        {
            lock (LockTarget)
            {
                Tape tape = GetTape(tapeId);
                SetTapeContents(tapeId, null);
                Tapes.Remove(tape);
            }
        }



        public List<Session> GetTapeContents(string tapeId)
        {
            lock (LockTarget)
            {
                Tape tape = GetTape(tapeId);
                if (tape == null)
                {
                    throw new Exception("Tape does not exist: " + tapeId);
                }

                var results = new List<Session>();
                if (_cache.Contains(tapeId))
                {
                    results = (List<Session>)_cache[tapeId];
                }
                else
                {
                    string path = GetTapeContentsPath(tapeId);
                    if (File.Exists(path))
                    {
                        results = ImportTape(path);
                    }
                    _cache[tapeId] = results;
                }
                return results;
            }
        }

        public void SetTapeContents(string tapeId, List<Session> contents)
        {
            lock (LockTarget)
            {
                Tape tape = GetTape(tapeId);
                if (tape == null)
                {
                    throw new Exception("Tape does not exist: " + tapeId);
                }


                // #TODO: find the Concurrent sync and lock the collectionId

                string path = GetTapeContentsPath(tapeId);

                if (contents == null)
                {
                    File.Delete(path);
                    if (_cache.Contains(tapeId))
                    {
                        _cache.Remove(tapeId);
                    }
                }
                else
                {
                    ExportTape(contents, path);
                    if (_cache.Contains(tapeId))
                    {
                        _cache[tapeId] = contents;
                    }
                }
            }
        }

        private static List<Session> ImportTape(string path)
        {
            lock (LockTarget)
            {
                return HttpArchiveFormat.ImportSessions(path);
            }
        }

        private static void ExportTape(List<Session> contents, string path)
        {
            lock (LockTarget)
            {
                HttpArchiveFormat.ExportSessions(contents, path);
            }
        }

        private string GetTapeContentsPath(string tapeId)
        {
            if (string.IsNullOrEmpty(Path))
            {
                throw new Exception("Path has not been set on library");
            }
            // ReSharper disable AssignNullToNotNullAttribute
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path), tapeId + ".har");
            // ReSharper restore AssignNullToNotNullAttribute
        }
    }
}