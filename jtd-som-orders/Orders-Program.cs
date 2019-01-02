using System;
using System.Data.SqlClient;
using System.Data;
using Ads.Soa.DomainObject.Security;
using Ads.Common.Service.Rest.Client;
using Ads.Common.Service.Rest.Client.Exception;
using Ads.Common.Service.Error;
using P21.Soa.Service.Rest.Common;
using Ads.Soa.Rest.Security;
using jtd_utilities;

namespace jtd_som_orders
{

    class Program
    {

        // RJL 01/02/2019 - Created jtd-utilities to centralize all settings and common functions
        static String strToken = "";
        static Int32 totalLines = 0;
        static jtd_utilities.sql SQL = new jtd_utilities.sql();
         
        static void Main(string[] args)
        {
         
            jtd_utilities.log.AppendLog("*** Order Processing Log ***");

            // Authenticate with the webservice
            strToken = SQL.P21Authenticate();

            // Connect to the Database
            SQL.Connect();

            // Get the list of orders
            DataSet dsHeaders = SqlGetOrdersList();

            int nReturnCode = 0;
            // For each invoice process the details and post them to the Middleware system
            foreach (DataRow drOrder in dsHeaders.Tables[0].Rows)
            {
                // Process the order
                nReturnCode = P21SalesOrder(drOrder);

                // If the order doesn't post correctly, we exit out immediately
                if (nReturnCode != 0)
                    break;
            }

            SQL.Disconnect();

            jtd_utilities.log.AppendLog("Total Order Items: " + totalLines.ToString());

            Environment.Exit(nReturnCode);
        }
 
        static DataSet SqlGetOrdersList()
        {
            SqlCommand cmd = new SqlCommand(Properties.order.Default.SqlQueryHeaders, SQL.cnnSQL);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            return dataset;

        }
        static DataSet SqlGetOrderLines(String sqlQuery, String row)
        {
            String strQuery = sqlQuery;
            strQuery = strQuery.Replace("?", row);
            SqlCommand cmd = new SqlCommand(strQuery, SQL.cnnSQL);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            return dataset;
        }

        static DataSet SqlGetOrderLineDetails(String sqlQuery, String importSet, String lineID)
        {
            String strQuery = sqlQuery;
            strQuery = strQuery.Replace("?", importSet);
            strQuery = strQuery.Replace("#", lineID);
            SqlCommand cmd = new SqlCommand(strQuery, SQL.cnnSQL);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset);
            return dataset;
        }

        // This function processes just one order
        static int P21SalesOrder(DataRow drOrder)
        {
            int nReturnCode = 0;

            try
            {

                RestClientSecurity rcs = RestResourceClientHelper.GetClientSecurity(strToken);

                // This call generates a client which has all of the exposed rest methods.  Token security is used based off the user and password.
                P21.Soa.Service.Rest.Sales.OrderResourceClient orderResourceClient =
                    new P21.Soa.Service.Rest.Sales.OrderResourceClient(Properties.order.Default.rootUri, rcs);

                // Setup the variables used to store data during processing
                orderResourceClient.AcceptType = AcceptTypes.xml;
                P21.DomainObject.Sales.Order.Order orderCreate = new P21.DomainObject.Sales.Order.Order();

                //Populate the main fields for the order
                orderCreate.CustomerId = Convert.ToInt32(drOrder["CustomerID"]);
                orderCreate.CompanyId = drOrder["CompanyID"].ToString();
                orderCreate.LocationId = Convert.ToInt32(drOrder["SalesLocationID"]);
                orderCreate.PoNo = drOrder["CustomerPONumber"].ToString();
                orderCreate.Taker = drOrder["Taker"].ToString();
                orderCreate.JobName = drOrder["JobName"].ToString();
                orderCreate.OrderDate = Convert.ToDateTime(drOrder["OrderDate"].ToString());
                orderCreate.RequestedDate = Convert.ToDateTime(drOrder["RequestedDate"].ToString());
                orderCreate.Quote = drOrder["Quote"].ToString();
                orderCreate.Approved = ((drOrder["Approved"].ToString() == "Y") ? true : false);
                orderCreate.ShipToId = Convert.ToInt32(drOrder["ShipToID"].ToString());
                orderCreate.ShipToName = drOrder["ShipToName"].ToString();
                orderCreate.ShipToAddress1 = drOrder["ShipToAddress1"].ToString();
                orderCreate.ShipToAddress2 = drOrder["ShipToAddress2"].ToString();
                orderCreate.ShipToAddress3 = drOrder["ShipToAddress3"].ToString();
                orderCreate.ShipToCity = drOrder["ShipToCity"].ToString();
                orderCreate.OeHdrShip2State = drOrder["ShipToState"].ToString();
                orderCreate.ZipCode = drOrder["ShipToZipCode"].ToString();
                orderCreate.ShipToCountry = drOrder["ShipToCountry"].ToString();
                orderCreate.SourceLocationId = Convert.ToInt32(drOrder["SourceLocationID"].ToString());
                orderCreate.CarrierId = Convert.ToInt32(drOrder["CarrierID"].ToString());
                orderCreate.PackingBasis = drOrder["PackingBasis"].ToString();
                orderCreate.DeliveryInstructions = drOrder["DeliveryInstructions"].ToString();
                orderCreate.Terms = drOrder["Terms"].ToString();
                orderCreate.Class1id = drOrder["Class1"].ToString();
                orderCreate.RMA = drOrder["RMAFlag"].ToString();
                orderCreate.WebReferenceNo = drOrder["WebReferenceNumber"].ToString();
                orderCreate.CreateInvoice = ((drOrder["CreateInvoice"].ToString() == "Y") ? true : false);

                // RJL 05/31/2018 - For Ohio Misc. customers, when there is tax, don't convert to an invoice.  
                //                  This is necssary because we don't know which P21 tax jurisdiction to apply.
                if ((orderCreate.CustomerId == 10164) || (orderCreate.CustomerId == 10165))
                {
                    if (Convert.ToDecimal(drOrder["TaxTotal"]) > 0)
                    {
                        orderCreate.CreateInvoice = false;
                    }
                }

                // Append Header Notes
                orderCreate.Notes = AppendOrderNotes(drOrder["ImportSet"].ToString());

                // Append Salesreps
                orderCreate.Salesreps = AppendSalesReps(drOrder["ImportSet"].ToString());

                // Go get the list of line items for this order from the SQL database
                //DataSet dsLines = SqlGetOrderLines(Properties.order.Default.SqlQueryLines, drOrder[1].ToString());
                DataSet dsLines = SqlGetOrderLines(Properties.order.Default.SqlQueryLines, drOrder["ImportSet"].ToString());

                // Declare the list of order detail lines
                P21.DomainObject.Sales.Order.OrderLines orderLines = new P21.DomainObject.Sales.Order.OrderLines();

                // For each line item we add them to the Order with the Notes and Serials
                int lineNum = 1;
                foreach (DataRow drLine in dsLines.Tables[0].Rows)
                {

                    P21.DomainObject.Sales.Order.OrderLine lineItem = new P21.DomainObject.Sales.Order.OrderLine();

                    // Populate the members of the lineItem object
                    lineItem.LineNo = lineNum;
                    lineItem.ItemId = drLine["ItemID"].ToString();
                    lineItem.UnitQuantity = Convert.ToDouble(drLine["UnitQuantity"].ToString());
                    lineItem.UnitOfMeasure = drLine["UnitOfMeasure"].ToString();
                    lineItem.UnitPrice = Convert.ToDouble(drLine["UnitPrice"].ToString());
                    lineItem.ExtendedDesc = drLine["ExtendedDescription"].ToString();
                    lineItem.SourceLocId = Convert.ToInt32(drLine["SourceLocationID"].ToString());
                    lineItem.ShipLocId = Convert.ToInt32(drLine["ShipLocationID"].ToString());
                    lineItem.ProductGroupId = drLine["ProductGroupID"].ToString();
                    lineItem.TaxItem = drLine["TaxItem"].ToString();
                    lineItem.PricingUnit = drLine["PricingUnit"].ToString();
                    if (drLine["CommissionCost"].ToString().Length > 0)
                        lineItem.CommissionCost = Convert.ToDouble(drLine["CommissionCost"].ToString());
                    lineItem.ManualPriceOveride = drLine["ManualPriceOverride"].ToString();

                    // add the serial numbers to the line
                    lineItem.Serials = AppendSerials(drOrder["ImportSet"].ToString(), drLine["LineID"].ToString(), lineNum);

                    // add any line comments/notes
                    lineItem.Notes = AppendOrderLineNotes(drOrder["ImportSet"].ToString(), drLine["LineID"].ToString(), lineNum);

                    // Add this line to the list of order lines
                    orderLines.list.Add(lineItem);

                    lineNum++;
                }

                orderCreate.Lines = orderLines;

                if ((orderCreate.WebReferenceNo != "3099949") && 
                    (orderCreate.WebReferenceNo != "3099978") && 
                    (orderCreate.WebReferenceNo != "3099983")) 
                {
                    // Now we have a populated Order XML Document and we can call the webservice
                    // Fingers Crossed!!
                    P21.DomainObject.Sales.Order.Order orderReturn = new P21.DomainObject.Sales.Order.Order();
                    orderReturn = orderResourceClient.Resource.CreateOrder(orderCreate);

                    //orderCreate.OrderNo = nOrderCount++.ToString();
                    jtd_utilities.log.AppendLog("Added Order: " + orderReturn.OrderNo.ToString() + " LinesOut " + orderCreate.Lines.list.Count.ToString() + " LinesIn " + orderReturn.Lines.list.Count.ToString());
                    if (orderReturn.APIInformation != null)
                        jtd_utilities.log.AppendLog("Return Message: " + orderReturn.APIInformation.ContextMessage);

                    totalLines += orderCreate.Lines.list.Count;

                    if (orderCreate.Lines.list.Count != orderReturn.Lines.list.Count)
                    {

                        // Identify the order for support
                        String msgText;
                        msgText = "Customer: " + drOrder["CustomerID"].ToString() + "  ";
                        msgText += "PO Number: " + drOrder["CustomerPONumber"].ToString() + "  ";
                        msgText += "Web Order: " + drOrder["WebReferenceNumber"].ToString();
                        msgText += "Number of Lines added to P21 does not match SOM lines received.";

                        jtd_utilities.mail.SendEmailMessage(msgText);
                    }
                }
                // RJL 01/02/2019 - Why didn't we ever close that
                orderResourceClient.Close();
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

                // Identify the order for support
                messageText = "Customer: " + drOrder["CustomerID"].ToString() + "  ";
                messageText += "PO Number: " + drOrder["CustomerPONumber"].ToString() + "  ";
                messageText += "Web Order: " + drOrder["WebReferenceNumber"].ToString();
                jtd_utilities.log.AppendLog(messageText);

                jtd_utilities.mail.SendEmailMessage(messageText);

                nReturnCode = 0;
            }
            finally
            {
            }

            return nReturnCode;
        }

        static P21.DomainObject.Sales.Order.OrderLineSerials AppendSerials(String ImportSet, String LineID, int lineNum)
        {
            // Declare the return list 
            P21.DomainObject.Sales.Order.OrderLineSerials lineSerials = new P21.DomainObject.Sales.Order.OrderLineSerials();

            DataSet dsSerials = SqlGetOrderLineDetails(Properties.order.Default.SqlQuerySerials, ImportSet, LineID);

            foreach (DataRow row in dsSerials.Tables[0].Rows)
            {

                P21.DomainObject.Sales.Order.OrderLineSerial serial = new P21.DomainObject.Sales.Order.OrderLineSerial();

                serial.LineNo = lineNum;
                serial.SerialNumber = row["SerialNumber"].ToString().Trim();

                lineSerials.list.Add(serial);
            }

            return lineSerials;
        }

        static P21.DomainObject.Sales.Order.OrderLineNotes AppendOrderLineNotes(String ImportSet, String LineID, int LineNum)
        {
            // Declare the return list or notes
            P21.DomainObject.Sales.Order.OrderLineNotes lineNotes = new P21.DomainObject.Sales.Order.OrderLineNotes();

            DataSet dsLineNotes = SqlGetOrderLineDetails(Properties.order.Default.SqlQueryLineNotes, ImportSet, LineID);

            foreach (DataRow row in dsLineNotes.Tables[0].Rows)
            {
                if (row["Note"].ToString().Length > 0)
                {
                    P21.DomainObject.Sales.Order.OrderLineNote lineNote = new P21.DomainObject.Sales.Order.OrderLineNote();

                    lineNote.LineNo = LineNum;
                    lineNote.Topic = row["Topic"].ToString();
                    lineNote.Note = row["Note"].ToString();
                    lineNote.ActivationDate = Convert.ToDateTime(row["ActivationDate"].ToString());
                    if (row["ExpirationDate"].ToString().Length > 0)
                        lineNote.ExpirationDate = Convert.ToDateTime(row["ExpirationDate"].ToString());
                    lineNote.EntryDate = Convert.ToDateTime(row["EntryDate"].ToString());
                    lineNote.NotepadClassId = row["NotepadClassID"].ToString();
                    lineNote.Mandatory = row["Mandatory"].ToString();

                    lineNotes.list.Add(lineNote);
                }
            }

            return lineNotes;
        }

        static P21.DomainObject.Sales.Order.OrderNotes AppendOrderNotes(String ImportSet)
        {
            // Declare the return list or notes
            P21.DomainObject.Sales.Order.OrderNotes orderNotes = new P21.DomainObject.Sales.Order.OrderNotes();

            DataSet dsNotes = SqlGetOrderLines(Properties.order.Default.SqlQueryHeaderNotes, ImportSet);

            foreach (DataRow row in dsNotes.Tables[0].Rows)
            {
                P21.DomainObject.Sales.Order.OrderNote note = new P21.DomainObject.Sales.Order.OrderNote();
                note.Topic = row["Topic"].ToString();
                note.Note = row["Note"].ToString();
                note.ActivationDate = Convert.ToDateTime(row["ActivationDate"].ToString());
                if (row["ExpirationDate"].ToString().Length > 0)
                    note.ExpirationDate = Convert.ToDateTime(row["ExpirationDate"].ToString());
                note.EntryDate = Convert.ToDateTime(row["EntryDate"].ToString());
                note.NotepadClassId = row["NotepadClassID"].ToString();
                note.Mandatory = ((row["Mandatory"].ToString() == "Y") ? true : false); 

                orderNotes.list.Add(note);
            }

            return orderNotes;
        }
        static P21.DomainObject.Sales.Order.OrderSalesreps AppendSalesReps(String ImportSet)
        {
            // Declare the return list or notes
            P21.DomainObject.Sales.Order.OrderSalesreps salesReps = new P21.DomainObject.Sales.Order.OrderSalesreps();

            DataSet dsReps = SqlGetOrderLines(Properties.order.Default.SqlQuerySalesReps, ImportSet);

            // Usually just one of these at JTD
            foreach (DataRow row in dsReps.Tables[0].Rows)
            {
                P21.DomainObject.Sales.Order.OrderSalesrep rep = new P21.DomainObject.Sales.Order.OrderSalesrep();
                rep.SalesrepId = row["SalesrepID"].ToString();
                rep.PrimarySalesrep = ((row["PrimarySalesrep"].ToString() == "Y") ? true : false);
                rep.CommissionSplit = Convert.ToDouble(row["CommissionSplit"].ToString());

                salesReps.list.Add(rep);
            }

            return salesReps;
        }
    }
}
