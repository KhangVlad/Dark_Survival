using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


    public static class Loader
    {
        public enum Scene
        {
            Loading=0,
            GamePlay=1,
            KhangScene=2,
        }

        public static Scene TargetScene;
        public static Scene PreviousScene;

        public static void Load(Scene targetScene, System.Action afterLoadScene = null)
        {
            PreviousScene = TargetScene;
            TargetScene = targetScene;
            SceneManager.LoadScene(Loader.TargetScene.ToString());
        }

        public static void LoadAsync(Scene targetScene, LoadSceneMode mode, float waitAfterLoad, System.Action onCompleted)
        {
            PreviousScene = TargetScene;
            TargetScene = targetScene;
            CoroutineManager.Instance.StartStaticCoroutine(LoadSceneAsyncCoroutine(targetScene, mode, waitAfterLoad, onCompleted));
        }

        public static AsyncOperation LoadAsyncDontActive(Scene targetScene)
        {
            TargetScene = targetScene;
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(targetScene.ToString(), LoadSceneMode.Single);
            // Prevent the scene from activating immediately
            asyncOperation.allowSceneActivation = false;
            return asyncOperation;
        }

        public static IEnumerator LoadSceneAsyncCoroutine(Scene targetScene, LoadSceneMode mode, float waitAfterLoad,
            System.Action onCompleted)
        {
            // Start loading the scene asynchronously in additive mode
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(targetScene.ToString(), mode);

            // Prevent the scene from activating immediately
            asyncOperation.allowSceneActivation = false;

            // While the scene is still loading
            while (!asyncOperation.isDone)
            {
                // Check loading progress (from 0 to 0.9)
                float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f); // Normalize the progress to 0-1
                                                                                // Debug.Log("Loading progress: " + progress * 100 + "%");
                // When the scene is almost done loading (progress >= 0.9)
                if (asyncOperation.progress >= 0.9f)
                {
                    // Debug.Log("Scene is almost loaded, waiting to activate");

                    // Wait a seconds in order show tips
                    yield return new WaitForSeconds(waitAfterLoad);

                    // Allow the scene to activate
                    asyncOperation.allowSceneActivation = true;
                }

                yield return null;
            }

            // After activation, the scene should be fully loaded and visible
            // Debug.Log("Scene fully loaded and activated.");
            onCompleted?.Invoke();
        }
    }
