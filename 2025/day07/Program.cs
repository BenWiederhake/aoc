using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

class Solver
{
    private bool[] Beams = {};
    public int ObservedSplits { get; private set; }

    public Solver() {
        ObservedSplits = 0;
    }

    public void DigestLine(string line) {
        if (Beams.Count() == 0) {
            Beams = line.Select(ch => ch == 'S').ToArray();
        } else {
            // Console.WriteLine("Potentially splitting across >>" + line + "<<");
            var w = line.Length;
            var newBeams = new bool[w];
            // I'm currently running .NET 6.0, and don't want to update right now, in order to get today's AoC puzzle done same-day.
            // Otherwise, I would use the fancier `.Index()`:
            int col = 0;
            foreach (var ch in line) {
                if (ch == '.') {
                    // pass-through beam
                    newBeams[col] |= Beams[col];
                } else if (Beams[col]) {
                    // beam is split
                    ObservedSplits += 1;
                    if (col - 1 >= 0) {
                        newBeams[col - 1] = true;
                    }
                    if (col + 1 < w) {
                        newBeams[col + 1] = true;
                    }
                }
                col += 1;
            }
            Beams = newBeams;
        }
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
            Console.WriteLine("observed " + solver.ObservedSplits + " splits");
        }
    }
}
