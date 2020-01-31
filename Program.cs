using System;
using System.IO;
using System.Reflection;

namespace sln
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				var versionString = Assembly.GetEntryAssembly()
					.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
					.InformationalVersion
					.ToString();

				Console.WriteLine($"sln v{versionString}");
				Console.WriteLine("-------------");
				Console.WriteLine("\nUsage:");
				Console.WriteLine("  sln <file>");
				return;
			}

			var projectPath = Path.GetFullPath(args[0]);
			Code.Run(projectPath);
		}
	}
}