namespace CsvUtil
module RailValidation =

    type Result<'TSuccess, 'TFailure> =
        | Success of 'TSuccess
        | Failure of 'TFailure

    let Bind switchFunction twoTrackInput = 
        match twoTrackInput with
            | Success s -> switchFunction s
            | Failure f -> Failure f
    
    let (>>=) twoTrackInput switchFunction =
        Bind switchFunction twoTrackInput


    let ResultToBoolean res =
        match res with
            | Success s -> true
            | Failure f -> false