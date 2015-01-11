[<Literal>]
let connectionString = @"Data Source=localhost;Initial Catalog=FVMaster;Integrated Security=True"

let AddCustomersFromCSV (fileName:string) =
    let file = CsvUtil.CSV.Load fileName
    // Curry that shit
    let addCust = CsvUtil.Customers.AddCustomer connectionString
    for row in file.Rows do addCust row |> ignore
    for row in file.Rows do CsvUtil.Contacts.AddContactFromRow connectionString row |> ignore
    //Customers.csv.printCust fileName |> ignore

[<EntryPoint>]
let main args =
    for row in args do AddCustomersFromCSV row |> ignore
    0