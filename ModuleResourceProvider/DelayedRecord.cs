using CatConfig;

namespace ModuleResourceProvider;

public class DelayedRecord : IUnitRecord
{
	private readonly IUnitRecord record;
	private IUnitRecord import;

	public DelayedRecord(IUnitRecord record, IUnitRecord import)
	{
		this.record = record;
		this.import = import;
	}




	public Function this[IDelayedUnit field] => a => GetDelayedValue(field, a);
	public IUnit this[string fieldName] => GetUnitValue(fieldName);

	private IUnit GetUnitValue(string fieldName)
	{
		var val = record[fieldName];

		if (val is IDelayedUnit delayed)
		{
			delayed = delayed.Resolve(import);
			val = record[delayed]();
		}

		if (val is IUnitRecord rec)
			val = new DelayedRecord(rec, import);


		return val;
	}

	public string Name => record.Name;
	public string[] FieldNames => record.FieldNames;
	public int Id => record.Id;

	public IUnitRecord Transform(IUnitRecord import, bool @override = false)
	{
		this.import = this.import.Transform(import);
		return this;
	}
	private IUnit GetDelayedValue(IDelayedUnit field, object[] args)
	{
		field = field.Resolve(import);
		var val = record[field](args);
		if (val is IUnitRecord rec)
			val = new DelayedRecord(rec, import);
		return val;

	}
}