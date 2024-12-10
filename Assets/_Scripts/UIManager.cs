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
            }
        }
    }

    public void OnCellClicked(int row, int col) {
        gameManager.OnCellClicked(row, col);
    }

    public void RevealEmptyCell(int row, int col) {
        Button cell = cells[row,col];

        ColorUtility.TryParseHtmlString("#808080", out Color color);
        cell.style.backgroundColor = new StyleColor(color);

    }

    internal void RevealMineCell(int row, int col) {
        Button cell = cells[row, col];

        ColorUtility.TryParseHtmlString("#801616", out Color color);
        cell.style.backgroundColor = new StyleColor(color);
    }
}
