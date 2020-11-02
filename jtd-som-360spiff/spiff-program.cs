using System;
using System.Data.SqlClient;
using System.Data;
using jtd_utilities;
using ClosedXML.Excel;

namespace jtd_som_360spiff
{
    class Program
    {
        static Int32 totalLines = 0;
        static jtd_utilities.sql SQL = new jtd_utilities.sql();
        
        static void Main(string[] args)
        {

            jtd_utilities.log.AppendLog("*** Spiff Processing Log  ***");

            // Check the Current GL Integration Balance.
            jtd_utilities.sql sqlConn = new sql();

            // Connect to the Database
            SQL.Connect();

            // Get the list of Serials
            DataTable dsHeaders = SqlGetSerialList();
            totalLines = dsHeaders.Rows.Count;

            // Write to File
            BuildExcel(dsHeaders);

            // Send the message
           //jtd_utilities.mail.SpiffEmailMessage("The export completed with " + totalLines.ToString() + " serial number(s). \r\n");

            return;
        } 
        static DataTable SqlGetSerialList()
        {
            SqlCommand cmd = new SqlCommand(Properties.Settings.Default.SqlQuery, SQL.cnnSQL);
            cmd.Parameters.Add("@INVDATE", SqlDbType.Date);
            cmd.Parameters["@INVDATE"].Value = DateTime.Now.Date;
            //cmd.Parameters["@INVDATE"].Value = Convert.ToDateTime("October 30, 2020");

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        static void BuildExcel(DataTable dt)
        {
            //Build the CSV file data as a Comma separated string.
            string csv = string.Empty;

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Tisdel");

            int row = 1;
            int col = 1;
            var rowFromWorksheet = ws.Row(row++);

            foreach (DataColumn column in dt.Columns)
            {
                rowFromWorksheet.Cell(col++).Value = column.ColumnName;
                       
            }

            foreach (DataRow datarow in dt.Rows)
            {
                rowFromWorksheet = ws.Row(row++);
                col = 1;

                foreach (DataColumn column in dt.Columns)
                {
                    if (col == 4)
                        rowFromWorksheet.Cell(col++).Value = "'" + datarow[column.ColumnName].ToString();
                    else 
                        rowFromWorksheet.Cell(col++).Value = datarow[column.ColumnName].ToString();
                }
            }

            workbook.SaveAs(@"C:\\SISM\\TisdelSpiff.xlsx"); ;

            return;

        }
    }
}
