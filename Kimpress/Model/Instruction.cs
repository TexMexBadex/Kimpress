namespace Kimpress.Model;


/// <summary>
/// The three kinds of instructions that can appear in a Kimpress compressed file.
/// </summary>
public enum InstructionType
{
  /// <summary>Output the literal text as-is.</summary>
  Literal,

  /// <summary>Output the text repeated a given number of times.</summary>
  RunLengthEncoded,

  /// <summary>Output the concatenated values of one or more other lines.</summary>
  Reference
}

/// <summary>
/// Represents one parsed line from the compressed file.
/// Only the fields relevant to the given <see cref="Type"/> are populated:
/// Literal uses <see cref="Text"/>; RunLengthEncoded uses <see cref="Text"/> and
/// <see cref="RepeatCount"/>; Reference uses <see cref="RefLines"/>.
/// </summary>
/// <param name="Type">Which kind of instruction this is.</param>
/// <param name="Text">The literal or repeated text, if applicable.</param>
/// <param name="RepeatCount">How many times to repeat <see cref="Text"/>, if applicable.</param>
/// <param name="RefLines">The 1-based line numbers this instruction refers to, if applicable.</param>
public record Instruction(
    InstructionType Type,
    string? Text = null,
    int RepeatCount = 0,
    IReadOnlyList<int>? RefLines = null
);