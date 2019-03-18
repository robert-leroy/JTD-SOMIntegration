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
            // Restart Flag
            string[] lines;

            if (File.Exists(jtd_utilities.log.FileName))
            {
                try
                {
                    lines = File.ReadAllLines(jtd_utilities.log.FileName);

                    for (int a = 0; a < lines.Count(); a++)
                    {
                        if (lines[a].Contains("TimeoutException") || lines[a].Contains("timed out"))
                        {
                            // TODO Uncomment this
                            jtd_utilities.mail.SendEmailMessage("JTD Error -- Looks like a timeout.");
                            jtd_utilities.mail.SendTwilioMessage("JTDSQL02 -- Looks like a timeout. Restarting the Job");

                            // ---------------------------
                            //     RESTART THE JOB
                            // ---------------------------
                            jtd_utilities.restart.restartJob();

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

            return;
        }
    }
}
