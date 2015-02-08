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
