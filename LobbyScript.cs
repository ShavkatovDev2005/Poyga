using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI lobbyCodeText;
    [SerializeField] TMP_InputField lobbyCode;
    [SerializeField] relay relay;
    public static Lobby hostLobby;
    public static Lobby joinedLobby;
    private float heartbeartTimer;
    public string playerName;

    private float lobbyUpdateTimer = 1.1f;

    void Start()
    {
        playerName = "sirojiddin" + UnityEngine.Random.Range(0, 1000);


        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        UnityServices.InitializeAsync(initializationOptions);

        AuthenticationService.Instance.SignedIn += () =>
        {
            //do nothing
            Debug.Log("Sigined in " + AuthenticationService.Instance.PlayerId);
        };

        AuthenticationService.Instance.SignInAnonymouslyAsync();

        // Lobby güncellemelerini başlat
        if (joinedLobby != null)
        {
            StartCoroutine(PollLobbyUpdates());
        }
    }
    private void Update()
    {
        HandleLobbyHeartBeat();
    }
    private IEnumerator PollLobbyUpdates()
    {
        while (true)
        {
            if (joinedLobby != null)
            {
                var lobbyTask = LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                while (!lobbyTask.IsCompleted)
                    yield return null;

                if (lobbyTask.IsCompletedSuccessfully)
                {
                    joinedLobby = lobbyTask.Result;
                    PrintPlayers(joinedLobby);  // istersen oyuncu listesini burada güncelleyebilirsin
                }
                else if (lobbyTask.IsFaulted)
                {
                    Debug.LogError("Lobby update failed: " + lobbyTask.Exception);
                }
            }

            yield return new WaitForSeconds(1.1f); // İstekler arası bekleme süresi
        }
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

                if (joinedLobby.Data["KEY_START_GAME"].Value != "0")
                {
                    if (!IsHost())
                    {
                        Debug.Log("Joining relay...");
                        await relay.JoinRelay();
                    }

                    joinedLobby = null;

                    // OnGameStarted?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
    bool IsHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
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
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "Marraga yet") },//DataObject.IndexOptions.S1 nima qiladi?
                    { "Map", new DataObject(DataObject.VisibilityOptions.Public, "1-xarita") },
                    { "KEY_START_GAME", new DataObject(DataObject.VisibilityOptions.Member, "0") }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = lobby;
            lobbyCodeText.text = "Xona kodi: " + lobby.LobbyCode;

            PrintPlayers(hostLobby);

            // Debug.Log($"Lobby created: {lobby.Name} with ID: {lobby.Id}" + " maxPlayers: " + lobby.MaxPlayers + " LobbyCode: " + lobby.LobbyCode);
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
                Debug.Log($"Lobby Name: {lobby.Name}, ID: {lobby.Id}, Players: {lobby.Players.Count}/{lobby.MaxPlayers}" + " gamemode=" + lobby.Data["GameMode"].Value + " map=" + lobby.Data["Map"].Value);
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
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode.text, joinLobbyByCodeOptions);
            joinedLobby = lobby;

            Debug.Log("Joined lobby with code: " + lobbyCode.text);

            PrintPlayers(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to list lobbies: {e.Message}");
        }
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {"Playername", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
            }
        };
    }

    public void PrintPlayers()
    {
        PrintPlayers(joinedLobby);
    }
    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log($"Lobby {lobby.Name} has {lobby.Players.Count} players. " + "gamemode=" + lobby.Data["GameMode"].Value + " map=" + lobby.Data["Map"].Value);
        foreach (var player in lobby.Players)
        {
            Debug.Log($"Player ID: {player.Id}, Name: {player.Data["Playername"].Value}");
        }
    }

    public async void UpdateLobbyGamemode(string gamemode)
    {
        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gamemode) }
                }
            });
            joinedLobby = hostLobby;

            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to update lobby gamemode: {e.Message}");
        }
    }

    public async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {"Playername", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
                }
            });
            Debug.Log($"Player name updated to: {playerName}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to update lobby gamemode: {e.Message}");
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
    public async void LeaveLobby()
    {
        try
        {
            if (joinedLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                joinedLobby = null;
                hostLobby = null;
                lobbyCodeText.text = "Left the lobby";
                Debug.Log("Left the lobby successfully.");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to leave lobby: {e.Message}");
        }
    }

    public void kickPlayer(string playerId)
    {
        if (hostLobby == null)
        {
            Debug.LogError("You are not the host of the lobby.");
            return;
        }

        if (joinedLobby.Players.Exists(p => p.Id == playerId))
        {
            LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
        }
        else
        {
            Debug.LogError("Player not found in the lobby.");
        }
    }

    private async void MigrateLobbyData()
    {
        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinedLobby.Players[1].Id,
            });
            joinedLobby = hostLobby;

            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to update lobby gamemode: {e.Message}");
        }
    }

    public async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to delete lobby: {e.Message}");
        }
    }
}
