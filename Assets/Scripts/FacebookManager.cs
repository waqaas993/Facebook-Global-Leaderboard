using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Facebook.Unity;

public class FacebookManager : MonoBehaviour
{

    [Header("SDK Initialized")]
    public bool IsSDKReady = false;
    [Header("Auto Login")]
    public bool AutoLoginEnabled;
    [Header("Login Permissions")]
    public bool fbCanAccessEmail = true;
    public bool fbCanAccessProfile = true;
    public bool fbCanAccessFriends = true;
    [Header("Login Status")]
    public bool AmILoggedIn = false;

    private int PERM_LENGTH = 0;
    private int PERM_NO = 0;
    [Header("Permissions Granted")]
    public bool PERM_EMAIL = false;
    public bool PERM_PUBLICPROFILE = false;
    public bool PERM_USERFRIENDS = false;
    public string[] EXTRA_PERMS = new string[1];

    [Header("REQUEST THESE INFO")]
    public bool _ReqEmail = true;
    public bool _ReqName = true;
    [Header("User Details")]
    public string myFbId = "";
    public string myName = "";
    public string myFirstName = "";
    public string myLastName = "";
    public string myEmail = "";

    private static FacebookManager instance;

    public static FacebookManager Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        instance = this;
        if (!FB.IsLoggedIn)
        {
            FB.Init(OnFBSDKInit, OnHideUnity, null);
        }
        else if (FB.IsLoggedIn)
        {
            AmILoggedIn = true;
            StartCoroutine(ServerManager.Instance.LogIn(myFbId, myName, myEmail));
        }
    }

    private void OnFBSDKInit()
    {
        if (FB.IsInitialized)
        {
            IsSDKReady = true;
            FacebookLog("Ready!", true, false, false);
            LogIn(false);
        }
        else
        {
            IsSDKReady = false;
            FacebookLog("Failed To Initialize", false, true, false);
        }
    }

    private void OnHideUnity(bool isGAMEReady)
    {

    }


    public void LogIn(bool IsClicked)
    {
        FacebookLog("Logging....", true, false, false);
        List<string> fbPerms = new List<string>();
        if (fbCanAccessProfile)
        {
            fbPerms.Add("public_profile");
        }
        if (fbCanAccessEmail)
        {
            fbPerms.Add("email");
        }
        if (fbCanAccessFriends)
        {
            fbPerms.Add("user_friends");
        }
        if (IsClicked)
        {
            FB.LogInWithReadPermissions(fbPerms, OnLoggedIn);
        }
        else
        {
            if (AutoLoginEnabled)
            {
                FB.LogInWithReadPermissions(fbPerms, OnLoggedIn);
            }
        }
    }

    private void OnLoggedIn(ILoginResult _LoginResponse)
    {
        if (_LoginResponse.Error != null)
        {
            FacebookLog("Failed To LOG IN", false, true, false);
            Debug.Log(_LoginResponse.Error);
        }
        else
        {
            AmILoggedIn = FB.IsLoggedIn;
            if (AmILoggedIn)
            {
                FacebookLog("Logged In", true, false, false);
                var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
                myFbId = aToken.UserId;
                PERM_LENGTH = 0;
                foreach (string perm in aToken.Permissions)
                {
                    PERM_LENGTH++;
                }
                PERM_NO = 0;
                EXTRA_PERMS = new string[PERM_LENGTH];
                foreach (string perm in aToken.Permissions)
                {
                    ScanFBPerm(perm);
                }

                RequestUserInfo();
            }
            else
            {
                FacebookLog("User Cancelled", false, true, false);
            }
        }
    }

    public void RequestUserInfo()
    {
        List<string> myPerms = new List<string>();
        if (_ReqEmail)
        {
            if (PERM_EMAIL)
            {
                myPerms.Add("email");
            }
            else
            {
                FacebookLog("EMAIL : Permission Required", false, true, false);
            }
        }
        if (_ReqName)
        {
            if (PERM_PUBLICPROFILE)
            {
                myPerms.Add("name");
                myPerms.Add("first_name");
                myPerms.Add("last_name");
            }
            else
            {
                FacebookLog("PUBLIC PROFILE : Permission Required", false, true, false);
            }
        }
        string myPermsStr = string.Join(",", myPerms.ToArray());
        Debug.Log(myPermsStr);
        FB.API("/me?fields=" + myPermsStr, HttpMethod.GET, OnUserInfoGrabbed);
    }

    private void OnUserInfoGrabbed(IResult _FbUserResp)
    {
        if (_FbUserResp.Error == null)
        {
            Debug.Log(_FbUserResp.RawResult);
            if (_ReqName)
            {
                myName = _FbUserResp.ResultDictionary["name"].ToString();
                myFirstName = _FbUserResp.ResultDictionary["first_name"].ToString();
                myLastName = _FbUserResp.ResultDictionary["last_name"].ToString();
            }
            if (_ReqEmail)
            {
                myEmail = _FbUserResp.ResultDictionary["email"].ToString();
            }
            StartCoroutine(ServerManager.Instance.LogIn(myFbId, myName, myEmail));
        }
        else
        {
            Debug.Log(_FbUserResp.Error);
        }
    }

    public void Share()
    {
        Uri gameUrl = new Uri("https://xxx/");
        Uri photoUrl = new Uri("https://xxx.jpg");
        ShareLink(gameUrl, "X Game", "X Description", photoUrl);
    }


    public void ShareLink(Uri gameUrl, string gameName, string contDes, Uri photoUrl)
    {
        if (AmILoggedIn)
        {
            FB.ShareLink(gameUrl, gameName, contDes, photoUrl, ShareLinkCallBack);
        }
        else
        {
            FacebookLog("LOGIN Required For LINK Sharing", false, true, false);
        }
    }

    private void ShareLinkCallBack(IResult _FBShareResp)
    {
        if (_FBShareResp.Error == null)
        {
            FacebookLog("Link Shared", true, false, false);
        }
    }

    // ---------------------

    public void ReqInvitableFriends(int _Limit)
    {
        if (PERM_USERFRIENDS)
        {
            Debug.Log("REQUESTING.....");
            FB.API("/me/invitable_friends?limit=" + _Limit, HttpMethod.GET, OnInvitableFriends);
        }
        else
        {
            FacebookLog("USER FIRENDS : Permission Required", false, true, false);
        }

    }

    private void OnInvitableFriends(IResult _FBFriend)
    {
        Debug.Log(_FBFriend.RawResult);
    }

    public void SendAppRequest(string _MESSAGE)
    {
        if (AmILoggedIn)
        {
            FB.AppRequest(
                _MESSAGE,
                null, null, null, null, null, null,
                delegate (IAppRequestResult appReqResp) {
                    Debug.Log(appReqResp.RawResult);
                }
            );
        }
        else
        {
            FacebookLog("LOGIN Required For Sending App Requests", false, true, false);
        }
    }

    private void ScanFBPerm(string _fbperm)
    {
        switch (_fbperm)
        {
            case "public_profile":
                PERM_PUBLICPROFILE = true;
                break;
            case "email":
                PERM_EMAIL = true;
                break;
            case "user_friends":
                PERM_USERFRIENDS = true;
                break;
            default:
                EXTRA_PERMS[PERM_NO] = _fbperm;
                PERM_NO++;
                break;
        }
    }

    public void FacebookLog(string _msg, bool IsLog, bool IsError, bool IsWarning)
    {
        if (IsLog)
        {
            Debug.Log("<facebook> " + _msg);

        }
        else if (IsError)
        {
            Debug.LogError("<facebook> " + _msg);
        }
        else
        { 
            Debug.LogWarning("<facebook> " + _msg);
        }
    }

}