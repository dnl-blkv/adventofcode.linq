<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    Action<BaseProgram>[] input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private static long Solve1(Action<BaseProgram>[] input)
{
    var p = new Program1();
    
    while (p.LastInstruction >= -1 && p.LastInstruction < input.Length - 1 && !p.IsStopped)
    {
        input[++p.LastInstruction](p);
    }
    
    return p.LastSound;
}

private static long Solve2(Action<BaseProgram>[] input)
{
    var p0 = new Program2(pVal: 0);
    var p1 = new Program2(pVal: 1);
    
    p0.SndCallback = p1.RcvQueue.Enqueue;
    p1.SndCallback = p0.RcvQueue.Enqueue;
    
    while (!(p0.IsWaiting && p1.IsWaiting) && !(p0.IsStopped && p1.IsStopped))
    {
        Execute(p0);
        Execute(p1);
    }
    
    return p1.SndCount;
    
    void Execute(Program2 p)
    {
        if (p.IsStopped)
        {
            return;
        }
    
        if (p.IsWaiting)
        {
            input[p.LastInstruction](p);
            return;
        }
        
        long currentInstruction = p.LastInstruction + 1;
        
        if (currentInstruction < 0 || currentInstruction >= input.Length)
        {
            p.IsStopped = true;
            return;
        }
        
        input[currentInstruction](p);
        p.LastInstruction++;
    }
}

private static Action<BaseProgram>[] ParseInput(IEnumerable<string> input) =>
    input
        .Select<string, Action<BaseProgram>>(line =>
        {
            string[] lineParts = line.Split(' ');
            string cmdType = lineParts[0];
            string arg1 = lineParts[1];
            string? arg2 = (lineParts.Length > 2 ? lineParts[2] : null);
            
            return cmdType switch
            {
                "snd" => p => p.Snd(arg1),
                "set" => p => p.Set(arg1, arg2!),
                "add" => p => p.Add(arg1, arg2!),
                "mul" => p => p.Mul(arg1, arg2!),
                "mod" => p => p.Mod(arg1, arg2!),
                "rcv" => p => p.Rcv(arg1),
                "jgz" => p => p.Jgz(arg1, arg2!),
                _ => throw new InvalidOperationException($"Unknown command type: {cmdType}")
            };
        })
        .ToArray();

private class Program1 : BaseProgram
{   
    public long LastSound { get; set; } = -1;
        
    public override void Rcv(string arg1)
    {
        if (this.GetValue(arg1) <= 0)
        {
            return;
        }
        
        this.IsStopped = true;
    }
    
    public override void Snd(string arg1) =>
        this.LastSound = this.GetValue(arg1);
}

private class Program2 : BaseProgram
{   
    public Program2(int pVal) => this.SetReg(name: "p", value: pVal);

    public Queue<long> RcvQueue { get; } = [];
    
    public Action<long> SndCallback { get; set; } = _ => Expression.Empty();
        
    public int SndCount { get; private set; } = 0;
    
    public bool IsWaiting { get; set; } = false;
        
    public override void Rcv(string arg1)
    {
        if (this.RcvQueue.Count == 0)
        {
            this.IsWaiting = true;
            return;
        }
        
        this.SetReg(name: arg1, this.RcvQueue.Dequeue());
        this.IsWaiting = false;
    }
    
    public override void Snd(string arg1)
    {
        this.SndCallback.Invoke(this.GetValue(arg1));
        this.SndCount++;
    }
}

private abstract class BaseProgram
{
    private readonly Dictionary<string, long> registers = [];
    
    public long LastInstruction { get; set; } = -1;
    
    public bool IsStopped { get; set; } = false;
    
    public abstract void Rcv(string arg1);
    
    public abstract void Snd(string arg1);
    
    public void Set(string arg1, string arg2) =>
        this.SetReg(name: arg1, value: this.GetValue(arg2));
    
    public void Add(string arg1, string arg2) =>
        this.SetReg(name: arg1, value: this.GetReg(arg1) + this.GetValue(arg2));
    
    public void Mul(string arg1, string arg2) =>
        this.SetReg(name: arg1, value: this.GetReg(arg1) * this.GetValue(arg2));
    
    public void Mod(string arg1, string arg2) =>
        this.SetReg(name: arg1, value: this.GetReg(arg1) % this.GetValue(arg2));
    
    public void Jgz(string arg1, string arg2)
    {
        if (this.GetValue(arg1) <= 0)
        {
            return;
        }
    
        this.LastInstruction += this.GetValue(arg2!) - 1;
    }
    
    protected void SetReg(string name, long value) =>
        registers[name] = value;
    
    protected long GetReg(string name) =>
        (registers.TryGetValue(name, out long value) ? value : 0);
        
    protected long GetValue(string arg) =>
        (long.TryParse(arg, out long value) ? value : this.GetReg(arg));
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}