using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Fiddler;

namespace Mocument.Data.Transcoders
{
    public static class HttpArchiveFormat
    {
        public static List<Session> ImportSessions(string path)
        {
            try
            {
                var listSessions = new List<Session>();
                var oSr = new StreamReader(path, Encoding.UTF8);
                HttpArchiveJsonImport.LoadStream(oSr, listSessions);
                oSr.Close();
                return listSessions ;
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to import HTTPArchive", exception);
            }
        }

        public static void ExportSessions(List<Session> oSessions, string path)
        {
            try
            {
                var swOutput = new StreamWriter(path, false, Encoding.UTF8);
                HttpArchiveJsonExport.WriteStream(swOutput, oSessions);
                swOutput.Close();
                
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to save HTTPArchive", exception);
            }
        }
    }
}