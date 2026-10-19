using System.Collections.Generic;
using NS.Core;
using UnityEngine;

namespace NS.Visuals
{
    public class TileStackVisuals : MonoBehaviour
    {
        [SerializeField] private Transform[] previewTilesTransforms;
        [SerializeField] private Transform genericTilesTransform;
        [SerializeField] private HexTileVisuals previewPrefab;
        [SerializeField] private GameObject genericTilePrefab;
        [SerializeField] private float genericTilesSpacing = 0.2f;
        [SerializeField] private int maxGenericTiles = 20; // more will go offscreen and we won't see them
        [SerializeField] private GameObject backgroundShadow;

        private readonly List<GameObject> spawnedObjects = new();
        private readonly List<HexTileData> cachedTiles = new();
        private int cachedCount = -1;

        private void Update()
        {
            var tileStack = Game.Instance.TileStack;

            if (RequiresRefresh(tileStack))
            {
                cachedTiles.Clear();
                cachedTiles.AddRange(tileStack.PeekTiles().ToArray());
                cachedCount = tileStack.Count;
                Set(tileStack);
            }
        }

        private bool RequiresRefresh(ITileStack tileStack)
        {
            if (tileStack.Count != cachedCount)
            {
                return true;
            }

            var preloadedTiles = tileStack.PeekTiles();
            if (preloadedTiles.Length != cachedTiles.Count)
            {
                return true;
            }

            for (int i = 0; i < preloadedTiles.Length; i++)
            {
                HexTileData actualTile = preloadedTiles[i];
                HexTileData cachedTile = cachedTiles[i];
                if (actualTile.Equals(cachedTile) == false)
                {
                    return true;
                }
            }

            return false;
        }

        private void Set(ITileStack stack)
        {
            foreach (GameObject spawnedObject in spawnedObjects)
            {
                Destroy(spawnedObject);
            }

            spawnedObjects.Clear();

            backgroundShadow.SetActive(stack.Count != 0);

            var previewTiles = stack.PeekTiles();
            int numGenericTiles = Mathf.Min(stack.Count - previewTiles.Length, maxGenericTiles);

            for (int i = 0; i < previewTiles.Length; i++)
            {
                HexTileData tile = previewTiles[i];
                HexTileVisuals instance = Instantiate(previewPrefab, previewTilesTransforms[i]);
                instance.transform.localPosition = Vector3.zero;
                instance.SetupForTile(tile);

                ChangeLayerForObjectAndChildren(instance.transform, LayerMask.NameToLayer("HexStack"));

                spawnedObjects.Add(instance.gameObject);
            }

            for (int j = 0; j < numGenericTiles; j++)
            {
                GameObject instance = Instantiate(genericTilePrefab, genericTilesTransform);
                ChangeLayerForObjectAndChildren(instance.transform, LayerMask.NameToLayer("HexStack"));
                instance.transform.localPosition = new Vector3(0, -j * genericTilesSpacing, 0);
                spawnedObjects.Add(instance);
            }

        }

        private static void ChangeLayerForObjectAndChildren(Transform t, int layer)
        {
            t.gameObject.layer = layer;

            foreach (Transform child in t)
            {
                ChangeLayerForObjectAndChildren(child, layer);
            }
        }

        public void Rotate(float angleDelta)
        {
            // rotate the tile container, so that even if we swap out the tile model (child)
            // it will have correct rotation.

            Quaternion rot = Quaternion.Euler(0, angleDelta, 0);
            foreach (Transform previewTransform in previewTilesTransforms)
            {
                previewTransform.localRotation *= rot;
            }
            genericTilesTransform.localRotation *= rot;
        }

#if UNITY_EDITOR
        [ContextMenu("Spawn dummy tiles")]
        private void EDITOR_SpawnDummyTiles()
        {
            for (int i = 0; i < previewTilesTransforms.Length; i++)
            {
                GameObject instance = Instantiate(genericTilePrefab, previewTilesTransforms[i]);
                ChangeLayerForObjectAndChildren(instance.transform, LayerMask.NameToLayer("HexStack"));
            }

            for (int i = 0; i < 10; i++)
            {
                GameObject instance = Instantiate(genericTilePrefab, genericTilesTransform);
                ChangeLayerForObjectAndChildren(instance.transform, LayerMask.NameToLayer("HexStack"));
                instance.transform.localPosition = new Vector3(0, -i * genericTilesSpacing, 0);
            }
        }

        [ContextMenu("Destroy dummy tiles (all children)")]
        private void EDITOR_DestroyDummyTiles()
        {
            List<GameObject> toDestroy = new();
            foreach (Transform t in previewTilesTransforms)
            {
                foreach (Transform child in t)
                {
                    toDestroy.Add(child.gameObject);
                }
            }

            foreach (Transform child in genericTilesTransform)
            {
                toDestroy.Add(child.gameObject);
            }

            foreach (GameObject go in toDestroy)
            {
                DestroyImmediate(go);
            }
        }
#endif
    }
}
