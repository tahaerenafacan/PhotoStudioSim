using TMPro;
using UnityEngine;

namespace SyntaxSultan.UI.Tutorial
{
    public class ObjectiveItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private UnityEngine.UI.Image completedImage;

        public void SetData(string description, int current, int required)
        {
            bool done = current >= required;

            descriptionText.text = description;
            if (done)
            {
                completedImage.gameObject.SetActive(true);
                progressText.text = "";
            }
            else
            {
                completedImage.gameObject.SetActive(false);
                progressText.text = $"{current}/{required}";
            }
        }
        
        
    }
}