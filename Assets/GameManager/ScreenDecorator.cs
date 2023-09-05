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
  }

  public void DrawPath(Vector3[] points)
  {
    if (m_LineRenderer == null)
      return;

    m_LineRenderer.SetPositions(points);
    m_LineRenderer.positionCount = points.Length;
  }

  public void ClearAllDecorations()
  {
    if (m_LineRenderer == null)
      return;

    Vector3[] pts = new Vector3[0];
    m_LineRenderer.SetPositions(pts);
    m_LineRenderer.positionCount = 0;
  }
}
