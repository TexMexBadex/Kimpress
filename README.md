# Kimpress Decompression Program

A .NET 10 decompression program that decodes files compressed with the Kimpress format.

## What This Is

This is a solution to the Kimpress challenge. It reads a compressed file, parses three instruction types (LIT, RLE, REF), resolves all references recursively, and streams the decompressed output to the console.

## Structure

```
Kimpress/
├── Parsing/
│   └── InstructionParser.cs       Parses lines into Instruction objects
├── Model/
│   └── Instruction.cs             Instruction record and InstructionType enum
├── Resolver/
│   └── InstructionResolver.cs     Resolves instructions with caching and circular reference detection
├── Program.cs                     Entry point
├── compressed_1.txt               Example 1
└── compressed_2.txt               Example 2
```

## Classes

- **InstructionParser**: Parses raw text into `Instruction` objects. Handles LIT (literal), RLE (repeat count + string), and REF (line numbers). Opcodes are case-insensitive.

- **InstructionResolver**: Resolves instructions by recursively following references. Caches results and detects circular references.

- **Instruction**: Record holding instruction type, text, repeat count, and reference lines.

## How to Run

### Compile
```powershell
dotnet build
```

### Run with Debug Profiles

Select a profile and press F5:
- **Kimpress** - runs compressed_1.txt
- **Example2** - runs compressed_2.txt

Or from terminal:
```powershell
dotnet run --project Kimpress Kimpres\compressed_1.txt
dotnet run --project Kimpress Kimpress\compressed_2.txt
```

## Assumptions

- Compressed files never exceed a few thousand lines
- Output can be very large (program streams output, not buffering everything in memory)
- Line numbers are 1-based
- Escape sequences (\n, \\, \") are the only valid escapes; unknown sequences throw an error
- Quoted strings must be properly closed with matching quotes
