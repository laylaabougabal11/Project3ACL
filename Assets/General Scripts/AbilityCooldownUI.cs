using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AbilityCooldownUI : MonoBehaviour
{
    public TMP_Text ability1;
    public TMP_Text ability2;
    public TMP_Text ability3;
    public TMP_Text ability4;

    private IAbilityCooldown abilityCooldownProvider;
    private Dictionary<string, float> cooldownTimers;
    private string[] abilityKeys = new string[4];

    void Start()
    {
        StartCoroutine(InitializeCooldownProvider());
    }

    private IEnumerator InitializeCooldownProvider()
    {
        // Wait a frame to ensure the correct Wanderer is active
        yield return new WaitForEndOfFrame();

        WandererController wanderer = FindObjectOfType<WandererController>();

        if (wanderer == null)
        {
            Debug.LogError("WandererController not found in the scene!");
            yield break;
        }

        abilityCooldownProvider = wanderer as IAbilityCooldown;

        if (abilityCooldownProvider == null)
        {
            Debug.LogError("The active Wanderer does not implement IAbilityCooldown!");
            yield break;
        }

        cooldownTimers = abilityCooldownProvider.GetCooldownTimers();
        SetAbilityKeys(wanderer);
    }

    private void Update()
    {
        if (cooldownTimers == null) return;

        UpdateAbilityText(ability1, cooldownTimers[abilityKeys[0]], abilityKeys[0]);
        UpdateAbilityText(ability2, cooldownTimers[abilityKeys[1]], abilityKeys[1]);
        UpdateAbilityText(ability3, cooldownTimers[abilityKeys[2]], abilityKeys[2]);
        UpdateAbilityText(ability4, cooldownTimers[abilityKeys[3]], abilityKeys[3]);
    }

    private void UpdateAbilityText(TMP_Text textField, float cooldown, string abilityName)
    {
        textField.text = cooldown > 0 ? cooldown.ToString("F1") : abilityName;
    }

    private void SetAbilityKeys(WandererController wanderer)
    {
        if (wanderer is Barbarian)
        {
            abilityKeys = new[] { "Bash", "Shield", "IronMaelstrom", "Charge" };
        }
        else if (wanderer is SorcererController)
        {
            abilityKeys = new[] { "Fireball", "Teleport", "Clone", "Inferno" };
        }
        else if (wanderer is RogueController)
        {
            abilityKeys = new[] { "Arrow", "SmokeBomb", "Dash", "ShowerOfArrows" };
        }
        else
        {
            Debug.LogError("Unknown Wanderer type!");
        }
    }
}
