using System.Collections;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [Header("References")]
    public Spawner spawner;
    public GridManager gridManager;

    private Block _currentBlock;
    private bool _canControl = true;

    [Header("Ghost Guide")]
    public Transform ghostGuide;

    void Start()
    {
        SpawnInitialBlock();
    }

    void Update()
    {
        if (!_canControl || _currentBlock == null)
        {
            if (ghostGuide != null) ghostGuide.gameObject.SetActive(false);
            return;
        }

        ghostGuide.gameObject.SetActive(true);
        HandleInput();
        UpdateGhostGuide();
    }

    private void UpdateGhostGuide()
    {
        if (ghostGuide != null && _currentBlock != null)
        {
            Vector3 newPos = ghostGuide.position;
            newPos.x = _currentBlock.transform.position.x;
            ghostGuide.position = newPos;
        }
    }

    private void SpawnInitialBlock()
    {
        _currentBlock = spawner.CreateBlock();
    }

    private void HandleInput()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[InputHandler] Camera.main is null. Skipping input handling.");
            return;
        }

        float targetWorldZ = gridManager != null ? gridManager.transform.position.z : 0f;

        Vector2 rawPos;
        bool pointerBegan = false;
        bool pointerEnded = false;

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            rawPos = t.position;
            pointerBegan = t.phase == TouchPhase.Began;
            pointerEnded = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
        }
        else
        {
            rawPos = Input.mousePosition;
            pointerBegan = Input.GetMouseButtonDown(0);
            pointerEnded = Input.GetMouseButtonUp(0);
        }

        float px = float.IsFinite(rawPos.x) ? rawPos.x : Screen.width * 0.5f;
        float py = float.IsFinite(rawPos.y) ? rawPos.y : Screen.height * 0.5f;
        px = Mathf.Clamp(px, 0f, Mathf.Max(0.001f, Screen.width));
        py = Mathf.Clamp(py, 0f, Mathf.Max(0.001f, Screen.height));

        Ray ray = cam.ScreenPointToRay(new Vector3(px, py, 0f));
        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, targetWorldZ));
        Vector3 worldPos;
        if (plane.Raycast(ray, out float enter))
        {
            worldPos = ray.GetPoint(enter);
        }
        else
        {
            return;
        }

        float totalWidth = gridManager.width * gridManager.cellSize;
        float boardX = gridManager.transform.position.x;
        float leftBound = boardX - (totalWidth / 2f);
        float rightBound = boardX + (totalWidth / 2f);
        float clampedWorldX = Mathf.Clamp(worldPos.x, leftBound, rightBound);

        Vector3 current = _currentBlock.transform.position;
        current.x = clampedWorldX;
        _currentBlock.transform.position = current;

        int column = Mathf.Clamp(Mathf.FloorToInt((clampedWorldX - leftBound) / gridManager.cellSize), 0, gridManager.width - 1);

        if (pointerBegan && _canControl)
        {
            _canControl = false;
            StartCoroutine(TapAndDropRoutine(column));
        }
    }

    private IEnumerator TapAndDropRoutine(int column)
    {
        if (gridManager.IsColumnFull(column))
        {
            _canControl = true;
            yield break;
        }

        int targetY = gridManager.FindBottomY(column);
        if (targetY < 0)
        {
            _canControl = true;
            yield break;
        }

        Vector3 topPos = gridManager.GetWorldPosition(column, gridManager.height - 1);
        yield return StartCoroutine(_currentBlock.MoveToTarget(topPos, 10f));
        yield return null;

        Vector3 targetPos = gridManager.GetWorldPosition(column, targetY);

        // Yeni sade düþüþ animasyonunu çaðýr
        yield return StartCoroutine(_currentBlock.FallToTargetSimple(targetPos, 0.28f, 0.14f));

        // Hafif pop
        yield return StartCoroutine(_currentBlock.PopAnimation(1.06f, 0.10f));

        gridManager.SetBlock(column, targetY, _currentBlock);
        yield return StartCoroutine(gridManager.CollapseColumnAnimated(column, 12f));

        MergeController mergeController = GetComponent<MergeController>();
        mergeController.CheckAndMerge(column, targetY);

        yield return StartCoroutine(mergeController.WaitUntilIdle());
        yield return null;

        _currentBlock = spawner.CreateBlock();
        _canControl = true;
    }
}