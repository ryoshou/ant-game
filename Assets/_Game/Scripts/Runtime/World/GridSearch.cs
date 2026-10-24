using System.Collections.Generic;
using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// Breadth-first searches over open tiles (4-neighbour). Buffers are preallocated per grid, so searches never
    /// allocate. Not thread-safe; one instance per simulation.
    /// </summary>
    public sealed class GridSearch
    {
        private enum Goal { CollectDigTargets, NextToDirt, Base }

        private static readonly Vector2Int[] Neighbours =
        {
            Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down,
        };

        private readonly DirtGrid grid;
        private readonly int[] queue;
        private readonly int[] parent;
        private readonly int[] visitedStamp;
        private readonly int[] candidateStamp;
        private int stamp;

        public GridSearch(DirtGrid grid)
        {
            this.grid = grid;
            int size = grid.Width * grid.Height;
            queue = new int[size];
            parent = new int[size];
            visitedStamp = new int[size];
            candidateStamp = new int[size];
        }

        /// <summary>
        /// Collects up to <paramref name="maxCandidates"/> distinct non-base dirt tiles that touch open ground
        /// reachable from <paramref name="start"/>, nearest first. Returns how many were found.
        /// </summary>
        public int FindDigTargets(Vector2Int start, int maxCandidates, List<Vector2Int> targetsOut)
        {
            targetsOut.Clear();
            if (maxCandidates <= 0) return 0;
            Run(start, Goal.CollectDigTargets, default, maxCandidates, targetsOut);
            return targetsOut.Count;
        }

        /// <summary>Shortest open path from <paramref name="from"/> to a tile next to <paramref name="dirtCell"/>.</summary>
        public bool FindPathToDig(Vector2Int from, Vector2Int dirtCell, List<Vector2Int> pathOut)
        {
            pathOut.Clear();
            int goal = Run(from, Goal.NextToDirt, dirtCell, 0, null);
            if (goal < 0) return false;
            BuildPath(from, goal, pathOut);
            return true;
        }

        /// <summary>Shortest open path from <paramref name="from"/> to the nearest base tile.</summary>
        public bool FindPathHome(Vector2Int from, List<Vector2Int> pathOut)
        {
            pathOut.Clear();
            int goal = Run(from, Goal.Base, default, 0, null);
            if (goal < 0) return false;
            BuildPath(from, goal, pathOut);
            return true;
        }

        public bool IsConnectedToBase(Vector2Int from) => Run(from, Goal.Base, default, 0, null) >= 0;

        /// <returns>The goal tile's index, or -1 (always -1 when collecting dig targets).</returns>
        private int Run(Vector2Int start, Goal goal, Vector2Int dirtCell, int maxCandidates, List<Vector2Int> targetsOut)
        {
            if (!grid.InBounds(start)) return -1;
            stamp++;
            int width = grid.Width;
            int head = 0, tail = 0;
            int startIndex = start.y * width + start.x;
            queue[tail++] = startIndex;
            visitedStamp[startIndex] = stamp;
            parent[startIndex] = -1;

            while (head < tail)
            {
                int index = queue[head++];
                var cell = new Vector2Int(index % width, index / width);
                if (IsGoal(goal, cell, dirtCell)) return index;

                for (int n = 0; n < Neighbours.Length; n++)
                {
                    Vector2Int next = cell + Neighbours[n];
                    if (!grid.InBounds(next)) continue;
                    int nextIndex = next.y * width + next.x;

                    if (!grid.IsOpen(next))
                    {
                        if (goal != Goal.CollectDigTargets || candidateStamp[nextIndex] == stamp) continue;
                        candidateStamp[nextIndex] = stamp;
                        targetsOut.Add(next);
                        if (targetsOut.Count >= maxCandidates) return -1;
                        continue;
                    }

                    if (visitedStamp[nextIndex] == stamp) continue;
                    visitedStamp[nextIndex] = stamp;
                    parent[nextIndex] = index;
                    queue[tail++] = nextIndex;
                }
            }
            return -1;
        }

        private bool IsGoal(Goal goal, Vector2Int cell, Vector2Int dirtCell)
        {
            switch (goal)
            {
                case Goal.NextToDirt: return Mathf.Abs(cell.x - dirtCell.x) + Mathf.Abs(cell.y - dirtCell.y) == 1;
                case Goal.Base: return grid.IsBase(cell);
                default: return false;
            }
        }

        private void BuildPath(Vector2Int from, int goalIndex, List<Vector2Int> pathOut)
        {
            int width = grid.Width;
            int fromIndex = from.y * width + from.x;
            for (int index = goalIndex; index != fromIndex && index >= 0; index = parent[index])
                pathOut.Add(new Vector2Int(index % width, index / width));
            pathOut.Reverse();
        }
    }
}
