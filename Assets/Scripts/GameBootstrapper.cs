using UnityEngine;

public static class GameBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeGame()
    {
        if (GameObject.FindAnyObjectByType<GameManager>() == null)
        {
            // Resources klasöründen ana prefab'i yükle
            GameObject managersPrefab = Resources.Load<GameObject>("GlobalManagers");

            if (managersPrefab != null)
            {
                GameObject instance = GameObject.Instantiate(managersPrefab);
                GameObject.DontDestroyOnLoad(instance);
                Debug.Log("<color=green>[Bootstrapper]</color> Global Yöneticiler Başarıyla Yüklendi.");
            }
            else
            {
                Debug.LogError("[Bootstrapper] 'Resources/GlobalManagers' prefabi bulunamadı!");
            }
        }
    }
}
