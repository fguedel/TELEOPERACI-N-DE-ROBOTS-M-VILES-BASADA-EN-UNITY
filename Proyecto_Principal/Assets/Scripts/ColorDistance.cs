
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorDistance : MonoBehaviour
{
    public GameObject robot;
    private List<GameObject> marcasCreadas = new List<GameObject>();
    public float changePeriod = 0.1f; // Publicación del topic cada publishMessagePeriod segundos.
    private float timeElapsed; // Se usa para llevar un conteo del tiempo transcurrido.
    private float rangoMax; // Rango entre el alcance del Lidar y el mínimo detectado de distancia (aportado por proceso padre).
    private Color[] rangoColoresMarca; // Array de colores precalculado en base a la distancia.
    public Gradient gradienteColor;


    private void Awake()
    {
        float distNormal;
        rangoColoresMarca = new Color[100]; // Por ejemplo un rango entre 0 y 100.
        for (int i = 0; i < rangoColoresMarca.Length; i++)
        {
            distNormal = i / 100f;
            rangoColoresMarca[i] = gradienteColor.Evaluate(distNormal); // Colores precalculados en el gradiente.
        }
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > changePeriod)
        {
            ChangeColors();
            timeElapsed = 0; // Reinicia el conteo hasta el siguiente tick.
        }
    }


    // Método que permite actualizar las variables físicas de distancia del Lidar y construir el array de gradientes de color.
    public void constructor(float max, float min) {rangoMax = max - min;}


    // Método que permite a componentes externos rellenar de objetos marca la lista de marcas.
    public void nuevaMarcaCreada(GameObject marcaNueva) {marcasCreadas.Add(marcaNueva);}


    private void ChangeColors()
    {
        foreach (GameObject marca in marcasCreadas)
        {
            float dist = Vector3.Distance(robot.transform.position, marca.transform.position);
            // Colorear la marca del color correspondiente:
            // Calcula la distancia y la normaliza entre 0 y 1.
            float distNormal = Mathf.Clamp01(dist / rangoMax);
            // En función de la distancia indica el índice del tono de color que debe buscar en el array.
            int colorIndex = Mathf.FloorToInt(distNormal * (rangoColoresMarca.Length - 1));
            Renderer renderer;
            if (marca.TryGetComponent(out renderer))
            {
                renderer.material.color = rangoColoresMarca[colorIndex]; // Establece el tono que le corresponde según el índice.
            }
        }
    }
}
