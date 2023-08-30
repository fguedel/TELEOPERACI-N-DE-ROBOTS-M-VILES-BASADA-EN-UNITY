
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorDistance : MonoBehaviour
{
    public float colorChangeInterval = 1.0f; // Interval at which the color changes
    public Color[] availableColors; // List of colors to choose from

    private float timer = 0.0f;
    private int currentColorIndex = 0;
    private List<GameObject> instantiatedObjects = new List<GameObject>();

    private void Start()
    {
        // Initialize the timer
        timer = colorChangeInterval;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= colorChangeInterval)
        {
            ChangeColors();
            timer = 0.0f;
        }
    }

    public void AddInstantiatedObject(GameObject obj)
    {
        instantiatedObjects.Add(obj);
    }

    private void ChangeColors()
    {
        currentColorIndex = (currentColorIndex + 1) % availableColors.Length;

        foreach (GameObject obj in instantiatedObjects)
        {
            Renderer objRenderer = obj.GetComponent<Renderer>();
            if (objRenderer != null)
            {
                objRenderer.material.color = availableColors[currentColorIndex];
            }
        }
    }
}
