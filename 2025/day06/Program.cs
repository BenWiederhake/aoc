using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

class Solver
{
    private List<string> Lines = new();

    public Solver() {
    }

    public void AddLine(string line) {
        Lines.Add(line);
    }

    private long LongSum(long lhs, long rhs) {
        return lhs + rhs;
    }

    private long LongMult(long lhs, long rhs) {
        return lhs * rhs;
    }

    private int GetNumberInColumn(int col) {
        var columnString = new String(Lines.SkipLast(1).Select(l => l[col]).ToArray());
        //Console.WriteLine("Interpreting column >>" + columnString + "<<");
        return Int32.Parse(columnString);
    }

    public long ComputeGrandTotal() {
        int width = Lines[0].Count();
        List<int> buf = new();
        // Note: Could also go left-to-right, since multiplication and addition are commutative and associative.
        long grandTotal = 0;
        for (int col = width - 1; col >= 0; col -= 1) {
            buf.Add(GetNumberInColumn(col));
            char op = Lines.Last()[col];
            if (op != ' ') {
                grandTotal += buf.Select(v => (long)v).Aggregate(op == '+' ? LongSum : LongMult);
                buf.Clear();
                col -= 1; // Skip empty column
            }
        }
        return grandTotal;
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
                solver.AddLine(line);
            }
            Console.WriteLine("grand total = " + solver.ComputeGrandTotal());
        }
    }
}
