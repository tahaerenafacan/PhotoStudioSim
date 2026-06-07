using UnityEngine;
using UnityEngine.UI;

public class ReviewItem : MonoBehaviour
{
    [SerializeField] private Image likeImage;
    [SerializeField] private RectTransform starsContainer;
    [SerializeField] private TMPro.TextMeshProUGUI commentText;

    public void Initialize(int starCount, Sprite likeIcon, string comment, GameObject starPrefab, GameObject emptyStarPrefab)
    {
        FunctionLibrary.SetStars(starsContainer, starCount, starPrefab, emptyStarPrefab);
        likeImage.sprite = likeIcon;
        commentText.text = comment;
    }
}