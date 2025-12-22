using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public AIController owner;
    public float passForce = 6f;

    private Rigidbody2D rb;

    private GameOverManager gameOverManager;

    void Start()
    {
        gameOverManager = FindAnyObjectByType<GameOverManager>();
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0f;       // Tidak jatuh
        rb.linearDamping = 1.5f;             // Hambatan linear (friksi udara)
        rb.angularDamping = 0.5f;      // Gesekan rotasi
    }

    void Update()
    {
        if (rb != null && rb.linearVelocity.magnitude > 0.01f)
            rb.linearVelocity *= 0.98f; // pelan-pelan melambat
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log("⚠️ Bola menyentuh enemy → GAME OVER!");
            //gameOverManager.ShowGameOver();
        }
    }

    public void SetOwner(AIController newOwner)
    {
        owner = newOwner;
        transform.position = owner.transform.position;
    }

    public void KickTo(Vector3 targetPos)
    {
        if (rb == null) return;

        Vector3 dir = (targetPos - transform.position).normalized;

        // ✅ Gunakan velocity, bukan linearVelocity
        rb.linearVelocity = dir * passForce;

        Debug.Log($"💨 Bola ditendang ke {targetPos} dengan kecepatan {passForce}");
    }

    public IEnumerator Kickoff(TeamAIManager teamAI)
    {
        yield return new WaitForSeconds(1.5f);

        // Kiper (GK) cari target
        var gk = teamAI.teammates.FirstOrDefault(t => t.assignedRole == "GK");
        if (gk == null) yield break;

        var targets = teamAI.teammates.Where(t => t != gk).ToList();
        if (targets.Count == 0) yield break;

        // Pilih teammate secara acak (AI bebas)
        var target = targets[Random.Range(0, targets.Count)];
        Debug.Log($"GK mengumpan bola ke {target.assignedRole}");

        KickTo(target.transform.position);
        owner = null;
    }
}
