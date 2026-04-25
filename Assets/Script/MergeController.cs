using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MergeController : MonoBehaviour
{
    public GridManager gridManager;
    public ScoreManager scoreManager; // Puaný artýrmak için

    [Header("Audio")]
    public AudioSource audioSource;       // Opsiyonel: GameObject'e AudioSource ekleyip atayabilirsiniz
    public AudioClip mergeClip;          // Birleþme sesi (Inspector'dan ata)
    [Range(0f, 1f)]
    public float mergeVolume = 1f;

    private bool _isMerging = false; // Bir merge iþlemi içinde miyiz
    private bool _processingQueue = false;
    private Queue<Vector2Int> _mergeQueue = new Queue<Vector2Int>();

    // Public API: merge isteðini kuyruða ekle
    public void CheckAndMerge(int x, int y)
    {
        EnqueueMerge(x, y);
    }

    private void EnqueueMerge(int x, int y)
    {
        // Basit koruma: geçersiz koordinatlar veya null bloklarý ekleme
        if (!gridManager.IsInBounds(x, y)) return;
        if (gridManager.GetBlock(x, y) == null) return;

        _mergeQueue.Enqueue(new Vector2Int(x, y));
        if (!_processingQueue)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    private IEnumerator ProcessQueue()
    {
        _processingQueue = true;

        while (_mergeQueue.Count > 0)
        {
            Vector2Int req = _mergeQueue.Dequeue();
            yield return StartCoroutine(ProcessMerges(req.x, req.y));
            // küçük bir bekleme, Unity durumu stabil hale getirsin
            yield return null;
        }

        _processingQueue = false;
    }

    // InputHandler veya baþka yerlerin merge bitimini beklemesi için
    public IEnumerator WaitUntilIdle()
    {
        while (_processingQueue || _isMerging)
        {
            yield return null;
        }
    }

    // Bir pozisyondan baþlayarak zincirlemeyi iþleyen coroutine
    private IEnumerator ProcessMerges(int startX, int startY)
    {
        _isMerging = true;

        int x = startX;
        int y = startY;

        while (true)
        {
            Block currentBlock = gridManager.GetBlock(x, y);
            if (currentBlock == null) break;

            // Kontrol edilecek yönler: Öncelikle down, sonra left/right, ve son olarak up
            Vector2Int[] directions = { Vector2Int.down, Vector2Int.left, Vector2Int.right, Vector2Int.up };
            bool mergedThisIteration = false;

            foreach (Vector2Int dir in directions)
            {
                int checkX = x + dir.x;
                int checkY = y + dir.y;

                Block neighbor = gridManager.GetBlock(checkX, checkY);

                if (neighbor != null && neighbor.Value == currentBlock.Value)
                {
                    yield return StartCoroutine(ExecuteMergeCoroutine(currentBlock, neighbor, x, y, checkX, checkY));
                    // Yeni birleþmiþ bloðun pozisyonuna geç ve tekrar kontrol et
                    x = checkX;
                    y = checkY;
                    mergedThisIteration = true;
                    break;
                }
            }

            if (!mergedThisIteration) break;
            yield return null;
        }

        _isMerging = false;
    }

    private IEnumerator ExecuteMergeCoroutine(Block mainBlock, Block neighbor, int mainX, int mainY, int neighX, int neighY)
    {
        // 1. Eski kayýtlarý sil
        gridManager.SetBlock(mainX, mainY, null);
        gridManager.SetBlock(neighX, neighY, null);

        // 2. Yeni deðeri ayarla
        int newValue = mainBlock.Value * 2;
        Spawner spawner = GetComponent<Spawner>();
        int exponent = (int)Mathf.Log(newValue, 2);
        int index = exponent - 1;

        // Önce sprite dizisinde uygun sprite var mý diye kontrol et
        if (spawner != null && spawner.settings != null &&
            spawner.settings.blockSprites != null && index >= 0 && index < spawner.settings.blockSprites.Length &&
            spawner.settings.blockSprites[index] != null)
        {
            mainBlock.Init(newValue, spawner.settings.blockSprites[index]);
        }
        else if (spawner != null && spawner.settings != null &&
                 spawner.settings.blockColors != null && index >= 0 && index < spawner.settings.blockColors.Length)
        {
            mainBlock.Init(newValue, spawner.settings.blockColors[index]);
        }
        else
        {
            mainBlock.Init(newValue, Color.white);
        }

        // 3. Komþu bloðu yok et (Destroy frame sonunda iþler)
        Destroy(neighbor.gameObject);

        // Bekle bir frame ki Destroy gerçeklesin
        yield return null;

        // 4. Ana bloðu komþunun olduðu yere animasyonla taþý
        Vector3 targetPos = gridManager.GetWorldPosition(neighX, neighY);
        yield return StartCoroutine(mainBlock.MoveToTarget(targetPos, 15f));

        // 4.5 Pop animasyonu çal (küçük squash & stretch)
        yield return StartCoroutine(mainBlock.PopAnimation(1.2f, 0.1f));

        // 5. Yeni pozisyonu matrise kaydet
        gridManager.SetBlock(neighX, neighY, mainBlock);

        // 6. Eski sütunu animasyonlu þekilde çökert (düþür)
        yield return StartCoroutine(gridManager.CollapseColumnAnimated(mainX, 12f));

        // 7. Skoru ekle
        scoreManager.AddScore(newValue);

        // Ses çal
        PlayMergeSound();

        // 8. Collapse ve taþýnma sonrasý çevrede oluþabilecek yeni eþleþmeleri tespit et ve kuyruða ekle
        EnqueueAdjacentMergesAround(neighX, neighY);
        EnqueueAdjacentMergesAround(mainX, mainY); // mainX,mainY burada önceki pozisyonu represent eder

        // Bir frame bekle, Unity güncellemeler için güvenli
        yield return null;
    }

    // Ses oynatma yardýmcý metodu
    private void PlayMergeSound()
    {
        if (mergeClip == null) return;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(mergeClip, mergeVolume);
        }
        else
        {
            // AudioSource atanmadýysa kameranýn pozisyonunda oynat
            Camera cam = Camera.main;
            Vector3 pos = cam != null ? cam.transform.position : Vector3.zero;
            AudioSource.PlayClipAtPoint(mergeClip, pos, mergeVolume);
        }
    }

    // Verilen hücre çevresinde (4 yön) eþleþme varsa onu kuyruða ekle
    private void EnqueueAdjacentMergesAround(int x, int y)
    {
        if (!gridManager.IsInBounds(x, y)) return;

        Block center = gridManager.GetBlock(x, y);
        if (center == null) return;

        Vector2Int[] dirs = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };
        foreach (var d in dirs)
        {
            int nx = x + d.x;
            int ny = y + d.y;
            if (!gridManager.IsInBounds(nx, ny)) continue;
            Block neighbor = gridManager.GetBlock(nx, ny);
            if (neighbor != null && neighbor.Value == center.Value)
            {
                EnqueueMerge(nx, ny);
                EnqueueMerge(x, y);
            }
        }
    }
}