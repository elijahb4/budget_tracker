# Budget Tracker Application
- What the application does, how it works and how to run it

## Background to the application and key details
This is a local budget tracker application for Windows. It allows you to add particulars of bank accounts, transactions, and tax information. The application can use this information to provide information such as projections on how much you'll earn in interest in a year to help you manage your finances. It also generates data visualisations for you with your data.
You can set your own personal tax limits and fiscal year start/end.
The application also allows you to search for data on US stocks.

The application is written in C# and is a .NET Windows Presentation Foundation application which uses PostgreSQL for persistent database storage.
You can store data on your local machine or connect to remote data base, if you don't have a local database initialised on your local machine the software will initialise one for you if you choose to connect to localhost.

- Target .NET Version: 9.0
- C# Version: 13.0
- PostgreSQL Version: 17

## Prerequisites for running the software - ensure you have all before attempting to run the software
 - Windows 10 or later
 - .NET Desktop Runtime [learn more](https://learn.microsoft.com/en-us/dotnet/core/install/windows)
 - Your hardware must meet the requirements of the .NET Desktop Runtime - see link above for more information
 - PostgreSQL installed where you want the database to be (local or remote)
 