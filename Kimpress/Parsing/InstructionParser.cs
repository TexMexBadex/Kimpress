using Kimpress.Model;
using System.Globalization;

namespace Kimpress.Parsing;

public static class InstructionParser
{
  /// <summary>
  /// Parses a single line of the compressed file into an <see cref="Instruction"/>.
  /// The opcode (first word) is matched case-insensitively.
  /// </summary>
  /// <param name="line">The raw line of text, as read from the file.</param>
  /// <param name="lineNumber">1-based line number, used for error messages.</param>
  /// <returns>A structured <see cref="Instruction"/> representing the line's meaning.</returns>
  /// <exception cref="FormatException">Thrown when the line is malformed or the opcode is unknown.</exception>
  public static Instruction Parse(string line, int lineNumber)
  {
    string trimmed = line.Trim();

    int firstSpace = trimmed.IndexOf(' ');
    if (firstSpace == -1)
      throw new FormatException($"Line {lineNumber}: missing arguments after opcode.");

    string opcode = trimmed[..firstSpace].ToUpperInvariant();
    string rest = trimmed[(firstSpace + 1)..].Trim();

    return opcode switch
    {
      "LIT" => ParseLit(rest, lineNumber),
      "RLE" => ParseRle(rest, lineNumber),
      "REF" => ParseRef(rest, lineNumber),
      _ => throw new FormatException($"Line {lineNumber}: unknown opcode '{opcode}'.")
    };
  }

  /// <summary>
  /// Parses the arguments of a LIT instruction: a single quoted string.
  /// Example input: "10 7 3 "
  /// </summary>
  /// <param name="rest">The part of the line after the "LIT " opcode.</param>
  /// <param name="lineNumber">1-based line number, used for error messages.</param>
  private static Instruction ParseLit(string rest, int lineNumber)
  {
    string text = ExtractQuotedString(rest, lineNumber);
    return new Instruction(InstructionType.Literal, Text: text);
  }

  /// <summary>
  /// Parses the arguments of an RLE instruction: a repeat count followed by a quoted string.
  /// Example input: 3 "21 "
  /// </summary>
  /// <param name="rest">The part of the line after the "RLE " opcode.</param>
  /// <param name="lineNumber">1-based line number, used for error messages.</param>
  private static Instruction ParseRle(string rest, int lineNumber)
  {
    int firstSpace = rest.IndexOf(' ');
    if (firstSpace == -1)
      throw new FormatException($"Line {lineNumber}: RLE requires a count and a quoted string.");

    string countPart = rest[..firstSpace];
    string quotedPart = rest[(firstSpace + 1)..].Trim();

    if (!int.TryParse(countPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out int count) || count < 0)
      throw new FormatException($"Line {lineNumber}: invalid RLE count '{countPart}'.");

    string text = ExtractQuotedString(quotedPart, lineNumber);
    return new Instruction(InstructionType.RunLengthEncoded, Text: text, RepeatCount: count);
  }

  /// <summary>
  /// Parses the arguments of a REF instruction: one or more space-separated 1-based line numbers.
  /// Example input: 2 3
  /// </summary>
  /// <param name="rest">The part of the line after the "REF " opcode.</param>
  /// <param name="lineNumber">1-based line number, used for error messages.</param>
  private static Instruction ParseRef(string rest, int lineNumber)
  {
    string[] parts = rest.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length == 0)
      throw new FormatException($"Line {lineNumber}: REF requires at least one line number.");

    var refs = new List<int>();
    foreach (string part in parts)
    {
      if (!int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out int refLine) || refLine < 1)
        throw new FormatException($"Line {lineNumber}: invalid REF line number '{part}'.");
      refs.Add(refLine);
    }

    return new Instruction(InstructionType.Reference, RefLines: refs);
  }

  /// <summary>
  /// Extracts and unescapes the content between a matching pair of double quotes.
  /// The input must start and end with '"'; anything else is treated as malformed.
  /// </summary>
  /// <param name="input">The candidate quoted string, e.g. "10 7 3 " (including the quotes).</param>
  /// <param name="lineNumber">1-based line number, used for error messages.</param>
  /// <returns>The unescaped text found between the quotes.</returns>
  private static string ExtractQuotedString(string input, int lineNumber)
  {
    if (input.Length < 2 || input[0] != '"' || input[^1] != '"')
      throw new FormatException($"Line {lineNumber}: expected a quoted string, got '{input}'.");

    string inner = input[1..^1]; //start from first character (after the '"') and take everything until last character (the last '"')
    return Unescape(inner, lineNumber);
  }

  /// <summary>
  /// Converts escape sequences within a quoted string into their real characters:
  /// \n becomes a newline, \\ becomes a literal backslash, \" becomes a literal quote.
  /// Any other backslash sequence is treated as an error.
  /// </summary>
  /// <param name="raw">The raw text between quotes, before unescaping.</param>
  /// <param name="lineNumber">1-based line number, used for error messages.</param>
  /// <returns>The text with all escape sequences resolved.</returns>
  private static string Unescape(string raw, int lineNumber)
  {
    var sb = new System.Text.StringBuilder(raw.Length);
    for (int i = 0; i < raw.Length; i++)
    {
      char c = raw[i];
      if (c == '\\' && i + 1 < raw.Length)
      {
        char next = raw[++i];
        sb.Append(next switch
        {
          'n' => '\n',
          '\\' => '\\',
          '"' => '"',
          _ => throw new FormatException($"Line {lineNumber}: unknown escape sequence '\\{next}'.")
        });
      }
      else
      {
        sb.Append(c);
      }
    }
    return sb.ToString();
  }
}
