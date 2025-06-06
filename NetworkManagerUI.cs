using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    // [SerializeField] private Button ServerButton;
    [SerializeField] private Button ClientButton;
    [SerializeField] private Button HostButton;
    
    void Awake()
    {
        // ServerButton.onClick.AddListener(() => NetworkManager.Singleton.StartServer());
        ClientButton.onClick.AddListener(() => NetworkManager.Singleton.StartClient());
        HostButton.onClick.AddListener(() => NetworkManager.Singleton.StartHost());
    }
}
