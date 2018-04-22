using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Finegamedesign.Utils
{
    public sealed class LoadNextSceneOnInput : MonoBehaviour
    {
        [SerializeField]
        private SceneList m_SceneList;

        private Action OnSelectDown_LoadNextScene;

        private int m_CurrentIndex = -1;
        public int currentIndex
        {
            get { return m_CurrentIndex; }
            set { m_CurrentIndex = value; }
        }

        public string GetNextSceneName(string[] sceneNames)
        {
            ++m_CurrentIndex;
            if (m_CurrentIndex >= sceneNames.Length || m_CurrentIndex < 0)
            {
                m_CurrentIndex = 0;
            }
            return sceneNames[m_CurrentIndex];
        }

        public void SetCurrent(string sceneName)
        {
            m_CurrentIndex = Array.IndexOf(m_SceneList.sceneNames, sceneName);
        }

        public void OnEnable()
        {
            OnSelectDown_LoadNextScene = LoadNextScene;

            // WebGL has a mysterious error.
            // blob:http://localhost:56635/8d33d038-b1f6-4569-bcb8-e16e70086f5c:8815 A script behaviour (script unknown or not yet loaded) has a different serialization layout when loading. (Read 48 bytes but expected 76 bytes)
            // Did you #ifdef UNITY_EDITOR a section of your serialized properties in any of your scripts?
            // ArgumentNullException: Argument cannot be null.  Parameter name: array
            if (m_SceneList.sceneNames == null || m_SceneList.sceneNames.Length == 0)
            {
                DebugUtil.Log("LoadNextSceneOnInput.OnEnable: scene names missing. Defaulting.");
                m_SceneList.sceneNames = new string[]
                {
                    "Level_0",
                    "Level_1"
                };
            }
            SetCurrent(SceneManager.GetActiveScene().name);
            VerticalInputSystem.onSelectDown += OnSelectDown_LoadNextScene;
        }

        private void OnDisable()
        {
            VerticalInputSystem.onSelectDown -= OnSelectDown_LoadNextScene;
        }

        // Avoid responding to spam.
        private void LoadNextScene()
        {
            if (!gameObject.activeSelf)
            {
                return;
            }
            gameObject.SetActive(false);
            string sceneName = GetNextSceneName(m_SceneList.sceneNames);
            SceneManager.LoadScene(sceneName);
        }
    }
}
