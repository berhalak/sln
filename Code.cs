using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace sln
{
	class Code
	{
		public static void Run(string projectPath)
		{
			var solutionPath = Path.Combine(Path.GetDirectoryName(projectPath), Path.GetFileNameWithoutExtension(projectPath) + ".sln");

			var sb = new StringBuilder();

			sb.AppendLine($"");
			sb.AppendLine($"Microsoft Visual Studio Solution File, Format Version 12.00");
			sb.AppendLine($"# Visual Studio 15");

			//Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "slntester", "slntester.csproj", "{4F4CD900-B834-435D-A2FD-DDC4FAF4C28F}"

			var pName = Path.GetFileNameWithoutExtension(projectPath);
			var pFileName = Path.GetFileName(projectPath);
			var pGuid = guid();

			var libTypeGuid = "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}";

			sb.AppendLine($"Project(\"{libTypeGuid}\") = \"{pName}\", \"{pFileName}\", \"{pGuid}\"");
			sb.AppendLine("EndProject");

			var dependency = @"Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""dependency"", ""dependency"", ""{7A6D7193-C1BD-4E91-919F-B06612235E00}""
EndProject";

			sb.AppendLine(dependency);

			var allProjects = BuildTree(projectPath).Distinct().ToList();

			var platformsGuids = new List<string>();

			platformsGuids.Add(pGuid);

			foreach (var p in allProjects)
			{
				var relativePath = GetRelativePath(Path.GetDirectoryName(solutionPath), p);
				var otherGuid = guid();
				var otherName = Path.GetFileNameWithoutExtension(p);
				sb.AppendLine($"Project(\"{libTypeGuid}\") = \"{otherName}\", \"{relativePath}\", \"{otherGuid}\"");
				sb.AppendLine("EndProject");

				platformsGuids.Add(otherGuid);
			}


			sb.AppendLine("Global");

			sb.AppendLine(@"	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection");

			// add connections

			sb.AppendLine("\tGlobalSection(NestedProjects) = preSolution");
			//{5A9D74C0-084A-47ED-875F-CD446985B459} = {7A6D7193-C1BD-4E91-919F-B06612235E00}

			foreach (var guid in platformsGuids.Skip(1))
			{
				sb.AppendLine($"\t\t{guid} = {{7A6D7193-C1BD-4E91-919F-B06612235E00}}");
			}

			sb.AppendLine("\tEndGlobalSection");

			sb.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");

			// add projects

			foreach (var platformGuid in platformsGuids)
			{
				sb.AppendLine($@"		{platformGuid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{platformGuid}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{platformGuid}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{platformGuid}.Release|Any CPU.Build.0 = Release|Any CPU");
			}
			sb.AppendLine("\tEndGlobalSection");


			sb.AppendLine(@"	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection");


			// add connections

			sb.AppendLine($@"	GlobalSection(ExtensibilityGlobals) = postSolution
		SolutionGuid = {guid()}
	EndGlobalSection");

			sb.AppendLine("EndGlobal");

			File.WriteAllText(solutionPath, sb.ToString());
		}

		private static string GetRelativePath(string workingDir, string otherPath)
		{
			var r = new Uri(workingDir + "\\").MakeRelativeUri(new Uri(otherPath)).ToString();
			r = r.Replace("/", "\\");
			return r;
		}

		private static IEnumerable<string> BuildTree(string projectPath)
		{
			var doc = new XmlDocument();
			using (var tr = new XmlTextReader(projectPath))
			{
				tr.Namespaces = false;
				doc.Load(tr);
			}
			var references = doc.SelectNodes("//ProjectReference");
			foreach (XmlNode n in references)
			{
				var relativePath = (n.Attributes["Include"] ?? n.Attributes["include"]).Value;
				var combined = Path.Combine(Path.GetDirectoryName(projectPath), relativePath);
				var fullpath = Path.GetFullPath(combined);
				yield return fullpath;

				foreach (var subProjects in BuildTree(fullpath))
				{
					yield return subProjects;
				}
			}
		}

		private static string guid_quata()
		{
			var pGuid = "\"" + guid() + "\"";
			return pGuid;
		}

		private static string guid()
		{
			var pGuid = "{" + Guid.NewGuid().ToString().ToUpper() + "}";
			return pGuid;
		}
	}
}
