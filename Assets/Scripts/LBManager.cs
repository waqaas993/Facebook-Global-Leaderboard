using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SimpleJSON;

public class LBManager : MonoBehaviour
{
    public GameObject LBScreen;
    public GameObject lbUserPrefab;
    //	public Text lbPageText;

    private float lbPosY;

    private JSONNode lbData;

    private static LBManager instance;
    public static LBManager Instance
    {
        get { return instance; }
    }

    void OnEnable()
    {
        instance = this;
        StartCoroutine(FetchLeaderBoard());
    }


    public IEnumerator FetchLeaderBoard()
    {
        WWW lbService = new WWW("http://yourhostaddress.com/GameServices.php/GameServices.php?action=getScore&gameId=" + Application.identifier + "&limit=" + (100).ToString());
        yield return lbService;
        Debug.Log(lbService.text);
        lbData = JSON.Parse(lbService.text);
        Debug.Log(lbData[0]["name"]);
        ConstructLBPage();
    }

    public void DestroyLB()
    {
        GameObject[] lbContent = GameObject.FindGameObjectsWithTag("lbUser");
        for (int userNo = 0; userNo < lbContent.Length; userNo++)
        {
            Destroy(lbContent[userNo]);
        }
    }

    public void ConstructLBPage()
    {
        lbPosY = 0;

        for (int userno = 0; userno < lbData.Count-1; userno++)
        {
            GameObject lbuser = Instantiate(lbUserPrefab,Vector3.zero, Quaternion.Euler(new Vector3(0f, 0f, 0f))) as GameObject;
            lbuser.transform.SetParent(LBScreen.transform);

            lbuser.transform.localScale = new Vector3(1, 1, 1f);
            lbuser.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, lbPosY, 0f);

            Text username = lbuser.transform.Find("name").GetComponent<Text>();
            username.text = lbData[userno]["name"];

            Text userscore = lbuser.transform.Find("score").GetComponent<Text>();
            userscore.text = lbData[userno]["score"];

            Image userimg = lbuser.transform.Find("pic").GetComponent<Image>();

            StartCoroutine(LoadFBPicture((lbData[userno]["id"]), userimg));

            lbPosY = lbPosY - 120f;
        }
    }

    public IEnumerator LoadFBPicture(string fbId, Image userImg)
    {
        WWW downloadDp = new WWW("https://graph.facebook.com/" + fbId + "/picture?width=90&height=90");
        yield return downloadDp;
        Texture2D texture = new Texture2D(90, 90, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.LoadImage(downloadDp.bytes);
        if (userImg != null)
        {
            userImg.sprite = Sprite.Create(texture, new Rect(0, 0, 90, 90), new Vector2(0.5f, 0.5f), 100.0f);
        }
    }

}