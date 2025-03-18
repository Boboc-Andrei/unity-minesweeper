using System;
using System.Collections.Generic;
using System.Linq;



public abstract class MoveHint : IPriority {
    protected MinesweeperGrid Grid { get; }
    protected MinesweeperSolver Solver { get; }
    public CellGroup CellGroup { get; set; }
    public abstract int Priority { get; }

    public MoveHint(CellGroup cellGroup, MinesweeperGrid grid, MinesweeperSolver solver) {
        CellGroup = cellGroup;
        Grid = grid;
        Solver = solver;
    }

    public override bool Equals(object? obj) {
        if (obj is MoveHint other) {
            return GetType() == other.GetType() && CellGroup == other.CellGroup;
        }
        return false;
    }
    public override int GetHashCode() {
        return HashCode.Combine(GetType(), CellGroup);
    }

    public abstract void Solve();
    public abstract List<Cell> GetAffectedCells();
    public abstract bool IsObsolete();
}

public class FlagsSatisfiedHint : MoveHint {
    public override int Priority { get; } = 1;
    public FlagsSatisfiedHint(CellGroup cellGroup, MinesweeperGrid grid, MinesweeperSolver solver) : base(cellGroup, grid, solver) { }

    public override List<Cell> GetAffectedCells() {
        List<Cell> affectedCells = Grid.GetUnrevealedNeighbours(CellGroup.Owner, includeFlagged: true);
        affectedCells.Add(CellGroup.Owner);
        return affectedCells;
    }

    public override bool IsObsolete() {
        return Grid.GetUnrevealedNeighbours(CellGroup.Owner, includeFlagged: false).Count == 0 || !CellGroup.Owner.HasAllMinesFlagged;
    }

    public override void Solve() {
        Grid.RevealNeighbours(CellGroup.Owner);
    }

    public override string ToString() {
        return $"(cell {CellGroup.Owner} flags satisfied)";
    }
}

public class FlagGroupHint : MoveHint {
    public override int Priority { get; } = 2;
    public FlagGroupHint(CellGroup cellGroup, MinesweeperGrid grid, MinesweeperSolver solver) : base(cellGroup, grid, solver) { }

    public override List<Cell> GetAffectedCells() {
        return new List<Cell>(CellGroup.Cells) { CellGroup.Owner };
    }

    public override bool IsObsolete() {
        return !CellGroup.Cells.Any(cell => !cell.IsFlagged);
    }

    public override void Solve() {
        foreach (var cell in CellGroup.Cells) {
            Grid.SetFlag(cell, true);
        }
    }
    public override string ToString() {
        string groupCells = string.Join(", ", CellGroup.Cells.Select(cell => cell.ToString()));
        return $"(cells [{groupCells}] must be flagged)";
    }
}

public class WrongFlagHint : MoveHint {
    public override int Priority { get; } = 0;
    public WrongFlagHint(CellGroup cellGroup, MinesweeperGrid grid, MinesweeperSolver solver) : base(cellGroup, grid, solver) { }

    public override List<Cell> GetAffectedCells() {
        return new List<Cell> { CellGroup.Owner };
    }

    public override bool IsObsolete() {
        return !CellGroup.Owner.IsFlagged || (CellGroup.Owner.IsFlagged && Solver.flaggableCells.Contains(CellGroup.Owner));
    }

    public override void Solve() {
        Grid.SetFlag(CellGroup.Owner, false);
    }

    public override string ToString() {
        return $"(cell ({CellGroup.Owner}) wrong flag)";
    }
}

internal class RevealCellsHint : MoveHint {
    public RevealCellsHint(CellGroup cellGroup, MinesweeperGrid grid, MinesweeperSolver solver) : base(cellGroup, grid, solver) {
    }

    public override int Priority { get; } = 3;

    public override List<Cell> GetAffectedCells() {
        return new List<Cell>(CellGroup.Cells) { CellGroup.Owner };
    }

    public override bool IsObsolete() {
        return !CellGroup.Cells.Any(cell => !cell.IsRevealed);
    }

    public override void Solve() {
        foreach (var cell in CellGroup.Cells) {
            Grid.RevealCellCascading(cell);
        }
    }
}

public interface IPriority {
    public abstract int Priority { get; }
}