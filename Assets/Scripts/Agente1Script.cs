using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

/// <summary>
/// Clase que representa al agente 1 en el entorno de ML-Agents.
/// Controla el movimiento, observaciones, recompensas y manejo de episodios.
/// </summary>
public class Agente1Script : Agent
{
    [SerializeField]
    [Tooltip("Referencia al transform del agente 2 para obtener su posición.")]
    private Transform _agente2;

    [SerializeField]
    [Tooltip("Velocidad de movimiento del agente.")]
    private float _moveSpeed = 1.5f;

    [SerializeField]
    [Tooltip("Velocidad de rotación del agente en grados por segundo.")]
    private float _rotationSpeed = 180f;

    [SerializeField]
    [Tooltip("Referencia al script del gestor del juego para reiniciar agentes.")]
    private gameManagerScript _gameManager;

    [SerializeField]
    [Tooltip("Referencia al gestor de la interfaz HUD para actualizar visualmente los episodios.")]
    private HUDManager hudManager;

    private Renderer _renderer;  // Renderer para cambiar el color del agente
    private int _episodesWon = 0; // Número de episodios ganados
    private int _episodesTotal = 0; // Número total de episodios jugados

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
        _episodesTotal++;
        if (_renderer != null)
            _renderer.material.color = Color.blue;

        if (_gameManager != null)
            _gameManager.SpawnAgents();

        if (hudManager != null)
            hudManager.OnEpisodeBegin();
    }

    /// <summary>
    /// Recopila las observaciones del entorno que serán usadas por la red neuronal.
    /// Normaliza posiciones y rotación para facilitar el aprendizaje.
    /// </summary>
    /// <param name="sensor">Sensor donde se agregan las observaciones.</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        float agente2PosX_normalized = _agente2.localPosition.x / 5f;
        float agente2PosZ_normalized = _agente2.localPosition.z / 5f;
        float agente1PosX_normalized = transform.localPosition.x / 5f;
        float agente1PosZ_normalized = transform.localPosition.z / 5f;
        float agente1Rotation_normalized = (transform.localRotation.eulerAngles.y / 360f) * 2f - 1f;

        sensor.AddObservation(agente2PosX_normalized);
        sensor.AddObservation(agente2PosZ_normalized);
        sensor.AddObservation(agente1PosX_normalized);
        sensor.AddObservation(agente1PosZ_normalized);
        sensor.AddObservation(agente1Rotation_normalized);
    }

    /// <summary>
    /// Método heurístico para controlar el agente manualmente con las flechas del teclado.
    /// Traduce la entrada de usuario en acciones discretas.
    /// </summary>
    /// <param name="actionsOut">Buffer donde se almacenan las acciones decididas manualmente.</param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            discreteActionsOut[0] = 1; // Mover hacia adelante
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActionsOut[0] = 2; // Rotar a la izquierda
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActionsOut[0] = 3; // Rotar a la derecha
        }
    }

    /// <summary>
    /// Recibe las acciones decididas por el modelo entrenado y las ejecuta.
    /// También aplica una pequeña penalización por paso para incentivar la rapidez.
    /// </summary>
    /// <param name="actions">Acciones discretas recibidas.</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);
        AddReward(-2f / MaxStep);
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
    /// Se suscribe al evento cuando el Agente2 es atrapado.
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        Agente2Script.OnAgente2Atrapado += OnAgente2Atrapado;
    }

    /// <summary>
    /// Se desuscribe del evento al deshabilitarse el agente.
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        Agente2Script.OnAgente2Atrapado -= OnAgente2Atrapado;
    }

    /// <summary>
    /// Callback que se ejecuta cuando el agente 2 es atrapado.
    /// Suma una victoria y muestra mensaje en consola.
    /// </summary>
    /// <param name="agente2">Referencia al agente 2 atrapado.</param>
    private void OnAgente2Atrapado(Agente2Script agente2)
    {
        AddWin();
        Debug.Log("Agente1 atrapó a Agente2 (evento)");
    }

    /// <summary>
    /// Método que se llama cuando el agente 1 alcanza al agente 2.
    /// Otorga recompensa, notifica al agente 2 y termina el episodio.
    /// </summary>
    private void Agente2Reached()
    {
        AddReward(1.0f);
        Debug.Log("Agente1 atrapó a Agente2");
        if (_agente2 != null && _agente2.TryGetComponent<Agente2Script>(out var script))
            script.NotificarAtrapado();
        EndEpisode();
    }

    /// <summary>
    /// Detecta colisiones para determinar interacciones con otros objetos.
    /// Penaliza colisiones con paredes y detecta atrapamiento del agente 2.
    /// </summary>
    /// <param name="collision">Información de la colisión.</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Agente2"))
        {
            if (collision.gameObject.TryGetComponent<Agente2Script>(out var script))
                script.NotificarAtrapado();
            Agente2Reached();
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            if (_renderer != null)
                _renderer.material.color = Color.red;
        }

        AddReward(-0.05f);
    }

    /// <summary>
    /// Penaliza continuamente mientras el agente está en contacto con una pared.
    /// </summary>
    /// <param name="collision">Información de la colisión.</param>
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.05f * Time.deltaTime);
        }
    }

    /// <summary>
    /// Restablece el color del agente a azul cuando deja de colisionar con una pared.
    /// </summary>
    /// <param name="collision">Información de la colisión.</param>
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (_renderer != null)
                _renderer.material.color = Color.blue;
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
