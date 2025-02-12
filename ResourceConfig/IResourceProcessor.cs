using CatConfig;

namespace ModuleResourceProvider;

public interface IResourceProcessor
{
	public string ResourceName { get; }
	public string ProcessorType { get; }
	public IUnit ProcessModule(IUnitRecord record,params object[] args);

}
