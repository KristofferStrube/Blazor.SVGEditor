using KristofferStrube.Blazor.SVGEditor.Extensions;
using System.Text.RegularExpressions;

namespace KristofferStrube.Blazor.SVGEditor.PathDataSequences;

public static class PathData
{
    public static List<IPathInstruction> Parse(string input)
    {
        string strippedInput = input.Replace(",", " ").Replace("-", " -");
        List<string> instructions = new() { "M", "m", "Z", "z", "L", "l", "H", "h", "V", "v", "C", "c", "S", "s", "Q", "q", "T", "t", "A", "a" };
        string standardizedInput = instructions.Aggregate(strippedInput, (accu, curr) => accu.Replace(curr, $",{curr} ")).TrimStart(' ');
        string removesDoubleSpaces = Regex.Replace(standardizedInput, @"\s+", " ");
        IEnumerable<string> splitInstructionSequences = removesDoubleSpaces.Split(",").Select(seq => NormalizeArgumentSequenceWithSpaceZeroDot(seq));
        List<IPathInstruction> list = Enumerable.Range(1, splitInstructionSequences.Count() - 1).Aggregate(
            new List<IPathInstruction>(),
            (list, curr) =>
                {
                    IPathInstruction previous = curr == 1 ? null : list.Last();
                    string seq = splitInstructionSequences.ElementAt(curr).TrimEnd(' ');
                    string instruction = seq[..1];
                    if (curr == 1 && instruction is not ("M" or "m"))
                    {
                        throw new ArgumentException($"The first sequence is not a move (\"m\" or \"M\") in {strippedInput}");
                    }

                    List<double> parameters = new();
                    if (seq != "Z" && seq != "z")
                    {
                        parameters = seq[2..].Split(" ").Select(p => p.ParseAsDouble()).ToList();
                    }
                    switch (instruction)
                    {
                        case "L" or "l":
                            if (parameters.Count % 2 != 0 && parameters.Count >= 2)
                            {
                                throw new ArgumentException($"Wrong number of parameters for '{instruction}' at number {curr} sequence in {strippedInput}");
                            }

                            Enumerable.Range(0, parameters.Count / 2).ToList().ForEach(i =>
                            {
                                list.Add(new LineInstruction(parameters[i * 2], parameters[i * 2 + 1], instruction == "l", previous) { ExplicitSymbol = i == 0 });
                                previous = list.Last();
                            });
                            break;
                        case "M" or "m":
                            if (parameters.Count % 2 != 0 && parameters.Count >= 2)
                            {
                                throw new ArgumentException($"Wrong number of parameters for '{instruction}' at number {curr} sequence in {strippedInput}");
                            }

                            Enumerable.Range(0, parameters.Count / 2).ToList().ForEach(i =>
                            {
                                list.Add(new MoveInstruction(parameters[i * 2], parameters[i * 2 + 1], instruction == "m", previous) { ExplicitSymbol = i == 0 });
                                previous = list.Last();
                            });
                            break;
                        case "H" or "h":
                            if (parameters.Count == 0)
                            {
                                throw new ArgumentException($"Wrong number of parameters for '{instruction}' at number {curr} sequence in {strippedInput}");
                            }

                            Enumerable.Range(0, parameters.Count).ToList().ForEach(i =>
                            {
                                list.Add(new HorizontalLineInstruction(parameters[i], instruction == "h", previous) { ExplicitSymbol = i == 0 });
                                previous = list.Last();
                            });
                            break;
                        case "V" or "v":
                            if (parameters.Count == 0)
                            {
                                throw new ArgumentException($"Wrong number of parameters for '{instruction}' at number {curr} sequence in {strippedInput}");
                            }

                            Enumerable.Range(0, parameters.Count).ToList().ForEach(i =>
                            {
                                list.Add(new VerticalLineInstruction(parameters[i], instruction == "v", previous) { ExplicitSymbol = i == 0 });
                                previous = list.Last();
                            });
                            break;
                        case "Z" or "z":
                            if (parameters.Count != 0)
                            {
                                throw new ArgumentException($"Wrong number of parameters for '{instruction}' at number {curr} sequence in {strippedInput}");
                            }

                            list.Add(new ClosePathInstruction(instruction == "z", previous));
                            previous = list.Last();
                            break;
                        case "C" or "c":
                            if (parameters.Count % 6 != 0 && parameters.Count >= 6)
                            {
                                throw new ArgumentException($"Wrong number of parameters for '{instruction}' at number {curr} sequence in {strippedInput}");
                            }

                            Enumerable.Range(0, parameters.Count / 6).ToList().ForEach(i =>
                            {
                                list.Add(new CubicBézierCurveInstruction(parameters[i * 6], parameters[i * 6 + 1], parameters[i * 6 + 2], parameters[i * 6 + 3], parameters[i * 6 + 4], parameters[i * 6 + 5], instruction == "c", previous) { ExplicitSymbol = i == 0 });
                                previous = list.Last();
                            });
                            break;
                        case "S" or "s":
                            if (parameters.Count % 4 != 0 && parameters.Count >= 4)
                            {
                                throw new ArgumentException($"Wrong number of parameters for '{instruction}' at number {curr} sequence in {strippedInput}");
                            }

                            Enumerable.Range(0, parameters.Count / 4).ToList().ForEach(i =>
                            {
                                list.Add(new ShorthandCubicBézierCurveInstruction(parameters[i * 4], parameters[i * 4 + 1], parameters[i * 4 + 2], parameters[i * 4 + 3], instruction == "s", previous) { ExplicitSymbol = i == 0 });
                                previous = list.Last();
                            });
                            break;
                        case "Q" or "q":
                            if (parameters.Count % 4 != 0 && parameters.Count >= 4)
                            {
                                throw new ArgumentException($"Wrong number of parameters for '{instruction}' at number {curr} sequence in {strippedInput}");
                            }

                            Enumerable.Range(0, parameters.Count / 4).ToList().ForEach(i =>
                            {
                                list.Add(new QuadraticBézierCurveInstruction(parameters[i * 4], parameters[i * 4 + 1], parameters[i * 4 + 2], parameters[i * 4 + 3], instruction == "q", previous) { ExplicitSymbol = i == 0 });
                                previous = list.Last();
                            });
                            break;
                        case "T" or "t":
                            if (parameters.Count % 2 != 0 && parameters.Count >= 2)
                            {
                                throw new ArgumentException($"Wrong number of parameters for '{instruction}' at number {curr} sequence in {strippedInput}");
                            }

                            Enumerable.Range(0, parameters.Count / 2).ToList().ForEach(i =>
                            {
                                list.Add(new ShorthandQuadraticBézierCurveInstruction(parameters[i * 2], parameters[i * 2 + 1], instruction == "t", previous) { ExplicitSymbol = i == 0 });
                                previous = list.Last();
                            });
                            break;
                        case "A" or "a":
                            if (parameters.Count % 7 != 0 && parameters.Count >= 7)
                            {
                                throw new ArgumentException($"Wrong number of parameters for '{instruction}' at number {curr} sequence in {strippedInput}");
                            }

                            Enumerable.Range(0, parameters.Count / 7).ToList().ForEach(i =>
                            {
                                list.Add(new EllipticalArcInstruction(parameters[i * 7], parameters[i * 7 + 1], parameters[i * 7 + 2], parameters[i * 7 + 3] == 1, parameters[i * 7 + 4] == 1, parameters[i * 7 + 5], parameters[i * 7 + 6], instruction == "a", previous) { ExplicitSymbol = i == 0 });
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

    public static string AsString(this List<IPathInstruction> pathData)
    {
        return string.Join(" ", pathData.Select(p => p.ToString()));
    }

    // Correct strings like "l-.004.007" and "l -0.004 0.007"
    // Supplied by https://github.com/ercgeek in comment https://github.com/KristofferStrube/Blazor.SVGEditor/issues/1#issuecomment-1006024496
    // Should try to refactor this in the future to use a more compact Regex or other performant method
    private static string NormalizeArgumentSequenceWithSpaceZeroDot(string seq)
    {
        string[] tokens = seq.Split(' ');
        for (int i = 0; i < tokens.Length; i++)
        {
            int numberOfPeriods = tokens[i].Count(f => f == '.');
            if (numberOfPeriods > 1)
            {
                int startIndex = tokens[i].IndexOf('.') + 1;
                for (int j = 1; j < numberOfPeriods; j++)
                {
                    int index = tokens[i].IndexOf('.', startIndex);
                    tokens[i] = tokens[i].Insert(index, " 0");
                    startIndex = index + 3;
                }
            }
        }
        return string.Join(" ", tokens);
    }
}
