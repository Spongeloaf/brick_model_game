using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

public class GameManager : MonoBehaviour
{
  private GameObject m_SelectedUnit;

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  public void DoUpdate(PlayerInputs inputs)
  {
    HandleInputs(inputs);
  }

  private void HandleInputs(PlayerInputs inputs)
  {
    if (inputs is null)
    {
      return;
    }

    if (inputs.actions.Contains(ButtonEvents.commit))
    {
      DoSelection(inputs.playerCursor);
    }
  }

  private void DoSelection(RaycastHit cursor)
  {
    UnSelectUnit();
    
    if (cursor.collider != null) 
      SelectUnit(cursor.collider.transform.gameObject);
  }

  private void UnSelectUnit()
  {
    PawnUtils.ClearHighlight(m_SelectedUnit);
    m_SelectedUnit = null;
  }

  private void SelectUnit(GameObject unit)
  {
    if (unit == null)
      return;

    if (unit.layer != Globals.LayerInts.Pawns)
      return;

    m_SelectedUnit = unit;
    PawnUtils.SetHighlight(unit, UnityEngine.Color.green);
  }
}
