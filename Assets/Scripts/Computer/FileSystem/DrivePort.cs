using UnityEngine;
using UnityEngine.Localization;

namespace SyntaxSultan.ComputerSystem.FileSystem
{
    /// <summary>
    /// Bilgisayar üzerindeki fiziksel sürücü portu (USB, SD kart yuvası).
    ///
    /// SAHNE KURULUMU:
    ///   1. Bu bileşeni kasanın collider'ına ekle
    ///   2. driveSlotTransform: takılan drive'ın görsel olarak oturacağı boş obje
    ///   3. Oyuncu elinde RemovableDriveItem tutarken E'ye basınca mount edilir
    ///      Sürücü zaten takılıyken E'ye basınca eject edilir
    /// </summary>
    public class DrivePort : MonoBehaviour, IInteractable
    {
        [SerializeField] private Computer computer;
        [SerializeField] private Transform driveSlotTransform;

        [Header("Localization")]
        [SerializeField] private LocalizedString insertHint;
        [SerializeField] private LocalizedString ejectHint;
        [SerializeField] private LocalizedString portName;

        private RemovableDriveItem insertedDrive;

        public LocalizedString InteractHint => insertedDrive == null ? insertHint : ejectHint;
        public LocalizedString InteractName => portName;

        // Bilgisayar açık VE (elde drive var ya da port doluysa) etkileşime izin ver
        public bool CanInteract
        {
            get
            {
                if (computer == null || !computer.IsPoweredOn) return false;
                bool holdingDrive = PlayerItemHolder.Instance != null &&
                                    PlayerItemHolder.Instance.CurrentItem is RemovableDriveItem;
                return holdingDrive || insertedDrive != null;
            }
        }

        public void Interact()
        {
            if (insertedDrive != null) EjectDrive();
            else TryInsertDriveFromPlayer();
        }

        private void TryInsertDriveFromPlayer()
        {
            if (PlayerItemHolder.Instance?.CurrentItem is not RemovableDriveItem driveItem) return;

            PlayerItemHolder.Instance.DetachForStorage();

            insertedDrive = driveItem;

            // Drive'ı portun slot pozisyonuna yerleştir
            if (driveSlotTransform != null)
            {
                driveItem.transform.SetPositionAndRotation(
                    driveSlotTransform.position,
                    driveSlotTransform.rotation);
            }
            else
            {
                driveItem.gameObject.SetActive(false);
            }

            VirtualFileSystem.Instance?.MountDrive(driveItem.Drive);
        }

        private void EjectDrive()
        {
            if (insertedDrive == null) return;

            VirtualFileSystem.Instance?.UnmountDrive(insertedDrive.Drive);

            // Drive'ı porttan çıkar, dünyaya bırak
            insertedDrive.gameObject.SetActive(true);
            insertedDrive.OnDrop(transform.forward, 1f);
            insertedDrive = null;
        }
    }
}