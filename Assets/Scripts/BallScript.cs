using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallScript : MonoBehaviour
{
    public List<Color> possibleColorsM;
    private MeshRenderer mr;
    private Rigidbody rb;
    public float bounce;
    public bool isSecond = false;

    public Vector3 defaultGrav;


    //moving 
    public Color getableColor;
    public Color[] possibleColors;

    public Color[] availableColors;
    public Transform[] startNormals;
    public List<Vector3> availableNormals;

    public int repeatCount = 0;

    public UnityEngine.UI.Image countDownImage;
    private float imageTimer = 0.0f;

    #region lerp var
    public Vector3 startPos;
    public Vector3 endPos;


    public float tweenSpeed;
    public float tweenTimer = 0;
    #endregion

    public float gravityMultiplier;

    bool falling = false;
    bool bouncing = false;

    Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        defaultGrav = Physics.gravity;
        possibleColorsM = new List<Color>();
        if (MenuAndSceneManager.CheckSceneIndex() != 1)
        {
            mr = gameObject.GetComponent<MeshRenderer>();
            foreach (GameObject go in MainMenuCubeController.instance.planes)
            {
                possibleColorsM.Add(go.GetComponent<MeshRenderer>().material.color);
            }
        }
    }

    private void Update()
    {
        if (isSecond)
        {
            if (!falling)
            {
                if (rb.velocity.y <= 0)
                {
                    Pause();
                }
            }

            if (rb.velocity.y < 0)
                falling = true;
            else
                falling = false;
        }

        if (countDownImage == null)
            return;
        if(countDownImage.enabled)
        {
            imageTimer += Time.deltaTime;
            countDownImage.fillAmount = imageTimer / 3.0f;
        }
    }

    private void LateUpdate()
    {
        if(bouncing)
        {
            rb.velocity = new Vector3(0, Mathf.Sqrt(-2.0f * (Physics.gravity.y) * bounce), 0);
            bouncing = false;
        }
    }

    public void Bounce()
    {
        bouncing = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        bouncing = true;
        //rb.velocity = new Vector3(0, Mathf.Sqrt(-2.0f * (Physics.gravity.y) * bounce), 0);

        if (MenuAndSceneManager.CheckSceneIndex() == 0)
            AudioManager.instance.PlayBounce();

        if (GameManager.instance == null)
        {
            ChangeColorInMenu();
        }
        else
        {
            GameManager.instance.CheckColor(this.gameObject);
            OnCollision();
            SimpleController.instance.SwapMaterial();
        }
    }

    void ChangeColorInMenu()
    {
        mr.material.color = possibleColorsM[Random.Range(0, possibleColorsM.Count)];
    }

    #region Moving over
    public void OnCollision()
    {
        ChangeColor(false);
    }

    public void ChangeColor(bool needsNew, Color newCol = default(Color), bool anyColor = false)
    {
        if(newCol != default)
        {
            getableColor = newCol;
            gameObject.GetComponent<MeshRenderer>().material.color = getableColor; 
            return;
        }


        var colors = /*GetAvailableColor()*/SimpleController.instance.availableColors;

        if (anyColor)
            colors = SimpleController.instance.AllColors;
        //if(GameManager.instance.colorChanger.activeInHierarchy)
        //{
        //    colors = new List<Color>(GameManager.instance.tempGetableColors);
        //}
        var newColor = colors[Random.Range(0, colors.Count)];

        if (needsNew)
        {
            while (newColor == getableColor)
            {
                newColor = colors[Random.Range(0, colors.Count)];
            }
        }
        else
        {
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
                                {
                                    Debug.Log("reached max");
                                    break;
                                }
                            }

                            repeatCount = 0;
                        }
                    }
                    else
                        repeatCount = 0;
                }
            }
        }
        getableColor = newColor;
        gameObject.GetComponent<MeshRenderer>().material.color = getableColor;
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
    #endregion

    public void Pause()
    {
        StartCoroutine(Stall());
    }

    public IEnumerator Stall()
    {
        rb.useGravity = false;
        yield return new WaitForSecondsRealtime(1);
        rb.useGravity = true;
    }

    public void BeSuspenseful(int time_seconds)
    {
        countDownImage.enabled = true;
        StartCoroutine(BeSuspensefulCR(time_seconds));
    }

    public IEnumerator BeSuspensefulCR(int time_seconds)
    {
        rb.isKinematic = true;
        yield return new WaitForSecondsRealtime(time_seconds);
        countDownImage.enabled = false;
        imageTimer = 0;
        rb.isKinematic = false;

        rb.velocity = new Vector3(0, -Mathf.Sqrt(-2.0f * (Physics.gravity.y) * bounce), 0);
        yield break;
    }

    private void OnDisable()
    {
        isSecond = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        //ChangeColor(true);
        //other.gameObject.SetActive(false);
    }

    
}
