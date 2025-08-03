using UnityEngine;

public class Sisyphus : MonoBehaviour
{
    [SerializeField] private Animator _anim;
    [SerializeField] private Transform _boulder;
    private ProgressManager _progressManager;

    public void Initialize(ProgressManager progressManager)
    {
        _progressManager = progressManager;

        if (_progressManager != null)
            progressManager.OnPushPerformed += HandlePushAnimation;

    }

    void OnDestroy()
    {
        if (_progressManager != null)
            _progressManager.OnPushPerformed -= HandlePushAnimation;
    }


    private void HandlePushAnimation()
    {
        _anim.SetTrigger("Push");
        _boulder.Rotate(0, 0, -2.5f);
    }
}
