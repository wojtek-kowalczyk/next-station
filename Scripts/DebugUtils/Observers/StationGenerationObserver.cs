using System;
using System.Collections.Generic;
using NS.Core.GameModes.Basic;
using UnityEngine;
using UnityEngine.Assertions;

namespace NS.DebugUtils.Observers
{
    public class StationGenerationObserver : IDisposable
    {
        private readonly StationSpawner observedSpawner;

        public List<Vector2Int> LastGenerationCandidateCells { get; init; }
        public int NumPlacedTilesAtTimeOfGeneration { get; private set; }

        public StationGenerationObserver(StationSpawner spawner)
        {
            Assert.IsNotNull(spawner);

            LastGenerationCandidateCells = new();

            observedSpawner = spawner;
            observedSpawner.CandidateCellsGenerated += OnCandidateCellsGenerated;
        }

        public void Dispose()
        {
            observedSpawner.CandidateCellsGenerated -= OnCandidateCellsGenerated;
        }

        private void OnCandidateCellsGenerated(IReadOnlyList<Vector2Int> cells)
        {
            LastGenerationCandidateCells.Clear();
            LastGenerationCandidateCells.AddRange(cells);

            NumPlacedTilesAtTimeOfGeneration = Game.Instance.Grid.TileCount;
        }
    }
}
