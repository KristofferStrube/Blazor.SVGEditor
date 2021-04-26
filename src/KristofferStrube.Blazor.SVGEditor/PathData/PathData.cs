using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KristofferStrube.Blazor.SVGEditor
{
    public static class PathData
    {
        public static List<IPathInstruction> Parse(string input)
        {
            var strippedInput = input.Replace(",", " ").Replace("-", " -");
            List<string> instructions = new() { "M", "m", "Z", "z", "L", "l", "H", "h", "V", "v", "C", "c", "S", "s", "Q", "q", "T", "t", "A", "a" };
            var standardizedInput = instructions.Aggregate(strippedInput, (accu, curr) => accu.Replace(curr, $",{curr} ")).TrimStart(' ');
            // This part looks for any number of spaces and replaces them with a single space.
            var removesDoubleSpaces = Regex.Replace(standardizedInput, @"\s+", " ");
            var splitInstructionSequences = removesDoubleSpaces.Split(",");
            var list = Enumerable.Range(1, splitInstructionSequences.Length - 1).Aggregate<int, List<IPathInstruction>>(
                new List<IPathInstruction>(),
                (list, curr) =>
                    {
                        var previous = curr == 1 ? null : list.Last();
                        var seq = splitInstructionSequences[curr].TrimEnd(' ');
                        var instruction = seq.Substring(0, 1);
                        if (curr == 1 && instruction is not ("M" or "m"))
                            throw new ArgumentException($"The first sequence is not a move (\"m\" or \"M\") in {strippedInput}");
                        var parameters = new List<double>();
                        if (seq != "Z" && seq != "z")
                        {
                            parameters = seq.Substring(2, seq.Length - 2).Split(" ").Select(p => double.Parse(p, CultureInfo.InvariantCulture)).ToList();
                        }
                        switch (instruction)
                        {
                            case "L" or "l":
                                if (parameters.Count % 2 != 0 && parameters.Count >= 2)
                                    throw new ArgumentException($"Wrong number of parameters for '{instruction}' at number {curr} sequence in {strippedInput}");
                                Enumerable.Range(0, parameters.Count / 2).ToList().ForEach(i =>
                                {
                                    list.Add(new LineInstruction(parameters[i * 2], parameters[i * 2 + 1], previous, instruction == "l") { ExplicitSymbol = i == 0 });
                                    previous = list.Last();
                                });
                                break;
                            case "M" or "m":
                                if (parameters.Count % 2 != 0 && parameters.Count >= 2)
                                    throw new ArgumentException($"Wrong number of parameters for '{instruction}' at number {curr} sequence in {strippedInput}");
                                Enumerable.Range(0, parameters.Count / 2).ToList().ForEach(i =>
                                {
                                    list.Add(new MoveInstruction(parameters[i * 2], parameters[i * 2 + 1], previous, instruction == "m") { ExplicitSymbol = i == 0 });
                                    previous = list.Last();
                                });
                                break;
                            case "H" or "h":
                                if (parameters.Count == 0)
                                    throw new ArgumentException($"Wrong number of parameters for '{instruction}' at number {curr} sequence in {strippedInput}");
                                Enumerable.Range(0, parameters.Count).ToList().ForEach(i =>
                                {
                                    list.Add(new HorizontalLineInstruction(parameters[i], previous, instruction == "h") { ExplicitSymbol = i == 0 });
                                    previous = list.Last();
                                });
                                break;
                            case "V" or "v":
                                if (parameters.Count == 0)
                                    throw new ArgumentException($"Wrong number of parameters for '{instruction}' at number {curr} sequence in {strippedInput}");
                                Enumerable.Range(0, parameters.Count).ToList().ForEach(i =>
                                {
                                    list.Add(new VerticalLineInstruction(parameters[i], previous, instruction == "v") { ExplicitSymbol = i == 0 });
                                    previous = list.Last();
                                });
                                break;
                            case "Z" or "z":
                                if (parameters.Count != 0)
                                    throw new ArgumentException($"Wrong number of parameters for '{instruction}' at number {curr} sequence in {strippedInput}");
                                list.Add(new ClosePathInstruction(previous, instruction == "z"));
                                previous = list.Last();
                                break;
                            case "C" or "c":
                                if (parameters.Count % 6 != 0 && parameters.Count >= 6)
                                    throw new ArgumentException($"Wrong number of parameters for '{instruction}' at number {curr} sequence in {strippedInput}");
                                Enumerable.Range(0, parameters.Count / 6).ToList().ForEach(i =>
                                {
                                    list.Add(new CubicBézierCurveInstruction(parameters[i * 2], parameters[i * 2 + 1], parameters[i * 2 + 2], parameters[i * 2 + 3], parameters[i * 2 + 4], parameters[i * 2 + 5], previous, instruction == "c") { ExplicitSymbol = i == 0 });
                                    previous = list.Last();
                                });
                                break;
                            default:
                                throw new ArgumentException($"Non supported sequence initializer: {instruction}");
                        }
                        return list;
                    });
            if (list.Count >= 2)
            {
                for (int i = 0; i < list.Count - 1; i++)
                {
                    list[i].NextInstruction = list[i + 1];
                }

            }
            return list;
        }

        public static string AsString(this List<IPathInstruction> pathData) => string.Join(" ", pathData.Select(p => p.ToString()));

        public static string AsString(this double d) => d.ToString(CultureInfo.InvariantCulture);
    }
}
