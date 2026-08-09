using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeSelector : MonoBehaviour
{
    public static int Mode = 0;

    public void StartAppleGame(int modeIndex)
    {
        Mode = modeIndex;
        SceneManager.LoadScene("AppleGameMain");
    }
}
