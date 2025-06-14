using UnityEngine;

public class lobbyCreateMenu : MonoBehaviour
{
    public GameObject privateText, publicText;
    void Start()
    {
        changeLobbyPravicy();
    }
    public void changeLobbyPravicy()
    {
        LobbyScript.lobbyIsPrivate = !LobbyScript.lobbyIsPrivate;
        privateText.SetActive(LobbyScript.lobbyIsPrivate);
        publicText.SetActive(!LobbyScript.lobbyIsPrivate);
    }
}
