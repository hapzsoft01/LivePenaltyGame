using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System;
using System.Net;
using TMPro;

public class Swipe : MonoBehaviour
{
    // touch start position, touch end position, swipe direction
    Vector2 startPos, endPos, direction;

    // to calculate swipe time to sontrol throw force inZdirection
    float touchTimeStart, touchTimeFinish, timeInterval;

    [SerializeField]
    // to control throw force inXandYdirections
    float throwForceInXandY = 1f;

    [SerializeField]
    // to control throw force inZdirection
    float throwfForceInZ = 50f;

    Rigidbody rb;

    public Vector3 startPosition;
    public static Vector3 contactPoint;

    public GameObject hitPoint;

    public string formatedText;
    public string winOrLose;

    public TextMeshProUGUI textMeshPro;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
    }



    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {

            // if you touch the screen
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                //if (EventSystem.current.IsPointerOverGameObject())
                //    return;

                // getting touch position and marking time when you touch the screen
                touchTimeStart = Time.time;
                startPos = Input.GetTouch(0).position;
            }

            // if you release your finger
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                // marking time when you release it
                touchTimeFinish = Time.time;

                // calculate swipe time interval
                timeInterval = touchTimeFinish - touchTimeStart;

                // getting release finger position
                endPos = Input.GetTouch(0).position;

                // calculating swipe direction in 2D space
                direction = startPos - endPos;

                // add force to balls rigidbody in 3D space depending on swipe time, direction and throw forces
                rb.isKinematic = false;
                rb.AddForce(-direction.x * throwForceInXandY, -direction.y * throwForceInXandY, throwfForceInZ / timeInterval);

                // Destroy ball in4seconds
                //Destroy(gameObject, 3f);
            }
        }
    }



    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Goal"))
        {
            contactPoint = collision.contacts[0].point;

            DataStorage ds = new DataStorage()
            {
                x = contactPoint.x,
                y = contactPoint.y,
            };
            formatedText = JsonUtility.ToJson(ds);

            Instantiate(hitPoint, contactPoint, Quaternion.Euler(90, 0, 0));
            StartCoroutine(Upload());
            StartCoroutine(GetRequest("https://www.example.com"));
            StartCoroutine(ReloadScene());
        }

        if (collision.collider.CompareTag("GameOver"))
        {
            StartCoroutine(ReloadScene());
        }
    }



    /// <summary>
    /// This will reload scene after swipe
    /// </summary>
    /// <returns></returns>
    private IEnumerator ReloadScene()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }



    /// <summary>
    /// API
    /// </summary>
    [Serializable]
    public class DataStorage
    {
        public float x, y;
    }

    /// <summary>
    /// For POST API
    /// </summary>
    /// <returns></returns>
    IEnumerator Upload()
    {
        yield return new WaitForSeconds(0.5f);

        UnityWebRequest www = UnityWebRequest.Post("https://94e554b0-553b-4040-9e9b-a6cfc3d20450.mock.pstmn.io/shoot", formatedText);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Response can be accessed through: request.downloadHandler.text
            winOrLose = www.downloadHandler.text;
            if(winOrLose == "true")
            {
                textMeshPro.text = "GOAL!";
            }
            else
            {
                textMeshPro.text = "MISS!";
            }
        }
    }


    IEnumerator GetRequest(string uri)
    {
        yield return new WaitForSeconds(1f);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }
        }
    }
}
