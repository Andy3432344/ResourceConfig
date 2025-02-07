using System.Security.AccessControl;
using System.Text;
using CatConfig;
using CatConfig.CclParser;
using CatConfig.CclUnit;
namespace ModuleResourceProvider;
public class ModuleProcessor : IDelayedProcessor
{
	private const string mod = "mod";
	private const string res = "res";
	private readonly IUnitRecord config;
	private readonly IFileSystem fs;

	public string Name => mod;
	public string ProtocolSchema => res;

	public ModuleProcessor(IFileSystem fs)
	{
		string ccl = GetFileContent(fs, $".{mod}");
		var parser = Parser.FromContent("", ccl);
		var unit = parser.ParseContent("", ccl);
		config = unit as IUnitRecord
			?? new NoRecord();
		this.fs = fs;
		setup();
		Constructor.RegisterProcessor(this);
	}

	private void setup()
	{
		var modules = config["modules"];
		List<string> mods = new();

		if (modules is IUnitArray m)
			mods.AddRange(m.Elements.Select(e => (e as IUnitValue)?.Value ?? ""));
		else if (modules is IUnitValue unit)
			mods.Add(unit.Value);


		foreach (var mod in mods)
		{
			var filename = config[mod] as IUnitValue;
			if (filename != null)
				_ = new ModuleResourceProvider(filename.Value, fs);
		}



	}

	private static string GetFileContent(IFileSystem fs, string path)
	{
		var file = fs.GetFileAtPath(path);
		byte[] data = new byte[file.Length];

		int length = (int)long.Clamp(file.Length, 0, Int32.MaxValue);
		file.Read(data, 0, length);
		string ccl = UTF8Encoding.UTF8.GetString(data);
		return ccl;
	}

	public IUnit ResolveDelayedUnit(int id, string name, UnitPath path)
	{
		string resource = path[0];

		return ResourceEngine.GetResource(id, mod, resource, path[1..]);
	}
}
