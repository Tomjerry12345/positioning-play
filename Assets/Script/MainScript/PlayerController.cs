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
//    [HideInInspector] public BallController ball;
//    private GridManager gridManager;
//    private GameManager gameManager;
//    private EnemyAIManager enemyAI; // ✅ TAMBAHAN BARU

//    [Header("Tracking Area Time")]
//    public string playerRole = "CM";
//    private float timeInRoleArea = 0f;
//    private float timeOutsideRoleArea = 0f;
//    public int totalPasses = 0;

//    [HideInInspector] public PlayerController currentHolder = null;
//    private Vector2Int movementDirAttach = Vector2Int.zero;
//    private bool isFirstAttach = false;

//    public Dictionary<string, RoleArea> roleAllowedAreas;

//    [Header("Interception Settings")] // ✅ TAMBAHAN BARU
//    [Tooltip("Jarak enemy untuk intercept bola saat player bawa bola")]
//    public float carryInterceptionRadius = 0.11f; // 0.8 cell * 0.14

//    void Start()
//    {
//        targetPosition = transform.position;
//        gridManager = FindAnyObjectByType<GridManager>();

//        // ✅ AUTO FIND EnemyAIManager (tidak perlu drag)
//        if (enemyAI == null)
//        {
//            enemyAI = FindAnyObjectByType<EnemyAIManager>();

//            if (enemyAI == null)
//                Debug.LogWarning("⚠️ EnemyAIManager tidak ditemukan di scene!");
//            else
//                Debug.Log("✅ EnemyAIManager berhasil ditemukan otomatis!");
//        }

//        if (!roleAllowedAreas.ContainsKey(playerRole))
//            Debug.LogWarning($"Role '{playerRole}' tidak ditemukan di roleAllowedAreas!");

//        PlayerPrefs.SetFloat("TimeInRoleArea", timeInRoleArea);
//        PlayerPrefs.SetFloat("TimeOutsideRoleArea", timeOutsideRoleArea);
//        PlayerPrefs.SetInt("TotalPasses", totalPasses);
//    }

//    void Update()
//    {
//        if (isMoving)
//        {
//            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
//            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
//            {
//                isMoving = false;
//                gameManager.OnPlayerMoveEnd();
//            }

//            TrackRoleAreaTime();
//        }

//        if (currentHolder != null)
//        {
//            AttachBallPlayer();

//            // ✅ TAMBAHAN BARU - Cek interception saat bawa bola
//            CheckBallCarryInterception();
//        }
//    }

//    // ✅ METHOD BARU - Cek enemy kena bola saat player bawa bola
//    private void CheckBallCarryInterception()
//    {
//        if (currentHolder != this || ball == null || enemyAI == null) return;

//        Vector3 ballPos = ball.transform.position;

//        foreach (var enemy in enemyAI.enemyTeammates)
//        {
//            if (enemy == null) continue;

//            float dist = Vector3.Distance(ballPos, enemy.transform.position);

//            if (dist < carryInterceptionRadius)
//            {
//                //Debug.LogError($"❌ Enemy {enemy.assignedRole} intercept bola yang dibawa player!");

//                GameOverManager gameOver = FindAnyObjectByType<GameOverManager>();
//                if (gameOver != null)
//                {
//                    gameOver.ShowGameOver();
//                }

//                currentHolder = null;
//                return;
//            }
//        }
//    }

//    public void MoveTo(Vector3 newPos)
//    {
//        targetPosition = newPos;
//        isMoving = true;
//    }

//    public void MoveToCell(Vector2Int clickedCellCoord, GameManager gameManager)
//    {
//        //Debug.Log($"Player MoveToCell ke {clickedCellCoord}");

//        if (gridManager == null) return;

//        if (gameManager != null) this.gameManager = gameManager;

//        Cell currentCell = gridManager.GetNearestCell(transform.position);
//        if (currentCell == null) return;

//        Vector2Int playerCoord = currentCell.gridCoord;
//        Vector2Int offset = clickedCellCoord - playerCoord;

//        if (currentHolder == this)
//        {
//            offset.x = Mathf.Clamp(offset.x, -1, 1);
//            offset.y = Mathf.Clamp(offset.y, -1, 1);

//            movementDirAttach = offset;
//        }

//        Vector2Int targetCoord = playerCoord + offset;
//        Cell targetCell = gridManager.GetCellAt(targetCoord);
//        if (targetCell != null)
//        {
//            StartCoroutine(MoveCoroutine(targetCell.transform.position));
//        }
//    }

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

//        gameManager.OnPlayerMoveEnd();
//    }

//    private void TrackRoleAreaTime()
//    {
//        if (gridManager == null) return;

//        Cell currentCell = gridManager.GetNearestCell(transform.position);
//        if (currentCell == null) return;

//        Vector2Int pos = currentCell.gridCoord;

//        if (!roleAllowedAreas.ContainsKey(playerRole))
//        {
//            Debug.LogWarning($"Role {playerRole} tidak ditemukan di roleAllowedAreas!");
//            return;
//        }

//        RoleArea area = roleAllowedAreas[playerRole];
//        bool inside = area.Contains(pos);

//        if (inside)
//        {
//            timeInRoleArea += Time.deltaTime;
//            //Debug.Log($"timeInRoleArea : {timeInRoleArea}");
//        }
//        else
//        {
//            timeOutsideRoleArea += Time.deltaTime;
//            //Debug.Log($"timeOutsideRoleArea : {timeOutsideRoleArea}");
//        }

//        PlayerPrefs.SetFloat($"TimeInRoleArea", timeInRoleArea);
//        PlayerPrefs.SetFloat($"TimeOutsideRoleArea", timeOutsideRoleArea);

//    }

//    public void TryPassBall(Vector3 worldPos)
//    {
//        if (currentHolder != this) return;

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

//        if (nearest != null && nearestDist < 3.5f)
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
//        isFirstAttach = true;
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
//            new Vector2Int(0, 1),
//            new Vector2Int(0, -1),
//            new Vector2Int(1, 0),
//            new Vector2Int(-1, 0),
//        };

//        AIController[] enemies = FindObjectsOfType<AIController>();

//        Vector2Int safestDir = dirs[0];
//        float safestDistance = -999f;

//        foreach (var dir in dirs)
//        {
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

//    private void AttachBallPlayer()
//    {
//        PlayerController to = currentHolder;
//        float attachSpeed = 1f;

//        Vector3 finalOffset = Vector3.zero;

//        if (isFirstAttach)
//        {
//            movementDirAttach = GetSafeAttachDirection();
//            isFirstAttach = false;
//        }

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
    private EnemyAIManager enemyAI;

    [Header("Positioning")]
    public string playerRole = "DM";
    private float timeInAreaIdeal = 0f;
    private float timeInAreaNetral = 0f;
    private float scorePositioning = 0f;


    [Header("Passing")]
    private int passingIdeal = 0;
    private int passingNetral = 0;
    private int scorePassing = 0;

    [Header("Feedback")]
    private int indexFeedback = 0;

    private AreaType currentAreaType = AreaType.NETRAL;

    public enum AreaType
    {
        IDEAL,    // Zona sesuai role → +1 poin per detik
        NETRAL,   // Zona aman tapi bukan ideal → 0 poin
        RAWAN     // Zona berbahaya → tidak dihitung
    }

    [HideInInspector] public PlayerController currentHolder = null;
    private Vector2Int movementDirAttach = Vector2Int.zero;
    private bool isFirstAttach = false;

    [Header("Interception Settings")]
    [Tooltip("Jarak enemy untuk intercept bola saat player bawa bola")]
    public float carryInterceptionRadius = 0.11f;

    // ✅ STRUKTUR AREA DENGAN 3 TIPE ZONA (Menggunakan class dari GridManager)
    private Dictionary<string, RoleAreaWithTypes> roleAreasWithTypes = new Dictionary<string, RoleAreaWithTypes>
    {
        // ⚽ CENTRAL MIDFIELDER
        {
            "CM", new RoleAreaWithTypes
            {
                idealZones = new List<ZoneRect>
                {
                    new ZoneRect(2, 5, 6, 10),  // Left-center zone
                    new ZoneRect(0, 4, 1, 8),   // Extended forward zone
                    new ZoneRect(7, 4, 8, 8)    // Extended forward zone
                },
                netralZones = new List<ZoneRect>
                {
                    new ZoneRect(0, 3, 8, 4),
                }
                // RAWAN = semua area lainnya (otomatis)
            }
        },

        // ⚽ DEFENSIVE MIDFIELDER
        {
            "DM", new RoleAreaWithTypes
            {
                idealZones = new List<ZoneRect>
                {
                    new ZoneRect(0, 3, 8, 6),    // Zona defensive tengah (IDEAL untuk DM)
                    new ZoneRect(2, 0, 6, 2)
                },
                netralZones = new List<ZoneRect>
                {
                    new ZoneRect(0, 7, 8, 9),   // Zona transition ke depan (NETRAL)
                }
            }
        },

        // ⚽ ATTACKING MIDFIELDER
        {
            "AM", new RoleAreaWithTypes
            {
                idealZones = new List<ZoneRect>
                {
                    new ZoneRect(0, 7, 8, 11),  // Wide attacking zone
                    new ZoneRect(2, 5, 6, 6)
                },
                netralZones = new List<ZoneRect>
                {
                    new ZoneRect(2, 3, 6, 4),
                    new ZoneRect(0, 5, 1, 6),
                    new ZoneRect(7, 5, 8, 6)
                }
            }
        },
    };

    void Start()
    {

        targetPosition = transform.position;
        gridManager = FindAnyObjectByType<GridManager>();

        if (enemyAI == null)
        {
            enemyAI = FindAnyObjectByType<EnemyAIManager>();

            if (enemyAI == null)
                Debug.LogWarning("⚠️ EnemyAIManager tidak ditemukan di scene!");
            else
                Debug.Log("✅ EnemyAIManager berhasil ditemukan otomatis!");
        }

        if (!roleAreasWithTypes.ContainsKey(playerRole))
            Debug.LogWarning($"Role '{playerRole}' tidak ditemukan di roleAreasWithTypes!");

        UpdateTotalScore();
    }

    void Update()
    {
        //DrawSingleRoleWithLegend("CM");

        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                isMoving = false;
                gameManager.OnPlayerMoveEnd();
            }

            TrackPositioningScore();
        }

        if (currentHolder != null)
        {
            AttachBallPlayer();
            CheckBallCarryInterception();
        }
    }

    // ✅ Method untuk menggambar zona IDEAL, NETRAL, dan RAWAN
    private void DrawRoleAreas()
    {
        if (gridManager == null) return;

        var allCells = gridManager.GetAllCells();

        // 🔥 PILIH ROLE YANG MAU DITAMPILKAN
        List<string> rolesToDraw = new List<string> { "CM", "DM", "AM" };

        // Reset semua cell dulu (warna default untuk RAWAN)
        foreach (var cell in allCells)
        {
            var sr = cell.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = new Color(1f, 0.3f, 0.3f, 0.3f); // Merah transparan untuk RAWAN
        }

        foreach (var role in rolesToDraw)
        {
            if (!roleAreasWithTypes.ContainsKey(role))
                continue;

            RoleAreaWithTypes areas = roleAreasWithTypes[role];

            // 🟢 GAMBAR ZONA IDEAL (Hijau)
            Color idealColor = GetIdealColor(role);
            foreach (var cell in allCells)
            {
                foreach (var zone in areas.idealZones)
                {
                    Vector2Int pos = cell.gridCoord;
                    if (pos.x >= zone.min.x && pos.x <= zone.max.x &&
                        pos.y >= zone.min.y && pos.y <= zone.max.y)
                    {
                        var sr = cell.GetComponent<SpriteRenderer>();
                        if (sr != null)
                            sr.color = idealColor;
                    }
                }
            }

            // 🟡 GAMBAR ZONA NETRAL (Kuning/Orange)
            Color netralColor = GetNetralColor(role);
            foreach (var cell in allCells)
            {
                foreach (var zone in areas.netralZones)
                {
                    Vector2Int pos = cell.gridCoord;
                    if (pos.x >= zone.min.x && pos.x <= zone.max.x &&
                        pos.y >= zone.min.y && pos.y <= zone.max.y)
                    {
                        var sr = cell.GetComponent<SpriteRenderer>();
                        if (sr != null)
                            sr.color = netralColor;
                    }
                }
            }
        }
    }

    // 🟢 Warna untuk zona IDEAL (Hijau terang)
    private Color GetIdealColor(string role)
    {
        switch (role)
        {
            case "CM": return new Color(0f, 1f, 0f, 0.6f);      // Hijau terang
            case "DM": return new Color(0f, 0.8f, 0.3f, 0.6f);  // Hijau agak gelap
            case "AM": return new Color(0.3f, 1f, 0.3f, 0.6f);  // Hijau muda
            case "LW": return new Color(0.2f, 0.9f, 0.5f, 0.6f);
            case "RW": return new Color(0.2f, 0.9f, 0.5f, 0.6f);
            case "ST": return new Color(0.5f, 1f, 0.2f, 0.6f);
            default: return new Color(0f, 1f, 0f, 0.6f);
        }
    }

    // 🟡 Warna untuk zona NETRAL (Kuning/Orange)
    private Color GetNetralColor(string role)
    {
        switch (role)
        {
            case "CM": return new Color(1f, 1f, 0f, 0.5f);      // Kuning
            case "DM": return new Color(1f, 0.8f, 0f, 0.5f);    // Kuning keemasan
            case "AM": return new Color(1f, 0.9f, 0.3f, 0.5f);  // Kuning muda
            case "LW": return new Color(1f, 0.7f, 0.2f, 0.5f);
            case "RW": return new Color(1f, 0.7f, 0.2f, 0.5f);
            case "ST": return new Color(1f, 0.85f, 0f, 0.5f);
            default: return new Color(1f, 1f, 0f, 0.5f);
        }
    }

    // 🔴 Zona RAWAN sudah di-set di awal dengan warna merah transparan
    // new Color(1f, 0.3f, 0.3f, 0.3f)


    // ============================================
    // 📊 ALTERNATIF: Tampilkan hanya 1 role dengan legend
    // ============================================
    private void DrawSingleRoleWithLegend(string role)
    {
        if (gridManager == null) return;
        if (!roleAreasWithTypes.ContainsKey(role)) return;

        var allCells = gridManager.GetAllCells();
        RoleAreaWithTypes areas = roleAreasWithTypes[role];

        // Reset ke RAWAN (merah)
        foreach (var cell in allCells)
        {
            var sr = cell.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = new Color(1f, 0.3f, 0.3f, 0.3f); // RAWAN
        }

        // IDEAL (hijau)
        foreach (var cell in allCells)
        {
            foreach (var zone in areas.idealZones)
            {
                Vector2Int pos = cell.gridCoord;
                if (pos.x >= zone.min.x && pos.x <= zone.max.x &&
                    pos.y >= zone.min.y && pos.y <= zone.max.y)
                {
                    var sr = cell.GetComponent<SpriteRenderer>();
                    if (sr != null)
                        sr.color = new Color(0f, 1f, 0f, 0.7f); // Hijau solid
                }
            }
        }

        // NETRAL (kuning)
        foreach (var cell in allCells)
        {
            foreach (var zone in areas.netralZones)
            {
                Vector2Int pos = cell.gridCoord;
                if (pos.x >= zone.min.x && pos.x <= zone.max.x &&
                    pos.y >= zone.min.y && pos.y <= zone.max.y)
                {
                    var sr = cell.GetComponent<SpriteRenderer>();
                    if (sr != null)
                        sr.color = new Color(1f, 1f, 0f, 0.6f); // Kuning
                }
            }
        }

        //Debug.Log($"🟢 IDEAL = Hijau | 🟡 NETRAL = Kuning | 🔴 RAWAN = Merah | Role: {role}");
    }


    private void GameOver()
    {
        GameOverManager gameOver = FindAnyObjectByType<GameOverManager>();
        if (gameOver != null)
        {
            gameOver.ShowGameOver();
        }

        currentHolder = null;
        return;
    }

    // ✅ METHOD BARU - Tracking skor positioning berdasarkan area
    private void TrackPositioningScore()
    {
        if (gridManager == null) return;

        Cell currentCell = gridManager.GetNearestCell(transform.position);
        if (currentCell == null) return;

        Vector2Int pos = currentCell.gridCoord;

        if (!roleAreasWithTypes.ContainsKey(playerRole))
        {
            Debug.LogWarning($"Role {playerRole} tidak ditemukan!");
            return;
        }

        // Cek tipe area saat ini
        currentAreaType = GetAreaType(pos);

        // Hitung skor berdasarkan area
        switch (currentAreaType)
        {
            case AreaType.IDEAL:
                // +1 poin per detik di IDEAL
                timeInAreaIdeal += Time.deltaTime;
                break;

            case AreaType.NETRAL:
                // 0 poin, tapi aman
                timeInAreaNetral += Time.deltaTime;
                break;
        }

        UpdateTotalScore();
        SaveScores();
    }

    // ✅ METHOD BARU - Cek tipe area berdasarkan koordinat (menggunakan ZoneRect dari GridManager)
    private AreaType GetAreaType(Vector2Int pos)
    {
        if (!roleAreasWithTypes.ContainsKey(playerRole))
            return AreaType.RAWAN;

        RoleAreaWithTypes areas = roleAreasWithTypes[playerRole];

        // Cek IDEAL dulu
        foreach (var zone in areas.idealZones)
        {
            if (pos.x >= zone.min.x && pos.x <= zone.max.x &&
                pos.y >= zone.min.y && pos.y <= zone.max.y)
                return AreaType.IDEAL;
        }

        // Lalu cek NETRAL
        foreach (var zone in areas.netralZones)
        {
            if (pos.x >= zone.min.x && pos.x <= zone.max.x &&
                pos.y >= zone.min.y && pos.y <= zone.max.y)
                return AreaType.NETRAL;
        }

        // Sisanya RAWAN
        return AreaType.RAWAN;
    }

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
                indexFeedback = 3;
                GameOver();
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

    public void TryPassBall(Vector3 worldPos)
    {
        if (currentHolder != this) return;

        // ✅ CEK AREA SEBELUM PASSING
        if (currentAreaType == AreaType.RAWAN)
        {
            //Debug.LogError("❌ PASSING DARI ZONA RAWAN → GAME OVER!");

            indexFeedback = 2;

            GameOver();
            return;
        }

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

        // ✅ BONUS PASSING HANYA JIKA DARI AREA IDEAL
        if (currentAreaType == AreaType.IDEAL)
        {
            passingIdeal += 2;
            Debug.Log("✅ Passing dari IDEAL → +2 bonus!");
        }
        else if (currentAreaType == AreaType.NETRAL)
        {
            passingNetral += 1;
            Debug.Log("❌ ⚪ Passing dari NETRAL → 0 bonus");
        }

        UpdateTotalScore();
        SaveScores();
    }

    private void UpdateTotalScore()
    {
        // ✅ JANGAN UPDATE JIKA SUDAH GAME OVER (index 3 atau 4)
        if (indexFeedback == 3 || indexFeedback == 4)
        {
            Debug.Log($"⚠️ Game Over state detected, keeping indexFeedback = {indexFeedback}");
            return;
        }

        // Hitung score terlebih dahulu
        scorePositioning = timeInAreaIdeal * 1;
        scorePassing = passingIdeal * 2;

        // Tentukan feedback berdasarkan performa
        if (timeInAreaIdeal == 0 && timeInAreaNetral == 0 &&
            passingIdeal == 0 && passingNetral == 0)
        {
            indexFeedback = 4;
        }
        else if (timeInAreaIdeal > timeInAreaNetral && passingIdeal > passingNetral)
        {
            indexFeedback = 0;
        }
        else if (timeInAreaNetral > timeInAreaIdeal && passingNetral > passingIdeal)
        {
            indexFeedback = 1;
        }
        //else if ((timeInAreaIdeal > timeInAreaNetral && passingIdeal <= passingNetral) ||
        //         (timeInAreaIdeal <= timeInAreaNetral && passingIdeal > passingNetral))
        //{
        //    indexFeedback = 2;
        //}
        else
        {
            indexFeedback = 4;
        }

        Debug.Log($"📊 Feedback: {indexFeedback} | Ideal: {timeInAreaIdeal:F1}s | Netral: {timeInAreaNetral:F1}s | Pass Ideal: {passingIdeal} | Pass Netral: {passingNetral}");
    }

    // ✅ METHOD BARU - Save scores ke PlayerPrefs
    private void SaveScores()
    {
        PlayerPrefs.SetFloat("IdealTime", timeInAreaIdeal);
        PlayerPrefs.SetFloat("NetralTime", timeInAreaNetral);
        PlayerPrefs.SetFloat("ScorePositioning", scorePositioning);

        PlayerPrefs.SetInt("PassingIdeal", passingIdeal);
        PlayerPrefs.SetInt("ScorePassing", scorePassing);

        PlayerPrefs.SetInt("IndexFeedback", indexFeedback);

    }

    // ✅ METHOD BARU - Get current area info (untuk UI/debug)
    public string GetCurrentAreaInfo()
    {
        return $"Area: {currentAreaType} | Positioning: {scorePositioning:F1} | passingIdeal: {passingIdeal} | Total: {scorePassing}";
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

// ✅ CLASS BARU - Role area dengan 3 tipe zona (Compatible dengan ZoneRect dari GridManager)
[System.Serializable]
public class RoleAreaWithTypes
{
    public List<ZoneRect> idealZones = new List<ZoneRect>();
    public List<ZoneRect> netralZones = new List<ZoneRect>();
    // rawanZones = semua area lainnya (tidak perlu list)
}