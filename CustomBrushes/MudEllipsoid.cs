/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

using System.Collections.Generic;

using Unity.Collections;
using UnityEngine;

#if MUDBUN_BURST
using Unity.Burst;
using Unity.Mathematics;
#endif

namespace MudBun
{
#if MUDBUN_BURST
  [BurstCompile]
#endif
  public class MudEllipsoid : MudSolid
  {
    [SerializeField] private float m_radius = 0.5f;
    public float Radius { get => m_radius; set { m_radius = value; MarkDirty(); } }
    
    [SerializeField]
    public Vector3 dimensions = Vector3.one;

    [Range(-1.0f, 1.0f)] public float PivotShift = 0.0f;
    public Vector3 PivotShiftOffset => -0.5f * transform.up * PivotShift * transform.localScale.y;

    public override Aabb RawBoundsRs
    {
      get
      {
        Vector3 r = m_radius * VectorUtil.Abs(dimensions);
        Vector3 posRs = PointRs(transform.position) + VectorRs(PivotShiftOffset);
        Aabb bounds = new Aabb(-r, r);
        bounds.Rotate(RotationRs(transform.rotation));
        bounds.Min += posRs;
        bounds.Max += posRs;
        return bounds;
      }
    }

    public override void SanitizeParameters()
    {
      base.SanitizeParameters();

      Validate.NonNegative(ref m_radius);
    }

    public override int FillComputeData(NativeArray<SdfBrush> aBrush, int iStart, List<Transform> aBone)
    {
      SdfBrush brush = SdfBrush.New();
      brush.Type = (int) SdfBrush.TypeEnum.Sphere;
      brush.Radius = m_radius;
      brush.Data0.x = PivotShift;
      
      //This is the only custom data set in this brush.
      //Rather than using transform.localScale for brush.Size, we use the 'dimensions' field.
      brush.Size = dimensions;

      if (aBone != null)
      {
        brush.BoneIndex = aBone.Count;
        aBone.Add(gameObject.transform);
      }

      aBrush[iStart] = brush;

      return 1;
    }
    
    public override void FillBrushData(ref SdfBrush brush, int iBrush)
    {
      base.FillBrushData(ref brush, iBrush);

      brush.Position = PointRs(transform.position);
      brush.Rotation = RotationRs(transform.rotation);
      brush.Size = dimensions;

      brush.Flags.AssignBit((int) SdfBrush.FlagBit.Hidden, Hidden);
    }

    // Remove code starting here if you don't want to allow unsafe code in the Player settings
#if MUDBUN_BURST
    [BurstCompile]
    [RegisterSdfBrushEvalFunc(SdfBrush.TypeEnum.Sphere)]
    public static unsafe float EvaluateSdf(float res, ref float3 p, in float3 pRel, SdfBrush* aBrush, int iBrush)
    {
      float3 pRelCopy = pRel;
      float3 h = math.abs(0.5f * aBrush[iBrush].Size);
      float pivotShift = aBrush[iBrush].Data0.x;
      pRelCopy.y += pivotShift * h.y;
      return Sdf.Ellipsoid(pRelCopy, aBrush[iBrush].Radius * aBrush[iBrush].Size);
    }
#endif
    // End remove code

    public override void DrawSelectionGizmosRs()
    {
      base.DrawSelectionGizmosRs();

      GizmosUtil.DrawInvisibleSphere(PointRs(transform.position) + VectorRs(PivotShiftOffset), m_radius, dimensions, RotationRs(transform.rotation));
    }

    public override void DrawOutlineGizmosRs()
    {
      base.DrawOutlineGizmosRs();
      
      GizmosUtil.DrawWireSphere(PointRs(transform.position) + VectorRs(PivotShiftOffset), m_radius, dimensions, RotationRs(transform.rotation));
    }
  }
}

