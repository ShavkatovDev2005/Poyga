using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class playerList : MonoBehaviour
{
    public GameObject playerUIPrefab, content;
    public LobbyScript lobbyScript;
    private List<GameObject> PlayerList;
    float time;
    string myPlayerId;
    [SerializeField] TextMeshProUGUI mapText,maxPlayers;

    void Start()
    {
        PlayerList = new List<GameObject>();
        myPlayerId = AuthenticationService.Instance.PlayerId;
    }
    void Update()
    {
        time += Time.deltaTime;
        if (time >= 3f)
        {
            time = 0f;
            PrintPlayers();
        }
    }
    private void PrintPlayers()
    {
        if (LobbyScript.joinedLobby == null)
            return;

        for (int i = 0; i < PlayerList.Count; i++)
        {
            Destroy(PlayerList[i]);
        }
        PlayerList.Clear();

        Lobby lobby = LobbyScript.joinedLobby;
        mapText.text = lobby.Data["Map"].Value;
        maxPlayers.text = lobby.Players.Count + " / " + lobby.MaxPlayers;

        for (int i = 0; i < lobby.Players.Count; i++)
        {
            GameObject playerUI = Instantiate(playerUIPrefab, content.transform);
            PlayerList.Add(playerUI);
            playerUI.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = lobby.Players[i].Data["Playername"].Value;
            playerUI.transform.GetChild(0).GetChild(1).gameObject.SetActive(lobby.Players[i].Id == lobby.HostId);// is it owner
            if (lobbyScript.host() && lobby.Players[i].Id != myPlayerId)
            {
                playerUI.name = lobby.Players[i].Id;
                playerUI.transform.GetChild(0).GetChild(2).gameObject.SetActive(true);
                playerUI.transform.GetChild(0).GetChild(2).GetComponent<Button>().onClick.AddListener(() => kickPlayer(playerUI.name));
            }
            else
            {
                playerUI.transform.GetChild(0).GetChild(2).gameObject.SetActive(false);
            }
        }

        // Debug.Log($"Lobby {lobby.Name} has {lobby.Players.Count} players. " + "gamemode=" + lobby.Data["GameMode"].Value + " map=" + lobby.Data["Map"].Value);
        // foreach (var player in lobby.Players)
        // {
        //     Debug.Log($"Player ID: {player.Id}, Name: {player.Data["Playername"].Value}");
        // }
    }

    private void kickPlayer(string playerId)
    {
        lobbyScript.kickPlayer(playerId);
    }
}
