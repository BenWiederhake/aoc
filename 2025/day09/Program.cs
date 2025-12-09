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

class Solver
{
    static void Main(string[] args)
    {
        string filename = "input";
        if (args.Length > 0) {
            filename = args[0];
        }
        List<Point> points = new();
        using (StreamReader sr = new StreamReader(filename))
        {
            string line;
            while ((line = sr.ReadLine()) != null) {
                points.Add(new Point(line));
            }
        }
        var maxArea = points
            .SelectMany((p, i) => points[(i + 1)..], (p, q) => p.AreaTo(q))
            .Max();
        Console.WriteLine($"Max area: {maxArea}");
    }
}
