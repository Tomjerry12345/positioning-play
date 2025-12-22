using UnityEngine;
using UnityEngine.InputSystem;

public class Cell : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color defaultColor;
    public Color hoverColor = Color.yellow;
    private float lastClickTime = 0f;
    private float doubleClickThreshold = 0.3f; // waktu maksimal antar klik

    private PlayerController player;
    private Camera cam;
    private GameManager gameManager;
    private TimeManager timeManager;

    public Transform enemyGoal;

    [HideInInspector] public Vector2Int gridCoord; // ✅ koordinat cell di grid

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        defaultColor = sr.color;
        player = FindObjectOfType<PlayerController>();
        cam = Camera.main;
        gameManager = FindObjectOfType<GameManager>();
        enemyGoal = GameObject.FindGameObjectWithTag("EnemyGoal").transform;
        timeManager = FindAnyObjectByType<TimeManager>();
    }

    void Update()
    {
        //if (Mouse.current.leftButton.wasPressedThisFrame && gameManager.IsPlayerTurn)
        //{
        //    Debug.Log("Klikked Mouse");
        //    Vector2 mousePos = Mouse.current.position.ReadValue();
        //    Ray ray = cam.ScreenPointToRay(mousePos);
        //    Plane plane = new Plane(Vector3.forward, Vector3.zero);
        //    float distance;

        //    if (plane.Raycast(ray, out distance))
        //    {
        //        Vector3 worldPos = ray.GetPoint(distance);
        //        Collider2D hit = Physics2D.OverlapPoint(worldPos);

        //        if (hit != null && hit.gameObject == gameObject)
        //        {
        //            player.MoveTo(transform.position);
        //        } 
        //        else
        //        {
        //            float timeSinceLastClick = Time.time - lastClickTime;
        //            lastClickTime = Time.time;

        //            if (timeSinceLastClick <= doubleClickThreshold)
        //            {
        //                HandleDoubleClick();
        //            }
        //        }
        //    }
        //}

        // MOUSE (PC & EDITOR)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("Klik PC");
        }

        // TOUCH (ANDROID)
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            //Debug.Log("Klik android");

            if (!gameManager.IsPlayerTurn)
                return;

            Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(touchPos);
            Plane plane = new Plane(Vector3.forward, Vector3.zero);
            float distance;

            if (plane.Raycast(ray, out distance))
            {
                Vector3 worldPos = ray.GetPoint(distance);
                Collider2D hit = Physics2D.OverlapPoint(worldPos);

                if (hit != null && hit.gameObject == gameObject)
                {
                    timeManager.ResetMovement();
                    player.MoveToCell(gridCoord, gameManager);
                }
                else
                {
                    float timeSinceLastClick = Time.time - lastClickTime;
                    lastClickTime = Time.time;

                    if (timeSinceLastClick <= doubleClickThreshold)
                    {
                        timeManager.ResetMovement();
                        HandleDoubleClick();
                    }
                }
            }
        }
    }


    void OnMouseEnter() => sr.color = hoverColor;
    void OnMouseExit() => sr.color = defaultColor;

    private void HandleDoubleClick()
    {
        if (player == null) return;

        Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(touchPos);
        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        float dist;

        if (!plane.Raycast(ray, out dist))
            return;

        Vector3 worldPos = ray.GetPoint(dist);

        // Cukup panggil player
        player.TryPassBall(worldPos);

    }

}
