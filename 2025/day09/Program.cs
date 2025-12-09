using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

class Point {
    public int X { get; }
    public int Y { get; }

    public Point(string line) {
        var parts = line.Split(',');
        X = Int32.Parse(parts[0]);
        Y = Int32.Parse(parts[1]);
    }

    public long AreaTo(Point that) {
        long w = Int32.Abs(this.X - that.X) + 1;
        long h = Int32.Abs(this.Y - that.Y) + 1;
        return w * h;
    }
}

class Solver {
    private List<Point> Points = new();

    public Solver() {
    }

    public void AddNext(Point p) {
        Points.Add(p);
    }

    public long FindMaxAreaPair() {
        return Points
            .SelectMany((p, i) => Points[(i + 1)..], this.AreaIfFullyInside)
            .Max();
    }

    public long AreaIfFullyInside(Point p, Point q) {
        // "Is the implied rectangle fully inside the polygon?"
        // is identical to "Is there no line that cuts through the implied rectangle?",
        // which is much easier to check!
        // Note that this assumes that no lines are "silly", i.e. do a 180° turn,
        // or move up only 1 space and go back, leaving a contiguous green (valid!) space behind.
        var anyCut = Points
            .Zip(Points.Skip(1).Append(Points[0]))
            // Who came up with the tuple-syntax?! Why is it is 1-indexed?!
            .Select<(Point, Point), bool>(line => DoesLineCutThroughRect(line.Item1, line.Item2, p, q))
            // Fun fact: Any() and Any(identityFunction) do *VERY* different things,
            // and passing the trivial "conversion" is strictly necessary here.
            // Any() with predicate works as you might expect:
            // https://learn.microsoft.com/en-us/dotnet/api/system.linq.queryable.any?view=net-10.0#system-linq-queryable-any-1(system-linq-iqueryable((-0))-system-linq-expressions-expression((system-func((-0-system-boolean)))))
            // Any() WITHOUT a predicate only checks whether the length is non-zero, so a single "false" would yield "true":
            // https://learn.microsoft.com/en-us/dotnet/api/system.linq.queryable.any?view=net-10.0#system-linq-queryable-any-1(system-linq-iqueryable((-0)))
            // This is the first language where is see this "feature". Ugh.
            .Any(b => b);
        if (anyCut) {
            return 0;
        }
        return p.AreaTo(q);
    }

    public static bool DoesLineCutThroughRect(Point a, Point b, Point p, Point q) {
        int loX = Int32.Min(p.X, q.X);
        int hiX = Int32.Max(p.X, q.X);
        int loY = Int32.Min(p.Y, q.Y);
        int hiY = Int32.Max(p.Y, q.Y);
        if (a.X == b.X) {
            // Line is vertical:
            return DoesIntersect(a.X, a.Y, b.Y, loX, hiX, loY, hiY);
        } else {
            // Line is horizontal:
            return DoesIntersect(a.Y, a.X, b.X, loY, hiY, loX, hiX);
        }
    }

    // Instead of thinking in X- and Y-axis, this method speaks in terms of cut-axis and depth-axis.
    public static bool DoesIntersect(int cutLevel, int depthStart, int depthEnd, int loCut, int hiCut, int loDepth, int hiDepth) {
        if (cutLevel <= loCut || hiCut <= cutLevel) {
            // The line passes by the rectangle, no matter how long/short it is!
            return false;
        }
        if (depthStart > depthEnd) {
            // Whoops, swap it the correct way around:
            (depthStart, depthEnd) = (depthEnd, depthStart);
        }
        if (depthEnd <= loDepth || hiDepth <= depthStart) {
            // If the line were longer, it would have intersected the rectangle; but as-is,
            // it does NOT intersect:
            return false;
        }
        // Either a point of the line lies inside the rectangle, or the line cuts the rectangle:
        return true;
    }

    static void Main(string[] args) {
        string filename = "input";
        if (args.Length > 0) {
            filename = args[0];
        }
        Solver solver = new();
        List<Point> points = new();
        using (StreamReader sr = new StreamReader(filename)) {
            string line;
            while ((line = sr.ReadLine()) != null) {
                solver.AddNext(new Point(line));
            }
        }
        var maxArea = solver.FindMaxAreaPair();
        Console.WriteLine($"Max area: {maxArea}");
    }
}
