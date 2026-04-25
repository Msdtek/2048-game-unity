using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameSettings settings;
    public GameObject blockPrefab;
    public Transform spawnPoint; // Blok'un ilk görüneceði yer

    public Block CreateBlock()
    {
        // Ayarlara göre üretilecek en yüksek exponent'i belirle (default 3 -> 2,4,8)
        int maxExp = 3;
        if (settings != null)
        {
            if (settings.blockSprites != null && settings.blockSprites.Length > 0)
            {
                maxExp = settings.blockSprites.Length;
            }
            else if (settings.blockColors != null && settings.blockColors.Length > 0)
            {
                maxExp = settings.blockColors.Length;
            }
        }

        // Spawn için sadece 2,4,8 isteniyorsa en fazla 3 (exponent 1..3)
        int spawnMaxExp = Mathf.Min(3, maxExp);

        int exponent = Random.Range(1, spawnMaxExp + 1); // 1..spawnMaxExp (þimdi en fazla 3)
        int value = (int)Mathf.Pow(2, exponent);

        GameObject go = Instantiate(blockPrefab, spawnPoint.position, Quaternion.identity);
        Block block = go.GetComponent<Block>();

        int index = exponent - 1;

        // Eðer sprite mevcutsa kullan, yoksa renk fallback'ine dön
        if (settings != null && settings.blockSprites != null && index >= 0 && index < settings.blockSprites.Length && settings.blockSprites[index] != null)
        {
            block.Init(value, settings.blockSprites[index]);
        }
        else if (settings != null && settings.blockColors != null && index >= 0 && index < settings.blockColors.Length)
        {
            block.Init(value, settings.blockColors[index]);
        }
        else
        {
            block.Init(value, Color.white);
        }

        // --- YENÝ: prefab'ý grid hücresine göre ölçekle ve küçük bir boþluk býrak ---
        GridManager gm = UnityEngine.Object.FindFirstObjectByType<GridManager>();
        float cellSize = gm != null ? gm.cellSize : 1f;
        // SpriteRenderer bul
        SpriteRenderer sr = go.GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            // sprite.bounds.size biriminin world birimine göre olduðunu unutma (varsayýlan scale = 1)
            float spriteWidth = sr.sprite.bounds.size.x;
            if (spriteWidth > 0f)
            {
                // Hücreye göre ölçek: 0.9f ile %10 boþluk býrakýyoruz (ayarlanabilir)
                float paddingFactor = 0.9f;
                float scale = (cellSize * paddingFactor) / spriteWidth;
                go.transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        return block;
    }
}