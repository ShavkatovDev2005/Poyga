using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyScript : MonoBehaviour
{
    [SerializeField] private relay relay;
    [SerializeField] TextMeshProUGUI lobbyCodeText;
    [SerializeField] string lobbyCode;
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float LobbyUpdateTimer;
    private float heartbeartTimer;
    [SerializeField] private string playerName;

    async void Start()
    {
        playerName = "sirojiddin " + Random.Range(0, 1000);

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
        HandleLobbyPollForUpdates();
    }

    public async void Authenticate(string playerName)
    {
        this.playerName = playerName;
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        await UnityServices.InitializeAsync(initializationOptions);

        AuthenticationService.Instance.SignedIn += () =>
        {
            //do nothing
            Debug.Log("Sigined in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
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
    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            LobbyUpdateTimer -= Time.deltaTime;
            if (LobbyUpdateTimer < 0)
            {
                
                float LobbyUpdateTimerMax = 1.1f;
                LobbyUpdateTimer = LobbyUpdateTimerMax;

                try
                {
                    Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                    joinedLobby = lobby;

                    if (joinedLobby.Data.ContainsKey("StartGame") && 
                        joinedLobby.Data["StartGame"].Value == "true" &&
                        !NetworkManager.Singleton.IsClient) // faqat hali ulanmaganlar uchun
                    {
                        string joinCode = joinedLobby.Data["RelayCode"].Value;
                        relay.JoinRelay(joinCode); // Join qilsin
                        NetworkManager.Singleton.StartClient(); // yoki StartHost/StartClient
                    }
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogError("Lobby update failed: " + e.Message);
                }
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
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "Marraga yet") },//DataObject.IndexOptions.S1 nima qiladi?
                    { "Map", new DataObject(DataObject.VisibilityOptions.Public, "1-xarita") }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = lobby;
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
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;

            Debug.Log("Joined lobby with code: " + lobbyCode);

            PrintPlayers(lobby);
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
