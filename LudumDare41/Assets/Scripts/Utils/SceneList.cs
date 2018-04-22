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
        public string[] sceneNames
        {
            get { return m_SceneNames; }
            set { m_SceneNames = value; }
        }
    }
}
