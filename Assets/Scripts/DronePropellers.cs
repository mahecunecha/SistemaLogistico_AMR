using UnityEngine;

public class DronePropellers : MonoBehaviour
{
    [Header("Hélices")]
    [Tooltip("Hélices que giran en sentido horario")]
    public Transform[] clockwisePropellers;

    [Tooltip("Hélices que giran en sentido antihorario")]
    public Transform[] counterClockwisePropellers;

    [Header("Ajustes de Giro")]
    public float maxSpeed = 2000f; // Grados por segundo a máxima velocidad
    public float accelerationRate = 2f; // Tiempo en alcanzar la velocidad máxima

    [Header("Eje de Giro")]
    public Vector3 rotationAxis = Vector3.up; // Cambiar a Vector3.forward o Vector3.right si el eje local varía

    [Header("Estado")]
    public bool isEngineOn = true;

    private float currentSpeed = 0f;

    void Update()
    {
        // Interpola la velocidad para dar un efecto de aceleración/frenado natural
        float targetSpeed = isEngineOn ? maxSpeed : 0f;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, maxSpeed * accelerationRate * Time.deltaTime);

        // Giro en sentido horario
        foreach (Transform prop in clockwisePropellers)
        {
            if (prop != null)
                prop.Rotate(rotationAxis * currentSpeed * Time.deltaTime, Space.Self);
        }

        // Giro en sentido antihorario
        foreach (Transform prop in counterClockwisePropellers)
        {
            if (prop != null)
                prop.Rotate(-rotationAxis * currentSpeed * Time.deltaTime, Space.Self);
        }
    }
}