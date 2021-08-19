using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleController : MonoBehaviour
{
    public static SimpleController instance;
    public GUIStyle style;

    #region Random public variables
    public GameObject colorCube;
    public Vector3 rightOffset;
    public Vector3 forwardOffset;

    public List<Color> availableColors;
    public Transform[] startNormals;
    public List<Vector3> availableNormals;
    List<Vector3> availableNormalPos;

    public Transform[] rayPoints;
    public LayerMask planeMask;
    public Color UpColor;

    Quaternion beginRotation;
    #endregion

    #region Input

    public bool ignoreInput = false;
    private bool pressed;
    private bool holdingDown;
    private bool released;

    private Vector2 startDrag;
    private Vector2 endDrag;

    /// <summary>
    /// MOBILE INPUT
    /// </summary>
    bool isTouching = false;
    Touch info;
    bool moved = false;
    #endregion

    #region Cube Behaviours

    private bool rotating;
    private Vector3 desiredRotation;
    private Vector3 startRotation;

    private Vector3 rotateAxis;
    private float rotateDegree;

    private float zero = 0;
    private float one = 1;
    private float lerpSpeed = 2.0f;

    private Vector3 startForward;
    private Vector3 startRight;
    private Vector3 startUp;

    public GameObject cubeHolder;

    public float tweenSpeed;

    #endregion

    #region GUI

    #endregion

    #region Colors
    public List<Color> AllColors;

    public Color getableColor;
    public GameObject[] planes;
    #endregion
  
    [System.Serializable]
    public class MaterialPair
    {
        public Material baseMat;
        public Material shaderMat;
    }

    public MaterialPair[] materialPairs;

    bool needsToCheckColors = false;

    private void Awake()
    {
        foreach (var pl in planes)
        {
            AllColors.Add(pl.GetComponent<MeshRenderer>().material.color);
        }

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        //if (instance == null)
        //{
        //    instance = this;
        //}
        //else
        //{
        //    Destroy(this);
        //}


        startForward = colorCube.transform.forward;
        startUp = colorCube.transform.up;
        startRight = colorCube.transform.right;
        availableNormals = new List<Vector3>();
        availableNormalPos = new List<Vector3>();

        beginRotation = transform.rotation;

        for (int i = 0; i < startNormals.Length; i++)
        {
            availableNormals.Add(startNormals[i].up);
            availableNormalPos.Add(startNormals[i].position);
        }
        GetAvailableColor();
        GetCurrentColor();
    }

    public void ResetCube()
    {
        iTween.Stop();
        colorCube.transform.forward = startForward;
        colorCube.transform.up = startUp;
        colorCube.transform.right = startRight;

        transform.rotation = beginRotation;

        GetCurrentColor();
    }

    float f = 0;

    private void Update()
    {
        HandleInputs();
        if (rotating)
        {
            GetCurrentColor();
        }

        //f = Mathf.PingPong(f, 1);
        //var mr = GetComponent<MeshRenderer>();
        //mr.material.SetFloat("Vector1_7C254603", f);
        //for (int i = 0; i < availableNormals.Count; i++)
        //{
        //    Debug.DrawRay(availableNormalPos[i], availableNormals[i] * 10);
        //}
        //
        //foreach (GameObject pl in planes)
        //{
        //    Debug.DrawRay(pl.transform.position, pl.transform.up * 5, Color.black);
        //}

        //GetAvailableColor();
    }

    private void LateUpdate()
    {
        if (needsToCheckColors)
        {
            GetCurrentColor();
            needsToCheckColors = false;
        }

        //GetCurrentColor();
    }

    private void HandleInputs()
    {
        if (rotating || ignoreInput)
            return;

        #region Unity
#if UNITY_EDITOR
        //if (Input.GetMouseButtonUp(0))
        //{
        //    pressed = true;

        //    if (holdingDown == true)
        //    {
        //        HandleRotation();
        //        holdingDown = false;
        //    }
        //}

        //if (Input.GetMouseButtonDown(0))
        //{
        //    startDrag = Input.mousePosition;
        //}
        //if (Input.GetMouseButton(0) || Input.touches.Length > 0)
        //{
        //    holdingDown = true;
        //    endDrag = Input.mousePosition;
        //}
        //else
        //{
        //    holdingDown = false;
        //}
#endif
        #endregion

        #region Mobile
        if (Input.touchCount == 1)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;
                isTouching = true;
                startDrag = Input.touches[0].position;
            }
            else if (Input.touches[0].phase == TouchPhase.Moved)
            {
                endDrag = Input.touches[0].position;
                moved = true;
            }
        }
        else
        {
            if (isTouching && moved)
            {
                HandleRotation();
                isTouching = false;
                moved = false;
            }
        }

        #endregion
    }

    private void HandleRotation()
    {
        Vector2 fPositionStart = new Vector2(startDrag.x / Screen.width, startDrag.y / Screen.height);
        Vector2 fPositionEnd = new Vector2(endDrag.x / Screen.width, endDrag.y / Screen.height);


        Vector2 dir = (endDrag - startDrag).normalized;
        //Debug.Log(dir);
        dir.x = Mathf.RoundToInt(dir.x);
        dir.y = Mathf.RoundToInt(dir.y);

        NewRotation(fPositionStart, dir);
    }

    private void NewRotation(Vector2 position, Vector2 direction)
    {
        startRotation = colorCube.transform.eulerAngles;

        //if clicking from left side of screen
        //should only be able to go right or up or down

        Hashtable itween = new Hashtable();
        itween.Add("time", tweenSpeed);
        itween.Add("space", "world");
        itween.Add("oncomplete", "SnapToNearestRotation");
        itween.Add("oncompletetarget", this.gameObject);

        if (position.x < 0.5f)
        {
            if (direction == new Vector2(1, 0))
            {
                itween.Add("amount", cubeHolder.transform.up.normalized * -0.25f);
            }
            else if (direction == new Vector2(0, 1) || direction == new Vector2(1, 1))
            {
                itween.Add("amount", (cubeHolder.transform.right.normalized + (rightOffset) * Mathf.PI / 180) * 0.25f);
            }
            else if (direction == new Vector2(0, -1))
            {
                itween.Add("amount", (cubeHolder.transform.right.normalized - (rightOffset) * Mathf.PI / 180) * -0.25f);
            }
        }

        //if clicking from right side of screen
        //should only be able to go left or up or down
        else
        {
            if (direction == new Vector2(-1, 0))
            {
                itween.Add("amount", cubeHolder.transform.up.normalized * 0.25f);
            }
            else if (direction == new Vector2(0, 1) || direction == new Vector2(-1, 1))
            {
                itween.Add("amount", (cubeHolder.transform.forward.normalized + (forwardOffset) * Mathf.PI / 180) * 0.25f);
            }
            else if (direction == new Vector2(0, -1))
            {
                itween.Add("amount", (cubeHolder.transform.forward.normalized - (forwardOffset) * Mathf.PI / 180) * -0.25f);
            }
        }

        //zero = 0;
        //one = 1;
        rotating = true;

        Rotate(rotateAxis, rotateDegree, itween);
    }

    private void Rotate(Vector3 axis, float degrees, Hashtable itween)
    {
        //if (axis == Vector3.zero || degrees == 0)
        //{
        //    rotating = false;
        //    return;
        //}
        if (rotating)
        {
            iTween.RotateBy(colorCube, itween);
            AudioManager.instance?.PlayRotate();
            rotateAxis = Vector2.zero;
            rotateDegree = 0;
        }
    }

    private void OnGUI()
    {
        //GUI.Label(new Rect(10, 10, 100, 20), Input.mousePosition.ToString());
        //GUI.Label(new Rect(10, 30, 100, 20), new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height).ToString());
        //
        //GUI.Label(new Rect(10, 50, 1000, 250), "Drag start: " + startDrag.ToString(), style);
        //GUI.Label(new Rect(10, 300, 1000, 250), "Drag end: " + endDrag.ToString(), style);
        //GUI.Label(new Rect(10, 550, 1000, 250), "Drag dir: " + (endDrag - startDrag).normalized.ToString(), style);
    }

    private void SnapToNearestRotation()
    {
        var vec = colorCube.transform.eulerAngles;
        vec.x = Mathf.Round(vec.x / 45) * 45;
        vec.y = Mathf.Round(vec.y / 45) * 45;
        vec.z = Mathf.Round(vec.z / 45) * 45;
        colorCube.transform.eulerAngles = vec;

        rotating = false;

        GetAvailableColor();
    }

    public void GetCurrentColor()
    {
        foreach (GameObject go in planes)
        {
            var roundedUpY = Mathf.Round(go.transform.up.y);
            if (roundedUpY == 1)
            {
                getableColor = go.GetComponent<MeshRenderer>().material.color;
            }
        }

        GetAvailableColor();
    }

    void GetAvailableColor()
    {
        availableColors.Clear();

        //go through each plane
        foreach (GameObject go in planes)
        {
            //temp normal is plane's normal
            var tempNorm = go.transform.up;
            var angleUp = Vector3.Angle(go.transform.up, Vector3.up);


            //if the temp normal is up
            if (go.transform.up == Vector3.up || angleUp < 45)
            {
                //add the plane's color to the availables
                availableColors.Add(go.GetComponent<MeshRenderer>().material.color);
                continue;
            }

            string color = ColorUtility.ToHtmlStringRGB(go.GetComponent<MeshRenderer>().material.color);
            Debug.Log(go.transform.up + ": Color = " + ColorUtility.ToHtmlStringRGB(go.GetComponent<MeshRenderer>().material.color));

            if (Mathf.Sign(go.transform.up.z) == -1 && go.transform.up.z < -0.5f)
            {
                availableColors.Add(go.GetComponent<MeshRenderer>().material.color);
            }

            #region oldCode

            //RaycastHit hitty;
            //if (Physics.Raycast(Camera.main.transform.position, go.transform.position - Camera.main.transform.position, out hitty, 10f, planeMask))
            //{
            //    if (hitty.collider.gameObject == go)
            //        availableColors.Add(go.GetComponent<MeshRenderer>().material.color);
            //}
            #endregion
        }

        #region oldCode

        //Debug.Break();


        //foreach(Transform t in rayPoints)
        //{
        //    RaycastHit hit;
        //    Ray ray = new Ray(t.position, t.forward * 1.5f);
        //    Debug.DrawRay(t.position, t.forward * 1.5f, Color.red);
        //    //Debug.Break();
        //    if(Physics.Raycast(t.position, t.forward, out hit, 1.5f, planeMask))
        //    {
        //        GameObject hitObj = hit.collider.gameObject;
        //        if(hitObj)
        //        {
        //            availableColors.Add(hitObj.GetComponent<MeshRenderer>().material.color);
        //        }
        //    }
        //}
        #endregion
    }

    private void OnDrawGizmos()
    {
        foreach (Transform t in rayPoints)
        {
            //Gizmos.DrawLine(t.position, t.forward * 0.5f);
        }
    }

    public void SwapMaterial()
    {
        GameObject upPlane = null;
        for(int i  = 0; i < planes.Length; i++)
        {
            var tempNorm = planes[i].transform.up;
            var angleUp = Vector3.Angle(planes[i].transform.up, Vector3.up);
            if (tempNorm == Vector3.up || angleUp < 45)
            {
                upPlane = planes[i];
            }
        }

        MeshRenderer mr = upPlane.GetComponent<MeshRenderer>();
        Material temp = mr.sharedMaterial;
        if(temp)
        {
            foreach(MaterialPair mp in materialPairs)
            {
                if(temp.color.Equals(mp.baseMat.color))
                {
                    //StartCoroutine(SwapMaterial(mr, mp.baseMat, mp.shaderMat));
                }
            }
        }
    }

    IEnumerator SwapMaterial(MeshRenderer holder, Material old, Material newMat)
    {
        holder.material = newMat;

        float timer = 0.0f;
        while(timer < 1f)
        {
            holder.material.SetFloat("Vector1_7C254603", timer);

            timer += Time.deltaTime * 2;
        }

        holder.material = old;
        yield return null;

        //holder.material = newMat;
        //yield return new WaitForSeconds(0.5f);
        //holder.material = old;
    }
}
