using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class Agente1Script : Agent
{
    [SerializeField] private Transform _agente2;
    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private float _rotationSpeed = 180f;
    [SerializeField] private gameManagerScript gameManager;
    [SerializeField] private HUDManager hudManager;

    private Renderer _renderer;
    private int _episodesWon = 0;
    private int _episodesTotal = 0;

    public int EpisodesWon => _episodesWon;
    public int EpisodesTotal => _episodesTotal;

    public override void Initialize()
    {
        _renderer = GetComponent<Renderer>();
    }

    public override void OnEpisodeBegin()
    {
        _episodesTotal++;
        if (_renderer != null)
            _renderer.material.color = Color.blue;
        if (gameManager != null)
            gameManager.SpawnAgents();
        if (hudManager != null)
            hudManager.OnEpisodeBegin();
    }

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

    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);
        AddReward(-2f / MaxStep);
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

    protected override void OnEnable()
    {
        base.OnEnable();
        Agente2Script.OnAgente2Atrapado += OnAgente2Atrapado;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        Agente2Script.OnAgente2Atrapado -= OnAgente2Atrapado;
    }

    private void OnAgente2Atrapado(Agente2Script agente2)
    {
        AddWin();
        Debug.Log("Agente1 atrapó a Agente2 (evento)");
    }

    private void Agente2Reached()
    {
        AddReward(1.0f);
        Debug.Log("Agente1 atrapó a Agente2");
        // Ya no llamamos AddWin aquí, lo hace el evento
        if (_agente2 != null && _agente2.TryGetComponent<Agente2Script>(out var script))
            script.NotificarAtrapado();
        EndEpisode();
    }

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

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.01f * Time.deltaTime);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (_renderer != null)
                _renderer.material.color = Color.blue;
        }
    }

    public void AddWin()
    {
        _episodesWon++;
    }
}
