using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Threading;
using System.Diagnostics;
using System.Xml;
using System.Data;


namespace WaveToMP3
{
    class Program
    {
        static void Main(string[] args)
        {
            string sourceDir = ConfigurationManager.AppSettings["SourceDir"];
            string destDir = ConfigurationManager.AppSettings["DestinationDir"];
            string processedDir = ConfigurationManager.AppSettings["ProcessedDir"];
            string lamePath = ConfigurationManager.AppSettings["LamePath"];
            string tempFolder = ConfigurationManager.AppSettings["TempFolder"];
            string dataFileName = System.AppDomain.CurrentDomain.BaseDirectory + "Data.xml";
            string tempFileName;

            // Data file loading
            FileInfo dataFile = new FileInfo(dataFileName);

            DataSet dsData = new DataSet();
            DataTable dtHistory = new DataTable();

            if (dataFile.Exists==true)
            {
                //Load the XML
                dsData.ReadXml(dataFileName);
                dtHistory = dsData.Tables[0];
                               
            }
            else
            {
                dtHistory.Columns.Add("Filename");
                dsData.Tables.Add(dtHistory);
            }
                       

            // get a list of files
            string[] files = Directory.GetFiles(sourceDir, "*.wav");

            foreach (string file in files)
            {

                //FileStream stream = null;
                FileInfo currentFile = new FileInfo(file);

                if (!currentFile.Name.StartsWith("."))
                {
                    DataRow[] foundRows = dtHistory.Select("Filename = '" + Path.GetFileNameWithoutExtension(currentFile.FullName) + "'");

                    if (foundRows.Length == 0)
                    {
                        string sourceFileNameString = "\"" + currentFile.FullName + "\"";//    string.Format(@\""{0}\"", currentFile.FullName);
                        tempFileName = tempFolder + currentFile.Name;
                        currentFile.CopyTo(tempFileName);


                        string destFileNameString = "\"" + tempFolder + Path.GetFileNameWithoutExtension(currentFile.Name) + ".mp3" + "\"";

                        FileInfo destFile = new FileInfo(tempFolder + Path.GetFileNameWithoutExtension(currentFile.Name) + ".mp3");

                        string artistString = "\"Pete Trimby\"";
                        string albumName = "\"Rekordbox Recordings\"";

                        // check that we haven't done the file before
                        


                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.FileName = lamePath;
                        psi.Arguments = "-V2 -b 320 --ta " + artistString + " --tl " + albumName + " \"" + tempFileName + "\"" + " \"" + destFile.FullName + "\"";
                        //psi.Arguments = "-V2 -b 320 " + sourceFileNameString + " " + destFileNameString;
                        psi.WindowStyle = ProcessWindowStyle.Hidden;
                        Process p = Process.Start(psi);
                        p.WaitForExit();



                        try
                        {
                            File.Delete(tempFileName);
                            File.Move(destFile.FullName, destDir + @"\" + Path.GetFileNameWithoutExtension(destFile + ".mp3"));
                            File.Move(currentFile.FullName, processedDir + currentFile.Name);

                        }
                        catch (System.IO.IOException ex)
                        {
                            // likely file already exists or a file lock on the wav

                        }

                        // add the file to the history table
                        dtHistory.Rows.Add(Path.GetFileNameWithoutExtension(currentFile.FullName));
                    }
                }

            }
            // Save the history file
            dsData.WriteXml(dataFileName);
            

        }

        private bool CheckFileLocked(FileInfo file)
        {
            bool locked = false;

            try
            {
                file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                
            }
            catch (System.IO.IOException ex)
            {

                locked = true;
            }
                
            


            return locked;


        }
    }
}
