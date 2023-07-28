
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;
using System;

//using RosMessageTypes.Standard;

// roslaunch ros_tcp_endpoint endpoint.launch tcp_ip:=192.168.1.34 tcp_port:=10000

public class XboxPublisherPosition : MonoBehaviour
{
    // Gestor de InputSystem.
    InputMaster input;
    Vector3 currentMov = Vector3.zero;
    bool movementPressed;
    private void OnEnable() {input.Robot0.Enable();}
    private void OnDisable() {input.Robot0.Disable();}

    // Parámetros físicos.
    public GameObject robot;
    float rotacion;
    float BURGER_MAX_LIN_VEL = 0.22F;
    float BURGER_MAX_ANG_VEL = 2.84F;
    //float LIN_VEL_STEP_SIZE = 0.01F;
    //float ANG_VEL_STEP_SIZE = 0.1F;

    // Gestión de los topics.
    ROSConnection ros;
    public string topicName = "tf";
    // Publish the cube's position and rotation every N seconds
    // Se elige este stepsize pues es el más lento del robot
    public float publishDelay = 0.1F;
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
        ros.RegisterPublisher<PosRotMsg>(topicName);
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > publishDelay)
        {
            PosRotMsg robotPos = Desplaza();
            // Finally send the message to server_endpoint.py running in ROS
            ros.Publish(topicName, robotPos);
            timeElapsed = 0;
        }
    }

    // La siguiente función tiene como objetivo modificar un array para actualizar el valor de la posición de un objeto (el robot).
    private PosRotMsg Desplaza()
    {
        PosRotMsg PosRot = new PosRotMsg(
            robot.transform.position.x,
            robot.transform.position.y,
            robot.transform.position.z,
            robot.transform.rotation.x,
            robot.transform.rotation.y,
            robot.transform.rotation.z,
            robot.transform.rotation.w
            );

        // Rotar el objeto basándose en currentMov.x. Regla de tres del giro.
        rotacion += currentMov.x * BURGER_MAX_ANG_VEL;
        Quaternion quatRotacion = Quaternion.Euler(0f, rotacion, 0f);
        PosRot.rot_x = quatRotacion.x;
        PosRot.rot_y = quatRotacion.y;
        PosRot.rot_z = quatRotacion.z;
        PosRot.rot_w = quatRotacion.w;

        // Avance en función de operación trigonométrica. Regla de tres del avance.
        float desplazamieto = currentMov.z * BURGER_MAX_LIN_VEL;
        float angulo = Mathf.Deg2Rad * rotacion; // Paso rotacion->radianes
        PosRot.pos_x += Mathf.Sin(angulo) * desplazamieto;
        PosRot.pos_z += Mathf.Cos(angulo) * desplazamieto;

        return PosRot;
    }
}
