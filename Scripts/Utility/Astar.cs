// Source: https://xfleury.github.io/graphsearch.html
// License: CC0 as stated in https://www.reddit.com/r/gamedev/comments/2zldhz/how_i_implemented_a_pathfinding_in_c_for_my_indie/
// for example usage/implementation see source.

using System;
using System.Collections.Generic;

namespace NS.Utility
{
    public class PriorityQueue<T> where T : IComparable<T>
    {
        private readonly List<T> items = new List<T>();

        public int Count => items.Count;

        public void Clear()
        {
            items.Clear();
        }

        public void Insert(T item)
        {
            int i = items.Count;
            items.Add(item);
            while (i > 0 && items[(i - 1) / 2].CompareTo(item) > 0)
            {
                items[i] = items[(i - 1) / 2];
                i = (i - 1) / 2;
            }

            items[i] = item;
        }

        public T Peek()
        {
            return items[0];
        }

        public T RemoveRoot()
        {
            T firstItem = items[0];
            T tempItem = items[items.Count - 1];
            items.RemoveAt(items.Count - 1);
            if (items.Count > 0)
            {
                int i = 0;
                while (i < items.Count / 2)
                {
                    int j = (2 * i) + 1;
                    if ((j < items.Count - 1) && (items[j].CompareTo(items[j + 1]) > 0)) ++j;
                    if (items[j].CompareTo(tempItem) >= 0) break;
                    items[i] = items[j];
                    i = j;
                }

                items[i] = tempItem;
            }

            return firstItem;
        }
    }

    public abstract class AStar<TPosition, TCost> where TCost : IComparable<TCost>
    {
        protected void Graph(Node start, PriorityQueue<Node> openList, Dictionary<TPosition, TCost> closedList)
        {
            openList.Insert(start);
            while (openList.Count > 0)
            {
                Node node = openList.RemoveRoot();
                if (closedList.ContainsKey(node.position)) continue;
                closedList.Add(node.position, node.cost);
                if (IsDestination(node.position)) return;
                AddNeighbours(node, openList);
            }
        }

        protected abstract void AddNeighbours(Node node, PriorityQueue<Node> openList);

        // return true if the presense of node in the closed list completes the search
        protected abstract bool IsDestination(TPosition position);

        public struct Node : IComparable<Node>
        {
            public TPosition position;
            public TCost cost;

            public Node(TPosition position, TCost cost)
            {
                this.position = position;
                this.cost = cost;
            }

            public int CompareTo(Node other)
            {
                return cost.CompareTo(other.cost);
            }
        }
    }
}
