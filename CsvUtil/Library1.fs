namespace CsvUtil.MVVM
    module ViewModel =
        // Include MVVM components
        open CsvUtil.MVVM.Components 

        type MainWindowViewModel() =
            inherit CsvUtil.MVVM.Components.ViewModelBase()
            let mutable _text : string = System.String.Empty

            member x.Text
                with get() = _text
                and set(value:string) =
                    _text <- value
                    x.NotifyPropertyChanged("Text")

            member x.Validate =
                new RelayCommand ((fun canExecute -> true),
                    (fun action-> x.Text <- "I am changing shit"))