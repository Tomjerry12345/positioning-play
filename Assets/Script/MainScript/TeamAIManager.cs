using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class TeamAIManager : MonoBehaviour
{
    [HideInInspector] public List<AIController> teammates = new List<AIController>();

    public GridManager gridManager;
    public FormationManager formationManager;
    public TimeManager timeManager;

    [HideInInspector]  public AIController currentHolder;

    [HideInInspector] public PlayerController player;
    [HideInInspector] public BallController ball;
    public EnemyAIManager enemyAI;

    public float moveRadius = 1f;

    [Header("⚽ Pass Settings")]
    public float passSpeed = 0.001f;           // ✨ Kecepatan bola saat terbang
    public float minPassDistance = 0.5f;
    public float maxPassDistance = 1f;

    [Header("🏃 Movement Settings")]
    [Tooltip("Kecepatan AI bergerak (semakin kecil = semakin lambat)")]
    public float aiMoveSpeed = 0.3f;         // ✨ SUPER SLOW untuk simulasi

    [Tooltip("Kecepatan pemain menerima bola")]
    public float receiverMoveSpeed = 0.4f;   // ✨ Sedikit lebih cepat dari AI biasa

    [Header("⏱️ Timing Settings")]
    [Tooltip("Delay setelah semua AI selesai bergerak")]
    public float postMoveDelay = 0.5f;

    [Tooltip("Area gerak")]
    public Dictionary<string, RoleArea> roleAllowedAreas = new Dictionary<string, RoleArea>()
{
        // ⚽ CENTER BACK LEFT (Zona bertahan kiri)
        {
            "CBL", new RoleArea
            {
                zones = new List<ZoneRect>
                {
                    new ZoneRect(1, 1, 3, 6) // Main defensive zone
                }
            }
        },
    
        // ⚽ CENTER BACK RIGHT (Zona bertahan kanan)
        {
            "CBR", new RoleArea
            {
                zones = new List<ZoneRect>
                {
                    new ZoneRect(5, 1, 7, 6)
                }
            }
        },
    
        // ⚽ LEFT BACK (Overlap zona bertahan + tengah)
        {
            "LB", new RoleArea
            {
                zones = new List<ZoneRect>
                {
                    new ZoneRect(0, 2, 2, 6),  // Defensive zone
                    new ZoneRect(0, 6, 1, 9)   // Push forward zone
                }
            }
        },
    
        // ⚽ RIGHT BACK
        {
            "RB", new RoleArea
            {
                zones = new List<ZoneRect>
                {
                    new ZoneRect(6, 2, 8, 6),
                    new ZoneRect(7, 6, 8, 9)
                }
            }
        },
    
        // ⚽ CENTRAL MIDFIELDER (Zona tengah luas - sesuai gambar tengah)
        {
            "CM", new RoleArea
            {
                zones = new List<ZoneRect>
                {
                    new ZoneRect(2, 4, 6, 10),  // Left-center zone
                    new ZoneRect(0, 4, 1, 8),  // Extended forward zone
                    new ZoneRect(7, 4, 8, 8)  // Extended forward zone
                }
            }
        },
    
        // ⚽ DEFENSIVE MIDFIELDER
        {
            "DM", new RoleArea
            {
                zones = new List<ZoneRect>
                {
                    new ZoneRect(0, 3, 8, 6)
                }
            }
        },
    
        // ⚽ ATTACKING MIDFIELDER (Zona serang tengah)
        {
            "AM", new RoleArea
            {
                zones = new List<ZoneRect>
                {
                    new ZoneRect(2, 6, 6, 12),  // Wide attacking zone
                    new ZoneRect(0, 8, 1, 13),  // Wide attacking zone
                    new ZoneRect(7, 8, 8, 13)  // Wide attacking zone
                }
            }
        },
    
        // ⚽ LEFT WINGER (Sayap kiri - sesuai gambar kanan)
        {
            "LW", new RoleArea
            {
                zones = new List<ZoneRect>
                {
                    new ZoneRect(0, 7, 2, 14),   // Wing zone
                    new ZoneRect(1, 11, 3, 13)   // Cut inside zone
                }
            }
        },
    
        // ⚽ RIGHT WINGER
        {
            "RW", new RoleArea
            {
                zones = new List<ZoneRect>
                {
                    new ZoneRect(6, 7, 8, 14),
                    new ZoneRect(5, 11, 7, 13)
                }
            }
        },
    
        // ⚽ STRIKER (Zona kotak penalti lawan)
        {
            "ST", new RoleArea
            {
                zones = new List<ZoneRect>
                {
                    new ZoneRect(2, 9, 6, 13),  // Main attacking third
                    new ZoneRect(3, 8, 5, 9)    // Deep lying zone
                },
                customCells = new List<Vector2Int> // Tambahkan cells spesifik jika perlu
                {
                    new Vector2Int(4, 14) // Posisi super advanced
                }
            }
        }
    };

    [Header("🥅 Goal Settings")]
    public Transform enemyGoal; // Titik koordinat gawang musuh
    public float shootSpeed = 0.9f; // Kecepatan bola saat ditembak

    // Test
    [HideInInspector] public AIController testController;
    Vector2 movementDir = Vector2.zero;
    Vector2Int movementDirAttach = Vector2Int.zero;
    private Vector2Int lastAttachDir = Vector2Int.zero;
    private Vector2Int lastDir = Vector2Int.zero; // untuk anti looping
    bool isAttacking = true;  // default
    bool isPlaySfxDribble = false;
    private bool aiFirstAttach = false;

    private CellReservationManager reservation;
    private Dictionary<AIController, Vector2Int> plannedMoves = new Dictionary<AIController, Vector2Int>();
    private AIController plannedHolder;

    private enum MoveResult
    {
        Success,
        Blocked
    }

    void Start()
    {
        if (formationManager == null)
            formationManager = FindObjectOfType<FormationManager>();

        currentHolder = teammates.FirstOrDefault(t => t.assignedRole == "GK");
        player.roleAllowedAreas = roleAllowedAreas;
        reservation = CellReservationManager.Instance;
    }

    private void Update()
    {
        DrawRoleAreas();

        // Mode testing manual (pakai F)
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            ManualTestStep();
        }

        AttachBallToHolder(); // bola selalu nempel

        Vector2 inputDir = Vector2.zero;

        // WASD input
        if (Keyboard.current.wKey.isPressed) inputDir.y += 1f;
        if (Keyboard.current.sKey.isPressed) inputDir.y -= 1f;
        if (Keyboard.current.aKey.isPressed) inputDir.x -= 1f;
        if (Keyboard.current.dKey.isPressed) inputDir.x += 1f;

        if (inputDir != Vector2.zero)
        {
            // Normalisasi supaya diagonal tidak lebih cepat
            inputDir.Normalize();

            // Konversi ke Vector2Int grid direction
            movementDirAttach = new Vector2Int(
                Mathf.RoundToInt(inputDir.x),
                Mathf.RoundToInt(inputDir.y)
            );

            // Stop semua Move Coroutine sebelumnya agar tidak tumpang tindih
            if (currentMoveCoroutine != null)
                StopCoroutine(currentMoveCoroutine);

            // Jalankan Move baru
            currentMoveCoroutine = StartCoroutine(Move(currentHolder, inputDir));
        } 
    }

    private Coroutine currentMoveCoroutine;

    private void ManualTestStep()
    {
        AIController ballHolder = currentHolder;
        if (ballHolder == null) return;

        // 1. pilih arah dribble
        Vector2Int aiDir = GetBestAvoidDirectionForAttachBall(ballHolder, true);

        if (aiDir != Vector2Int.zero)
        {
            movementDirAttach = aiDir;
            movementDir = aiDir;

            Vector2 norm = movementDir.normalized;

            if (CheckMove(ballHolder, norm))
                StartCoroutine(Move(ballHolder, norm));
        }

        // 2. gerakkan non-holder
        var nonHolders = teammates
           .Where(t => t != null && t != ballHolder && t.assignedRole != "GK")
           .ToList();

        Vector3 bhPos = ballHolder.transform.position;

        foreach (var ai in nonHolders)
        {
            Vector2Int dir = GetBestMovementForNonBallHolder(ai, bhPos, true);

            if (dir == Vector2Int.zero) continue;

            Vector2 v2 = dir;

            if (CheckMove(ai, v2))
                StartCoroutine(Move(ai, v2));
        }
    }

    private void DrawRoleAreas()
    {
        if (gridManager == null) return;

        var allCells = gridManager.GetAllCells();

        // Reset warna
        foreach (var cell in allCells)
        {
            var sr = cell.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = new Color(1f, 1f, 1f, 0.2f);
        }

        // 🔥 HANYA GAMBAR ROLE TERTENTU (tambahkan role yang mau ditampilkan)
        //List<string> rolesToDraw = new List<string> { "CM" };

        //foreach (var role in rolesToDraw)
        //{
        //    if (!roleAllowedAreas.ContainsKey(role))
        //        continue;

        //    RoleArea area = roleAllowedAreas[role];
        //    Color color = GetRoleColor(role);

        //    foreach (var cell in allCells)
        //    {
        //        if (area.Contains(cell.gridCoord))
        //        {
        //            var sr = cell.GetComponent<SpriteRenderer>();
        //            if (sr != null)
        //                sr.color = color;
        //        }
        //    }
        //}
    }

    private Color GetRoleColor(string role)
    {

        switch (role)
            {
                case "CBL": return new Color(0f, 0.5f, 1f, 0.5f);
                case "CBR": return new Color(0.3f, 0.8f, 1f, 0.5f);
                case "LB": return new Color(0f, 1f, 1f, 0.5f);
                case "RB": return new Color(0.2f, 0.4f, 1f, 0.5f);
                case "CM": return new Color(0f, 1f, 0f, 0.5f);
                case "DM": return new Color(0.7f, 0.6f, 1f, 0.5f);
                case "AM": return new Color(1f, 0.7f, 0f, 0.5f);
                case "LW": return new Color(0.5f, 0.3f, 1f, 0.5f);
                case "RW": return new Color(0.4f, 0.8f, 0.5f, 0.5f);
                case "ST": return new Color(1f, 0.2f, 0.2f, 0.5f);
                default: return Color.white;
            }
    }

    // -------------------------------------------------------
    // ⚽️ Start dari GK saat kickoff
    // -------------------------------------------------------
    public void StartPlay()
    {
        AIController gk = teammates.Find(t => t.assignedRole == "GK");
        if (gk != null && ball != null)
            StartCoroutine(GKPassRoutine(gk));
    }

    private IEnumerator GKPassRoutine(AIController gk)
    {
        yield return new WaitForSeconds(0.5f);

        var target = FindPassTarget(gk, true);
        //var target = testController;

        if (target != null)
        {
            AudioManager.instance.PlaySFX(AudioManager.instance.sfxPass);

            var aiTarget = target.GetComponent<AIController>();

            if (aiTarget != null)
            {
                //Debug.Log($"🏁 GK mengoper bola ke {aiTarget.assignedRole}");
                yield return PassBall(gk, aiTarget);
            }
            else if (target == player.transform)
            {
                //Debug.Log($"⚽ {gk.assignedRole} memutuskan umpan ke PLAYER");
                yield return PassBallToPlayer(gk, player);
            }

            if (timeManager != null) 
                timeManager.StartMovement();
        }
        else
        {
            Debug.Log("❌ GK tidak menemukan target untuk diumpan.");
        }
    }

    // -------------------------------------------------------
    // 🎮 Giliran AI (dipanggil GameManager)
    // -------------------------------------------------------
    //public void Action()
    //{
    //    if (teammates == null || teammates.Count == 0)
    //    {
    //        Debug.LogWarning("⚠️ Tidak ada AI terdaftar di tim.");
    //        return;
    //    }

    //    //Debug.Log("🤖 Semua AI mulai bergerak...");
    //    StartCoroutine(AIRoutine());
    //}

    //private IEnumerator AIRoutine()
    //{
    //    AIController holder = currentHolder;
    //    List<Coroutine> moveCoroutines = new List<Coroutine>();

    //    bool playerHasBall = (player != null && player.currentHolder == player);

    //    if (holder == null && !playerHasBall)
    //    {
    //        Debug.LogWarning("❌ Tidak ada holder");
    //        yield break;
    //    }

    //    Vector3 ballHolderPos;
    //    string ballHolderRole;

    //    if (playerHasBall)
    //    {
    //        ballHolderPos = player.transform.position;
    //        ballHolderRole = player.playerRole;
    //    }
    //    else
    //    {
    //        ballHolderPos = holder.transform.position;
    //        ballHolderRole = holder.assignedRole;
    //    }

    //    Cell holderCell = gridManager.GetNearestCell(ballHolderPos);
    //    if (holderCell == null) yield break;

    //    float holderY = holderCell.gridCoord.y;
    //    float attackBoundaryY = 11f;

    //    if ((ballHolderRole == "LW" || ballHolderRole == "ST" || ballHolderRole == "RW") &&
    //        holderY >= attackBoundaryY)
    //    {
    //        isAttacking = false;
    //    }

    //    if ((ballHolderRole == "CBL" || ballHolderRole == "CBR") &&
    //        holderY < attackBoundaryY)
    //    {
    //        isAttacking = true;
    //    }

    //    // === PASS & DRIBBLE HOLDER ===
    //    if (!playerHasBall && holder != null)
    //    {
    //        var passTarget = FindPassTarget(holder, isAttacking);

    //        if (passTarget != null)
    //        {
    //            AudioManager.instance.PlaySFX(AudioManager.instance.sfxPass);

    //            AIController targetAI = passTarget.GetComponent<AIController>();
    //            PlayerController targetPlayer = passTarget.GetComponent<PlayerController>();

    //            if (targetAI != null)
    //            {
    //                moveCoroutines.Add(StartCoroutine(PassBall(holder, targetAI)));
    //            }
    //            else if (targetPlayer != null)
    //            {
    //                moveCoroutines.Add(StartCoroutine(PassBallToPlayer(holder, targetPlayer)));
    //            }

    //            isPlaySfxDribble = false;
    //        }

    //        Vector2Int dir = GetBestAvoidDirectionForAttachBall(holder, isAttacking);
    //        if (dir != Vector2Int.zero)
    //        {
    //            if (isPlaySfxDribble)
    //                AudioManager.instance.PlaySFX(AudioManager.instance.sfxDribble);

    //            movementDirAttach = new Vector2Int(
    //               Mathf.RoundToInt(dir.x),
    //               Mathf.RoundToInt(dir.y)
    //            );

    //            // 🔥 RESERVE via CellReservationManager
    //            Cell currentCell = gridManager.GetNearestCell(holder.transform.position);
    //            if (currentCell != null)
    //            {
    //                Vector2Int targetCoord = currentCell.gridCoord + dir;
    //                reservation.ReleaseCell(currentCell.gridCoord);
    //                reservation.TryReserveCell(targetCoord, currentCell.gridCoord, $"TEAM-{holder.assignedRole}");
    //            }

    //            Vector2 move = dir;
    //            moveCoroutines.Add(StartCoroutine(Move(holder, move.normalized)));

    //            isPlaySfxDribble = true;
    //        }
    //    }

    //    // === GERAKKAN NON-HOLDER ===
    //    var nonHolders = teammates
    //        .Where(t => t != null && t != holder && t.assignedRole != "GK")
    //        .ToList();

    //    foreach (var ai in nonHolders)
    //    {
    //        Vector2Int nd = GetBestMovementForNonBallHolder(ai, ballHolderPos, isAttacking);
    //        if (nd == Vector2Int.zero) continue;

    //        Cell currentCell = gridManager.GetNearestCell(ai.transform.position);
    //        if (currentCell == null) continue;

    //        Vector2Int targetCoord = currentCell.gridCoord + nd;

    //        // 🔥 CEK & RESERVE via CellReservationManager
    //        if (!reservation.TryReserveCell(targetCoord, currentCell.gridCoord, $"TEAM-{ai.assignedRole}"))
    //        {
    //            Debug.Log($"⚠️ {ai.assignedRole} skip - cell {targetCoord} tidak bisa direserve");
    //            continue;
    //        }

    //        Vector2 v2 = nd;
    //        moveCoroutines.Add(StartCoroutine(Move(ai, v2)));
    //    }

    //    foreach (var co in moveCoroutines)
    //    {
    //        yield return co;
    //    }

    //    yield return new WaitForSeconds(0.3f);
    //}

    public void Action()
    {
        // Legacy method - sekarang pakai PlanMoves() dan ExecuteMoves()
        StartCoroutine(ActionRoutine());
    }

    private IEnumerator ActionRoutine()
    {
        yield return PlanMoves();
        yield return ExecuteMoves();
    }

    // 🔥 FASE 1: PLANNING (decide moves, tidak ada movement)
    public IEnumerator PlanMoves()
    {
        if (teammates == null || teammates.Count == 0)
        {
            Debug.LogWarning("⚠️ Tidak ada AI terdaftar di tim.");
            yield break;
        }

        //Debug.Log("🧠 [TEAM] Planning phase started...");

        plannedMoves.Clear();
        AIController holder = currentHolder;
        plannedHolder = holder;

        bool playerHasBall = (player != null && player.currentHolder == player);

        if (holder == null && !playerHasBall)
        {
            Debug.LogWarning("❌ Tidak ada holder");
            yield break;
        }

        Vector3 ballHolderPos;
        string ballHolderRole;

        if (playerHasBall)
        {
            ballHolderPos = player.transform.position;
            ballHolderRole = player.playerRole;
        }
        else
        {
            ballHolderPos = holder.transform.position;
            ballHolderRole = holder.assignedRole;
        }

        Cell holderCell = gridManager.GetNearestCell(ballHolderPos);
        if (holderCell == null) yield break;

        float holderY = holderCell.gridCoord.y;
        float attackBoundaryY = 11f;

        if ((ballHolderRole == "LW" || ballHolderRole == "ST" || ballHolderRole == "RW") &&
            holderY >= attackBoundaryY)
        {
            isAttacking = false;
        }

        if ((ballHolderRole == "CBL" || ballHolderRole == "CBR") &&
            holderY < attackBoundaryY)
        {
            isAttacking = true;
        }

        // === PLAN HOLDER MOVEMENT ===
        if (!playerHasBall && holder != null)
        {
            Vector2Int dir = GetBestAvoidDirectionForAttachBall(holder, isAttacking);

            if (dir != Vector2Int.zero)
            {
                Cell currentCell = gridManager.GetNearestCell(holder.transform.position);
                if (currentCell != null)
                {
                    Vector2Int targetCoord = currentCell.gridCoord + dir;

                    bool reserved = reservation.TryReserveCell(targetCoord, currentCell.gridCoord, $"TEAM-{holder.assignedRole}");

                    if (reserved)
                    {
                        plannedMoves[holder] = targetCoord;
                        movementDirAttach = new Vector2Int(
                           Mathf.RoundToInt(dir.x),
                           Mathf.RoundToInt(dir.y)
                        );
                        //Debug.Log($"📍 [TEAM] {holder.assignedRole} (HOLDER): {currentCell.gridCoord} → {targetCoord}");
                    }
                    else
                    {
                        plannedMoves[holder] = currentCell.gridCoord; // stay
                        //Debug.LogWarning($"⚠️ [TEAM] {holder.assignedRole} gagal reserve, tetap di tempat");
                    }
                }
            }
        }

        // === PLAN NON-HOLDER MOVEMENT ===
        var nonHolders = teammates
            .Where(t => t != null && t != holder && t.assignedRole != "GK")
            .ToList();

        foreach (var ai in nonHolders)
        {
            Vector2Int nd = GetBestMovementForNonBallHolder(ai, ballHolderPos, isAttacking);

            if (nd == Vector2Int.zero)
            {
                Cell currentCell = gridManager.GetNearestCell(ai.transform.position);
                if (currentCell != null)
                {
                    plannedMoves[ai] = currentCell.gridCoord; // stay
                }
                continue;
            }

            Cell currentCell2 = gridManager.GetNearestCell(ai.transform.position);
            if (currentCell2 == null) continue;

            Vector2Int targetCoord = currentCell2.gridCoord + nd;

            bool reserved = reservation.TryReserveCell(targetCoord, currentCell2.gridCoord, $"TEAM-{ai.assignedRole}");

            if (reserved)
            {
                plannedMoves[ai] = targetCoord;
                //Debug.Log($"📍 [TEAM] {ai.assignedRole}: {currentCell2.gridCoord} → {targetCoord}");
            }
            else
            {
                plannedMoves[ai] = currentCell2.gridCoord; // stay
                //Debug.LogWarning($"⚠️ [TEAM] {ai.assignedRole} gagal reserve, tetap di tempat");
            }
        }

        //Debug.Log("✅ [TEAM] Planning phase complete!");
        yield return null;
    }

    // 🔥 FASE 2: EXECUTION (movement bersamaan)
    public IEnumerator ExecuteMoves()
    {
        //Debug.Log("⚡ [TEAM] Execution phase started...");

        List<Coroutine> moveCoroutines = new List<Coroutine>();
        AIController holder = plannedHolder;
        bool playerHasBall = (player != null && player.currentHolder == player);

        // === PASS & DRIBBLE HOLDER ===
        if (!playerHasBall && holder != null)
        {
            var passTarget = FindPassTarget(holder, isAttacking);

            if (passTarget != null)
            {
                AudioManager.instance.PlaySFX(AudioManager.instance.sfxPass);

                AIController targetAI = passTarget.GetComponent<AIController>();
                PlayerController targetPlayer = passTarget.GetComponent<PlayerController>();

                if (targetAI != null)
                {
                    moveCoroutines.Add(StartCoroutine(PassBall(holder, targetAI)));
                }
                else if (targetPlayer != null)
                {
                    moveCoroutines.Add(StartCoroutine(PassBallToPlayer(holder, targetPlayer)));
                }

                isPlaySfxDribble = false;
            }
            else
            {
                // Execute holder movement
                if (plannedMoves.ContainsKey(holder))
                {
                    Vector2Int targetCoord = plannedMoves[holder];
                    Cell currentCell = gridManager.GetNearestCell(holder.transform.position);

                    if (currentCell != null && targetCoord != currentCell.gridCoord)
                    {
                        if (isPlaySfxDribble)
                            AudioManager.instance.PlaySFX(AudioManager.instance.sfxDribble);

                        Vector2 dir = (targetCoord - currentCell.gridCoord);
                        moveCoroutines.Add(StartCoroutine(Move(holder, dir.normalized)));
                        isPlaySfxDribble = true;
                    }
                }
            }
        }

        // === EXECUTE NON-HOLDER MOVEMENT ===
        foreach (var kvp in plannedMoves)
        {
            AIController ai = kvp.Key;
            if (ai == holder) continue; // holder sudah dihandle

            Vector2Int targetCoord = kvp.Value;
            Cell currentCell = gridManager.GetNearestCell(ai.transform.position);

            if (currentCell != null && targetCoord != currentCell.gridCoord)
            {
                Vector2 dir = (targetCoord - currentCell.gridCoord);
                moveCoroutines.Add(StartCoroutine(Move(ai, dir)));
            }
        }

        // 🔥 TUNGGU SEMUA MOVEMENT SELESAI BERSAMAAN
        foreach (var co in moveCoroutines)
        {
            yield return co;
        }

        yield return new WaitForSeconds(0.3f);
        //Debug.Log("✅ [TEAM] Execution phase complete!");
    }

    private bool IsPassBlocked(Vector3 from, Vector3 to)
    {
        // -----------------------------------------
        // ⚙ Konfigurasi threshold (dalam cell)
        // -----------------------------------------
        float blockDistanceInCells = 1.5f;   // jarak musuh ke garis operan
        float blockMidRangeInCells = 3f;   // area tengah lintasan yang dianggap rawan
        float cellSize = 0.14f;            // 1 cell = 0.14 unit
                                           // -----------------------------------------

        // Konversi ke Unity unit
        float lineBlockDistance = blockDistanceInCells * cellSize;
        float midBlockDistance = blockMidRangeInCells * cellSize;

        Vector3 passDir = (to - from).normalized;
        float passLength = Vector3.Distance(from, to);

        Vector3 midPoint = (from + to) * 0.5f;

        foreach (var e in enemyAI.enemyTeammates)
        {
            if (e == null) continue;

            Vector3 enemyPos = e.transform.position;

            // 1️⃣ Cek jarak musuh ke garis operan
            float distToLine = DistancePointToLine(enemyPos, from, to);

            if (distToLine > lineBlockDistance)
                continue; // musuh terlalu jauh dari garis → aman

            // 2️⃣ Pastikan musuh berada DI DEPAN lintasan, bukan di belakang pengumpan
            float proj = Vector3.Dot(enemyPos - from, passDir);

            if (proj < 0 || proj > passLength)
                continue;

            // 3️⃣ Cek musuh dekat area tengah lintasan
            float distToMid = Vector3.Distance(enemyPos, midPoint);

            if (distToMid < midBlockDistance)
                return true; // ❌ musuh blok lintasan
        }

        return false;
    }

    private float DirectionPenalty(Vector3 from, Vector3 to, bool isAttacking)
    {
        Vector3 dir = (to - from).normalized;

        // Untuk game top-down, Y+ dianggap ke depan
        float forward = dir.y;

        if (isAttacking)
        {
            if (forward < 0f)
                return 20f;   // penalti besar untuk pass mundur
        }
        else
        {
            if (forward > 0f)
                return 20f;   // penalti besar untuk pass ke depan kalau sedang bertahan
        }

        return 0f;
    }


    // Jarak titik ke garis
    private float DistancePointToLine(Vector3 point, Vector3 a, Vector3 b)
    {
        return Vector3.Cross(b - a, point - a).magnitude / (b - a).magnitude;
    }

    // -------------------------------------------------------
    // 🎯 Cari target umpan terdekat
    // -------------------------------------------------------
    private Transform FindPassTarget(AIController from, bool isAttacking)
    {
        Vector3 fromPos = from.transform.position;

        //Debug.Log($"🔍 [{from.assignedRole}] Mencari target pass... isAttacking={isAttacking}");

        var possibleTargets = teammates
            .Where(t => t != null && t != from && t.assignedRole != "GK")
            .ToList();

        //Debug.Log($"📋 Jumlah AI targets: {possibleTargets.Count}");

        float bestScore = float.MaxValue;
        Transform bestTarget = null;
        string bestTargetName = "";

        foreach (var t in possibleTargets)
        {
            float dist = Vector3.Distance(fromPos, t.transform.position);

            float dirPenalty = 0f;
            if (isAttacking && t.transform.position.y < fromPos.y)
                dirPenalty = 0.3f;
            else if (!isAttacking && t.transform.position.y > fromPos.y)
                dirPenalty = 0.3f;

            float totalDist = dist + dirPenalty;

            //Debug.Log($"  🤖 {t.assignedRole}: dist={dist:F2}, penalty={dirPenalty:F2}, total={totalDist:F2}");

            bool blocked = IsPassBlocked(fromPos, t.transform.position);
            if (blocked)
            {
                Debug.Log($"    ❌ DIBLOK musuh");
                continue;
            }

            if (totalDist < minPassDistance)
            {
                Debug.Log($"    ⚠️ Terlalu DEKAT (min={minPassDistance})");
                continue;
            }

            if (totalDist > maxPassDistance)
            {
                Debug.Log($"    ⚠️ Terlalu JAUH (max={maxPassDistance})");
                continue;
            }

            if (totalDist < bestScore)
            {
                bestScore = totalDist;
                bestTarget = t.transform;
                bestTargetName = t.assignedRole;
                Debug.Log($"    ✅ NEW BEST TARGET!");
            }
        }

        // 🔥 CEK PLAYER (HANYA jika player TIDAK sedang pegang bola)
        if (player != null && player.currentHolder != player)
        {
            float playerDist = Vector3.Distance(fromPos, player.transform.position);

            //Debug.Log($"  👤 PLAYER: pos={player.transform.position}, dist={playerDist:F2}");

            float dirPenalty = 0f;
            if (isAttacking && player.transform.position.y < fromPos.y)
                dirPenalty = 0.3f;
            else if (!isAttacking && player.transform.position.y > fromPos.y)
                dirPenalty = 0.3f;

            float totalDist = playerDist + dirPenalty;

            //Debug.Log($"    penalty={dirPenalty:F2}, total={totalDist:F2}");

            bool blocked = IsPassBlocked(fromPos, player.transform.position);
            if (!blocked &&
                totalDist >= minPassDistance &&
                totalDist <= maxPassDistance &&
                totalDist < bestScore)
            {
                bestScore = totalDist;
                bestTarget = player.transform;
                bestTargetName = "PLAYER";
                Debug.Log($"    ✅ PLAYER adalah BEST TARGET!");
            }
        }
        else if (player != null && player.currentHolder == player)
        {
            Debug.Log($"  👤 PLAYER sedang pegang bola - SKIP sebagai target");
        }

        if (bestTarget == null)
        {
            Debug.LogWarning($"❌ [{from.assignedRole}] TIDAK ADA target pass valid!");
        }
        else
        {
            Debug.Log($"🎯 [{from.assignedRole}] Final target: {bestTargetName} (score={bestScore:F2})");
        }

        return bestTarget;
    }

    // -------------------------------------------------------
    // 🔄 Umpan bola antar pemain (halus & realistis tanpa physics)
    // -------------------------------------------------------
    private IEnumerator PassBall(AIController from, AIController to)
    {
        if (ball == null || from == null || to == null) yield break;

        currentHolder = null; // bola sedang terbang

        // 🎯 Pastikan durasi proporsional tapi tidak terlalu lama
        float duration = 0.8f; // durasi tetap untuk passing yang konsisten
        float elapsed = 0f;

        Vector3 start = from.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            // 🎯 Update posisi target setiap frame (ikuti pergerakan AI penerima)
            Vector3 end = to.transform.position;
            float distance = Vector3.Distance(start, end);

            // Lintasan parabola biar kelihatan realistis
            Vector3 linearPos = Vector3.Lerp(start, end, smoothT);
            float arcHeight = Mathf.Min(0.3f, distance * 0.12f); // busur berdasarkan jarak
            float arc = Mathf.Sin(smoothT * Mathf.PI) * arcHeight;

            ball.transform.position = linearPos + new Vector3(0f, arc, 0f);

            yield return null;
        }

        // ✅ Pastikan bola PASTI sampai ke posisi penerima
        ball.transform.position = to.transform.position;

        // ✅ Langsung attach tanpa AI menjemput
        currentHolder = to;
        aiFirstAttach = true;

        //player.currentHolder = null;



        //currentHolder.hasJustReceivedBall = true;

        //Debug.Log($"✅ {to.assignedRole} menerima bola langsung");
    }

    private IEnumerator PassBallToPlayer(AIController from, PlayerController to)
    {
        if (ball == null || from == null || to == null) yield break;

        //AudioManager.instance.PlaySFX(AudioManager.instance.sfxPass);

        currentHolder = null; // bola sedang terbang

        float duration = 0.8f;
        float elapsed = 0f;

        Vector3 start = from.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            Vector3 end = to.transform.position;
            float distance = Vector3.Distance(start, end);

            Vector3 linearPos = Vector3.Lerp(start, end, smoothT);
            float arcHeight = Mathf.Min(0.3f, distance * 0.12f);
            float arc = Mathf.Sin(smoothT * Mathf.PI) * arcHeight;

            ball.transform.position = linearPos + new Vector3(0f, arc, 0f);

            yield return null;
        }

        ball.transform.position = to.transform.position;
        to.ReceiveBall();
        to.ball = ball;

        //Debug.Log($"✅ Bola diterima oleh PLAYER di posisi {to.transform.position}");
    }

    // ====================================
    // HAPUS METHOD CheckPassInterception
    // (yang cek sebelum pass dimulai)
    // ====================================

    // ✅ TETAP PAKAI CheckBallInterceptionRealtime
    private bool CheckBallInterceptionRealtime(Vector3 ballPos)
    {
        if (enemyAI == null || enemyAI.enemyTeammates == null)
            return false;

        float cellSize = 0.14f;
        float catchRadius = 0.8f * cellSize; // 0.8 cell radius untuk catch

        foreach (var enemy in enemyAI.enemyTeammates)
        {
            if (enemy == null) continue;

            float dist = Vector3.Distance(ballPos, enemy.transform.position);

            if (dist < catchRadius)
            {
                Debug.LogWarning($"⚠️ Bola tertangkap oleh {enemy.assignedRole}!");
                return true;
            }
        }

        return false;
    }

    public IEnumerator PassBallFromPlayer(PlayerController from, AIController to)
    {
        if (ball == null || from == null || to == null) yield break;

        AudioManager.instance.PlaySFX(AudioManager.instance.sfxPass);

        from.currentHolder = null;
        currentHolder = null;

        // ❌ HAPUS CheckPassInterception - jangan cek sebelum pass!
        // Langsung mulai animasi passing

        float duration = 0.8f;
        float elapsed = 0f;
        Vector3 start = from.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            Vector3 end = to.transform.position;
            float distance = Vector3.Distance(start, end);

            Vector3 linearPos = Vector3.Lerp(start, end, smoothT);
            float arcHeight = Mathf.Min(0.3f, distance * 0.12f);
            float arc = Mathf.Sin(smoothT * Mathf.PI) * arcHeight;

            ball.transform.position = linearPos + new Vector3(0f, arc, 0f);

            // ✅ CEK INTERCEPTION REALTIME saat bola terbang
            if (CheckBallInterceptionRealtime(ball.transform.position))
            {
                //Debug.LogError("❌❌❌ BALL INTERCEPTED DURING FLIGHT!");

                GameOverManager gameOver = FindAnyObjectByType<GameOverManager>();
                if (gameOver != null)
                {
                    gameOver.ShowGameOver();
                }

                yield break;
            }

            yield return null;
        }

        ball.transform.position = to.transform.position;
        currentHolder = to;
    }

    //public IEnumerator PassBallFromPlayer(PlayerController from, AIController to)
    //{
    //    if (ball == null || from == null || to == null) yield break;

    //    AudioManager.instance.PlaySFX(AudioManager.instance.sfxPass);

    //    from.currentHolder = null; // player kehilangan bola
    //    currentHolder = null;       // bola sedang terbang

    //    float duration = 0.8f;
    //    float elapsed = 0f;

    //    Vector3 start = from.transform.position;

    //    while (elapsed < duration)
    //    {
    //        elapsed += Time.deltaTime;
    //        float t = Mathf.Clamp01(elapsed / duration);
    //        float smoothT = Mathf.SmoothStep(0f, 1f, t);

    //        Vector3 end = to.transform.position;
    //        float distance = Vector3.Distance(start, end);

    //        Vector3 linearPos = Vector3.Lerp(start, end, smoothT);
    //        float arcHeight = Mathf.Min(0.3f, distance * 0.12f);
    //        float arc = Mathf.Sin(smoothT * Mathf.PI) * arcHeight;

    //        ball.transform.position = linearPos + new Vector3(0f, arc, 0f);
    //        yield return null;
    //    }

    //    ball.transform.position = to.transform.position;
    //    currentHolder = to;

    //    //Debug.Log($"✅ PLAYER mengoper bola ke {to.assignedRole}");
    //}

    private bool CheckMove(AIController ai, Vector2 inputDir)
    {
        string role = ai.assignedRole;

        if (!roleAllowedAreas.ContainsKey(role))
        {
            Debug.LogError($"Role {role} tidak ada di roleAllowedAreas!");
            return false;
        }

        RoleArea area = roleAllowedAreas[role];
        Cell currentCell = gridManager.GetNearestCell(ai.transform.position);

        if (currentCell == null) return false;

        Vector2Int current = currentCell.gridCoord;
        Vector2Int offset = new Vector2Int(
            Mathf.RoundToInt(inputDir.x),
            Mathf.RoundToInt(inputDir.y)
        );
        Vector2Int target = current + offset;

        // 🔥 CEK ZONA
        if (!area.Contains(target))
        {
            return false;
        }

        // 🔥 CEK SHARED RESERVATION (otomatis cek Team AI, Enemy AI, dan Player)
        if (target != current)
        {
            if (reservation.IsReserved(target) || reservation.IsOccupied(target))
            {
                return false;
            }
        }

        // 🔥 CEK CELL VALID
        if (gridManager.GetCellAt(target) == null)
        {
            return false;
        }

        return true;
    }

    private Vector2Int GetBestAvoidDirectionForAttachBall(AIController ai, bool isAttacking)
    {
        Cell currentCell = gridManager.GetNearestCell(ai.transform.position);
        if (currentCell == null) return Vector2Int.zero;

        string role = ai.assignedRole;

        // 🔹 Temukan target passing terdepan (teammate)
        var nearestFrontTeammate = teammates
            .Where(t => t != null && t != ai)
            .OrderBy(t => Vector3.Distance(ai.transform.position, t.transform.position))
            .FirstOrDefault();

        Vector3 passTargetPos = nearestFrontTeammate != null ? nearestFrontTeammate.transform.position
                                                             : ai.transform.position + Vector3.up * 3f;
        string targetRole = nearestFrontTeammate != null ? nearestFrontTeammate.assignedRole : "NONE";
        //Debug.Log($"<color=cyan>AI: {role} → Target Passing: {targetRole}</color>");

        // 🔹 Semua arah
        Vector2Int[] dirs =
        {
        new Vector2Int(0,1),
        new Vector2Int(1,1),
        new Vector2Int(-1,1),
        new Vector2Int(1,0),
        new Vector2Int(-1,0),
        new Vector2Int(0,-1),
        new Vector2Int(1,-1),
        new Vector2Int(-1,-1)
    };

        float bestScore = -999f;
        Vector2Int bestDir = Vector2Int.zero;

        foreach (var dir in dirs)
        {
            Cell targetCell = gridManager.GetCellAt(currentCell.gridCoord + dir);
            if (targetCell == null) continue;
            if (dir == -lastDir) continue;
            if (!CheckMove(ai, dir)) continue;

            Vector3 targetPos = targetCell.transform.position;

            // 🔹 Penalti musuh dalam radius
            float enemyScore = 0f;
            float avoidRadius = 2f; // radius 2 cell
            foreach (var enemy in enemyAI.enemyTeammates)
            {
                if (enemy == null) continue;
                float d = Vector3.Distance(targetPos, enemy.transform.position);
                if (d < avoidRadius)
                {
                    enemyScore -= (avoidRadius - d) * 5f; // makin dekat musuh → penalti besar
                }
            }

            // 🔹 Bonus mendekati target passing
            float distTeammate = Vector3.Distance(targetPos, passTargetPos);
            float teammateScore = -distTeammate;

            // 🔹 Directional scoring
            float forwardScore = 0f;
            if (isAttacking)
            {
                forwardScore = dir.y * 3f; // Y+ = maju (bonus)
            }
            else
            {
                forwardScore = -dir.y * 2f; // Y+ = maju (penalty), Y- = mundur (bonus)
            }

            float total = enemyScore + teammateScore + forwardScore;

            if (total > bestScore)
            {
                bestScore = total;
                bestDir = dir;
            }
        }

        if (bestDir != Vector2Int.zero)
            lastDir = bestDir;

        return bestDir;
    }
    
    private Vector3 GetSupportSpot(AIController ai, Vector3 ballHolderPos, bool isAttacking)
    {
        Vector3 myPos = ai.transform.position;
        float idealDist = 0.6f;

        Vector3 dirToBall = (ballHolderPos - myPos).normalized;
        float distToBall = Vector3.Distance(myPos, ballHolderPos);

        Vector3 target;

        if (distToBall > idealDist)
        {
            target = myPos + dirToBall * 0.4f;
        }
        else
        {
            Vector3 side = Vector3.Cross(dirToBall, Vector3.forward).normalized;
            target = myPos + side * 0.3f;
        }

        // 🔥 CLAMP KE ZONA YANG DIIZINKAN
        target = ClampToRoleArea(ai.assignedRole, target);

        return target;
    }

    private Vector3 ClampToRoleArea(string role, Vector3 targetPos)
    {
        if (!roleAllowedAreas.ContainsKey(role))
            return targetPos;

        Cell targetCell = gridManager.GetNearestCell(targetPos);
        if (targetCell == null)
            return targetPos;

        Vector2Int gridCoord = targetCell.gridCoord;
        RoleArea area = roleAllowedAreas[role];

        // Jika sudah di dalam area, return langsung
        if (area.Contains(gridCoord))
            return targetPos;

        // Cari cell terdekat yang valid
        Cell nearestValidCell = gridManager.GetAllCells()
            .Where(c => area.Contains(c.gridCoord))
            .OrderBy(c => Vector3.Distance(c.transform.position, targetPos))
            .FirstOrDefault();

        return nearestValidCell != null ? nearestValidCell.transform.position : targetPos;
    }

    private Vector2Int GetBestMovementForNonBallHolder(AIController ai, Vector3 ballHolderPos, bool isAttacking)
    {
        Cell currentCell = gridManager.GetNearestCell(ai.transform.position);
        if (currentCell == null) return Vector2Int.zero;

        string role = ai.assignedRole;
        Vector3 myPos = ai.transform.position;

        // 🔥 CEK APAKAH PLAYER MELEWATI PENYERANG
        bool playerAhead = ballHolderPos.y > myPos.y; // player sudah di depan AI
        float distToBall = Vector3.Distance(myPos, ballHolderPos);

        Vector2Int[] dirs =
        {
        new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(-1,1),
        new Vector2Int(1,0), new Vector2Int(-1,0),
        new Vector2Int(0,-1), new Vector2Int(1,-1), new Vector2Int(-1,-1)
    };

        // 🔥 LOGIC KHUSUS PENYERANG: Jika player sudah lewat, LANGSUNG MAJU
        if ((role == "ST" || role == "LW" || role == "RW") && playerAhead && isAttacking)
        {
            Debug.Log($"🏃 {role} terlewati player! AUTO MAJU");

            // Prioritaskan gerakan maju (Y+)
            Vector2Int[] forwardDirs =
            {
            new Vector2Int(0, 1),   // maju lurus (prioritas tertinggi)
            new Vector2Int(1, 1),   // maju kanan
            new Vector2Int(-1, 1),  // maju kiri
        };

            foreach (var dir in forwardDirs)
            {
                Cell targetCell = gridManager.GetCellAt(currentCell.gridCoord + dir);
                if (targetCell == null) continue;
                if (dir == -lastDir) continue;
                if (!CheckMove(ai, dir)) continue;

                // ✅ LANGSUNG RETURN tanpa scoring kompleks
                return dir;
            }
        }

        // 🔥 LOGIC KHUSUS: Jika terlalu jauh dari bola DAN attacking, jemput bola
        if ((role == "ST" || role == "RW" || role == "LW") && distToBall > 1.2f && isAttacking)
        {
            Vector2Int towardBall = gridManager.GetNearestCell(ballHolderPos).gridCoord - currentCell.gridCoord;
            towardBall.x = Mathf.Clamp(towardBall.x, -1, 1);
            towardBall.y = Mathf.Clamp(towardBall.y, -1, 1);

            if (CheckMove(ai, towardBall))
            {
                Debug.Log($"🏃 {role} mengejar bola (dist={distToBall:F2})");
                return towardBall;
            }
        }

        // 🔥 SCORING NORMAL untuk role lain atau kondisi normal
        Vector3 supportSpot = GetSupportSpot(ai, ballHolderPos, isAttacking);

        float bestScore = -999f;
        Vector2Int bestDir = Vector2Int.zero;

        foreach (var dir in dirs)
        {
            Cell targetCell = gridManager.GetCellAt(currentCell.gridCoord + dir);
            if (targetCell == null) continue;
            if (dir == -lastDir) continue;
            if (!CheckMove(ai, dir)) continue;

            Vector3 targetPos = targetCell.transform.position;

            // ⚡ SCORING COMPONENTS

            // 1. Ball proximity (lebih kuat untuk penyerang)
            float ballDistance = Vector3.Distance(targetPos, ballHolderPos);
            float ballWeight = (role == "ST" || role == "LW" || role == "RW") ? 3f : 2f;
            float ballScore = -ballDistance * ballWeight;

            // 2. Support positioning (lebih lemah agar tidak dominan)
            float distSupport = Vector3.Distance(targetPos, supportSpot);
            float supportScore = -distSupport * 1f;

            // 3. Forward movement (DIPERKUAT untuk attacking)
            float forwardScore = 0f;
            if (isAttacking)
            {
                // ✅ Bonus besar untuk maju
                if (dir.y > 0) forwardScore = 3f;
                else if (dir.y < 0) forwardScore = -2f;  // penalty mundur
            }
            else
            {
                // Bertahan: bonus mundur
                if (dir.y < 0) forwardScore = 2f;
                else if (dir.y > 0) forwardScore = -1f;
            }

            // 4. Penalty numpuk dengan teman
            float teammateScore = 0f;
            foreach (var teammate in teammates)
            {
                if (teammate == null || teammate == ai) continue;
                float d = Vector3.Distance(targetPos, teammate.transform.position);
                if (d < 0.4f) teammateScore -= (0.4f - d) * 8f;
            }

            // 5. Penalty dekat musuh
            float enemyScore = 0f;
            foreach (var enemy in enemyAI.enemyTeammates)
            {
                if (enemy == null) continue;
                float d = Vector3.Distance(targetPos, enemy.transform.position);
                if (d < 0.5f) enemyScore -= (0.5f - d) * 5f;
            }

            // 🔥 TOTAL SCORE
            float total = ballScore + supportScore + forwardScore + teammateScore + enemyScore;

            // Debug per direction
            //Debug.Log($"  {role} dir={dir}: ball={ballScore:F1}, support={supportScore:F1}, forward={forwardScore:F1}, total={total:F1}");

            if (total > bestScore)
            {
                bestScore = total;
                bestDir = dir;
            }
        }

        if (bestDir != Vector2Int.zero)
        {
            lastDir = bestDir;
            //Debug.Log($"✅ {role} pilih dir={bestDir} (score={bestScore:F1})");
        }
        else
        {
            //Debug.Log($"❌ {role} tidak ada movement valid");
        }

        return bestDir;
    }

    private IEnumerator Move(AIController ai, Vector2 dir)
    {

        Cell currentCell = gridManager.GetNearestCell(ai.transform.position);
        if (currentCell == null)
        {
            yield break;
        }

        Vector2Int current = currentCell.gridCoord;

        Vector2Int offset = new Vector2Int(
            Mathf.RoundToInt(dir.x),
            Mathf.RoundToInt(dir.y)
        );

        Vector2Int target = current + offset;

        Cell targetCell = gridManager.GetCellAt(target);
        if (targetCell == null)
        {
            yield break;
        }

        Vector3 startPos = ai.transform.position;
        Vector3 targetPos = targetCell.transform.position;

        float duration = 0.6f;
        float elapsed = 0f;

        //AudioManager.instance.PlaySFX(AudioManager.instance.sfxDribble);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            ai.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        ai.transform.position = targetPos;

    }

    private Vector2Int GetSafeAttachDirectionAI(AIController holder)
    {
        // Urutan prioritas: depan → kanan → kiri → belakang
        Vector2Int[] dirs = new Vector2Int[]
        {
        new Vector2Int(0, 1),   // depan
        new Vector2Int(1, 0),   // kanan
        new Vector2Int(-1, 0),  // kiri
        new Vector2Int(0, -1),  // belakang
        };

        var enemies = enemyAI.enemyTeammates.ToArray();
        //Debug.Log($"Enemies count: {enemies.Length}");

        float dangerDistance = 1 * 0.14f; // batas aman (atur sesuai kebutuhan)

        // Cek setiap arah berdasarkan prioritas
        foreach (var dir in dirs)
        {
            Vector3 offset = new Vector3(dir.x * 0.14f, dir.y * 0.14f, 0f);
            Vector3 pos = holder.transform.position + offset;

            bool safe = true;

            foreach (var enemy in enemies)
            {
                float d = Vector3.Distance(pos, enemy.transform.position);

                if (d < dangerDistance)
                {
                    safe = false;
                    break;
                }
            }

            // Kalau arah ini aman → langsung pilih
            if (safe)
                return dir;
        }

        // Kalau semua arah berbahaya, pilih yang paling jauh (fallback)
        Vector2Int safestDir = dirs[0];
        float bestDist = -999f;

        foreach (var dir in dirs)
        {
            Vector3 offset = new Vector3(dir.x * 0.14f, dir.y * 0.14f, 0f);
            Vector3 pos = holder.transform.position + offset;

            float minDist = 999f;

            foreach (var enemy in enemies)
            {
                float d = Vector3.Distance(pos, enemy.transform.position);
                if (d < minDist)
                    minDist = d;
            }

            if (minDist > bestDist)
            {
                bestDist = minDist;
                safestDir = dir;
            }
        }

        return safestDir;
    }


    void AttachBallToHolder()
    {
        AIController to = currentHolder;

        //Debug.Log($"Before to : {to}");

        if (to == null || ball == null)
        {
            movementDirAttach = Vector2Int.zero;

            return;
        }

        if (aiFirstAttach)
        {
            movementDirAttach = GetSafeAttachDirectionAI(to);
            aiFirstAttach = false;  // cukup sekali
        }

        //Debug.Log($"After to : {to}");

        float attachSpeed = 1f; // ✅ Naikkan speed biar responsif
        Vector3 baseOffset = new Vector3(0f, 0.14f, 0f);
        Vector3 finalOffset = baseOffset;

        Vector2Int dir = movementDirAttach; // ✅ Langsung pakai, jangan RoundToInt lagi!

        //Debug.Log($"dir team : {dir}");

        if (dir == Vector2Int.zero)
        {
            ball.transform.position = Vector3.MoveTowards(
                    ball.transform.position,
                    to.transform.position + finalOffset,
                    attachSpeed * Time.deltaTime
             );
     
            return;
        }

        // ✅ FIX: Cek mundur DULU sebelum maju
        if (dir.y == -1 && dir.x == 0)       // mundur murni
            finalOffset = new Vector3(0f, -0.14f, 0f);
        else if (dir.y == 1 && dir.x == 0)   // maju murni
            finalOffset = new Vector3(0f, 0.14f, 0f);
        else if (dir.x == -1 && dir.y == 0)  // kiri murni
            finalOffset = new Vector3(-0.14f, 0f, 0f);
        else if (dir.x == 1 && dir.y == 0)   // kanan murni
            finalOffset = new Vector3(0.14f, 0f, 0f);
        else // diagonal
        {
            finalOffset = new Vector3(dir.x * 0.10f, dir.y * 0.10f, 0);
        }

        ball.transform.position = to.transform.position + finalOffset;
    }

}