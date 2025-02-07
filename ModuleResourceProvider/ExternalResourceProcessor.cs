using CatConfig;
using CatConfig.CclUnit;
namespace ModuleResourceProvider;

public class ExternalResourceProcessor : IDelayedProcessor
{

	public ExternalResourceProcessor(IFileSystem fs)
	{
		_ = new ExternalResourceProvider(fs);
		Constructor.RegisterProcessor(this);
	}

	private const string ext = nameof(ext);
	private const string res = nameof(res);
	public string Name { get; } = ext;
	public string ProtocolSchema { get; } = res;

	public IUnit ResolveDelayedUnit(int id, string name, UnitPath path)
	{
		string resource = path[0];
		return ResourceEngine.GetResource(id, Name, resource, path[1..]);
	}
}