
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Sensor;

public class PruebaScan : MonoBehaviour
{
    // Parámetros físicos.
    public GameObject robot;
    // Gestión de los topics.
    public string topicName = "/scan";
    // Almacén de la información del mensage de la transformada al que se suscribe.
    private LaserScanMsg lastLaserScanMsg;
    [SerializeField] private ROSConnection ros;
    // Publish the robot's position and rotation every N seconds
    public float publishMessagePeriod = 0.1f;
    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<LaserScanMsg>(topicName, LIDARRobot);
    }

    // Actualizar la posición del cubo.
    void LateUpdate()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > publishMessagePeriod && lastLaserScanMsg != null)
        {
            for (int i = 0; i < 60; i++) // disparos
            {
                float dist = lastLaserScanMsg.ranges[i];
                Debug.Log(dist);
            }
            // Reinicia el conteo.
            timeElapsed = 0;
        }
    }

    // Actualizar valor de odometría.
    void LIDARRobot(LaserScanMsg robotScan) {lastLaserScanMsg = robotScan;}
}
