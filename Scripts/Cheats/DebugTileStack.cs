using System;
using System.Text;
using NS.Core;
using NS.Core.Blueprints;
using UnityEngine;
using Random = NS.Core.Random;

namespace NS.Cheats
{
    public class DebugTileStack : MonoBehaviour, ITileStack
    {
        private TileType SelectedType => (TileType)(1 << selectedTypeIndex);
        private int selectedTypeIndex;
        private bool isSelectedTypeDirty;
        private HexTileData[] tiles;

        public int Count => 999;

        private void OnEnable()
        {
            GenerateTile(isStation: Input.GetKey(KeyCode.LeftShift));
        }

        public HexTileData PeekTile()
        {
            EnsureFresh();
            return tiles[0];
        }

        public HexTileData PopTile()
        {
            EnsureFresh();
            return tiles[0];
        }

        public Span<HexTileData> PeekTiles()
        {
            EnsureFresh();
            return tiles;
        }

        private void EnsureFresh()
        {
            if (isSelectedTypeDirty == false)
            {
                return;
            }

            isSelectedTypeDirty = false;
            GenerateTile(isStation: Input.GetKey(KeyCode.LeftShift));
        }

        public void AddTiles(int count)
        {
        }

        public void PushNextTile(HexTileData tile)
        {
        }

        private void GenerateTile(bool isStation)
        {
            var edges = new[] { EdgeType.Mountain, EdgeType.Basic, EdgeType.Town };
            var types = new HexTileDataBlueprint.TileTypeWithWeight[]
            {
                new() { type = SelectedType, weight = 1 },
            };
            var stationBp = new StationDataBlueprint(ResourceType.Wood, 1, gives: ResourceType.People, 1);
            var bp = new HexTileDataBlueprint(types, edges, isStation? stationBp : null);
            tiles = new[] { bp.Generate(new Random(0)) }; // always the same seed
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.LeftShift))
            {
                isSelectedTypeDirty = true;
            }

            // HACK : prevent conflict with zooming
            if (Input.GetKey(KeyCode.LeftControl) == false)
            {
                return;
            }

            int newIndex = Input.mouseScrollDelta.y switch
            {
                > 0 => selectedTypeIndex - 1,
                < 0 => selectedTypeIndex + 1,
                _ => selectedTypeIndex
            };

            if (newIndex >= TileTypeExtensions.TILE_TYPE_COUNT)
            {
                newIndex = 0;
            }
            else if (newIndex < 0)
            {
                newIndex = TileTypeExtensions.TILE_TYPE_COUNT - 1;
            }

            if (selectedTypeIndex != newIndex)
            {
                selectedTypeIndex = newIndex;
                isSelectedTypeDirty = true;
            }
        }

        private void OnGUI()
        {
            const float paddingY = 400;
            GUILayout.BeginArea(new Rect(0, paddingY, 500, Screen.height - paddingY));

            StringBuilder sb = new();
            sb.Append("[ctrl+scroll] to change\n[C] to disable\nhold [L shift] for station");
            GUILayout.Label(sb.ToString());

            for (int i = 0; i < TileTypeExtensions.TILE_TYPE_COUNT; i++)
            {
                var type = (TileType)(1 << i);

                sb.Clear();
                sb.Append('[');
                sb.Append(type == SelectedType ? 'x' : ' ');
                sb.Append(']');
                sb.Append(' ');
                sb.Append(type.ToString());

                if (type == SelectedType)
                {
                    var color = GUI.contentColor;
                    GUI.contentColor = Color.green;
                    GUILayout.Label(sb.ToString());
                    GUI.contentColor = color;
                }
                else
                {
                    GUILayout.Label(sb.ToString());
                }
            }

            GUILayout.EndArea();
        }
    }
}
