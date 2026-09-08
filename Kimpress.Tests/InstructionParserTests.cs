using Kimpress.Model;
using Kimpress.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kimpress.Tests;

/// <summary>
/// Tests for <see cref="InstructionParser"/>, covering successful parsing
/// of all three opcodes, case-insensitivity, escape sequences
/// </summary>
public class InstructionParserTests
{
  [Fact]
  public void Parse_LiteralInstruction_ReturnsTextVerbatim()
  {
    //Arrange
    string line = "LIT \"10 7 3\"";

    //Act
    Instruction result = InstructionParser.Parse(line, lineNumber: 1);

    //Assert
    Assert.Equal(InstructionType.Literal, result.Type);
  }

  [Fact]
  public void Parse_LiteralInstruction_ExtractsTextBetweenQuotes()
  {
    // Arrange
    string line = "LIT \"10 7 3 \"";

    // Act
    Instruction result = InstructionParser.Parse(line, lineNumber: 1);

    // Assert
    Assert.Equal("10 7 3 ", result.Text);
  }

  [Fact]
  public void Parse_RleInstruction_HasRunLengthEncodedType()
  {
    // Arrange
    string line = "RLE 3 \"21 \"";

    // Act
    Instruction result = InstructionParser.Parse(line, lineNumber: 1);

    // Assert
    Assert.Equal(InstructionType.RunLengthEncoded, result.Type);
  }

  [Fact]
  public void Parse_RleInstruction_ExtractsText()
  {
    // Arrange
    string line = "RLE 3 \"21 \"";

    // Act
    Instruction result = InstructionParser.Parse(line, lineNumber: 1);

    // Assert
    Assert.Equal("21 ", result.Text);
  }

  [Fact]
  public void Parse_RleInstruction_ExtractsRepeatCount()
  {
    // Arrange
    string line = "RLE 3 \"21 \"";

    // Act
    Instruction result = InstructionParser.Parse(line, lineNumber: 1);

    // Assert
    Assert.Equal(3, result.RepeatCount);
  }

  [Fact]
  public void Parse_ReferenceInstruction_HasReferenceType()
  {
    // Arrange
    string line = "REF 4 1 4";

    // Act
    Instruction result = InstructionParser.Parse(line, lineNumber: 5);

    // Assert
    Assert.Equal(InstructionType.Reference, result.Type);
  }

  [Fact]
  public void Parse_ReferenceInstruction_ExtractsAllLineNumbersInOrder()
  {
    // Arrange
    string line = "REF 4 1 4";

    // Act
    Instruction result = InstructionParser.Parse(line, lineNumber: 5);

    // Assert
    Assert.Equal(new[] { 4, 1, 4 }, result.RefLines);
  }

  [Theory]
  [InlineData("lit \"test\"")]
  [InlineData("Lit \"test\"")]
  [InlineData("LIT \"test\"")]
  public void Parse_OpcodeCasing_IsCaseInsensitive(string line)
  {
    // Act
    Instruction result = InstructionParser.Parse(line, lineNumber: 1);

    // Assert
    Assert.Equal(InstructionType.Literal, result.Type);
  }

  [Fact]
  public void Parse_EscapedNewline_BecomesRealNewlineCharacter()
  {
    // Arrange
    string line = "RLE 2 \"well hello\\n\"";

    // Act
    Instruction result = InstructionParser.Parse(line, lineNumber: 1);

    // Assert
    Assert.Equal("well hello\n", result.Text);
  }

  [Fact]
  public void Parse_EscapedBackslash_StaysAsLiteralBackslash()
  {
    // Arrange
    string line = "LIT \"\\\\n should result in a new line \"";

    // Act
    Instruction result = InstructionParser.Parse(line, lineNumber: 1);

    // Assert
    Assert.Equal("\\n should result in a new line ", result.Text);
  }

  [Fact]
  public void Parse_UnknownOpcode_ThrowsFormatException()
  {
    // Arrange
    string line = "FOO \"bar\"";

    // Act + Assert
    Assert.Throws<FormatException>(() => InstructionParser.Parse(line, lineNumber: 7));
  }

  [Fact]
  public void Parse_MissingClosingQuote_ThrowsFormatException()
  {
    // Arrange
    string line = "LIT \"unterminated";

    // Act + Assert
    Assert.Throws<FormatException>(() => InstructionParser.Parse(line, lineNumber: 1));
  }

  [Fact]
  public void Parse_NonNumericRleCount_ThrowsFormatException()
  {
    // Arrange
    string line = "RLE abc \"text\"";

    // Act + Assert
    Assert.Throws<FormatException>(() => InstructionParser.Parse(line, lineNumber: 1));
  }

  [Fact]
  public void Parse_NonNumericRefLineNumber_ThrowsFormatException()
  {
    // Arrange
    string line = "REF abc";

    // Act + Assert
    Assert.Throws<FormatException>(() => InstructionParser.Parse(line, lineNumber: 1));
  }

  [Fact]
  public void Parse_UnknownEscapeSequence_ThrowsFormatException()
  {
    // Arrange: \q is not a recognized escape sequence.
    string line = "LIT \"bad\\qescape\"";

    // Act + Assert
    Assert.Throws<FormatException>(() => InstructionParser.Parse(line, lineNumber: 1));
  }

}
