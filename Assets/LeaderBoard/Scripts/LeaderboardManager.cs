using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class LeaderboardEntry
{
    public string teamName;
    public string activityTime;

    public LeaderboardEntry(string teamName, string activityTime)
    {
        this.teamName = teamName;
        this.activityTime = activityTime;

    }
}


[System.Serializable]
public class Leaderboard
{
    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

}

public class LeaderboardManager : MonoBehaviour
{
    public GameObject leaderboardScreen;
    public Transform entryContainer;
    public Transform entryTemplate;

    public Sprite firstRankImage;
    public Sprite secondRankImage;
    public Sprite thirdRankImage;
    public Sprite defaultRankImage;

    public Sprite firstRankMedal;
    public Sprite secondRankMedal;
    public Sprite thirdRankMedal;

    private List<Transform> highscoreEntryTransformList;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ClearLeaderboard();
        }
    }
    public void ClearLeaderboard()
    {
        // Create a new empty leaderboard.
        Leaderboard emptyLeaderboard = new Leaderboard();

        // Serialize the empty leaderboard to JSON and save it to PlayerPrefs.
        string emptyJson = JsonUtility.ToJson(emptyLeaderboard);
        PlayerPrefs.SetString("Leaderboard", emptyJson);
        PlayerPrefs.Save();

        Debug.Log("Leaderboard cleared.");

        // Refresh the leaderboard display.
        ShowLeaderboard();
    }
    public void ShowLeaderboard()
    {
        //leaderboardScreen.SetActive(true);

        // Load leaderboard data from PlayerPrefs or initialize if not found
        string jsonString = PlayerPrefs.GetString("Leaderboard", "{}");
        Leaderboard leaderboard = JsonUtility.FromJson<Leaderboard>(jsonString);

        if (leaderboard == null || leaderboard.entries.Count == 0)
        {
            Debug.Log("No leaderboard data found. Initializing with default values.");
            // InitializeDefaultLeaderboard(); // You can implement this method if needed
            jsonString = PlayerPrefs.GetString("Leaderboard", "{}");
            leaderboard = JsonUtility.FromJson<Leaderboard>(jsonString);
        }

        // Sort the leaderboard entries by activity time in ascending order
        leaderboard.entries.Sort((x, y) => x.activityTime.CompareTo(y.activityTime));

        highscoreEntryTransformList = new List<Transform>();

        // Display up to 10 leaderboard entries
        for (int i = 0; i < leaderboard.entries.Count && i < 10; i++)
        {
            LeaderboardEntry entry = leaderboard.entries[i];
            CreateHighscoreEntryTransform(entry, entryContainer, highscoreEntryTransformList, i + 1);
        }
    }

    private void CreateHighscoreEntryTransform(LeaderboardEntry entry, Transform container, List<Transform> transformList, int rank)
    {
        if (entryTemplate == null || container == null || transformList == null)
        {
            Debug.LogError("Missing references in CreateHighscoreEntryTransform");
            return;
        }

        float templateHeight = 110f; // Adjust the height as per your UI design
        Transform entryTransform = Instantiate(entryTemplate, container);

        if (entryTransform == null)
        {
            Debug.LogError("Failed to instantiate entry template");
            return;
        }

        RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * transformList.Count);
        entryTransform.gameObject.SetActive(true);

        // Set UI elements if found
        //TMP_Text rankText = entryTransform.Find("rankText")?.GetComponent<TMP_Text>();
        //if (rankText != null)
        //{
        //    rankText.text = $"{rank}{GetOrdinalSuffix(rank)}";
        //}
        // Handle rank display
        TMP_Text rankText = entryTransform.Find("rankText")?.GetComponent<TMP_Text>();
        Image rankImage = entryTransform.Find("rankText/RankImage")?.GetComponent<Image>(); // Get child image
        if (rankText != null && rankImage != null)
        {
            if (rank <= 3)
            {
                // Show medal for top 3 ranks
                //rankText.gameObject.SetActive(false);
                rankImage.gameObject.SetActive(true);

                switch (rank)
                {
                    case 1:
                        rankImage.sprite = firstRankMedal; // Set the first medal image
                        break;
                    case 2:
                        rankImage.sprite = secondRankMedal; // Set the second medal image
                        break;
                    case 3:
                        rankImage.sprite = thirdRankMedal; // Set the third medal image
                        break;
                }
            }
            else
            {
                // Show text for other ranks
                rankText.gameObject.SetActive(true);
                rankImage.gameObject.SetActive(false);
                rankText.text = $"{rank}{GetOrdinalSuffix(rank)}";
            }
        }

        TMP_Text teamText = entryTransform.Find("teamText")?.GetComponent<TMP_Text>();
        if (teamText != null)
        {
            teamText.text = entry.teamName;
        }

        TMP_Text activityTimeText = entryTransform.Find("activityTimeText")?.GetComponent<TMP_Text>();
        if (activityTimeText != null)
        {
            //activityTimeText.text = entry.activityTime.ToString("F2");
            activityTimeText.text = entry.activityTime;
        }

        Image backgroundPanel = entryTransform.Find("Panel")?.GetComponent<Image>();
        if (backgroundPanel != null)
        {
            switch (rank)
            {
                case 1:
                    backgroundPanel.sprite = firstRankImage;
                    break;
                case 2:
                    backgroundPanel.sprite = secondRankImage;
                    break;
                case 3:
                    backgroundPanel.sprite = thirdRankImage;
                    break;
                default:
                    backgroundPanel.sprite = defaultRankImage;
                    break;
            }

            backgroundPanel.color = (rank % 2 == 1) ? Color.white : new Color(0.9f, 0.9f, 0.9f);
        }

        transformList.Add(entryTransform);
    }


    private string GetOrdinalSuffix(int number)
    {
        if (number <= 0) return "";

        switch (number % 100)
        {
            case 11:
            case 12:
            case 13:
                return "th";
            default:
                switch (number % 10)
                {
                    case 1: return "st";
                    case 2: return "nd";
                    case 3: return "rd";
                    default: return "th";
                }
        }
    }
}

//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using System.Collections.Generic;
//using System;

//[System.Serializable]
//public class LeaderboardEntry
//{
//    public Dictionary<string, object> customData;

//    public LeaderboardEntry(Dictionary<string, object> customData)
//    {
//        this.customData = customData;
//    }

//    // Optionally, you can add methods to make it easier to access specific fields
//    public object GetCustomData(string key)
//    {
//        if (customData.ContainsKey(key))
//        {
//            return customData[key];
//        }
//        return null; // Or handle as needed
//    }
//}

//[System.Serializable]
//public class Leaderboard
//{
//    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

//}

//public class LeaderboardManager : MonoBehaviour
//{
//    public GameObject leaderboardScreen;
//    public Transform entryContainer;
//    public Transform entryTemplate;

//    public Sprite firstRankImage;
//    public Sprite secondRankImage;
//    public Sprite thirdRankImage;
//    public Sprite defaultRankImage;

//    public Sprite firstRankMedal;
//    public Sprite secondRankMedal;
//    public Sprite thirdRankMedal;

//    private List<Transform> highscoreEntryTransformList;

//    public void ShowLeaderboard()
//    {
//        // Load leaderboard data from PlayerPrefs or initialize if not found
//        string jsonString = PlayerPrefs.GetString("Leaderboard", "{}");
//        Leaderboard leaderboard = JsonUtility.FromJson<Leaderboard>(jsonString);

//        if (leaderboard == null || leaderboard.entries.Count == 0)
//        {
//            Debug.Log("No leaderboard data found. Initializing with default values.");
//            // InitializeDefaultLeaderboard(); // You can implement this method if needed
//            jsonString = PlayerPrefs.GetString("Leaderboard", "{}");
//            leaderboard = JsonUtility.FromJson<Leaderboard>(jsonString);
//        }

//        // Sort the leaderboard entries by activity time in ascending order
//        leaderboard.entries.Sort((x, y) =>
//        {
//            string xTime = x.GetCustomData("activityTime")?.ToString();
//            string yTime = y.GetCustomData("activityTime")?.ToString();
//            return string.Compare(xTime, yTime); // Adjust comparison logic as needed
//        });

//        highscoreEntryTransformList = new List<Transform>();

//        // Display up to 10 leaderboard entries
//        for (int i = 0; i < leaderboard.entries.Count && i < 10; i++)
//        {
//            LeaderboardEntry entry = leaderboard.entries[i];
//            CreateHighscoreEntryTransform(entry, entryContainer, highscoreEntryTransformList, i + 1);
//        }
//    }

//    private void CreateHighscoreEntryTransform(LeaderboardEntry entry, Transform container, List<Transform> transformList, int rank)
//    {
//        if (entryTemplate == null || container == null || transformList == null)
//        {
//            Debug.LogError("Missing references in CreateHighscoreEntryTransform");
//            return;
//        }

//        float templateHeight = 110f; // Adjust the height as per your UI design
//        Transform entryTransform = Instantiate(entryTemplate, container);

//        if (entryTransform == null)
//        {
//            Debug.LogError("Failed to instantiate entry template");
//            return;
//        }

//        RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
//        entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * transformList.Count);
//        entryTransform.gameObject.SetActive(true);

//        // Set UI elements if found
//        //TMP_Text rankText = entryTransform.Find("rankText")?.GetComponent<TMP_Text>();
//        //if (rankText != null)
//        //{
//        //    rankText.text = $"{rank}{GetOrdinalSuffix(rank)}";
//        //}
//        // Handle rank display
//        TMP_Text rankText = entryTransform.Find("rankText")?.GetComponent<TMP_Text>();
//        Image rankImage = entryTransform.Find("rankText/RankImage")?.GetComponent<Image>(); // Get child image
//        if (rankText != null && rankImage != null)
//        {
//            if (rank <= 3)
//            {
//                // Show medal for top 3 ranks
//                //rankText.gameObject.SetActive(false);
//                rankImage.gameObject.SetActive(true);

//                switch (rank)
//                {
//                    case 1:
//                        rankImage.sprite = firstRankMedal; // Set the first medal image
//                        break;
//                    case 2:
//                        rankImage.sprite = secondRankMedal; // Set the second medal image
//                        break;
//                    case 3:
//                        rankImage.sprite = thirdRankMedal; // Set the third medal image
//                        break;
//                }
//            }
//            else
//            {
//                // Show text for other ranks
//                rankText.gameObject.SetActive(true);
//                rankImage.gameObject.SetActive(false);
//                rankText.text = $"{rank}{GetOrdinalSuffix(rank)}";
//            }
//        }

//        TMP_Text teamText = entryTransform.Find("teamText")?.GetComponent<TMP_Text>();
//        if (teamText != null)
//        {
//            // Set the team name text
//            teamText.text = entry.GetCustomData("teamName")?.ToString() ?? "Unknown";
//        }

//        TMP_Text activityTimeText = entryTransform.Find("activityTimeText")?.GetComponent<TMP_Text>();
//        if (activityTimeText != null)
//        {
//            // Extract activity time from the entry's custom data (assuming activityTime is stored in "activityTime")
//            float activityTime = entry.GetCustomData("activityTime") != null ? Convert.ToSingle(entry.GetCustomData("activityTime")) : 0f;

//            // Format the activity time as MM:SS or similar (you can adjust this to fit your format)
//            int minutes = Mathf.FloorToInt(activityTime / 60F);
//            int seconds = Mathf.FloorToInt(activityTime % 60F);
//            string formattedActivityTime = $"{minutes:00}:{seconds:00}";

//            // Set the activity time text
//            activityTimeText.text = formattedActivityTime;
//        }

//        Image backgroundPanel = entryTransform.Find("Panel")?.GetComponent<Image>();
//        if (backgroundPanel != null)
//        {
//            switch (rank)
//            {
//                case 1:
//                    backgroundPanel.sprite = firstRankImage;
//                    break;
//                case 2:
//                    backgroundPanel.sprite = secondRankImage;
//                    break;
//                case 3:
//                    backgroundPanel.sprite = thirdRankImage;
//                    break;
//                default:
//                    backgroundPanel.sprite = defaultRankImage;
//                    break;
//            }

//            backgroundPanel.color = (rank % 2 == 1) ? Color.white : new Color(0.9f, 0.9f, 0.9f);
//        }

//        transformList.Add(entryTransform);
//    }


//    private string GetOrdinalSuffix(int number)
//    {
//        if (number <= 0) return "";

//        switch (number % 100)
//        {
//            case 11:
//            case 12:
//            case 13:
//                return "th";
//            default:
//                switch (number % 10)
//                {
//                    case 1: return "st";
//                    case 2: return "nd";
//                    case 3: return "rd";
//                    default: return "th";
//                }
//        }
//    }
//}