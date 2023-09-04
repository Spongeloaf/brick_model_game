using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CameraController : MonoBehaviour
{
  private GameObject anchor;
  private GameObject bottomReference;
  private GameObject topReference;
  private Camera playerCamera;
  private float sensitivity = 0.3f;
  private float maxTiltAngle = 89.9f;
  private float minTiltAngle = 1.0f;
  private float minZoomDist = 0.2f;
  private float maxZoomDist = 1.0f;

  private float camDistanceScalar = 0.5f;
  private float camTiltAngle = 20f;
  private float camPanAngle = 0.0f;
  private float hypotenuse = 0.0f;

  public void Awake()
  {
    playerCamera = GetComponentInChildren<Camera>();
    Assert.IsNotNull(playerCamera);

    anchor = new GameObject();
    bottomReference = new GameObject();
    topReference = new GameObject();

    anchor.transform.position = Vector3.zero;

    // Magic numbers! Programmers normally hate these, but whatever.
    // I suppose the better option would be to place a single anchor point
    // in the prefab and do all the math from htere. Sue me, this works,
    // and only takes a moment to update.
    bottomReference.transform.position = new Vector3(0, 0, -200);
    topReference.transform.position = new Vector3(0, 200, -200);
    hypotenuse = 300;

    Vector3 side1 = bottomReference.transform.position - anchor.transform.position;
    Vector3 side2 = topReference.transform.position - anchor.transform.position;
    camTiltAngle = Vector3.Angle(side1, side2);
    RotateCamera(new Vector2(), 0.0f, new Vector2());
  }

  private void AdjustAnglesAndDistance(Vector2 panAndTilt, float zoom, Vector2 move)
  {
    panAndTilt *= sensitivity;

    camPanAngle += panAndTilt.x;
    camTiltAngle = Mathf.Clamp(camTiltAngle - panAndTilt.y, minTiltAngle, maxTiltAngle);
    camDistanceScalar = Mathf.Clamp(camDistanceScalar += zoom, minZoomDist, maxZoomDist);
  }

  public void RotateCamera(in Vector2 panAndTilt, in float zoom, in Vector2 move)
  {
    // This function cannot handle the camera going outside of a 90 degree window because
    // the 3d angle math isn't robust. A superior method would probably be to use vector
    // math and ome triangle magic to rotate the camera then use geometry to figure out
    // what the distance and height should be. That would let me set simple angular 
    // restarinats for rotation.

    if (playerCamera == null)
      return;

    AdjustAnglesAndDistance(panAndTilt, zoom, move);

    // Make a triangle out of the top point, the anchor point and refernce point.
    float cameraYPos = Mathf.Sin(Mathf.Deg2Rad * camTiltAngle) * hypotenuse;
    float cameraZPos = Mathf.Cos(Mathf.Deg2Rad * camTiltAngle) * hypotenuse;

    topReference.transform.position = new Vector3(0, cameraYPos, cameraZPos);
    playerCamera.transform.position = Vector3.Lerp(anchor.transform.position, topReference.transform.position, camDistanceScalar);
    playerCamera.transform.LookAt(anchor.transform.position);

    // Pan horizontally
    playerCamera.transform.RotateAround(anchor.transform.position, Vector3.up, camPanAngle);
  }

  public float GetCameraHeading()
  {
    return camPanAngle;
  }
}
