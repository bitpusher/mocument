using System;
using Salient.HTTPArchiveModel;

namespace Mocument.DataAccess.SQLite
{
    [Serializable]
    public class EntryCompareResult
    {
        public Entry Match { get; set; }
    }
}