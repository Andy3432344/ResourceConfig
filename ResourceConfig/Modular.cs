namespace ModuleResourceProvider;

public static class Modular
{
	private static Dictionary<string, IResourceProcessor> processors = new(StringComparer.OrdinalIgnoreCase);

	public static bool RegisterModuleProvider(IResourceProcessor processor)
	{
		return processors.TryAdd(processor.ProcessorType, processor);
	}

	public static IResourceProcessor? GetModuleProcessor(string type)
	{
		if (processors.TryGetValue(type, out var processor))
			return processor;

		return null;

	}
}
