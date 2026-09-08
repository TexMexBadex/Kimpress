using Kimpress.Evaluation;
using Kimpress.Model;
using Kimpress.Parsing;

PrintBanner();

if (args.Length != 1)
{
  Console.Error.WriteLine("Usage: kimpress <Kimpress\\compressed.txt>");
  return 1;
}

string filePath = args[0];

if (!File.Exists(filePath))
{
  Console.Error.WriteLine($"Error: file not found: {filePath}");
  return 1;
}

try
{
  List<Instruction> instructions = ParseFile(filePath);
  WriteDecompressedOutput(instructions);
}
catch (FormatException ex)
{
  Console.Error.WriteLine($"Error: {ex.Message}");
  return 1;
}

return 0;


/// <summary>
/// Reads the compressed file line by line and parses each non-blank line
/// into a structured <see cref="Instruction"/>. Blank lines (e.g. a trailing
/// newline at the end of the file) are skipped.
/// </summary>
/// <param name="filePath">Path to the compressed file on disk.</param>
/// <returns>All parsed instructions, in the order they appear in the file.</returns>
/// <exception cref="FormatException">Thrown when a line cannot be parsed.</exception>
static List<Instruction> ParseFile(string filePath)
{
  var instructions = new List<Instruction>();
  int lineNumber = 0;

  foreach (string line in File.ReadLines(filePath))
  {
    lineNumber++;
    if (string.IsNullOrWhiteSpace(line))
      continue;

    instructions.Add(InstructionParser.Parse(line, lineNumber));
  }

  return instructions;
}

/// <summary>
/// Resolves each instruction to its final text value, in file order, and writes
/// each result directly to standard output as soon as it is available. Output is
/// streamed rather than accumulated into a single string, since the fully
/// decompressed content may be too large to hold in memory.
/// </summary>
/// <param name="instructions"> All parsed instructions, in file order. </param>
/// <exception cref="FormatException">
/// Thrown when a REF instruction points to a missing line, or a circular
/// reference between lines is detected.
/// </exception>
static void WriteDecompressedOutput(List<Instruction> instructions)
{
  var resolver = new InstructionResolver(instructions);

  for (int i = 1; i <= instructions.Count; i++)
  {
    Console.Write(resolver.Resolve(i));
  }
}

/// <summary>
/// Prints the Kimpress ASCII art banner to the console.
/// </summary>
static void PrintBanner()
{
  const string banner =
      "  _  ___                                   \r\n" +
      " | |/ (_)_ __ ___  _ __  _ __ ___  ___ ___ \r\n" +
      " | ' /| | '_ ` _ \\| '_ \\| '__/ _ \\/ __/ __|\r\n" +
      " | . \\| | | | | | | |_) | | |  __/\\__ \\__ \\\r\n" +
      " |_|\\_\\_|_| |_| |_| .__/|_|  \\___||___/___/\r\n" +
      "                  |_|                      ";
  Console.WriteLine(banner);
}