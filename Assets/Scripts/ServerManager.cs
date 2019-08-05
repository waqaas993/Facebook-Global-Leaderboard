using UnityEngine;
using System.Collections;
using SimpleJSON;
using UnityEngine.UI;

public class ServerManager : MonoBehaviour
{
    public bool myStatus = false;

    public JSONNode logInData;

    public bool ServerLoggedIn = false;

    private static ServerManager instance;
    public static ServerManager Instance
    {
        get { return instance; }
    }
    void Awake()
    {
        instance = this;
    }

    public IEnumerator LogIn(string fbId, string name, string email)
    {
        ServerLog("Logging.....");
        string logInString = "http://yourhostaddress.com/GameServices.php?action=login&gameId=" + Application.identifier + "&fbId=" + fbId + "&name=" + name + "&email=" + email;

        WWW logInCall = new WWW(logInString);
        yield return logInCall;
        //---- ON LOGIN
        ServerLog("Logged In");

        logInData = JSON.Parse(logInCall.text);

        //Error
        if (logInCall.error != null)
        {
            ServerLoggedIn = false;
        }
        //Success
        else
        {
            Debug.Log("You've a Score of: " + int.Parse(logInData["score"]));
            ServerLoggedIn = true;
        }
    }

    public IEnumerator UpdateToSever(string fbId, int score)
    {
        ServerLog("Updating to Server .....");
        Debug.Log(fbId);
        Debug.Log(score);
        string updateString = "http://yourhostaddress.com/GameServices.php/GameServices.php?action=updateScore&fbId=" + fbId + "&gameId=" + Application.identifier + "&Score=" + score.ToString();
        WWW updateCall = new WWW(updateString);

        yield return updateCall;

        ServerLog("Progress Updated on Server");
        ServerLog(score.ToString() + " Has been posted to Server!");
    }

    public void ServerLog(string msg)
    {
        Debug.Log("<Server> " + msg);
    }
}