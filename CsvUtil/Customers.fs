// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

namespace CsvUtil
    module Customers =

        // Business Objects
        type CustomerSiteID = {CustomerID : System.String; SiteID : System.String}
        type Contact = 
            {
                FirstAddr : string;
                SecondAddr : string;
                City : string;
                State : string;
                Zip : string;
                Name : string;
                Phone : string;
                Fax : string;
                Email : string;
            }

        // Helper functions
        let CompareCustomerSiteIDs (a:CustomerSiteID) (b:CustomerSiteID) =
            a = b

        let GetMatchingSites (custIds:CustomerSiteID[]) (targetIds:CustomerSiteID) =
            custIds |> Array.filter (fun t -> CompareCustomerSiteIDs t targetIds) |> Array.length
            
        let ValidatePhoneNumber phoneNumber =
            let regex = new System.Text.RegularExpressions.Regex(@"^\W*1?\W*([2-9][0-8][0-9])\W*([2-9][0-9]{2})\W*([0-9]{4})(\se?x?t?(\d*))?\W*$")
            regex.IsMatch(phoneNumber) || System.String.IsNullOrWhiteSpace(phoneNumber)

        
namespace CsvUtil.CSV
    open FSharp.Data
    module Customers =
        type Customers = CsvProvider<"../Data/customers_vkw.csv">
        type CsvCust = Customers.Row
        let Load (fName:string) = 
            Customers.Load(fName)


namespace CsvUtil.SQL
    module Customers =
        open FSharp.Data.SqlClient
        open FSharp.Data
        open Microsoft.FSharp.Data.TypeProviders


        // Compile time connection string, used for static type provider
        [<Literal>]
        let connectionString = @"Data Source=localhost\sqlexpress;Initial Catalog=FVMaster_Data;Integrated Security=True"

        // Insert command for customers
        [<Literal>]
        let insertString = 
            @"INSERT INTO xCustHolding (CustomerID, Name, SiteID, SiteName, Terms, CreditLimit, CurrencyCulture, LocationID)
              VALUES (@custID, @name, @siteID, @siteName, @terms, @creditLimit, 'en-US', 'MAIN')"

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
            @"SELECT DISTINCT CustomerID, SiteID FROM Customers WHERE 
              TypeID = 0"

        // Type Providers
        type insertCustomer = SqlCommandProvider<insertString, connectionString>
        type createTempTable = SqlCommandProvider<createTempTableString, connectionString>
        type commitCustomers = SqlCommandProvider<commitCustomersString, connectionString>
        type getUniqueCustomerSite = SqlCommandProvider<getUniqueCustomerSiteString, connectionString>

        type ContactType = SqlEnumProvider<"SELECT Description, ID FROM ContactTypes", connectionString>

                    
       // Database accessor functions
        let GetCustomerSiteIDs (connString:string) =
            let getUniqueCmd = new getUniqueCustomerSite(connString)
            getUniqueCmd.Execute()
            
        let AddCustomer (connString:string) (cust:CsvUtil.CSV.Customers.CsvCust) = 
            let insertCmd = new insertCustomer(connString)
            insertCmd.Execute(cust.CustomerID.ToString(), cust.Name, cust.SiteID.ToString(), cust.SiteName,cust.Terms, if System.String.IsNullOrWhiteSpace(cust.CreditLimit) then 0.0M else System.Convert.ToDecimal(cust.CreditLimit))

        let CreateCustomerTempTable (connString:string) = 
            let createTempTableCmd = new createTempTable(connString)
            createTempTableCmd.Execute ()

        let CommitCustomer (connString:string) =
            let commitTableCmd = new commitCustomers(connString)
            commitTableCmd.Execute ()
