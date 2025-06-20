// using System;
// using System.Collections.Generic;
// using Unity.Collections;
// using Unity.Netcode;
// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class ServerManager : NetworkBehaviour
// {
//     [Header("Settings")]
//     [SerializeField] private string charakterSelectSceneName = "charakterSelect";
//     [SerializeField] private string GamePlaySceneName = "GamePlay";
//     public static ServerManager Instance { get; private set; }

//     public bool gameHasStarted;
//     public Dictionary<ulong, ClientData> ClientData { get; private set; }

//     void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//         }
//         else
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//     }


//     public void StartHost()
//     {
//         NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
//         NetworkManager.Singleton.OnServerStarted += OnNetworkReady;

//         ClientData = new Dictionary<ulong, ClientData>();

//         NetworkManager.Singleton.StartHost();
//     }

//     public void StartServer()
//     {
//         NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
//         NetworkManager.Singleton.OnServerStarted += OnNetworkReady;

//         ClientData = new Dictionary<ulong, ClientData>();

//         NetworkManager.Singleton.StartServer();
//     }

//     private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
//     {
//         if (ClientData.Count >= 4 || gameHasStarted)
//         {
//             response.Approved = false;
//             return;
//         }

//         response.Approved = true;
//         response.CreatePlayerObject = false;
//         response.Pending = false;

//         ClientData[request.ClientNetworkId] = new ClientData(request.ClientNetworkId);

//         Debug.Log($"Added client {request.ClientNetworkId}");
//     }

//     private void OnNetworkReady()
//     {
//         NetworkManager.Singleton.OnClientConnectedCallback += OnClientDisconnect;
//         NetworkManager.Singleton.SceneManager.LoadScene(charakterSelectSceneName, LoadSceneMode.Single);
//     }

//     private void OnClientDisconnect(ulong clientId)
//     {
//         if (ClientData.ContainsKey(clientId))
//         {
//             if (ClientData.Remove(clientId))
//             {
//                 Debug.Log($"Removed client {clientId}");
//             }
//         }
//     }

//     public void SetCharakter(ulong clientId, int charakterId)
//     {
//         if (ClientData.TryGetValue(clientId, out ClientData data))
//         {
//             data.charakterId = charakterId;
//         }
//     }

//     public void StartGame()
//     {
//         gameHasStarted = true;

//         NetworkManager.Singleton.SceneManager.LoadScene(GamePlaySceneName, LoadSceneMode.Single);
//     }
// }
