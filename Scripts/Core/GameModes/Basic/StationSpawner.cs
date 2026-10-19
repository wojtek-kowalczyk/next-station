using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NS.Core.Blueprints;
using NS.Core.Model;
using NS.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace NS.Core.GameModes.Basic
{
    public class StationSpawner : MonoBehaviour
    {
        /// Given List of cells is only valid/alive for the duration of the call. Copy it away if you need to store it.
        public event Action<IReadOnlyList<Vector2Int>> CandidateCellsGenerated;
        public event Action StationAwarded;
        public delegate void ProgressChangedHandler(float newProgress01, bool wrappedAround, bool becauseOfPlacement);
        public event ProgressChangedHandler ProgressChanged;

        [SerializeField] private HexTileDataBlueprintSO stationBp;
        [SerializeField] private int innerRadiusIncl;
        [SerializeField] private int outerRadiusIncl;
        [SerializeField] private int dilateInnerRadiusIncl = 2;
        [SerializeField] private int dilateOuterRadiusIncl = 4;
        [SerializeField] private int distanceFromOtherTiles;
        [SerializeField] private int distanceFromOtherStations;
        [SerializeField] private int minCandidateCellsRequired = 5;
        [SerializeField] private float givesGoldProbability = 0.25f;
        [SerializeField] private List<int> pointThresholds;

        public float CurrentProgress01 { get; private set; }

        private void OnEnable()
        {
            Game.Instance.ModelChanged += OnGameModelChanged;
        }

        private void OnDisable()
        {
            Game.Instance.ModelChanged -= OnGameModelChanged;
        }

        private void OnGameModelChanged(GameModel oldModel, GameModel newModel, Game.ModelChangeReason reason)
        {
            int oldIndex = GetThresholdIndex(oldModel.Points);
            int newIndex = GetThresholdIndex(newModel.Points);
            float newProgress = GetStationAwardProgress01(newModel.Points, newIndex);

            if (Mathf.Approximately(CurrentProgress01, newProgress) == false)
            {
                CurrentProgress01 = newProgress;
                ProgressChanged?.Invoke(CurrentProgress01, oldIndex != newIndex, reason == Game.ModelChangeReason.PlaceTile);
            }
        }

        /// Note: this only supports changing game state "forward". Undo is expected to reload the whole state.
        public void OnPointsAdded(GameModel model, int oldValue, int newValue, IReadOnlyList<RailNetwork> railNetworks, Random rand)
        {
            Assert.IsTrue(oldValue != newValue); // wouldn't break logic, but shouldn't happen

            int oldIndex = GetThresholdIndex(oldValue);
            int newIndex = GetThresholdIndex(newValue);

            CurrentProgress01 = GetStationAwardProgress01(newValue, newIndex);
            ProgressChanged?.Invoke(CurrentProgress01, oldIndex != newIndex, true);

            if (oldIndex == newIndex)
            {
                return;
            }

            int toAward = newIndex - oldIndex;
            for (int i = 0; i < toAward; i++)
            {
                AwardStation(model, railNetworks, rand);
            }

            // Note:
            // we're never removing stations here because awarded stations are in game model
            // and only way to lose points here is to reload the model, so the stations are
            // already missing from the new model.
        }

        public void AwardStation(GameModel model, IReadOnlyList<RailNetwork> railNetworks, Random rand)
        {
            RailNetwork.SupplyInfo supplyInfo = RailNetwork.GetSupplyInfo(railNetworks);
            var stationDataBp = GetStationDataBlueprint(supplyInfo, rand);
            HexTileData stationTile = stationBp.Blueprint.Generate(rand, stationDataBp);
            model.TileStack.PushNextTile(stationTile);
            StationAwarded?.Invoke();

            Debug.Log($"[StationSpawner] Awarded station: {stationDataBp.Generate(new Random())}. Supply info: {supplyInfo}");
        }

        public StationDataBlueprint GetStationDataBlueprint(RailNetwork.SupplyInfo supplyInfo, Random rand)
        {
            float[] wantsProbabilities = new float[ResourceTypeExtensions.RESOURCE_TYPE_COUNT];
            ResourceType[] resourceTypes = ResourceTypeExtensions.ResourceTypes().ToArray();
            Assert.IsTrue(wantsProbabilities.Length == resourceTypes.Length);

            for (int i = 0; i < resourceTypes.Length; i++)
            {
                // the probability that a station will want something is proportional
                // to the probability that a random station gives it.
                wantsProbabilities[i] = Mathf.Clamp01(supplyInfo.GivesProb01(resourceTypes[i]));
            }

            // special case: if rail networks give nothing we can want whatever basic resource (not gold)
            if (supplyInfo.GivesNothing())
            {
                for (int i = 0; i < resourceTypes.Length; i++)
                {
                    wantsProbabilities[i] = resourceTypes[i] == ResourceType.Gold? 0 : 1;
                }
            }

            ResourceType wantsResource = resourceTypes[Utils.WeightedSample(wantsProbabilities, rand)];
            int wantAmount = GetWantAmount(supplyInfo.Gives(wantsResource), rand);
            Assert.IsTrue(wantAmount > 0);

            // TODO : currently no station can give gold apart from the ones generated here, so this is always false
            bool giveGold = supplyInfo.Gives(ResourceType.Gold) > 0 && rand.NextFloat() < givesGoldProbability;

            return new StationDataBlueprint(
                wants: wantsResource,
                wantsAmount: wantAmount,
                gives: giveGold? ResourceType.Gold : ResourceType.None,
                givesAmount: giveGold? 1 : 0
            );
        }

        private static int GetWantAmount(int gives, Random rand)
        {
            return gives switch
            {
                <= 2 => rand.NextInt(minInclusive: 1, maxExclusive: 3),
                <= 4 => rand.NextInt(minInclusive: 3, maxExclusive: 6),
                _ => rand.NextInt(minInclusive: 5, maxExclusive: 7)
            };
        }

        // returns the index of pointThreshold that is the current one to be achieved
        private int GetThresholdIndex(int numPoints)
        {
            int idx = 0;
            foreach (int threshold in pointThresholds)
            {
                if (numPoints >= threshold)
                {
                    idx++;
                }
            }

            return idx;
        }

        public bool GenerateCandidateCells(HexGrid grid, Vector2Int centerCell, List<Vector2Int> outCells = null)
        {
            if (GameplaySettingsManager.Instance?.Get(GameplaySettingsManager.Setting.StationPlacementAllAroundIsland) ?? true)
            {
                return GenerateCandidateCellsAllAroundGrid(grid, outCells);
            }
            else
            {
                return GenerateCandidateCellsAroundCenterCell(grid, centerCell, outCells);
            }
        }

        private bool GenerateCandidateCellsAroundCenterCell(HexGrid grid, Vector2Int centerCell, List<Vector2Int> outCells)
        {
            using var po1 = ListPool<Vector2Int>.Get(out var cells);
            using var po2 = ListPool<Vector2Int>.Get(out var tempList);

            for (int increment = 0; increment < 50; increment++)
            {
                cells.Clear();
                grid.Math.GetCellsInRange(centerCell, innerRadiusIncl, outerRadiusIncl + increment, cells);

                tempList.Clear();
                tempList.AddRange(cells.Where(cell => grid.HasTile(cell) == false));
                (cells, tempList) = (tempList, cells);

                tempList.Clear();
                tempList.AddRange(cells.Where(cell => grid.IsAnyCellWithin(cell, distanceFromOtherTiles) == false));
                (cells, tempList) = (tempList, cells);

                tempList.Clear();
                tempList.AddRange(cells.Where(cell => grid.IsAnyStationWithin(cell, distanceFromOtherStations) == false));
                (cells, tempList) = (tempList, cells);

                if (cells.Count > minCandidateCellsRequired)
                {
                    break;
                }
            }

            outCells?.AddRange(cells);
            CandidateCellsGenerated?.Invoke(new List<Vector2Int>(cells));

            return true;
        }

        private bool GenerateCandidateCellsAllAroundGrid(HexGrid grid, [CanBeNull] List<Vector2Int> outCells)
        {
            using var po1 = HashSetPool<Vector2Int>.Get(out var inner);
            using var po2 = HashSetPool<Vector2Int>.Get(out var outer);

            grid.Dilate(dilateInnerRadiusIncl, inner);
            grid.Dilate(dilateOuterRadiusIncl, outer);

            outer.ExceptWith(inner);

            outCells?.AddRange(outer);

            CandidateCellsGenerated?.Invoke(new List<Vector2Int>(outer));

            return true;
        }

        public float GetStationAwardProgress01(int currPoints, int currThresholdIdx)
        {
            if (currThresholdIdx < 0)
                return 0.0f;
            if (currThresholdIdx > pointThresholds.Count - 1)
                return 1.0f;

            int currThresholdPoints = pointThresholds[currThresholdIdx];
            int prevThresholdPoints = currThresholdIdx == 0 ? 0 : pointThresholds[currThresholdIdx - 1];

            return Mathf.Clamp01((float)(currPoints - prevThresholdPoints) / (currThresholdPoints - prevThresholdPoints));
        }
    }
}
