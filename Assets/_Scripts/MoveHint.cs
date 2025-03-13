using System;
using System.Collections.Generic;

public abstract class MoveHint {
    protected MinesweeperGrid Grid { get; }
    public Cell AffectedCell { get; set; }

    public MoveHint(Cell affectedCell, MinesweeperGrid grid) {
        AffectedCell = affectedCell;
        Grid = grid;
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
    public FlagsSatisfiedHint(Cell affectedCell, MinesweeperGrid grid) : base(affectedCell, grid) { }

    public override List<Cell> GetAffectedCells() {
        List<Cell> affectedCells = Grid.GetUnrevealedNeighbours(AffectedCell, includeFlagged: true);
        affectedCells.Add(AffectedCell);
        return affectedCells;
    }

    public override bool IsObsolete() {
        return Grid.GetUnrevealedNeighbours(AffectedCell, includeFlagged: true).Count == 0;
    }

    public override void Solve() {
        Grid.RevealNeighbours(AffectedCell);
    }
}

public class FlagCellHint : MoveHint {
    public FlagCellHint(Cell affectedCell, MinesweeperGrid grid) : base(affectedCell, grid) { }

    public override List<Cell> GetAffectedCells() {
        return new List<Cell>() { AffectedCell };
    }

    public override bool IsObsolete() {
        return AffectedCell.IsFlagged;
    }

    public override void Solve() {
        Grid.SetFlag(AffectedCell, true);
    }
}

public class WrongFlagHint : MoveHint {
    public WrongFlagHint(Cell affectedCell, MinesweeperGrid grid) : base(affectedCell, grid) { }

    public override List<Cell> GetAffectedCells() {
        return new List<Cell> { AffectedCell };
    }

    public override bool IsObsolete() {
        return !AffectedCell.IsFlagged;
    }

    public override void Solve() {
        Grid.SetFlag(AffectedCell, false);
    }
}