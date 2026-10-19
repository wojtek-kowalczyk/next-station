using System;
using System.Collections.Generic;
using NS.Utility;
using UnityEngine;
using UnityEngine.Pool;

namespace NS.Core
{
    public static class RailGraphBuilder
    {
        public static event Action<List<RailGraph>> RailGraphsBuilt;

        public static List<RailGraph> BuildRailGraphs(HexGrid grid)
        {
            var results = new List<RailGraph>();

            var visitedCells = new HashSet<Vector2Int>();
            var cellsToVisit = new Queue<Vector2Int>();

            using var po1 = ListPool<Vector2Int>.Get(out var stations);
            grid.GetStationTiles(stations);
            IEnumerator<Vector2Int> stationsEnumerator = stations.GetEnumerator();

            while(stationsEnumerator.MoveNext())
            {
                var graph = new RailGraph();

                cellsToVisit.Enqueue(stationsEnumerator.Current);

                while (cellsToVisit.Count > 0)
                {
                    Vector2Int cell = cellsToVisit.Dequeue();
                    if (visitedCells.Add(cell) == false)
                    {
                        continue;
                    }

                    graph.visitedCellsInOrder ??= new List<Vector2Int>();
                    graph.visitedCellsInOrder.Add(cell);

                    grid.TryGetTile(cell, out HexTileData tile);
                    if (tile.IsStation)
                    {
                        graph.connectedStations.Add(cell);
                    }

                    using var po2 = ListPool<Vector2Int>.Get(out var neighbors);
                    grid.GetNeighborsWithRailConnection(cell, neighbors);
                    foreach (Vector2Int neighborCell in neighbors)
                    {
                        cellsToVisit.Enqueue(neighborCell);
                    }
                }

                // It's possible that we produce no connected stations if a station has been connected already
                // to some other network, and we start iterating from it after finishing some previous network.
                if (graph.connectedStations.Count > 0)
                {
                    results.Add(graph);
                }
            }

            RailGraphsBuilt?.Invoke(results);

            return results;
        }

        public static Path? FindPath(HexGrid grid, Vector2Int from, Vector2Int to)
        {
            if (from == to)
            {
                return null;
            }

            var solver = new Solver(grid);
            var openList = new PriorityQueue<AStar<Vector2Int, Cost>.Node>();
            var closedList = new Dictionary<Vector2Int, Cost>();
            solver.FindPath(from, to, openList, closedList);
            if (solver.Solution.HasValue == false)
            {
                Debug.LogWarning($"[RailGraphBuilder] No path found between {from} and {to}");
                return null;
            }

            // reconstruct path
            List<Vector2Int> path = new();
            Vector2Int pos = solver.Solution.Value.position;
            Cost cost = solver.Solution.Value.cost;
            path.Add(pos);
            do {
                pos = cost.parentCell;
                cost = closedList[pos];
                path.Add(pos);
            }
            while (cost.parentCell != Cost.NoParent);

            path.Reverse();

            return new Path(path);
        }

        public class RailGraph
        {
            public readonly List<Vector2Int> connectedStations = new(); // cells with stations that are connected to the network
            public List<Vector2Int> visitedCellsInOrder; // debug data
        }

        public readonly struct Path
        {
            public readonly List<Vector2Int> cells;

            public Path(List<Vector2Int> cells) => this.cells = cells;
            public override string ToString() => string.Join(", ", cells);
        }

        private readonly struct Cost : IComparable<Cost>
        {
            public static readonly Vector2Int NoParent = new(int.MaxValue, int.MaxValue); // unlikely sentinel value

            public readonly Vector2Int parentCell;
            public readonly int distanceTravelled;
            private readonly int totalCost;

            public Cost(Vector2Int parentCell, int distanceTravelled, int totalCost)
            {
                this.parentCell = parentCell;
                this.distanceTravelled = distanceTravelled;
                this.totalCost = totalCost;
            }

            public int CompareTo(Cost other)
            {
                return this.totalCost.CompareTo(other.totalCost);
            }
        }

        private class Solver : AStar<Vector2Int, Cost>
        {
            private readonly HexGrid grid;

            private Vector2Int destination;
            private Dictionary<Vector2Int, Cost> closedList;

            public Node? Solution { get; private set; }

            public Solver(HexGrid grid)
            {
                this.grid = grid;
            }

            // ReSharper disable once ParameterHidesMember
            public void FindPath(Vector2Int from, Vector2Int to, PriorityQueue<Node> openList, Dictionary<Vector2Int, Cost> closedList)
            {
                this.closedList = closedList;
                destination = to;
                Graph(new Node(from, new Cost(Cost.NoParent, 0, GetDistance(from, to))), openList, closedList);
            }

            protected override void AddNeighbours(Node node, PriorityQueue<Node> openList)
            {
                Vector2Int cell = node.position;
                using var po = ListPool<Vector2Int>.Get(out var neighbors);
                grid.GetNeighborsWithRailConnection(cell, neighbors);
                foreach (Vector2Int neighborCell in neighbors)
                {
                    int costSoFar = node.cost.distanceTravelled + 1; // step cost is always 1 in our case
                    int estimatedTotalCost = costSoFar + GetDistance(neighborCell, destination);
                    openList.Insert(new Node(neighborCell, new Cost(cell, costSoFar, estimatedTotalCost)));
                }
            }

            private static int GetDistance(Vector2Int a, Vector2Int b)
            {
                // return Mathf.Abs(a.x - b.x) + Mathf.Abs(b.x - b.y); // manhattan
                return Mathf.RoundToInt((a - b).magnitude); // euclidean
            }

            protected override bool IsDestination(Vector2Int position)
            {
                bool isSolved = position == destination;
                if (isSolved) Solution = new Node(position, closedList[position]);
                return isSolved;
            }
        }
    }
}
