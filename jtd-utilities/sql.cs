using System;
using System.Data.SqlClient;
using Ads.Soa.Rest.Security;
using Microsoft.SqlServer.Management.IntegrationServices;
using Microsoft.SqlServer.Management.Smo;

namespace jtd_utilities
{
    public class sql
    {
        public SqlConnection cnnSQL;

        public void Connect()
        {
            string connetionString = null;
            SqlConnection cnn;

            connetionString = Properties.sql.Default.connectionStringIntegrated;

            cnn = new SqlConnection(connetionString);
            try
            {
                cnn.Open();
            }
            catch (Exception ex)
            {
                jtd_utilities.log.AppendLog("Can not open connection ! ");
            }

            cnnSQL = cnn;

        }
        public void Disconnect()
        {
            cnnSQL.Close();
        }

        public string P21Authenticate()
        {
            Ads.Soa.DomainObject.Security.Token token = null;

            try
            {
                //try to logon and get a token
                token = TokenResourceClient.CreateUserToken(Properties.sql.Default.rootUri, Properties.sql.Default.userName, Properties.sql.Default.passWord);
                //                token = TokenResourceClient.AuthenticateUser(rootUri + "api", "admin", "69646");

            }
            catch (Exception ex)
            {
                jtd_utilities.log.AppendLog("Error logging into the Api (" + Properties.sql.Default.rootUri + "): " + ex.ToString());
            }


            return token.AccessToken;
        }

        public void BackupDatabase()
        {
            // Build the backup file name
            string filename = string.Format("P21-{0}.bak", DateTime.Now.ToString("yyyy-MM-dd"));

            // Query to execute against the database
            string query = String.Format("BACKUP DATABASE P21 TO DISK = '{0}' WITH INIT", filename);

            // Execute the backup
            SqlCommand cmd = new SqlCommand(query, cnnSQL);
            cmd.CommandTimeout = 180;
            cmd.ExecuteNonQuery();
            
        }

        public void RestoreDatabase()
        {

            // Query to execute against the database
            string query = "RESTORE DATABASE P21 FROM DISK = 'E:\\SQL\\Backup\\P21-SOM-Integration-Post-Import.bak' WITH REPLACE, RECOVERY";

            // Execute the backup
            SqlCommand cmd = new SqlCommand(query, cnnSQL);
            cmd.CommandTimeout = 180;
            cmd.ExecuteNonQuery();

        }

        public static void RemoveUsers()
        {
            Server server = new Server("JTDSQL02.JTDINC.LOCAL");
            //Server server = new Server("localhost");
            Database database = new Database(server, "P21");
            database.Refresh();
            server.KillAllProcesses("P21");
            database.Alter(TerminationClause.RollbackTransactionsImmediately);
        }

        public void RestartIntegration()
        {
            string folderName = "Subzero Order Management";
            string projectName = "SOM Integration Project";
            string packageName = "SOM Integration Integrate With P21.dtsx";
            //string projectName = "SOM Invoice Comments";
            //string packageName = "SOM Update Invoice Comments.dtsx";

            try
            {

                // Create the Integration Services object
                IntegrationServices integrationServices = new IntegrationServices(this.cnnSQL);

                // Get the Integration Services catalog
                Catalog catalog = integrationServices.Catalogs["SSISDB"];
                jtd_utilities.log.AppendLog("Got Catalog.");


                // Get the folder
                CatalogFolder folder = catalog.Folders[folderName];
                jtd_utilities.log.AppendLog("Got Folder.");

                // Get the project
                ProjectInfo project = folder.Projects[projectName];
                jtd_utilities.log.AppendLog("Got Project.");

                // Get the package
                PackageInfo package = project.Packages[packageName];
                jtd_utilities.log.AppendLog("Got Package.");

                // Run the package
                package.Execute(false, null);
                jtd_utilities.log.AppendLog("Job restarted.");


            }
            catch (Exception ex)
            {
                jtd_utilities.log.AppendLog("Can't restart job.  Error:" + ex.Message);
            }

            return;

        }
        public void RunComments()
        {
            string folderName = "Subzero Order Management";
            //string projectName = "SOM Integration Project";
            //string packageName = "SOM Integration Integrate With P21.dtsx";
            string projectName = "SOM Invoice Comments";
            string packageName = "SOM Update Invoice Comments.dtsx";

            try
            {

                // Create the Integration Services object
                IntegrationServices integrationServices = new IntegrationServices(this.cnnSQL);

                // Get the Integration Services catalog
                Catalog catalog = integrationServices.Catalogs["SSISDB"];

                // Get the folder
                CatalogFolder folder = catalog.Folders[folderName];

                // Get the project
                ProjectInfo project = folder.Projects[projectName];

                // Get the package
                PackageInfo package = project.Packages[packageName];

                // Run the package
                package.Execute(false, null);

            }
            catch (Exception ex)
            {
                jtd_utilities.log.AppendLog("Can't restart job.  Error:" + ex.Message);
            }

            return;

        }

        public void Check2200Balance()
        {

            // Query to execute against the database
            string query = "SELECT period_balance FROM balances ";
            query += String.Format("WHERE period = {0} AND year_for_period = {1} AND account_no = 220010", DateTime.Now.Month, DateTime.Now.Year);

            // Execute the backup
            SqlCommand cmd = new SqlCommand(query, cnnSQL);
            cmd.CommandTimeout = 180;
            Decimal CurrBalance = (Decimal)cmd.ExecuteScalar();

            if(CurrBalance != 0)
            {
                jtd_utilities.log.AppendLog("2200-10 Balance is Non-Zero.");
                jtd_utilities.mail.SendEmailMessage("2200-10 Balance is Non-Zero.  Please check the SOM Integration processing.");
            }

        }

    }
}

