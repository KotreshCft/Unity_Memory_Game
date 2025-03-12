using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SettingManager : MonoBehaviour
{
    public GameObject settingPanel;
    public TMP_Dropdown gameDurationDropdown;
    public TMP_Dropdown previewDurationDropdown;
    public TMP_Dropdown endScreenDurationDropdown;

    // Keys for PlayerPrefs
    private const string GameDurationKey = "GameDuration";
    private const string PreviewDurationKey = "PreviewDuration";
    private const string EndScreenDurationKey = "EndScreenDuration";

    void Start()
    {
        // Load saved durations or use defaults
        int gameDuration = PlayerPrefs.GetInt(GameDurationKey, 45);
        int previewDuration = PlayerPrefs.GetInt(PreviewDurationKey, 15);
        int endScreenDuration = PlayerPrefs.GetInt(EndScreenDurationKey, 10);

        // Initialize dropdowns with options and default values
        InitializeDropdown(gameDurationDropdown, new List<int> { 15, 25, 30, 45, 50, 60 }, gameDuration);
        InitializeDropdown(previewDurationDropdown, new List<int> { 3, 5, 7, 10, 15 }, previewDuration);
        InitializeDropdown(endScreenDurationDropdown, new List<int> { 5, 10, 15 }, endScreenDuration);

        // Add listeners to handle changes
        gameDurationDropdown.onValueChanged.AddListener(delegate { OnGameDurationChanged(); });
        previewDurationDropdown.onValueChanged.AddListener(delegate { OnPreviewDurationChanged(); });
        endScreenDurationDropdown.onValueChanged.AddListener(delegate { OnEndScreenDurationChanged(); });
    }

    // Initialize dropdown with options and set default value
    private void InitializeDropdown(TMP_Dropdown dropdown, List<int> options, int defaultValue)
    {
        dropdown.ClearOptions();
        List<string> optionStrings = options.ConvertAll(option => option.ToString());
        dropdown.AddOptions(optionStrings);

        // Set the default value
        int defaultIndex = options.IndexOf(defaultValue);
        if (defaultIndex != -1)
        {
            dropdown.value = defaultIndex;
        }
    }

    // Event handlers for dropdown changes
    private void OnGameDurationChanged()
    {
        int selectedValue = int.Parse(gameDurationDropdown.options[gameDurationDropdown.value].text);
        PlayerPrefs.SetInt(GameDurationKey, selectedValue);
        PlayerPrefs.Save();
        Debug.Log("Game Duration set to: " + selectedValue);
    }

    private void OnPreviewDurationChanged()
    {
        int selectedValue = int.Parse(previewDurationDropdown.options[previewDurationDropdown.value].text);
        PlayerPrefs.SetInt(PreviewDurationKey, selectedValue);
        PlayerPrefs.Save();
        Debug.Log("Preview Duration set to: " + selectedValue);
    }

    private void OnEndScreenDurationChanged()
    {
        int selectedValue = int.Parse(endScreenDurationDropdown.options[endScreenDurationDropdown.value].text);
        PlayerPrefs.SetInt(EndScreenDurationKey, selectedValue);
        PlayerPrefs.Save();
        Debug.Log("End Screen Duration set to: " + selectedValue);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            settingPanel.SetActive(!settingPanel.activeSelf);
        }
    }
}
