
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Geometry;
using RosMessageTypes.Sensor;
using RosMessageTypes.Tf2;

// https://github.com/Unity-Technologies/ROS-TCP-Connector/blob/main/com.unity.robotics.ros-tcp-connector/Runtime/Messages/Sensor/msg/LaserScanMsg.cs
// https://github.com/Unity-Technologies/ROS-TCP-Connector/blob/main/com.unity.robotics.ros-tcp-connector/Runtime/Messages/Tf2/msg/TFMessageMsg.cs
public class ObstacleFinderV3 : MonoBehaviour
{
    // Parámetros físicos.
    public GameObject robot;
    private int disparos; // El número de disparos que efectuará el Lidar.
    private float[,] trigon; // Valores cartesianos unitarios de todos los disparos del Lidar.
    public GameObject marcaPrefab; // El prefab del punto que será generado para mostrar un obstáculo.
    // Para limitar la superposición de marcas.
    private HashSet<Vector3> posicionesMarcas = new HashSet<Vector3>(); // Almacén de posiciones de las marcas creadas.
    public float separacionMarcas = 0.1f; // Distancia mínima tolerada entre las marcas.

    // Gestión de los topics.
    public string topicName = "/scan";
    public string topicNameTransform = "/tf";
    private LaserScanMsg lastLaserScanMsg; // Almacén de la información del mensage del Lidar al que se suscribe.
    private TFMessageMsg lastTransformMsg;
    [SerializeField] private ROSConnection ros;
    public float publishMessagePeriod = 0.1f; // Publicación del topic cada publishMessagePeriod segundos.
    private float timeElapsed; // Se usa para llevar un conteo del tiempo transcurrido.

    // Transmisión de información al manejador del cambio de color.
    public ColorDistance colorChanger;

    
    void Awake()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<LaserScanMsg>(topicName, LIDARRobot);
        ros.Subscribe<TFMessageMsg>(topicNameTransform, MovimientoRobot);
    }

    void LateUpdate()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > publishMessagePeriod && lastLaserScanMsg != null)
        {
            float[] disparos = lastLaserScanMsg.ranges; // Array de distancias producido por le lidar.
            float incrementoAngular = lastLaserScanMsg.angle_increment; // Ángulo entre disparos del Lidar.
            // El ángulo de partida es la orientación del robot.
            var rosQuaternion = lastTransformMsg.transforms[0].transform;

            Quaternion unityQuaternion = new Quaternion(
                (float)rosQuaternion.rotation.x,
                (float)rosQuaternion.rotation.z,
                (float)rosQuaternion.rotation.y,
                (float)rosQuaternion.rotation.w);
                
            float anguloRadianes = unityQuaternion.eulerAngles.y * Mathf.Deg2Rad; // Paso a radianes.

            foreach (float distancia in disparos) // Para cada uno de los disparos...
            {
                if (!float.IsInfinity(distancia)) // Se filtran las distancias que sean infinitas.
                {
                    float xUnit = (float)Math.Cos(anguloRadianes);
                    float yUnit = (float)Math.Sin(anguloRadianes);
                    Vector3 posicionMarca = robot.transform.position + new Vector3(xUnit * distancia, 0, yUnit * distancia); // Posición cartesiana.
                    CreaMarca(posicionMarca);
                }
                anguloRadianes += incrementoAngular;
            }
            timeElapsed = 0; // Reinicia el conteo hasta el siguiente tick.
        }
    }


    // Actualizar valor del Lidar
    void LIDARRobot(LaserScanMsg robotScan)
    {
        lastLaserScanMsg = robotScan;
        colorChanger.constructor(lastLaserScanMsg.range_max, lastLaserScanMsg.range_min); // Indica al coloreador de marcas las distancias extremas.
    }


    // Actualizar valor de transformada.
    void MovimientoRobot(TFMessageMsg robotTransform){lastTransformMsg = robotTransform;}


    // Instancia una nueva marca de obstáculo en el mapa.
    void CreaMarca(Vector3 posicion) // Genera una marca en el mapa en función de la distancia de choque y el rayo que le toque del array.
    {
        if (!hayMarcaCerca(posicion)) // No pondrá una nueva marca en caso de haber otra cerca.
        {
            GameObject marca = Instantiate(marcaPrefab, posicion, Quaternion.identity);
            posicionesMarcas.Add(posicion); // Añadir la posición de la nueva marca a la lista de marcas.
            colorChanger.nuevaMarcaCreada(marca); // Indica al coloreador de marcas una nueva marca creada.
        }
    }


    // Comprueba que no haya otras marcas cerca de la marca indicada.
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
