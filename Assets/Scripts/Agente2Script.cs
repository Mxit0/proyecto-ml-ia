using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class Agente2Script : Agent
{
    public static event System.Action<Agente2Script> OnAgente2Atrapado;

    [SerializeField] private Transform _agente1;
    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private float _rotationSpeed = 180f;
    [SerializeField] private gameManagerScript gameManager;

    private Renderer _renderer;
    private int _episodesWon = 0;
    private int _episodesTotal = 0;
    private bool fueAtrapado = false;

    public int EpisodesWon => _episodesWon;
    public int EpisodesTotal => _episodesTotal;

    public override void Initialize()
    {
        _renderer = GetComponent<Renderer>();
    }

    public override void OnEpisodeBegin()
    {
        _episodesTotal++;
        fueAtrapado = false;
        if (_renderer != null)
            _renderer.material.color = Color.yellow;
        if (gameManager != null)
            gameManager.SpawnAgents();
    }

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

    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);
        AddReward(-2f / MaxStep);

        float distance = Vector3.Distance(transform.localPosition, _agente1.localPosition);
        AddReward(0.01f * distance);

        if (StepCount >= MaxStep - 1 && !fueAtrapado)
        {
            AddWin();
            AddReward(1.0f);
            Debug.Log("Agente2 ganó el episodio (sobrevivió)");
            EndEpisode();
        }
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var action = act[0];
        switch (action)
        {
            case 1:
                transform.position += transform.forward * _moveSpeed * Time.deltaTime;
                break;
            case 2:
                transform.Rotate(0f, -_rotationSpeed * Time.deltaTime, 0f);
                break;
            case 3:
                transform.Rotate(0f, _rotationSpeed * Time.deltaTime, 0f);
                break;
        }
    }

    public void NotificarAtrapado()
    {
        fueAtrapado = true;
        AddReward(-1.0f);
        Debug.Log("Agente2 fue atrapado por Agente1");
        OnAgente2Atrapado?.Invoke(this); // Notifica a los suscriptores
        EndEpisode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (_renderer != null)
                _renderer.material.color = Color.red;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (_renderer != null)
                _renderer.material.color = Color.yellow;
        }
    }

    public void AddWin()
    {
        _episodesWon++;
    }
}