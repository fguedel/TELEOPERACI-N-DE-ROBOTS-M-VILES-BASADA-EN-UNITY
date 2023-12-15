
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Geometry;

// roslaunch ros_tcp_endpoint endpoint.launch tcp_ip:=10.217.7.146 tcp_port:=10000

// roslaunch turtlebot3_gazebo turtlebot3_empty_world.launch

public class XboxPublisherTwist : MonoBehaviour
{
    public float MITIGADOR = 0.5F;

    // Gestor de InputSystem.
    InputMaster input;
    Vector3 currentMov;
    bool movementPressed;
    private void OnEnable() {input.Robot0.Enable();}
    private void OnDisable() {input.Robot0.Disable();}

    // Parámetros físicos.
    float BURGER_MAX_LIN_VEL = 1F;//0.22F;
    float BURGER_MAX_ANG_VEL = 2.84F;

    // Gestión de los topics.
    public string topicName = "/cmd_vel";
    // https://github.com/Unity-Technologies/ROS-TCP-Connector/blob/main/com.unity.robotics.ros-tcp-connector/Runtime/Messages/Geometry/msg/TwistMsg.cs
    // Esta variable será publicada.
    private TwistMsg twistMessage;
    [SerializeField] ROSConnection ros;
    // Publish the robot's position and rotation every N seconds
    public float publishMessagePeriod = 0.1f;
    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;

    private void Awake()
    {
        input = new InputMaster();
        input.Robot0.Movimiento.performed += ctx => {
            currentMov = ctx.ReadValue<Vector3>();
            movementPressed = currentMov.x != 0 || currentMov.z != 0;
            };
    }

    private void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<TwistMsg>(topicName); // Use 'Twist' from RosMessageTypes.Geometry
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > publishMessagePeriod)
        {
            // Generación de los arrays de posición y orientación.
            Vector3Msg linearVel = new Vector3Msg
            {x = currentMov.z * BURGER_MAX_LIN_VEL * MITIGADOR, y = 0, z = 0};
            Debug.Log(linearVel);
            Vector3Msg angularVel = new Vector3Msg
            {x = 0, y = 0, z = -currentMov.x * BURGER_MAX_ANG_VEL * MITIGADOR};
            // Update the Twist message with the new velocities
            twistMessage = new TwistMsg(linearVel, angularVel); // Use 'Twist' from RosMessageTypes.Geometry

            // Publish the Twist message to the /cmd_vel to server_endpoint.py running in ROS
            ros.Publish(topicName, twistMessage);

            // Reinicia el conteo.
            timeElapsed = 0;
        }
        
    }
}
