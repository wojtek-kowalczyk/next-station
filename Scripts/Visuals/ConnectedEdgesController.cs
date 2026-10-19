using System.Collections.Generic;
using NS.Core;
using UnityEngine;

namespace NS.Visuals
{
    public class ConnectedEdgesController : MonoBehaviour
    {
        [SerializeField] private EdgeType edgeType;
        [SerializeField] private List<GameObject> regularEdges;
        [SerializeField] private List<GameObject> connectedEdges;

        private void Awake()
        {
            foreach (GameObject edge in connectedEdges)
            {
                edge.SetActive(false);
            }
        }

        public void Refresh(HexGrid grid, Vector2Int myCell)
        {
            // this only works when TAATAA tile is disallowed, i.e.,
            // there's at most only one continuous edge of one type per tile

            HexTileData rotatedMe = grid.GetTile(myCell);

            for (int i = 0; i < HexGridMath.Directions.Length; i++)
            {
                // check if should replace with connected version of this edge
                if (rotatedMe.edges[i] != edgeType)
                {
                    continue;
                }

                if (!grid.TryGetTile(myCell + HexGridMath.Directions[i], out HexTileData rotatedNeighbor))
                {
                    continue;
                }

                if (rotatedNeighbor.edges[HexGridMath.GetOppositeEdgeIndex(i)] != edgeType)
                {
                    continue;
                }

                // do replace with connected version

                // find decoration rotation in an unrotated tile (assuming no TAATAA tile)
                // so for AABBCC tile where B is my edge type the deco rotation is 2.
                HexTileData unrotatedMe = rotatedMe.Rotated(-rotatedMe.placementRotation);
                int decoRotation = 0;
                for (int j = 0; j < 6; j++)
                {
                    if (unrotatedMe.edges[j] != edgeType)
                    {
                        decoRotation++;
                    }
                    else
                    {
                        break;
                    }
                }

                // arrays are starting at 0 and going until last edge of this type.
                // so first undo tile rotation, and then undo decoration rotation.
                int index = i - rotatedMe.placementRotation - decoRotation;
                index = (index + 6) % 6; // wrap around

                // TODO : these index bounds checks should be redundant but I have managed to get an exception
                // here under some configuration that I'm not able to reproduce. Thus, I'm putting these in
                // as a failsafe. Worst case scenario the tile is gonna look weird, but the game will be fine.

                // replace with connected version
                if (index >= 0 && index < regularEdges.Count)
                    regularEdges[index].SetActive(false);
                if (index >= 0 && index < connectedEdges.Count)
                    connectedEdges[index].SetActive(true);
            }
        }
    }
}
