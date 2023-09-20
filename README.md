
Data Retrieval, Transfer, and Synchronization between SQL and FTP
This C# project performs data retrieval, transfer, and synchronization between a SQL database and a remote FTP server using CSV files.It allows you to retrieve data from a SQL database, export it to a CSV file, upload the CSV file to an FTP server, download the CSV file from the FTP server, read it back into memory, filter duplicates, and write the new/updated data back to the SQL database, effectively synchronizing the data between the SQL and FTP locations.

Prerequisites
Before running the project, make sure you have the following:

.NET Core SDK installed
SQL Server installed and running
FTP server credentials (server URL, username, and password)
Configuration
The project uses the appsettings.json file for configuration.You need to provide the following information:

serverName: The name of the SQL Server instance.
databaseName: The name of the SQL database.
tableName: The name of the SQL table to retrieve data from.
ftpServerUrl: The URL of the FTP server.
ftpUsername: The username for the FTP server.
ftpPassword: The password for the FTP server.
Usage
Build and run the project using the .NET Core CLI or an IDE of your choice.

The project will retrieve data from the specified SQL database table and export it to a CSV file named output.csv in the project directory.

The CSV file will be uploaded to the specified FTP server using the provided credentials.

The project will then download the CSV file from the FTP server and save it to the download directory as output.csv.

The downloaded CSV file will be read back into memory and any duplicate rows will be filtered out.

The filtered CSV data will be written back to the SQL database table, effectively synchronizing the data between the SQL and FTP locations.

Code Structure
The project consists of the following files:

Program.cs: Contains the main entry point and the implementation of the data retrieval, transfer, and synchronization logic.
UserData.cs: Defines the UserData class representing the data structure of the SQL table.
appsettings.json: Configuration file for specifying the SQL server, database, table, FTP server, and credentials.
output.csv: The output CSV file generated during the data export process.
download/output.csv: The downloaded CSV file from the FTP server.
Dependencies
The project uses the following dependencies:

Microsoft.Extensions.Configuration: For reading configuration settings from appsettings.json.
System.Data: For working with ADO.NET and DataTable.
System.Net: For FTP operations.
System.IO: For file operations.
Limitations
The project assumes that the SQL table has two columns: ID (integer) and Name (string). You may need to modify the code to match your table structure.
The project uses a simple CSV format with comma-separated values. If your data contains special characters or complex structures, you may need to modify the code to handle those cases.
The project does not handle errors or exceptions gracefully. You may need to add proper error handling and logging based on your requirements.
Conclusion
This project provides a basic framework for retrieving data from a SQL database, exporting it to a CSV file, transferring the CSV file to an FTP server, and synchronizing the data between the SQL and FTP locations.You can use this project as a starting point and customize it according to your specific needs.