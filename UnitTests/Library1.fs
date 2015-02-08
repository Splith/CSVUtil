module UnitTests
open NUnit.Framework


type CSID = CsvUtil.Customers.Model.CustomerSiteID

let custIDs : CSID[] = 
    [| 
    {CustomerID = "A"; SiteID = "A"} 
    {CustomerID = "B"; SiteID = "B"} 
    {CustomerID = "C"; SiteID = "C"} 
    {CustomerID = "D"; SiteID = "D"} 
    |]

let CustomerValidationUnique = CsvUtil.Customers.Model.ValidateCustomerUnique custIDs
[<Test>]
let ``Customer Cannot Be Double Added`` () =
    let x : CSID = {CustomerID = "A"; SiteID = "B"}
    Assert.IsTrue(CsvUtil.Customers.Model.CompareCustomerSiteIDs x x)
    
[<Test>]
let ``Different Customers Compare False`` () =
    let x : CSID = {CustomerID = "A"; SiteID = "B"}
    let y : CSID = {CustomerID = "Z"; SiteID = "Y"}
    Assert.IsFalse(CsvUtil.Customers.Model.CompareCustomerSiteIDs x y)
    
[<Test>]
let ``Failed When Adding Existing Customer`` () =
    let x : CSID = {CustomerID = "A"; SiteID = "A"}
    Assert.AreEqual("Failure", CustomerValidationUnique x)
    
[<Test>]
let ``Success When Adding New Customer`` () =
    let x : CSID = {CustomerID = "Z"; SiteID = "Not The Same"}
    Assert.AreEqual("Success", CustomerValidationUnique x)