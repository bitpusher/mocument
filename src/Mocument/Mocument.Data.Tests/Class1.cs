using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Mocument.Data.Tests
{
    [TestFixture]
    public class Class1
    {
        [Test]
        public void CRUD()
        {
            var lib = new TapeLibrary();
            Tape t = lib.Select("foo.bar");
            Assert.IsNull(t);

            var t2 = new Tape()
                         {
                             AllowedIpAddress = "0.0.0.0",
                             Description = "Desc",
                             Id = "foo.bar",
                             OpenForRecording = false,
                             Content = "some json"
                         };
            lib.AddOrUpdate(t2);
            
            t = lib.Select(t2.Id);
            Assert.AreEqual(t.Id,t2.Id);
            Assert.AreEqual(t.OpenForRecording, t2.OpenForRecording);
            Assert.AreEqual(t.AllowedIpAddress, t2.AllowedIpAddress);
            Assert.AreEqual(t.Content, t2.Content);
            Assert.AreEqual(t.Description, t2.Description);

            t2.Description = "new description";
            lib.AddOrUpdate(t2);
            Assert.AreNotEqual(t.Description,t2.Description);
            t = lib.Select(t2.Id);
            Assert.AreEqual(t.Description, t2.Description);
            lib.Delete(t.Id);
            t = lib.Select(t.Id);
            Assert.IsNull(t);
        }
    }
}
