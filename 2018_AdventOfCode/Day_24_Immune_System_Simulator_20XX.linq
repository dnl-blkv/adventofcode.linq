<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    (IReadOnlyList<Group> immuneSystem, IReadOnlyList<Group> infection) = ParseInput(GetInput());
	Solve1(immuneSystem, infection).Dump();
	Solve2(immuneSystem, infection).Dump();
}

private static long Solve1(IReadOnlyList<Group> immuneSystem, IReadOnlyList<Group> infection) =>
    Solve(immuneSystem: immuneSystem, infection: infection, immuneSystemBoost: 0).UnitsRemaining;

private static long Solve2(IReadOnlyList<Group> immuneSystem, IReadOnlyList<Group> infection)
{
    int left = 0;
    int maxInfectionGroupTotalHp = infection.Max(g => g.TotalHp);
    Group weakestImmuneGroup = immuneSystem.MinBy(g => g.EffectivePower)!;
    int weakestImmuneGroupIdealDamage = (int)Math.Ceiling((decimal)maxInfectionGroupTotalHp / weakestImmuneGroup.Count);
    int right = weakestImmuneGroupIdealDamage - weakestImmuneGroup.Damage;
    
    while (true)
    {
        int center = (left + right) / 2;
        (long unitsRemaining, bool immunityWon) =
            Solve(immuneSystem: immuneSystem, infection: infection, immuneSystemBoost: center);
        
        if (left == right)
        {
            return unitsRemaining;
        }
        
        if (immunityWon)
        {
            right = center;
        }
        else
        {
            left = Math.Min(right, center + 1);
        }
    }
}

private static (long UnitsRemaining, bool ImmunityWon) Solve(
    IReadOnlyList<Group> immuneSystem,
    IReadOnlyList<Group> infection,
    int immuneSystemBoost)
{
    HashSet<Group> remainingImmuneSystem = [];
    remainingImmuneSystem.UnionWith(CreateRegisteredCopies(immuneSystem, onKill: remainingImmuneSystem.Remove));
    
    foreach (Group immunityGroup in remainingImmuneSystem)
    {
        immunityGroup.Boost(boostValue: immuneSystemBoost);
    }
        
    HashSet<Group> remainingInfection = [];
    remainingInfection.UnionWith(CreateRegisteredCopies(infection, onKill: remainingInfection.Remove));
    
    while (remainingImmuneSystem.Count * remainingInfection.Count > 0)
    {
        Dictionary<Group, Group> unorderedAttackMap =
            Enumerable.Empty<KeyValuePair<Group, Group>>()
                .Concat(SelectTargets(remainingImmuneSystem, remainingInfection))
                .Concat(SelectTargets(remainingInfection, remainingImmuneSystem))
                .ToDictionary();
                
        if (unorderedAttackMap.Count is 0)
        {
            break;
        }
                
        var attackMap =
            new SortedDictionary<Group, Group>(
                dictionary: unorderedAttackMap,
                comparer: Comparer<Group>.Create((g1, g2) => g2.Initiative.CompareTo(g1.Initiative)));
            
        foreach ((Group attacker, Group defender) in attackMap)
        {
            if (attacker.IsDead || defender.IsDead)
            {
                continue;
            }
            
            defender.TakeDamageFrom(attacker);
        }
    }
    
    return (
        UnitsRemaining: remainingImmuneSystem.Concat(remainingInfection).Sum(g => g.Count),
        ImmunityWon: remainingInfection.Count is 0);
    
    IEnumerable<Group> CreateRegisteredCopies(IEnumerable<Group> groups, Func<Group, bool> onKill) =>
        groups.Select(g =>
        {
            var newGroup = new Group(g);
            newGroup.OnKill = () => onKill(newGroup);
            
            return newGroup;
        });
    
    IEnumerable<KeyValuePair<Group, Group>> SelectTargets(HashSet<Group> attackers, HashSet<Group> defenders)
    {
        Dictionary<Group, Group> result = [];
        var defendersRemaining = new HashSet<Group>(defenders);
    
        foreach (Group attacker in attackers.OrderByDescending(g => (g.EffectivePower, g.Initiative)))
        {
            if (defendersRemaining.Count is 0)
            {
                break;
            }
        
            (Group defender, int damageEstimate) = defendersRemaining
                .Select(g => (Group: g, DamageEstimate: g.EstimateDamageFrom(attacker)))
                .MaxBy(t => (t.DamageEstimate, t.Group.EffectivePower, t.Group.Initiative));
            
            if (damageEstimate is 0)
            {
                continue;
            }
            
            yield return new KeyValuePair<Group, Group>(attacker, defender);
            defendersRemaining.Remove(defender);
        }
    }
}

private static (IReadOnlyList<Group> ImmuneSystem, IReadOnlyList<Group> Infection) ParseInput(
    IEnumerable<string> input)
{
    bool buildingImmuneSystem = true;
    List<Group> immuneSystem = [];
    List<Group> infection = [];
    
    foreach (string line in input)
    {
        if (line.Length is 0)
        {
            buildingImmuneSystem = false;
            continue;
        }
    
        if (!char.IsDigit(line[0]))
        {
            continue;
        }
        
        (buildingImmuneSystem ? immuneSystem : infection).Add(Group.Parse(line));
    }

    return (ImmuneSystem: immuneSystem, Infection: infection);
}

private class Group
{
    private static string[] SplitOn = ["hit points", "with an attack that does"];
    
    private readonly HashSet<string> immunities;
    private readonly HashSet<string> weaknesses;

    public Group(
        int count,
        int hp,
        int damage,
        string damageType,
        int initiative,
        HashSet<string> immunities,
        HashSet<string> weaknesses)
    {
        this.Count = count;
        this.Hp = hp;
        this.Damage = damage;
        this.DamageType = damageType;
        this.Initiative = initiative;
        this.immunities = immunities;
        this.weaknesses = weaknesses;
        this.OnKill = () => Expression.Empty();
    }
    
    public Group(Group group)
        : this(
            count: group.Count,
            hp: group.Hp,
            damage: group.Damage,
            damageType: group.DamageType,
            initiative: group.Initiative,
            immunities: new HashSet<string>(group.immunities),
            weaknesses: new HashSet<string>(group.weaknesses)) => Expression.Empty();
    
    public int Count { get; private set; }
    
    public int Hp { get; }
    
    public int Damage { get; private set; }
    
    public string DamageType { get; }
    
    public int Initiative { get; }
    
    public Action OnKill { get; set; }
    
    public int EffectivePower => this.Damage * this.Count;
    
    public int TotalHp => this.Hp * this.Count;
    
    public bool IsDead => this.Count <= 0;

    public static Group Parse(string line)
    {
        string[] lineParts = line.Split(SplitOn, StringSplitOptions.TrimEntries);
        string[] leftParts = lineParts[0].Split(' ');
        string[] rightParts = lineParts[2].Split(' ');
        
        Dictionary<string, HashSet<string>> allQualities =
            lineParts[1].Split("; ", StringSplitOptions.RemoveEmptyEntries)
                .Select(middlePart =>
                {
                    string[] qualities = middlePart.Split(' ').Select(p => p.Trim('(', ',', ')')).ToArray();
                    
                    return (qualities[0], qualities[2..].ToHashSet());
                })
                .ToDictionary();
        
        return new Group(
            count: int.Parse(leftParts[0]),
            hp: int.Parse(leftParts[^1]),
            damage: int.Parse(rightParts[0]),
            damageType: rightParts[1],
            initiative: int.Parse(rightParts[^1]),
            immunities: GetQualities("immune"),
            weaknesses: GetQualities("weak")
        );
        
        HashSet<string> GetQualities(string qualityType) =>
            allQualities.TryGetValue(qualityType, out HashSet<string>? immunities)
                ? immunities
                : new HashSet<string>();
    }
    
    public int EstimateDamageFrom(Group attacker)
    {
        string damageType = attacker.DamageType;
        int damageMultiplier = (this.IsImmuneTo(damageType) ? 0 : (this.IsWeakTo(damageType) ? 2 : 1));
        
        return damageMultiplier * attacker.Count * attacker.Damage;
    }
    
    public void TakeDamageFrom(Group attacker)
    {
        if ((this.Count -= this.EstimateDamageFrom(attacker) / this.Hp) > 0)
        {
            return;
        }
        
        this.OnKill.Invoke();
    }
    
    public void Boost(int boostValue) => this.Damage += boostValue;
    
    private bool IsImmuneTo(string damageType) => this.immunities.Contains(damageType);
    
    private bool IsWeakTo(string damageType) => this.weaknesses.Contains(damageType);
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}