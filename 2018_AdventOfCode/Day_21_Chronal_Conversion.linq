<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string[] input = ParseInput(GetInput());
    // Uncomment the line below to see the input program:
    // input.Dump();
    Solve().Dump();
}

/***
Unoptimized program, translated to C# + pseudocode GOTO's:
int p0 = 0;
int p1 = 0;
int p2 = 0;
int p3 = 0;
int p5 = 0;

0:   p1 = 123;
1:   p1 &= 456;
2:   p1 = (p1 == 72 ? 1 : 0);
3:   GOTO 4 + p1;
4:   GOTO 1;
5:   p1 = 0;
6:   p2 = p1 | 65536
7:   p1 = 8725355
8:   p5 = p2 & 255;
9:   p1 += p5
10:  p1 &= 16777215
11:  p1 *= 65899
12:  p1 &= 16777215
13:  p5 = (256 > p2 ? 1 : 0)
14:  GOTO 15 + p5;
15:  GOTO 17;
16:  GOTO 28;
17:  p5 = 0;
18:  p3 = p5 + 1;
19:  p3 *= 256;
20:  p3 = (p3 > p2 ? 1 : 0);
21:  GOTO 22 + p3;
22:  GOTO 24;
23:  GOTO 26;
24:  p5++;
25:  GOTO 18;
26:  p2 = p5;
27:  GOTO 8;
28:  p5 = (p1 == p0 ? 1 : 0)
29:  GOTO 30 + p5;
30:  GOTO 6;
***/
private static (long Result1, long Result2) Solve()
{
    HashSet<int> haltedAt = [];
    int result1 = -1;
    int result2 = -1;
    int p1 = 0;
    int p2 = 0;

    do
    {
        p2 = p1 | 65536;
        p1 = 8725355;
        
        do
        {
            p1 = (((p1 + (p2 & 255)) & 16777215) * 65899) & 16777215;
        
            if (p2 < 256)
            {
                if (haltedAt.Add(p1))
                {
                    result1 = (result1 < 0 ? p1 : result1);
                    result2 = p1;
                    break;
                }
                
                return (Result1: result1, Result2: result2);
            }
            
            p2 /= 256;
        } while (true);
    } while (true);
}

private static string[] ParseInput(IEnumerable<string> input) => input.ToArray();

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
