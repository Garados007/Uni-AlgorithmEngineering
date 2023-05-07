using Library;
using Library.Parser;

namespace Test;

[TestClass]
public class TestDistanceMetrics
{
    [TestInitialize]
    public void Setup()
    {
        var path = Environment.GetEnvironmentVariable("TEST_DATA");
        Assert.IsNotNull(path);
        Assert.IsTrue(Directory.Exists(path), "data path should exists");
        Environment.CurrentDirectory = path;
    }

    private void TestCanonicalTour(string path, int expectedDistance)
    {
        if (!File.Exists(path))
            Assert.Fail($"file not found at {Path.GetFullPath(path)}");
            // Assert.Inconclusive("file not found");

        using var reader = new StreamReader(path);
        var graph = DataFile.Parse(reader);
        Assert.IsNotNull(graph);
        Assert.IsNotNull(graph.Specification.EdgeWeightType);
        Assert.IsNotNull(graph.NodeCoord2D);

        var distFunc = DistanceMetrics.GetFunc2D(graph.Specification.EdgeWeightType.Value);
        Assert.IsNotNull(distFunc);

        var coords = graph.NodeCoord2D!;
        var currentDist = 0;
        var from = coords[0];
        for (int i = 1; i < coords.Length; ++i)
        {
            var to = coords[i];
            currentDist += distFunc(from, to);
            from = to;
        }

        Assert.AreEqual(expectedDistance, currentDist, "distances should be equal");
    }

    [TestMethod]
    public void Test_pcb442()
    {
        Assert.Inconclusive("cannot verify number");
        TestCanonicalTour("TSPLIB95/tsp/pcb442.tsp", 221_440);
    }

    [TestMethod]
    public void Test_gr666()
    {
        Assert.Inconclusive("cannot verify number");
        TestCanonicalTour("TSPLIB95/tsp/gr666.tsp", 423_710);
    }

    [TestMethod]
    public void Test_att532()
    {
        Assert.Inconclusive("cannot verify number");
        TestCanonicalTour("TSPLIB95/tsp/att532.tsp", 309_636);
    }
}
