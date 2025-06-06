using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI lobbyCodeText;
    [SerializeField] string lobbyCode;
    private Lobby hostLobby;
    private float heartbeartTimer;

    async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in successfully! " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    private void Update()
    {
        HandleLobbyHeartBeat();
    }

    private async void HandleLobbyHeartBeat()
    {
        if (hostLobby != null)
        {
            heartbeartTimer -= Time.deltaTime;
            if (heartbeartTimer < 0)
            {
                float heartbeatTimerMax = 15;
                heartbeartTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }
    public async void createLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 4;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer()
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;
            lobbyCodeText.text = "Lobby Code: " + lobby.LobbyCode;

            PrintPlayers(hostLobby);

            Debug.Log($"Lobby created: {lobby.Name} with ID: {lobby.Id}" + " maxPlayers: " + lobby.MaxPlayers + " LobbyCode: " + lobby.LobbyCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to create lobby: {e.Message}");
        }
    }

    public async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            Debug.Log($"Found {response.Results.Count} lobbies:");
            foreach (var lobby in response.Results)
            {
                Debug.Log($"Lobby Name: {lobby.Name}, ID: {lobby.Id}, Players: {lobby.Players.Count}/{lobby.MaxPlayers}");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to list lobbies: {e.Message}");
        }
    }


    public async void JoinLobbyByCode()
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };
            Lobby JoinedLobbie = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);

            Debug.Log("Joined lobby with code: " + lobbyCode);

            PrintPlayers(JoinedLobbie);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to list lobbies: {e.Message}");
        }
    }

    public async void quickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to quick join lobby: {e.Message}");
        }
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {"Playername", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "sirojiddin "+ Random.Range(0,1000))}
            }
        };
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log($"Lobby {lobby.Name} has {lobby.Players.Count} players:");
        foreach (var player in lobby.Players)
        {
            Debug.Log($"Player ID: {player.Id}, Name: {player.Data["Playername"].Value}");
        }
    }
}
