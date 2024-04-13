using System.Reflection;

namespace Common.ExtensionMethods;

public static class AssemblyExtensions
{
	public static string GetProjectName()
		=> Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown Application";
}