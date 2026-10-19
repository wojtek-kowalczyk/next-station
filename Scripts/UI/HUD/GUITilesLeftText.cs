using TMPro;
using UnityEngine;

namespace NS.UI.HUD
{
    public class GUITilesLeftText : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        private int cachedCount = -1;
        private void Update()
        {
            var tileStack = Game.Instance?.TileStack;
            if (tileStack == null)
            {
                return;
            }

            if (cachedCount == tileStack.Count)
            {
                return;
            }

            cachedCount = tileStack.Count;
            text.text = $"Tiles left: {tileStack.Count}";
        }
    }
}
