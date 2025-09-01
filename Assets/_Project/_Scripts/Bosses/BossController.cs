// FILE: _Project/Scripts/Bosses/BossController.cs (VERSION 3.0 - FINAL)

using UnityEngine;
using System.Collections;
using _Project._Scripts.Bosses.AttackPatterns;
using _Project._Scripts.Bosses.MovementPatterns;
using _Project._Scripts.Core;
using _Project._Scripts.UI;

namespace _Project._Scripts.Bosses
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BossHealth))]
    public class BossController : MonoBehaviour
    {
        [Header("Data & Core Components")]
        [Tooltip("Kéo file ScriptableObject chứa dữ liệu của boss vào đây.")]
        public BossData bossData;

        [Header("Tham chiếu Nội bộ")] 
        [Tooltip("Kéo GameObject con '_AttackPatternHolder' vào đây.")] 
        [SerializeField] private Transform attackPatternHolder;
        [Tooltip("Kéo GameObject con '_MovementPatternHolder' vào đây.")] 
        [SerializeField] private Transform movementPatternHolder;

        // --- Biến nội bộ ---
        private Rigidbody2D rb;
        private BossHealth bossHealth;
        private UIManager uiManager;
        
        private int currentStageIndex = -1;
        private BossStage currentStage;
        
        private Coroutine currentAttackCoroutine;
        private Coroutine spellCardTimerCoroutine;
        
        private BossMovementPattern currentMovementPattern;
        private GameObject currentMovementInstance;

        private bool isDying = false;

        #region Lifecycle & Initialization
        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            bossHealth = GetComponent<BossHealth>();
        }
        
        void FixedUpdate()
        {
            if (currentMovementPattern != null && !isDying)
            {
                currentMovementPattern.Move();
            }
        }

        public void Initialize()
        {
            uiManager = UIManager.Instance;
            bossHealth.OnStageHealthDepleted += GoToNextStage;
            GoToNextStage();
        }
        #endregion

        #region Stage & Pattern Management
        void GoToNextStage()
        {
            StopCurrentStageActions();
            currentStageIndex++;
            if (currentStageIndex >= bossData.stageSequence.Length)
            {
                Die();
                return;
            }
            currentStage = bossData.stageSequence[currentStageIndex];
            
            ActivateMovementPattern(currentStage.movementPatternPrefab);
            currentAttackCoroutine = StartCoroutine(ExecuteStageAttacks());

            bossHealth.SetNewStage(currentStage.health);
            uiManager?.UpdateBossHealthBar(1f);

            if (currentStage.isSpellCard)
            {
                uiManager?.DeclareSpellCard(currentStage.spellCardName, currentStage.timeLimit);
                spellCardTimerCoroutine = StartCoroutine(SpellCardTimer());
            }
            else
            {
                uiManager?.ClearSpellCardDeclaration();
            }
        }
        
        private void ActivateMovementPattern(GameObject prefab)
        {
            if (prefab != null && movementPatternHolder != null)
            {
                currentMovementInstance = Instantiate(prefab, movementPatternHolder); // Làm con của Holder
                currentMovementPattern = currentMovementInstance.GetComponent<BossMovementPattern>();
                currentMovementPattern?.Initialize(this.rb);
            }
        }

        IEnumerator ExecuteStageAttacks()
        {
            foreach (var patternPrefab in currentStage.attackPatternPrefabs)
            {
                if (patternPrefab == null || attackPatternHolder == null) continue;
                GameObject attackInstance = Instantiate(patternPrefab, attackPatternHolder); // Làm con của Holder
                AttackPattern pattern = attackInstance.GetComponent<AttackPattern>();
                if (pattern != null) yield return StartCoroutine(pattern.Execute());
            }
        }

        void StopCurrentStageActions()
        {
            if (currentAttackCoroutine != null) StopCoroutine(currentAttackCoroutine);
            if (spellCardTimerCoroutine != null) StopCoroutine(spellCardTimerCoroutine);

            // ---- ĐOẠN SỬA LỖI QUAN TRỌNG ----
            // Chỉ hủy các con bên trong các Holder, không hủy chính Holder
            if (attackPatternHolder != null)
            {
                foreach (Transform child in attackPatternHolder) Destroy(child.gameObject);
            }
            if (movementPatternHolder != null)
            {
                foreach (Transform child in movementPatternHolder) Destroy(child.gameObject);
            }
            // ---------------------------------
            
            currentMovementPattern = null;
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
        #endregion

        #region Death Sequence
        void Die()
        {
            if (isDying) return;
            isDying = true;
            Debug.Log(bossData.bossName + " defeated!");
            StopCurrentStageActions();
            GetComponent<Collider2D>().enabled = false;
            uiManager?.ClearSpellCardDeclaration();
            StartCoroutine(DeathSequence());
        }
        
        private IEnumerator DeathSequence()
        {
            Debug.Log("Starting death sequence...");
            yield return new WaitForSeconds(1.5f);
            
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if(sr != null) sr.enabled = false;
            
            yield return new WaitForSeconds(0.5f);
            
            if (bossData.guaranteedUpgradeDrop != null)
                Instantiate(bossData.guaranteedUpgradeDrop, transform.position, Quaternion.identity);
            if (bossData.rareHealthDrop != null)
                Instantiate(bossData.rareHealthDrop, (Vector2)transform.position + new Vector2(0.5f, 0), Quaternion.identity);
            
            yield return new WaitForSeconds(1.0f);
            
            LevelManager.Instance?.GoToNextLevel();
            Destroy(gameObject);
        }

        IEnumerator SpellCardTimer()
        {
            yield return new WaitForSeconds(currentStage.timeLimit);
            Debug.Log("Spell Card Timed Out! Player survived.");
            GoToNextStage();
        }
        #endregion
    }
}