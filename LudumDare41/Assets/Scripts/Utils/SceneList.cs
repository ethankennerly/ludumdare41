using UnityEngine;

namespace Finegamedesign.Utils
{
    [CreateAssetMenu(menuName="Fine Game Design/SceneList", fileName="New SceneList")]
    public sealed class SceneList : ScriptableObject
    {
        [Header("Each scene in the build to cycle through.")]
        [SerializeField]
        private string[] m_SceneNames;

        private int m_SelectedIndex = -1;

        public string GetNextSceneName()
        {
            ++m_SelectedIndex;
            if (m_SelectedIndex >= m_SceneNames.Length)
            {
                m_SelectedIndex = 0;
            }
            return m_SceneNames[m_SelectedIndex];
        }
    }
}
