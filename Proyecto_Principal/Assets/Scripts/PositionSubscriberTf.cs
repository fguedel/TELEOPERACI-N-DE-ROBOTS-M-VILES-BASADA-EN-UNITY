
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Tf2;

public class PositionSubscriberTf : MonoBehaviour
{
    // Parámetros físicos.
    public GameObject robot;
    // Gestión de los topics.
    public string topicName = "/tf";
    // Almacén de la información del mensage de la transformada al que se suscribe.
    private TFMessageMsg lastTransformMsg;
    [SerializeField] private ROSConnection ros;
    // Publish the robot's position and rotation every N seconds
    public float publishMessagePeriod = 0.1f;
    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<TFMessageMsg>(topicName, MovimientoRobot);
    }

    // Actualizar la posición del cubo.
    void LateUpdate()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > publishMessagePeriod && lastTransformMsg != null && lastTransformMsg.transforms.Length > 0)
        {
            // Se ha de recoger la última transformada del array en el topic.
            var transformStamped = lastTransformMsg.transforms[0].transform;

            // Al recibir la pose en formato Vector3Msg, se establece en la del robot en forma de Vector3.
            robot.transform.position = new Vector3(
                (float)transformStamped.translation.x,
                (float)transformStamped.translation.z,
                (float)transformStamped.translation.y);
            
            // Al recibir la orientación en QuaternionMsg se establece en la orientación del robot como Quaternion.
            robot.transform.rotation = new Quaternion(
                (float)transformStamped.rotation.x,
                -(float)transformStamped.rotation.z,
                (float)transformStamped.rotation.y,
                (float)transformStamped.rotation.w);

            // Reinicia el conteo.
            timeElapsed = 0;
        }
    }

    // Actualizar valor de odometría.
    void MovimientoRobot(TFMessageMsg robotTransform){lastTransformMsg = robotTransform;}

    // https://github.com/Unity-Technologies/ROS-TCP-Connector/blob/main/com.unity.robotics.ros-tcp-connector/Runtime/Messages/Nav/msg/OdometryMsg.cs
}
