using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using Unity.Collections.LowLevel.Unsafe;

public class GameManager : MonoBehaviour
{
    [SerializeField] public List<Sprite> imageArray = new List<Sprite>(); // brand images are here
    [SerializeField] public List<Sprite> logoArray = new List<Sprite>(); // brand logo with description are here

    //[SerializeField] public List<string> logoDescription = new List<string>();

    [SerializeField] private Button nextButton; // to transiste to start screen
    [SerializeField] private Button startButton; // to start the game

    [SerializeField] private GameObject firstScreen; // next button screen
    [SerializeField] private GameObject secondScreen; // start button screen
    [SerializeField] private GameObject videoScreen; // let'go and congratulation video screen
    [SerializeField] private GameObject mainScreen; // main game screen
    [SerializeField] private GameObject resultScreen; // better luck next time message screen
    [SerializeField] private GameObject lastScreen; //  5 sec inforamtion screen // madntory after result
    
    [SerializeField] private GameObject GridGameObject;

    [SerializeField] private VideoPlayer videoPlayer; //  to play videos

    [SerializeField] private VideoClip[] clips; //  1. Information, 2. Congratulation

    private List<GameObject> children = new List<GameObject>();
    private Dictionary<GameObject, bool> isRotating = new Dictionary<GameObject, bool>();

    public Sprite[] BaseImage; //1. for imageArray _Brand LOCK; 2. logoArray _Brand Description KEY
    public Coroutine gameTimerCoRoutine;
    public BoxAssigner boxAssigner;
    public Image textimaghe;

    public TMP_Text headerText;
    public TMP_Text attemptsText;
    public TMP_Text gameTimerText;

    public GameObject AttemptGameObject;
   

    //setting data from inspecter 
    //public float gameDuration=45f;
    //public float previewCountdown = 10f;
    //public float attempts = 4;
    //setting data from prefs
    private float gameDuration = 45f;
    private float previewCountdown = 10f;
    private float attempts = 4;

    private List<int> selectedIndices = new List<int>();
    private List<Sprite> shuffledImages = new List<Sprite>();

    private int validationCounter = 2;
    private List<int> validatingIndices = new List<int>();
    private List<GameObject> validatingObjects = new List<GameObject>();
    private bool itemsClickable = false;
    public RenderTexture rt;
        void Start()
        {
            nextButton.onClick.AddListener(OnNextButtonClick);
            firstScreen.SetActive(true);
            secondScreen.SetActive(false);
            mainScreen.SetActive(false);
            resultScreen.SetActive(false);
            lastScreen.SetActive(false);
            previewCountdown = PlayerPrefs.GetFloat("initialTiming", 30f);
            gameDuration = PlayerPrefs.GetFloat("matchingTiming", 15f);
            attempts = PlayerPrefs.GetFloat("attempts", 3f);

    }
        private void OnNextButtonClick()
        {
            firstScreen.SetActive(false);
            PlayReadyGameVideo();
           // secondScreen.SetActive(true);
        }
        private void OnStartButtonClick()
        {
            secondScreen.SetActive(false);
            PlayReadyGameVideo();
        }
        private void PlayReadyGameVideo()
        {
            videoScreen.SetActive(true);
            ResetVideoPlayer(); // Reset the video player before playing the new video
            videoPlayer.clip = clips[0];
            videoPlayer.Play(); // Play the video
            videoPlayer.loopPointReached += OnVideoFinished; // Trigger method when video finishes
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


        private void OnVideoFinished(VideoPlayer vp)
        {
            videoScreen.SetActive(false);
            mainScreen.SetActive(true);
            InitializeContent();
        }
        void InitializeContent()
        {
            boxAssigner = new BoxAssigner();

            // Select 6 pairs from both imageArray and logoArray
            selectedIndices = boxAssigner.AssignRandomIndices(imageArray, logoArray, 6);

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

            textimaghe.enabled = false;

            itemsClickable = true;

            attemptsText.text = attempts.ToString();
            AttemptGameObject.SetActive(true);

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
            Debug.LogError($"Mismatch! {shuffledImages[firstIndex].name} and {shuffledImages[secondIndex].name}");

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
        private void PlayResultVideo()
        {
            mainScreen.SetActive(false);
            Debug.LogWarning("result video is playing");
            videoScreen.SetActive(true); // Activate video player
            ResetVideoPlayer(); // Reset the video player before playing the new video
            videoPlayer.clip = clips[1];
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnResultVideoFinished; // Trigger when the video ends
        }

        private void OnResultVideoFinished(VideoPlayer vp)
        {
            videoScreen.SetActive(false); 
            lastScreen.SetActive(true);
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
            yield return new WaitForSeconds(resultScreenDuraion);
            lastScreen.SetActive(true);
            StartCoroutine(WaitAndReload());
        }
        private IEnumerator WaitAndReload()
        {
            yield return new WaitForSeconds(endScreenDuraion);
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }
}