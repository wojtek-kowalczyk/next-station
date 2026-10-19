using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace NS.Core
{
    public partial class RailNetwork
    {
        private readonly List<Vector2Int> stationCells;
        private readonly HexGrid grid;

        public IReadOnlyList<Vector2Int> StationCells => stationCells;
        public int StationCount => stationCells.Count;

        public RailNetwork(IReadOnlyList<Vector2Int> stationCells, HexGrid grid)
        {
            Assert.IsNotNull(grid);
            Assert.IsNotNull(stationCells);
            Assert.IsTrue(stationCells.Count > 0);

            this.grid = grid;
            this.stationCells = new List<Vector2Int>(stationCells);

            foreach (Vector2Int cell in stationCells)
            {
                Assert.IsTrue(grid.HasStation(cell));
            }
        }

        /// <summary>
        /// Balances supply and demand among stations. Changes state of the stations.
        /// Calling this multiple times will not change the state,
        /// unless a new station has been added to this network since the last call.
        /// </summary>
        public BalancingResult Balance()
        {
            if (stationCells.Count == 1)
            {
                return default;
            }

            List<BalancingResult.Pair> balancedPairs = new();

            for (int i = 0; i < stationCells.Count; i++)
            {
                for (int j = i + 1; j < stationCells.Count; j++)
                {
                    HexTileData tileA = grid.GetTile(stationCells[i]);
                    HexTileData tileB = grid.GetTile(stationCells[j]);

                    StationData stationA = tileA.stationData;
                    StationData stationB = tileB.stationData;

                    bool didTransfer = false;
                    didTransfer |= StationData.TryTransferResourcesTo(ref stationA, ref stationB);
                    didTransfer |= StationData.TryTransferResourcesTo(ref stationB, ref stationA);
                    if (didTransfer == false)
                    {
                        continue;
                    }

                    balancedPairs.Add(new BalancingResult.Pair
                        (stationCells[i], stationCells[j], stationA, stationB));

                    grid.ReplaceTile(stationCells[i], tileA.WithStation(stationA));
                    grid.ReplaceTile(stationCells[j], tileB.WithStation(stationB));
                }
            }

            return new BalancingResult(balancedPairs);
        }

        public readonly struct BalancingResult
        {
            public readonly struct Pair
            {
                public readonly Vector2Int station1Cell;
                public readonly Vector2Int station2Cell;
                public readonly StationData newStation1Data;
                public readonly StationData newStation2Data;

                public Pair(Vector2Int station1Cell, Vector2Int station2Cell,
                    StationData newStation1Data, StationData newStation2Data)
                {
                    this.station1Cell = station1Cell;
                    this.station2Cell = station2Cell;
                    this.newStation1Data = newStation1Data;
                    this.newStation2Data = newStation2Data;
                }
            }

            public readonly List<Pair> pairs;

            public BalancingResult(IEnumerable<Pair> pairs)
            {
                this.pairs = new List<Pair>(pairs);
            }

            public bool IsEmpty => pairs == null || pairs.Count == 0;
        }

        protected bool Equals(RailNetwork other)
        {
            if (stationCells.Count != other.stationCells.Count)
            {
                return false;
            }

            foreach (Vector2Int cell in stationCells)
            {
                // stations are identified by the cells they're on
                if (other.stationCells.Contains(cell) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((RailNetwork)obj);
        }

        public override int GetHashCode()
        {
            throw new InvalidOperationException("Should not be hashed");
        }

        public static SupplyInfo GetSupplyInfo(IReadOnlyList<RailNetwork> railNetworks)
        {
            if (railNetworks == null || railNetworks.Count == 0)
            {
                return default;
            }

            ResourceType[] givesResources = ResourceTypeExtensions.ResourceTypes().ToArray();
            int[] givesAmounts = new int[givesResources.Length];

            ValidateRailNetworksAreOnOneGrid(); // just info, don't exit early if failed

            foreach (RailNetwork railNetwork in railNetworks)
            {
                foreach (Vector2Int stationCell in railNetwork.stationCells)
                {
                    StationData stationData = railNetwork.grid.GetTile(stationCell).stationData;

                    for (int i = 0; i < givesResources.Length; i++)
                    {
                        if (givesResources[i] == stationData.givesResource)
                            givesAmounts[i] += stationData.givesResourceAmount;
                        if (givesResources[i] == stationData.wantsResource)
                            givesAmounts[i] -= stationData.wantsResourceAmount;
                    }
                }
            }

            for (int i = 0; i < givesAmounts.Length; i++)
            {
                givesAmounts[i] = Mathf.Max(0, givesAmounts[i]);
            }

            return new SupplyInfo(givesResources, givesAmounts);

            void ValidateRailNetworksAreOnOneGrid()
            {
                HexGrid lastGrid = null;
                foreach (RailNetwork railNetwork in railNetworks)
                {
                    lastGrid ??= railNetwork.grid;
                    if (!ReferenceEquals(railNetwork.grid, lastGrid))
                    {
                        Debug.LogWarning("[RailNetwork] Computing supply info of rail networks on different grids!");
                    }
                }
            }
        }
    }
}
