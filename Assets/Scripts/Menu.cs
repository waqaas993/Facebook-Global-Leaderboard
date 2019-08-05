using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Menu : MonoBehaviour {

    public GameObject MainMenu;
    public GameObject Leaderboard;

    public void leaderboard() {
        MainMenu.SetActive(false);
        Leaderboard.SetActive(true);
    }

    public void mainMenu()
    {
        MainMenu.SetActive(true);
        LBManager.Instance.DestroyLB();
        Leaderboard.SetActive(false);
    }

    public void UpdateMyScore() {
        StartCoroutine(ServerManager.Instance.UpdateToSever(FacebookManager.Instance.myFbId, Random.Range(0, 20)));
    }

}