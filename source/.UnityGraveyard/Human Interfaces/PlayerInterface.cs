using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Assertions;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using System.Reflection;
using System;
using ActionList = System.Collections.Generic.List<ButtonEvents>;

public enum ButtonEvents
{
  commit,
  cancel,
  action1,
  selectNext,
  selectPrev,
}

public enum InputType
{
  MouseAndKeyboard,
  Controller,
}

public class PlayerInputs
{
  public RaycastHit playerCursor;
  public Vector2 controllerDelta;
  public ActionList actions;
  public InputType activeInputType;
  public float cameraHeading = 0.0f;  // Used to orient directional imputs
}
public class PlayerInterface : MonoBehaviour
{
  // If you add any new actions, be sure to also add them in CheckInputType()!
  public InputAction MkbMouseCursorMove;
  public InputAction MkbCommit;
  public InputAction MkbBack;
  public InputAction MkbAction1;

  public InputAction GamepadIncreaseBuildHeight;
  public InputAction GamepadDecreaseBuildHeight;
  public InputAction GamepadCommit;
  public InputAction GamepadBack;
  public InputAction GamepadCursorMove;
  public InputAction GamepadCameraMove;
  public InputAction GamepadSelectNext;
  public InputAction GamepadSelectPrev;
  public InputAction GamepadSwitchBuildType;

  private List<InputAction> gamepadActions;
  private List<InputAction> mkbActions;

  private float zoomScalar = 0.1f;
  private string label1;
  private string label2;
  private Vector2 PanAndTiltLastMousePos;
  private Vector2 lastMousePos;
  private CameraController cameraController;
  private float controllerSensitivity = 1.5f;
  LineRenderer lineRenderer;
  GameManager m_gameManager;
  //MaterialLibrary m_matLib;
  private InputType activeInputType;
  bool isGameStarted = false;
  //GameUI gameUi;

  void Awake()
  {
    SetupInputLists();

    var myparent = transform.parent;
    m_gameManager = myparent.GetComponentInChildren<GameManager>();
    Assert.IsNotNull(m_gameManager);
    if (m_gameManager == null)
    {
      Debug.Log("PlayerInterface failed to find game manager!");
      return;
    }
    else
    {
      Debug.Log("PlayerInterface found game manager");
    }
  }

  void Start()
  {
    var myparent = transform.parent;
    cameraController = myparent.GetComponentInChildren<CameraController>();
    Assert.IsNotNull(cameraController);
    if (cameraController is null)
      return;

    Vector2 pantAndTilt = Vector2.zero;
    pantAndTilt.y = 90;
    cameraController.RotateCamera(pantAndTilt, 0.1f, Vector2.zero);
    Debug.Log("PlayerInterface is started");

    //try
    //{
    //  gameUi = myparent.transform.GetComponentInChildren<GameUI>();
    //}
    //catch
    //{ }

    //if (gameUi == null)
    //  Debug.LogError("Could not locate UI");
  }

  private void SetupInputLists()
  {
    gamepadActions = new List<InputAction>();
    mkbActions = new List<InputAction>();

    FieldInfo[] myMemberInfo;
    Type myType = this.GetType();
    myMemberInfo = myType.GetFields();

    foreach (FieldInfo member in myMemberInfo)
    {
      try
      {
        // Do this first to shortcut the rest of the work if its not an InputAction object
        InputAction action = (InputAction)member.GetValue(this);

        if (member.Name.StartsWith("gamepad", StringComparison.OrdinalIgnoreCase))
          gamepadActions.Add(action);
        else if (member.Name.StartsWith("mkb", StringComparison.OrdinalIgnoreCase))
          mkbActions.Add(action);
      }
      catch
      {
        //  Do nothing, we only care about input actions.
      }
    }
  }

  // Update is called once per frame
  void Update()
  {
    UpdateCamera();
    PlayerInputs inputs = GetPlayerInputs();
    m_gameManager.DoUpdate(inputs);

    // call this last!
    lastMousePos = Mouse.current.position.ReadValue();
  }

  private bool WasMouseOrKeyboardUsedThisFrame()
  {
    bool result = false;
    foreach (InputAction action in mkbActions)
      result |= action.WasPerformedThisFrame();

    result |= (lastMousePos != Mouse.current.position.ReadValue());
    return result;
  }

  private bool WasControllerPressedThisFrame()
  {
    bool result = false;
    foreach (InputAction action in gamepadActions)
      result |= action.WasPerformedThisFrame();

    return result;
  }

  private void CheckInputType()
  {
    if (WasControllerPressedThisFrame())
      activeInputType = InputType.Controller;
    else if (WasMouseOrKeyboardUsedThisFrame())
      activeInputType = InputType.MouseAndKeyboard;
  }

  private PlayerInputs GetPlayerInputs()
  {
    CheckInputType();

    PlayerInputs result = new PlayerInputs();
    result.actions = GetButtonEvents();
    result.playerCursor = GetPlayerCursorPositionInWorld();
    result.controllerDelta = GetControllerDelta();
    result.activeInputType = activeInputType;
    result.cameraHeading = cameraController.GetCameraHeading();
    return result;
  }

  private Vector2 GetPanAndTiltMouse()
  {
    Vector2 currentMousePos = Mouse.current.position.ReadValue();
    
    // This prevents the camera jumping when you start panning
    if (Input.GetMouseButtonDown(2))
      PanAndTiltLastMousePos = currentMousePos;
    
    Vector2 Result = currentMousePos - PanAndTiltLastMousePos;
    PanAndTiltLastMousePos = currentMousePos;
    return Result;
  }

  private Vector2 GetPanAndTiltMController()
  {
    Vector2 result = GamepadCameraMove.ReadValue<Vector2>();
    result *= controllerSensitivity;
    return result;
  }

  private Vector2 GetPanAndTilt()
  {
    if (Input.GetMouseButton(2))
      return GetPanAndTiltMouse();
    else if (GamepadCameraMove.WasPerformedThisFrame() || true)
      return GetPanAndTiltMController();

    return Vector2.zero;
  }

  void UpdateCamera()
  {
    if (cameraController == null)
      return;

    Vector2 cameraVector = GetPanAndTilt();

    // -1 to invert the scroll axis
    float zoom = Input.mouseScrollDelta.y * zoomScalar * (-1);

    if (zoom != 0.0f || cameraVector != Vector2.zero)
      cameraController.RotateCamera(cameraVector, zoom, new Vector2());
  }

  //void DoGui()
  //{
  //  Vector2 mousePos = Mouse.current.position.ReadValue();

  //  Ray ray = Camera.main.ScreenPointToRay(mousePos);
  //  RaycastHit hit;
  //  Vector3 mouse3D = new Vector3();
  //  LayerMask hitLayers = LayerMask.GetMask(Globals.LayerStrings.Gameboard);
  //  if (Physics.Raycast(ray, out hit, Mathf.Infinity, hitLayers))
  //    mouse3D = hit.point;

  //  label1 = string.Format("Mouse 2D: {0}, {1}",
  //    mousePos.x,
  //    mousePos.y);

  //  label2 = string.Format("Mouse 3D: {0}, {1}, {2}",
  //   mouse3D.x,
  //   mouse3D.y,
  //   mouse3D.z);

  //  // Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100, Color.blue);
  //  // Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);

  //  //Vector3 godView = Vector3.zero;
  //  //godView.y = 100;
  //  //Vector3[] points = { godView, hit.point };
  //  //lineRenderer.SetPositions(points);
  //}

  //void OnGUI()
  //{
  //  GUI.Label(new Rect(10, 30, 300, 50), label1);
  //  GUI.Label(new Rect(10, 50, 300, 50), label2);
  //}

  bool IsMouseOverUI()
  {
    if (EventSystem.current.IsPointerOverGameObject())
      return true;

    return false;
  }

  ActionList GetButtonEvents()
  {
    ActionList inputs = new ActionList();
    if (MkbCommit.WasReleasedThisFrame() || GamepadCommit.WasReleasedThisFrame())
      inputs.Add(ButtonEvents.commit);

    if (MkbBack.WasReleasedThisFrame() || GamepadBack.WasReleasedThisFrame())
      inputs.Add(ButtonEvents.cancel);

    // Don't allow increase and decrease in the same frame!
    if (MkbAction1.WasReleasedThisFrame())
      inputs.Add(ButtonEvents.action1);

    // Don't allow both at the tsame time!
    if (GamepadSelectNext.WasPressedThisFrame())
      inputs.Add(ButtonEvents.selectNext);
    else if (GamepadSelectPrev.WasPressedThisFrame())
      inputs.Add(ButtonEvents.selectPrev);

    return inputs;
  }

  public void OnEnable()
  {
    foreach (InputAction action in mkbActions)
      action.Enable();

    foreach (InputAction action in gamepadActions)
      action.Enable();
  }

  public void OnDisable()
  {
    foreach (InputAction action in mkbActions)
      action.Disable();

    foreach (InputAction action in gamepadActions)
      action.Disable();
  }

  // Gets the world position from the cursor or whatever input device the player is using
  private RaycastHit GetPlayerCursorPositionInWorld()
  {
    Vector3 cursor = Mouse.current.position.ReadValue();
    cursor.z = Camera.main.nearClipPlane;

    // Currently only supports the mouse
    Ray castPoint = Camera.main.ScreenPointToRay(cursor);
    RaycastHit hit;
    Physics.Raycast(castPoint, out hit, Mathf.Infinity);
    return hit;
  }

  private Vector2 GetControllerDelta()
  {
    Vector2 result = GamepadCursorMove.ReadValue<Vector2>();
    result *= controllerSensitivity;
    return result;
  }
}
