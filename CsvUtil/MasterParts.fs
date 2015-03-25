

namespace CsvUtil
    module MasterParts =
        // Business Objects
        type MasterPart = {
            PartNo:string;
            Description:string;
            Orderable:bool;
            Width:double;
            Height:double;
        }

namespace CsvUtil.CSV
    open FSharp.Data
    open CsvUtil.MasterParts
    open CsvUtil.RailValidation
    module MasterParts =
        type PartList = CsvProvider<"C:\\Users\\mcgraw\\Source\\Repos\\CSVUtil\\Data\\partlist.csv">
        let LoadPartList (path:string) =
            try
                let result = PartList.Load(path)
                Success result
            with
                | ex -> Failure ex.Message

        let FileExists (path:string) =
            match System.IO.File.Exists(path) with
                | false -> Failure (sprintf "File not found at path: %s" path)
                | true -> Success "Connection to file established"

        let CsvToMasterPart (record:PartList.Row) =
            let mp : MasterPart = {
                PartNo = record.``Part Number``;
                Description = record.``Part Description``;
                Orderable = record.Orderable;
                Width = System.Convert.ToDouble(record.Width)
                Height = System.Convert.ToDouble(record.Height)
            }
            mp

namespace CsvUtil.SQL
    module MasterParts =
        open FSharp.Data
        open FSharp.Data.SqlClient
        open CsvUtil.MasterParts
        open CsvUtil.RailValidation

        //Literal connection string
        [<Literal>]
        let connectionString = @"Data Source=localhost\sqlexpress;Initial Catalog=FVMaster_Data;Integrated Security=True"

        // Create temp table
        [<Literal>]
        let createPartTempTableSql = @"
            IF OBJECT_ID('xMasterParts') IS NOT NULL
                DROP TABLE xMasterParts
            CREATE TABLE xMasterParts (MasterPartNo VARCHAR(35), Description VARCHAR(100), Ordered bit) "

        type CreatePartTempTableCommand = SqlCommandProvider<createPartTempTableSql, connectionString, ResultType.Records>

        [<Literal>]
        let commitPartTempTableSql = @"
            INSERT INTO MasterPartList (MasterPartNo, PartNoSuffix, Description, Ordered)
            SELECT MasterPartNo, '0000', Description, Ordered
            FROM xMasterParts
            DROP TABLE xMasterParts"

        type CommitPartTempTableCommand = SqlCommandProvider<commitPartTempTableSql, connectionString, ResultType.Records>

        [<Literal>]
        let rollbackPartTempTable = @"DROP TABLE xMasterParts"

        type RollbackPartTempTableCommand = SqlCommandProvider<rollbackPartTempTable, connectionString, ResultType.Records>

        [<Literal>]
        let insertPartSql = @"
            INSERT INTO xMasterParts (MasterPartNo, Description, Ordered)
            VALUES (@PartNo, @Desc, @Ordered)"

        type InsertPartCommand = SqlCommandProvider<insertPartSql, connectionString, ResultType.Records>

        [<Literal>]
        let getExistingPartNumbersSql = @"
            SELECT MasterPartNo
            FROM MasterPartList"

        type GetExistingPartNumbersCommand = SqlCommandProvider<getExistingPartNumbersSql, connectionString, ResultType.Records>

        [<Literal>]
        let testConnectionSql = @"
            SELECT 1"
        type TestConnectionCommand = SqlCommandProvider<testConnectionSql,connectionString>

        let TestConnection (connString:string) =
            try
                let cmd = new TestConnectionCommand(connString)
                cmd.Execute() |> ignore
                Success "Connection Established"
            with
                | ex -> Failure (sprintf "Connection Failed:%s" ex.Message)

        let AddPart (connString:string) (part:MasterPart) =
            try
                let cmd = new InsertPartCommand(connString)
                cmd.Execute(part.PartNo, part.Description, part.Orderable) |> ignore
                Success part
            with
                | _ -> Failure (sprintf "PartNo:%s failed to insert" part.PartNo)

        let CreatePartTempTable (connString:string) =
            try
                let cmd = new CreatePartTempTableCommand(connString)
                cmd.Execute() |> ignore
                Success "Query completed successfully!"
            with
                | ex -> Failure  (sprintf "Failed to create temp table: %s" ex.Message)

        let CommitPartTempTable (connString:string) =
            try
                let cmd = new CommitPartTempTableCommand(connString)
                cmd.Execute() |> ignore
                Success "Query completed successfully!"
            with
                | ex -> Failure  (sprintf "Failed to commit temp table: %s" ex.Message)

        let RollbackPartTempTable (connString:string) =
            try
                let cmd = new CommitPartTempTableCommand(connString)
                cmd.Execute() |> ignore
                Success "Query completed successfully!"
            with
                | ex -> Failure  (sprintf "Failed to rollback temp table: %s" ex.Message)

        let GetExistingPartNumbers (connString:string) =
            try
                let cmd = new GetExistingPartNumbersCommand(connString)
                Success (cmd.Execute())
            with
                | ex -> Failure (sprintf "Failed to Get Part Numbers: %s" ex.Message)