using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using TMPro;

public class PlayFabManager : MonoBehaviour
{
    public GameObject rowPrefab;
    public Transform rowsParent;
    public TMP_InputField nameInputField;

    // Start is called before the first frame update
    void Start()
    {
        Login();
    }

    void Login()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
             }
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnSuccess, OnError);
        
    }

    void OnSuccess(LoginResult result)
    {
        Debug.Log("Successful login/account create!");
        string name = null;
        if(result.InfoResultPayload.PlayerProfile != null)
        name = result.InfoResultPayload.PlayerProfile.DisplayName;

        if (name == null)
        {
            nameInputField.gameObject.SetActive(true);
        }
        else
        {
            nameInputField.gameObject.SetActive(false);

        }
    }

    void OnError(PlayFabError error)
    {
        Debug.Log("Error while logging in/creating account!");
        Debug.Log(error.GenerateErrorReport());
    }

    /// <summary>
    /// Use this method to send the score from GameManager to PlayFab leaderboard
    /// </summary>
    /// <param name="score"></param>
    public void SendLeaderBoard(int score)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            //Statistics == Leaderboards
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "Anoroc Score",
                    Value = score
                }
            }

        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderBoardUpdate, OnError);
    }

    void OnLeaderBoardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Successfull leaderboard sent");
    }

    public void GetLeaderBoard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = "Anoroc Score",
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnError);
    }

    /// <summary>
    /// Get Leaderboard from the web and display it to the player
    /// </summary>
    /// <param name="result"></param>
    void OnLeaderboardGet(GetLeaderboardResult result)
    {
        

        foreach(Transform item in rowsParent)
        {
            Destroy(item.gameObject);
        }

        foreach(var item in result.Leaderboard)
        {
            
            
            GameObject newGo = Instantiate(rowPrefab, rowsParent);
            TextMeshProUGUI[] texts = newGo.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = item.Position+1.ToString();
            
            //name
            texts[1].text = item.DisplayName;
            
            texts[2].text = item.StatValue.ToString();
            //newGo.transform.GetChild(0).GetComponent<Text>().text = item.Position + 1.ToString();
            //newGo.transform.GetChild(1).GetComponent<Text>().text = item.PlayFabId;
            //newGo.transform.GetChild(2).GetComponent<Text>().text = item.StatValue.ToString();
            Debug.Log(string.Format("Place: {0} | ID: {1} | VALUE: {2}",item.Position, item.PlayFabId, item.StatValue));
        }

    }

    public void SubmitNameButton()
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = nameInputField.text,
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameUpdate, OnError);
    }

    void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("Updated display name!");
        nameInputField.gameObject.SetActive(false);
    }
}
