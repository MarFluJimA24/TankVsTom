// Assets/Scripts/PlayerTiltMove.cs
using UnityEngine;
using UnityEngine.InputSystem;

// Movimiento del jugador solo con acelerómetro (portrait fijo), eje X.
// Sin invertir, sin calibrar, sin auto-detección.
// Compatible con New Input System (sin UnityEngine.Input).
public class PlayerTiltMove : MonoBehaviour
{
    [Header("Movement Limits")]
    public float maxX = 6f;

    [Header("Tilt")]
    public float tiltMultiplier = 8f; // Escala del acelerómetro.
    public float smoothing = 12f;     // Suavizado para evitar temblores.
    public float deadZone = 0.05f;    // Zona muerta para evitar jitter.

    private float currentX;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        // Asegura que el acelerómetro está habilitado en New Input System.
        if (Accelerometer.current != null)
            InputSystem.EnableDevice(Accelerometer.current);
    }

    void Start()
    {
        currentX = transform.position.x;
    }

    void Update()
    {
        float tiltX = ReadTiltX();

        // Hacer visible el sensor en HUD (si existe).
        GameManager.I?.SetTiltValue(tiltX);

        // Convertir tilt a posición objetivo en X (posición absoluta relativa al centro).
        float targetX = tiltX * tiltMultiplier;

        // Limitar para no salir del área.
        targetX = Mathf.Clamp(targetX, -maxX, maxX);

        // Suavizado.
        currentX = Mathf.Lerp(currentX, targetX, Time.deltaTime * smoothing);

        // Seguridad extra (por si acaso).
        currentX = Mathf.Clamp(currentX, -maxX, maxX);

        // Aplicar movimiento (Rigidbody si existe, si no transform).
        if (rb != null && !rb.isKinematic)
        {
            Vector3 p = rb.position;
            p.x = currentX;
            rb.MovePosition(p);
        }
        else
        {
            Vector3 p = transform.position;
            p.x = currentX;
            transform.position = p;
        }
    }

    // Tope definitivo: si cualquier otro script o física mueve el player después de Update,
    // aquí lo recortamos igualmente.
    void LateUpdate()
    {
        if (rb != null && !rb.isKinematic)
        {
            Vector3 p = rb.position;
            p.x = Mathf.Clamp(p.x, -maxX, maxX);
            rb.position = p;
        }
        else
        {
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, -maxX, maxX);
            transform.position = p;
        }
    }

    private float ReadTiltX()
    {
        float tiltX = 0f;

        // En móvil: acelerómetro del New Input System.
        if (Accelerometer.current != null)
        {
            tiltX = Accelerometer.current.acceleration.ReadValue().x;
        }
        else
        {
            // En editor/PC: fallback con teclado (New Input System).
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) tiltX -= 0.2f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) tiltX += 0.2f;
            }
        }

        // Zona muerta
        if (Mathf.Abs(tiltX) < deadZone) tiltX = 0f;

        // Limitar a rango razonable
        return Mathf.Clamp(tiltX, -1f, 1f);
    }
}
