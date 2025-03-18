using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.Burst.CompilerServices;
using UnityEditor.SpeedTree.Importer;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour {

    [SerializeField]
    private bool ShowActiveCells = true;

    public VisualElement root;
    public GameManager gameManager;

    private Button newGameButton;
    private Label hintLabel;
    private Button hintButton;
    private Button doHintButton;
    private Button solveButton;
    private Label mineCounter;
    public EnumField difficultyDropDown;

    private VisualElement gridContainer;
    private Button[,] cells;
    private int Rows, Cols;


    public TileSprites sprites;

    public void Awake() {
        root = GetComponent<UIDocument>().rootVisualElement;
        gridContainer = root.Q<GroupBox>("Grid");
        mineCounter = root.Q<Label>("MineCounter");
        newGameButton = root.Q<Button>("NewGameButton");
        hintLabel = root.Q<Label>("HintLabel");
        hintButton = root.Q<Button>("HintButton");
        doHintButton = root.Q<Button>("DoHintButton");
        solveButton = root.Q<Button>("SolveButton");
        difficultyDropDown = root.Q<EnumField>("DifficultyDropdown");

        difficultyDropDown.Init(Difficulty.Hard);
    }

    public void OnEnable() {
        GameEvents.OnGridInitialized += InitializeGridUI;
        GameEvents.OnEmptyCellRevealed += RevealEmptyCell;
        GameEvents.OnMineCellRevealed += RevealMineCell;
        GameEvents.OnFlagSet += SetFlag;
        GameEvents.OnCellsHighlighted += HighlightCells;
        GameEvents.OnFlagCounterUpdate += UpdateMineCounter;
        GameEvents.OnUpdateActiveCells += HighlightActiveCells;

        newGameButton.RegisterCallback<ClickEvent>(evt => {
            if (evt.button == 0) {
                GameEvents.NewGame();
            }
        });

        hintButton.RegisterCallback<ClickEvent>(evt => {
            if (evt.button == 0) {
                gameManager.ShowHint();
            }
        });

        doHintButton.RegisterCallback<ClickEvent>(evt => {
            if (evt.button == 0) {
                gameManager.PerformHint();
            }
        });

        solveButton.RegisterCallback<ClickEvent>(evt => {
            if (evt.button == 0) {
                gameManager.SolveGrid();
            }
        });

        difficultyDropDown.RegisterCallback<ChangeEvent<Enum>>((evt) => {
            GameEvents.DifficultyChanged((Difficulty)evt.newValue);
        });
    }

    public void OnDisable() {
        GameEvents.OnGridInitialized -= InitializeGridUI;
        GameEvents.OnEmptyCellRevealed -= RevealEmptyCell;
        GameEvents.OnMineCellRevealed -= RevealMineCell;
        GameEvents.OnFlagSet -= SetFlag;
        GameEvents.OnCellsHighlighted -= HighlightCells;
        GameEvents.OnFlagCounterUpdate -= UpdateMineCounter;
    }

    public void InitializeGridUI(int rows, int cols, Difficulty difficulty) {
        gridContainer.Clear();
        difficultyDropDown.value = difficulty;
        Rows = rows;
        Cols = cols;
        cells = new Button[Rows, Cols];

        for (int row = 0; row < Rows; row++) {
            VisualElement rowContainer = new VisualElement();
            rowContainer.AddToClassList("row");
            gridContainer.Add(rowContainer);

            for (int col = 0; col < Cols; col++) {
                Button button = new Button();
                button.AddToClassList("cell");
                button.style.backgroundImage = new StyleBackground(sprites.notRevealed);
                rowContainer.Add(button);
                cells[row, col] = button;

                int r = row;
                int c = col;

                button.RegisterCallback<PointerUpEvent>(evt => {
                    if (evt.button == 0) {
                        GameEvents.CellClicked(r, c);
                    }
                    else if (evt.button == 1) {
                        GameEvents.CellRightClicked(r, c);
                    }
                });
            }
        }
    }

    public void RevealEmptyCell(int row, int col, int neighbouringMines) {
        Button cell = cells[row, col];
        cell.style.backgroundImage = new StyleBackground(sprites.revealed[neighbouringMines]);
    }

    internal void RevealMineCell(int row, int col) {
        Button cell = cells[row, col];
        cell.style.backgroundImage = new StyleBackground(sprites.mineClicked);
    }

    internal void SetFlag(int row, int col, bool isFlag) {
        Button cell = cells[row, col];
        cell.style.backgroundImage = isFlag ? new StyleBackground(sprites.Flag) : new StyleBackground(sprites.notRevealed);
    }

    internal void UpdateMineCounter(int count) {
        if (mineCounter == null) print("mine counter is null");
        mineCounter.text = count.ToString();
    }

    public void HighlightCells(List<(int, int)> cellsToHighlight) {
        foreach(var (row, col) in cellsToHighlight) {
            StartCoroutine(HighlightCellForSeconds(cells[row, col], 1));
        }
    }

    private void HighlightActiveCells(List<(int, int)> activeCells) {
        if (!ShowActiveCells) return;
        ResetActiveCellsHighlight();
        foreach(var (row, col) in activeCells) {
            cells[row, col].AddToClassList("active");
        }
    }
    private void ResetActiveCellsHighlight() {
        for(int row = 0; row < Rows; row++) {
            for(int col = 0; col < Cols; col++) {
                cells[row, col].RemoveFromClassList("active");
            }
        }
    }

    public IEnumerator HighlightCellForSeconds(Button cell, float time) {
        cell.AddToClassList("highlighted");
        yield return new WaitForSeconds(time);
        cell.RemoveFromClassList("highlighted");
    }
}
