using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasRenderer))]
public class CurvedSegmentGraphic : MaskableGraphic, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, Min(0f)] private float innerRadius = 100f;
    [SerializeField, Min(0f)] private float outerRadius = 200f;
    [SerializeField, Range(0f, 360f)] private float startAngle = 0f;
    [SerializeField, Range(0f, 360f)] private float endAngle = 60f;
    [SerializeField, Range(4, 256)] private int segments = 32;
    [SerializeField, Min(0f)] private float dividerThickness = 4f;
    [SerializeField] private Color dividerColor = new Color(0.5f, 0.5f, 0.5f, 0);
    [SerializeField] private Color hoverColor = new Color(0.8f, 0.8f, 0.8f, 0);

    private Vector2 centerOffset = Vector2.zero;
    private Color baseColor;

    protected override void Awake ()
    {
        base.Awake();
        baseColor = color;
    }

    public void Configure (float inner, float outer, float startDeg, float endDeg, int segs, Vector2 centerLocalOffset, float? divider = null)
    {
        innerRadius = inner;
        outerRadius = outer;
        startAngle = startDeg;
        endAngle = endDeg;
        segments = Mathf.Clamp(segs, 4, 256);
        centerOffset = centerLocalOffset;

        if (divider.HasValue)
            dividerThickness = divider.Value;

        SetVerticesDirty();
    }

    protected override void OnPopulateMesh (VertexHelper vh)
    {
        vh.Clear();

        float arc = Mathf.Repeat(endAngle - startAngle, 360f);
        if (arc <= 0.0001f) return;

        float startRad = startAngle * Mathf.Deg2Rad;
        float step = arc * Mathf.Deg2Rad / segments;
        Vector2 center = centerOffset;

        var verts = new List<UIVertex>();

        for (int i = 0; i <= segments; i++)
        {
            float a = startRad + step * i;
            Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));

            Vector2 inner = center + dir * innerRadius;
            Vector2 outer = center + dir * outerRadius;

            verts.Add(MakeVertex(inner, color));
            verts.Add(MakeVertex(outer, color));
        }

        for (int i = 0; i < verts.Count; i++) 
            vh.AddVert(verts[i]);

        for (int i = 0; i < segments; i++)
        {
            int idx = i * 2;
            vh.AddTriangle(idx, idx + 1, idx + 2);
            vh.AddTriangle(idx + 1, idx + 3, idx + 2);
        }

        AddDivider(vh, startAngle, center, innerRadius, outerRadius, dividerThickness, dividerColor);
        AddDivider(vh, endAngle, center, innerRadius, outerRadius, dividerThickness, dividerColor);
    }

    private void AddDivider (VertexHelper vh, float angleDeg, Vector2 center, float inner, float outer, float thickness, Color c)
    {
        float a = angleDeg * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
        Vector2 tangent = new Vector2(-dir.y, dir.x);

        Vector2 innerPos = center + dir * inner;
        Vector2 outerPos = center + dir * outer;

        Vector2 innerPos2 = innerPos + tangent * thickness;
        Vector2 outerPos2 = outerPos + tangent * thickness;

        int startIndex = vh.currentVertCount;

        vh.AddVert(MakeVertex(innerPos, c));
        vh.AddVert(MakeVertex(outerPos, c));
        vh.AddVert(MakeVertex(outerPos2, c));
        vh.AddVert(MakeVertex(innerPos2, c));

        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
    }

    private UIVertex MakeVertex (Vector2 pos, Color c)
    {
        var v = UIVertex.simpleVert;
        v.position = pos;
        v.color = c;
        return v;
    }

    public override bool Raycast (Vector2 sp, Camera eventCamera)
    {
        Vector2 local;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, eventCamera, out local))
            return false;

        local -= centerOffset;

        float dist = local.magnitude;
        if (dist < innerRadius || dist > outerRadius) return false;

        float angle = Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        float arc = Mathf.Repeat(endAngle - startAngle, 360f);
        float rel = Mathf.Repeat(angle - startAngle, 360f);
        return rel <= arc;
    }

    public void SetHover (bool isHovering)
    {
        color = isHovering ? hoverColor : baseColor;
        SetVerticesDirty();
    }

    public void OnPointerEnter (PointerEventData eventData)
    {
        SetHover(true);
    }

    public void OnPointerExit (PointerEventData eventData)
    {
        SetHover(false);
    }
}
