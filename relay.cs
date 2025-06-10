using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class relay : MonoBehaviour
{
    public async void CreateRelay()//o'yinni boshlash
    {
        try
        {
            await SceneManager.LoadSceneAsync(1);

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            string joincode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(LobbyScript.joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "KEY_START_GAME", new DataObject(DataObject.VisibilityOptions.Member, joincode) }
                }
            });
            LobbyScript.joinedLobby = lobby;
            // Debug.Log("Relay created with join code: " + LobbyScript.joinedLobby.Data["KEY_START_GAME"].Value);

            NetworkManager.Singleton.StartHost();
            // Debug.Log(joincode);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Failed to create relay: " + e.Message);
        }
    }
    public async Task<bool> JoinRelay()
    {
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(LobbyScript.joinedLobby.Data["KEY_START_GAME"].Value);

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
        return !string.IsNullOrEmpty(LobbyScript.joinedLobby.Data["KEY_START_GAME"].Value) && NetworkManager.Singleton.StartClient();
    }

    // public async void JoinRelay()
    // {
    //     try
    //     {
    //         await SceneManager.LoadSceneAsync(1);

    //         JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(LobbyScript.joinedLobby.Data["KEY_START_GAME"].Value);

    //         RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

    //         NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

    //         NetworkManager.Singleton.StartClient();
    //     }
    //     catch (RelayServiceException e)
    //     {
    //         Debug.LogError("Failed to join relay: " + e.Message);
    //     }
    // }

}
