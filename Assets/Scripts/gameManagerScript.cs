using UnityEngine;

public class gameManagerScript : MonoBehaviour
{
    public Transform agente1;
    public Transform agente2;

    public void SpawnAgents()
    {
        // Agente1 en el centro
        agente1.localRotation = Quaternion.identity;
        agente1.localPosition = new Vector3(0f, 0.5f, 0f);

        // Agente2 lejos de paredes y de Agente1
        float randomAngle = Random.Range(0f, 360f);
        Vector3 randomDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;
        float randomDistance = Random.Range(2.5f, 4f); // Aumenta la distancia mínima

        Vector3 agente2Position = agente1.localPosition + randomDirection * randomDistance;
        agente2.localPosition = new Vector3(agente2Position.x, 0.5f, agente2Position.z);
        agente2.localRotation = Quaternion.identity;
    }
}
