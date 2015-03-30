namespace CsvUtil
    module ValidationTypes =
        open RailValidation

        let MatchingElementsInSet compare items target =
            let curriedCompare = compare target
            Array.filter curriedCompare items

        // Public functions
        let MatchingInSet comparitor elements target =
            match elements |> Seq.map comparitor |> Seq.filter (fun t -> t) |> (fun x -> Seq.length x)  with
                | 0 -> Success target
                | _ -> Failure "More than one elements exists in collection"