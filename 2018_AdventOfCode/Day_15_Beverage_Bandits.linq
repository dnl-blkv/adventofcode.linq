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

private const int InitialHp = 200;
private const int DefaultAttack = 3;

private static readonly IReadOnlyList<Point> Directions =
[
    new Point(I:  0, J:  1),
    new Point(I:  1, J:  0),
    new Point(I:  0, J: -1),
    new Point(I: -1, J:  0)
];

private static long Solve1(string[] input) =>
    Solve(input, stopOnLostElf: false, elfAttack: DefaultAttack).Outcome;

private static long Solve2(string[] input)
{
    long result = -1;
    int left = DefaultAttack + 1;
    int right = InitialHp;
    
    while (left < right)
    {
        int current = (left + right) / 2;
        (long outcome, bool lostElves) = Solve(input, stopOnLostElf: false, elfAttack: (left + right) / 2);
        
        if (lostElves)
        {
            left = Math.Max(left + 1, current);
            continue;
        }
        
        right = Math.Min(right - 1, current);
        result = outcome;
    }
    
    return result;
}

private static (long Outcome, bool LostElves) Solve(string[] input, bool stopOnLostElf, int elfAttack)
{
    (Dictionary<Point, Unit> elves, Dictionary<Point, Unit> goblins) = FindUnits(input, elfAttack);
    int initialElfCount = elves.Count;
    IReadOnlyList<string> cleanMap = CleanMap(input);
    int roundsCompleted = 0;
    
    while (elves.Count * goblins.Count > 0)
    {
        bool gameOver = false;
    
        foreach (Unit unit in elves.Concat(goblins).OrderBy(kv => kv.Key).Select(kv => kv.Value).ToArray())
        {
            if (unit.Hp <= 0)
            {
                continue;
            }
        
            if (unit.TryPickNextStep(cleanMap, out Point nextStep))
            {
                unit.Position = nextStep;
            }
            
            if (unit.TryPickAttackTarget(cleanMap, out Point attackTarget))
            {
                unit.Enemies[attackTarget].Hp -= unit.Attack;
            }
            
            if (elves.Count * goblins.Count is 0 || stopOnLostElf && elves.Count < initialElfCount)
            {
                gameOver = true;
                break;
            }
        }
        
        if (gameOver)
        {
            break;
        }
        
        roundsCompleted++;
    }
    
    return (
        Outcome: roundsCompleted * elves.Concat(goblins).Sum(kv => kv.Value.Hp),
        LostElves: elves.Count != initialElfCount);
}

private static (Dictionary<Point, Unit> Elves, Dictionary<Point, Unit> Goblins) FindUnits(
    string[] input,
    int elfAttack)
{
    Dictionary<Point, Unit> elves = [];
    Dictionary<Point, Unit> goblins = [];

    for (int i = 0; i < input.Length; i++)
    {
        for (int j = 0; j < input[i].Length; j++)
        {
            char c = input[i][j];
            
            if (!char.IsLetter(c))
            {
                continue;
            }
            
            var position = new Point(I: i, J: j);
            _ = c is 'E'
                ? new Unit(allies: elves, enemies: goblins, position: position, attack: elfAttack)
                : new Unit(allies: goblins, enemies: elves, position: position, attack: DefaultAttack);
        }
    }
    
    return (elves, goblins);
}

private static IReadOnlyList<string> CleanMap(string[] map) =>
    map.Select(l => string.Join(string.Empty, l.Select(c => c is '#' ? c : '.'))).ToArray();

private static string[] ParseInput(IEnumerable<string> input) => input.ToArray();
    
private class Unit
{
    private int hp = InitialHp;
    private Point position = new Point(I: int.MinValue, J: int.MinValue);
    
    public Unit(
        Dictionary<Point, Unit> allies,
        IReadOnlyDictionary<Point, Unit> enemies,
        Point position,
        int attack)
    {
        this.Allies = allies;
        this.Enemies = enemies;
        this.Position = position;
        this.Attack = attack;
    }
    
    public IReadOnlyDictionary<Point, Unit> Enemies { get; }
    
    public Point Position
    {
        get => this.position;
        set
        {
            this.Allies.Remove(this.Position);
            this.position = value;
            this.Allies[this.Position] = this;
        }
    }

    public int Hp
    {
        get => this.hp;
        set
        {
            this.hp = value;
            
            if (this.hp > 0)
            {
                return;
            }
            
            this.Allies.Remove(this.Position);
        }
    }
    
    public int Attack { get; }
 
    private Dictionary<Point, Unit> Allies { get; }
    
    private IEnumerable<Point> Neighborhood => Directions.Select(d => this.Position + d);
    
    public bool TryPickNextStep(IReadOnlyList<string> map, out Point firstStep)
    {
        (IEnumerable<Point> closestEnemies, int distance) =
            FindClosestTargets(map, this.Position, this.Enemies.ContainsKey);
        IReadOnlyList<Point> targetCandidates = closestEnemies.ToArray();
        
        if (targetCandidates.Count == 0 || distance < 2)
        {
            firstStep = default;
            return false;
        }
        
        HashSet<Point> actorNeighborhood = this.Neighborhood.ToHashSet();
        Point targetEnemy = targetCandidates.Min();
        firstStep = FindClosestTargets(map, targetEnemy, actorNeighborhood.Contains).ClosestTargets.Min();
        return true;
    }
    
    public bool TryPickAttackTarget(IReadOnlyList<string> map, out Point attackTarget)
    {
        IReadOnlyList<Point> attackableEnemies = this.Neighborhood.Where(this.Enemies.ContainsKey).ToArray();
        
        if (attackableEnemies.Count == 0)
        {
            attackTarget = default;
            return false;
        }
    
        attackTarget = attackableEnemies.MinBy(p => (this.Enemies[p].Hp, p));
        return true;
    }
    
    private (IEnumerable<Point> ClosestTargets, int Distance) FindClosestTargets(
        IReadOnlyList<string> map,
        Point startPosition,
        Func<Point, bool> isTargetPosition)
    {
        HashSet<Point> currentStates = [startPosition];
        HashSet<Point> visited = [startPosition];
        int distance = 0;
        
        while (!currentStates.Any(isTargetPosition) && currentStates.Count > 0)
        {
            currentStates =
                currentStates
                    .SelectMany(s => Directions.Select(d => s + d))
                    .Where(p => map[p.I][p.J] is '.' && !this.Allies.ContainsKey(p) && visited.Add(p))
                    .ToHashSet();
            distance++;
        }
        
        return (ClosestTargets: currentStates.Where(isTargetPosition), Distance: distance);
    }
}
    
private record struct Point(int I, int J) : IComparable
{
    public static Point operator +(Point a, Point b) =>
        new Point(I: a.I + b.I, J: a.J + b.J);

    public int CompareTo(object? obj) =>
        (obj is not Point other || this.I > other.I || this.I == other.I && this.J > other.J)
            ? 1
            : this.I == other.I && this.J == other.J
                ? 0
                : -1;
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
