using System;
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
    public async Task<String> CreateRelay()//o'yinni boshlash
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(LobbyScript.maxPlayers);

            string joincode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Debug.Log(joincode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            return joincode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Failed to create relay: " + e.Message);
            return null;
        }
    }

    public async void JoinRelay(string joinCode)//o'yinga qo'shilish
    {
        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Failed to join relay: " + e.Message);
        }
    }
}
