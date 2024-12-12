using UnityEngine;

[CreateAssetMenu(fileName = "TileSprites", menuName = "Scriptable Objects/TileSprites")]
public class TileSprites : ScriptableObject
{
    public Sprite notRevealed;
    public Sprite[] revealed = new Sprite[9];
    public Sprite mineClicked;
    public Sprite mineUnclicked;
    public Sprite Flag;
    public Sprite QuestionMark;
}
