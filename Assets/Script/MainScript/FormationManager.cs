using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FormationManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject teammatePrefab;
    public GameObject enemyPrefab;
    public GameObject ballPrefab;

    private PlayerController activePlayer;

    [Header("References")]
    public GridManager gridManager;

    [System.Serializable]
    public class FormationPosition
    {
        public Vector2Int gridCoord;
        public string role;
        public bool isPlayerControlled;

        public Sprite sprite;   // 👈 tambahkan ini
        public Sprite spriteController;
    }

    [Header("Formation Settings")]
    public List<FormationPosition> listTeammate = new List<FormationPosition>()
    {
        // 🧤 GK (Goalkeeper)
        new FormationPosition() { gridCoord = new Vector2Int(4, 0), role = "GK", isPlayerControlled = false },

        // 🛡️ DEFENSE (4 pemain)
        new FormationPosition() { gridCoord = new Vector2Int(1, 4), role = "LB", isPlayerControlled = false },
        new FormationPosition() { gridCoord = new Vector2Int(2, 2), role = "CBL", isPlayerControlled = false },
        new FormationPosition() { gridCoord = new Vector2Int(6, 2), role = "CBR", isPlayerControlled = false },
        new FormationPosition() { gridCoord = new Vector2Int(7, 4), role = "RB", isPlayerControlled = false },

        // ⚙️ MIDFIELD (3 pemain)
        new FormationPosition() { gridCoord = new Vector2Int(2, 6), role = "CM", isPlayerControlled = false },
        new FormationPosition() { gridCoord = new Vector2Int(4, 7), role = "AM", isPlayerControlled = false },
        new FormationPosition() { gridCoord = new Vector2Int(6, 6), role = "DM", isPlayerControlled = false },

        // ⚡ FORWARD (3 pemain)
        new FormationPosition() { gridCoord = new Vector2Int(1, 9), role = "LW", isPlayerControlled = false },
        new FormationPosition() { gridCoord = new Vector2Int(4, 10), role = "ST", isPlayerControlled = false }, // Player
        new FormationPosition() { gridCoord = new Vector2Int(7, 9), role = "RW", isPlayerControlled = false }
    };

    [Header("Formation Settings")]
    [NonSerialized]
    public List<FormationPosition> listEnemy = new List<FormationPosition>()
    {
        // 🧤 GK (Goalkeeper)
        new FormationPosition() { gridCoord = new Vector2Int(4, 14), role = "GK", isPlayerControlled = false },

        //// 🛡️ DEFENSE (4 pemain)
        new FormationPosition() { gridCoord = new Vector2Int(1, 11), role = "LB", isPlayerControlled = false },
        new FormationPosition() { gridCoord = new Vector2Int(3, 12), role = "CBL", isPlayerControlled = false },
        new FormationPosition() { gridCoord = new Vector2Int(5, 12), role = "CBR", isPlayerControlled = false },
        new FormationPosition() { gridCoord = new Vector2Int(7, 11), role = "RB", isPlayerControlled = false },

        // ⚙️ MIDFIELD (3 pemain)
        new FormationPosition() { gridCoord = new Vector2Int(1, 7), role = "LCM", isPlayerControlled = false },
        new FormationPosition() { gridCoord = new Vector2Int(4, 8), role = "DM", isPlayerControlled = false },
        new FormationPosition() { gridCoord = new Vector2Int(7, 7), role = "RCM", isPlayerControlled = false },

        // ⚡ FORWARD (3 pemain)
        new FormationPosition() { gridCoord = new Vector2Int(2, 5), role = "LW", isPlayerControlled = false },
        new FormationPosition() { gridCoord = new Vector2Int(4, 6), role = "ST", isPlayerControlled = false }, // Player
        new FormationPosition() { gridCoord = new Vector2Int(6, 5), role = "RW", isPlayerControlled = false }
    };

    private List<Cell> allCells = new List<Cell>();

    // ✨ TAMBAHKAN LIST UNTUK TRACKING
    private List<AIController> spawnedTeammates = new List<AIController>();
    private List<AIController> spawnedEnemies = new List<AIController>();


    Cell GetCell(int x, int y)
    {
        int cols = gridManager.cellCountX; // 5
        int index = y * cols + x;
        if (index >= 0 && index < allCells.Count)
            return allCells[index];

        //Debug.LogWarning($"Cell index {index} out of range (x:{x}, y:{y}, cols:{cols}, totalCells:{allCells.Count})");
        return null;
    }

    // -------------------------------------------------------
    // 🔁 Reset posisi TEAMMATE ke formasi awal
    // -------------------------------------------------------
    public void ApplyFormation(List<AIController> teammates)
    {
        if (gridManager == null)
        {
            Debug.LogWarning("⚠️ GridManager belum diset di FormationManager!");
            return;
        }

        if (teammates == null || teammates.Count == 0)
        {
            Debug.LogWarning("⚠️ Tidak ada pemain untuk di-reset formasinya.");
            return;
        }

        Debug.Log("📋 [TEAM] Menerapkan formasi ulang ke semua pemain...");

        foreach (var t in teammates)
        {
            // Cari data formasi berdasarkan role pemain
            var form = listTeammate.Find(f => f.role == t.assignedRole);
            if (form != null)
            {
                Cell targetCell = GetCell(form.gridCoord.x, form.gridCoord.y);
                if (targetCell != null)
                {
                    t.transform.position = targetCell.transform.position;
                    //Debug.Log($"🔄 [TEAM] {t.assignedRole} dipindahkan ke {form.gridCoord}");
                }
            }
            else
            {
                //Debug.LogWarning($"❌ [TEAM] Role {t.assignedRole} tidak ditemukan di formasi433");
            }
        }
    }

    // -------------------------------------------------------
    // 🔁 Reset posisi ENEMY ke formasi awal
    // -------------------------------------------------------
    public void ApplyEnemyFormation(List<AIController> enemies)
    {
        if (gridManager == null)
        {
            Debug.LogWarning("⚠️ GridManager belum diset di FormationManager!");
            return;
        }

        if (enemies == null || enemies.Count == 0)
        {
            Debug.LogWarning("⚠️ Tidak ada enemy untuk di-reset formasinya.");
            return;
        }

        Debug.Log("📋 [ENEMY] Menerapkan formasi ulang ke semua enemy...");

        foreach (var e in enemies)
        {
            // Cari data formasi berdasarkan role enemy
            var form = listEnemy.Find(f => f.role == e.assignedRole);
            if (form != null)
            {
                Cell targetCell = GetCell(form.gridCoord.x, form.gridCoord.y);
                if (targetCell != null)
                {
                    e.transform.position = targetCell.transform.position;
                    //Debug.Log($"🔄 [ENEMY] {e.assignedRole} dipindahkan ke {form.gridCoord}");
                }
            }
            else
            {
                //Debug.LogWarning($"❌ [ENEMY] Role {e.assignedRole} tidak ditemukan di formasi enemy");
            }
        }
    }

    void Start()
    {
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();

        // Ambil semua cell dari gridManager
        allCells.AddRange(FindObjectsOfType<Cell>());

        // Urutkan cell dari bawah ke atas, kiri ke kanan
        allCells.Sort((a, b) =>
        {
            int compareY = a.transform.position.y.CompareTo(b.transform.position.y);
            return compareY == 0 ? a.transform.position.x.CompareTo(b.transform.position.x) : compareY;
        });

        //Debug.Log("Total cell ditemukan: " + allCells.Count);

        StartSpawn();
    }

    private void StartSpawn()
    {
        AudioManager.instance.PlaySFX(AudioManager.instance.sfxPenonton);

        SpawnFormation433();
    }

    void SpawnFormation433()
    {
        TeamAIManager teamAI = FindObjectOfType<TeamAIManager>();
        EnemyAIManager enemyAI = FindObjectOfType<EnemyAIManager>();

        spawnTeammate();
        spawnEnemy();
        spawnBall(teamAI, enemyAI);
        RegisterTeammates();
        RegisterEnemy();

        if (teamAI != null)
            StartCoroutine(StartKickoff(teamAI));
    }

    void spawnTeammate()
    {
        string roleShort = PlayerPrefs.GetString("RoleShort");

        foreach (var pos in listTeammate)
        {
            Cell cell = GetCell(pos.gridCoord.x, pos.gridCoord.y);
            if (cell == null)
            {
                Debug.LogWarning($"Cell not found at {pos.gridCoord} for {pos.role}");
                continue;
            }

            // 🔥 Tentukan siapa player sebelum spawn prefab
            if (pos.role == roleShort)
                pos.isPlayerControlled = true;
            else
                pos.isPlayerControlled = false;

            GameObject prefab = pos.isPlayerControlled ? playerPrefab : teammatePrefab;
            GameObject obj = Spawn(prefab, cell);

            // ✨ TAG SEBAGAI TEAMMATE
            obj.tag = "Teammate";

            //SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            SpriteRenderer sr = obj.GetComponentInChildren<SpriteRenderer>();

            Debug.Log($"{pos.role} Sprite: {pos.sprite}");

            if (sr != null && pos.sprite != null)
            {
                sr.sprite = pos.sprite;
            }

            AIController teammate = obj.GetComponent<AIController>();
            if (teammate != null)
            {
                teammate.SetRole(pos.role);
                spawnedTeammates.Add(teammate); // ✨ SIMPAN KE LIST TEAMMATE
                //Debug.Log($"✅ [TEAM] Spawned {pos.role} at {pos.gridCoord}");
            }

            if (pos.isPlayerControlled)
            {
                activePlayer = obj.GetComponent<PlayerController>();
                sr.sprite = pos.spriteController;
            }
        }
    }

    void spawnEnemy()
    {
        //Debug.Log("Total enemy ditemukan: " + listEnemy.Count);

        foreach (var pos in listEnemy)
        {
            Cell cell = GetCell(pos.gridCoord.x, pos.gridCoord.y);
            if (cell == null)
            {
                Debug.LogWarning($"Cell not found at {pos.gridCoord} for {pos.role}");
                continue;
            }

            GameObject obj = Spawn(enemyPrefab, cell);

            // ✨ TAG SEBAGAI ENEMY
            obj.tag = "Enemy";

            AIController enemy = obj.GetComponent<AIController>();
            if (enemy != null)
            {
                enemy.SetRole(pos.role);
                enemy.SetGridCoord(pos.gridCoord);
                spawnedEnemies.Add(enemy); // ✨ SIMPAN KE LIST ENEMY
                //Debug.Log($"✅ [ENEMY] Spawned {pos.role} at {pos.gridCoord}");
            }
        }
    }

    void spawnBall(TeamAIManager teamAI, EnemyAIManager enemyAI)
    {
        // Ball di tengah lapangan (x:2, y:5)
        var midCell = GetCell(4, 1);
        if (midCell != null)
        {
            GameObject ball = Instantiate(ballPrefab, midCell.transform.position, Quaternion.identity);
            BallController ballCtrl = ball.GetComponent<BallController>();
            if (ballCtrl == null)
                ballCtrl = ballPrefab.AddComponent<BallController>();

            //Debug.Log("Ball spawned at center");

            // Daftarkan bola ke TeamAI
            if (teamAI != null)
            {
                teamAI.ball = ballCtrl;
                teamAI.player = activePlayer;
            }

            // ✨ Daftarkan bola ke EnemyAI juga
            if (enemyAI != null)
            {
                enemyAI.ball = ballCtrl;
            }
        }
    }

    GameObject Spawn(GameObject prefab, Cell cell)
    {
        if (cell == null) return null;

        GameObject obj = Instantiate(prefab, cell.transform.position, Quaternion.identity);
        obj.transform.SetParent(transform);
        return obj;
    }

    void RegisterTeammates()
    {
        TeamAIManager teamAI = FindObjectOfType<TeamAIManager>();
        if (teamAI != null)
        {
            teamAI.teammates.Clear();
            // ✨ GUNAKAN LIST YANG SUDAH DI-TRACK, BUKAN FindObjectsOfType
            teamAI.teammates.AddRange(spawnedTeammates);

            // Assign formation manager reference ke TeamAI
            teamAI.formationManager = this;

            //Debug.Log($"✅ [TEAM] Teammate terdaftar di TeamAIManager: {teamAI.teammates.Count}");
        }
    }

    void RegisterEnemy()
    {
        EnemyAIManager enemyAI = FindObjectOfType<EnemyAIManager>();
        if (enemyAI != null)
        {
            enemyAI.enemyTeammates.Clear();
            // ✨ GUNAKAN LIST YANG SUDAH DI-TRACK
            enemyAI.enemyTeammates.AddRange(spawnedEnemies);

            // ✨ Set reference
            enemyAI.enemyFormationManager = this; // Untuk reset formasi enemy
            enemyAI.gridManager = gridManager;

            // ✨ Set reference ke player team
            TeamAIManager teamAI = FindObjectOfType<TeamAIManager>();
            if (teamAI != null)
            {
                enemyAI.playerTeam = teamAI;
            }

            //Debug.Log($"✅ [ENEMY] Enemy terdaftar di EnemyTeamManager: {enemyAI.enemyTeammates.Count}");
        }
    }

    private IEnumerator StartKickoff(TeamAIManager teamAI)
    {
        yield return new WaitForSeconds(1f);
        teamAI.StartPlay();
    }
}