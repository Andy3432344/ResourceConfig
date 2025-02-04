using System.Text;
using CatConfig;
using CatConfig.CclParser;
using CatConfig.CclUnit;

namespace ModuleResourceProvider.Test;

public class ResourceTests
{
    string meta = TestHelpers.GetMeta('\t', 1, '=', '\0', '"') + '\n';
    TestProcessor processor = new();
    ModuleProcessor modProc;

    public ResourceTests()
    {
        TestFileStream mod = new(UTF8Encoding.UTF8.GetBytes(meta + "modules=\n\t=std\nstd=std.mod"));
        TestFileStream std = new(UTF8Encoding.UTF8.GetBytes(meta + "module=std\nexports=\n\tGetFileName\nlibrary=\n\t{GetFileName}=\n\t\tURL=test://GetFileName/{FilePath}"));

        Dictionary<string, TestFileStream> files = new()
        {
            { "mod.mod", mod },
            { "std", std }
        };

        TestFileSystem fs = new(files);
        modProc = new(fs);
    }


    [Fact]
    public void ResourceFetchedToDetermineUnitValue()
    {
        var ccl = meta + "File=\n\t{FileName}=\n\t\tURL=res://mod/std/GetFileName/\"{FilePath}\"";
        var parser = Parser.FromContent("", ccl);
        var unit = parser.ParseContent("", ccl);

        var record = unit as IUnitRecord;
        Assert.NotNull(record);

        var func = record["FileName"] as IDelayedUnit;
        Assert.NotNull(func);

        var filename = record[func]("/usr/bin/ls") as IUnitValue;
        Assert.NotNull(filename);

        Assert.Equal("ls", filename.Value);

    }


}

public class TestProcessor : IDelayedProcessor
{
    public TestProcessor()
    {
        Constructor.RegisterProcessor(this);
    }
    public string Name => "GetFileName";
    public string ProtocolSchema => "test";

    public IUnit ResolveDelayedUnit(int id, string name, UnitPath path)
    {
        return new UnitValue(id, Path.GetFileNameWithoutExtension(path));
    }
}
