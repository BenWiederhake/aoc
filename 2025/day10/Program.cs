using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

using LpSolveDotNet;

class Solver {
    private List<int> Buttons;
    private List<int> GoalJoltages;

    public Solver(string line) {
        var parts = line.Split(' ');
        Buttons = parts[1..^1].Select(ParseButton).ToList();
        GoalJoltages = ParseGoalJoltages(parts[^1]);
    }

    private static int ParseButton(string buttonSpec) {
        if (buttonSpec == "()") {
            // The below .Split() call would return a single element, and not zero elements
            // (which would cause a different problem with .Aggregate()).
            // So instead of uglifying the below code any further,
            // let's just handle this corner case especially:
            return 0;
        }
        return buttonSpec[1..^1] // Unwrap "(" and ")"
            .Split(',')
            .Select(lightIndex => 1 << Int32.Parse(lightIndex))
            .Aggregate((a, b) => a | b);
    }

    private static List<int> ParseGoalJoltages(string joltageSpec) {
        return joltageSpec[1..^1] // Unwrap "{" and "}"
            .Split(',')
            .Select(Int32.Parse)
            .ToList();
    }

    public int ComputeMinimum() {
        int result;
        using (LpSolve lp = LpSolve.make_lp(0, Buttons.Count()))
        {
            ArgumentNullException.ThrowIfNull(lp); // TODO: Better type?
            result = ComputeMinimumUsing(lp);
        }
        if (result < 0) {
            throw new System.Exception($"Error {result}?!");
        }
        return result;
    }

    //private static bool MyDebug(int i, int b, int joltageMask) {
    //    Console.WriteLine($"    checking button #{i + 1} (1-indexed), which has mask {b}, intersection is {b & joltageMask}, match {(b & joltageMask) > 0}");
    //    return true;
    //}

    private int ComputeMinimumUsing(LpSolve lp) {
        // Set all columns to "int".
        // "The column number of the variable that must be set. Must be between 1 and the number of columns in the lp."
        for (int i = 1; i < Buttons.Count() + 1; ++i) {
            lp.set_int(i, true);
        }

        // "Note that it is advised to set the objective function […] before adding rows."
        // For set_obj_fn, first parameter: 
        lp.set_obj_fn(Enumerable.Repeat(1.0, 1 + Buttons.Count()).ToArray());
        // "The default of lp_solve is to minimize"

        // Then, add the rows,i.e. joltage requirements:
        lp.set_add_rowmode(true);
        foreach (var (index, goalJoltage) in GoalJoltages.Index()) {
            int joltageMask = 1 << index;
            // Console.WriteLine($"  joltageMask={joltageMask}");
            // Buttons are columns
            //foreach (var (i, b) in Buttons.Index()) {
            //    Console.WriteLine($"    #{i + 1}: b={b}, b&mask={b&joltageMask}, take={(b & joltageMask) > 0}");
            //}
            var columnIndices = Buttons
                .Index()
                //.Where(tuple => MyDebug(tuple.Item1, tuple.Item2, joltageMask)) // Tuples are 1-indexed, ugh
                .Where(tuple => (tuple.Item2 & joltageMask) > 0) // Tuples are 1-indexed, ugh
                .Select(tuple => tuple.Item1 + 1) // Columns are also 1-indexed, ugh
                .ToArray();
            // Console.WriteLine($"  -> columnIndices={String.Join(',', columnIndices)}");
            Debug.Assert(columnIndices.Length > 0);
            var columnValues = Enumerable.Repeat(1.0, columnIndices.Length).ToArray();
            if (lp.add_constraintex(columnIndices.Length, columnValues, columnIndices, lpsolve_constr_types.EQ, goalJoltage) == false)
            {
                return 501;
            }
        }
        // rowmode should be turned off again when done building the model
        lp.set_add_rowmode(false);

        // lp.write_lp("/dev/stdout");

        // Actually solve it:
        lp.set_verbose(lpsolve_verbosity.IMPORTANT);
        lpsolve_return s = lp.solve();
        if (s != lpsolve_return.OPTIMAL)
        {
            return 502;
        }

        // Console.WriteLine("Objective value: " + lp.get_objective());
        return (int)(lp.get_objective() + 0.5); // Round, because sometimes it returns values like 86.99999999999999. Huh.
    }

    static void Main(string[] args) {
        Console.WriteLine("Parsing tests:");
        foreach (string buttonSpec in "() (0) (1) (0,1) (2) (0,2) (1,2) (0,1,2)".Split(' ')) {
            Console.WriteLine($"  buttonSpec=>>{buttonSpec}<< => {ParseButton(buttonSpec)}");
        }
        foreach (string goalJoltageSpec in "{1} {2,3} {4,5,6}".Split(' ')) {
            Console.WriteLine($"  goalJoltageSpec=>>{goalJoltageSpec}<< => {ParseGoalJoltages(goalJoltageSpec)}");
        }
        Console.WriteLine("Selftest complete.");
        LpSolve.Init();
        string filename = "input";
        if (args.Length > 0) {
            filename = args[0];
        }
        int buttonPresses = 0;
        using (StreamReader sr = new StreamReader(filename)) {
            string line;
            while ((line = sr.ReadLine()) != null) {
                Solver solver = new(line);
                buttonPresses += solver.ComputeMinimum();
            }
        }
        Console.WriteLine($"buttonPresses: {buttonPresses}");
    }
}
