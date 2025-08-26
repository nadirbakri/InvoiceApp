# InvoiceWebApp — Getting Started

## Prerequisites
- Visual Studio 2019 (or newer) with **.NET Framework 4.7.2 Developer Pack**
- SQL Server (LocalDB / SQLEXPRESS / any SQL Server instance)
- NuGet package restore enabled

## Steps
1. Open the solution in Visual Studio.
2. Restore NuGet packages (on first build VS will restore automatically).
3. Create the database `assessmentdb` on your SQL Server and **run the provided SQL script** to create tables and seed data (`msproduct`, `mssales`, `mscourier`, `mspayment`, `ltcourierfee`, `trinvoice`, `trinvoicedetail`).
4. Set the connection string in `Web.config` (root):
   ```xml
   <connectionStrings>
     <add name="AppDb"
          connectionString="Data Source=YOUR_SERVER;Initial Catalog=assessmentdb;Integrated Security=True;MultipleActiveResultSets=True"
          providerName="System.Data.SqlClient" />
   </connectionStrings>
  
5. Replace YOUR_SERVER with your SQL Server instance (e.g., .\SQLEXPRESS).
6. Build the solution (Rebuild).
7. Run the app (F5).
