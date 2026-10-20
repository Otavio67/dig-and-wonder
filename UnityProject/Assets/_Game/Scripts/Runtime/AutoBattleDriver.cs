using System;
using UnityEngine;

namespace DigAndWonder
{
    public sealed class AutoBattleDriver : MonoBehaviour
    {
        [SerializeField] private CombatBalanceDefinition balance;
        [SerializeField] private float periodicSaveSeconds = 5f;

        public IdleCombatSimulation Simulation { get; private set; }
        public event Action<CombatEvent> CombatEventRaised;
        public string LastSaveError { get; private set; }
        public bool IsSaveBlocked => !string.IsNullOrEmpty(LastSaveError);
        public bool IsReady => !startupBlocked && !IsSaveBlocked && balance != null && saveRepository != null;

        public int CurrentAttackMaximum => balance != null && balance.progression != null && balance.progression.attackUpgrade != null
            ? balance.progression.attackUpgrade.maxLevel : 0;
        public int CurrentHealthMaximum => balance != null && balance.progression != null && balance.progression.healthUpgrade != null
            ? balance.progression.healthUpgrade.maxLevel : 0;
        public float CurrentAttackDamage => balance == null || balance.progression == null || balance.progression.attackUpgrade == null || Simulation == null
            ? 0f : balance.drillAttackDamage * (1f + Simulation.Profile.AttackLevel * balance.progression.attackUpgrade.additiveBonus);

        private AtomicSaveRepository saveRepository;
        private GameSaveData loadedProfile;
        private float accumulator;
        private float saveTimer;
        private bool startupBlocked;
        private bool saveRequested;
        private bool applicationPaused;

        private void Awake()
        {
            if (balance == null) balance = Resources.Load<CombatBalanceDefinition>("Data/CombatBalance");
            if (balance == null)
            {
                Debug.LogError("CombatBalanceDefinition não encontrado em Resources/Data.", this);
                startupBlocked = true;
                enabled = false;
                return;
            }

            saveRepository = new AtomicSaveRepository(Application.persistentDataPath);
            bool recovered;
            bool hasValidSave = saveRepository.TryLoad(out GameSaveData profile, out recovered);
            if (!hasValidSave && saveRepository.HasAnySaveFile)
            {
                Debug.LogError("Há arquivos de progresso, mas nenhum passou pela validação. O jogo não iniciará um perfil vazio por cima deles.", this);
                startupBlocked = true;
                enabled = false;
                return;
            }

            if (!hasValidSave)
            {
                loadedProfile = new GameSaveData();
                return;
            }

            if (recovered) Debug.LogWarning("Save recuperado do backup validado.", this);

            loadedProfile = profile;
            if (profile.ActiveRun != null || profile.Gold > 0 || profile.Keys > 0 || profile.Chests > 0 ||
                profile.AttackLevel > 0 || profile.HealthLevel > 0 || profile.UnlockedStageId != "prado_cristalino")
                CreateSimulation(profile);
        }

        public void StartNewGame()
        {
            if (!IsReady) return;
            if (Simulation != null && Simulation.IsActive) return;
            if (Simulation != null) loadedProfile = Simulation.Profile;
            if (loadedProfile == null) loadedProfile = new GameSaveData();
            loadedProfile.ActiveRun = null;
            CreateSimulation(loadedProfile);
            SaveNow();
        }

        public bool TryStartNewGame()
        {
            if (!IsReady) return false;
            StartNewGame();
            return Simulation != null;
        }

        private void CreateSimulation(GameSaveData profile)
        {
            if (Simulation != null) Simulation.EventRaised -= OnSimulationEvent;
            CombatConfiguration config = CombatConfigurationFactory.Create(balance);
            string stageId = profile.ActiveRun != null ? profile.ActiveRun.StageId : profile.UnlockedStageId;
            uint seed = (uint)DateTime.UtcNow.Ticks;
            Simulation = new IdleCombatSimulation(config, profile, stageId, seed);
            Simulation.EventRaised += OnSimulationEvent;
            accumulator = 0f;
            saveTimer = 0f;
        }

        private void Update()
        {
            if (Simulation == null || applicationPaused) return;
            saveTimer += Time.unscaledDeltaTime;
            if (IsSaveBlocked)
            {
                // Uma falha de disco pausa a tentativa; a próxima escrita tenta preservar o mesmo estado.
                if (saveTimer >= periodicSaveSeconds) SaveNow();
                return;
            }
            if (!Simulation.IsActive) return;
            accumulator += Time.deltaTime;
            float maxCatchUp = balance.maximumFrameCatchUp;
            if (accumulator > maxCatchUp) accumulator = maxCatchUp;
            while (accumulator >= balance.simulationStep && Simulation.IsActive)
            {
                Simulation.Step(balance.simulationStep);
                accumulator -= balance.simulationStep;
            }

            // Eventos podem ocorrer no meio de um golpe. Só persistimos depois do passo completo.
            if (saveRequested || saveTimer >= periodicSaveSeconds) SaveNow();
        }

        private void OnSimulationEvent(CombatEvent combatEvent)
        {
            CombatEventRaised?.Invoke(combatEvent);
            if (combatEvent.Type == CombatEventType.EnemyDefeated ||
                combatEvent.Type == CombatEventType.UpgradePurchased ||
                combatEvent.Type == CombatEventType.BossSpawned ||
                combatEvent.Type == CombatEventType.StageCleared ||
                combatEvent.Type == CombatEventType.PlayerDefeated)
                saveRequested = true;
        }

        private void OnApplicationPause(bool paused)
        {
            applicationPaused = paused;
            accumulator = 0f;
            if (paused) SaveNow();
        }

        private void OnApplicationQuit() => SaveNow();

        private void OnDestroy()
        {
            if (Simulation != null) Simulation.EventRaised -= OnSimulationEvent;
        }

        private void SaveNow()
        {
            if (saveRepository == null || Simulation == null) return;
            try
            {
                if (Simulation.IsActive) Simulation.Profile.ActiveRun = Simulation.CaptureRun();
                saveRepository.Save(Simulation.Profile);
                LastSaveError = null;
                saveRequested = false;
            }
            catch (Exception exception)
            {
                if (!IsSaveBlocked) Debug.LogError("Não foi possível salvar o progresso. O combate foi pausado. " + exception.Message, this);
                LastSaveError = "NÃO FOI POSSÍVEL SALVAR — COMBATE PAUSADO. TENTANDO NOVAMENTE.";
                accumulator = 0f;
            }
            saveTimer = 0f;
        }

        public int GetAttackUpgradeCost() => balance.progression.attackUpgrade.GetCost(Simulation.Profile.AttackLevel);
        public int GetHealthUpgradeCost() => balance.progression.healthUpgrade.GetCost(Simulation.Profile.HealthLevel);
        public bool TryPurchaseUpgrade(UpgradeKind kind)
        {
            if (Simulation == null || !Simulation.IsActive || IsSaveBlocked || applicationPaused) return false;
            bool purchased = Simulation.TryPurchaseUpgrade(kind);
            if (purchased) SaveNow();
            return purchased;
        }

        public bool TryPurchaseUpgradeAtBase(UpgradeKind kind)
        {
            if (Simulation == null || IsSaveBlocked || applicationPaused) return false;
            bool purchased = Simulation.TryPurchaseUpgradeAtBase(kind);
            if (purchased) SaveNow();
            return purchased;
        }
    }
}
