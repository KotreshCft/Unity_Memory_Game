using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.IO;
using System.Reflection;
using System;
public class FlipkartManager : MonoBehaviour
{
    public static FlipkartManager Instance { get; private set; }

    [SerializeField] public List<Sprite> imageArray = new List<Sprite>(); // brand images are here
    [SerializeField] public List<Sprite> logoArray = new List<Sprite>(); // brand logo with description are here

    //[SerializeField] public List<string> logoDescription = new List<string>();

    [SerializeField] private Button leaderboardButton; // to start the game
    [SerializeField] private Button backButton; // to start the game
    [SerializeField] private Button restartButton; // to start the game

    [SerializeField] private GameObject firstScreen; // next button screen
    [SerializeField] private GameObject videoScreen; // let'go and congratulation video screen
    [SerializeField] private GameObject mainScreen; // main game screen
    [SerializeField] private GameObject resultScreen; // better luck next time message screen
    [SerializeField] private GameObject leaderboardScreen; // better luck next time message screen

    [SerializeField] private GameObject GridGameObject;

    [SerializeField] private VideoPlayer videoPlayer; //  to play videos

    private List<GameObject> children = new List<GameObject>();
    private Dictionary<GameObject, bool> isRotating = new Dictionary<GameObject, bool>();

    public Sprite[] BaseImage; //1. for imageArray _Brand LOCK; 2. logoArray _Brand Description KEY
    public Coroutine gameTimerCoRoutine;
    public BoxAssigner boxAssigner;

    public TMP_Text headerText;
    //public TMP_Text attemptsText;
    public TMP_Text gameTimerText;

    // just un comment the line if you wnt to set from inspectr

    //public GameObject AttemptGameObject;
    //public int attempts = 4;
    //public float gameDuration = 45f;
    //public float previewCountdown = 10f;

    //game setting by using prefs
    private float attempts = 4;
    private float gameDuration = 45f;
    private float previewCountdown = 10f;

    private List<int> selectedIndices = new List<int>();
    private List<Sprite> shuffledImages = new List<Sprite>();

    private int validationCounter = 2;
    private List<int> validatingIndices = new List<int>();
    private List<GameObject> validatingObjects = new List<GameObject>();
    private bool itemsClickable = false;
    public RenderTexture rt;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        leaderboardButton.onClick.AddListener(ShowLeaderScreen);
        backButton.onClick.AddListener(BackToMainScreen);
        restartButton.onClick.AddListener(RestartGame);
        firstScreen.SetActive(true);
        mainScreen.SetActive(false);
        resultScreen.SetActive(false);
        restartButton.gameObject.SetActive(false);
        leaderboardScreen.SetActive(false);

        previewCountdown = PlayerPrefs.GetFloat("initialTiming", 30f);
        gameDuration = PlayerPrefs.GetFloat("matchingTiming", 15f);
        attempts = PlayerPrefs.GetFloat("attempts", 3f);
       
    }
    public float popDuration = 0.3f; // Duration for the pop animation
    public Vector3 popScale = new Vector3(1.2f, 1.2f, 1); // Scale factor for the pop effect
    public Vector3 normalScale = Vector3.one; // Original scale of the button
    public LeaderboardManager leaderboardManager;
    void ShowLeaderScreen()
    {
        StartCoroutine(ButtonPopAnimation(leaderboardButton));
        StartCoroutine(AfterAnimationShowLeaderboard());
    }
    private IEnumerator AfterAnimationShowLeaderboard()
    {
        yield return new WaitForSeconds(popDuration * 2);

        leaderboardScreen.SetActive(true);
        firstScreen.SetActive(false);
        leaderboardManager.ShowLeaderboard();
    }
    void BackToMainScreen()
    {
        StartCoroutine(ButtonPopAnimation(backButton));
        StartCoroutine(AfterAnimationBackGame());
    }
    private IEnumerator AfterAnimationBackGame()
    {
        yield return new WaitForSeconds(popDuration * 2);

        leaderboardScreen.SetActive(false);
        firstScreen.SetActive(true);
    }
    public void UpdateLeaderboard(string teamName, float activityTime)
    {
        // Format the activity time as MM:SS.mmm (minutes, seconds, milliseconds)
        int minutes = Mathf.FloorToInt(activityTime / 60F);
        int seconds = Mathf.FloorToInt(activityTime % 60F);
        //int milliseconds = Mathf.FloorToInt((activityTime * 1000F) % 1000F);
        string formattedActivityTime = $"{seconds:00} sec";

        LeaderboardEntry newEntry = new LeaderboardEntry(teamName, formattedActivityTime);

        // Load existing leaderboard data
        string jsonString = PlayerPrefs.GetString("Leaderboard", "{}");
        Leaderboard leaderboard = JsonUtility.FromJson<Leaderboard>(jsonString);

        if (leaderboard == null)
            leaderboard = new Leaderboard();

        // Add the new entry
        leaderboard.entries.Add(newEntry);

        // Save updated leaderboard data
        string updatedJsonString = JsonUtility.ToJson(leaderboard);
        PlayerPrefs.SetString("Leaderboard", updatedJsonString);
        PlayerPrefs.Save();

        // Show the updated leaderboard
        leaderboardManager.ShowLeaderboard();
    }

    //[System.Serializable]
    //public class PlayerData
    //{
    //    public string playerName;
    //    public float totalGameTime;

    //    // Constructor
    //    public PlayerData(string playerName, float totalGameTime)
    //    {
    //        this.playerName = playerName;
    //        this.totalGameTime = totalGameTime;
    //    }
    //}
    //public void UpdateLeaderboard(object entryData)
    //{
    //    // Create a dictionary to hold custom data from the object
    //    Dictionary<string, object> customData = new Dictionary<string, object>();

    //    // Use reflection to extract properties from the object
    //    PropertyInfo[] properties = entryData.GetType().GetProperties();

    //    foreach (var property in properties)
    //    {
    //        customData[property.Name] = property.GetValue(entryData);
    //    }

    //    // Extract specific fields, here assuming teamName and activityTime
    //    string teamName = customData.ContainsKey("teamName") ? customData["teamName"].ToString() : "Unknown";
    //    float activityTime = customData.ContainsKey("activityTime") ? Convert.ToSingle(customData["activityTime"]) : 0f;

    //    // Format the activity time as MM:SS.mmm (minutes, seconds, milliseconds)
    //    int minutes = Mathf.FloorToInt(activityTime / 60F);
    //    int seconds = Mathf.FloorToInt(activityTime % 60F);
    //    string formattedActivityTime = $"{seconds:00} sec";

    //    // Create a new leaderboard entry
    //    LeaderboardEntry newEntry = new LeaderboardEntry(customData);

    //    // Load existing leaderboard data
    //    string jsonString = PlayerPrefs.GetString("Leaderboard", "{}");
    //    Leaderboard leaderboard = JsonUtility.FromJson<Leaderboard>(jsonString);

    //    if (leaderboard == null)
    //        leaderboard = new Leaderboard();

    //    // Add the new entry
    //    leaderboard.entries.Add(newEntry);

    //    // Save updated leaderboard data
    //    string updatedJsonString = JsonUtility.ToJson(leaderboard);
    //    PlayerPrefs.SetString("Leaderboard", updatedJsonString);
    //    PlayerPrefs.Save();

    //    // Show the updated leaderboard
    //    leaderboardManager.ShowLeaderboard();
    //}
    public IEnumerator ButtonPopAnimation(Button button)
    {
        // Scale up the button to create the pop effect
        float elapsedTime = 0f;
        while (elapsedTime < popDuration)
        {
            button.transform.localScale = Vector3.Lerp(normalScale, popScale, elapsedTime / popDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the button ends at the pop scale
        button.transform.localScale = popScale;

        // Scale the button back to normal
        elapsedTime = 0f;
        while (elapsedTime < popDuration)
        {
            button.transform.localScale = Vector3.Lerp(popScale, normalScale, elapsedTime / popDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure it ends at the normal scale
        button.transform.localScale = normalScale;
    }
    public void StartNewGame()
    {
        firstScreen.SetActive(false);
        videoScreen.SetActive(false);
        mainScreen.SetActive(true);
        InitializeContent();
    }
    public RawImage ri;  // The RawImage that displays the video
    private void ResetVideoPlayer()
    {
        // Stop the current video (if any is playing)
        videoPlayer.Stop();

        // Reset to the first frame
        videoPlayer.frame = 0;

        // Clear the current target texture by creating a new empty RenderTexture
        if (videoPlayer.targetTexture != null)
        {
            // Create a new RenderTexture (same size as current target texture)
            RenderTexture emptyTexture = new RenderTexture(ri.texture.width, ri.texture.height, 24);

            // Clear the texture by applying it to the RawImage (this will clear the previous content)
            ri.texture = emptyTexture;

            // Set the video player target texture to the new empty texture
            videoPlayer.targetTexture = emptyTexture;
        }

        // Prepare the video player with the new texture
        videoPlayer.Prepare();

        // Set the video player to play the next video
        videoPlayer.Play();
    }
    private float gameStartTime;
    void InitializeContent()
    {
        gameStartTime = Time.time;
        boxAssigner = new BoxAssigner();

        // Select 6 pairs from both imageArray and logoArray
        selectedIndices = boxAssigner.AssignRandomIndices(imageArray, logoArray, 8);

        // Combine images from imageArray and logoArray based on selected indices
        shuffledImages.Clear();
        foreach (int index in selectedIndices)
        {
            shuffledImages.Add(imageArray[index]);
            shuffledImages.Add(logoArray[index]);
        }

        // Shuffle the combined list
        shuffledImages = boxAssigner.ShuffleList(shuffledImages);

        // Log all correct pairs
        Debug.Log("Correct Pairs:");
        foreach (int index in selectedIndices)
        {
            Debug.Log($"Pair: {imageArray[index].name} - {logoArray[index].name}");
        }

        // Assign shuffled images to grid children
        children = GetChildrenList(GridGameObject);

        for (int i = 0; i < children.Count; i++)
        {
            children[i].GetComponent<Image>().sprite = shuffledImages[i];
            isRotating[children[i]] = false;
        }

        StartCoroutine(AssignBaseImage(previewCountdown));
    }
    public string headerMessage1 = "HURRY!<br>Memorize the cards before they flip away.";
    public string headerMessage2 = "Find the pairs that make a perfect match";
    IEnumerator AssignBaseImage(float time)
    {
        headerText.text = headerMessage1;
        float remainingTime = time;
        while (remainingTime > 0)
        {
            // Update the countdown text with the remaining time, formatted as a whole number
            gameTimerText.text = Mathf.Ceil(remainingTime).ToString();
            yield return new WaitForSeconds(1);
            remainingTime--;
        }
        itemsClickable = true;
        for (int i = 0; i < children.Count; i++)
        {
            // Determine the appropriate base image based on the type of shuffledImages
            Sprite baseImage = GetBaseImageForIndex(i);
            StartCoroutine(RotateCard(children[i], baseImage));
        }

        gameTimerText.text = "GO!";
        headerText.text = headerMessage2;

        gameTimerCoRoutine = StartCoroutine(GameTimer(gameDuration));
    }
    private Sprite GetBaseImageForIndex(int index)
    {
        if (IsLogoArray(shuffledImages[index]))
        {
            return BaseImage[1]; // Base image for logoArray (_Brand Description KEY)
        }
        else
        {
            return BaseImage[0]; // Base image for imageArray (_Brand LOCK)
        }
    }
    private bool IsLogoArray(Sprite sprite)
    {
        return logoArray.Contains(sprite); // Assuming logoArray is a list of sprites
    }

    private IEnumerator ValidationCheck()
    {
        yield return new WaitForSeconds(0.3f);

        if (isGameOver) yield break; // Stop if the game is already over

        int firstIndex = validatingIndices[0];
        int secondIndex = validatingIndices[1];

        if (IsValidPair(shuffledImages[firstIndex], shuffledImages[secondIndex]))
        {
            Debug.LogWarning($"Correct pair selected: {shuffledImages[firstIndex].name} and {shuffledImages[secondIndex].name}");
            ShowMatchedImage(validatingObjects[0], shuffledImages[firstIndex]);
            ShowMatchedImage(validatingObjects[1], shuffledImages[secondIndex]);

            if (CheckIfAllImagesAreDisabled() && !isGameOver) // Ensure no duplicates
            {
                isGameOver = true;
                PlayResultVideo();
            }
        }
        else
        {
            Debug.Log($"Mismatch! {shuffledImages[firstIndex].name} and {shuffledImages[secondIndex].name}");

            // If not valid, reset both images
            yield return StartCoroutine(RotateCard(validatingObjects[0], GetBaseImageForIndex(firstIndex)));
            yield return StartCoroutine(RotateCard(validatingObjects[1], GetBaseImageForIndex(secondIndex)));
        }

        validatingIndices.Clear();
        validatingObjects.Clear();
        validationCounter = 2;
        itemsClickable = true; // Allow interactions again
    }

    public void ShowMatchedImage(GameObject imageObject, Sprite matchedSprite)
    {
        imageObject.GetComponent<Image>().sprite = matchedSprite;
        imageObject.GetComponent<Button>().interactable = false;
    }
    public void gameObjectValidate(int index)
    {
        if (!itemsClickable || isRotating[children[index]]) return;

        Debug.Log($"Tile {index} clicked: {shuffledImages[index].name}");

        validationCounter--;
        validatingIndices.Add(index);
        validatingObjects.Add(children[index]);
        StartCoroutine(RotateCard(children[index], shuffledImages[index]));

        if (validationCounter == 0)
        {
            itemsClickable = false; // Lock interaction during validation
            StartCoroutine(ValidationCheck());
        }
    }
    public TMP_Text winTime;
    private void PlayResultVideo()
    {
        mainScreen.SetActive(false);
        Debug.LogWarning("result video is playing");
        videoScreen.SetActive(true); // Activate video player
        ResetVideoPlayer(); // Reset the video player before playing the new video
        videoPlayer.Play();
        videoPlayer.loopPointReached += OnResultVideoFinished; // Trigger when the video ends
                                                               // Calculate total game time
        float totalGameTime = Time.time - gameStartTime;
        StartCoroutine(AnimateCongratulationText());
        // Get the player's name from InputManager
        string playerName = InputManager.Instance.nameInput.text.Trim();

        // Update leaderboard with player's name and time
         UpdateLeaderboard(playerName, totalGameTime);
       
    }

    IEnumerator AnimateCongratulationText()
    {
        winTime.gameObject.SetActive(true);
        winTime.transform.localScale = Vector3.zero; // Start with no size

        CanvasGroup canvasGroup = winTime.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = winTime.gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0; // Start invisible

        float duration = 1.5f; // Total animation duration
        float elapsedTime = 0f;

        // Animation: Scale up with a slight bounce
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Smooth scale-up with bounce effect
            float scale = Mathf.Lerp(0, 1.2f, elapsedTime / duration); // Overshoot scale
            scale = Mathf.PingPong(elapsedTime * 1.5f, 0.2f) + Mathf.Lerp(0, 1, elapsedTime / duration); // Slight bounce
            winTime.transform.localScale = new Vector3(scale, scale, 1);

            // Fade in
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / (duration * 0.7f)); // Faster fade-in

            yield return null;
        }

        // Ensure final state is correct
        winTime.transform.localScale = Vector3.one; // Normal size
        canvasGroup.alpha = 1f; // Fully visible
    }
    private void OnResultVideoFinished(VideoPlayer vp)
    {
        videoScreen.SetActive(false);
        leaderboardScreen.SetActive(true);
        restartButton.gameObject.SetActive(true);
        StartCoroutine(WaitAndReload());
    }
    private bool IsValidPair(Sprite sprite1, Sprite sprite2)
    {
        int indexInImageArray = imageArray.IndexOf(sprite1);
        int indexInLogoArray = logoArray.IndexOf(sprite2);

        // Check if the pair is valid: one sprite is in imageArray, and the other is in logoArray, with matching indices
        bool isValid = (indexInImageArray != -1 && indexInLogoArray != -1 && indexInImageArray == indexInLogoArray);

        if (!isValid)
        {
            // Reverse check: the first sprite is in logoArray, and the second is in imageArray
            indexInImageArray = imageArray.IndexOf(sprite2);
            indexInLogoArray = logoArray.IndexOf(sprite1);
            isValid = (indexInImageArray != -1 && indexInLogoArray != -1 && indexInImageArray == indexInLogoArray);
        }

        return isValid;
    }


    public void ToggleImageVisibility(GameObject imageComponent, bool isVisible)
    {
        imageComponent.GetComponent<Image>().enabled = isVisible;
    }

    public bool CheckIfAllImagesAreDisabled()
    {
        return children.All(child => !child.GetComponent<Button>().interactable);

    }

    IEnumerator RotateCard(GameObject card, Sprite sprite)
    {
        if (isRotating[card]) yield break;

        isRotating[card] = true;

        // Rotate the card 90 degrees in both directions
        for (float angle = 0f; angle <= 90f; angle += 15f)
        {
            card.transform.rotation = Quaternion.Euler(0, angle, 0);
            yield return new WaitForSeconds(0.01f);
        }

        // If the sprite is not null, assign it
        if (sprite != null)
        {
            card.GetComponent<Image>().sprite = sprite;
        }

        // Rotate the card back to its original state
        for (float angle = 90f; angle >= 0f; angle -= 15f)
        {
            card.transform.rotation = Quaternion.Euler(0, angle, 0);
            yield return new WaitForSeconds(0.01f);
        }

        isRotating[card] = false;
    }

    IEnumerator GameTimer(float time)
    {
        float remainingTime = time;
        while (remainingTime > 0 && !isGameOver) // Stop if the game is over
        {
            gameTimerText.text = Mathf.Ceil(remainingTime).ToString();
            yield return new WaitForSeconds(1);
            remainingTime--;
        }

        if (!isGameOver) // Only trigger GameOverScreen if the game hasn't ended already
        {
            isGameOver = true;
            GameOverScreen();
        }
    }

    private List<GameObject> GetChildrenList(GameObject parent)
    {
        return parent.transform.Cast<Transform>().Select(child => child.gameObject).ToList();
    }
    private bool isGameOver = false; // To track if the game is already over

    public void GameOverScreen()
    {
        resultScreen.SetActive(true);
        StartCoroutine(DisplayImageAndReload(true));
    }
    public float resultScreenDuraion = 3f;
    public float endScreenDuraion = 10f;
    private IEnumerator DisplayImageAndReload(bool byTimer)
    {
        leaderboardManager.ShowLeaderboard();
        leaderboardScreen.SetActive(true);
        restartButton.gameObject.SetActive(true);
        backButton.gameObject.SetActive(false);
        yield return new WaitForSeconds(resultScreenDuraion);
        StartCoroutine(WaitAndReload());
    }
    private IEnumerator WaitAndReload()
    {
        yield return new WaitForSeconds(endScreenDuraion);
        RestartGame();
    }
    void RestartGame()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
