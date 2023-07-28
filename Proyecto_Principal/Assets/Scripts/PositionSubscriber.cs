
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Nav;

// roslaunch ros_tcp_endpoint endpoint.launch tcp_ip:=192.168.1.34 tcp_port:=10000

public class PositionSubscriber : MonoBehaviour
{
    // Parámetros físicos.
    public GameObject robot;
    // Gestión de los topics.
    public string topicName = "/odom";
    // Almacén de la información del mensage de odometría al que se suscribe.
    private OdometryMsg lastOdometryMsg;
    [SerializeField] private ROSConnection ros;
    // Publish the robot's position and rotation every N seconds
    public float publishMessagePeriod = 0.1f;
    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<OdometryMsg>(topicName, MovimientoRobot);
    }

    // Actualizar la posición del cubo.
    void LateUpdate()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > publishMessagePeriod && lastOdometryMsg != null)
        {
            // Al recibir la pose se establece en la del robot.
            robot.transform.position = new Vector3(
                (float)lastOdometryMsg.pose.pose.position.x,
                (float)lastOdometryMsg.pose.pose.position.y,
                (float)lastOdometryMsg.pose.pose.position.z);

            // Al recibir la orientación en angulares se pasa a cuaternión.
            Quaternion quatRotation = Quaternion.Euler(
                (float)lastOdometryMsg.twist.twist.angular.x, 
                (float)lastOdometryMsg.twist.twist.angular.y, 
                (float)lastOdometryMsg.twist.twist.angular.z);

            // Se establece el cuaternión en la orientación del robot.
            robot.transform.rotation = new Quaternion(
                quatRotation.x,
                quatRotation.y,
                quatRotation.z,
                quatRotation.w);

            // Reinicia el conteo.
            timeElapsed = 0;
        }
    }

    // Actualizar valor de odometría.
    void MovimientoRobot(OdometryMsg robotOdom){lastOdometryMsg = robotOdom;}

    // https://github.com/Unity-Technologies/ROS-TCP-Connector/blob/main/com.unity.robotics.ros-tcp-connector/Runtime/Messages/Nav/msg/OdometryMsg.cs
}
