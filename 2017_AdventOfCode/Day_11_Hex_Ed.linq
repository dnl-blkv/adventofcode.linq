<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string[] input = ParseInput(GetInput());
    Solve(input).Dump();
}

private static (long Result1, long Result2) Solve(string[] input)
{
    int n = 0;
    int nw = 0;
    int ne = 0;
    int lastDistance = 0;
    int maxDistance = 0;
    
    foreach (string direction in input)
    {
        switch (direction)
        {
            case "n":
                n++;
                break;
            
            case "s":
                n--;
                break;
                
            case "nw":
                nw++;
                break;
                
            case "se":
                nw--;
                break;
                
            case "ne":
                ne++;
                break;
                
            case "sw":
                ne--;
                break;
                
            default:
                throw new InvalidOperationException($"Unknown direction {direction}");
        }
        
        lastDistance = GetDistance(nw: nw, n: n, ne: ne);
        maxDistance = Math.Max(maxDistance, lastDistance);
    }
    
    return (Result1: lastDistance, Result2: maxDistance);
}

private static int GetDistance(int n, int nw, int ne)
{
    int aN = Math.Abs(n);
    int aNw = Math.Abs(nw);
    int aNe = Math.Abs(ne);
    int minWE = Math.Min(aNw, aNe);
    int maxWE = Math.Max(aNw, aNe);

    if (n >= 0 && nw >= 0 && ne >= 0 || n <= 0 && nw <= 0 && ne <= 0)
    {
        return aN + maxWE;
    }
    
    if (n <= 0 && nw >= 0 && ne >= 0 || n >= 0 && nw <= 0 && ne <= 0)
    {
        int nMinAdjusted = Math.Abs(aN - minWE);
        return nMinAdjusted + Math.Max(0, maxWE - minWE - nMinAdjusted);
    }

    int rV = nw + ne;
    
    return Math.Abs(n) + (rV * n >= 0 ? 1 : -1) * Math.Abs(rV) + minWE;
}

private static string[] ParseInput(IEnumerable<string> input) =>
    input.Single().Split(',').ToArray();

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
