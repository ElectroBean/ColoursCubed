using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspenseBall : MonoBehaviour
{

    public int currentIndex = 0;
    private int nextIndex;

    public float changeColourTime = 2.0f;

    private float lastChange = 0.0f;
    private float timer = 0.0f;
    MeshRenderer mr;
    public Color[] colors;

    float hitCounter = 0;

    private void Awake()
    {
        mr = GetComponent<MeshRenderer>();
        
    }

    // Start is called before the first frame update
    void Start()
    {
        colors = SimpleController.instance.AllColors.ToArray();
        nextIndex = (currentIndex + 1) % colors.Length;
    }

    // Update is called once per frame
    void Update()
    {
        RainbowColor();
    }

    private void OnTriggerStay(Collider other)
    {
        hitCounter += Time.deltaTime;
        if (hitCounter > 0.5f)
            stoof(other);
    }

    private void stoof(Collider other)
    {
        BallScript bs = other.gameObject.GetComponent<BallScript>();
        bs.ChangeColor(true, default(Color), true);
        bs.BeSuspenseful(3);
        gameObject.SetActive(false);
        hitCounter = 0;
        GameManager.instance.specialEnabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {

    }

    public void Respawn()
    {

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
