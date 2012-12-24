using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json;
using Salient.HTTPArchiveModel;

namespace Mocument.DataAccess.Tests
{
    [TestFixture]
    public class EntryComparisonFixture
    {
        [Test]
        public void DefaultExactMatchComparer()
        {

            IEntryComparer comparer = new DefaultEntryComparer();

            var entry1Json = File.ReadAllText("entry1.txt");

            // compare identical entries

            var potentialMatch = JsonConvert.DeserializeObject<Entry>(entry1Json);
            var entryToMatch = JsonConvert.DeserializeObject<Entry>(entry1Json);

            var potentialMatches = new List<Entry>() {potentialMatch};
            var result = comparer.FindMatch(potentialMatches, entryToMatch);
            Assert.IsNotNull(result.Match);


            // have less query params in potentialMatches - should match
            potentialMatch = JsonConvert.DeserializeObject<Entry>(entry1Json);
            potentialMatch.request.queryString.Clear();
            potentialMatches = new List<Entry>() {potentialMatch};
            
            entryToMatch = JsonConvert.DeserializeObject<Entry>(entry1Json);
            
            result = comparer.FindMatch(potentialMatches, entryToMatch);
            Assert.IsNotNull(result.Match);


        }
    }
}
