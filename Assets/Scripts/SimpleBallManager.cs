using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBallManager : MonoBehaviour
{
    public static SimpleBallManager instance;

    public GameObject ball;
    public Color getableColor;
    public Color[] possibleColors;

    public Color[] availableColors;
    public Transform[] startNormals;
    public List<Vector3> availableNormals;

    private int repeatCount = 0;
    #region lerp var
    public Vector3 startPos;
    public Vector3 endPos;


    public float tweenSpeed;
    public float tweenTimer = 0;
    #endregion

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        for (int i = 0; i < startNormals.Length; i++)
        {
            availableNormals.Add(startNormals[i].up);
        }
    }

    private void Update()
    {

    }

    public void OnCollision()
    {
       
        ChangeColor();
    }

    void ChangeColor()
    {
        var colors = /*GetAvailableColor()*/SimpleController.instance.availableColors;
        var newColor = colors[Random.Range(0, colors.Count)];

        if (GameManager.instance)
        {
            if (!GameManager.instance.sandbox)
            {
                if (newColor == getableColor)
                {
                    repeatCount++;
                    if (repeatCount > 1)
                    {
                        int counter = 0;
                        while (newColor.Equals(getableColor))
                        {
                            newColor = colors[Random.Range(0, colors.Count)];
                            counter++;
                            if (counter > 1000)
                                break;
                        }

                        repeatCount = 0;
                    }
                }
                else
                    repeatCount = 0;
            }
        }
        getableColor = newColor;
        ball.GetComponent<MeshRenderer>().material.color = getableColor;
    }

    List<Color> GetAvailableColor()
    {
        List<Color> colors = new List<Color>();
        foreach (GameObject go in SimpleController.instance.planes)
        {
            var tempNorm = go.transform.up;
            tempNorm.y = 0;

            if (go.transform.up == Vector3.up)
            {
                colors.Add(go.GetComponent<MeshRenderer>().material.color);
                continue;
            }

            foreach (Vector3 normal in availableNormals)
            {
                if (normal == tempNorm)
                {
                    colors.Add(go.GetComponent<MeshRenderer>().material.color);
                }
            }
        }

        return colors;
    }

    private void OnTriggerEnter(Collider other)
    {
        ChangeColor();
        other.gameObject.SetActive(false);
    }
}
