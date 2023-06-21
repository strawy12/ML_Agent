using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CheetahAgent : Agent
{
    private Rigidbody rigidBody;
    private Transform targetTransform;

    public float moveSpeed = 1.5f;
    public float turnSpeed = 200f;

    public override void Initialize()
    {
        rigidBody = GetComponent<Rigidbody>();
        targetTransform = transform.parent.Find("Apple");
    }

    public override void OnEpisodeBegin()
    {
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;

        transform.localPosition = new Vector3(Random.Range(-11.5f, 11.5f), 0.05f, Random.Range(-11.5f, 11.5f));
        targetTransform.localPosition = new Vector3(Random.Range(-11.5f, 11.5f), 0.55f, Random.Range(-11.5f, 11.5f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 총 8개 관측
        sensor.AddObservation(targetTransform.localPosition);
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rigidBody.velocity.x);
        sensor.AddObservation(rigidBody.velocity.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var action = actions.DiscreteActions;

        Vector3 dir = Vector3.zero;
        Vector3 rot = Vector3.zero;

        // Branch 0 : 정지 / 전진 / 후진
        switch (action[0])
        {
            case 1: dir = transform.forward; break;
            case 2: dir = -transform.forward; break;
        }

        // Branch 1 : 정지 / 좌회전 / 우회전
        switch (action[1])
        {
            case 1: rot = -transform.up; break;
            case 2: rot = transform.up; break;
        }

        transform.Rotate(rot, Time.fixedDeltaTime * turnSpeed);
        rigidBody.AddForce(dir * moveSpeed, ForceMode.VelocityChange);

        // 마이너스 페널티를 적용
        AddReward(-1 / (float)MaxStep);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var action = actionsOut.DiscreteActions;
        actionsOut.Clear();

        // Branch 0 - 이동로직에 쓸 키 맵핑
        // Branch 0의 Size : 3
        //  정지    /   전진    /   후진
        //  Non     /   W       /   S
        //  0       /   1       /   2
        if (Input.GetKey(KeyCode.W))
        {
            action[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            action[0] = 2;
        }

        // Branch 1 - 회전로직에 쓸 키 맵핑
        // Branch 1의 Size : 3
        //  정지    /   좌회전  /   우회전
        //  Non     /   A       /   D
        //  0       /   1       /   2
        if (Input.GetKey(KeyCode.A))
        {
            action[1] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            action[1] = 2;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            SetReward(-1.0f);
            EndEpisode();
        }

        if (collision.collider.CompareTag("Apple"))
        {
            SetReward(1.0f);
            EndEpisode();
        }
    }
}
