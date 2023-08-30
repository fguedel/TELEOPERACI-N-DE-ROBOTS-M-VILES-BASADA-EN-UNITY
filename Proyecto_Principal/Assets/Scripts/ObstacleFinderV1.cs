
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Sensor;

// https://github.com/Unity-Technologies/ROS-TCP-Connector/blob/main/com.unity.robotics.ros-tcp-connector/Runtime/Messages/Sensor/msg/LaserScanMsg.cs
public class ObstacleFinderV1 : MonoBehaviour
{
    // Parámetros físicos.
    private int disparos; // El número de disparos que efectuará el Lidar.
    private float[,] trigon; // Valores cartesianos unitarios de todos los disparos del Lidar.
    public GameObject marcaPrefab; // El prefab del punto que será generado para mostrar un obstáculo.
    public Gradient gradienteColor;
    // Para limitar la superposición de marcas.
    public string topicName = "/scan";
    private LaserScanMsg lastLaserScanMsg; // Almacén de la información del mensage al que se suscribe.
    [SerializeField] private ROSConnection ros;
    // Publish the robot's position and rotation every N seconds
    public float publishMessagePeriod = 0.1f;
    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;
    private Color[] markerColors; // Array de colores precalculado en base a la distancia.


    void Awake()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<LaserScanMsg>(topicName, LIDARRobot);

        // Pre-calculate marker colors based on gradient
        markerColors = new Color[100]; // Assuming a range_max of 100
        for (int i = 0; i < markerColors.Length; i++)
        {
            float distNormal = i / 100f;
            markerColors[i] = gradienteColor.Evaluate(distNormal);
        }
    }

    // Actualizar la posición del cubo.
    void LateUpdate()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > publishMessagePeriod && lastLaserScanMsg != null)
        {
            for (int i = 0; i < disparos; i++) // Cada uno de los disparos.
            {
                float dist = lastLaserScanMsg.ranges[i];
                // Para reducir carga de trabajo para las gafas, se representarán solo los índices impares en caso de estar lejos.
                // Además, se filtran las distancias que sean infinitas.
                if ((i % 2 == 0 || dist > lastLaserScanMsg.range_max/2) && !float.IsInfinity(dist))
                {
                    CreaMarca(dist, i);
                }
            }
            timeElapsed = 0; // Reinicia el conteo hasta el siguiente tick.
        }
    }


    void LIDARRobot(LaserScanMsg robotScan) // Actualizar valor del Lidar.
    {
        lastLaserScanMsg = robotScan;
        disparos = lastLaserScanMsg.ranges.Length;
        trigon = new float[disparos, 2];
        float anguloRadianes = 0;
        for (int i = 0; i < disparos; i++)
        {
            trigon[i, 0] = (float)Math.Cos(anguloRadianes);
            trigon[i, 1] = (float)Math.Sin(anguloRadianes);
            anguloRadianes += lastLaserScanMsg.angle_increment;
        }
    }


    void CreaMarca(float dist, int rayo) // Genera una marca en el mapa en función de la distancia de choque y el rayo que le toque del array.
    {
        Vector3 posicion = new Vector3(-trigon[rayo, 1] * dist, 0, trigon[rayo, 0] * dist); // Posición cartesiana con trigonometría y distancias.
        GameObject marca = Instantiate(marcaPrefab, posicion, Quaternion.identity);

        // Colorear la marca del color correspondiente:
        // Calcula la distancia y la normaliza entre 0 y 1.
        float distNormal = Mathf.Clamp01(dist / (lastLaserScanMsg.range_max - lastLaserScanMsg.range_min));
        // En función de la distancia indica el índice del tono de color que debe buscar en el array.
        int colorIndex = Mathf.FloorToInt(distNormal * (markerColors.Length - 1));
        Renderer renderer;
        if (marca.TryGetComponent(out renderer))
        {
            renderer.material.color = markerColors[colorIndex]; // Establece el tono que le corresponde según el índice.
        }
    }
}
