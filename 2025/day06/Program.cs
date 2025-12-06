using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

class Solver
{
    private List<List<int>> Table = new();
    // Instead of storing the entire table in memory, we could just keep a running sum and product (separately) for each column.
    // This would reduce memory consumption to O(width).

    public Solver() {
    }

    public void DigestLine(string line) {
        Table.Add(
            line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(Int32.Parse)
                .ToList()
        );
    }

    private long LongSum(long lhs, long rhs) {
        return lhs + rhs;
    }

    private long LongMult(long lhs, long rhs) {
        return lhs * rhs;
    }

    public long ComputeSubtotal(string op, int index) {
        return Table.Select(l => (long)(l[index]))
            .Aggregate(op == "+" ? LongSum : LongMult);
    }

    public long ApplyOperations(string line) {
        return line
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select((op, index) => ComputeSubtotal(op, index))
            .Sum();
    }

    static void Main(string[] args)
    {
        string filename = "input";
        if (args.Length > 0) {
            filename = args[0];
        }
        using (StreamReader sr = new StreamReader(filename))
        {
            string previousLine = "";
            string line;
            Solver solver = new();
            while ((line = sr.ReadLine()) != null) {
                if (previousLine != "") {
                    solver.DigestLine(previousLine);
                }
                previousLine = line;
            }
            Console.WriteLine("grand total = " + solver.ApplyOperations(previousLine));
        }
    }
}
