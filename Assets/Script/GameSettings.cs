using UnityEngine;

// Bu satýr, Unity içinde sað týklandýðýnda "Create" menüsüne bu ayar dosyasýný ekler.
[CreateAssetMenu(fileName = "NewGameSettings", menuName = "Settings/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Block Sprites (index 0 -> 2, index 1 -> 4, ...)")]
    public Sprite[] blockSprites;

    [Header("Block Colors (fallback)")]
    // Index 0 -> 2 sayýsý için, Index 1 -> 4 sayýsý için...
    public Color[] blockColors;

    [Header("Game Physics")]
    public float dropSpeed = 10f;
    public float mergeSpeed = 0.1f;
}