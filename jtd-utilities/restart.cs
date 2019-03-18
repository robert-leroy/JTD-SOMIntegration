using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ServiceProcess;
using jtd_utilities;

namespace jtd_utilities
{
    public class restart
    {
        public static void restartJob()
        {

            // Test File for Recover
            if (checkPriorRestart())
            {
                jtd_utilities.log.AppendLog("Second restart aborted.");
                return;
            }

            // Append Log that we are recovering
            jtd_utilities.log.AppendLog("Restarting Integartion.");

            // Restart IIS
            string serviceName = "W3SVC"; //W3SVC refers to IIS service
            ServiceController service = new ServiceController(serviceName);
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped);// Wait till the service started and is running
            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running);// Wait till the service started and is running

            // Check SQL DB Users and Disconnect them
            jtd_utilities.sql.RemoveUsers();
            jtd_utilities.sql SQL = new sql();
            SQL.Connect();
            SQL.cnnSQL.ChangeDatabase("master");

            // Backup the Database
            SQL.BackupDatabase();

            // Restore the Database
            SQL.RestoreDatabase();
            SQL.cnnSQL.ChangeDatabase("P21");

            // Start the Import Package
            // Start the Comments Package
            // Append Log that we're done       
            SQL.RestartIntegration();
            SQL.Disconnect();

        }

        private static Boolean checkPriorRestart()
        {
            Boolean restarted = false;
            string[] lines;

            if (File.Exists(jtd_utilities.log.FileName))
            {
                try
                {
                    lines = File.ReadAllLines(jtd_utilities.log.FileName);

                    for (int a = 0; a < lines.Count(); a++)
                    {
                        if (lines[a].Contains("Restart"))
                        {
                            restarted = true;
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return restarted;

        }
    }
}