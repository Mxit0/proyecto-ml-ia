using UnityEngine;
using System.Collections;
/// <summary>
/// Clase encargada de gestionar la ubicación inicial de los agentes en la escena.
/// </summary>
public class gameManagerScript : MonoBehaviour
{
    /// <summary>
    /// Componente Transform del Agente1 (generalmente el perseguidor).
    /// </summary>
    public Transform agente1;

    /// <summary>
    /// Componente Transform del Agente2 (generalmente el perseguido).
    /// </summary>
    public Transform agente2;

    [Header("Renderer del suelo")]
    public Renderer groundRenderer;

    private Material groundMaterial;
    private Coroutine glowCoroutine;
    private Color baseColor;

    void Awake()
    {
        if (groundRenderer != null)
        {
            groundMaterial = groundRenderer.material;
            baseColor = groundMaterial.GetColor("_EmissionColor");
        }
    }

    /// <summary>
    /// Llama este método para hacer el glow del suelo.
    /// </summary>
    public void PlayGroundGlow(Color color, float glowIntensity = 2.5f, float glowTime = 0.3f, float fadeTime = 0.4f)
    {
        if (glowCoroutine != null)
            StopCoroutine(glowCoroutine);
        glowCoroutine = StartCoroutine(GroundGlowCoroutine(color, glowIntensity, glowTime, fadeTime));
    }

    private IEnumerator GroundGlowCoroutine(Color color, float intensity, float glowTime, float fadeTime)
    {
        if (groundMaterial == null) yield break;

        // Glow fuerte
        groundMaterial.SetColor("_EmissionColor", color * intensity);
        DynamicGI.SetEmissive(groundRenderer, color * intensity);

        yield return new WaitForSeconds(glowTime);

        // Desvanecer suavemente
        float t = 0f;
        Color startEmission = color * intensity;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            Color faded = Color.Lerp(startEmission, baseColor, t / fadeTime);
            groundMaterial.SetColor("_EmissionColor", faded);
            DynamicGI.SetEmissive(groundRenderer, faded);
            yield return null;
        }

        // Restaurar color base
        groundMaterial.SetColor("_EmissionColor", baseColor);
        DynamicGI.SetEmissive(groundRenderer, baseColor);
    }

    /// <summary>
    /// Método para posicionar ambos agentes al inicio de un episodio.
    /// Coloca Agente1 en el centro con rotación neutral.
    /// Posiciona Agente2 aleatoriamente a cierta distancia de Agente1,
    /// evitando que esté cerca de las paredes o del Agente1.
    /// </summary>
    public void SpawnAgents()
    {
        // Coloca Agente1 en la posición central con rotación neutra
        agente1.localRotation = Quaternion.identity;
        agente1.localPosition = new Vector3(0f, 0.5f, 0f);

        // Genera un ángulo aleatorio para posicionar Agente2 alrededor de Agente1
        float randomAngle = Random.Range(0f, 360f);
        Vector3 randomDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;

        // Define una distancia aleatoria entre 2.5 y 4 unidades para mantener separación
        float randomDistance = Random.Range(2.5f, 4f);

        // Calcula la posición final de Agente2 en base a la dirección y distancia aleatoria
        Vector3 agente2Position = agente1.localPosition + randomDirection * randomDistance;

        // Ajusta la posición en Y a 0.5 para mantener altura constante
        agente2.localPosition = new Vector3(agente2Position.x, 0.5f, agente2Position.z);

        // Coloca Agente2 con rotación neutra
        agente2.localRotation = Quaternion.identity;

        /*
        if (groundRenderer != null)
        {
            baseColor = groundMaterial.GetColor("_EmissionColor");
        }
        */
    }

    private void OnEnable()
    {
        Agente1Script.OnAgente1Gano += OnAgente1Gano;
        Agente2Script.OnAgente2Gano += OnAgente2Gano;
    }

    private void OnDisable()
    {
        Agente1Script.OnAgente1Gano -= OnAgente1Gano;
        Agente2Script.OnAgente2Gano -= OnAgente2Gano;
    }

    private void OnAgente1Gano()
    {
        PlayGroundGlow(Color.blue);
    }

    private void OnAgente2Gano()
    {
        PlayGroundGlow(Color.yellow);
    }
}
