using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


//https://discussions.unity.com/t/is-there-an-event-for-before-a-scene-is-unloaded/899365/3
public class CustomSceneManager
{
    public static event UnityAction beforeSceneUnload;


    

    //Research on async:
    //Great unity example:https://stackoverflow.com/questions/69282112/how-to-do-waituntil-in-async
    //Horrible discussion forum thread where no one ever actually gets anywhere: https://discussions.unity.com/t/help-with-async-await/664159/38
    //MSDN: https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/

    //This is an async method,
    //it's basically like starting a coroutine
    //and you can just do LoadSceneAsync()
    //and it'll start.
    public static async Task LoadSceneAsync(string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode = LoadSceneMode.Single)
    {
        try
        {



            //If we exit the SnakeWakeupAsHero scene,
            //we should instead save the scene we should load in at
            //as the WorldStartInHouse Scene,
            //This is just in case the player happens to exit to the menu
            //and loads back in, just to prevent them from sequence breaking.
            //Basically, only load into the scene where they can't unlock this
            //character again if they've already unlocked the souse. 
            //otherwise, load into the scene with the souse to allow them to unlock it.
            /*        if (SceneManager.GetActiveScene().name.Contains("SnakeWakeupAsHero") && (DataPersistenceManager.instance.GetGameData().unlockedCharacters.Contains(CharacterType.Souse) || Player.Instance != null && Player.Instance.availableCharacters.Exists(c => c.characterType == CharacterType.Souse)))
                    {
                        GameManager.Instance.sceneToLoadOnStart = "WorldStartInHouse";
                    }*/



            var lst = new List<GameObject>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var sc = SceneManager.GetSceneAt(i);
                lst.Clear();
                sc.GetRootGameObjects(lst);
                //If any MonoBehavior has a method
                //called "OnBeforeSceneUnloaded"
                //it will be called here.
                foreach (var go in lst) go.BroadcastMessage("OnBeforeSceneUnloaded", null, SendMessageOptions.DontRequireReceiver);
            }

            Debug.LogWarning("BEGIN WAITING 1");
            //call the event if anything is subscribed to it before we load.
            beforeSceneUnload?.Invoke();

            Debug.LogWarning("BEGIN WAITING 2");


            //Wait and make sure we aren't in a cutscene
            //this is because fading in/out counts
            //as a cut scene.
            await WaitTillNotCutscene();

            Debug.LogWarning("STOP WAITING");

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, mode);
            asyncOperation.allowSceneActivation = false; // Prevent immediate activation

            while (!asyncOperation.isDone)
            {
                // Update loading progress UI
                float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
                Debug.LogWarning(progress);
                // ... update UI with 'progress'

                if (asyncOperation.progress >= 0.9f)
                {
                    // When ready, allow scene activation
                    asyncOperation.allowSceneActivation = true;
                }
                await Task.Delay(1);
            }

            //Actually load the scene asynchronously.
            //await SceneManager.LoadSceneAsync(sceneName, mode);
        }
        catch (Exception e)
        {
            Debug.LogError($"LoadSceneAsync failed for {sceneName}: {e}");
        }
    }




    //Because we can't use coroutines we're
    //using the System.Threading.Tasks
    //to do the same thing that corouitines 
    //do, which is to wait.
    static async Task WaitTillNotCutscene()
    {
        Debug.LogWarning("START WAITING");
        while (GameManager.Instance.inCutscene)
        {
            Debug.LogWarning("WAITING");

            //Wait 50 milliseconds before checking the condition again,
            //basically like calling yield return new waitForSecondsRealtime
            //and giving it ms.
            await Task.Delay(50);
        }
        Debug.LogWarning("STOP WAITING");



        //Another thing to do before we unload,
        //Make sure Time.timeScale is set to 1 
        //just in case we've used the pause menu to exit
        //a scene and haven't set it back.
        Time.timeScale = 1f;

        //Yet another thing to do, 
        //We need to make sure the
        //player gets control of the mouse
        //cursor back just in case
        //we don't give it back to them
        //before the next scene
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

}