using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenDecorator : MonoBehaviour
{
  private LineRenderer m_LineRenderer;

  public void Start()
  {
    m_LineRenderer = GetComponent<LineRenderer>();
    if (m_LineRenderer == null)
    {
      Debug.LogError("ScreenDecorator could not find its line renderer");
      return;
    }

    m_LineRenderer.sortingOrder = 1;
  }

  public void DrawPath(Vector3[] points)
  {
    if (m_LineRenderer == null)
      return;

    m_LineRenderer.SetPositions(points);
  }
}
