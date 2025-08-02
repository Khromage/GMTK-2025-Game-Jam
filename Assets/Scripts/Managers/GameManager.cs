using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // singleton instance
    public static GameManager Instance { get; private set; }

    [Header("Manager References")]
    [SerializeField] private CurrencyManager _currencyManager;
    [SerializeField] private UpgradeManager _upgradeManager;
    [SerializeField] private PrestigeManager _prestigeManager;
    [SerializeField] private ProgressManager _progressManager;
    [SerializeField] private SaveManager _saveManager;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private AudioManager _audioManager;
    [SerializeField] private InputManager _inputManager;

    // public getters to access managers & their methods without accidentally changing the reference(s)
    public CurrencyManager CurrencyManager => _currencyManager;
    public UpgradeManager UpgradeManager => _upgradeManager;
    public PrestigeManager PrestigeManager => _prestigeManager;
    public ProgressManager ProgressManager => _progressManager;
    public SaveManager SaveManager => _saveManager;
    public UIManager UIManager => _uiManager;
    public AudioManager AudioManager => _audioManager;
    public InputManager InputManager => _inputManager;

    void Awake()
    {
        // singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // might not be needed if everything is in one scene?
            InitializeManagers();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadGame();
        _upgradeManager.ManualCheckUnlocks();
        StartGameLoop();
    }

    private void InitializeManagers()
    {
        // Initialize managers in dependency order
        _currencyManager.Initialize();
        _upgradeManager.Initialize(_currencyManager);
        _progressManager.Initialize(_upgradeManager);
        _prestigeManager.Initialize(_currencyManager, _progressManager, _upgradeManager);
        _saveManager.Initialize();
        _audioManager.Initialize();
        _inputManager.Initialize(_progressManager, _upgradeManager, _prestigeManager);
        _uiManager.Initialize(_currencyManager, _upgradeManager, _progressManager, _prestigeManager, _audioManager);
    }
    
    private void LoadGame()
    {
        if (_saveManager.HasSaveFile())
        {
            SaveData saveData = _saveManager.LoadGame();
            ApplySaveDataToManagers(saveData);
        }
        else
        {
            InitializeDefaultGameState();
        }
    }
    
    private void ApplySaveDataToManagers(SaveData saveData)
    {
        _currencyManager.LoadFromSaveData(saveData);
        _upgradeManager.LoadFromSaveData(saveData);
        _progressManager.LoadFromSaveData(saveData);
        _prestigeManager.LoadFromSaveData(saveData);
    }
    
    private void InitializeDefaultGameState()
    {
        _currencyManager.SetDefaultValues();
        _upgradeManager.SetDefaultValues();
        _progressManager.SetDefaultValues();
        _prestigeManager.SetDefaultValues();
    }

    private void StartGameLoop()
    {
        StartCoroutine(AutoSaveCoroutine());
        StartCoroutine(AutoClickCoroutine());
    }

    private IEnumerator AutoSaveCoroutine()
    {
        while (true)
        {
            // auto save interval. can update this later to be a variable based on player selected interval
            yield return new WaitForSeconds(30f);
            SaveGame();
        }
    }

    private IEnumerator AutoClickCoroutine()
    {
        while (true)
        {
            float autoClickRate = _upgradeManager.GetAutoClickRate();
            if (autoClickRate > 0)
            {
                yield return new WaitForSeconds(1f / autoClickRate);
                _progressManager.AutoPush();
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    public void SaveGame()
    {
        SaveData saveData = CreateSaveData();
        _saveManager.SaveGame(saveData);
    }

    private SaveData CreateSaveData()
    {
        return new SaveData
        {
            grit = _currencyManager.GetGrit(),
            favors = _currencyManager.GetFavors(),
            normalUpgradeLevels = _upgradeManager.GetNormalUpgradeLevels(),
            prestigeUpgradeLevels = _upgradeManager.GetPrestigeUpgradeLevels(),
            currentDistance = _progressManager.GetCurrentDistance(),
            currentSlope = _progressManager.GetCurrentSlope(),
            comboState = _progressManager.GetComboState(),
            lastSaveTime = System.DateTimeOffset.Now.ToUnixTimeSeconds()
        };
    }

    // called when system goes to sleep
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SaveGame();
    }
    // called when switching apps
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            SaveGame();
    }
    // called on exit
    void OnApplicationQuit()
    {
        SaveGame();
    }
}
