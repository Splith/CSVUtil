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

            member x.Text
                with get() = _text
                and set(value:string) =
                    _text <- value
                    x.NotifyPropertyChanged("Text")
                    x.NotifyPropertyChanged("Validate")

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
                    | Success s -> x.Execute connString (Seq.map CSV.MasterParts.CsvToMasterPart s.Rows)
                    | Failure f -> x.Errors.Add(DisplayItem("CSV Load Failed", f, ValidationLevel.Failure))

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
                            | Success s -> x.Errors.Clear()
                            | Failure f -> x.Errors.Add(DisplayItem("Committ Error", f, ValidationLevel.Failure))
                    | Failure f -> 
                        SQL.MasterParts.RollbackPartTempTable connString |> ignore
                        for error in f do
                            match error with
                                | Failure f -> x.Errors.Add(DisplayItem("Routine Error", f, ValidationLevel.Failure))
                                | _ -> x.Errors.Add(DisplayItem("Error", "Error found where error should not be!", ValidationLevel.Failure))

