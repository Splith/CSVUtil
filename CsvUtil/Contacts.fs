namespace CsvUtil
    module Contacts =
        open FSharp.Data

        // Compile time connection string, used for static type provider
        [<Literal>]
        let connectionString = @"Data Source=localhost\sqlexpress;Initial Catalog=FVMaster_Data;Integrated Security=True"
        
        type ContactType = SqlEnumProvider<"SELECT Description, ID FROM ContactTypes", connectionString>

        // Commands
        [<Literal>]
        let insertContactString = 
            @"INSERT INTO xContactHolding (CustomerID, SiteID, CustomerType, Name, Title, PhoneNumber, MobileNumber, FaxNumber, EmailAddress, ContactTypeID, Address1, Address2, City, State, ZipCode)
              VALUES (@customerID, @siteID, @customerType, @name, NULL, @phoneNumber, NULL, @faxNumber, @emailAddress, @contactTypeID, @address1, @address2, @city, @state, @zipCode)"

        [<Literal>]
        let createTempTableString =
            @"IF OBJECT_ID('xContactHolding') IS NOT NULL
	          BEGIN
		          DROP TABLE xContactHolding
	          END
	          CREATE TABLE xContactHolding (CustomerID VARCHAR(16), SiteID VARCHAR(16), CustomerType TINYINT, Name VARCHAR(50), Title NVARCHAR(25), PhoneNumber NVARCHAR(50), MobileNumber NVARCHAR(20), FaxNumber NVARCHAR(40), EmailAddress NVARCHAR(100), ContactTypeID INT, Address1 NVARCHAR(150), Address2 NVARCHAR(150), City NVARCHAR(50), State NVARCHAR(30), ZipCode NVARCHAR(15))"
        
        [<Literal>]
        let commitContactsString =
            @"INSERT INTO Contacts (CustomerID, SiteID, CustomerType, Name, Title, PhoneNumber, MobileNumber, FaxNumber, EmailAddress, ContactTypeID, Address1, Address2, City, State, ZipCode)
              SELECT *
              FROM xContactHolding"

        type insertContact = SqlCommandProvider<insertContactString, connectionString, ResultType.Records>
        type createContactTempTable = SqlCommandProvider<createTempTableString, connectionString, ResultType.Records>
        type commitContacts = SqlCommandProvider<commitContactsString, connectionString, ResultType.Records>

        let AddContactFromRow (connString:string) (row:CsvUtil.CSV.Customers.CsvCust) =
            let cmd = new insertContact(connString)
            cmd.Execute((row.CustomerID.ToString()), (row.SiteID.ToString()), 0uy, row.Name, row.GeneralPhone, row.GeneralFax, row.GeneralEmail, ContactType.``{General}``, row.GeneralFirstAddr, row.GeneralSecondAddr, row.GeneralCity, row.GeneralState, row.GeneralZip) |> ignore
            cmd.Execute((row.CustomerID.ToString()), (row.SiteID.ToString()), 0uy, row.Name, row.BillingPhone, row.BillingFax, row.BillingEmail, ContactType.``{Financial}``, row.BillingFirstAddr, row.BillingSecondAddr, row.BillingCity, row.BillingState, row.BillingZip) |> ignore
            cmd.Execute((row.CustomerID.ToString()), (row.SiteID.ToString()), 0uy, row.Name, row.ShippingPhone, row.ShippingFax, row.ShippingEmail, ContactType.``{Shipping}``, row.ShippingFirstAddr, row.ShippingSecondAddr, row.ShippingCity, row.ShippingState, row.ShippingZip) |> ignore
        
        let CreateContactTempTable (connString:string) =
            let cmd = new createContactTempTable(connString)
            cmd.Execute() |> ignore

        let CommitContact (connString:string) =
            let cmd = new commitContacts(connString)
            cmd.Execute() |> ignore