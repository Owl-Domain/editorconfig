namespace Generator;

class Program
{
	#region Fields
	private static readonly string PartsDirectory = Path.GetFullPath("../../parts");
	private static readonly string BuildDirectory = Path.GetFullPath("../..");
	private static readonly string RootFilePath = Path.Combine(PartsDirectory, "root.editorconfig");
	private static readonly string CommonFilePath = Path.Combine(PartsDirectory, "common.editorconfig");
	#endregion

	#region Functions
	static void Main(string[] args)
	{
		if (Directory.Exists(BuildDirectory) is false)
			Directory.CreateDirectory(BuildDirectory);

		string path = Path.Combine(BuildDirectory, ".editorconfig");
		IEnumerable<string> partPaths = GetOrderedPartPaths();

		using StreamWriter writer = new(path, false);
		Merge(writer, partPaths);
	}
	#endregion

	#region Helpers
	private static void Merge(StreamWriter writer, IEnumerable<string> partPaths)
	{
		bool first = false;
		foreach (string path in partPaths)
		{
			if (first)
				writer.WriteLine();
			else
				first = true;

			string relative = Path.GetRelativePath(PartsDirectory, path);
			Console.WriteLine($"Appending: {relative}");

			using StreamReader reader = new(path);
			string? line = reader.ReadLine();

			if (line is not null)
			{
				if (line.StartsWith('#') is false)
					throw new InvalidOperationException($"The first line in the part file ({relative}) did not start with a comment.");

				writer.WriteLine($"{line} | source: {relative}");
				line = reader.ReadLine();
			}

			while (line is not null)
			{
				writer.WriteLine(line);
				line = reader.ReadLine();
			}
		}
	}
	private static IEnumerable<string> GetOrderedPartPaths()
	{
		IEnumerable<string> files = Directory.EnumerateFiles(PartsDirectory, "*.editorconfig", SearchOption.AllDirectories)
			.Except([RootFilePath, CommonFilePath])
			.Order(StringComparer.OrdinalIgnoreCase)
			.Prepend(CommonFilePath)
			.Prepend(RootFilePath);

		return files;
	}
	#endregion
}
