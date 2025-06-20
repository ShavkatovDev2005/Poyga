using System;
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
    [SerializeField] GameObject[] playerPrifabs;
    [SerializeField] TextMeshProUGUI lobbyCodeTEXT;
    [SerializeField] relay relay;

    public static int maxPlayers;
    public static string lobbyName;
    public static string GameMode;
    public static string Map;
    public static string Password;
    public static bool lobbyIsPrivate;

    public static Lobby hostLobby;
    public static Lobby joinedLobby;
    private float heartbeartTimer;
    public string playerName;

    private float lobbyUpdateTimer = 1.1f;

    void Start()
    {
        playerName = "sirojiddin" + UnityEngine.Random.Range(0, 1000);
        lobbyName = "MyLobby";
        maxPlayers = 4;
        GameMode = "Deathmatch";
        Map = "Map1";
        lobbyIsPrivate = true;


        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        UnityServices.InitializeAsync(initializationOptions);

        AuthenticationService.Instance.SignedIn += () =>
        {
            //do nothing
            Debug.Log("Sigined in " + AuthenticationService.Instance.PlayerId);
        };

        AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    public async void StartGame()
    {
        if (IsLobbyHost())
        {
            try
            {
                Debug.Log("Start Game");

                // await SceneManager.LoadSceneAsync(1);

                string relayCode = await relay.CreateRelayAndStartGame();
                NetworkManager.Singleton.SceneManager.LoadScene( "map", LoadSceneMode.Single );


                Lobby lobby = await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { "KEY_START_GAME", new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                    }
                });

                joinedLobby = lobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to start game: {e.Message}");
            }
        }
    }

    private async void HandleLobbyPooling()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0)
            {
                lobbyUpdateTimer = 1.1f;

                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);

                if (!IsPlayerInLobby(AuthenticationService.Instance.PlayerId))
                {
                    joinedLobby = null;
                    await SceneManager.LoadSceneAsync(0);
                }

                if (joinedLobby.Data["KEY_START_GAME"].Value != "0")
                {
                    //start game
                    if (!IsLobbyHost())
                    {
                        // await SceneManager.LoadSceneAsync(1);

                        relay.JoinRelayAndStartGame(joinedLobby.Data["KEY_START_GAME"].Value);
                        joinedLobby = null;
                    }
                }

                // OnGameStarted?.Invoke(this, LobbyEventArgs.Empty);
            }
        }
    }
    public bool IsPlayerInLobby(string playerId)
    {
        if (joinedLobby == null || joinedLobby.Players == null)
            return false;

        foreach (var player in joinedLobby.Players)
        {
            if (player.Id == playerId)
                return true;
        }

        return false;
    }
    bool IsLobbyHost()
    {
        if (hostLobby != null)
        {
            return hostLobby.HostId == AuthenticationService.Instance.PlayerId;
        }
        else if (joinedLobby != null)
        {
            return joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
        }
        return false;
    }
    private void Update()
    {
        HandleLobbyPooling();
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
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = lobbyIsPrivate,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, GameMode ) },
                    { "Map", new DataObject(DataObject.VisibilityOptions.Public, Map ) },
                    { "KEY_START_GAME", new DataObject(DataObject.VisibilityOptions.Member, "0") }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = lobby;
            lobbyCodeTEXT.text = "XONA KODI: " + lobby.LobbyCode;

            PrintPlayers(hostLobby);

            // Debug.Log($"Lobby created: {lobby.Name} with ID: {lobby.Id}" + " maxPlayers: " + lobby.MaxPlayers + " LobbyCode: " + lobby.LobbyCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to create lobby: {e.Message}");
        }
    }

    public async Task<QueryResponse> ListLobbies()
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

            // Debug.Log($"Found {response.Results.Count} lobbies:");
            // foreach (var lobby in response.Results)
            // {
            //     Debug.Log($"Lobby Name: {lobby.Name}, ID: {lobby.Id}, Players: {lobby.Players.Count}/{lobby.MaxPlayers}" + " gamemode=" + lobby.Data["GameMode"].Value + " map=" + lobby.Data["Map"].Value);
            // }
            return response;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to list lobbies: {e.Message}");
            return null;
        }
    }


    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;

            refleshLobbylist.Instance.joinedLobby();

            Debug.Log("Joined lobby with code: " + lobbyCode);

            PrintPlayers(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to list lobbies: {e.Message}");
        }
    }
    public async void JoinLobbyById(string lobbyId)
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByCodeOptions = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId, joinLobbyByCodeOptions);
            joinedLobby = lobby;

            refleshLobbylist.Instance.joinedLobby();

            Debug.Log("Joined lobby with code: " + lobbyId);
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
                {"Playername", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)}
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
                    {"Playername", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)}
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

    public void changeLobbyname(TMP_InputField inputField)
    {
        lobbyName = inputField.text;
    }
    public void enterThePassword(TMP_InputField inputField)
    {
        Password = inputField.text;
    }

    public bool host()
    {
        return hostLobby != null;
    }

}
