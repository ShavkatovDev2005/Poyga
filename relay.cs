using System.Net.Security;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class relay : MonoBehaviour
{
    public TMP_InputField joinCodeInputField;
    // async void Start()
    // {
    //     await UnityServices.InitializeAsync();

    //     AuthenticationService.Instance.SignedIn += () =>
    //     {
    //         Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
    //     };
    //     await AuthenticationService.Instance.SignInAnonymouslyAsync();
    // }
    

    public async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joincode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log(joincode);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Failed to create relay: " + e.Message);
        }
    }

    public async void JoinRelay()
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCodeInputField.text);

            Debug.Log("Joined relay with allocation ID: " + joinAllocation.AllocationId);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Failed to join relay: " + e.Message);
        }
    }
}
