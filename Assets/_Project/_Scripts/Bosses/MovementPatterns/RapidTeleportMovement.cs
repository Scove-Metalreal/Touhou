// FILE: _Project/_Scripts/Bosses/MovementPatterns/RapidTeleportMovement.cs

using System.Collections;
using UnityEngine;

namespace _Project._Scripts.Bosses.MovementPatterns
{
    /// <summary>
    /// A more aggressive teleportation pattern. The boss constantly and rapidly
    /// teleports to new random locations, creating a challenging and unpredictable fight.
    /// It includes a "telegraph" effect to briefly warn the player of the next location.
    /// </summary>
    public class RapidTeleportMovement : BossMovementPattern
    {
        [Header("Rapid Teleport Settings")]
        [Tooltip("The minimum time the boss waits before starting the next teleport (in seconds).")]
        [SerializeField] private float minTeleportInterval = 0.5f;
        
        [Tooltip("The maximum time the boss waits before starting the next teleport (in seconds).")]
        [SerializeField] private float maxTeleportInterval = 1.2f;
        
        [Tooltip("The duration of the 'telegraph' warning effect before the boss appears (in seconds).")]
        [SerializeField] private float telegraphDuration = 0.3f;

        [Header("Effects")]
        [Tooltip("The visual effect prefab to show where the boss is about to appear (e.g., a warning circle, particles).")]
        [SerializeField] private GameObject telegraphVFX_Prefab;

        [Header("Teleport Area")]
        [Tooltip("The bottom-left corner of the area where the boss can teleport.")]
        [SerializeField] private Vector2 teleportAreaMin = new Vector2(-7f, 1f);
        
        [Tooltip("The top-right corner of the area where the boss can teleport.")]
        [SerializeField] private Vector2 teleportAreaMax = new Vector2(7f, 4.5f);
        
        // --- Internal State ---
        private Coroutine teleportCoroutine;

        public override void StartMoving()
        {
            base.StartMoving();
            
            if (teleportCoroutine != null)
            {
                StopCoroutine(teleportCoroutine);
            }
            
            if (canMove)
            {
                teleportCoroutine = StartCoroutine(RapidTeleportRoutine());
            }
        }

        public override void StopMoving()
        {
            base.StopMoving();
            
            if (teleportCoroutine != null)
            {
                StopCoroutine(teleportCoroutine);
                teleportCoroutine = null;
            }
        }
        
        // Move() is left empty as all logic is handled in the coroutine.
        public override void Move() { }

        private IEnumerator RapidTeleportRoutine()
        {
            // Hide the boss initially to start the teleport cycle
            if (bossTransform.GetComponent<SpriteRenderer>() != null)
            {
                bossTransform.GetComponent<SpriteRenderer>().enabled = false;
            }

            while (canMove)
            {
                // 1. Wait for a random interval before the next teleport
                float waitTime = Random.Range(minTeleportInterval, maxTeleportInterval);
                yield return new WaitForSeconds(waitTime);

                if (!canMove || bossTransform == null) break; // Exit if stopped during wait

                // 2. Choose the next position
                float randomX = Random.Range(teleportAreaMin.x, teleportAreaMax.x);
                float randomY = Random.Range(teleportAreaMin.y, teleportAreaMax.y);
                Vector2 newPosition = new Vector2(randomX, randomY);

                // 3. Show a telegraph/warning effect at the new position
                GameObject telegraphInstance = null;
                if (telegraphVFX_Prefab != null)
                {
                    telegraphInstance = Instantiate(telegraphVFX_Prefab, newPosition, Quaternion.identity);
                }

                // Wait for the telegraph duration
                yield return new WaitForSeconds(telegraphDuration);

                // Clean up the telegraph effect
                if (telegraphInstance != null)
                {
                    Destroy(telegraphInstance);
                }

                if (!canMove) break; // Exit if stopped during telegraph

                // 4. Teleport the boss to the new position and make it visible
                bossTransform.position = newPosition;
                if (bossTransform.GetComponent<SpriteRenderer>() != null)
                {
                    bossTransform.GetComponent<SpriteRenderer>().enabled = true;
                }
            }
            
            // Ensure boss is visible when the pattern stops
            if (bossTransform != null && bossTransform.GetComponent<SpriteRenderer>() != null)
            {
                 bossTransform.GetComponent<SpriteRenderer>().enabled = true;
            }
        }
        
        // Gizmos help visualize the teleport area in the Scene view
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 0.5f, 0, 0.5f); // Orange, transparent
            Vector3 center = (teleportAreaMin + teleportAreaMax) / 2;
            Vector3 size = teleportAreaMax - teleportAreaMin;
            Gizmos.DrawCube(center, size);
        }
    }
}