using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionOnline : MonoBehaviour 
{
    public void SelectCharacter(int characterIndex)
    {
        // Serverga tanlangan karakter indexini yuboramiz
        SelectCharacterServerRpc(characterIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    void SelectCharacterServerRpc(int characterIndex, ServerRpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        CharacterSpawnManager.Instance.SpawnCharacter(characterIndex, clientId);
    }
}
