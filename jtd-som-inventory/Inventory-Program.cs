using System;
using System.Data.SqlClient;
using System.Data;
using Ads.Soa.DomainObject.Security;
using Ads.Common.Service.Rest.Client;
using P21.DomainObject.Inventory;
using Ads.Common.Service.Rest.Client.Exception;
using Ads.Common.Service.Error;
using P21.Soa.Service.Rest.Common;
using Ads.Soa.Rest.Security;
using jtd_utilities;

namespace jtd_som_inventory
{

    class Program
    {
        static String strToken = "";
        static jtd_utilities.sql SQL = new jtd_utilities.sql();

        static void Main(string[] args)
        {

            // RJL 01/02/2019 - Created jtd-utilities to centralize all settings and common functions
            jtd_utilities.log.AppendLog("*** Inventory Processing Log ***");

            // Authenticate with the webservice
            strToken = SQL.P21Authenticate();

            // Connect to the Database
            SQL.Connect();

            // Call the Middleware function to post Adjustments
            int nRC = P21InventoryAdjustment();

            SQL.Disconnect();

            jtd_utilities.log.AppendLog("\r\n");

            Environment.Exit(nRC);
        }

        // Retrieve the list of items to be added to inventory
        static DataSet SqlGetInventory()
        {
            String strQuery = Properties.inventory.Default.SqlQueryGetInventory;
            SqlCommand cmd = new SqlCommand(strQuery, SQL.cnnSQL);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            return dataset;
        }

        // Retrieve specific serial numbers for adjustments
        static DataSet SqlGetSerialForLine(Int32 nItemSet, String strItem)
        {
            String strQuery = Properties.inventory.Default.SqlQuerySerialLine;
            strQuery = strQuery.Replace("?", nItemSet.ToString());
            strQuery = strQuery.Replace("#", "'" + strItem + "'");
            SqlCommand cmd = new SqlCommand(strQuery, SQL.cnnSQL);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            return dataset;
        }

        static int P21InventoryAdjustment()
        {
            int nReturnCode = 0;
            int lineCount = 0;

            try
            {

                RestClientSecurity rcs = RestResourceClientHelper.GetClientSecurity(strToken);

                // This call generates a client which has all of the exposed rest methods.  
                // Token security is used based off the user and password.
                P21.Soa.Service.Rest.Inventory.InventoryAdjustmentResourceClient inventoryAdjustmentResourceClient =
                    new P21.Soa.Service.Rest.Inventory.InventoryAdjustmentResourceClient(Properties.inventory.Default.rootUri, rcs);

                // Setup the variables used to store data during processing
                inventoryAdjustmentResourceClient.AcceptType = AcceptTypes.xml;
                InventoryAdjustment adjustmentReturn = new InventoryAdjustment();

                // Go get the list of adjustments from the SQL database
                DataSet dsInventory = SqlGetInventory();

                // For each line item we post directly to P21
                foreach (DataRow drLine in dsInventory.Tables[0].Rows)
                {
                    // Retrieve the serial numbers for this inventory item
                    DataSet dsSerials = SqlGetSerialForLine(Convert.ToInt32(drLine[0].ToString()), drLine[1].ToString());

                    // If we have serial numbers, they are included in the call to middleware
                    if (dsSerials.Tables[0].Rows.Count > 0)
                    {
                        
                        // It's possible we could have more then one serial number for this item.
                        // We post them individually
                        foreach (DataRow drSerial in dsSerials.Tables[0].Rows)
                        {
                            
                            adjustmentReturn = inventoryAdjustmentResourceClient.Resource.CreateWmsAdjustmentWithCost(
                                        drLine["SourceLocationID"].ToString(),  // Location
                                        "SOM Inventory Adjustment",             // Reason
                                        "Y",                                    // Approved
                                        "",                                     // Description
                                        drLine["ItemID"].ToString(),            // Item
                                        "1",                                    // Qty
                                        drLine["UnitOfMeasure"].ToString(),     // UOM
                                        "",                                     // Bin
                                        "",                                     // Lot
                                        drSerial["SerialNumber"].ToString().Trim(),  // Serial
                                        drLine["Cost"].ToString());             // Cost

                            if (adjustmentReturn.Lines.list[0].Serials.list.Count > 0)
                            {
                                jtd_utilities.log.AppendLog("Added Item -- " + adjustmentReturn.Lines.list[0].ItemId + " -- " + adjustmentReturn.Lines.list[0].Serials.list[0].SerialNumber);
                            }
                            else
                            {
                                jtd_utilities.log.AppendLog("Added Item -- " + adjustmentReturn.Lines.list[0].ItemId + " -- Serial not returned");
                            }
                           
                        }
                    }
                    else
                    {
                        
                        // If there is no serial number, we still post the adjustment for stockable items but the Quantity can be multiples.
                        adjustmentReturn = inventoryAdjustmentResourceClient.Resource.CreateWmsAdjustmentWithCost(
                                    drLine["SourceLocationID"].ToString(),  // Location
                                    "SOM Inventory Adjustment",             // Reason
                                    "Y",                                    // Approved
                                    "",                                     // Description
                                    drLine["ItemID"].ToString(),            // Item
                                    drLine["AdjAmount"].ToString(),         // Qty
                                    drLine["UnitOfMeasure"].ToString(),     // UOM
                                    "",                                     // Bin
                                    "",                                     // Lot
                                    "",                                     // Serial
                                    drLine["Cost"].ToString());
                        
                        jtd_utilities.log.AppendLog("Added Item -- " + adjustmentReturn.Lines.list[0].ItemId);

                    }

                    lineCount++;
                }
                // RJL -- 01/02/2019 - Why did you not close this - DUMMY!
                inventoryAdjustmentResourceClient.Close();
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
            }

            jtd_utilities.log.AppendLog("Total Items: " + lineCount.ToString());
            return nReturnCode;
        }
    }
}
