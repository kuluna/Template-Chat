using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Template.Chat
{
    [CreateAssetMenu(fileName = "Pictures", menuName = "Chat/Pictures")]
    public class Pictures : ScriptableObject
    {
        public List<Picture> pictures = new();

        [System.Serializable]
        public class Picture
        {
            public string pictureName = string.Empty;
            public Sprite sprite = null!;
        }
    }
}
