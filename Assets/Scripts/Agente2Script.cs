using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

/// <summary>
/// Clase que representa al agente 2 en el entorno de ML-Agents.
/// Controla movimiento, observaciones, recompensas y gestión de episodios.
/// </summary>
public class Agente2Script : Agent
{
    /// <summary>
    /// Evento que notifica cuando el agente 2 ha sido atrapado.
    /// </summary>
    public static event System.Action<Agente2Script> OnAgente2Atrapado;
    public static event System.Action OnAgente2Gano;

    [SerializeField]
    [Tooltip("Referencia al transform del agente 1 para obtener su posición.")]
    private Transform _agente1;

    [SerializeField]
    [Tooltip("Velocidad de movimiento del agente.")]
    private float _moveSpeed = 1.5f;

    [SerializeField]
    [Tooltip("Velocidad de rotación del agente en grados por segundo.")]
    private float _rotationSpeed = 180f;

    [SerializeField]
    [Tooltip("Referencia al script del gestor del juego para reiniciar agentes.")]
    private gameManagerScript gameManager;

    private Renderer _renderer; // Renderer para cambiar el color del agente
    private int _episodesWon = 0; // Número de episodios ganados
    private int _episodesTotal = 0; // Número total de episodios jugados
    private bool fueAtrapado = false; // Indica si el agente fue atrapado en el episodio actual

    /// <summary>
    /// Propiedad para obtener el número de episodios ganados.
    /// </summary>
    public int EpisodesWon => _episodesWon;

    /// <summary>
    /// Propiedad para obtener el número total de episodios jugados.
    /// </summary>
    public int EpisodesTotal => _episodesTotal;

    /// <summary>
    /// Inicializa el agente, obteniendo componentes necesarios.
    /// </summary>
    public override void Initialize()
    {
        _renderer = GetComponent<Renderer>();
    }

    /// <summary>
    /// Se llama al inicio de cada episodio.
    /// Incrementa el contador de episodios, cambia el color del agente y reinicia posiciones.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        _episodesTotal++; // Solo para estadísticas internas de Agente2
        fueAtrapado = false;
        if (_renderer != null)
            _renderer.material.color = Color.yellow;

        if (gameManager != null)
            gameManager.SpawnAgents();

        // NO LLAMES a hudManager.OnEpisodeBegin() aquí
    }

    /// <summary>
    /// Recopila las observaciones del entorno para alimentar la red neuronal.
    /// Normaliza posiciones y rotación para facilitar el aprendizaje.
    /// </summary>
    /// <param name="sensor">Sensor donde se agregan las observaciones.</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        float agente1PosX_normalized = _agente1.localPosition.x / 5f;
        float agente1PosZ_normalized = _agente1.localPosition.z / 5f;
        float agente2PosX_normalized = transform.localPosition.x / 5f;
        float agente2PosZ_normalized = transform.localPosition.z / 5f;
        float agente2Rotation_normalized = (transform.localRotation.eulerAngles.y / 360f) * 2f - 1f;

        sensor.AddObservation(agente1PosX_normalized);
        sensor.AddObservation(agente1PosZ_normalized);
        sensor.AddObservation(agente2PosX_normalized);
        sensor.AddObservation(agente2PosZ_normalized);
        sensor.AddObservation(agente2Rotation_normalized);
    }

    /// <summary>
    /// Método heurístico para controlar el agente manualmente con teclas W, A, D.
    /// Traduce la entrada de usuario en acciones discretas.
    /// </summary>
    /// <param name="actionsOut">Buffer donde se almacenan las acciones decididas manualmente.</param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;

        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1; // Mover hacia adelante
        }
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 2; // Rotar a la izquierda
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3; // Rotar a la derecha
        }
    }

    /// <summary>
    /// Recibe las acciones del modelo entrenado, ejecuta movimiento y asigna recompensas.
    /// Penaliza por paso y recompensa mantener distancia del agente 1.
    /// Si se agotan los pasos y no ha sido atrapado, se considera victoria.
    /// </summary>
    /// <param name="actions">Acciones discretas recibidas.</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);
        AddReward(-2f / MaxStep);

        float distance = Vector3.Distance(transform.localPosition, _agente1.localPosition);
        AddReward(0.01f * distance); // Recompensa por alejarse del agente1

        if (StepCount >= MaxStep - 1 && !fueAtrapado)
        {
            AddWin();
            AddReward(1.0f);
            Debug.Log("Agente2 ganó el episodio (sobrevivió)");
            OnAgente2Gano?.Invoke();
            EndEpisode();
        }
    }

    /// <summary>
    /// Ejecuta el movimiento o rotación del agente según la acción recibida.
    /// </summary>
    /// <param name="act">Segmento de acciones discretas.</param>
    public void MoveAgent(ActionSegment<int> act)
    {
        var action = act[0];
        switch (action)
        {
            case 1:
                // Mover hacia adelante
                transform.position += transform.forward * _moveSpeed * Time.deltaTime;
                break;
            case 2:
                // Rotar a la izquierda
                transform.Rotate(0f, -_rotationSpeed * Time.deltaTime, 0f);
                break;
            case 3:
                // Rotar a la derecha
                transform.Rotate(0f, _rotationSpeed * Time.deltaTime, 0f);
                break;
        }
    }

    /// <summary>
    /// Marca al agente como atrapado, aplica penalización, notifica a suscriptores y termina episodio.
    /// </summary>
    public void NotificarAtrapado()
    {
        fueAtrapado = true;
        AddReward(-1.0f);
        Debug.Log("Agente2 fue atrapado por Agente1");
        OnAgente2Atrapado?.Invoke(this); // Notifica a los suscriptores del evento
        EndEpisode();
    }

    /// <summary>
    /// Cambia el color del agente a rojo cuando colisiona con una pared.
    /// </summary>
    /// <param name="collision">Información de la colisión.</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (_renderer != null)
                _renderer.material.color = Color.red;
        }
    }

    /// <summary>
    /// Penaliza mientras el agente permanece en contacto con una pared.
    /// </summary>
    /// <param name="collision">Información de la colisión.</param>
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.09f * Time.deltaTime);
        }
    }

    /// <summary>
    /// Restaura el color amarillo del agente cuando deja de colisionar con una pared.
    /// </summary>
    /// <param name="collision">Información de la colisión.</param>
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (_renderer != null)
                _renderer.material.color = Color.yellow;
        }
    }

    /// <summary>
    /// Incrementa el contador de episodios ganados.
    /// </summary>
    public void AddWin()
    {
        _episodesWon++;
    }
}
