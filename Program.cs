using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;

namespace DataRetrieval_Transfer_SQL_CSV_FTP
{
    internal class Program
    {
        static void Main(string[] args)
        {

            // Build the configuration
            var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>() 
    .Build();




            string serverName = "LENEVO\\SQLEXPRESS";
            string databaseName = "InterraTest";
            string connectionString = $"Server={serverName};Database={databaseName};Integrated Security=True;";
            string tableName = "User";


            //retrieving data from SQL server
            List<UserData> userDataList = RetrieveDataFromDatabase(connectionString, tableName);

            //  pathto save the CSV file
            string csvFilePath = "output.csv";

            //writing retrieved data to a csv file
            WriteDataToCSV(userDataList, csvFilePath);


            string ftpServerUrl = configuration["ftpServerUrl"];
            string ftpUsername = configuration["ftpUsername"];
            ;
            string ftpPassword = configuration["ftpPassword"];
            string localFilePath = "output.csv"; // Local file path
            string remoteFileName = "output.csv"; // Name to use on the FTP server

            //uploading the csv file to remote ftp server
            UploadFileToFTP(ftpServerUrl, ftpUsername, ftpPassword, localFilePath, remoteFileName);



            string remoteFilePath = "output.csv"; // Path to the remote CSV file
            string localFilePathD = "download/output.csv"; // Path  to save the file locally

            //downloading csv file from remote ftp server
            DownloadFileFromFTP(ftpServerUrl, ftpUsername, ftpPassword, remoteFilePath, localFilePathD);


            string downloadedCsvFilePath = localFilePathD; // Path to the downloaded CSV file


            //Retrieving data from downloaded csv file 
            DataTable csvData = ReadCSV(downloadedCsvFilePath);


            // Assuming 'csvData' DataTable is already populated
            string copyTableName = "UserCopy"; // Name of the SQL Server table

            string keyColumnName = "ID"; // Replace with your key column name

            RemoveExistingRowsFromCsvData(csvData, connectionString, copyTableName, keyColumnName);


            //writing retrieved data to sql server

            if (csvData != null)
            {
                WriteDataTableToSQL(csvData, connectionString, copyTableName);
            }



        }




        static List<UserData> RetrieveDataFromDatabase(string connectionString, string tableName)
        {
            List<UserData> userDataList = new List<UserData>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = $"SELECT * FROM [{tableName}]";

                SqlCommand command = new SqlCommand(sqlQuery, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(reader.GetOrdinal("ID"));
                        string name = reader.GetString(reader.GetOrdinal("Name"));

                        userDataList.Add(new UserData { ID = id, Name = name });
                    }
                }
            }

            return userDataList;
        }

        static void WriteDataToCSV(List<UserData> userDataList, string csvFilePath)
        {
            // Create a StreamWriter to write to the CSV file
            using (StreamWriter writer = new StreamWriter(csvFilePath))
            {
                // Write header row
                writer.WriteLine("ID,Name"); // Replace with your column names

                foreach (var userData in userDataList)
                {
                    // Write data to CSV file
                    writer.WriteLine($"{userData.ID},{userData.Name}");
                }
            }

            Console.WriteLine("Data exported to CSV file: " + csvFilePath);
        }

        class UserData
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }


        public static void UploadFileToFTP(string ftpServerUrl, string ftpUsername, string ftpPassword, string localFilePath, string remoteFileName)
        {
            try
            {
                // Create FTP request
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(new Uri(ftpServerUrl + "/" + remoteFileName));
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                ftpRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                // Read the local file
                using (FileStream localFileStream = new FileStream(localFilePath, FileMode.Open))
                {
                    using (Stream requestStream = ftpRequest.GetRequestStream())
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead;
                        while ((bytesRead = localFileStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            requestStream.Write(buffer, 0, bytesRead);
                        }
                    }
                }

                // Get the FTP response
                FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                Console.WriteLine($"Upload File Complete, Status: {ftpResponse.StatusDescription}");
                ftpResponse.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
            }
        }


        public static void DownloadFileFromFTP(string ftpServerUrl, string ftpUsername, string ftpPassword, string remoteFilePath, string localFilePath)
        {
            try
            {
                // Create FTP request
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(new Uri(ftpServerUrl + "/" + remoteFilePath));
                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                ftpRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                // Get the FTP response
                using (FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
                {
                    using (Stream responseStream = ftpResponse.GetResponseStream())
                    {
                        using (FileStream localFileStream = new FileStream(localFilePath, FileMode.Create))
                        {
                            byte[] buffer = new byte[1024];
                            int bytesRead;
                            while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                localFileStream.Write(buffer, 0, bytesRead);
                            }
                        }
                    }

                    Console.WriteLine($"File downloaded successfully. Status: {ftpResponse.StatusDescription}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading file: {ex.Message}");
            }
        }


        public static DataTable ReadCSV(string csvFilePath)
        {
            try
            {
                DataTable csvData = new DataTable();

                using (StreamReader reader = new StreamReader(csvFilePath))
                {
                    string[] headers = reader.ReadLine().Split(',');
                    foreach (string header in headers)
                    {
                        csvData.Columns.Add(header);
                    }

                    while (!reader.EndOfStream)
                    {
                        string[] data = reader.ReadLine().Split(',');
                        DataRow row = csvData.NewRow();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            row[i] = data[i];
                        }
                        csvData.Rows.Add(row);
                    }
                }

                return csvData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading CSV: {ex.Message}");
                return null;
            }
        }

        public static void WriteDataTableToSQL(DataTable dataTable, string connectionString, string tableName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.KeepIdentity))
                    {
                        bulkCopy.DestinationTableName = tableName;

                        foreach (DataColumn column in dataTable.Columns)
                        {
                            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                        }

                        bulkCopy.WriteToServer(dataTable);
                    }
                }

                Console.WriteLine("Data imported from DataTable to SQL Server.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing data to SQL Server: {ex.Message}");
            }
        }



        public static void RemoveExistingRowsFromCsvData(DataTable csvData, string connectionString, string tableName, string keyColumnName)
        {
            // Define a list to store the keys from the database table
            List<int> databaseKeys = new List<int>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand selectCommand = new SqlCommand($"SELECT {keyColumnName} FROM [{tableName}]", connection))
                    {
                        using (SqlDataReader reader = selectCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int databaseKey = reader.GetInt32(0); // Assuming the key column is of type int
                                databaseKeys.Add(databaseKey);

                            }
                        }
                    }
                }

                // Iterate through 'csvData' and remove rows with keys that exist in 'databaseKeys'
                for (int i = csvData.Rows.Count - 1; i >= 0; i--)
                {
                    //Console.WriteLine(csvData.Rows[i][keyColumnName].GetType());
                    int csvKey = Int32.Parse(csvData.Rows[i][keyColumnName].ToString());
                    //Console.WriteLine(csvKey);
                    if (databaseKeys.Contains(csvKey))
                    {
                        csvData.Rows.RemoveAt(i);
                    }
                }

                Console.WriteLine("Removed duplicate rows from 'csvData' based on database keys.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving database keys: {ex.Message}");
            }
        }



    }
}
