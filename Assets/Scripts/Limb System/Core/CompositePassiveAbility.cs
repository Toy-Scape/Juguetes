using System.Collections.Generic;

public class CompositePassiveAbility : IPassiveAbility
{
    private readonly List<IPassiveAbility> abilities = new();

    public CompositePassiveAbility (params IPassiveAbility[] abilities)
    {
        this.abilities.AddRange(abilities);
    }

    public void Apply (LimbContext context)
    {
        foreach (var ability in abilities)
            ability.Apply(context);
    }

    public void Remove (LimbContext context)
    {
        foreach (var ability in abilities)
            ability.Remove(context);
    }
}
