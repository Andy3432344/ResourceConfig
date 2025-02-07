using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleResourceProvider;

public static class Environment
{
	public static void Start(IFileSystem fs)
	{
		_ = new ExternalResourceProcessor(fs);
		_ = new ModuleProcessor(fs);
	}
}
