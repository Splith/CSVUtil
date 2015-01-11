namespace CsvUtil
    module Contacts =
        open FSharp.Data

        type ContactType =
            | Employee = -4
            | Shipping = -3
            | Financial = -2
            | General = -1

        // Compile time connection string, used for static type provider
        [<Literal>]
        let connectionString = @"Data Source=localhost\sqlexpress;Initial Catalog=FVMaster_Data;Integrated Security=True"

        // Commands
        [<Literal>]
        let insertContact = 
            @"INSERT INTO Contacts (CustomerID, SiteID, CustomerType, Name, Title, PhoneNumber, MobileNumber, FaxNumber, EmailAddress, ContactTypeID, Address1, Address2, City, State, ZipCode)
              VALUES (@customerID, @siteID, @customerType, @name, NULL, @phoneNumber, NULL, @faxNumber, @emailAddress, @contactTypeID, @address1, @address2, @city, @state, @zipCode)"
        
        type insertContact = SqlCommandProvider<insertContact, connectionString, ResultType.Records>

        let AddContactFromRow (connString:string) (row:CSV.Customers.Row) =
            let cmd = new insertContact(connString)
            cmd.Execute((row.CustomerID.ToString()), (row.SiteID.ToString()), 0uy, row.Name, row.GeneralPhone, row.GeneralFax, row.GeneralEmail, int ContactType.General, row.GeneralFirstAddr, row.GeneralSecondAddr, row.GeneralCity, row.GeneralState, row.GeneralZip) |> ignore
            cmd.Execute((row.CustomerID.ToString()), (row.SiteID.ToString()), 0uy, row.Name, row.BillingPhone, row.BillingFax, row.BillingEmail, int ContactType.Financial, row.BillingFirstAddr, row.BillingSecondAddr, row.BillingCity, row.BillingState, row.BillingZip) |> ignore
            cmd.Execute((row.CustomerID.ToString()), (row.SiteID.ToString()), 0uy, row.Name, row.ShippingPhone, row.ShippingFax, row.ShippingEmail, int ContactType.Shipping, row.ShippingFirstAddr, row.ShippingSecondAddr, row.ShippingCity, row.ShippingState, row.ShippingZip) |> ignore