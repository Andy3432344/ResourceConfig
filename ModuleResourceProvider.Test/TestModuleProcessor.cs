namespace ModuleResourceProvider.Test;

public class TestModuleProcessor : BaseModuleProcessor
{

	public TestModuleProcessor()
	{
		Modular.RegisterModuleProvider(this);
	}
	public override string ResourceName => "Test";
	public override string ProcessorType => "Test";


}

