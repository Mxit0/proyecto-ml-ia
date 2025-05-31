/*
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
public class Agente2Script : Agent
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
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("OnEpisodeBegin()");
    }

    public override void CollectObservations(VectorSensor sensor)
    {

    }


    public override void OnActionReceived(ActionBuffers actions)
    {

    }
}
*/