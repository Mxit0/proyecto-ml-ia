using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class Agente2Script : Agent
{
    [SerializeField] private Transform _agente1;
    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private float _rotationSpeed = 180f;
    [SerializeField] private gameManagerScript gameManager;

    private Renderer _renderer;
    private int _currentEpisode = 0;
    private float _cumulativeReward = 0f;

    public override void Initialize()
    {
        _renderer = GetComponent<Renderer>();
        _currentEpisode = 0;
        _cumulativeReward = 0f;
    }

    public override void OnEpisodeBegin()
    {
        _currentEpisode++;
        _cumulativeReward = 0f;
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

        _cumulativeReward = GetCumulativeReward();
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
    private void Agente1Caught()
    {
        AddReward(-1.0f);
        _cumulativeReward = GetCumulativeReward();
        EndEpisode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Agente1"))
        {
            Agente1Caught();
        }

        AddReward(-0.05f);
        if (_renderer != null)
        {
            _renderer.material.color = Color.red;
        }
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
        if (_renderer != null)
        {
            _renderer.material.color = Color.yellow;
        }
    }
}