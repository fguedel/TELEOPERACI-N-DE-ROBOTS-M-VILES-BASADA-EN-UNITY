
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Nav;

public class PositionSubscriberOdom : MonoBehaviour
{
    // Parámetros físicos.
    public GameObject robot;
    private float rotacion;
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
                (float)lastOdometryMsg.pose.pose.position.z, // El orden de las coordenadas está invertido en Unity.
                (float)lastOdometryMsg.pose.pose.position.y);

            // Al recibir la orientación en angulares se pasa a cuaternión.
            rotacion -= (float)lastOdometryMsg.twist.twist.angular.z; // El orden de las coordenadas está invertido en Unity.
            Quaternion quatRotation = Quaternion.Euler(0, rotacion, 0);

            // Se establece el cuaternión en la orientación del robot.
            robot.transform.rotation = quatRotation;

            // Reinicia el conteo.
            timeElapsed = 0;
        }
    }

    // Actualizar valor de odometría.
    void MovimientoRobot(OdometryMsg robotOdom){lastOdometryMsg = robotOdom;}

    // https://github.com/Unity-Technologies/ROS-TCP-Connector/blob/main/com.unity.robotics.ros-tcp-connector/Runtime/Messages/Nav/msg/OdometryMsg.cs
}
