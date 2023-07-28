
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;

// roslaunch ros_tcp_endpoint endpoint.launch tcp_ip:=192.168.1.34 tcp_port:=10000

public class PositionSubscriberQuaternion : MonoBehaviour
{
    // Parámetros físicos.
    public GameObject robot;
    // Gestión de los topics.
    public string topicName = "/odometry";
    // Publish the cube's position and rotation every N seconds
    // Se elige este stepsize pues es el más lento del robot
    public float publishDelay = 0.1F;
    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;

    void Start()
    {
        //ROSConnection.GetOrCreateInstance().Subscribe<PosRotMsg>("pos_rot", MovimientoRobot); // posiblemente topic tf
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > publishDelay)
        {
            ROSConnection.GetOrCreateInstance().Subscribe<PosRotMsg>(topicName, MovimientoRobot); // posiblemente topic tf
            timeElapsed = 0;
        }
    }

    void MovimientoRobot(PosRotMsg robotPosRot)
    {
        robot.transform.position = new Vector3(robotPosRot.pos_x, robotPosRot.pos_y, robotPosRot.pos_z);
        robot.transform.rotation = new Quaternion(robotPosRot.rot_x, robotPosRot.rot_y, robotPosRot.rot_z, robotPosRot.rot_w);
    }
}
