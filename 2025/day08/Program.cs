using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

class Point {
    // Some coordinates are close to zero, some are close to 100k, so we cannot just use UInt16.
    public int X { get; }
    public int Y { get; }
    public int Z { get; }

    public Point(int x, int y, int z) {
        X = x;
        Y = y;
        Z = z;
    }

    public long DistanceSqTo(Point that) {
        long dx = this.X - that.X;
        long dy = this.Y - that.Y;
        long dz = this.Z - that.Z;
        return dx * dx + dy * dy + dz * dz;
    }
}

class Distance : IComparable<Distance> {
    // Assuming that coordinates range from 0 through 100k, the maximum distance squared
    // is 3 * (100k ** 2) = 30e9, which doesn't fit into int.
    public long DistanceSq { get; }
    public int I { get; }
    public int J { get; }

    public Distance(long distanceSq, int i, int j) {
        DistanceSq = distanceSq;
        I = i;
        J = j;
    }

    public int CompareTo(Distance? that)
    {
        if (that == null) return -1;
        if (this.DistanceSq <  that.DistanceSq) return -1;
        if (this.DistanceSq == that.DistanceSq) return 0;
        return 1;
    }
}

// The classic "UnionFind" data structure:
class CircuitBuilder {
    // Each point is only represented implicitly by its index of this array.
    // The value is the smallest known element in the connected component,
    // i.e. the point with the smallest ID in the "circuit".
    // Note that the value is the smallest *KNOWN* element, which may be different from the smallest *ACTUAL* element!
    // This allows us to do lazy updates, which in turn allows Find() to work without allocating,
    // which in turn enables the fantastical runtime of O(alpha(n)), where alpha is the inverse Ackermann function.
    // And *THAT* is why UnionFind is my favorite data structure! :D
    // https://en.wikipedia.org/wiki/Disjoint-set_data_structure#Time_complexity
    private short[] Parents;
    private short[] NumChildren;

    public CircuitBuilder(int numPoints) {
        Debug.Assert(numPoints <= Int16.MaxValue);
        Parents = new short[numPoints];
        NumChildren = new short[numPoints];
        for (short i = 0; i < numPoints; i += 1) {
            Parents[i] = i;
        }
    }

    // Also known as "Union by size":
    public void EnsureConnected(int pointI, int pointJ) {
        var parentI = Find((short)pointI);
        // Because parentI is a root note, it will not be affected by the following Find() operation:
        var parentJ = Find((short)pointJ);
        if (parentI == parentJ) {
            // Nothing to do!
            return;
        }
        // Want to make "parentI" the new root node. Swap if necessary:
        if (NumChildren[parentI] < NumChildren[parentJ]) {
            (parentJ, parentI) = (parentI, parentJ);
        }
        Parents[parentJ] = parentI;
        // We add to parentI the entire subtree of parentJ, plus parentJ itself.
        NumChildren[parentI] += (short)(NumChildren[parentJ] + 1);
    }

    // Finds the smallest element connected to the given id. Update the encountered pointers along the way.
    private short Find(short id) {
        // "Path halving", i.e. only update every other pointer:
        while (Parents[id] != id) {
            Parents[id] = Parents[Parents[id]];
            id = Parents[id];
        }
        // Note that since we only use sizes of root nodes, it doesn't matter that this method
        // results in internal nodes with inconsistent sizes.
        return id;
    }

    public long ComponentSizeIfRootOrZero(int point) {
        // if (point == 0) {
        //     Console.WriteLine("State after all Connect()s:");
        //     foreach ((var i, var p) in Parents.Index()) {
        //         Console.WriteLine($"[{i}]: Parent={p}, NumChildren={NumChildren[i]}");
        //     }
        // }
        if (Parents[point] != point) {
            return 0;
        }
        // Addition automatically converts to int. Casting this back to short seems pointless,
        // given that we're about to cast it to long afterwards anyway.
        return 1 + NumChildren[point];
    }
}

class Solver
{
    public List<Point> Points = new();

    public Solver() {
    }

    public void DigestLine(string line) {
        var parts = line.Split(',');
        Points.Add(new Point(
            Int32.Parse(parts[0]),
            Int32.Parse(parts[1]),
            Int32.Parse(parts[2])
        ));
    }

    public long ProductOfLargestThreeCircuits() {
        // Enumerate *all* distances. This takes Theta(n^2) time, and there are faster ways to do this
        // since we just want to know the "top part" of this list, but this is good enough for n=1000.
        List<Distance> distances = new();
        foreach ((int i, Point p_i) in Points.Index()) {
            foreach ((int j_minus_i_minus_1, Point p_j) in Points[(i + 1)..].Index()) {
                int j = j_minus_i_minus_1 + i + 1;
                long distanceSq = p_i.DistanceSqTo(p_j);
                distances.Add(new Distance(distanceSq, i, j));
            }
        }
        Console.WriteLine("Computed " + distances.Count() + " distance objects.");

        // Sort Distance objects by distance (squared):
        distances.Sort();

        // "connect together the 1000 pairs of junction boxes which are closest together"
        const int NUM_CONNECTIONS = 1000;
        // const int NUM_CONNECTIONS = 10;
        CircuitBuilder cb = new(Points.Count());
        foreach (var distance in distances.Take(NUM_CONNECTIONS)) {
            cb.EnsureConnected(distance.I, distance.J);
        }

        // Find the largest components:
        List<long> circuitSizes = Enumerable.Range(0, Points.Count()).Select(cb.ComponentSizeIfRootOrZero).ToList();
        // foreach ((int i, long size) in circuitSizes.Index()) {
        //     Console.WriteLine($"circuit with root #{i} has size {size}");
        // }
        circuitSizes.Sort();
        var n = circuitSizes.Count();
        Console.WriteLine($"Largest components: {circuitSizes[n - 1]}, {circuitSizes[n - 2]}, {circuitSizes[n - 3]}");
        // Console.WriteLine($"Smallest components: {circuitSizes[0]}, {circuitSizes[1]}, {circuitSizes[2]}");
        return circuitSizes[n - 1] * circuitSizes[n - 2] * circuitSizes[n - 3];
    }

    static void Main(string[] args)
    {
        string filename = "input";
        if (args.Length > 0) {
            filename = args[0];
        }
        using (StreamReader sr = new StreamReader(filename))
        {
            string line;
            Solver solver = new();
            while ((line = sr.ReadLine()) != null) {
                solver.DigestLine(line);
            }
            Console.WriteLine("loaded " + solver.Points.Count() + " points");
            Console.WriteLine("observed " + solver.ProductOfLargestThreeCircuits() + " timelines");
        }
    }
}
