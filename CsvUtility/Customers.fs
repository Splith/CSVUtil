// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

namespace CsvUtil
    open FSharp.Data
    module CSV =
        type Customers = CsvProvider<"customers_vkw.csv">

        let PrintCustomer (x:Customers.Row) =
            printfn "DATA: %i   %s  %i  %s" (System.Convert.ToInt32 x.CustomerID) x.Name (System.Convert.ToInt32 x.SiteID) x.SiteName

        let Load (fName:string) =
            Customers.Load(fName)

        let PrintCustomerCSV (fName:string) = 
            let file = Load fName
            for row in file.Rows do PrintCustomer row

            System.Console.ReadKey()
    
    module Customers =
        open FSharp.Data
        // Compile time connection string, used for static type provider
        [<Literal>]
        let connectionString = @"Data Source=localhost\sqlexpress;Initial Catalog=FVMaster_Data;Integrated Security=True"
        // Commands
        [<Literal>]
        let insertString = 
            @"INSERT INTO Customers (CustomerID, Name, SiteID, SiteName, Terms, CreditLimit, CurrencyCulture, LocationID)
              VALUES (@custID, @name, @siteID, @siteName, @terms, @creditLimit, 'en-US', 'MAIN')"
        [<Literal>]
        let getCustomerGuidString = 
            @"SELECT CustomerGUID FROM Customers WHERE CustomerID = @customerID AND SiteID = @siteID"
        
        // Type Providers with respective commands
        type getCustomerGuid = SqlCommandProvider<getCustomerGuidString, connectionString, ResultType.Records>
        let getCustomerGuidCmd = new getCustomerGuid(connectionString)
        type insertCustomer = SqlCommandProvider<insertString, connectionString>
        let insertCmd = new insertCustomer(connectionString)

        // Public functions
        let AddCustomer (connString:string) (cust:CSV.Customers.Row) = 
            let insertCmd = new insertCustomer(connString)
            insertCmd.Execute(cust.CustomerID.ToString(), cust.Name, cust.SiteID.ToString(), cust.SiteName,cust.Terms, if System.String.IsNullOrWhiteSpace(cust.CreditLimit) then 0.0M else System.Convert.ToDecimal(cust.CreditLimit))
        
        let CustomerGUID (connString:string) (siteID:string) (customerID:string) =
            let getCustomerGuidCmd = new getCustomerGuid(connString)
            getCustomerGuidCmd.Execute(customerID, siteID) |> Seq.head 
        