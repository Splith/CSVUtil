namespace CsvUtil
    module ValidationTypes =
        type CreationResult<'T> = Success of 'T | Error of string

        let MatchingElementsInSet compare items target =
            let curriedCompare = compare target
            Array.filter curriedCompare items