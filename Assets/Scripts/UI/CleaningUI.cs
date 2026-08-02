using SyntaxSultan.DirtSystem;
using UnityEngine;

public class CleaningUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup cleanUICG;
    [SerializeField] private Evo.UI.ProgressBar progressBar;
    private ICleanable currentCleanable;

    private void Start()
    {
        FunctionLibrary.SetCanvasGroupActive(ref cleanUICG, false);
        PlayerInteraction.Instance.OnCleanableChanged += PlayerInteraction_OnCleanableChanged;
    }

    private void OnDestroy()
    {
        if (PlayerInteraction.Instance != null)
            PlayerInteraction.Instance.OnCleanableChanged -= PlayerInteraction_OnCleanableChanged;
        if (currentCleanable != null)
            currentCleanable.OnProgressChanged -= SetProgress;
    }

    private void PlayerInteraction_OnCleanableChanged(ICleanable cleanable)
    {
        if (currentCleanable != null)
            currentCleanable.OnProgressChanged -= SetProgress;
        
        bool active = cleanable != null;
        
        currentCleanable = cleanable;
        if (active)
        {
            FunctionLibrary.SetCanvasGroupActive(ref cleanUICG, true);
            cleanable.OnProgressChanged += SetProgress;
            SetProgress(cleanable.NormalizedProgress);
        }
        else
        {
            FunctionLibrary.SetCanvasGroupActive(ref cleanUICG, false);
        }
    }

    private void SetProgress(float progress01)
    {
        progressBar.SetValue(progress01 * 100);
    }
}
