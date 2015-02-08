// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

namespace CsvUtil.Customers
    open FSharp.Data
    module CSV =
        type Customers = CsvProvider<"../Data/customers_vkw.csv">

        let PrintCustomer (x:Customers.Row) =
            printfn "DATA: %i   %s  %i  %s" (System.Convert.ToInt32 x.CustomerID) x.Name (System.Convert.ToInt32 x.SiteID) x.SiteName

        let Load (fName:string) = 
            Customers.Load(fName)

        let PrintCustomerCSV (fName:string) = 
            let file = Load fName
            for row in file.Rows do PrintCustomer row

            System.Console.ReadKey()
    
    module Model =
        open FSharp.Data.SqlClient
        open FSharp.Data
        open Microsoft.FSharp.Data.TypeProviders
        // Compile time connection string, used for static type provider
        [<Literal>]
        let connectionString = @"Data Source=localhost\sqlexpress;Initial Catalog=FVMaster_Data;Integrated Security=True"

        // Insert command for customers
        // Note: Location is NULL so I can track which operations are part of this transaction, on failure kill all NULL locations
        [<Literal>]
        let insertString = 
            @"INSERT INTO xCustHolding (CustomerID, Name, SiteID, SiteName, Terms, CreditLimit, CurrencyCulture, LocationID)
              VALUES (@custID, @name, @siteID, @siteName, @terms, @creditLimit, 'en-US', NULL)"

        [<Literal>]
        let createTempTableString =
            @"	IF OBJECT_ID('xCustHolding') IS NOT NULL
	            BEGIN
		            DROP TABLE xCustHolding
	            END
	            CREATE TABLE xCustHolding (CustomerID VARCHAR(16), Name NVARCHAR(100), SiteID VARCHAR(16), SiteName NVARCHAR(100), Terms NVARCHAR(100), CreditLimit MONEY NULL, CurrencyCulture VARCHAR(20), LocationID VARCHAR(6))"

        [<Literal>]
        let commitCustomersString =
            @"INSERT INTO Customers (CustomerID, Name, SiteID, SiteName, Terms, CreditLimit, CurrencyCulture, LocationID)
              SELECT * FROM xCustHolding
              DROP TABLE xCustHolding"
        
        [<Literal>]
        let getUniqueCustomerSiteString = 
            @"SELECT DISTINCT CustomerID, SiteID FROM Customers WHERE TypeID = 0"

        // Type Providers with respective commands
        type insertCustomer = SqlCommandProvider<insertString, connectionString>
        type createTempTable = SqlCommandProvider<createTempTableString, connectionString>
        type commitCustomers = SqlCommandProvider<commitCustomersString, connectionString>
        type getUniqueCustomerSite = SqlCommandProvider<getUniqueCustomerSiteString, connectionString>
        type CustomerSiteID = {CustomerID : System.String; SiteID : System.String}

        // Helper functions
        let CompareCustomerSiteIDs (a:CustomerSiteID) (b:CustomerSiteID) =
            a.CustomerID = b.CustomerID && a.SiteID = b.SiteID

        // Public functions
        let ValidateCustomerUnique (custIds:CustomerSiteID[]) (targetIds:CustomerSiteID) =
            let matchingElements = custIds |> Array.filter (fun t -> CompareCustomerSiteIDs t targetIds) |> Array.length
            match matchingElements with
                | 0 -> "Success"
                | _ -> "Failure"

        let GetCustomerSiteIDs (connString:string) =
            let getUniqueCmd = new getUniqueCustomerSite(connString)
            getUniqueCmd.Execute()
            
        let AddCustomer (connString:string) (cust:CSV.Customers.Row) = 
            let insertCmd = new insertCustomer(connString)
            insertCmd.Execute(cust.CustomerID.ToString(), cust.Name, cust.SiteID.ToString(), cust.SiteName,cust.Terms, if System.String.IsNullOrWhiteSpace(cust.CreditLimit) then 0.0M else System.Convert.ToDecimal(cust.CreditLimit))

        let CreateCustomerTempTable (connString:string) = 
            let createTempTableCmd = new createTempTable(connString)
            createTempTableCmd.Execute ()

        let CommitCustomer (connString:string) =
            let commitTableCmd = new commitCustomers(connString)
            commitTableCmd.Execute ()