using DG.Tweening;
using NS.Core;
using UnityEngine;
using UnityEngine.Assertions;
using Vector3 = UnityEngine.Vector3;

namespace NS.Visuals
{
    public class PreviewTile : MonoBehaviour
    {
        [SerializeField] private MeshRenderer[] baseMeshRenderers;
        [SerializeField] private Material canPlaceMaterialDecoration;
        [SerializeField] private Material cannotPlaceMaterialDecoration;
        [SerializeField] private Material canPlaceMaterialBase;
        [SerializeField] private Material cannotPlaceMaterialBase;
        [SerializeField] private HexTileVisuals hexTileVisuals;
        [SerializeField] private float lerpSpeed = 15.0f;
        [SerializeField] private float appearTweenDuration = 0.3f;
        [SerializeField] private Vector3 appearTweenPunch = Vector3.one;

        private Tween rotationTween;
        private Tween appearTween;

        public StationData? StationData { get; private set; }

        public void SetCanPlace(bool canPlace)
        {
            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.material = canPlace? canPlaceMaterialDecoration : cannotPlaceMaterialDecoration;
            }

            foreach (MeshRenderer meshRenderer in baseMeshRenderers)
            {
                meshRenderer.material = canPlace? canPlaceMaterialBase : cannotPlaceMaterialBase;
            }
        }

        public void SetPosition(Vector3 worldPos, bool instant = false)
        {
            if (instant)
            {
                transform.position = worldPos;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, worldPos, Time.deltaTime * lerpSpeed);
            }
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public void RotateOneStep(int direction)
        {
            Assert.IsTrue(direction == 1 || direction == -1);

            rotationTween?.Kill(complete: true);
            Vector3 rotationEuler = Vector3.up * 60f * direction;
            rotationTween = transform.DOLocalRotate(rotationEuler, 0.35f, RotateMode.LocalAxisAdd).SetEase(Ease.OutQuint);
        }

        public void SetVisible(bool visible)
        {
            bool wasVisible = gameObject.activeInHierarchy;

            gameObject.SetActive(visible);

            if (wasVisible == false && visible)
            {
                appearTween.Kill(complete: true);
                appearTween = transform.DOPunchScale(appearTweenPunch, appearTweenDuration);
            }
        }

        public bool IsVisible()
        {
            return gameObject.activeSelf;
        }

        public void SetupForTile(HexTileData tile)
        {
            hexTileVisuals.SetupForTile(tile);
            StationData = tile.IsStation ? tile.stationData : null;
        }
    }
}
