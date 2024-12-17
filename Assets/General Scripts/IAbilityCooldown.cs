using System.Collections.Generic;

public interface IAbilityCooldown
{
    Dictionary<string, float> GetCooldownTimers();
    Dictionary<string, float> GetCooldownDurations();
}
