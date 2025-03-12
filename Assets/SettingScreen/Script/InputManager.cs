
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
    public TMP_InputField initialTiming;
    public TMP_InputField matchingTiming;
    public TMP_InputField attempts;
    public TMP_Text errorMessage;
    public Button submitButton;
    [Header("Game SceneName")]
    public string MainGame_Indeed = "FlipkartMain"; // Assets/Indeed/Scenes/MainGame_Indeed.unity
                                                       //Assets/Flipkart/FlipkartMain.unity


    void Start()
    {
        // Load and display saved data on startup
        LoadData();

        // Update slider value display
       
        // Add listener to the submit button
        submitButton.onClick.AddListener(SaveData);
    }

    void LoadData()
    {



        initialTiming.text = PlayerPrefs.HasKey("initialTiming") ? PlayerPrefs.GetFloat("initialTiming").ToString() : "10";
        matchingTiming.text = PlayerPrefs.HasKey("matchingTiming") ? PlayerPrefs.GetFloat("matchingTiming").ToString() : "30";
        attempts.text = PlayerPrefs.HasKey("attempts") ? PlayerPrefs.GetFloat("attempts").ToString() : "3";

        // Load slider value


        Debug.Log("Loaded previous data successfully!");
    }

    void SaveData()
    {

        errorMessage.text = ""; // Clear any previous error message

        // Validate and save each input field individually as float
        if (!ValidateAndSave(initialTiming, "initialTiming", true)) return;
        if (!ValidateAndSave(matchingTiming, "matchingTiming", true)) return;
        if (!ValidateAndSave(attempts, "attempts", true)) return;


        Debug.Log("All data saved successfully!");

        // Display success message in green
        errorMessage.color = Color.green;
        errorMessage.text = "Data saved successfully!";
        LoadNewScene();

    }
   
    public void LoadNewScene()
    {
        // Check if the scene name is not empty
        if (!string.IsNullOrEmpty(MainGame_Indeed))
        {
            // Load the specified scene
            SceneManager.LoadScene(MainGame_Indeed); //Assets / setting_screen / Setting_scene.unity
        }
        else
        {
            Debug.LogError("Scene name is not set!");
        }
    }

    bool ValidateAndSave(TMP_InputField field, string key, bool allowDecimal = false)
    {
        string input = field.text;

        // If the input is empty, load the previous value from PlayerPrefs
        if (string.IsNullOrEmpty(input))
        {
            float savedValue = PlayerPrefs.GetFloat(key, 0f);
            field.text = savedValue.ToString();
            Debug.LogWarning($"Empty input for {field.name}. Loaded saved value: {savedValue}");
            return true;
        }

        // Check for alphabets or non-integer input
        if (Regex.IsMatch(input, @"[a-zA-Z]") || !Regex.IsMatch(input, @"^\d+$"))
        {
            // Display error message in red
            errorMessage.color = Color.red;
            errorMessage.text = $"Invalid input in {field.name}. Please enter a valid whole number.";
            Debug.Log($"Invalid input in {field.name} (contains alphabets or decimal): {input}");
            return false;
        }

        // Try to parse the input as a float (even though it's an integer, we store it as a float)
        if (float.TryParse(input, out float floatValue))
        {
            // Save valid input to PlayerPrefs as a float
            PlayerPrefs.SetFloat(key, floatValue);
            Debug.Log($"Saved {field.name}: {floatValue}");
            return true;
        }
        else
        {
            // Display error message in red
            errorMessage.color = Color.red;
            errorMessage.text = $"Invalid input in {field.name}. Please enter a valid number.";
            Debug.Log($"Invalid input in {field.name} (not a float): {input}");
            return false;
        }
    }
}