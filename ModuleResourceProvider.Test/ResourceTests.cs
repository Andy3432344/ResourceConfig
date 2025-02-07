using System.Reflection.Metadata;
using System.Text;
using CatConfig;
using CatConfig.CclParser;

namespace ModuleResourceProvider.Test;

public class ResourceTests
{
	string meta = TestHelpers.GetMeta('\t', 1, '=', '\0', '"') + '\n';
	string TestResourceModule =
"""
  module = Test_ResourceModule
  Type = test
  import =
  	file-path = 
  	std = res://mod/std
  exports =
  	= Component
  library =
  	Component = 
  		{Property} = 
  			URL = {std}/GetFileName/"{file-path}"
  """;

	TestProcessor processor = new();
	ModuleProcessor modProc;
	TestModuleProcessor testProc = new();

	public ResourceTests()
	{

		TestFileStream mod = new(UTF8Encoding.UTF8.GetBytes(meta + "modules=\n\t=std\n\t=Test_ResourceModule\nstd=std.mod\nTest_ResourceModule = Test_ResourceModule.mod"));//modules array
		TestFileStream std = new(UTF8Encoding.UTF8.GetBytes(meta + "module=std\nexports=\n\tGetFileName\nlibrary=\n\t{GetFileName}=\n\t\tURL=test://GetFileName/{FilePath}"));//standard mod
		TestFileStream test = new(UTF8Encoding.UTF8.GetBytes(meta + TestResourceModule));//a test module

		Dictionary<string, TestFileStream> files = new()
		{
			{ ".mod", mod },
			{ "std.mod", std },
			{"Test_ResourceModule.mod",test }
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

		var filenameValue = record["FileName"] as IDelayedUnit;
		Assert.NotNull(filenameValue);

		var filename = record[filenameValue]("/usr/bin/ls") as IUnitValue;
		Assert.NotNull(filename);

		Assert.Equal("ls", filename.Value);

	}

	[Fact]
	public void LoadedModuleResolvedByName()
	{

		string ccl = meta +
		"""
	Test =
		{Test} =
			URL = res://mod/Test_ResourceModule/"{FilePath}"/Component
	""";

		var parser = Parser.FromContent("", ccl);
		var unit = parser.ParseContent("", ccl);
		var record = unit as IUnitRecord;
		Assert.NotNull(record);

		var test = record["Test"] as IDelayedUnit;
		Assert.NotNull(test);

		var component = record[test]("/usr/bin/ls") as IUnitRecord;
		Assert.NotNull(component);

		var property = component["property"] as IUnitValue;
		Assert.NotNull(property);

		Assert.Equal("ls", property.Value);


	}

}

