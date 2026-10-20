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
        public bool IsReady => !startupBlocked && balance != null && saveRepository != null;

        public int CurrentAttackMaximum => balance != null && balance.progression != null && balance.progression.attackUpgrade != null
            ? balance.progression.attackUpgrade.maxLevel : 0;
        public int CurrentHealthMaximum => balance != null && balance.progression != null && balance.progression.healthUpgrade != null
            ? balance.progression.healthUpgrade.maxLevel : 0;

        private AtomicSaveRepository saveRepository;
        private GameSaveData loadedProfile;
        private float accumulator;
        private float saveTimer;
        private bool startupBlocked;

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
        }

        private void Update()
        {
            if (Simulation == null || !Simulation.IsActive) return;
            accumulator += Time.deltaTime;
            float maxCatchUp = balance.maximumFrameCatchUp;
            if (accumulator > maxCatchUp) accumulator = maxCatchUp;
            while (accumulator >= balance.simulationStep)
            {
                Simulation.Step(balance.simulationStep);
                accumulator -= balance.simulationStep;
            }

            saveTimer += Time.unscaledDeltaTime;
            if (saveTimer >= periodicSaveSeconds)
            {
                SaveNow();
                saveTimer = 0f;
            }
        }

        private void OnSimulationEvent(CombatEvent combatEvent)
        {
            CombatEventRaised?.Invoke(combatEvent);
            if (combatEvent.Type == CombatEventType.EnemyDefeated ||
                combatEvent.Type == CombatEventType.UpgradePurchased ||
                combatEvent.Type == CombatEventType.BossSpawned ||
                combatEvent.Type == CombatEventType.StageCleared ||
                combatEvent.Type == CombatEventType.PlayerDefeated)
                SaveNow();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) SaveNow();
            else accumulator = 0f;
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
            }
            catch (Exception exception)
            {
                Debug.LogError("Não foi possível salvar o progresso. " + exception.Message, this);
            }
        }

        public int GetAttackUpgradeCost() => balance.progression.attackUpgrade.GetCost(Simulation.Profile.AttackLevel);
        public int GetHealthUpgradeCost() => balance.progression.healthUpgrade.GetCost(Simulation.Profile.HealthLevel);
        public bool TryPurchaseUpgrade(UpgradeKind kind)
        {
            if (Simulation == null || !Simulation.IsActive) return false;
            bool purchased = Simulation.TryPurchaseUpgrade(kind);
            if (purchased) SaveNow();
            return purchased;
        }
    }
}
