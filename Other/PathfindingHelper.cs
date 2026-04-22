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

/*
public class PathfindingHelper : ModSystem
{
    private static bool enabled = false;
    public static void EnableTicking()
    {
        if (enabled) return;
        enabled = true;
        field = new PathInfo[Main.tile.Height * Main.tile.Width];
    }

    public static bool IsEnabled()
    {
        return enabled;
    }
    
    public override void Load()
    {
        enabled = false;
    }

    public override void Unload()
    {
        enabled = false;
    }

    public override void PreUpdateEntities()
    {
        if (enabled)
            TickTrackerJacker();
        else
        {
            field = [];
            indicesToVisit = [];
        }
        enabled = false;
    }

    public static Vector2 GetDirection(Vector2 pos) => GetDirection(pos.ToTileCoordinates());

    public static Vector2 GetDirection(Point pos) => GetDirection(pos.X, pos.Y);

    private struct PathInfo()
    {
        public short? up = 0, right = 0, down = 0, left = 0;
        public ushort? dist = null;
        public bool isConflicted = false;
    }

    private static PathInfo[] field = [];

    private static List<int> indicesToVisit = [];

    public static Vector2 GetDirection(int x, int y)
    {
        int idx = IndexOf(x, y);
        if (IndexInvalid(idx)) return Vector2.Zero;
        PathInfo info = field[x];
        return new Vector2(info.right.GetValueOrDefault(0) - info.left.GetValueOrDefault(0),
            info.down.GetValueOrDefault(0) - info.up.GetValueOrDefault(0))
            .SafeNormalize(Vector2.Zero);
    }

    private static Point PosOf(int idx) => new(idx % Main.tile.Width, idx / Main.tile.Width);

    private static int IndexOf(Point p) => IndexOf(p.X, p.Y);
    private static int IndexOf(int x, int y) => x + y * Main.tile.Width;

    private static bool IndexValid(int idx) => idx >= 0 && idx < field.Length;
    private static bool IndexInvalid(int idx) => !IndexValid(idx);

    private static void TickTrackerJacker()
    {
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.CanBeChasedBy())
            {
                Point p = npc.Center.ToTileCoordinates();
                int x = p.X;
                int y = p.Y;
                int idx = IndexOf(x, y);
                if (IndexInvalid(idx)) continue;
                field[idx] = new PathInfo
                {
                    dist = 0,
                    down = 0,
                    isConflicted = false,
                    left = 0,
                    right = 0,
                    up = 0
                };
                CheckIndex(idx);
            }
        }

        int eliminate = 0;
        for (int i = 0; i < Math.Min(indicesToVisit.Count, 16); i++)
        {
            int idx = indicesToVisit[i];
            PathInfo pathInfo = field[idx];
            for (int j = 0; j < 4; j++)
            {
                Point off = new Point(j switch
                {
                    0 => -1,
                    1 => 1,
                    _ => 0
                }, j switch
                {
                    2 => -1,
                    3 => 1,
                    _ => 0
                });
                int newIdx = IndexOf(PosOf(idx)) + IndexOf(off);
                if (IndexInvalid(newIdx)) continue;
                PathInfo pI2 = field[newIdx];
                bool isSolid = !Collision.SolidCollision(PosOf(newIdx).ToWorldCoordinates(), 0, 0);
                if (isSolid) continue;
                int dist = pathInfo.dist.GetValueOrDefault(0) + 1;
                if (pI2.dist == null || pI2.dist.Value > dist)
                {
                    pI2.dist = (ushort)dist;
                    if (off.X == 1)
                    {
                        pI2.left = (short)(pI2.left.GetValueOrDefault(0) + 1);
                        pI2.right = 0;
                    }
                    if (off.X == -1)
                    {
                        pI2.right = (short)(pI2.left.GetValueOrDefault(0) + 1);
                        pI2.left = 0;
                    }
                    if (off.Y == 1)
                    {
                        pI2.down = (short)(pI2.left.GetValueOrDefault(0) + 1);
                        pI2.up = 0;
                    }
                    if (off.Y == -1)
                    {
                        pI2.up = (short)(pI2.left.GetValueOrDefault(0) + 1);
                        pI2.down = 0;
                    }
                    CheckIndex(newIdx);
                }
            }
            eliminate++;
        }

        indicesToVisit.RemoveRange(0, eliminate);
        if (indicesToVisit.Count > 256)
        {
            indicesToVisit.RemoveRange(256, indicesToVisit.Count - 256);
        }
    }

    private static void CheckIndex(int idx)
    {
        if (IndexInvalid(idx)) return;
        if (indicesToVisit.Contains(idx)) return;
        indicesToVisit.Add(idx);
    }
}*/
