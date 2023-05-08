using Library;

namespace Test;

[TestClass]
public class TestDepotConsumer
{
    [TestMethod]
    public void TestSingleDepot()
    {
        var depot = new Depot()
        {
            Data = { 0, },
        };
        var result = depot.GetConsumerAssignments(5);
        result.Span.AssertEqual(new[] { 1, 2, 3, 4 });
    }

    [TestMethod]
    public void TestTwoDepots()
    {
        var depot = new Depot()
        {
            Data = { 0, 2, },
        };
        var result = depot.GetConsumerAssignments(5);
        result.Span.AssertEqual(new[] { 1, 3, 4 });
    }
}
