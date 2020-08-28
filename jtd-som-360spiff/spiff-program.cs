using System;
using System.Data.SqlClient;
using System.Data;
using jtd_utilities;

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

            // Write to File
            String csvContents = BuildCSV(dsHeaders);
            System.IO.File.WriteAllText(@"./TisdelSpiff.txt", csvContents);

            // Send the message
            jtd_utilities.mail.SpiffEmailMessage("Spiff Export Complete.");

            return;
        } 
        static DataTable SqlGetSerialList()
        {
            SqlCommand cmd = new SqlCommand(Properties.Settings.Default.SqlQuery, SQL.cnnSQL);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        static String BuildCSV(DataTable dt)
        {
            //Build the CSV file data as a Comma separated string.
            string csv = string.Empty;

            foreach (DataColumn column in dt.Columns)
            {
                //Add the Header row for CSV file.
                csv += column.ColumnName + ',';
            }

            //Add new line.
            csv += "\r\n";

            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn column in dt.Columns)
                {
                    //Add the Data rows.
                    csv += row[column.ColumnName].ToString().Replace(",", ";") + ',';
                }

                //Add new line.
                csv += "\r\n";
            }

            return (csv);

        }
    }
}
