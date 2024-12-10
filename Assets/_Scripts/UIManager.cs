using System;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour {

    public VisualElement root;
    private VisualElement gridContainer;
    public GameManager gameManager;

    private Button[,] cells;

    public void InitializeGridUI(int rows, int cols) {
        root = GetComponent<UIDocument>().rootVisualElement;
        gridContainer = root.Q<GroupBox>("Grid");
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

        //ColorUtility.TryParseHtmlString("#808080", out Color color);
        //cell.style.backgroundColor = new StyleColor(color);

        cell.AddToClassList("cell-empty");

        if (neighbouringMines > 0) {
            cell.text = neighbouringMines.ToString();
        }
    }

    internal void RevealMineCell(int row, int col) {
        Button cell = cells[row, col];

        //ColorUtility.TryParseHtmlString("#801616", out Color color);
        //cell.style.backgroundColor = new StyleColor(color);

        cell.AddToClassList("cell-mine");
    }

    internal void FlagCell(int row, int col) {
        print($"cell {row}, {col} flagged");
        Button cell = cells[row, col];
        cell.AddToClassList("cell-flagged");
    }

    internal void UnflagCell(int row, int col) {
        print($"cell {row}, {col} unflagged");
        Button cell = cells[row, col];
        cell.RemoveFromClassList("cell-flagged");
    }
}
