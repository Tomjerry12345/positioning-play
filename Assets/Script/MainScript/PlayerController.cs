//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class PlayerController : MonoBehaviour
//{
//    [Header("Movement")]
//    public float moveSpeed = 5f;
//    private bool isMoving = false;
//    private Vector3 targetPosition;

//    [Header("References")]
//    public BallController ball;
//    private GridManager gridManager; // wajib diassign
//    private GameManager gameManager;

//    [Header("Tracking Area Time")]
//    public string playerRole = "CM";  // role player
//    private float timeInRoleArea = 0f;
//    private float timeOutsideRoleArea = 0f;
//    public int totalPasses = 0;

//    [HideInInspector] public PlayerController currentHolder = null;
//    private Vector2Int movementDirAttach = Vector2Int.zero;
//    private bool isFirstAttach = false;

//    public Dictionary<string, RoleArea> roleAllowedAreas;

//    void Start()
//    {
//        targetPosition = transform.position;
//        gridManager = FindAnyObjectByType<GridManager>();

//        // cek kalau playerRole tidak ditemukan
//        if (!roleAllowedAreas.ContainsKey(playerRole))
//            Debug.LogWarning($"Role '{playerRole}' tidak ditemukan di roleAllowedAreas!");

//        PlayerPrefs.SetFloat("TimeInRoleArea", timeInRoleArea);
//        PlayerPrefs.SetFloat("TimeOutsideRoleArea", timeOutsideRoleArea);
//        PlayerPrefs.SetInt("TotalPasses", totalPasses);
//    }

//    void Update()
//    {
//        // Smooth move ke target (hanya untuk move bebas)
//        if (isMoving)
//        {
//            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
//            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
//            {
//                isMoving = false;
//                gameManager.OnPlayerMoveEnd(); // giliran AI 
//            }

//            TrackRoleAreaTime();
//        }

//        // Bola menempel saat player memegang bola
//        if (currentHolder != null)
//            AttachBallPlayer();
//    }

//    // ====================
//    // Move bebas ke posisi world
//    // ====================
//    public void MoveTo(Vector3 newPos)
//    {
//        targetPosition = newPos;
//        isMoving = true;
//    }

//    // ====================
//    // Move via grid (1 cell jika pegang bola)
//    // ====================
//    public void MoveToCell(Vector2Int clickedCellCoord, GameManager gameManager)
//    {
//        Debug.Log($"Player MoveToCell ke {clickedCellCoord}");

//        if (gridManager == null) return;

//        if (gameManager != null) this.gameManager = gameManager;

//        Cell currentCell = gridManager.GetNearestCell(transform.position);
//        if (currentCell == null) return;

//        Vector2Int playerCoord = currentCell.gridCoord;
//        Vector2Int offset = clickedCellCoord - playerCoord;

//        if (currentHolder == this)
//        {
//            // ⚡ Batasi 1 cell maksimal
//            offset.x = Mathf.Clamp(offset.x, -1, 1);
//            offset.y = Mathf.Clamp(offset.y, -1, 1);

//            movementDirAttach = offset; // arah bola
//        }

//        Vector2Int targetCoord = playerCoord + offset;
//        Cell targetCell = gridManager.GetCellAt(targetCoord);
//        if (targetCell != null)
//        {
//            StartCoroutine(MoveCoroutine(targetCell.transform.position));
//        }
//    }

//    // ====================
//    // Coroutine smooth move (1 cell)
//    // ====================
//    private IEnumerator MoveCoroutine(Vector3 targetPos)
//    {
//        Vector3 startPos = transform.position;
//        float duration = 0.6f;
//        float elapsed = 0f;

//        isMoving = true;

//        AudioManager.instance.PlaySFX(AudioManager.instance.sfxDribble);

//        while (elapsed < duration)
//        {
//            elapsed += Time.deltaTime;
//            float t = elapsed / duration;
//            transform.position = Vector3.Lerp(startPos, targetPos, t);
//            yield return null;
//        }

//        transform.position = targetPos;
//        isMoving = false;

//        gameManager.OnPlayerMoveEnd(); // giliran AI
//    }

//    private void TrackRoleAreaTime()
//    {
//        if (gridManager == null) return;

//        Cell currentCell = gridManager.GetNearestCell(transform.position);
//        if (currentCell == null) return;

//        Vector2Int pos = currentCell.gridCoord;

//        // 🔥 Cek apakah role ada di dictionary
//        if (!roleAllowedAreas.ContainsKey(playerRole))
//        {
//            Debug.LogWarning($"Role {playerRole} tidak ditemukan di roleAllowedAreas!");
//            return;
//        }

//        RoleArea area = roleAllowedAreas[playerRole];

//        // 🔥 GUNAKAN METHOD Contains() yang sudah kita buat
//        bool inside = area.Contains(pos);

//        if (inside)
//            timeInRoleArea += Time.deltaTime;
//        else
//            timeOutsideRoleArea += Time.deltaTime;

//        // Save ke PlayerPrefs
//        PlayerPrefs.SetFloat($"TimeInRoleArea_{playerRole}", timeInRoleArea);
//        PlayerPrefs.SetFloat($"TimeOutsideRoleArea_{playerRole}", timeOutsideRoleArea);
//    }

//    public void TryPassBall(Vector3 worldPos)
//    {
//        if (currentHolder != this) return;

//        // cek collider AI langsung
//        Collider2D hit = Physics2D.OverlapPoint(worldPos);
//        if (hit != null)
//        {
//            AIController ai = hit.GetComponent<AIController>();
//            if (ai != null)
//            {
//                PassTo(ai);
//                return;
//            }
//        }

//        // 2️⃣ Jika tidak tepat mengenai collider -> cari AI terdekat
//        AIController[] allAIs = FindObjectsOfType<AIController>();
//        AIController nearest = null;
//        float nearestDist = 999f;

//        foreach (var ai in allAIs)
//        {
//            float dist = Vector3.Distance(ai.transform.position, worldPos);

//            if (dist < nearestDist)
//            {
//                nearestDist = dist;
//                nearest = ai;
//            }
//        }

//        // 3️⃣ Jika ada AI cukup dekat, tetap lakukan passing
//        if (nearest != null && nearestDist < 3.5f) // bisa ubah 3.5f -> lebih fleksibel
//        {
//            PassTo(nearest);
//        }
//        else
//        {
//            Debug.Log("⚠ Tidak ada AI di sekitar area klik.");
//        }
//    }

//    public void ReceiveBall()
//    {
//        currentHolder = this;
//        isFirstAttach = true; // tandai ini attach pertama
//    }

//    private void PassTo(AIController ai)
//    {
//        var teamAI = FindAnyObjectByType<TeamAIManager>();

//        if (teamAI != null)
//            StartCoroutine(teamAI.PassBallFromPlayer(this, ai));

//        gameManager.OnPlayerMoveEnd();
//        totalPasses += 1;
//        PlayerPrefs.SetInt("TotalPasses", totalPasses);
//    }

//    private Vector2Int GetSafeAttachDirection()
//    {
//        Vector2Int[] dirs = new Vector2Int[]
//        {
//        new Vector2Int(0, 1),   // depan
//        new Vector2Int(0, -1),  // belakang
//        new Vector2Int(1, 0),   // kanan
//        new Vector2Int(-1, 0),  // kiri
//        };

//        AIController[] enemies = FindObjectsOfType<AIController>();

//        //Debug.Log($"Enemies count: {enemies.Length}");

//        Vector2Int safestDir = dirs[0];
//        float safestDistance = -999f;

//        foreach (var dir in dirs)
//        {
//            // ubah ke world offset buat hitung jarak ke musuh
//            Vector3 offset = new Vector3(dir.x * 0.14f, dir.y * 0.14f, 0f);
//            Vector3 attachPos = transform.position + offset;

//            float minDist = 999f;

//            foreach (var enemy in enemies)
//            {
//                float d = Vector3.Distance(attachPos, enemy.transform.position);
//                if (d < minDist)
//                    minDist = d;
//            }

//            if (minDist > safestDistance)
//            {
//                safestDistance = minDist;
//                safestDir = dir;
//            }
//        }

//        return safestDir;
//    }


//    // ====================
//    // Bola menempel ke player
//    // ====================
//    //private void AttachBallPlayer()
//    //{
//    //    Debug.Log($"AttachBallPlayer dipanggil : {currentHolder}");
//    //    if (currentHolder == null)
//    //    {
//    //        movementDirAttach = GetSafeAttachDirection();
//    //        //return;
//    //    }

//    //    //PlayerController to = currentHolder;
//    //    float attachSpeed = 1f;
//    //    Vector3 baseOffset = new Vector3(0f, -0.14f, 0f);
//    //    Vector3 finalOffset = baseOffset;

//    //    Vector2Int dir = movementDirAttach;

//    //    //Debug.Log($"dir player : {dir}");

//    //    if (dir == Vector2Int.zero)
//    //    {
//    //        ball.transform.position = Vector3.MoveTowards(ball.transform.position, currentHolder.transform.position + finalOffset, attachSpeed * Time.deltaTime);
//    //        return;
//    //    }

//    //    // Mundur / maju / kiri / kanan / diagonal
//    //    if (dir.y == -1 && dir.x == 0) finalOffset = new Vector3(0f, -0.14f, 0f);
//    //    else if (dir.y == 1 && dir.x == 0) finalOffset = new Vector3(0f, 0.14f, 0f);
//    //    else if (dir.x == -1 && dir.y == 0) finalOffset = new Vector3(-0.14f, 0f, 0f);
//    //    else if (dir.x == 1 && dir.y == 0) finalOffset = new Vector3(0.14f, 0f, 0f);
//    //    else finalOffset = new Vector3(dir.x * 0.10f, dir.y * 0.10f, 0);

//    //    ball.transform.position = currentHolder.transform.position + finalOffset;
//    //}

//    private void AttachBallPlayer()
//    {
//        PlayerController to = currentHolder;
//        float attachSpeed = 1f;

//        Vector3 finalOffset = Vector3.zero;

//        if (isFirstAttach)
//        {
//            // pilih arah yang paling aman dari musuh
//            movementDirAttach = GetSafeAttachDirection();
//            isFirstAttach = false; // cukup sekali
//        }

//        // hitung offset berdasarkan movementDirAttach
//        if (movementDirAttach == Vector2Int.zero)
//            finalOffset = new Vector3(0f, -0.14f, 0f);
//        else
//            finalOffset = new Vector3(
//                movementDirAttach.x * 0.14f,
//                movementDirAttach.y * 0.14f,
//                0f
//            );

//        ball.transform.position =
//            Vector3.MoveTowards(ball.transform.position,
//            to.transform.position + finalOffset,
//            attachSpeed * Time.deltaTime);
//    }

//}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    private bool isMoving = false;
    private Vector3 targetPosition;

    [Header("References")]
    [HideInInspector] public BallController ball;
    private GridManager gridManager;
    private GameManager gameManager;
    private EnemyAIManager enemyAI; // ✅ TAMBAHAN BARU

    [Header("Tracking Area Time")]
    public string playerRole = "CM";
    private float timeInRoleArea = 0f;
    private float timeOutsideRoleArea = 0f;
    public int totalPasses = 0;

    [HideInInspector] public PlayerController currentHolder = null;
    private Vector2Int movementDirAttach = Vector2Int.zero;
    private bool isFirstAttach = false;

    public Dictionary<string, RoleArea> roleAllowedAreas;

    [Header("Interception Settings")] // ✅ TAMBAHAN BARU
    [Tooltip("Jarak enemy untuk intercept bola saat player bawa bola")]
    public float carryInterceptionRadius = 0.11f; // 0.8 cell * 0.14

    void Start()
    {
        targetPosition = transform.position;
        gridManager = FindAnyObjectByType<GridManager>();

        // ✅ AUTO FIND EnemyAIManager (tidak perlu drag)
        if (enemyAI == null)
        {
            enemyAI = FindAnyObjectByType<EnemyAIManager>();

            if (enemyAI == null)
                Debug.LogWarning("⚠️ EnemyAIManager tidak ditemukan di scene!");
            else
                Debug.Log("✅ EnemyAIManager berhasil ditemukan otomatis!");
        }

        if (!roleAllowedAreas.ContainsKey(playerRole))
            Debug.LogWarning($"Role '{playerRole}' tidak ditemukan di roleAllowedAreas!");

        PlayerPrefs.SetFloat("TimeInRoleArea", timeInRoleArea);
        PlayerPrefs.SetFloat("TimeOutsideRoleArea", timeOutsideRoleArea);
        PlayerPrefs.SetInt("TotalPasses", totalPasses);
    }

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                isMoving = false;
                gameManager.OnPlayerMoveEnd();
            }

            TrackRoleAreaTime();
        }

        if (currentHolder != null)
        {
            AttachBallPlayer();

            // ✅ TAMBAHAN BARU - Cek interception saat bawa bola
            CheckBallCarryInterception();
        }
    }

    // ✅ METHOD BARU - Cek enemy kena bola saat player bawa bola
    private void CheckBallCarryInterception()
    {
        if (currentHolder != this || ball == null || enemyAI == null) return;

        Vector3 ballPos = ball.transform.position;

        foreach (var enemy in enemyAI.enemyTeammates)
        {
            if (enemy == null) continue;

            float dist = Vector3.Distance(ballPos, enemy.transform.position);

            if (dist < carryInterceptionRadius)
            {
                //Debug.LogError($"❌ Enemy {enemy.assignedRole} intercept bola yang dibawa player!");

                GameOverManager gameOver = FindAnyObjectByType<GameOverManager>();
                if (gameOver != null)
                {
                    gameOver.ShowGameOver();
                }

                currentHolder = null;
                return;
            }
        }
    }

    public void MoveTo(Vector3 newPos)
    {
        targetPosition = newPos;
        isMoving = true;
    }

    public void MoveToCell(Vector2Int clickedCellCoord, GameManager gameManager)
    {
        //Debug.Log($"Player MoveToCell ke {clickedCellCoord}");

        if (gridManager == null) return;

        if (gameManager != null) this.gameManager = gameManager;

        Cell currentCell = gridManager.GetNearestCell(transform.position);
        if (currentCell == null) return;

        Vector2Int playerCoord = currentCell.gridCoord;
        Vector2Int offset = clickedCellCoord - playerCoord;

        if (currentHolder == this)
        {
            offset.x = Mathf.Clamp(offset.x, -1, 1);
            offset.y = Mathf.Clamp(offset.y, -1, 1);

            movementDirAttach = offset;
        }

        Vector2Int targetCoord = playerCoord + offset;
        Cell targetCell = gridManager.GetCellAt(targetCoord);
        if (targetCell != null)
        {
            StartCoroutine(MoveCoroutine(targetCell.transform.position));
        }
    }

    private IEnumerator MoveCoroutine(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float duration = 0.6f;
        float elapsed = 0f;

        isMoving = true;

        AudioManager.instance.PlaySFX(AudioManager.instance.sfxDribble);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;

        gameManager.OnPlayerMoveEnd();
    }

    private void TrackRoleAreaTime()
    {
        if (gridManager == null) return;

        Cell currentCell = gridManager.GetNearestCell(transform.position);
        if (currentCell == null) return;

        Vector2Int pos = currentCell.gridCoord;

        if (!roleAllowedAreas.ContainsKey(playerRole))
        {
            Debug.LogWarning($"Role {playerRole} tidak ditemukan di roleAllowedAreas!");
            return;
        }

        RoleArea area = roleAllowedAreas[playerRole];
        bool inside = area.Contains(pos);

        if (inside)
        {
            timeInRoleArea += Time.deltaTime;
            //Debug.Log($"timeInRoleArea : {timeInRoleArea}");
        }
        else
        {
            timeOutsideRoleArea += Time.deltaTime;
            //Debug.Log($"timeOutsideRoleArea : {timeOutsideRoleArea}");
        }

        PlayerPrefs.SetFloat($"TimeInRoleArea", timeInRoleArea);
        PlayerPrefs.SetFloat($"TimeOutsideRoleArea", timeOutsideRoleArea);

    }

    public void TryPassBall(Vector3 worldPos)
    {
        if (currentHolder != this) return;

        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        if (hit != null)
        {
            AIController ai = hit.GetComponent<AIController>();
            if (ai != null)
            {
                PassTo(ai);
                return;
            }
        }

        AIController[] allAIs = FindObjectsOfType<AIController>();
        AIController nearest = null;
        float nearestDist = 999f;

        foreach (var ai in allAIs)
        {
            float dist = Vector3.Distance(ai.transform.position, worldPos);

            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = ai;
            }
        }

        if (nearest != null && nearestDist < 3.5f)
        {
            PassTo(nearest);
        }
        else
        {
            Debug.Log("⚠ Tidak ada AI di sekitar area klik.");
        }
    }

    public void ReceiveBall()
    {
        currentHolder = this;
        isFirstAttach = true;
    }

    private void PassTo(AIController ai)
    {
        var teamAI = FindAnyObjectByType<TeamAIManager>();

        if (teamAI != null)
            StartCoroutine(teamAI.PassBallFromPlayer(this, ai));

        gameManager.OnPlayerMoveEnd();
        totalPasses += 1;
        PlayerPrefs.SetInt("TotalPasses", totalPasses);
    }

    private Vector2Int GetSafeAttachDirection()
    {
        Vector2Int[] dirs = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
        };

        AIController[] enemies = FindObjectsOfType<AIController>();

        Vector2Int safestDir = dirs[0];
        float safestDistance = -999f;

        foreach (var dir in dirs)
        {
            Vector3 offset = new Vector3(dir.x * 0.14f, dir.y * 0.14f, 0f);
            Vector3 attachPos = transform.position + offset;

            float minDist = 999f;

            foreach (var enemy in enemies)
            {
                float d = Vector3.Distance(attachPos, enemy.transform.position);
                if (d < minDist)
                    minDist = d;
            }

            if (minDist > safestDistance)
            {
                safestDistance = minDist;
                safestDir = dir;
            }
        }

        return safestDir;
    }

    private void AttachBallPlayer()
    {
        PlayerController to = currentHolder;
        float attachSpeed = 1f;

        Vector3 finalOffset = Vector3.zero;

        if (isFirstAttach)
        {
            movementDirAttach = GetSafeAttachDirection();
            isFirstAttach = false;
        }

        if (movementDirAttach == Vector2Int.zero)
            finalOffset = new Vector3(0f, -0.14f, 0f);
        else
            finalOffset = new Vector3(
                movementDirAttach.x * 0.14f,
                movementDirAttach.y * 0.14f,
                0f
            );

        ball.transform.position =
            Vector3.MoveTowards(ball.transform.position,
            to.transform.position + finalOffset,
            attachSpeed * Time.deltaTime);
    }
}