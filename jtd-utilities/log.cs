using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace jtd_utilities
{
    public class log
    {
        public static void AppendLog(String txtMessage)
        {

            string strPath = GetDirectory();

            // Write to the file
            using (StreamWriter w = File.AppendText(strPath))
            {
                w.WriteLine(txtMessage);
            }
        }

        public static string GetDirectory()
        {

            // Build Archive Path based on current date
            String strPath = "C:\\SISM\\Archive\\";
            strPath += DateTime.Now.Year.ToString("0000");
            strPath += DateTime.Now.Month.ToString("00");
            strPath += DateTime.Now.Day.ToString("00");

            // If the path doesn't exist, create it
            if (!Directory.Exists(strPath))
            {
                Directory.CreateDirectory(strPath);
            }

            strPath += "\\Integration-log.txt";

            return strPath;
        }
    }
}
