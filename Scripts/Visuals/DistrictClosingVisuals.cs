using System.Collections;
using System.Collections.Generic;
using NS.Core.GameModes.Common;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using DG.Tweening;

namespace NS.Visuals
{
    public class DistrictClosingVisuals : MonoBehaviour
    {
        [SerializeField] private float delay = 0.5f;
        [SerializeField] private float goodWaitTime = 3.0f;
        [SerializeField] private float badWaitTime = 0.75f;
        [SerializeField] private float tweenInTime;
        [SerializeField] private float tweenOutTime;
        [SerializeField] private Material goodMat;
        [SerializeField] private Material badMat;

        [SerializeField] private GameObject indicatorPrefab;

        private Transform placeholdersParent;

        private void Awake()
        {
            var container = new GameObject("Placeholder Container");
            placeholdersParent = container.transform;
        }

        private void OnEnable()
        {
            Game.Instance.GameMode.DistrictClosed += OnDistrictClosed;
        }

        private void OnDisable()
        {
            Game.Instance.GameMode.DistrictClosed -= OnDistrictClosed;
        }

        private void OnDistrictClosed(DistrictFinder.Result result, bool gotReward)
        {
            var grid = Game.Instance.Grid;

            // Results are collected as BFS goes, but this does not necessarily make it visually pleasing.
            // Sort them by distance to the placed tile for more juicy feedback.

            List<Vector2> trianglePositions = new();
            foreach (DistrictFinder.EdgeLocator edgeLocator in result.districtEdges)
            {
                Vector3 centerPoint = grid.Math.EdgeTriangleToWorld(edgeLocator.cell, edgeLocator.edgeIndex);
                trianglePositions.Add(centerPoint.To2());
            }

            Vector2 origin = grid.Math.HexToWorld(result.startingCell);
            trianglePositions.Sort((v1, v2) =>
            {
                // closer one first
                Vector2 toV1 = v1 - origin;
                Vector2 toV2 = v2 - origin;
                return toV1.sqrMagnitude <= toV2.sqrMagnitude ? 1 : -1;
            });

            StartCoroutine(gotReward ? AnimateGood(trianglePositions) : AnimateBad(trianglePositions));
        }

        private IEnumerator AnimateGood(IEnumerable<Vector2> positions)
        {
            List<GameObject> spawned = new();
            foreach (Vector2 pos in positions)
            {
                Vector3 pos3 = pos.To3();

                if (indicatorPrefab != null)
                {
                    GameObject obj = Instantiate(indicatorPrefab, placeholdersParent);
                    obj.transform.position = pos3;
                    float originalScale = obj.transform.localScale.x;
                    obj.transform.localScale = Vector3.zero;
                    obj.transform.DOScale(originalScale, tweenInTime).SetEase(Ease.OutBack);
                    spawned.Add(obj);
                }

                int points = Game.Instance.GameMode.PointAwardValues.forEachEdgeInDistrict;
                Game.Instance.FeedbackText?.PutText($"+{points}", pos3);

                yield return new WaitForSeconds(delay);
            }

            foreach (GameObject obj in spawned)
            {
                obj.GetComponentInChildren<MeshRenderer>().material = goodMat;
            }

            yield return new WaitForSeconds(goodWaitTime);

            foreach (GameObject obj in spawned)
            {
                obj.transform.DOScale(0.0f, tweenOutTime).SetEase(Ease.InBack);

                yield return new WaitForSeconds(delay);
            }

            yield return new WaitForSeconds(tweenOutTime + 0.1f); // 0.1f just to be safe

            foreach (GameObject obj in spawned)
            {
                Destroy(obj);
            }
        }

        private IEnumerator AnimateBad(IEnumerable<Vector2> positions)
        {
            if (indicatorPrefab == null)
            {
                yield break;
            }

            List<GameObject> spawned = new();
            foreach (Vector2 pos in positions)
            {
                GameObject obj = Instantiate(indicatorPrefab, placeholdersParent);
                obj.transform.position = pos.To3();
                float originalScale = obj.transform.localScale.x;
                obj.transform.localScale = Vector3.zero;
                obj.transform.DOScale(originalScale, tweenInTime).SetEase(Ease.OutBack);
                spawned.Add(obj);

                yield return new WaitForSeconds(delay);
            }

            foreach (GameObject obj in spawned)
            {
                obj.GetComponentInChildren<MeshRenderer>().material = badMat;
            }

            yield return new WaitForSeconds(badWaitTime);

            foreach (GameObject obj in spawned)
            {
                obj.transform.DOScale(0.0f, tweenOutTime).SetEase(Ease.InBack);
            }

            yield return new WaitForSeconds(tweenOutTime + 0.1f); // 0.1f just to be safe

            foreach (GameObject obj in spawned)
            {
                Destroy(obj);
            }
        }
    }
}
