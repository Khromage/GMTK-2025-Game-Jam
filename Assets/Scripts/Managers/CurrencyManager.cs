using UnityEngine;
using UnityEngine.Events;
public class CurrencyManager : MonoBehaviour
{
    [Header("Currency Values")]
    [SerializeField] private int _grit = 0;
    [SerializeField] private int _favors = 0;

    // events used to send update notification to systems like ui
    public UnityAction<int> OnGritChanged;
    public UnityAction<int> OnFavorsChanged;

    // public getters for private values
    public int GetGrit() => _grit;
    public int GetFavors() => _favors;

    private void NotifyGritChanged() => OnGritChanged?.Invoke(_grit);
    private void NotifyFavorsChanged() => OnFavorsChanged?.Invoke(_favors);

    public void Initialize()
    {
        // setup events to be handled and pass value parameter
        OnGritChanged = new UnityAction<int>((value) => { });
        OnFavorsChanged = new UnityAction<int>((value) => { });
    }

    public void SetDefaultValues()
    {
        _grit = 0;
        _favors = 0;
        NotifyGritChanged();
        NotifyFavorsChanged();
    }

    public void LoadFromSaveData(SaveData saveData)
    {
        _grit = saveData.grit;
        _favors = saveData.favors;
        NotifyGritChanged();
        NotifyFavorsChanged();
    }

    public void AddGrit(int amount)
    {
        if (amount <= 0)
            return;

        _grit += amount;
        NotifyGritChanged();

        // play sound effect
        //GameManager.Instance.AudioManager.PlaySFX(SFXType.GritEarned);
    }

    public bool SpendGrit(int amount)
    {
        if (amount <= 0 || _grit < amount)
            return false;

        _grit -= amount;
        NotifyGritChanged();

        // play sound effect
        //GameManager.Instance.AudioManager.PlaySFX(SFXType.GritSpent);
        return true;
    }
    
    public void AddFavors(int amount)
    {
        if (amount <= 0) return;
        
        _favors += amount;
        NotifyFavorsChanged();
        
        // Optional: Play sound effect
        //GameManager.Instance.AudioManager.PlaySFX(SFXType.FavorsEarned);
    }
    
    public bool SpendFavors(int amount)
    {
        if (amount <= 0 || _favors < amount) return false;
        
        _favors -= amount;
        NotifyFavorsChanged();
        return true;
    }
}
