using System;
using System.Linq;
using System.Text;
using Salient.HTTPArchiveModel;

namespace Mocument.Model
{
    [Serializable]
    public class Tape : HTTPArchive
    {

        public string Id { get; set; }
        public string Description { get; set; }
        public bool OpenForRecording { get; set; }
        public string AllowedIpAddress { get; set; }
        public string Comment { get; set; }
    }
}
