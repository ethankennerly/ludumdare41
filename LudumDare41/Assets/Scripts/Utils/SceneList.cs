using System;
using UnityEngine;

namespace Finegamedesign.Utils
{
    [CreateAssetMenu(menuName="Fine Game Design/SceneList", fileName="New SceneList")]
    public sealed class SceneList : ScriptableObject
    {
        [Header("Each scene in the build to cycle through.")]
        [SerializeField]
        private string[] m_SceneNames;

        // Resets each play.  If serialized, the value persists.
        [NonSerialized]
        private int m_CurrentIndex = 0;

        public string GetNextSceneName()
        {
            ++m_CurrentIndex;
            if (m_CurrentIndex >= m_SceneNames.Length)
            {
                m_CurrentIndex = 0;
            }
            return m_SceneNames[m_CurrentIndex];
        }
    }
}
