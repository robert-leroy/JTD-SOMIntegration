using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using jtd_utilities;

namespace jtd_som_check
{
    class Program
    {
        static void Main(string[] args)
        {

            //string FileName = @"\\jtdsql02.jtdinc.local\c$\sism\Archive\20190110\Integration-log.txt";
            string FileName = jtd_utilities.log.GetDirectory();
            string[] lines;

            if (File.Exists(FileName))
            {
                try
                {
                    lines = File.ReadAllLines(FileName);

                    for (int a = 0; a < lines.Count(); a++)
                    {
                        if (lines[a].Contains("TimeoutException") || lines[a].Contains("timed out"))
                        {
                            jtd_utilities.mail.SendEmailMessage("JTD Error -- Looks like a timeout.");
                            jtd_utilities.mail.SendTwilioMessage("JTDSQL02 -- Looks like a timeout. Restarting the Job");

                            jtd_utilities.sql SQL = new sql();
                            SQL.Connect(true);
                            SQL.RestartIntegration();
                            SQL.Disconnect();

                            break;
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                jtd_utilities.mail.SendEmailMessage("JTD Error -- No Log File.");
                jtd_utilities.mail.SendTwilioMessage("JTDSQL02 -- No Log File. Need to check the FTP process");
            }
        }
    }
}
