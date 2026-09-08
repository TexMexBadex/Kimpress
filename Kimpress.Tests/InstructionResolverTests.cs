using Kimpress.Evaluation;
using Kimpress.Model;

namespace Kimpress.Tests;

/// <summary>
/// Tests for <see cref="InstructionResolver"/>, covering literal/RLE resolution,
/// simple and forward references, nested references, and error cases
/// (out-of-range references, circular references).
/// </summary>
public class InstructionResolverTests
{
  [Fact]
  public void Resolve_LiteralInstruction_ReturnsItsText()
  {
    // Arrange
    var instructions = new List<Instruction>
        {
            new Instruction(InstructionType.Literal, Text: "10 7 3 ")
        };
    var resolver = new InstructionResolver(instructions);

    // Act
    string result = resolver.Resolve(1);

    // Assert
    Assert.Equal("10 7 3 ", result);
  }

  [Fact]
  public void Resolve_RleInstruction_RepeatsTextTheGivenNumberOfTimes()
  {
    // Arrange
    var instructions = new List<Instruction>
        {
            new Instruction(InstructionType.RunLengthEncoded, Text: "21 ", RepeatCount: 3)
        };
    var resolver = new InstructionResolver(instructions);

    // Act
    string result = resolver.Resolve(1);

    // Assert
    Assert.Equal("21 21 21 ", result);
  }

  [Fact]
  public void Resolve_RleInstructionWithZeroCount_ReturnsEmptyString()
  {
    // Arrange: repeating zero times should produce an empty string,
    // not an error — worth locking down explicitly as its own case.
    var instructions = new List<Instruction>
        {
            new Instruction(InstructionType.RunLengthEncoded, Text: "21 ", RepeatCount: 0)
        };
    var resolver = new InstructionResolver(instructions);

    // Act
    string result = resolver.Resolve(1);

    // Assert
    Assert.Equal(string.Empty, result);
  }

  [Fact]
  public void Resolve_ReferenceToEarlierLines_ConcatenatesTheirValuesInOrder()
  {
    // Arrange: this mirrors Example #1 exactly.
    //   Line 1: LIT "10 7 3 "
    //   Line 2: RLE 3 "21 "
    //   Line 3: LIT "8 "
    //   Line 4: REF 2 3
    var instructions = new List<Instruction>
        {
            new Instruction(InstructionType.Literal, Text: "10 7 3 "),
            new Instruction(InstructionType.RunLengthEncoded, Text: "21 ", RepeatCount: 3),
            new Instruction(InstructionType.Literal, Text: "8 "),
            new Instruction(InstructionType.Reference, RefLines: new[] { 2, 3 })
        };
    var resolver = new InstructionResolver(instructions);

    // Act
    string result = resolver.Resolve(4);

    // Assert
    Assert.Equal("21 21 21 8 ", result);
  }

  [Fact]
  public void Resolve_ReferenceToLaterLine_ResolvesCorrectlyEvenThoughItAppearsFirst()
  {
    // Arrange: the "forward reference" case from Example #2 —
    // line 1 refers to line 3, which hasn't been "reached" yet in file order.
    //   Line 1: REF 3
    //   Line 2: RLE 2 "well hello\n"
    //   Line 3: REF 2
    var instructions = new List<Instruction>
        {
            new Instruction(InstructionType.Reference, RefLines: new[] { 3 }),
            new Instruction(InstructionType.RunLengthEncoded, Text: "well hello\n", RepeatCount: 2),
            new Instruction(InstructionType.Reference, RefLines: new[] { 2 })
        };
    var resolver = new InstructionResolver(instructions);

    // Act
    string result = resolver.Resolve(1);

    // Assert
    Assert.Equal("well hello\nwell hello\n", result);
  }

  [Fact]
  public void Resolve_SameLineReferencedTwiceInOneInstruction_AppearsTwiceInResult()
  {
    // Arrange: REF 1 1 should include line 1's value twice — this also
    // exercises the cache, since line 1 is resolved twice from one call.
    var instructions = new List<Instruction>
        {
            new Instruction(InstructionType.Literal, Text: "x"),
            new Instruction(InstructionType.Reference, RefLines: new[] { 1, 1 })
        };
    var resolver = new InstructionResolver(instructions);

    // Act
    string result = resolver.Resolve(2);

    // Assert
    Assert.Equal("xx", result);
  }

  [Fact]
  public void Resolve_ReferenceLineNumberBelowOne_ThrowsFormatException()
  {
    // Arrange
    var instructions = new List<Instruction>
        {
            new Instruction(InstructionType.Reference, RefLines: new[] { 0 })
        };
    var resolver = new InstructionResolver(instructions);

    // Act + Assert
    Assert.Throws<FormatException>(() => resolver.Resolve(1));
  }

  [Fact]
  public void Resolve_ReferenceLineNumberBeyondFileLength_ThrowsFormatException()
  {
    // Arrange: only 1 instruction exists, but it refers to line 99.
    var instructions = new List<Instruction>
        {
            new Instruction(InstructionType.Reference, RefLines: new[] { 99 })
        };
    var resolver = new InstructionResolver(instructions);

    // Act + Assert
    Assert.Throws<FormatException>(() => resolver.Resolve(1));
  }

  [Fact]
  public void Resolve_DirectSelfReference_ThrowsFormatException()
  {
    // Arrange: line 1 refers to itself — the simplest possible cycle.
    var instructions = new List<Instruction>
        {
            new Instruction(InstructionType.Reference, RefLines: new[] { 1 })
        };
    var resolver = new InstructionResolver(instructions);

    // Act + Assert
    Assert.Throws<FormatException>(() => resolver.Resolve(1));
  }

  [Fact]
  public void Resolve_IndirectCircularReference_ThrowsFormatException()
  {
    // Arrange: line 1 -> line 2 -> line 1, a two-step cycle.
    var instructions = new List<Instruction>
        {
            new Instruction(InstructionType.Reference, RefLines: new[] { 2 }),
            new Instruction(InstructionType.Reference, RefLines: new[] { 1 })
        };
    var resolver = new InstructionResolver(instructions);

    // Act + Assert
    Assert.Throws<FormatException>(() => resolver.Resolve(1));
  }


}