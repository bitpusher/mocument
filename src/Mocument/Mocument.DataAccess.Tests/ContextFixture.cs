using System;
using System.Collections.Generic;
using Mocument.Model;
using NUnit.Framework;

namespace Mocument.DataAccess.Tests
{
    [TestFixture]
    public class ContextFixture
    {
        private bool CompareTapeStores(List<Tape> tapes, List<Tape> tapes2)
        {
            if (tapes.Count != tapes2.Count)
            {
                return false;
            }
            return true;
        }

        [Test, ExpectedException(typeof (Exception), ExpectedMessage = "Tape foo does not exist.")]
        public void CannotDeleteNonexistentTape()
        {
            try
            {
                Context.Open("Mocument");
                Context.DeleteTape("foo");
            }
            finally
            {
                Context.Close();
            }
        }

        [Test, ExpectedException(typeof (Exception), ExpectedMessage = "Context is closed.")]
        public void CannotUseClosedContext()
        {
            Context.Close(); // just to be sure
            Context.GetTapeById("a");
        }

        [Test]
        public void ContextCanAddFindReplace()
        {
            Context.Open("Mocument");
            try
            {
                
                Tape nullTape;
                Assert.Throws<Exception>(() => nullTape = Context.GetTapeById("a"));


                nullTape = new Tape
                               {
                                   Id = "a"
                               };
                Context.SaveTape(nullTape);
                nullTape = Context.GetTapeById("a");
                Assert.AreEqual("a", nullTape.Id);
                Assert.IsNull(nullTape.Comment);
                nullTape.Comment = "c";
                Context.SaveTape(nullTape);

                nullTape = Context.GetTapeById("a");
                Assert.AreEqual("a", nullTape.Id);
                Assert.AreEqual("c", nullTape.Comment);
            }
            finally
            {
                Context.Close();
            }
        }

        [Test]
        public void ContextCanDelete()
        {
            Context.Open("Mocument");
            try
            {
                List<Tape> tapes = Context.FetchData();
                Assert.AreEqual(0, tapes.Count);
                tapes.Add(new Tape
                              {
                                  AllowedIpAddress = "AllowedIpAddress",
                                  Description = "Description",
                                  OpenForRecording = true,
                                  Id = "Id",
                                  Comment = "Comment",
                              });
                Context.SaveData(tapes);
                List<Tape> tapes2 = Context.FetchData();
                Assert.AreEqual(1, tapes2.Count);
                Context.DeleteTape(tapes2[0].Id);
                tapes = Context.FetchData();
                Assert.AreEqual(0, tapes.Count);
            }
            finally
            {
                Context.Close();
            }
        }

        [Test]
        public void ContextCanListTapes()
        {
            Context.Open("Mocument");
            try
            {
                var tape = new Tape
                               {
                                   AllowedIpAddress = "AllowedIpAddress",
                                   Description = "Description",
                                   OpenForRecording = true,
                                   Id = "Id",
                                   Comment = "Comment",
                               };
                Context.SaveTape(tape);
                tape.Id = "ID2";
                Context.SaveTape(tape);

                List<Tape> tapes2 = Context.FetchData();
                Assert.AreEqual(2, tapes2.Count);
            }
            finally
            {
                Context.Close();
            }
        }

        [Test]
        public void MemoryMappedTapeStoreCanSaveAndFetch()
        {
            List<Tape> tapes2;
            List<Tape> tapes;

            Context.Open("Mocument");
            try
            {
                tapes = Context.FetchData();
                tapes.Add(new Tape
                              {
                                  AllowedIpAddress = "AllowedIpAddress",
                                  Description = "Description",
                                  OpenForRecording = true,
                                  Id = "Id",
                                  Comment = "Comment",
                              });
                Context.SaveData(tapes);
                tapes2 = Context.FetchData();
            }
            finally
            {
                Context.Close();
            }

            Assert.True(CompareTapeStores(tapes, tapes2));
        }
    }
}