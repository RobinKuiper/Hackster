using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using System.Collections.Generic;
using PlayFab.DataModels;
using EntityKey = PlayFab.DataModels.EntityKey;
using PlayFab.CloudScriptModels;
using PlayFab.Json;

public class PlayFabController : MonoBehaviour
{
    public static PlayFabController PFC;

    private string username;
    private string email;
    private string password;
    private string passwordC;

    private string entityId;
    private string entityType;

    public GameObject panel;

    private void OnEnable()
    {
        if(PlayFabController.PFC == null)
            PlayFabController.PFC = this;
        else
            if(PlayFabController.PFC != this)
                Destroy(this.gameObject);
        DontDestroyOnLoad(this.gameObject);
    }

    public void Start()
    {
#if UNITY_ANDROID
        var request = new LoginWithAndroidDeviceIDRequest { AndroidDeviceId = ReturnMobileID(), CreateAccount = true };
        PlayFabClientAPI.LoginWithAndroidDeviceID(request, OnLoginSuccess, OnLoginFailure);

#elif UNITY_IOS
        var request = new LoginWithIOSDeviceIDRequest { DeviceId = ReturnMobileID(), CreateAccount = true };
        PlayFabClientAPI.LoginWithIOSDeviceID(request, OnLoginSuccess, OnLoginFailure);

#else
        var request = new LoginWithCustomIDRequest { CustomId = ReturnMobileID(), CreateAccount = true };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
#endif
    }

    #region Login
    private void OnLoginSuccess(LoginResult result)
    {
        // Get Entity Information
        entityId = result.EntityToken.Entity.Id;
        entityType = result.EntityToken.Entity.Type;

        // Get Account info to see if user has a linked account.
        var request = new GetAccountInfoRequest { PlayFabId = result.PlayFabId };
        PlayFabClientAPI.GetAccountInfo(request, 
            resultA => {
                // If no linked account show the link account panel.
                if(resultA.AccountInfo.Username == "" || resultA.AccountInfo.Username == null && 
                    (!PlayerPrefs.HasKey("LINK_ACCOUNT_REMINDER") || PlayerPrefs.GetInt("LINK_ACCOUNT_REMINDER") == 1))
                    panel.SetActive(true);
            }, 
            error => { Debug.LogError(error.GenerateErrorReport()); });

        // Get object of title entity.
        var getRequest = new GetObjectsRequest { Entity = new EntityKey { Id = entityId, Type = entityType } };
        PlayFabDataAPI.GetObjects(getRequest,
            r => {
                // If user has no pc yet, create one with the server function.
                if (!r.Objects.ContainsKey("pc1"))
                {
                    var cloudscriptrequest = new ExecuteEntityCloudScriptRequest { FunctionName = "createFirstComputer", GeneratePlayStreamEvent = true };
                    PlayFabCloudScriptAPI.ExecuteEntityCloudScript(cloudscriptrequest,
                        re => {
                            GameManager.gm.SetComputer("cpu1", "mem1");
                        },
                        error => { Debug.LogError(error.GenerateErrorReport()); });
                }
                else
                {
                    JsonObject jsonResult = (JsonObject)r.Objects["pc1"].DataObject;

                    GameManager.gm.SetComputer(jsonResult["cpu"].ToString(), jsonResult["memory"].ToString());
                }

                // A way to loop through dictionary.
                /*foreach(KeyValuePair<string, ObjectResult> obj in r.Objects)
                {
                    Debug.Log(obj.Key);
                    Debug.Log(obj.Value.ObjectName);
                }*/
            },
            error => { });
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }
    
    public static string ReturnMobileID()
    {
        string deviceID = SystemInfo.deviceUniqueIdentifier;
        return deviceID;
    }
    #endregion

    #region Account
    public void SetUsername(string usernameIn)
    {
        username = usernameIn;
    }

    public void SetEmail(string emailIn)
    {
        email = emailIn;
    }

    public void SetPassword(string passwordIn)
    {
        password = passwordIn;
    }

    public void SetPasswordC(string passwordCIn)
    {
        passwordC = passwordCIn;
    }

    public void clickCreateEmailAccount()
    {
        if (password != passwordC)
        {
            Debug.Log("Password do not match!");
        }
        else
        {
            var request = new AddUsernamePasswordRequest { Email = email, Password = password, Username = username };
            PlayFabClientAPI.AddUsernamePassword(request, OnAddLoginSuccess, OnAddLoginFailure);
        }
    }

    private void OnAddLoginSuccess(AddUsernamePasswordResult result)
    {
        Debug.Log("Succesfully Linked!");
    }

    private void OnAddLoginFailure(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }
    #endregion
}
