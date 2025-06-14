using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using System.Threading.Tasks;

public class refleshLobbylist : MonoBehaviour
{
    public GameObject lobbyPrefab, content;
    public LobbyScript lobbyScript;
    private List<GameObject> lobbyList;
    [SerializeField] TMP_InputField lobbyCode;

    void Start()
    {
        lobbyList = new List<GameObject>();
        refreshLobby();
    }
    public void RefreshLobby()
    {
        refreshLobby();
    }
    async void refreshLobby()
    {
        QueryResponse response = await lobbyScript.ListLobbies();

        for (int i = 0; i < lobbyList.Count; i++)
        {
            Destroy(lobbyList[i]);
        }
        lobbyList.Clear();
        
        if (lobbyList.Count == 0) return;

        for (int i = 0; i < response.Results.Count; i++)
        {
            GameObject lobby = Instantiate(lobbyPrefab, content.transform);
            lobbyList.Add(lobby);
            lobby.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = response.Results[i].Name;
            lobby.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = response.Results[i].Players.Count + "/" + response.Results[i].MaxPlayers;
            lobby.transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().text = response.Results[i].Data["Map"].Value;
            lobby.transform.GetChild(0).GetChild(3).GetComponent<TextMeshProUGUI>().text = response.Results[i].IsPrivate ? "Kodlangan" : "Ochiq";
            lobby.name = response.Results[i].Id;
            lobby.transform.GetChild(0).gameObject.GetComponent<Button>().onClick.AddListener(() => JoinLobby(lobby.name));
        }
        // foreach (var lobby in response.Results)
        // {
        //     Debug.Log($"Lobby Name: {lobby.Name}, ID: {lobby.Id}, Players: {lobby.Players.Count}/{lobby.MaxPlayers}" +
        //     " gamemode=" + lobby.Data["GameMode"].Value +
        //     " map=" + lobby.Data["Map"].Value);
        // }
    }
    async void JoinLobby(string lobbyId)
    {
        QueryResponse response = await getLobbyList();

        foreach (var lobby in response.Results)
        {
            if (lobby.Id == lobbyId)
            {
                lobbyScript.JoinLobbyById(lobbyId);
                return;
            }
        }
    }
    public void joinLobbyByLobbyCode()
    {
        if (lobbyCode.text != "")
        {
            lobbyScript.JoinLobbyByCode(lobbyCode.text);
        }
    }
    async Task<QueryResponse> getLobbyList()
    {
        return await LobbyService.Instance.QueryLobbiesAsync();
    }
}
