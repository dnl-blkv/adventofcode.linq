<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string[] input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

const string Password1 = "abcdefgh";
const string Password2 = "fbgdceah";

private static string Solve1(string[] input) =>
    Solve(input, Password1, Compile1);

private static string Solve2(string[] input) =>
    Solve(input, Password2, Compile2);

private static string Solve(
    string[] input,
    string password,
    Func<string[], IEnumerable<Action<char[]>>> compiler)
{
    char[] arr = password.ToCharArray();
    
    foreach (Action<char[]> transformFunc in compiler.Invoke(input))
    {
        transformFunc.Invoke(arr);
    }
    
    return new string(arr);
}
        
private static IEnumerable<Action<char[]>> Compile1(IEnumerable<string> input) =>
    input
        .Select<string, Action<char[]>>(
            line =>
            {
                string[] lineParts = line.Split(' ');
                string operation = $"{lineParts[0]} {lineParts[1]}";
                
                switch (operation)
                {
                    case "swap position":
                        return arr => SwapPos(arr, a: int.Parse(lineParts[2]), b: int.Parse(lineParts[5]));
                        
                    case "swap letter":
                        return arr => SwapLetter(arr, x: lineParts[2][0], y: lineParts[5][0]);
                
                    case "rotate right":
                        return arr => Rotate(arr, stepCount: int.Parse(lineParts[2]));
                        
                    case "rotate left":
                        return arr => Rotate(arr, stepCount: -int.Parse(lineParts[2]));
                        
                    case "rotate based":
                        return arr => RotateOn(arr, anchor: lineParts[6][0]);
                        
                    case "reverse positions":
                        return arr => Reverse(arr, from: int.Parse(lineParts[2]), to: int.Parse(lineParts[4]));
                        
                    case "move position":
                        return arr => Move(arr, from: int.Parse(lineParts[2]), to: int.Parse(lineParts[5]));
                
                    default:
                        throw new ArgumentException($"Unknown scrambling function: '{operation}'.");
                }
            })
        .ToArray();
        
private static IEnumerable<Action<char[]>> Compile2(IEnumerable<string> input) =>
    input
        .Reverse()
        .Select<string, Action<char[]>>(
            line =>
            {
                string[] lineParts = line.Split(' ');
                string operation = $"{lineParts[0]} {lineParts[1]}";
                
                switch (operation)
                {
                    case "swap position":
                        return arr => SwapPos(arr, a: int.Parse(lineParts[2]), b: int.Parse(lineParts[5]));
                        
                    case "swap letter":
                        return arr => SwapLetter(arr, x: lineParts[2][0], y: lineParts[5][0]);
                
                    case "rotate right":
                        return arr => Rotate(arr, stepCount: -int.Parse(lineParts[2]));
                        
                    case "rotate left":
                        return arr => Rotate(arr, stepCount: int.Parse(lineParts[2]));
                        
                    case "rotate based":
                        return arr => UnRotateOn(arr, anchor: lineParts[6][0]);
                        
                    case "reverse positions":
                        return arr => Reverse(arr, from: int.Parse(lineParts[2]), to: int.Parse(lineParts[4]));
                        
                    case "move position":
                        return arr => Move(arr, from: int.Parse(lineParts[5]), to: int.Parse(lineParts[2]));
                
                    default:
                        throw new ArgumentException($"Unknown scrambling function: '{operation}'.");
                }
            })
        .ToArray();

private static void SwapPos(char[] input, int a, int b) =>
    (input[a], input[b]) = (input[b], input[a]);

private static void SwapLetter(char[] input, char x, char y)
{
    for (int i = 0; i < input.Length; i++)
    {
        if (input[i] == x)
        {
            input[i] = y;
        }
        else if (input[i] == y)
        {
            input[i] = x;
        }
    }
}

private static void Rotate(char[] input, int stepCount) =>
    NRotate(size: input.Length, get: i => input[i], set: (i, v) => input[i] = v, n: stepCount);

private static void RotateOn(char[] input, char anchor)
{
    int anchorPos = Array.IndexOf(input, anchor);
    int stepCount = 1 + anchorPos + (anchorPos >= 4 ? 1 : 0);
    Rotate(input, stepCount);
}

private static void UnRotateOn(char[] input, char anchor)
{
    int anchorPos = Array.IndexOf(input, anchor);
    int stepCount = anchorPos switch // Possibly imporvable to a formula, but is it?
    {
        0 =>  7,
        1 => -1,
        2 =>  2,
        3 => -2,
        4 =>  1,
        5 => -3,
        6 =>  0,
        7 => -4,
        _ => throw new InvalidOperationException($"Unexpected anchor index: '{anchorPos}'.")
    };
    Rotate(input, stepCount);
}
    
private static void Reverse(char[] input, int from, int to)
{
    for (int i = 0; i < (to - from + 1) / 2; i++)
    {
        int curFrom = from + i;
        int curTo = to - i;
        (input[curFrom], input[curTo]) = (input[curTo], input[curFrom]);
    }
}

private static void Move(char[] input, int from, int to)
{
    char buff = input[from];
    int shiftIndex = (from < to ? 1 : -1);
    
    for (int i = from; i != to; i += shiftIndex)
    {
        input[i] = input[i + shiftIndex];
    }
    
    input[to] = buff;
}

private static void NRotate<T>(int size, Func<int, T> get, Action<int, T> set, int n)
{
    int nCorrected = (size + (n % size)) % size;

    for (int s = 0; s < GetGreatestCommonDivisor(size, nCorrected); s++)
    {
        int current = s;
    
        do
        {
            current = (current + nCorrected + size) % size;
            T sValue = get(s);
            set(s, get(current));
            set(current, sValue);
            
        } while (current != s);
    }
}

private static long GetGreatestCommonDivisor(params long[] vals)
{
    var curVals = new long[vals.Length];
    vals.CopyTo(curVals, 0);
    long result = 1;
    
    for (long div = 2; div <= vals.Min(); div++)
    {
        while (curVals.All(v => v % div == 0))
        {
            for (int i = 0; i < curVals.Length; i++)
            {
                curVals[i] /= div;
            }
            
            result *= div;
        }
    }
    
    return result;
}

private static string[] ParseInput(IEnumerable<string> input) => input.ToArray();

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
