using System.Collections;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 5;
    public int height = 7;
    public float cellSize = 1.2f; // Bloklar arasý boþluk için

    [Header("Layout")]
    [Tooltip("Pozitif deðer board'u aþaðý doðru kaydýrýr (Inspector'dan ayarlanýr).")]
    public float boardYOffset = 0f;

    private Block[,] _grid;
    internal object settings;


    void Awake()
    {
        _grid = new Block[width, height];
    }
    public Vector3 GetWorldPosition(int x, int y)
    {
        // Grid'i Board objesinin olduðu yere göre hesaplýyoruz
        float totalWidth = width * cellSize;
        float totalHeight = height * cellSize;

        // Board'un (Gri Kutu) dünya pozisyonunu baz alýyoruz
        Vector3 boardPos = transform.position;

        // Inspector'dan verilen offset kadar board'u aþaðý kaydýr
        boardPos.y -= boardYOffset;

        float offsetX = boardPos.x - (totalWidth / 2f) + (cellSize / 2f);
        float offsetY = boardPos.y - (totalHeight / 2f) + (cellSize / 2f);

        return new Vector3(x * cellSize + offsetX, y * cellSize + offsetY, 0);
    }

    public bool IsInBounds(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;

    public Block GetBlock(int x, int y) => IsInBounds(x, y) ? _grid[x, y] : null;

    public void SetBlock(int x, int y, Block block)
    {
        // Koordinatlarýn grid sýnýrlarý içinde olduðunu kontrol et
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            _grid[x, y] = block;
        }
    }

    public bool IsColumnFull(int x)
    {
        // En üst satýr (Height - 1) doluysa sütun doludur
        return GetBlock(x, height - 1) != null;
    }

    public int FindBottomY(int column)
    {
        // Aþaðýdan yukarýya doðru (0'dan height-1'e) ilk boþ yeri bulalým
        for (int y = 0; y < height; y++)
        {
            if (_grid[column, y] == null)
            {
                return y; // En alt boþ yeri döndürür
            }
        }
        return -1; // Sütun doluysa
    }

    // Mevcut anlýk (instant) sürüm: (korunuyor)
    public void CollapseColumn(int x)
    {
        if (x < 0 || x >= width) return;

        for (int y = 0; y < height; y++)
        {
            if (_grid[x, y] == null)
            {
                int k = y + 1;
                while (k < height && _grid[x, k] == null) k++;

                if (k >= height) break;

                Block moving = _grid[x, k];
                _grid[x, y] = moving;
                _grid[x, k] = null;

                if (moving != null)
                {
                    moving.transform.position = GetWorldPosition(x, y);
                }
            }
        }
    }

    // Yeni: Animasyonlu (koroutine) collapse. Her taþýnmasýný animasyonla bekler.
    public IEnumerator CollapseColumnAnimated(int x, float dropSpeed)
    {
        if (x < 0 || x >= width) yield break;

        for (int y = 0; y < height; y++)
        {
            if (_grid[x, y] == null)
            {
                int k = y + 1;
                while (k < height && _grid[x, k] == null) k++;

                if (k >= height) break;

                Block moving = _grid[x, k];
                _grid[x, y] = moving;
                _grid[x, k] = null;

                if (moving != null)
                {
                    // Animasyonlu olarak hedefe gitmesini saðla ve bitmesini bekle
                    yield return StartCoroutine(moving.MoveToTarget(GetWorldPosition(x, y), dropSpeed));
                }

                // Bir frame boþluk býrakmak çoðu zaman Unity'nin Destroy iþlemlerini iþlemesi için faydalý olur
                yield return null;
            }
        }
    }
}