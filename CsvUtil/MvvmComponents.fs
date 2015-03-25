namespace CsvUtil.MVVM

module Components =
    open System.ComponentModel
    open System.Windows.Input
    open System

    type ViewModelBase() =
        let propertyChanged = Event<_, _>()

        interface INotifyPropertyChanged with
            [<CLIEvent>]
            member x.PropertyChanged = propertyChanged.Publish

        member me.NotifyPropertyChanged (myProperty:string) =
            propertyChanged.Trigger(me,new PropertyChangedEventArgs(myProperty))

    type RelayCommand (canExecute:(obj -> bool), action:(obj -> unit)) =
        let event = new DelegateEvent<EventHandler>()
        interface ICommand with
            [<CLIEvent>]
            member x.CanExecuteChanged = event.Publish
            member x.CanExecute arg = canExecute(arg)
            member x.Execute arg = action(arg)

    
    type ValidationLevel =
        | Suceess
        | Warning
        | Failure

    type DisplayItem (item, errorText, errorLevel)  =
        inherit ViewModelBase()
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