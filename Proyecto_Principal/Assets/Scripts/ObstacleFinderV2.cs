
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Sensor;

// https://github.com/Unity-Technologies/ROS-TCP-Connector/blob/main/com.unity.robotics.ros-tcp-connector/Runtime/Messages/Sensor/msg/LaserScanMsg.cs
// https://github.com/Unity-Technologies/ROS-TCP-Connector/blob/main/com.unity.robotics.ros-tcp-connector/Runtime/Messages/Tf2/msg/TFMessageMsg.cs
public class ObstacleFinderV2 : MonoBehaviour
{
    // Parámetros físicos.
    public GameObject robot;
    private int disparos; // El número de disparos que efectuará el Lidar.
    private float[,] trigon; // Valores cartesianos unitarios de todos los disparos del Lidar.
    public GameObject marcaPrefab; // El prefab del punto que será generado para mostrar un obstáculo.
    public Gradient gradienteColor;
    // Para limitar la superposición de marcas.
    private HashSet<Vector3> posicionesMarcas = new HashSet<Vector3>(); // Almacén de posiciones de las marcas creadas.
    public float separacionMarcas = 0.1f; // Distancia mínima tolerada entre las marcas.

    // Gestión de los topics.
    public string topicName = "/scan";
    private LaserScanMsg lastLaserScanMsg; // Almacén de la información del mensage del Lidar al que se suscribe.
    [SerializeField] private ROSConnection ros;
    public float publishMessagePeriod = 0.1f; // Publicación del topic cada publishMessagePeriod segundos.
    private float timeElapsed; // Se usa para llevar un conteo del tiempo transcurrido.
    private Color[] markerColors; // Array de colores precalculado en base a la distancia.


    public ColorDistance colorChanger;


    void Awake()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<LaserScanMsg>(topicName, LIDARRobot);

        // Colores precalculados en el gradiente.
        markerColors = new Color[100]; // Por ejemplo un rango entre 0 y 100.
        for (int i = 0; i < markerColors.Length; i++)
        {
            float distNormal = i / 100f;
            markerColors[i] = gradienteColor.Evaluate(distNormal);
        }
    }


    void LateUpdate()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > publishMessagePeriod && lastLaserScanMsg != null)
        {
            float anguloRadianes = -robot.transform.rotation.eulerAngles.y * Mathf.Deg2Rad; // Paso a radianes.

            for (int i = 0; i < disparos; i++)
            {
                trigon[i, 0] = (float)Math.Cos(anguloRadianes);
                trigon[i, 1] = (float)Math.Sin(anguloRadianes);
                anguloRadianes += lastLaserScanMsg.angle_increment;
            }

            for (int i = 0; i < disparos; i++) // Cada uno de los disparos.
            {
                float dist = lastLaserScanMsg.ranges[i];
                if (!float.IsInfinity(dist)) // Se filtran las distancias que sean infinitas.
                    CreaMarca(dist, i);
            }
            timeElapsed = 0; // Reinicia el conteo hasta el siguiente tick.
        }
    }


    // Actualizar valor del Lidar
    void LIDARRobot(LaserScanMsg robotScan)
    {
        lastLaserScanMsg = robotScan;
        disparos = lastLaserScanMsg.ranges.Length;
        trigon = new float[disparos, 2];
    }


    // Instancia una nueva marca de obstáculo en el mapa.
    void CreaMarca(float dist, int rayo) // Genera una marca en el mapa en función de la distancia de choque y el rayo que le toque del array.
    {
        Vector3 posicion = robot.transform.position + new Vector3(trigon[rayo, 0] * dist, 0, trigon[rayo, 1] * dist); // Posición cartesiana con trigonometría y distancias.
        
        if (!hayMarcaCerca(posicion))
        {
            GameObject marca = Instantiate(marcaPrefab, posicion, Quaternion.identity);
            posicionesMarcas.Add(posicion); // Añadir la posición de la nueva marca a la lista de marcas.

            /*
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
            */

            colorChanger.AddInstantiatedObject(marca);
        }
    }


    bool hayMarcaCerca(Vector3 nuevaMarca)
    {
        // Revisará todos los macadores para comprobar que no estén demqasiado cerca.
        foreach (Vector3 otraMarca in posicionesMarcas)
        {
            if (Vector3.Distance(nuevaMarca, otraMarca) < separacionMarcas)
                return true; // Si encuentra un obstáculo lo indica y se sale.
        }
        return false; // Si no encontró ninguno lo indicará.
    }
}
