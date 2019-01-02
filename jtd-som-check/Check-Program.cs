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

            //string FileName = @"C:\sism\Archive\2018 Archive\20181219\Integration-log.txt";
            string FileName = jtd_utilities.log.GetDirectory();

            try
            {

                string[] lines = File.ReadAllLines(FileName);

                for (int a = 0; a < lines.Count(); a++)
                {
                    if (lines[a].Contains("TimeoutException"))
                    {
                        jtd_utilities.mail.SendEmailMessage("Test from your Dad.  Looks like a timeout.");
                        jtd_utilities.mail.SendTwilioMessage("JTDSQL02 -- Looks like a timeout. Restarting the Job");

                        jtd_utilities.sql SQL = new sql();
                        SQL.Connect();
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
    }
}
