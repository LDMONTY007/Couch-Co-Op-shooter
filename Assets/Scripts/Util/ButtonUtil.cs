using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonUtil : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadScene(string sceneName)
    {
        CustomSceneManager.LoadSceneAsync(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
