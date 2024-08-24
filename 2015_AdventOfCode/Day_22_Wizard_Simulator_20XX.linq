<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    Wizard boss = ParseInput(GetInput());
	Solve1(boss).Dump();
	Solve2(boss).Dump();
}

private const string PhysicalDamageEffectName = "PhysicalDamage";
private const string HardModeEffectName = "HardMode";
private const int InitialMana = 500;
private const int InitialHitPoints = 50;

private static readonly Wizard Player = new Wizard(hitPoints: InitialHitPoints);

private static IReadOnlyList<ISpell> AllSpells =
    new List<ISpell>
    {
        new MagicMissile(),
        new Drain(),
        new Shield(),
        new Poison(),
        new Recharge()
    };

private static int Solve1(Wizard boss) => Solve(Player, boss);

private static int Solve2(Wizard boss)
{
    Player.TryChange(
        manaChange: 0,
        result: out Wizard? newPlayer,
        newEffect: new HardModeEffect());
    
    return Solve(newPlayer!, boss);
}

private static int Solve(Wizard player, Wizard boss)
{
    (Wizard Player, Wizard Boss, int ManaSpent) initialState = (player, boss, ManaSpent: 0);
    var currentLevel = new List<(Wizard Player, Wizard Boss, int ManaSpent)>
    {
        initialState
    };
    int leastManaSpent = int.MaxValue;
    int i = 0;
    
    while (currentLevel.Count > 0)
    {
        var nextLevel = new List<(Wizard Player, Wizard Boss, int ManaSpent)>();
        
        foreach ((Wizard CurrentPlayer, Wizard CurrentBoss, int ManaSpent) p in currentLevel)
        {
            (Wizard? currentPlayer, Wizard? currentBoss, int manaSpent) = p;
            currentPlayer = currentPlayer.Tick();
            currentBoss = currentBoss.Tick();
            
            if (currentBoss.HitPoints <= 0)
            {
                leastManaSpent = Math.Min(leastManaSpent, manaSpent);
                continue;
            }
        
            if (i % 2 == 0)
            {
                foreach (ISpell spell in AllSpells)
                {
                    if (!spell.TryApply(currentPlayer, currentBoss, out Wizard? nextPlayer, out Wizard? nextBoss))
                    {
                        continue;
                    }
                    
                    int nextManaSpent = manaSpent + spell.ManaCost;
                    
                    if (nextManaSpent >= leastManaSpent)
                    {
                        continue;
                    }
                    
                    nextLevel.Add((nextPlayer, nextBoss, nextManaSpent));
                }
                
                continue;
            }
            
            if (currentPlayer.TryChange(
                manaChange: 0,
                result: out currentPlayer,
                hitPointsChange: -currentBoss.PhysicalDamage,
                physicalDamage: true)
                && currentPlayer.HitPoints > 0)
            {
                nextLevel.Add((currentPlayer, currentBoss, manaSpent));
            }
        }

        currentLevel = nextLevel;
        i++;
    }
    
    return leastManaSpent;
}

private static Wizard ParseInput(IEnumerable<string> input)
{
    List<string>? nextList = new List<string>();
    
    foreach (string line in input)
    {
        if (line is { Length: 0 })
        {
            nextList = new List<string>();
            continue;
        }
        
        nextList.Add(line);
    }
    
    return ParseBoss(nextList);
}

private static Wizard ParseBoss(IReadOnlyList<string> lines)
{
    int[] values = lines.Select(l => int.Parse(l.Split(": ")[1])).ToArray();
    var effects = new Dictionary<string, Effect>
    {
        [PhysicalDamageEffectName] =
            new Effect(PhysicalDamageEffectName, ticksLeft: int.MaxValue, damageChange: values[1])
    };
    
    return new Wizard(hitPoints: values[0], effects: effects);
}
    
private class Wizard
{
    private readonly Dictionary<string, Effect> effects;
    
    public Wizard(int hitPoints, int? mana = null, Dictionary<string, Effect>? effects = null)
    {
        this.HitPoints = hitPoints;
        this.Mana = mana ?? InitialMana;
        this.effects = effects ?? new Dictionary<string, Effect>();
    }
    
    public int HitPoints { get; }
    
    public int Mana { get; }
    
    public int Armor =>
        this.effects.TryGetValue(nameof(Shield), out Effect? shieldEffect)
            ? shieldEffect.ArmorChange
            : 0;
            
    public int PhysicalDamage =>
        this.effects.TryGetValue(nameof(PhysicalDamage), out Effect? physicalDamageEffect)
            ? physicalDamageEffect.DamageChange
            : 0;
            
    public bool HasEffect(string name) =>
        this.effects.ContainsKey(name);
        
    public bool TryChange(
        int manaChange,
        [MaybeNullWhen(returnValue: false)]out Wizard result,
        int hitPointsChange = 0,
        Effect? newEffect = null,
        bool physicalDamage = false)
    {
        if (this.Mana + manaChange < 0
            || newEffect is not null && this.effects.ContainsKey(newEffect.Name))
        {
            result = new Wizard(this.HitPoints, this.Mana, this.effects);
            return false;
        }
            
        var newEffects = new Dictionary<string, Effect>(this.effects);
        
        if (newEffect is not null)
        {
            newEffects.Add(newEffect.Name, newEffect);
        }
        
        int adjustedHitPointsChange =
            hitPointsChange < 0 && physicalDamage
                ? hitPointsChange + Math.Min(this.Armor, -hitPointsChange - 1)
                : hitPointsChange;
        
        result = new Wizard(this.HitPoints + adjustedHitPointsChange, this.Mana + manaChange, newEffects);
        return true;
    }
    
    public Wizard Tick()
    {
        int hitPointsAfterTick =    
            this.HitPoints + this.effects.Values.Sum(e => e.HitPointsTick);
        int manaAfterTick =
            this.Mana + effects.Values.Sum(e => e.ManaTick);
        Dictionary<string, Effect> effectsAfterTick =
            this.effects.Values
                .Select(e => e.Tick())
                .Where(e => e.TicksLeft > 0)
                .ToDictionary(e => e.Name, e => e);
        
        return new Wizard(hitPointsAfterTick, manaAfterTick, effectsAfterTick);
    }
}

private class HardModeEffect : Effect
{
    public HardModeEffect()
        : base(name: nameof(HardModeEffect), ticksLeft: int.MaxValue, hitPointsTick: -1) =>
        Expression.Empty();
            
    public override int HitPointsTick =>
        this.TicksLeft % 2 == 1
            ? base.HitPointsTick
            : 0;
}

private class Effect
{
    public Effect(
        string name,
        int ticksLeft,
        int armorChange = 0,
        int damageChange = 0,
        int manaTick = 0,
        int hitPointsTick = 0)
    {
        this.Name = name;
        this.TicksLeft = ticksLeft;
        this.ArmorChange = armorChange;
        this.DamageChange = damageChange;
        this.ManaTick = manaTick;
        this.HitPointsTick = hitPointsTick;
    }
    
    public string Name { get; }
    
    public int TicksLeft { get; }
    
    public int ArmorChange { get; }
    
    public int DamageChange { get; }
    
    public int ManaTick { get; }
    
    public virtual int HitPointsTick { get; }
    
    public Effect Tick() =>
        new Effect(
            name: this.Name,
            armorChange: this.ArmorChange,
            damageChange: this.DamageChange,
            manaTick: this.ManaTick,
            hitPointsTick: this.HitPointsTick,
            ticksLeft: this.TicksLeft - 1);
}

private interface ISpell
{
    int ManaCost { get; }

    bool TryApply(
        Wizard caster,
        Wizard target,
        [MaybeNullWhen(returnValue: false)]out Wizard newCaster,
        [MaybeNullWhen(returnValue: false)]out Wizard newTarget);
}

private class MagicMissile : ISpell
{
    private const int Damage = 4;

    public int ManaCost => 53;

    public bool TryApply(
        Wizard caster,
        Wizard target,
        [MaybeNullWhen(returnValue: false)]out Wizard newCaster,
        [MaybeNullWhen(returnValue: false)]out Wizard newTarget)
    {
        newTarget = null;
        return caster.TryChange(manaChange: -this.ManaCost, result: out newCaster)
            && target.TryChange(manaChange: 0, result: out newTarget, hitPointsChange: -Damage);
    }
}

private class Drain : ISpell
{
    private const int DrainValue = 2;

    public int ManaCost => 73;

    public bool TryApply(
        Wizard caster,
        Wizard target,
        [MaybeNullWhen(returnValue: false)]out Wizard newCaster,
        [MaybeNullWhen(returnValue: false)]out Wizard newTarget)
    {
        newTarget = null;
        return caster.TryChange(manaChange: -this.ManaCost, result: out newCaster, hitPointsChange: DrainValue)
            && target.TryChange(manaChange: 0, result: out newTarget, hitPointsChange: -DrainValue);
    }
}

private class Shield : ISpell
{
    private const int EffectTicks = 6;
    private const int EffectCasterArmorChange = 7;

    public int ManaCost => 113;

    public bool TryApply(
        Wizard caster,
        Wizard target,
        [MaybeNullWhen(returnValue: false)]out Wizard newCaster,
        [MaybeNullWhen(returnValue: false)]out Wizard newTarget)
    {
        newTarget = null;
        return caster.TryChange(
                manaChange: -this.ManaCost,
                result: out newCaster,
                newEffect: new Effect(
                    name: nameof(Shield),
                    ticksLeft: EffectTicks,
                    armorChange: EffectCasterArmorChange))
            && target.TryChange(manaChange: 0, result: out newTarget);
    }
}

private class Poison : ISpell
{
    private const int EffectTicks = 6;
    private const int EffectTargetHitPointsTick = -3;

    public int ManaCost => 173;

    public bool TryApply(
        Wizard caster,
        Wizard target,
        [MaybeNullWhen(returnValue: false)]out Wizard newCaster,
        [MaybeNullWhen(returnValue: false)]out Wizard newTarget)
    {
        newTarget = null;
        return caster.TryChange(manaChange: -this.ManaCost, result: out newCaster)
            && target.TryChange(
                manaChange: 0,
                result: out newTarget,
                newEffect: new Effect(
                    name: nameof(Poison),
                    ticksLeft: EffectTicks,
                    hitPointsTick: EffectTargetHitPointsTick));
    }
}

private class Recharge : ISpell
{
    private const int EffectTicks = 5;
    private const int EffectCasterManaTick = 101;

    public int ManaCost => 229;

    public bool TryApply(
        Wizard caster,
        Wizard target,
        [MaybeNullWhen(returnValue: false)]out Wizard newCaster,
        [MaybeNullWhen(returnValue: false)]out Wizard newTarget)
    {
        newTarget = null;
        return caster.TryChange(
                manaChange: -this.ManaCost,
                result: out newCaster,
                newEffect: new Effect(
                    name: nameof(Recharge),
                    ticksLeft: EffectTicks,
                    manaTick: EffectCasterManaTick))
            && target.TryChange(manaChange: 0, result: out newTarget);
    }
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}