namespace CsvUtil.MVVM
    module MasterParts =
        // Include MVVM components
        open System.Collections.ObjectModel
        open CsvUtil
        open CsvUtil.MVVM.Components 
        open CsvUtil.RailValidation

        type PartsViewModel() =
            inherit CsvUtil.MVVM.Components.ViewModelBase()
            let mutable _text : string = System.String.Empty
            let mutable _connString : string = System.String.Empty
            let mutable _errors : ObservableCollection<DisplayItem> = new ObservableCollection<DisplayItem>()
            let mutable _validText : bool = false

            member x.Text
                with get() = _text
                and set(value:string) =
                    _text <- value
                    if System.IO.File.Exists(x.Text) then
                        x.ValidText <- true
                    else
                        x.ValidText <- false
                    x.NotifyPropertyChanged("Text")
                    x.NotifyPropertyChanged("Validate")

            member x.ValidText
                with get() = _validText
                and set(valid:bool) =
                    if _validText <> valid then
                        _validText <- valid
                        x.NotifyPropertyChanged("ValidText")

            member x.ConnString
                with get() = _connString
                and set(value:string) =
                    _connString <- value
                    x.NotifyPropertyChanged("ConnString")

            member x.Errors
                with get () = _errors
                and private set(value) =
                    _errors <- value
                    x.NotifyPropertyChanged("Errors")

            member x.Validate=
                new RelayCommand (
                    (fun canExecute ->  not (System.String.IsNullOrWhiteSpace x.Text) && not (System.String.IsNullOrWhiteSpace x.ConnString)), 
                    (fun i ->
                    x.Errors.Clear()
                    match CSV.MasterParts.FileExists x.Text with
                        | Success s ->
                            match SQL.MasterParts.TestConnection(x.ConnString) with
                                | Success s -> x.ParseFile x.ConnString x.Text
                                | Failure f -> x.Errors.Add(DisplayItem("Connection Failed", f, ValidationLevel.Failure))
                        | Failure f -> x.Errors.Add(DisplayItem("File Not Found", f, ValidationLevel.Failure))
                    )
                )

            member x.ParseFile connString filePath = 
                match CSV.MasterParts.LoadPartList filePath with
                    | Success s -> 
                        let masterParts = Seq.map CSV.MasterParts.CsvToMasterPart s.Rows
                        let partNos = Seq.map (fun (i:MasterParts.MasterPart) -> i.PartNo) masterParts
                        
                        match x.ValidateParts partNos with
                            | Success r -> x.Execute connString masterParts
                            | Failure f -> x.Errors.Add(DisplayItem("Failed Part Validation", f, ValidationLevel.Failure))
                    | Failure f -> x.Errors.Add(DisplayItem("CSV Load Failed", f, ValidationLevel.Failure))

            member x.ValidateParts parts =
                let partValResults = 
                    Seq.map (fun t -> 
                        match MasterParts.ValidPartNo t with
                            | Success s -> Success s
                            | Failure f -> Failure (t, f))
                        parts 
                    |> Seq.filter (fun i ->
                        ResultToBoolean i |> not)
                match Seq.length partValResults with
                    | 0 -> Success parts
                    | _ ->
                        for u in partValResults do
                            match u with
                                | Failure f -> 
                                    let part = fst f
                                    let error = snd f
                                    x.Errors.Add(DisplayItem((sprintf "PartNo: %s" part),error,ValidationLevel.Failure))
                                | Success s -> () // We should never get here
                        Failure "Part Validation Failed"

            member x.Execute connString masterParts =
                // curry down some functions to make the last step look pretty
                let AddPartCmd = SQL.MasterParts.AddPart connString

                // Helper functions
                let filterErrors res =
                    match res with
                        | Success s -> false
                        | Failure f -> true

                let addParts parts =
                    let results = Seq.map AddPartCmd parts
                    let errors = Seq.filter filterErrors results
                    match Seq.length errors with
                        | 0 -> Success "Everything worked!"
                        | _ -> Failure errors

                // Lets do this! LEEEEEEEEEEEEEEROY JENNNKINNS!
                SQL.MasterParts.CreatePartTempTable connString |> ignore
                match addParts masterParts with
                    | Success s -> 
                        match SQL.MasterParts.CommitPartTempTable connString with
                            | Success s -> x.Errors.Add(DisplayItem("Insert Complete", "MasterPart Insert Routine Finished!", ValidationLevel.Suceess))
                            | Failure f -> x.Errors.Add(DisplayItem("Committ Error", f, ValidationLevel.Failure))
                    | Failure f -> 
                        SQL.MasterParts.RollbackPartTempTable connString |> ignore
                        for error in f do
                            match error with
                                | Failure f -> x.Errors.Add(DisplayItem("Routine Error", f, ValidationLevel.Failure))
                                | _ -> x.Errors.Add(DisplayItem("Error", "Error found where error should not be!", ValidationLevel.Failure))

