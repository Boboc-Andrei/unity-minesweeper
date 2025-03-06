using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour {

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
        newGameButton.RegisterCallback<ClickEvent>(evt => {
            if (evt.button == 0) {
                gameManager.NewGame();
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
    }

    public void InitializeGridUI(int rows, int cols) {

        gridContainer.Clear();
        cells = new Button[rows, cols];

        for (int row = 0; row < rows; row++) {
            VisualElement rowContainer = new VisualElement();
            rowContainer.AddToClassList("row");
            gridContainer.Add(rowContainer);

            for (int col = 0; col < cols; col++) {
                Button button = new Button();
                button.AddToClassList("cell");
                button.style.backgroundImage = new StyleBackground(sprites.notRevealed);
                rowContainer.Add(button);
                cells[row, col] = button;

                int r = row;
                int c = col;

                button.clicked += () => OnCellClicked(r, c);

                button.RegisterCallback<PointerDownEvent>(evt => {
                    if (evt.button == 0) {
                        OnCellClicked(r, c);
                    }
                    else if (evt.button == 1) {
                        OnCellRightClicked(r, c);
                    }
                });
            }
        }
    }

    private void OnCellRightClicked(int row, int col) {
        gameManager.OnCellRightClicked(row, col);
    }

    public void OnCellClicked(int row, int col) {
        gameManager.OnCellClicked(row, col);
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

    public void HighlightCells(List<Cell> cellsToHighlight) {
        foreach(var cell in cellsToHighlight) {
            StartCoroutine(HighlightCellForSeconds(cells[cell.Row, cell.Col], 1));
        }
    }

    public IEnumerator HighlightCellForSeconds(Button cell, float time) {
        cell.AddToClassList("highlighted");
        yield return new WaitForSeconds(time);
        cell.RemoveFromClassList("highlighted");
    }
}
