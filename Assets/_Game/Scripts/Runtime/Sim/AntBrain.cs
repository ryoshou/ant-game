using Game.Core;
using UnityEngine;

namespace Game.Sim
{
    /// <summary>One ant's behaviour per tick. Stateless; all state lives on <see cref="Ant"/>.</summary>
    internal static class AntBrain
    {
        private const int WanderRadiusTiles = 3;

        private enum WalkResult { Moving, Arrived, Blocked }

        private static readonly Vector2Int[] Neighbours =
        {
            Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down,
        };

        public static void Step(Ant ant, float dt, Colony colony)
        {
            switch (ant.State)
            {
                case AntState.ToDig: StepToDig(ant, dt, colony); break;
                case AntState.Digging: StepDigging(ant, dt, colony); break;
                case AntState.Returning: StepReturning(ant, dt, colony); break;
                case AntState.Depositing: StepDepositing(ant, dt, colony); break;
                case AntState.Idle: StepIdle(ant, dt, colony); break;
                case AntState.DiggingOut: StepDiggingOut(ant, dt, colony); break;
            }
        }

        private static void StepToDig(Ant ant, float dt, Colony colony)
        {
            if (ant.NeedsPlan && !PlanDig(ant, colony))
            {
                EnterIdle(ant, colony);
                return;
            }

            if (colony.Grid.IsOpen(ant.Target))
            {
                ant.NeedsPlan = true;
                return;
            }

            switch (Walk(ant, dt, colony))
            {
                case WalkResult.Blocked: ant.NeedsPlan = true; break;
                case WalkResult.Arrived: EnterDigging(ant); break;
            }
        }

        private static bool PlanDig(Ant ant, Colony colony)
        {
            Vector2Int cell = colony.CellOf(ant);
            int found = colony.Search.FindDigTargets(cell, colony.Config.digCandidates, colony.Candidates);
            if (found == 0) return false;

            ant.Target = colony.Candidates[colony.Random.Next(found)];
            if (!colony.Search.FindPathToDig(cell, ant.Target, ant.Path)) return false;
            ant.PathIndex = 0;
            ant.NeedsPlan = false;
            ant.TripTiles = ant.Path.Count + 1;
            return true;
        }

        private static void EnterDigging(Ant ant)
        {
            Vector2Int target = ant.Target;
            ant.Enter(AntState.Digging);
            ant.Target = target;
        }

        private static void StepDigging(Ant ant, float dt, Colony colony)
        {
            Vector2 toTarget = colony.CenterOf(ant.Target) - ant.Position;
            ant.Heading = Mathf.Atan2(toTarget.y, toTarget.x);

            int carry = Mathf.RoundToInt(colony.Upgrades.Value(UpgradeId.CarryAmount));
            if (colony.Grid.IsOpen(ant.Target) || ant.Carrying >= carry)
            {
                FinishDigging(ant);
                return;
            }

            ant.DigProgress += colony.Upgrades.Value(UpgradeId.DigSpeed) * dt;
            int wanted = (int)ant.DigProgress;
            if (wanted == 0) return;
            ant.DigProgress -= wanted;
            ant.Carrying += colony.Grid.Dig(ant.Target, Mathf.Min(wanted, carry - ant.Carrying));

            if (ant.Carrying >= carry || colony.Grid.IsOpen(ant.Target)) FinishDigging(ant);
        }

        private static void FinishDigging(Ant ant) =>
            ant.Enter(ant.Carrying > 0 ? AntState.Returning : AntState.ToDig);

        private static void StepReturning(Ant ant, float dt, Colony colony)
        {
            if (ant.NeedsPlan)
            {
                if (!colony.Search.FindPathHome(colony.CellOf(ant), ant.Path))
                {
                    ant.Enter(AntState.DiggingOut);
                    return;
                }
                ant.PathIndex = 0;
                ant.NeedsPlan = false;
            }

            switch (Walk(ant, dt, colony))
            {
                case WalkResult.Blocked: ant.NeedsPlan = true; break;
                case WalkResult.Arrived:
                    ant.Enter(AntState.Depositing);
                    ant.Timer = colony.Config.depositSeconds;
                    break;
            }
        }

        private static void StepDepositing(Ant ant, float dt, Colony colony)
        {
            ant.Timer -= dt;
            if (ant.Timer > 0f) return;
            colony.CompleteDeposit(ant);
            ant.Enter(AntState.ToDig);
        }

        private static void EnterIdle(Ant ant, Colony colony)
        {
            ant.Enter(AntState.Idle);
            ant.Timer = colony.Config.idleRecheckSeconds;
        }

        private static void StepIdle(Ant ant, float dt, Colony colony)
        {
            ant.Timer -= dt;
            if (ant.Timer <= 0f)
            {
                ant.Timer = colony.Config.idleRecheckSeconds;
                if (colony.Search.FindDigTargets(colony.CellOf(ant), 1, colony.Candidates) > 0)
                {
                    ant.Enter(AntState.ToDig);
                    return;
                }
            }

            if (ant.PathIndex >= ant.Path.Count) PlanWander(ant, colony);
            if (Walk(ant, dt, colony) == WalkResult.Blocked) ant.Path.Clear();
        }

        /// <summary>Head home when far from the base, otherwise take one random open step near it.</summary>
        private static void PlanWander(Ant ant, Colony colony)
        {
            ant.Path.Clear();
            ant.PathIndex = 0;
            Vector2Int cell = colony.CellOf(ant);
            if (DistanceFromBase(cell, colony) > WanderRadiusTiles)
            {
                colony.Search.FindPathHome(cell, ant.Path);
                return;
            }

            int start = colony.Random.Next(Neighbours.Length);
            for (int i = 0; i < Neighbours.Length; i++)
            {
                Vector2Int next = cell + Neighbours[(start + i) % Neighbours.Length];
                if (!colony.Grid.IsOpen(next) || DistanceFromBase(next, colony) > WanderRadiusTiles) continue;
                ant.Path.Add(next);
                return;
            }
        }

        private static void StepDiggingOut(Ant ant, float dt, Colony colony)
        {
            Vector2Int cell = colony.CellOf(ant);
            // Connectivity only grows when a tile opens, so search again only after one has.
            if (ant.ConnectivityCheckedAt != colony.Grid.OpenedTiles)
            {
                ant.ConnectivityCheckedAt = colony.Grid.OpenedTiles;
                if (colony.Search.IsConnectedToBase(cell))
                {
                    ant.Enter(AntState.Returning);
                    return;
                }
            }

            Vector2Int next = StepTowardBase(cell, colony);
            if (colony.Grid.IsOpen(next))
            {
                if (ant.PathIndex >= ant.Path.Count || ant.Path[ant.PathIndex] != next)
                {
                    ant.Path.Clear();
                    ant.Path.Add(next);
                    ant.PathIndex = 0;
                }
                Walk(ant, dt, colony);
                return;
            }

            ant.DigProgress += colony.Upgrades.Value(UpgradeId.DigSpeed) * dt;
            int wanted = (int)ant.DigProgress;
            if (wanted == 0) return;
            ant.DigProgress -= wanted;
            int carry = Mathf.RoundToInt(colony.Upgrades.Value(UpgradeId.CarryAmount));
            int removed = colony.Grid.Dig(next, wanted);
            ant.Carrying = Mathf.Min(carry, ant.Carrying + removed);
        }

        /// <summary>The in-bounds neighbour closest (Manhattan) to the base centre — always one step closer.</summary>
        private static Vector2Int StepTowardBase(Vector2Int cell, Colony colony)
        {
            Vector2Int goal = colony.Grid.BaseCenter;
            Vector2Int best = cell;
            int bestDistance = int.MaxValue;
            for (int i = 0; i < Neighbours.Length; i++)
            {
                Vector2Int next = cell + Neighbours[i];
                if (!colony.Grid.InBounds(next)) continue;
                int distance = Mathf.Abs(next.x - goal.x) + Mathf.Abs(next.y - goal.y);
                if (distance >= bestDistance) continue;
                best = next;
                bestDistance = distance;
            }
            return best;
        }

        private static int DistanceFromBase(Vector2Int cell, Colony colony)
        {
            RectInt home = colony.Grid.BaseRect;
            int dx = Mathf.Max(0, Mathf.Max(home.xMin - cell.x, cell.x - (home.xMax - 1)));
            int dy = Mathf.Max(0, Mathf.Max(home.yMin - cell.y, cell.y - (home.yMax - 1)));
            return Mathf.Max(dx, dy);
        }

        /// <summary>Moves along the ant's path at walk speed; stops if the next tile has been filled.</summary>
        private static WalkResult Walk(Ant ant, float dt, Colony colony)
        {
            float budget = colony.Upgrades.Value(UpgradeId.WalkSpeed) * dt;
            while (ant.PathIndex < ant.Path.Count)
            {
                Vector2Int nextCell = ant.Path[ant.PathIndex];
                if (!colony.Grid.IsOpen(nextCell)) return WalkResult.Blocked;

                Vector2 delta = colony.CenterOf(nextCell) - ant.Position;
                float distance = delta.magnitude;
                if (distance > 1e-5f) ant.Heading = Mathf.Atan2(delta.y, delta.x);
                if (distance > budget)
                {
                    ant.Position += delta * (budget / distance);
                    return WalkResult.Moving;
                }

                ant.Position = colony.CenterOf(nextCell);
                budget -= distance;
                ant.PathIndex++;
            }
            return WalkResult.Arrived;
        }
    }
}
