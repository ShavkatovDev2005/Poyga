using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class offlineSettings : MonoBehaviour
{
    public GameObject playerOfflinePrifab;
    [SerializeField] Animator anim;
    private string selectedMap;
    public void open()
    {
        anim.Play("open");
    }
    public void selectMap(Button button)
    {
        anim.Play("close");
        selectedMap = button.gameObject.name;
    }
    public async void StartGame()
    {
        await SceneManager.LoadSceneAsync(1);
        Instantiate(playerOfflinePrifab);
    }
}
