using Kimpress.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kimpress.Evaluation;

public class InstructionResolver
{
  private readonly IReadOnlyList<Instruction> _instructions; //whole list of parsed instructions
  private readonly Dictionary<int, string> _cache = new(); //instead of calculating same line several times, save the value here and ask for value instead next time
  private readonly HashSet<int> _inProgress = new(); //collection without ducplicates to discover circular references

  public InstructionResolver(IReadOnlyList<Instruction> instructions)
  {
    _instructions = instructions;
  }

  /// <summary>
  /// Computes the final text value of the instruction at the given 1-based line number,
  /// recursively resolving any REF instructions it depends on.
  /// </summary>
  /// <param name="lineNumber"> 1-based line number to resolve. </param>
  /// <returns> The fully resolved text for that line. </returns>
  /// <exception cref="FormatException">
  /// Thrown when <paramref name="lineNumber"/> is out of range, or when a circular
  /// reference is detected (e.g. line 2 refers to line 4, which refers back to line 2).
  /// </exception>
  public string Resolve(int lineNumber)
  {
  //if calculated, return saved result
    if (_cache.TryGetValue(lineNumber, out string? cached))
      return cached; 

      //check if referenced line number actually exist, if not, throw exception
    if (lineNumber < 1 || lineNumber > _instructions.Count)
      throw new FormatException($"Reference to line {lineNumber}, which does not exist.");

      //check for circular references
    if (!_inProgress.Add(lineNumber))
      throw new FormatException($"Circular reference detected involving line {lineNumber}.");

    var result = ComputeValue(_instructions[lineNumber - 1]);

    _inProgress.Remove(lineNumber); //remove line from ongoing
    _cache[lineNumber] = result; //save result for future use
    return result; 
  }

  /// <summary>
  /// Computes the text value of a single instruction, assuming any REF targets
  /// it depends on can themselves be resolved via <see cref="Resolve"/>.
  /// </summary>
  /// <param name="instruction">The instruction to evaluate.</param>
  private string ComputeValue(Instruction instruction) =>
    instruction.Type switch
    {
      InstructionType.Literal => instruction.Text!,
      InstructionType.RunLengthEncoded => 
        string.Concat(Enumerable.Repeat(instruction.Text!, instruction.RepeatCount)),
      InstructionType.Reference => 
        string.Concat(instruction.RefLines!.Select(Resolve)),
      _ => throw new InvalidOperationException($"Unhandled instruction type: {instruction.Type}")
    };
}

