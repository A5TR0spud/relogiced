using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Other;

internal class Node
{
    public Point P;
    public int? G;
    public int? H;
    public Node Parent;
    public int? F => G * 10 + H;
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
        int nextStepIndex = path.FindLastIndex(i => 
            Collision.CanHit(entity.TopLeft, 0, 0,
                i.ToWorldCoordinates(8 - entity.width / 2f, 8 - entity.width / 2f), 0, 0) &&
            Collision.CanHit(entity.TopRight, 0, 0,
                i.ToWorldCoordinates(8 + entity.width / 2f, 8 - entity.width / 2f), 0, 0) &&
            Collision.CanHit(entity.BottomLeft + new Vector2(0, entity.height), 0, 0,
                i.ToWorldCoordinates(8 - entity.width / 2f, 8 + entity.width / 2f), 0, 0) &&
            Collision.CanHit(entity.BottomRight + new Vector2(entity.width, entity.height), 0, 0,
                i.ToWorldCoordinates(8 + entity.width / 2f, 8 + entity.width / 2f), 0, 0));
        if (nextStepIndex == -1) nextStepIndex = 0;
        return nextStepIndex;
    }

    public static Point NextStep(Entity entity, List<Point> path)
    {
        return path[NextStepIndex(entity, path)];
    }
    
    public static List<Point> AStar(Point startLocation, Point endLocation
        , int maxLength = 200, int maxTilesToCheck = 1000, int width = 16, int height = 16,
        DistanceMode distanceMode = DistanceMode.Taxicab, Func<Point, Point, int> costFunction = null)
    {
        if (Collide(endLocation, (int)Math.Ceiling(width / 16f), (int)Math.Ceiling(height / 16f)) ||
            Collide(startLocation, (int)Math.Ceiling(width / 16f), (int)Math.Ceiling(height / 16f))) return [endLocation];
        if (distanceMode == DistanceMode.EuclidianSquared) maxLength *= maxLength;
        
        if (startLocation == endLocation) return [endLocation];
        if (Distance(startLocation, endLocation, distanceMode) > maxLength) return [endLocation];
        if (maxTilesToCheck <= 0) return [endLocation];
        costFunction ??= (_, _) => 1;
        List<Node> pathNodes = [];

        Node startNode = new Node()
        {
            P = startLocation,
            G = 0,
            H = (int)(10 * Distance(startLocation, endLocation, distanceMode)),
            Parent = null,
            Neighbors = null
        };
        Node endNode = null;

        List<Node> openNodes = [startNode];
        List<Node> closedNodes = [];
        bool targetAcquired = false;

        bool CheckPos(Point pos, out Node node)
        {
            Node result = null;
            Node posInOpen = openNodes.Find(i => i.P == pos);
            if (posInOpen != null) result = posInOpen;
            Node posInClosed = closedNodes.Find(i => i.P == pos);
            if (posInClosed != null) result = posInClosed;
            if (result == null)
                result = new Node()
                {
                    P = pos,
                    G = null,
                    H = (int)(10 * Distance(pos, endLocation, distanceMode)),
                    Neighbors = null,
                    Parent = null
                };
            node = result;
            if (Collide(result.P, (int)Math.Ceiling(width / 16f), (int)Math.Ceiling(height / 16f)))
                return false;
            return true;
        }
        
        List<Node> ValidNeighbors(Node toCheck)
        {
            if (toCheck.Neighbors != null && toCheck.Neighbors.Count > 0)
                return toCheck.Neighbors;
            List<Node> ret = [];
            if (CheckPos(toCheck.P + new Point(1, 0), out Node n1))
                ret.Add(n1);
            if (CheckPos(toCheck.P + new Point(-1, 0), out Node n2))
                ret.Add(n2);
            if (CheckPos(toCheck.P + new Point(0, 1), out Node n3))
                ret.Add(n3);
            if (CheckPos(toCheck.P + new Point(0, -1), out Node n4))
                ret.Add(n4);
            toCheck.Neighbors = ret;
            return ret;
        }

        while (openNodes.Count > 0 && maxTilesToCheck > 0)
        {
            maxTilesToCheck--;
            Node current = openNodes[0];
            foreach (Node t in openNodes)
                if (t.F < current.F || (int)t.F == (int)current.F && t.H < current.H)
                    current = t;

            /*if (Main.rand.NextBool(20))
                Dust.NewDustPerfect(current.P.ToWorldCoordinates(), DustID.Clentaminator_Red,
                    Vector2.Zero);*/
            
            closedNodes.Add(current);
            openNodes.Remove(current);

            if (current.P == endLocation)
            {
                endNode = current;
                targetAcquired = true;
                break;
            }

            foreach (Node neighbor in ValidNeighbors(current))
            {
                if (closedNodes.Contains(neighbor))
                    continue;
                bool inSearch = openNodes.Contains(neighbor);

                int gCostToNeighbor = current.G.Value + costFunction(current.P, neighbor.P);

                if (neighbor.G == null || gCostToNeighbor < neighbor.G)
                {
                    neighbor.G = gCostToNeighbor;
                    neighbor.Parent = current;
                }
                
                if (!inSearch && neighbor.G < maxLength)
                {
                    openNodes.Add(neighbor);
                }
            }
        }

        if (!targetAcquired)
            return [endLocation];

        Node currentPathTile = endNode;
        while (currentPathTile != startNode)
        {
            pathNodes.Insert(0, currentPathTile);
            currentPathTile = currentPathTile.Parent;
        }

        List<Point> pathPoints = [];
        foreach (Node n in pathNodes)
        {
            pathPoints.Add(n.P);
        }

        return pathPoints;
    }
}