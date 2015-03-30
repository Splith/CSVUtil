namespace CsvUtil.UnitTests
    module MasterParts =
        open NUnit.Framework
        open CsvUtil.MasterParts
        open CsvUtil.RailValidation
        open CsvUtil.CSV.MasterParts

        type csvMasterPart = CsvUtil.CSV.MasterParts.PartList.Row
        type myMasterPart = CsvUtil.MasterParts.MasterPart


        [<Test>]
        let ``Leading Trailing Whitespace`` () =
            let invalidPartNo = [
                " Leading Whitespace"; 
                "Trailing Whitespace "; 
                "In excess of over 35 characters or something, I cannot remember";
            ]

            let IsFailureResult result =
                match result with
                    | Success s -> false
                    | Failure f -> true

            let passedItems = List.filter (fun s -> ValidPartNo s |> IsFailureResult) invalidPartNo

            Assert.AreEqual(invalidPartNo.Length, passedItems.Length)

        [<Test>]
        let ``Valid MasterPartNo`` () =
            let validPartNo = [
                "MONO";
                "IG";
                "Gl060.CL.-.-";
                "Spaces are okay";
            ]

            let IsSuccessResult result =
                match result with
                    | Success s -> true
                    | Failure f -> false

            let passedItems = List.filter (fun s -> ValidPartNo s |> IsSuccessResult) validPartNo

            Assert.AreEqual(validPartNo.Length, passedItems.Length)