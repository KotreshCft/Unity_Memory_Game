📚 Memory Game - Unity Project
This Unity memory game project contains three main scenes:

SettingSceen.unity - The settings screen where users can customize preferences.
MainGame_Indeed.unity - The primary game scene for the memory game.
FlipkartMain.unity - An alternative main game scene with a Flipkart theme.
🛠️ Setting Up the Scene Navigation
In the Settings Scene, the project uses a script called InputManager. This script handles user input and saves preferences using PlayerPrefs.

To enable navigation to the correct game scene based on requirements, the InputManager script needs to be configured to store the desired scene name. You can set the scene name as either:

"MainGame_Indeed"
"FlipkartMain"
The selected scene name will be saved to PlayerPrefs and used to navigate to the appropriate game scene when triggered.

🚀 How to Use
Open the SettingSceen.unity in the Unity Editor.
Assign the desired scene name (MainGame_Indeed or FlipkartMain) in the InputManager script.
The game will navigate to the selected scene based on the saved preference.
