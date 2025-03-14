using System;
using System.Collections.Generic;



public abstract class MoveHint : IPriority {
    protected MinesweeperGrid Grid { get; }
    protected MinesweeperSolver Solver { get; }
    public Cell AffectedCell { get; set; }
    public abstract int Priority { get; }

    public MoveHint(Cell affectedCell, MinesweeperGrid grid, MinesweeperSolver solver) {
        AffectedCell = affectedCell;
        Grid = grid;
        Solver = solver;
    }

    public override bool Equals(object? obj) {
        if(obj is MoveHint other) {
            return GetType() == other.GetType() && AffectedCell == other.AffectedCell;
        }
        return false;
    }
    public override int GetHashCode() {
        return HashCode.Combine(GetType(), AffectedCell);
    }

    public abstract void Solve();
    public abstract List<Cell> GetAffectedCells();
    public abstract bool IsObsolete();
}

public class FlagsSatisfiedHint : MoveHint {
    public override int Priority { get; } = 2;
    public FlagsSatisfiedHint(Cell affectedCell, MinesweeperGrid grid, MinesweeperSolver solver) : base(affectedCell, grid, solver) { }

    public override List<Cell> GetAffectedCells() {
        List<Cell> affectedCells = Grid.GetUnrevealedNeighbours(AffectedCell, includeFlagged: true);
        affectedCells.Add(AffectedCell);
        return affectedCells;
    }

    public override bool IsObsolete() {
        return Grid.GetUnrevealedNeighbours(AffectedCell, includeFlagged: false).Count == 0;
    }

    public override void Solve() {
        Grid.RevealNeighbours(AffectedCell);
    }

    public override string ToString() {
        return $"(cell ({AffectedCell.Row}, {AffectedCell.Col}) flags satisfied)";
    }
}

public class FlagCellHint : MoveHint {
    public override int Priority { get; } = 1;
    public FlagCellHint(Cell affectedCell, MinesweeperGrid grid, MinesweeperSolver solver) : base(affectedCell, grid, solver) { }

    public override List<Cell> GetAffectedCells() {
        return new List<Cell>() { AffectedCell };
    }

    public override bool IsObsolete() {
        return AffectedCell.IsFlagged;
    }

    public override void Solve() {
        Grid.SetFlag(AffectedCell, true);
    }
    public override string ToString() {
        return $"(cell ({AffectedCell.Row}, {AffectedCell.Col}) must be flagged)";
    }
}

public class WrongFlagHint : MoveHint {
    public override int Priority { get; } = 0;
    public WrongFlagHint(Cell affectedCell, MinesweeperGrid grid, MinesweeperSolver solver) : base(affectedCell, grid, solver) { }

    public override List<Cell> GetAffectedCells() {
        return new List<Cell> { AffectedCell };
    }

    public override bool IsObsolete() {
        return !AffectedCell.IsFlagged || (AffectedCell.IsFlagged && Solver.flaggableCells.Contains(AffectedCell));
    }

    public override void Solve() {
        Grid.SetFlag(AffectedCell, false);
    }

    public override string ToString() {
        return $"(cell ({AffectedCell.Row}, {AffectedCell.Col}) wrong flag)";
    }
}

public interface IPriority {
    public abstract int Priority { get; }
}