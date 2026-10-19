using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NS.Core;

namespace NS.DebugUtils.Observers
{
    public class RailGraphBuildObserver : IDisposable
    {
        [CanBeNull]
        public List<RailGraphBuilder.RailGraph> LastRailGraphs { get; private set; }

        public RailGraphBuildObserver()
        {
            RailGraphBuilder.RailGraphsBuilt += OnRailGraphsBuilt;
        }

        public void Dispose()
        {
            RailGraphBuilder.RailGraphsBuilt -= OnRailGraphsBuilt;
        }

        private void OnRailGraphsBuilt(List<RailGraphBuilder.RailGraph> searchResult)
        {
            LastRailGraphs = searchResult;
        }
    }
}
