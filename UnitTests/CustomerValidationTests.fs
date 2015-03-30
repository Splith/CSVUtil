namespace CsvUtil.UnitTests
    module Customers =
        open NUnit.Framework


        type CSID = CsvUtil.Customers.CustomerSiteID

        let custIDs : CSID[] = 
            [| 
            {CustomerID = "A"; SiteID = "A"} 
            {CustomerID = "B"; SiteID = "B"} 
            {CustomerID = "C"; SiteID = "C"} 
            {CustomerID = "D"; SiteID = "D"} 
            |]

        let CustomerValidationUnique = CsvUtil.Customers.GetMatchingSites custIDs
        [<Test>]
        let ``Customer Cannot Be Double Added`` () =
            let x : CSID = {CustomerID = "A"; SiteID = "B"}
            Assert.IsTrue(CsvUtil.Customers.CompareCustomerSiteIDs x x)
    
        [<Test>]
        let ``Different Customers Compare False`` () =
            let x : CSID = {CustomerID = "A"; SiteID = "B"}
            let y : CSID = {CustomerID = "Z"; SiteID = "Y"}
            Assert.IsFalse(CsvUtil.Customers.CompareCustomerSiteIDs x y)
    
        [<Test>]
        let ``Same Customers Compare True`` () =
            let x : CSID = {CustomerID = "A"; SiteID = "B"}
            let y : CSID = {CustomerID = "A"; SiteID = "B"}
            Assert.IsTrue(CsvUtil.Customers.CompareCustomerSiteIDs x y)

        [<Test>]
        let ``Failed When Adding Existing Customer`` () =
            let x : CSID = {CustomerID = "A"; SiteID = "A"}
            Assert.AreEqual(1, CustomerValidationUnique x)
    
        [<Test>]
        let ``Success When Adding New Customer`` () =
            let x : CSID = {CustomerID = "Z"; SiteID = "Not The Same"}
            Assert.AreEqual(0, CustomerValidationUnique x)

        [<Test>]
        let ``Valid Phone Numbers`` () = 
            let valContactPhone = CsvUtil.Customers.ValidatePhoneNumber
            Assert.IsTrue(valContactPhone "5853090942")
            Assert.IsTrue(valContactPhone "1(585)309-9042")
            Assert.IsTrue(valContactPhone "1 (585)309-9042")
            Assert.IsTrue(valContactPhone "1 .585.309.9042")
            Assert.IsTrue(valContactPhone "585.309.9042")
            Assert.IsTrue(valContactPhone "5853090942  ")
            Assert.IsTrue(valContactPhone "  15853090942")
            Assert.IsTrue(valContactPhone "5853090942,")
            Assert.IsTrue(valContactPhone "")
            Assert.IsTrue(valContactPhone " ")

        [<Test>]
        let ``Invalid Phone Numbers`` () =
            let valContactPhone = CsvUtil.Customers.ValidatePhoneNumber
            Assert.IsFalse(valContactPhone "3090942")
            Assert.IsFalse(valContactPhone "PH: (309)1230942")
            Assert.IsFalse(valContactPhone "FX: 5853090942")