using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SorcererController : WandererController
{
    private Dictionary<string, float> cooldownTimers;
    private Dictionary<string, float> cooldownDurations;

    private bool isAbilityActive = false;

    [SerializeField] private GameObject fireballPrefab;  // Fireball prefab
    [SerializeField] private Transform fireballSpawnPoint; // Fireball spawn point




    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // Initialize cooldowns
        cooldownTimers = new Dictionary<string, float>
        {
            { "Fireball", 0f },
            { "Teleport", 0f },
            { "Clone", 0f },
            { "Inferno", 0f }
        };

        cooldownDurations = new Dictionary<string, float>
        {
            { "Fireball", 1f },
            { "Teleport", 10f },
            { "Clone", 10f },
            { "Inferno", 15f }
        };

    }

    // Update is called once per frame
    private void Update()
    {
        base.Update(); // Call the parent class's Update method to handle shared logic
        HandleCooldowns();
        HandleInputs();


    }

    private void HandleCooldowns()
    {
        var keys = new List<string>(cooldownTimers.Keys);

        foreach (var key in keys)
        {
            if (cooldownTimers[key] > 0)
            {
                cooldownTimers[key] -= Time.deltaTime;
            }
        }
    }

    protected override void HandleInputs()
    {
        if (Input.GetMouseButtonDown(1) && cooldownTimers["Fireball"] <= 0)
        {
            CastFireball();
        }
        if (Input.GetKeyDown(KeyCode.W) && cooldownTimers["Teleport"] <= 0)
        {

        }
        if (Input.GetKeyDown(KeyCode.Q) && cooldownTimers["Clone"] <= 0)
        {

        }
        if (Input.GetKeyDown(KeyCode.E) && cooldownTimers["Inferno"] <= 0)
        {

        }
    }

    private void CastFireball()
    {
        // Get the mouse position and spawn the Fireball
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (fireballPrefab != null && fireballSpawnPoint != null)
            {
                // Instantiate the Fireball
                GameObject fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, Quaternion.identity);

                // Initialize the Fireball with the target position
                Fireball fireballScript = fireball.GetComponent<Fireball>();
                if (fireballScript != null)
                {
                    fireballScript.Initialize(hit.point);
                }

                // Reset the Fireball cooldown
                cooldownTimers["Fireball"] = cooldownDurations["Fireball"];
            }
        }
    }

}
