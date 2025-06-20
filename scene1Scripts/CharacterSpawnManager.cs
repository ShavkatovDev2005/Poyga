using Unity.Netcode;
using UnityEngine;

public class CharacterSpawnManager : NetworkBehaviour
{
    public static CharacterSpawnManager Instance;

    public GameObject[] characterPrefabs; // Prefablar arrayi

    void Awake()
    {
        Instance = this;
    }

    public void SpawnCharacter(int characterIndex, ulong clientId)
    {
        if (characterIndex < 0 || characterIndex >= characterPrefabs.Length)
            return;

        GameObject character = Instantiate(characterPrefabs[characterIndex]);

        // O'yinchi uchun player object sifatida spawnlash
        character.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}
