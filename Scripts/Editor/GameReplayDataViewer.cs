using System.Text;
using NS.Analytics;
using NS.Utility;
using UnityEditor;
using UnityEngine;

namespace NS.Editor
{
    public class GameReplayDataViewer : EditorWindow
    {
        private string text;

        [MenuItem("NS/Game Replay Data Viewer")]
        public static void ShowExample()
        {
            var window = GetWindow<GameReplayDataViewer>();
            window.Show();
        }

        private void OnGUI()
        {
            text = GUILayout.TextArea(text);
            if (GUILayout.Button("parse"))
            {
                var replayData = Utils.FromBase64<GameRecorder.ReplayData>(text);
                var sb = new StringBuilder();
                sb.AppendLine(replayData.gameVersion);
                sb.AppendLine(replayData.gameSeed.ToString());
                foreach (Vector2Int cell in replayData.placementCellsInOrder)
                {
                    sb.AppendLine(cell.ToString());
                }

                Debug.Log(sb.ToString());
            }
        }
    }
}
