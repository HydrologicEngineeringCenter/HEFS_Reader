using System.IO;
using System.IO.Compression;
using System.Collections;
 


namespace Reclamation.Core
{
    /// <summary>
    /// Summary description for ZipFile.
    /// </summary>
    public class ZipFileUtility
    {


        /// <summary>
        /// compress a single file into a zip file.
        /// </summary>
        /// <param name="fileToZip"></param>
        /// <param name="outputZipFile"></param>
        public static void CompressFile(string fileToZip, string outputZipFile)
        {
            File.Delete(outputZipFile);
            using (var zip = System.IO.Compression.ZipFile.Open(outputZipFile, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(fileToZip, fileToZip);
            }
        }

    


        /// <summary>
        /// Return a list of files in the Zip archive.
        /// </summary>
        /// <param name="zipFilename"></param>
        /// <returns></returns>
        public static string[] ZipInfo(string zipFilename)
        {
            using (var zip = System.IO.Compression.ZipFile.Open(zipFilename, ZipArchiveMode.Read))
            {
                ArrayList list = new ArrayList();
                foreach (var item in zip.Entries)
                {
                    list.Add(item.Name);
                }
                string[] rval = new string[list.Count];
                list.CopyTo(rval);
                return rval;
            }
        }


        /// <summary>
        /// unzips single zip entry.
        /// </summary>
        /// <param name="zipFilename">input zip file</param>
        /// <param name="unzipFile">output unzipped filename</param>
        /// <returns></returns>
        public static void UnzipFile(string zipFilename, string unzipFile)
        {
            using (var zip = System.IO.Compression.ZipFile.Open(zipFilename, ZipArchiveMode.Read))
            {
                var unzipDir = Path.GetDirectoryName(unzipFile);
                File.Delete(unzipFile);
                var zipEntry = Path.Combine(unzipDir, zip.Entries[0].Name);
                File.Delete(zipEntry);
                zip.ExtractToDirectory(unzipDir);
                File.Move(zipEntry, unzipFile);
            }        
        }

        /// <summary>
        /// unzip a file into specified directory recursively.
        /// </summary>
        /// <param name="zipFilename"></param>
        /// <param name="unzipDirectory"></param>
        public static void UnzipDir(string zipFilename, string unzipDirectory)
        {
            UnzipFile(zipFilename, unzipDirectory);            
        }
    }
}

