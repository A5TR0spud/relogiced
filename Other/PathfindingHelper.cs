using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Relogiced.Other;

public class Node
{
    public Point P;
    public int? G;
    public int? Taxi;
    public int? H;
    public Node Parent;
    public int? F => G + H;
    public List<Node> Neighbors;
}

public class PathfindingHelper : ModSystem
{
    public enum DistanceMode
    {
        EuclidianSquared,
        Taxicab,
        Chebyshev,
        Euclidian,
    }
    public enum FailureMode
    {
        StraightToTarget,
        PathToBest,
        DoNothing
    }

    public static float Distance(Point from, Point to, DistanceMode distanceMode = DistanceMode.EuclidianSquared)
    {
        switch (distanceMode)
        {
            case DistanceMode.Chebyshev:
            {
                return Math.Max(Math.Abs(from.X - to.X), Math.Abs(from.Y - to.Y));
            }
            case DistanceMode.Taxicab:
            {
                return Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
            }
            case DistanceMode.Euclidian:
            {
                return (float)Math.Sqrt((from.X - to.X) * (from.X - to.X) + (from.Y - to.Y) * (from.Y - to.Y));
            }
            default:
            {
                return (from.X - to.X) * (from.X - to.X) + (from.Y - to.Y) * (from.Y - to.Y);
            }
        }
    }
    
    public static bool Collide(Point p, int width = 1, int height = 1)
    {
        return Collision.SolidTiles(p.X, p.X + width - 1, p.Y, p.Y + height - 1);
    }

    /*public static List<Point> CleanPath(Entity entity, List<Point> path)
    {
        int idx;
        do
        {
            idx = NextStepIndex(path[0].ToWorldCoordinates(-entity.width / 2f, -entity.height / 2f), entity.width, entity.height, path);
            path.RemoveRange(0, idx);
        } while (idx != 0);

        return path;
    }*/
    
    public static int NextStepIndex(Entity entity, List<Point> path)
    {
        if (path == null || path.Count == 0) return -1;

        int originIndex = path.FindLastIndex(i => entity.Hitbox.Intersects(new Rectangle(
            i.ToWorldCoordinates(0, 0).ToPoint().X, i.ToWorldCoordinates(0, 0).ToPoint().Y,
            16, 16
            )) || entity.Center.ToTileCoordinates() == i);
        
        int nextStepIndex = path.FindLastIndex(i => 
            Collision.CanHit(entity.TopLeft, 0, 0,
                i.ToWorldCoordinates(8 - entity.width / 2f, 8 - entity.width / 2f), 0, 0) &&
            Collision.CanHit(entity.TopRight, 0, 0,
                i.ToWorldCoordinates(8 + entity.width / 2f, 8 - entity.width / 2f), 0, 0) &&
            Collision.CanHit(entity.BottomLeft + new Vector2(0, entity.height), 0, 0,
                i.ToWorldCoordinates(8 - entity.width / 2f, 8 + entity.width / 2f), 0, 0) &&
            Collision.CanHit(entity.BottomRight + new Vector2(entity.width, entity.height), 0, 0,
                i.ToWorldCoordinates(8 + entity.width / 2f, 8 + entity.width / 2f), 0, 0));
        nextStepIndex = Math.Max(originIndex + 1, nextStepIndex);
        if (nextStepIndex == -1) nextStepIndex = 0;
        if (nextStepIndex >= path.Count)
            nextStepIndex = path.Count - 1;
        return nextStepIndex;
    }

    public static Point? NextStep(Entity entity, List<Point> path)
    {
        if (path == null || path.Count == 0) return null;
        return path[NextStepIndex(entity, path)];
    }

    public static List<Point> AStarIterative(Point startLocation, Point endLocation,
        ref List<Node> openNodes, ref List<Node> closedNodes, out bool isComplete,
        int maxLength = 200, int maxTilesToCheck = 1000, int width = 16, int height = 16,
        DistanceMode distanceMode = DistanceMode.Taxicab, FailureMode failureMode = FailureMode.DoNothing,
        Func<Point, Point, int?> costFunction = null,
        List<Point> moveSet = null
        )
    {
        costFunction ??= (_, to) => Collide(to, (int)Math.Ceiling(width / 16f), (int)Math.Ceiling(height / 16f)) ? null : 10;
        if (costFunction(startLocation, startLocation) == null || costFunction(endLocation, endLocation) == null)
        {
            isComplete = false;
            if (failureMode == FailureMode.DoNothing)
            {
                return [];
            }

            return [endLocation];
        }
        if (startLocation == endLocation)
        {
            isComplete = true;
            return [endLocation];
        }
        if (Distance(startLocation, endLocation, distanceMode) > (distanceMode == DistanceMode.EuclidianSquared ? maxLength * maxLength : maxLength))
        {
            isComplete = false;
            return [endLocation];
        }
        if (maxTilesToCheck <= 0)
        {
            if (failureMode == FailureMode.DoNothing)
            {
                isComplete = false;
                return [];
            }

            isComplete = false;
            return [endLocation];
        }
        moveSet ??= [new Point(0, 1), new Point(0, -1), new Point(1, 0), new Point(-1, 0)];

        openNodes ??= [];
        closedNodes ??= [];
        
        CheckPos(null, startLocation, out Node startNode, ref openNodes, ref closedNodes);
        startNode.G = 0;
        startNode.Taxi = 0;
        Node endNode = null;

        if (openNodes.Count == 0 && closedNodes.Count == 0)
            openNodes = [startNode];

        while (openNodes.Count > 0 && maxTilesToCheck > 0)
        {
            maxTilesToCheck--;
            Node current = openNodes[0];
            foreach (Node t in openNodes)
            {
                if (t.F < current.F || t.F == current.F && t.H < current.H)
                    current = t;
            }

            /*if (Main.rand.NextBool(20))
                Dust.NewDustPerfect(current.P.ToWorldCoordinates(), DustID.Clentaminator_Green,
                    Vector2.Zero);*/
            
            closedNodes.Add(current);
            openNodes.Remove(current);

            if (current.P == endLocation)
            {
                endNode = current;
                break;
            }

            foreach (Node neighbor in ValidNeighbors(current, ref openNodes, ref closedNodes))
            {
                if (closedNodes.Contains(neighbor))
                    continue;
                bool inSearch = openNodes.Contains(neighbor);

                int? gCostToNeighbor = current.G + costFunction(current.P, neighbor.P);

                if (gCostToNeighbor < neighbor.G)
                {
                    neighbor.G = gCostToNeighbor;
                    neighbor.Taxi = current.Taxi + 1;
                    neighbor.Parent = current;
                }
                
                if (!inSearch && neighbor.Taxi < maxLength && neighbor.G != null)
                {
                    openNodes.Add(neighbor);
                }
            }
        }
        
        if (endNode == null && closedNodes.Count > 0 && failureMode == FailureMode.PathToBest)
        {
            Node current = closedNodes[0];
            foreach (Node t in closedNodes)
            {
                if (t.F < current.F || t.F == current.F && t.H < current.H)
                    current = t;
            }
            endNode = current;
        }

        if (endNode == null)
        {
            isComplete = false;
            if (failureMode == FailureMode.StraightToTarget)
            {
                return [endLocation];
            }
            if (failureMode == FailureMode.DoNothing)
                return [];
            return [startLocation];
        }

        Node currentPathTile = endNode;
        List<Point> pathPoints = [];
        while (currentPathTile != startNode && currentPathTile != null)
        {
            pathPoints.Insert(0, currentPathTile.P);
            currentPathTile = currentPathTile.Parent;
        }

        isComplete = endNode.P == endLocation;
        return pathPoints;

        List<Node> ValidNeighbors(Node toCheck, ref List<Node> openNodes, ref List<Node> closedNodes)
        {
            if (toCheck.Neighbors != null)
                return toCheck.Neighbors;
            toCheck.Neighbors = [];
            foreach (Point move in moveSet)
            {
                if (CheckPos(toCheck, toCheck.P + move, out Node n1, ref openNodes, ref closedNodes))
                    toCheck.Neighbors.Add(n1);
                /*if (Main.rand.NextBool(5))
                    Dust.NewDustPerfect((toCheck.P + move).ToWorldCoordinates(), DustID.Clentaminator_Red,
                        Vector2.Zero);*/
            }
            return toCheck.Neighbors;
        }

        bool CheckPos(Node origin, Point pos, out Node node, ref List<Node> openNodes, ref List<Node> closedNodes)
        {
            Node result = null;

            Node isInOpen = openNodes.Find(i => i.P == pos);
            if (isInOpen != null)
                result = isInOpen;

            Node isInClosed = closedNodes.Find(i => i.P == pos);
            if (isInClosed != null)
                result = isInClosed;

            int g = 0;
            int t = 0;
            Point p = pos;
            if (origin != null)
            {
                g = origin.G ?? 0;
                t = origin.Taxi ?? 0;
                p = origin.P;
            }

            if (result == null)
                result = new Node()
                {
                    P = pos,
                    G = g + costFunction(p, pos),
                    H = (int)(10 * Distance(pos, endLocation, distanceMode)),
                    Taxi = t + 1,
                    Neighbors = null,
                    Parent = origin
                };
            node = result;
            if (result.G == null)
                return false;
            return true;
        }
    }
    
    public static List<Point> AStar(Point startLocation, Point endLocation,
        int maxLength = 200, int maxTilesToCheck = 1000, int passCount = 1, int width = 16, int height = 16,
        DistanceMode distanceMode = DistanceMode.Taxicab, FailureMode failureMode = FailureMode.PathToBest,
        Func<Point, Point, int?> costFunction = null)
    {
        List<Node> open = null;
        List<Node> closed = null;
        return AStarIterative(startLocation, endLocation,
            ref open, ref closed, out bool _,
            maxLength, maxTilesToCheck,
            width, height,
            distanceMode, failureMode,
            costFunction);
    }
}