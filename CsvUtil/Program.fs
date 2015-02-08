namespace CsvUtil

    module Routine =
        [<Literal>]
        let connectionString = @"Data Source=localhost\sqlexpress;Initial Catalog=FVMaster_Data;Integrated Security=True"

        let AddCustomersFromCSV (fileName:string) =
            let file = CsvUtil.Customers.CSV.Load fileName
    
            // Validate data in spreadsheet to make sure it will 
            // import well and work with FeneVision applications

            // Curry that shit
            let addCust = CsvUtil.Customers.Model.AddCustomer connectionString
            let addContact = CsvUtil.Contacts.AddContactFromRow connectionString

            // Construct xTables for storing data prior to final transaction
            CsvUtil.Customers.Model.CreateCustomerTempTable(connectionString) |> ignore
            CsvUtil.Contacts.createTempTableString |> ignore

            // Add records to xTables
            for row in file.Rows do addCust row |> ignore
            for row in file.Rows do CsvUtil.Contacts.AddContactFromRow connectionString row |> ignore

            // Commit data to make transaction atomic
            CsvUtil.Customers.Model.CommitCustomer(connectionString) |> ignore
            CsvUtil.Contacts.CommitContact(connectionString) |> ignore

        //[<EntryPoint>]
        //let main args =
            //for row in args do AddCustomersFromCSV row |> ignore
            //0