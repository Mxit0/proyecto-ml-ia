using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Transactions;
public class Agente1Script : Agent
{

    [SerializeField] private Transform _agente2;
    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private float _rotationSpeed = 180f;

    private Renderer _renderer;

    private int _currentEpisode = 0;
    private float _cumulativeReward = 0f;
    public override void Initialize()
    {
        Debug.Log("Initialize");

        _renderer = GetComponent<Renderer>();
        _currentEpisode = 0;
        _cumulativeReward = 0f;
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("OnEpisodeBegin()");

        _currentEpisode++;
        _cumulativeReward = 0f;
        _renderer.material.color = Color.blue;

        SpawnObjects();
    }

    private void SpawnObjects()
    {
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector3(0f, 0.15f, 0f);

        float randomAngle = Random.Range(0f, 360f);
        Vector3 randomDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;

        float randomDistance = Random.Range(1f, 2.5f);

        Vector3 agente2Position = transform.localPosition + randomDirection * randomDistance;

        _agente2.localPosition = new Vector3(agente2Position.x, 0.3f, agente2Position.z);
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

    private void OTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Agente2"))
        {
            Agente2Reached();
        }
    }

    private void Agente2Reached()
    {
        AddReward(1.0f);
        _cumulativeReward = GetCumulativeReward();

        EndEpisode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        AddReward(-0.05f);

        if (_renderer != null)
        {
            _renderer.material.color = Color.red;
        }
    }

    private void OnColisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.01f * Time.deltaTime);
        }
    }

    private void OnColisionExit(Collision collision)
    {
        if (_renderer != null)
        {
            _renderer.material.color = Color.blue;
        }
    }

}
