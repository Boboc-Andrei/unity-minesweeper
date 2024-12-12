using System;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour {

    public VisualElement root;
    private VisualElement gridContainer;
    public GameManager gameManager;

    private Button[,] cells;
    private Button newGameButton;
    private Label mineCounter;

    public void Awake() {
        root = GetComponent<UIDocument>().rootVisualElement;
        gridContainer = root.Q<GroupBox>("Grid");
        mineCounter = root.Q<Label>("MineCounter");
        newGameButton = root.Q<Button>("NewGameButton");

        if (newGameButton == null) print("new game button is null");
        print(newGameButton);
    }

    public void OnEnable() {
        newGameButton.RegisterCallback<ClickEvent>(evt => {
            print("New game clicked");
            if (evt.button == 0) {
                gameManager.NewGame();
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
        cell.AddToClassList("cell-empty");

        if (neighbouringMines > 0) {
            cell.text = neighbouringMines.ToString();
        }
    }

    internal void RevealMineCell(int row, int col) {
        Button cell = cells[row, col];
        cell.AddToClassList("cell-mine");
    }

    internal void FlagCell(int row, int col) {
        Button cell = cells[row, col];
        cell.AddToClassList("cell-flagged");
    }

    internal void UnflagCell(int row, int col) {
        Button cell = cells[row, col];
        cell.RemoveFromClassList("cell-flagged");
    }

    internal void UpdateMineCounter(int count) {
        if (mineCounter == null) print("mine counter is null");

        mineCounter.text = count.ToString();
    }

}
