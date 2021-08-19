using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    public float rotateSpeed;
    public float moveAmount;
    public float moveTime;

    MeshRenderer mr;

    public Vector3 startPos;

    bool rainbow = false;
    public Color getableColor;
    public Color[] colors;

    /// <summary>
    /// Color lerp
    /// </summary>
    public int currentIndex = 0;
    private int nextIndex;

    public float changeColourTime = 2.0f;

    private float lastChange = 0.0f;
    private float timer = 0.0f;

    private void Awake()
    {
        
        mr = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        startPos = transform.position;
        //transform.LookAt(Camera.main.transform);

        colors = SimpleController.instance.AllColors.ToArray();
        nextIndex = (currentIndex + 1) % colors.Length;
    }

    private void Update()
    {
        if (rainbow)
            RainbowColor();
        //transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
    }

    public void Respawn()
    {
        //if (Random.Range(1, 8) == 1)
        //{
            rainbow = true;
        //}
        //else
        //{
        //    rainbow = false;
        //    getableColor = SimpleController.instance.availableColors[Random.Range(0, SimpleController.instance.availableColors.Count)];
        //    mr.material.color = getableColor;
        //}
    }

    public void MoveDown()
    {
        Hashtable temp = new Hashtable();
        temp.Add("position", transform.position + -Vector3.up * moveAmount);
        temp.Add("time", moveTime);
        iTween.MoveTo(gameObject, temp);
    }

    private void OnTriggerEnter(Collider other)
    {
        BallScript bs = other.gameObject.GetComponent<BallScript>();

        if (rainbow)
            bs.ChangeColor(true);
        else
            bs.ChangeColor(true, getableColor);

        iTween.Stop(gameObject);
        transform.position = startPos;
        gameObject.SetActive(false);
        GameManager.instance.specialEnabled = false;
    }

    void RainbowColor()
    {
        timer += Time.deltaTime;

        if (timer > changeColourTime)
        {
            currentIndex = (currentIndex + 1) % colors.Length;
            nextIndex = (currentIndex + 1) % colors.Length;
            timer = 0.0f;

        }
        mr.material.color = Color.Lerp(colors[currentIndex], colors[nextIndex], timer / changeColourTime);
    }
}
