using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject projectile;
    public GameObject altProjectile;
    public float fireRate = 0.25f;
    public float altFireRate = 0.25f;
    public Transform paintIndicator0;
    public Transform paintIndicator1;

    public WorldGenerator wg;

    private float timer;
    private float altTimer;
    private bool painter;
    private float paintRadius = 1f;
    private float paintStr = 1f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            painter = !painter;

        if(timer <= 0 && Input.GetMouseButton(0))
        {
            Fire();
        }
        if (altTimer <= 0 && Input.GetMouseButton(1))
        {
            AltFire();
        }

        if (timer > 0)
            timer -= Time.deltaTime;
        if (altTimer > 0)
            altTimer -= Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            paintRadius += Input.mouseScrollDelta.y;
            paintRadius = Mathf.Clamp(paintRadius, 1, 25);
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            paintStr += Input.mouseScrollDelta.y;
            paintStr = Mathf.Clamp(paintStr, 0.1f, 25);
        }

        if (painter)
        {
            paintIndicator0.gameObject.SetActive(true);
            paintIndicator1.gameObject.SetActive(true);
            paintIndicator0.localScale = new Vector3(paintRadius, paintRadius, paintRadius);
            RaycastHit hit;
            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                paintIndicator0.position = new Vector3((int)hit.point.x, (int)hit.point.y, (int)hit.point.z);
                paintIndicator1.position = new Vector3((int)hit.point.x, (int)hit.point.y, (int)hit.point.z);
            } else
            {
                paintIndicator0.gameObject.SetActive(false);
                paintIndicator1.gameObject.SetActive(false);
            }
        } else
        {
            paintIndicator0.gameObject.SetActive(false);
            paintIndicator1.gameObject.SetActive(false);
        }
    }

    void Fire()
    {
        timer = fireRate;

        if (!painter)
        {
            Instantiate(projectile, Camera.main.transform.position + Camera.main.transform.forward * 2f, Camera.main.transform.rotation);
        } else
        {
            Paint(paintRadius * 2, paintStr);
        }
    }

    void AltFire()
    {
        altTimer = fireRate;

        if (!painter)
        {
            Instantiate(altProjectile, Camera.main.transform.position + Camera.main.transform.forward * 2f, Camera.main.transform.rotation);
        } else
        {
            Paint(paintRadius * 2, -paintStr);
        }
    }

    void Paint(float r, float w)
    {
        wg.ModifyTerrain(paintIndicator0.position, r, w);
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();

        GUILayout.Box($"Paint Mode: {painter} (Press P to toggle...)\nPaint Radius: {paintRadius} (LeftShift + ScrollWheel to change...)\nPaint Strength: {paintStr} (LeftCtrl + ScrollWheel to change...)");

        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}
