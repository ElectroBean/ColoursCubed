using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCubeController : MonoBehaviour
{
    public static MainMenuCubeController instance;
    public GameObject[] planes;
    public GameObject cubeHolder;
    public GameObject colorCube;
    public Vector3 rightOffset;
    public Vector3 forwardOffset;

    public float tweenSpeed;

    Vector2 lastDirection = Vector2.zero;

    private void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
        RotateRandomDirection();
    }

    void WaitForNext()
    {
        var vec = colorCube.transform.eulerAngles;
        vec.x = Mathf.Round(vec.x / 45) * 45;
        vec.y = Mathf.Round(vec.y / 45) * 45;
        vec.z = Mathf.Round(vec.z / 45) * 45;
        colorCube.transform.eulerAngles = vec;
        StartCoroutine(DoNext());
    }

    IEnumerator DoNext()
    {
        yield return new WaitForSecondsRealtime(2.0f);
        RotateRandomDirection();
    }

    void RotateRandomDirection()
    {
        Hashtable itween = new Hashtable();
        itween.Add("time", tweenSpeed);
        itween.Add("space", "world");
        itween.Add("oncomplete", "WaitForNext");
        itween.Add("oncompletetarget", this.gameObject);

        var direction = new Vector2(Random.Range(-1, 2), Random.Range(-1, 2));
        while(direction == Vector2.zero || direction == Vector2.right || direction == -Vector2.right || direction == Vector2.up || direction == -Vector2.up || direction == lastDirection)
        {
            direction = new Vector2(Random.Range(-1, 2), Random.Range(-1, 2));
        }

        //right
        //if (direction == new Vector2(1, 0))
        //{
        //    itween.Add("amount", cubeHolder.transform.up.normalized * -0.25f);
        //}
        ////left
        //else if (direction == new Vector2(-1, 0))
        //{
        //    itween.Add("amount", cubeHolder.transform.up.normalized * 0.25f);
        //}
        //up right
        if (direction == new Vector2(1, 1))
        {
            itween.Add("amount", (cubeHolder.transform.right.normalized + (rightOffset) * Mathf.PI / 180) * 0.25f);
            lastDirection = direction;
        }
        //up left
        else if (direction == new Vector2(-1, 1))
        {
            itween.Add("amount", (cubeHolder.transform.forward.normalized + (forwardOffset) * Mathf.PI / 180) * 0.25f);
            lastDirection = direction;
        }
        //down right
        else if (direction == new Vector2(1, -1))
        {
            itween.Add("amount", (cubeHolder.transform.right.normalized - (rightOffset) * Mathf.PI / 180) * -0.25f);
            lastDirection = direction;
        }
        //down left
        else if (direction == new Vector2(-1, -1))
        {
            itween.Add("amount", (cubeHolder.transform.forward.normalized - (forwardOffset) * Mathf.PI / 180) * -0.25f);
            lastDirection = direction;
        }

        Debug.Log("new rotation");
        iTween.RotateBy(colorCube, itween);
        AudioManager.instance.PlayRotate();
    }
}
