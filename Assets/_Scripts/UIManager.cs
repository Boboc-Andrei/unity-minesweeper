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

    public TileSprites sprites;

    public void Awake() {
        root = GetComponent<UIDocument>().rootVisualElement;
        gridContainer = root.Q<GroupBox>("Grid");
        mineCounter = root.Q<Label>("MineCounter");
        newGameButton = root.Q<Button>("NewGameButton");
    }

    public void OnEnable() {
        newGameButton.RegisterCallback<ClickEvent>(evt => {
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

    internal void FlagCell(int row, int col) {
        Button cell = cells[row, col];
        cell.style.backgroundImage = new StyleBackground(sprites.Flag);
    }

    internal void UnflagCell(int row, int col) {
        Button cell = cells[row, col];
        cell.style.backgroundImage = new StyleBackground(sprites.notRevealed);
    }
    
    internal void UpdateMineCounter(int count) {
        if (mineCounter == null) print("mine counter is null");
        mineCounter.text = count.ToString();
    }

}
