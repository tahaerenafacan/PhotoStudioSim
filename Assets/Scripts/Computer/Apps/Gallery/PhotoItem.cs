using System;
using UnityEngine;
using UnityEngine.UI;

namespace Computer.Apps.Gallery
{
    public class PhotoItem : MonoBehaviour
    {
        [SerializeField] private RawImage photoImage;
        [SerializeField] private Button button;

        public void SetImage(Texture2D texture, Action onClick)
        {
            photoImage.texture = texture;
            button.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}