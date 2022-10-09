// Filename: AdvancedSwipeTracker.cs
// Author: 0xFirekeeper
// Description: Tracks finger and stores them in a Vector2 List.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AdvancedSwipeTracker : MonoBehaviour
{
    public float minSwipeTime = 0f;
    public bool canSwipe = true;
    // Trail Stuff
    public GameObject trailPrefab;
    GameObject thisTrail;
    Plane objPlane;
    //Tracking stuff
    List<Vector2> pointTrack;
    bool mouseDown;
    bool swipeSuccessful = false;
    Vector2 direction;
    Vector2[] curveOffsets;

    private float swipeTime = 0f;

    public static bool IsPointerOverGameObject()
    {
        //check mouse
        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        //check touch
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId))
                return true;
        }

        return false;
    }

    void Start()
    {
        mouseDown = false;
        swipeSuccessful = false;
        objPlane = new Plane(Camera.main.transform.forward * -1, this.transform.position);
    }

    void Update()
    {
        if (!canSwipe)
            return;

        if (Input.GetMouseButtonDown(0) && !IsPointerOverGameObject())
        {
            pointTrack = new List<Vector2>();
            mouseDown = true;
            StartCoroutine(SavePoints());
            swipeTime = Time.time;

            if (trailPrefab != null)
            {
                thisTrail = Instantiate(trailPrefab, this.transform.position, Quaternion.identity);
                Ray mRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                float rayDistance;
                if (objPlane.Raycast(mRay, out rayDistance))
                    thisTrail.transform.position = mRay.GetPoint(rayDistance);
            }

        }
        else if (Input.GetMouseButton(0) && mouseDown)
        {
            if (thisTrail != null)
            {
                Ray mRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                float rayDistance;
                if (objPlane.Raycast(mRay, out rayDistance))
                    thisTrail.transform.position = mRay.GetPoint(rayDistance);
            }

        }
        else if (Input.GetMouseButtonUp(0) && mouseDown)
        {
            mouseDown = false;
            swipeTime = Time.time - swipeTime;

            if (thisTrail != null)
                Destroy(thisTrail);
        }

        if (swipeSuccessful)
        {
            swipeSuccessful = false;
            Debug.Log("Swipe overall direction: " + direction);
            if (direction == Vector2.zero)
            {
                Debug.LogWarning("Invalid Direction");
                return;
            }

            if (curveOffsets == null)
            {
                Debug.Log("No Curves");
                curveOffsets = new Vector2[0];
            }

            foreach (Vector2 v in curveOffsets)
            {
                Debug.Log("Curve " + v);
            }

        }

    }

    IEnumerator SavePoints()
    {
        Vector2 lastPos = (Vector2)Input.mousePosition;
        Vector2 pos;
        pointTrack.Add(lastPos);

        float tolerance = 5 * 5; //5 pixel tolerance

        while (mouseDown)
        {
            pos = (Vector2)Input.mousePosition;

            if ((pos - lastPos).sqrMagnitude >= tolerance)
            {
                pointTrack.Add(pos);
                lastPos = pos;
            }

            yield return null;
        }

        if (pointTrack.Count < 2 || swipeTime < minSwipeTime)
        {
            yield return false;
        }
        swipeSuccessful = true;

        direction = pointTrack[pointTrack.Count - 1] - pointTrack[0];

        if (pointTrack.Count < 3)
        {
            yield return true;
        }

        float tolerance2 = 10;
        int dirChange = -1;
        Vector2 perpDir = new Vector2(-direction.y, direction.x).normalized; //left side
        int lastSide = 0;
        int side = 0;
        float largestOffset = 0;

        List<Vector2> offsets = new List<Vector2>();

        for (int i = 1; i < pointTrack.Count - 1; i++)
        {
            Vector2 offset = (pointTrack[i] - pointTrack[0]) - (Vector2)Vector3.Project(pointTrack[i] - pointTrack[0], direction);

            side = (int)Mathf.Sign(Vector3.Dot(perpDir, offset));

            if (offset.sqrMagnitude < tolerance2 * tolerance2) continue;

            if (side != lastSide)
            {
                dirChange++;
                lastSide = side;
                largestOffset = 0;
            }

            if (offset.sqrMagnitude > largestOffset)
            {
                largestOffset = offset.sqrMagnitude;

                if (dirChange < offsets.Count)
                {
                    offsets[dirChange] = offset;
                }
                else
                {
                    offsets.Add(offset);
                }
            }
        }
        curveOffsets = offsets.ToArray();
    }
}
