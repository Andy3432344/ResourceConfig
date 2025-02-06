using CatConfig;
using ModuleResourceProvider;
using System.Reflection;

namespace ResourceProcessor;

public interface IResource
{
	string Name { get; }
	IResource[] Resources { get; }
}



public record Resource(string Name, IResource[] Resources) : IResource;

public abstract class BaseModuleProcessor : IResourceProcessor
{

	public BaseModuleProcessor()
	{
		Modular.RegisterModuleProvider(this);
	}

	public abstract string ResourceName{ get; }
	public abstract string ProcessorType { get; }

	public virtual IUnit ProcessModule(IUnitRecord record, params object[] args)
	{
		var lib = record["library"];
		var import = record["import"];
		var result = lib;
		if (args.Length > 0)
		{
			int index = 0;
			while (result is IUnitRecord sub && index < args.Length)
			{
				string component = args[index].ToString() ?? "";
				result = sub[component];
				index++;
			}
		}
		return result;
	}


}

