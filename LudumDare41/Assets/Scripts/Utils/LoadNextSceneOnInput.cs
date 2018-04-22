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

        public void OnEnable()
        {
            OnSelectDown_LoadNextScene = LoadNextScene;
            VerticalInputSystem.onSelectDown += OnSelectDown_LoadNextScene;
        }

        private void OnDisable()
        {
            VerticalInputSystem.onSelectDown -= OnSelectDown_LoadNextScene;
        }

        private void LoadNextScene()
        {
            string sceneName = m_SceneList.GetNextSceneName();
            SceneManager.LoadScene(sceneName);
        }
    }
}
