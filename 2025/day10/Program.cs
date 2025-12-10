using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

class Solver {
    private int Goal;
    private List<int> Buttons;
    private HashSet<int> CurrentReachable;
    private int CurrentNumPresses;

    public Solver(string line) {
        var parts = line.Split(' ');
        Goal = ParseGoal(parts[0]);
        Buttons = parts[1..^0].Select(ParseButton).ToList();
        CurrentReachable = new();
        CurrentReachable.Add(0);
        CurrentNumPresses = 0;
    }

    private static int ParseGoal(string goalSpec) {
        return goalSpec[1..^0] // Unwrap "[" and "]"
            .Select((ch, index) => (ch == '#') ? (1 << index) : 0)
            .Aggregate((a, b) => a | b);
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

    private void PressAnotherButton() {
        CurrentReachable = CurrentReachable
            .SelectMany(r => Buttons.Select(b => b ^ r))
            .ToHashSet();
        CurrentNumPresses += 1;
    }

    public int ComputeMinimum() {
        while (!CurrentReachable.Contains(Goal)) {
            Debug.Assert(CurrentNumPresses <= Buttons.Count());
            PressAnotherButton();
        }
        Console.WriteLine($"-> {CurrentNumPresses} presses");
        return CurrentNumPresses;
    }

    static void Main(string[] args) {
        Console.WriteLine("Parsing tests:");
        foreach (string goalSpec in "[...] [#..] [.#.] [##.] [..#] [#.#] [.##] [###]".Split(' ')) {
            Console.WriteLine($"  goalSpec=>>{goalSpec}<< => {ParseGoal(goalSpec)}");
        }
        foreach (string buttonSpec in "() (0) (1) (0,1) (2) (0,2) (1,2) (0,1,2)".Split(' ')) {
            Console.WriteLine($"  buttonSpec=>>{buttonSpec}<< => {ParseButton(buttonSpec)}");
        }
        Console.WriteLine("Selftest complete.");
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
