using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

class Solver
{
    private long[] BeamTimelines = {};

    public Solver() {
    }

    public void DigestLine(string line) {
        if (BeamTimelines.Count() == 0) {
            BeamTimelines = line.Select(ch => ch == 'S' ? 1L : 0).ToArray();
        } else {
            // Console.WriteLine("Potentially splitting across >>" + line + "<<");
            var w = line.Length;
            var newBeamTimelines = new long[w];
            // I'm currently running .NET 6.0, and don't want to update right now, in order to get today's AoC puzzle done same-day.
            // Otherwise, I would use the fancier `.Index()`:
            int col = 0;
            foreach (var ch in line) {
                if (ch == '.') {
                    // pass-through beam
                    newBeamTimelines[col] += BeamTimelines[col];
                } else if (BeamTimelines[col] > 0) {
                    // beam is split
                    if (col - 1 >= 0) {
                        newBeamTimelines[col - 1] += BeamTimelines[col];
                    }
                    if (col + 1 < w) {
                        newBeamTimelines[col + 1] += BeamTimelines[col];
                    }
                }
                col += 1;
            }
            BeamTimelines = newBeamTimelines;
        }
    }

    public long TotalTimelines() {
        return BeamTimelines.Sum();
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
            Console.WriteLine("observed " + solver.TotalTimelines() + " timelines");
        }
    }
}
