namespace CsvUtil.MVVM
    module ViewModel =
        // Include MVVM components
        open System.Collections.ObjectModel
        open CsvUtil.MVVM.Components 
        open CsvUtil.Customers
        open CsvUtil.RailValidation

        type ValidationLevel =
            | Suceess
            | Warning
            | Failure

        type DisplayItem (item, errorText, errorLevel)  =
            inherit CsvUtil.MVVM.Components.ViewModelBase()
            let mutable _item : string = item
            let mutable _errorText: string = errorText
            let mutable _errorLevel : ValidationLevel = errorLevel

            member x.Item
                with get () = _item
                and private set (value) = _item <- value

            member x.ErrorText
                with get () = _errorText
                and private set (value) = _errorText <- value

            member x.ErrorLevel
                with get () = _errorLevel
                and private set (value) = _errorLevel <- value

        type CustomerViewModel() =
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
                    (fun canExecute ->  not (System.String.IsNullOrWhiteSpace x.Text)), 
                    (fun i ->
                    match System.IO.File.Exists(x.Text) with
                        | false -> 
                            x.Errors.Clear()
                            x.Errors.Add(DisplayItem("File Not Found", "No file found at designated path", ValidationLevel.Failure))
                        | true -> 
                            let data = CsvUtil.CSV.Customers.Load(x.Text)
                            ()
                    )
                )