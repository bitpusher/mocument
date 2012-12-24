using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using Mocument.Model;
using Salient.HTTPArchiveModel;

namespace Mocument.DataAccess
{
    // #TODO: abstract this into a generic object repository in the salient.namespace
    // #TODO: bring the level of stored structure up one level and store server configuration
    // #        so webui can control the server

    // to prove this concept and avoid wasting time figuring out the concurrent persistence layer
    // i am going to build a data access facade and use a memory mapped file so proxy and management
    // can access without encountering sharing issues or performance hits introduced by the db technology
    // http://blogs.msdn.com/b/salvapatuel/archive/2009/06/08/working-with-memory-mapped-files-in-net-4.aspx
    public static class Context
    {
        public static string Path { get; private set; }

        public static Entry MatchEntry(string tapeId, Entry entryToMatch, IEntryComparer[] comparers = null)
        {
            try
            {
                Mutex.WaitOne();
                Tape tape = GetTapeById(tapeId);

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
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        public static void Open(string path)
        {
            Mutex.WaitOne();
            try
            {
                Path = path;
                Close();

                if (_pagedMemoryMapped != null)
                {
                    return;
                }
                var customSecurity = new MemoryMappedFileSecurity();

                //http://social.msdn.microsoft.com/Forums/en-GB/ncl/thread/3fa10429-e0d6-4f40-b8b4-b306c6d5e5c4  <--- needle in 1,000 haystacks - #TODO: this needs to go on stack overflow
                customSecurity.AddAccessRule(new AccessRule<MemoryMappedFileRights>("everyone", MemoryMappedFileRights.FullControl, AccessControlType.Allow));

                _pagedMemoryMapped = MemoryMappedFile.CreateOrOpen(
                    Path, // Name
                    1024 * 1024, // Size
                    MemoryMappedFileAccess.ReadWrite, // Access type
                    MemoryMappedFileOptions.DelayAllocatePages, // Pseudo reserve/commit
                    customSecurity, // You can customize the security
                    HandleInheritability.Inheritable); // Inherit to child process

                try
                {
                    FetchData();
                }
                catch
                {
                    SaveData(new List<Tape>());
                }
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        public static void Close()
        {
            Mutex.WaitOne();
            try
            {
                if (_pagedMemoryMapped == null)
                {
                    return;
                }
                _pagedMemoryMapped.Dispose();
                _pagedMemoryMapped = null;
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        #region CRUD

        /// <summary>
        /// List all tapes
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">if context is not open</exception>
        public static List<Tape> ListTapes()
        {
            Mutex.WaitOne();
            try
            {
                return FetchData();
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Delete an existing tape by id
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="Exception">if tape does not exist</exception>
        /// <exception cref="Exception">if context is not open</exception>
        public static void DeleteTape(string id)
        {
            Mutex.WaitOne();
            try
            {
                List<Tape> store = FetchData();
                Tape tape = FindTape(id, store);

                if (tape == null)
                {
                    throw new Exception("Tape " + id + " does not exist.");
                }

                store.Remove(tape);
                SaveData(store);
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Get an existing tape by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="throwIfAbsent"> </param>
        /// <returns></returns>
        /// <exception cref="Exception">if tape does not exist and throwIfAbsent is true</exception>
        /// <exception cref="Exception">if context is not open</exception>
        public static Tape GetTapeById(string id, bool throwIfAbsent = true)
        {
            Mutex.WaitOne();
            try
            {
                Tape tape = FindTape(id, FetchData());
                if (tape == null && throwIfAbsent)
                {
                    throw new Exception("Tape " + id + " does not exist.");
                }
                return tape;
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Adds or updates a tape
        /// </summary>
        /// <param name="t"></param>
        /// <exception cref="Exception">if context is not open</exception>
        public static void SaveTape(Tape t)
        {
            Mutex.WaitOne();
            try
            {
                List<Tape> store = FetchData();
                Tape tape = FindTape(t.Id, store);

                if (tape != null)
                {
                    store.Remove(tape);
                }
                store.Add(t);
                SaveData(store);
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        #endregion

        #region Implementation

        private static MemoryMappedFile _pagedMemoryMapped;

        private static readonly Mutex Mutex;

        static Context()
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
            Mutex = new Mutex(false, "MOCUMENT_IPC", out createdNew, security);
        }

        internal static List<Tape> FetchData()
        {
            Mutex.WaitOne();
            try
            {
                EnsureMapIsOpen();

                List<Tape> tapes;
                using (MemoryMappedViewStream mapStream = _pagedMemoryMapped.CreateViewStream(0, 1024 * 1024, MemoryMappedFileAccess.Read))
                {
                    var formatter = new BinaryFormatter();
                    tapes = (List<Tape>)formatter.Deserialize(mapStream);
                }

                return tapes;
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        internal static void SaveData(List<Tape> tapes)
        {
            Mutex.WaitOne();
            try
            {
                EnsureMapIsOpen();
                using (
                    MemoryMappedViewStream mapStream = _pagedMemoryMapped.CreateViewStream(0, 1024 * 1024,
                                                                                           MemoryMappedFileAccess.
                                                                                               Write))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(mapStream, tapes);
                }
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        private static void EnsureMapIsOpen()
        {
            if (_pagedMemoryMapped == null)
            {
                throw new Exception("Context is closed.");
            }
        }

        private static Tape FindTape(string id, IEnumerable<Tape> store)
        {
            return store.FirstOrDefault(t => t.Id == id);
        }

        #endregion
    }
}