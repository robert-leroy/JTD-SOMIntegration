using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Ads.Soa.Rest.Security;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.IntegrationServices;

namespace jtd_utilities
{
    public class sql
    {
        public SqlConnection cnnSQL;

        public void Connect(Boolean UseIntegratedSecurity = false)
        {
            string connetionString = null;
            SqlConnection cnn;

            if (UseIntegratedSecurity)
            {
                connetionString = Properties.sql.Default.connectionStringIntegrated;
            }
            else
            {
                connetionString = Properties.sql.Default.connectionString;
            }

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

        public void RestartIntegration()
        {
            string folderName = "Subzero Order Management";
            string projectName = "SOM Recover Process";
            string packageName = "SOM Recover Process.dtsx";
            //string projectName = "SOM Invoice Comments";
            //string packageName = "SOM Update Invoice Comments.dtsx";

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

                jtd_utilities.log.AppendLog("Restarting Job");

                // Run the package
                package.Execute(false, null);

            } catch (Exception ex)
            {
                jtd_utilities.log.AppendLog("Can't restart job.  Error:" + ex.Message);
            }

            return;

        }
    }
}

