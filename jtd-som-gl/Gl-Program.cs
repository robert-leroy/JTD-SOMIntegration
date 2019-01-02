using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using Ads.Soa.DomainObject.Security;
using Ads.Common.Service.Rest.Client;
using Ads.Common.Service.Rest.Client.Exception;
using Ads.Common.Service.Error;
using P21.Soa.Service.Rest.Common;
using Ads.Soa.Rest.Security;
using P21.Soa.Service.Rest.Accounting.Common;
using P21.DomainObject.Accounting.Common;
using jtd_utilities;

namespace jtd_som_gl
{
    class Program
    {
        static String strToken = "";
        static jtd_utilities.sql SQL = new jtd_utilities.sql();

        static void Main(string[] args)
        {
            jtd_utilities.log.AppendLog("*** GL Processing Log ***");

            // Authenticate with the webservice
            strToken = SQL.P21Authenticate();

            // Connect to the Database
            SQL.Connect();

            // Call the P21 API to post the GL Entry
            int nRC = P21GLEntry();

            SQL.Disconnect();

            jtd_utilities.log.AppendLog("\r\n");

            // Exit the program
            Environment.Exit(nRC);
        }

        static DataSet SqlGetGlEntry()
        {
            // Get the GL Entry using SQL saved in the program settings.
            String strQuery = Properties.gl.Default.SqlQueryGlEntry;
            SqlCommand cmd = new SqlCommand(strQuery, SQL.cnnSQL);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            return dataset;
        }


        static int P21GLEntry()
        {
            int nReturnCode = 0;

            try
            {

                // This call generates a client which has all of the exposed rest methods.  Token security is used based off the user and password. 
                RestClientSecurity rcs = RestResourceClientHelper.GetClientSecurity(strToken);
                GlResourceClient glrc = new GlResourceClient(Properties.gl.Default.rootUri, rcs);

                // Setup the new objects
                Gl gl = new Gl();
                Gl gl1 = new Gl();
                List<Gl> gls = new List<Gl>();
                DateTime txDate = DateTime.Now;

                // Get the Dataset from the P21 Database
                DataSet dsGL = SqlGetGlEntry();
                Double GlAmount = 0;
                String GlSource = "";

                // Should only be one row.  Save the data we need
                foreach (DataRow row in dsGL.Tables[0].Rows)
                {
                    GlSource = row[0].ToString();
                    GlAmount = Convert.ToDouble(row[1].ToString());
                }
                
                // First half of the GL entry
                gl.CompanyNo = "JTD";
                gl.AccountNumber = "220010";
                gl.Period = DateTime.Now.Month;
                gl.YearForPeriod = DateTime.Now.Year;
                gl.JournalId = "PJ";
                gl.Amount = GlAmount;
                gl.Description = "SOM Integration";
                gl.ForeignAmount = GlAmount;
                gl.Source = GlSource;
                gl.TransactionDate = txDate;
                gls.Add(gl);

                // Second half of the GL entry
                gl1.CompanyNo = "JTD";
                gl1.AccountNumber = "221510";
                gl1.Period = DateTime.Now.Month;
                gl1.YearForPeriod = DateTime.Now.Year;
                gl1.JournalId = "PJ";
                gl1.Amount = GlAmount * -1;
                gl1.Description = "SOM Integration";
                gl1.ForeignAmount = GlAmount * -1;
                gl1.Source = GlSource;
                gl1.TransactionDate = txDate;
                gls.Add(gl1);

                // Call the Middleware API
                Gl[] glReturn = glrc.Resource.CreateGl(gls.ToArray());

                glrc.Close();

                jtd_utilities.log.AppendLog("GL Update successful -- " + GlAmount.ToString());

            }
            catch (Exception ex)
            {
                // Depending on the exception additional error information may be returned in the form or a ResourceError. This helper
                // will extract that from the exception. 
                ResourceError rErr = ExceptionHandler.GetResourceError(ex);

                string messageText = ex.ToString();
                if (rErr != null)
                {
                    messageText += Environment.NewLine + Environment.NewLine + "Details: " + Environment.NewLine + rErr.ErrorMessage;
                }

                jtd_utilities.log.AppendLog(messageText);
                nReturnCode = -1;
            }
			finally
			{
                nReturnCode = 0;
			}

            return nReturnCode;
        }
    }
}
