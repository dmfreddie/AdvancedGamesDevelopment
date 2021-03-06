﻿using UnityEngine;

public class EnemyBaseLocal : MonoBehaviour
{

    public enum EnemyMap { Higher, Lower, Same, Nullus };
    public EnemyMap enemyMap = EnemyMap.Nullus;
    public bool firstPass = false;
    public float maxFloatHeight = 50f;
    [HideInInspector]
    public NavMeshAgent agent;
    [SerializeField]
    private float walkRadius = 10;
    [SerializeField]
    public Vector3 target = Vector3.zero;
    //GhostBodyAdjustments ghostPosition;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        //ghostPosition = GetComponentInChildren<GhostBodyAdjustments>();
    }

    // Update is called once per frame on the server
    void FixedUpdate()
    {
            //Currently gets a random point on the navmesh
            if (!agent.hasPath)
            {
                Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
                randomDirection += transform.position;
                NavMeshHit hit;
                NavMesh.SamplePosition(randomDirection, out hit, walkRadius, 1);
                Vector3 finalPosition = hit.position;
                float randY = Random.Range(0, maxFloatHeight);
                target = new Vector3(finalPosition.x, randY, finalPosition.z);
                SetPosition(target);
            }
    }

    //Set the target of the ai agent across the network
    public void SetPosition(Vector3 position)
    {
        target = position;
        // Vector3.Slerp(transform.position, position, );
        agent.SetDestination(target);
    }

    //Draw the agents path towards the target
    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(target, 0.5f);
            Gizmos.DrawLine(transform.position, agent.path.corners[0]);
            for (int i = 0; i < agent.path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(agent.path.corners[i], agent.path.corners[i + 1]);
            }
        }
    }
}
