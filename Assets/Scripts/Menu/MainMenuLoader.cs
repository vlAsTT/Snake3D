using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Menu
{
    /// <summary>
    /// Responsible for handling all main menu logic & game scene loading
    /// </summary>
    /// <seealso cref="SceneManager"/>
    public class MainMenuLoader : MonoBehaviour
    {
        #region Variables

        #region References

        /// <summary>
        /// Reference to the parent menu object
        /// </summary>
        [Header("Main Menu & Loading Screen")]
        [Tooltip("Reference to the parent menu object")][SerializeField] private GameObject menu;
        
        /// <summary>
        /// Reference to the parent loading screen object
        /// </summary>
        [Tooltip("Reference to the parent loading screen object")][SerializeField] private GameObject loadingScreen;
        
        /// <summary>
        /// Reference to the Image of Loading Bar Progress
        /// </summary>
        /// <seealso cref="Image"/>
        [Tooltip("Reference to the Image of Loading Bar Progress")][SerializeField] private Image loadingProgressBar;

        /// <summary>
        /// Reference to the top score text object
        /// </summary>
        [Tooltip("Reference to the top score text object that displays the top score")]
        [SerializeField] private TextMeshProUGUI topScoreField;

        #endregion

        /// <summary>
        /// List of scenes that are being loaded when user proceeds to the Game scene
        /// </summary>
        private List<AsyncOperation> _scenesToLoad = new List<AsyncOperation>();

        #endregion

        #region Methods

        #region Unity Standard

        /// <summary>
        /// Checks that all variables are initialized
        /// </summary>
        private void Start()
        {
            if (!menu || !loadingScreen || !loadingProgressBar || !topScoreField)
            {
                Debug.LogError($"Some variables are not initialized correctly at {name}");
            }
        }

        #endregion
        
        #region Menu Buttons Methods

        /// <summary>
        /// Loads Game Scene & Runs Loading Screen
        /// </summary>
        /// <seealso cref="HideMenu"/>
        /// <seealso cref="ShowLoadingScreen"/>
        /// <seealso cref="SceneManager"/>
        public void StartGame()
        {
            HideMenu();
            ShowLoadingScreen();

            // Load all scenes required for the Game scene
            SceneManager.LoadSceneAsync("_Scenes/Game/Game");
            SceneManager.LoadSceneAsync("Core", LoadSceneMode.Additive);

            StartCoroutine(StartLoadingScreen());
        }

        /// <summary>
        /// Displays actual Top Score on the UI
        /// </summary>
        public void ShowTopScore()
        {
            topScoreField.text = PlayerPrefs.GetInt("TopScore", 0).ToString();
        }

        /// <summary>
        /// Exits the application
        /// </summary>
        public void Exit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
            Application.Quit(1);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Hides the Main Menu
        /// </summary>
        private void HideMenu()
        {
            menu.SetActive(false);
        }

        /// <summary>
        /// Shows Loading Screen Object
        /// </summary>
        private void ShowLoadingScreen()
        {
            loadingScreen.SetActive(true);
        }

        /// <summary>
        /// Updates loading progress on the loading bar
        /// </summary>
        /// <returns>IEnumerator</returns>
        /// <seealso cref="Coroutine"/>
        IEnumerator StartLoadingScreen()
        {
            var totalProgress = 0f;

            foreach (var scene in _scenesToLoad)
            {
                while (!scene.isDone)
                {
                    totalProgress += scene.progress;
                    loadingProgressBar.fillAmount = totalProgress / _scenesToLoad.Count;
                    yield return null;
                }
            }
        }

        #endregion

        #endregion
    }
}
