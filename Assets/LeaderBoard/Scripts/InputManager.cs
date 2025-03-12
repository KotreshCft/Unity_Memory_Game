using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [SerializeField] public TMP_InputField nameInput;
    //[SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField contactInput;
    [SerializeField] private Button registerButton;
    [SerializeField] private GameObject inGameKeyboard;
    [SerializeField] private Button overlayButton; // Transparent button for clicking outside input fields

    private TMP_InputField activeInputField; // Tracks the currently selected input field

    private string dataFilePath; // Path for the CSV file

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

        // Set up the file path in StreamingAssets/Data folder
        string folderPath = Path.Combine(Application.streamingAssetsPath, "Data");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        dataFilePath = Path.Combine(folderPath, "user_data.csv");
    }

    private void Start()
    {
        // Initially hide the in-game keyboard
        inGameKeyboard.SetActive(false);

        // Add listeners to the input fields
        nameInput.onSelect.AddListener((_) => ShowKeyboard(nameInput));
        //emailInput.onSelect.AddListener((_) => ShowKeyboard(emailInput));
        contactInput.onSelect.AddListener((_) => ShowKeyboard(contactInput));

        // Add listener to the start button
        registerButton.onClick.AddListener(RegisterPlayer);

        // Add listener to the overlay button
        overlayButton.onClick.AddListener(HideKeyboard);

        // Initialize CSV file if it doesn't exist
        InitializeCSV();

        if (customFont == null)
        {
            // Load the font dynamically from the specified path
            customFont = Resources.Load<Font>("Frigus/Fonts/MessinaSans-Bold");
            if (customFont == null)
            {
                Debug.LogError("Custom font not found! Please check the path or assign the font manually.");
                return;
            }
        }

        CreateNotificationUI();
    }

    private void InitializeCSV()
    {
        if (!File.Exists(dataFilePath))
        {
            // Add headers to the CSV file
            string headers = "Name,Contact";
            File.WriteAllText(dataFilePath, headers + "\n");
        }
    }

    private void Update()
    {
        // Check if both Shift and Escape are pressed
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyUp(KeyCode.Escape))
        {
            PlayerPrefs.DeleteAll();
            // Clear the data in the CSV file (only keep the headers)
            if (File.Exists(dataFilePath))
            {
                string headers = "Name,Contact"; // Your CSV headers
                File.WriteAllText(dataFilePath, headers + "\n"); // Overwrite file with headers only
                Debug.Log("CSV data cleared, only headers remain.");
                ShowErrorNotification("CSV data cleared, only headers remain.");
            }
        }
    }

    private void ShowKeyboard(TMP_InputField inputField)
    {
        activeInputField = inputField;
        inGameKeyboard.SetActive(true);
    }

    public void HideKeyboard()
    {
        inGameKeyboard.SetActive(false);
        activeInputField = null;
    }

    public void AddLetter(string letter)
    {
        if (activeInputField != null)
        {
            activeInputField.text += letter;
        }
    }

    public void DeleteLetter()
    {
        if (activeInputField != null && activeInputField.text.Length > 0)
        {
            activeInputField.text = activeInputField.text.Remove(activeInputField.text.Length - 1);
        }
    }

    public void SubmitWord()
    {
        if (activeInputField != null)
        {
            Debug.Log("Submitted: " + activeInputField.text);

            // Check if the current input field is `nameInput`
            if (activeInputField == nameInput)
            {
                // Move focus to `contactInput`
                contactInput.Select();
                activeInputField = contactInput;
            }
            else if (activeInputField == contactInput)
            {
                // Hide the keyboard if the current input is `contactInput`
                HideKeyboard();
            }
        }
        else
        {
            Debug.Log("No active input field.");
        }
        // Handle submit action if needed
        Debug.Log("Submitted: " + (activeInputField != null ? activeInputField.text : "No active input"));
    }
    private void RegisterPlayer()
    {
        // Start the pop animation on the button
        StartCoroutine(FlipkartManager.Instance.ButtonPopAnimation(registerButton));

        // Start the game logic after the animation is finished
        StartCoroutine(DelayValidateAndStartGame());
    }
    private IEnumerator DelayValidateAndStartGame()
    {
        yield return new WaitForSeconds(FlipkartManager.Instance.popDuration * 2); // Wait for both scale up and scale down duration
        ValidateData();
    }

    private void ValidateData()
    {
        //AudioManager.Instance.PlayClickSound();
        string name = nameInput.text.Trim();
        //string email = emailInput.text.Trim();
        string contact = contactInput.text.Trim();

        // Validation checks
        if (string.IsNullOrEmpty(name))
        {
            ShowErrorNotification("Please Enter Name!");
            return;
        }

        //if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
        //{
        //    ShowErrorNotification("Please enter a valid email!");
        //    return;
        //}

        if (string.IsNullOrEmpty(contact) || contact.Length != 10 || !IsDigitsOnly(contact))
        {
            ShowErrorNotification("Please Enter Valid Contact Number!");
            return;
        }

        //Debug.Log($"Name: {name}; Email: {email}; Contact: {contact}");

        // Save data to CSV
        SaveDataToCSV(name, contact); //email,

        // If all validations pass, start the game
        FlipkartManager.Instance.StartNewGame();
    }
    private void SaveDataToCSV(string name, string contact) //, string email
    {
        string dataLine = $"{name},{contact}"; // ,{email}
        File.AppendAllText(dataFilePath, dataLine + "\n");
        Debug.Log("Data saved to CSV: " + dataLine);
    }

    private bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$"; // Basic email regex pattern
        return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
    }

    private bool IsDigitsOnly(string str)
    {
        foreach (char c in str)
        {
            if (!char.IsDigit(c))
            {
                return false;
            }
        }
        return true;
    }
    private GameObject notificationPanel;
    private Text notificationText;
    public Font customFont; // Assign your custom font here
    private void ShowErrorNotification(string errorMessage)
    {
        Debug.LogWarning(errorMessage);
        notificationPanel.SetActive(true);
        notificationPanel.GetComponent<Image>().color = Color.red; // Error notifications are red
        notificationText.text = errorMessage;

        // Cancel any previous scheduled hide to ensure a fresh countdown
        CancelInvoke(nameof(HideNotification));

        // Hide after 3 seconds
        Invoke(nameof(HideNotification), 3f);
    }
    // Dynamically create the notification UI
    private void CreateNotificationUI()
    {
        // Create the panel
        notificationPanel = new GameObject("NotificationPanel");
        notificationPanel.transform.SetParent(this.transform);
        notificationPanel.transform.localScale = Vector3.one;

        RectTransform panelRect = notificationPanel.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(800, 70); // Panel size
        panelRect.anchorMin = new Vector2(0.5f, 1); // Center-top
        panelRect.anchorMax = new Vector2(0.5f, 1);
        panelRect.pivot = new Vector2(0.5f, 1);
        panelRect.anchoredPosition = new Vector2(0, -845); // Offset from top-left corner

        Image panelImage = notificationPanel.AddComponent<Image>();
        panelImage.color = Color.clear; // Initially transparent

        // Create the text
        GameObject textObj = new GameObject("NotificationText");
        textObj.transform.SetParent(notificationPanel.transform);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(680, 80); // Text size
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;

        notificationText = textObj.AddComponent<Text>();
        notificationText.transform.localScale = Vector3.one;
        notificationText.font = customFont; // Use your custom font
        notificationText.fontSize = 24;
        notificationText.alignment = TextAnchor.MiddleCenter;
        notificationText.color = Color.black;

        notificationPanel.SetActive(false); // Initially hide the panel
    }
    private void HideNotification()
    {
        notificationPanel.SetActive(false);
    }
}
