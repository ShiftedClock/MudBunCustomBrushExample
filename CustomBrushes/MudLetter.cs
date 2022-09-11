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
  public class MudLetter : MudSolid
  { 
    // this value matches kLetter in SdfLetter, used in CustomBrush.cginc
    public static readonly int TypeId = 906;

    [SerializeField]
    private Character character = Character.CapitalA;

    [SerializeField]
    [Min(0.01f)]
    private float fontSize = 32f;

    [SerializeField]
    [Min(0.01f)]
    private float depth = 0.5f;
    
    [SerializeField]
    private float m_round = 0.0f;
    public float Round { get => m_round; set { m_round = value; MarkDirty(); } }

    [SerializeField]
    private bool flipped = false;

    private Vector2 offset = Vector2.zero;
    private Texture2D _fontTexture;
    private Texture2D _fontTextureFlipped;
    private int _fontTextureId;
    private int _fontTextureFlippedId;

    public override Aabb RawBoundsRs
    {
      get
      {
        Vector3 r = new Vector3(0.03125f*fontSize, 0.03125f*fontSize, depth);
        Vector3 posRs = PointRs(transform.position);
        Aabb bounds = new Aabb(-r, r);
        bounds.Rotate(RotationRs(transform.rotation));
        bounds.Min += posRs;
        bounds.Max += posRs;
        return bounds;
      }
    }

    private void Awake()
    {
      _fontTextureId = Shader.PropertyToID("ShadertoyFontTexture");
      _fontTextureFlippedId = Shader.PropertyToID("ShadertoyFontFlippedTexture");
      UpdateTextures();
    }

    private void UpdateTextures()
    {
      _fontTexture = Resources.Load<Texture2D>("shadertoy_font");
      _fontTextureFlipped = Resources.Load<Texture2D>("shadertoy_font_flipped");
      Shader.SetGlobalTexture(_fontTextureId, _fontTexture); 
      Shader.SetGlobalTexture(_fontTextureFlippedId, _fontTextureFlipped);
      Shader.EnableKeyword("MUD_FONT_ENABLED");
    }

    public override void SanitizeParameters()
    {
      base.SanitizeParameters();

      Validate.NonNegative(ref m_round);
      Validate.Positive(ref fontSize);
      Validate.Positive(ref depth);
      UpdateTextures();
    }

    public override int FillComputeData(NativeArray<SdfBrush> aBrush, int iStart, List<Transform> aBone)
    {
      SdfBrush brush = SdfBrush.New();
      brush.Type = TypeId; //This needs to match the custom brush identifier used in the shader
      brush.Radius = m_round;
      
      //Add any necessary data to the brush's Data0, Data1, Data2, Data3 vectors.
      //You can also set brush.Position, Rotation, Size, Radius, and a few other things.
      //Check out the SdfBrush struct in SdfBrush.cs for all the relevant data.
      brush.Data0.x = 1f/fontSize;
      
      CharacterMap.TryGetValue(character, out offset);
      //Flip the offset if the texture is flipped
      if (flipped) offset = new Vector2(Mathf.Abs(offset.x - 15), offset.y);
      //Multiplying by fontSize keeps the letter centered
      brush.Data0.y = fontSize*(offset.x+0.5f)/16f;
      brush.Data0.z = fontSize*(offset.y+0.5f)/16f;

      brush.Data0.w = depth;

      brush.Data1.x = flipped ? 1.0f : 0.0f;

      if (aBone != null)
      {
        brush.BoneIndex = aBone.Count;
        aBone.Add(gameObject.transform);
      }

      aBrush[iStart] = brush;

      return 1;
    }

    // Remove code starting here if you don't want to allow unsafe code in the Player settings
#if MUDBUN_BURST
    [BurstCompile]
    [RegisterSdfBrushEvalFunc(SdfBrush.TypeEnum.Box)]
    public static unsafe float EvaluateSdf(float res, ref float3 p, in float3 pRel, SdfBrush* aBrush, int iBrush)
    {
      //TODO: Make this use new Vector3(scale, scale, thickness) for raycast detection
      float3 pRelCopy = pRel;
      float3 h = math.abs(0.5f * aBrush[iBrush].Size);
      float pivotShift = aBrush[iBrush].Data0.x;
      pRelCopy.y += pivotShift * h.y;
      return Sdf.Box(pRelCopy, h, aBrush[iBrush].Radius);
    }
#endif
    // End remove code

    public override void DrawSelectionGizmosRs()
    {
      base.DrawSelectionGizmosRs();

      GizmosUtil.DrawInvisibleBox(PointRs(transform.position), transform.localScale * 0.5f, RotationRs(transform.rotation));
    }

    public override void DrawOutlineGizmosRs()
    {
      base.DrawOutlineGizmosRs();

      GizmosUtil.DrawWireBox(PointRs(transform.position), transform.localScale * 0.5f, RotationRs(transform.rotation));
    }

    public override void OnEnable()
    {
      base.OnEnable(); 
      UpdateTextures();
    }

    public override void OnDisable()
    {
      Renderer.MarkNeedsCompute();
      base.OnDisable();
    }

    // 16 rows x 16 columns = 256 characters, minus 4 blanks
    private enum Character
    {
      Backtick=0, One, Two, Three, Four, Five, Six, Seven, Eight, Nine, Zero, Minus, Equal,
      a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p, q, r, s, t, u, v, w, x, y, z,
      CapitalA, CapitalB, CapitalC, CapitalD, CapitalE, CapitalF, CapitalG, CapitalH, CapitalI,
      CapitalJ, CapitalK, CapitalL, CapitalM, CapitalN, CapitalO, CapitalP, CapitalQ, CapitalR,
      CapitalS, CapitalT, CapitalU, CapitalV, CapitalW, CapitalX, CapitalY, CapitalZ,
      Tilde, Exclamation, At, Pound, Dollar, Percent, Caret, Ampersand, Asterix, LeftParens, RightParens, Underscore, Plus,
      LeftSquareBracket, RightSquareBracket, BackSlash,
      LeftCurlyBrace, RightCurlyBrace, Pipe,
      SemiColon, SingleQuote,
      Colon, Quote,
      Comma, Period, ForwardSlash,
      LessThan, GreaterThan, QuestionMark,
      LeftArrow, UpArrow, RightArrow, DownArrow,
      LeftRightArrows, UpDownArrows,
      UpperLeftArrow, UpperRightArrow, LowerRightArrow, LowerLeftArrow,
      CounterClockwiseArrow, ClockwiseArrow,
      PreviousTrack, Back, Reverse, Record, Stop, Play, Pause, FastForward, NextTrack, Mute, Audio,
      QuarterNote, EighthNote, SixteenthNotes,
      Star, SmileyFace,
    }

    //NOTE: Keys are (column, row) pairs
    //column 0 is far left, column 15 is far right
    //row 15 is top, row 0 is bottom
    private static readonly Dictionary<Character, Vector2> CharacterMap =
      new Dictionary<Character, Vector2>() {
        { Character.PreviousTrack, new Vector2(0, 15) },
        { Character.Back, new Vector2(1, 15) },
        { Character.Reverse, new Vector2(2, 15) },
        { Character.Record, new Vector2(3, 15) },
        { Character.Stop, new Vector2(4, 15) },
        { Character.Play, new Vector2(5, 15) },
        { Character.Pause, new Vector2(6, 15) },
        { Character.FastForward, new Vector2(7, 15) },
        { Character.NextTrack, new Vector2(8, 15) },
        { Character.QuarterNote, new Vector2(9, 15) },
        { Character.EighthNote, new Vector2(10, 15) },
        { Character.SixteenthNotes, new Vector2(11, 15) },
        
        { Character.LeftArrow, new Vector2(0, 14) },
        { Character.UpArrow, new Vector2(1, 14) },
        { Character.RightArrow, new Vector2(2, 14) },
        { Character.DownArrow, new Vector2(3, 14) },
        { Character.LeftRightArrows, new Vector2(4, 14) },
        { Character.UpDownArrows, new Vector2(5, 14) },
        { Character.UpperLeftArrow, new Vector2(6, 14) },
        { Character.UpperRightArrow, new Vector2(7, 14) },
        { Character.LowerRightArrow, new Vector2(8, 14) },
        { Character.LowerLeftArrow, new Vector2(9, 14) },
        { Character.CounterClockwiseArrow, new Vector2(10, 14) },
        { Character.ClockwiseArrow, new Vector2(11, 14) },
        { Character.Star, new Vector2(12, 14) },
        { Character.SmileyFace, new Vector2(13, 14) },
        { Character.Mute, new Vector2(14, 14) },
        { Character.Audio, new Vector2(15, 14) },
        
        //First column of this row is blank in shadertoy_font.png
        { Character.Exclamation, new Vector2(1, 13) },
        { Character.Quote, new Vector2(2, 13) },
        { Character.Pound, new Vector2(3, 13) },
        { Character.Dollar, new Vector2(4, 13) },
        { Character.Percent, new Vector2(5, 13) },
        { Character.Ampersand, new Vector2(6, 13) },
        { Character.SingleQuote, new Vector2(7, 13) },
        { Character.LeftParens, new Vector2(8, 13) },
        { Character.RightParens, new Vector2(9, 13) },
        { Character.Asterix, new Vector2(10, 13) },
        { Character.Plus, new Vector2(11, 13) },
        { Character.Comma, new Vector2(12, 13) },
        { Character.Minus, new Vector2(13, 13) },
        { Character.Period, new Vector2(14, 13) },
        { Character.ForwardSlash, new Vector2(15, 13) },
        
        { Character.Zero, new Vector2(0, 12) },
        { Character.One, new Vector2(1, 12) },
        { Character.Two, new Vector2(2, 12) },
        { Character.Three, new Vector2(3, 12) },
        { Character.Four, new Vector2(4, 12) },
        { Character.Five, new Vector2(5, 12) },
        { Character.Six, new Vector2(6, 12) },
        { Character.Seven, new Vector2(7, 12) },
        { Character.Eight, new Vector2(8, 12) },
        { Character.Nine, new Vector2(9, 12) },
        { Character.Colon, new Vector2(10, 12) },
        { Character.SemiColon, new Vector2(11, 12) },
        { Character.LessThan, new Vector2(12, 12) },
        { Character.Equal, new Vector2(13, 12) },
        { Character.GreaterThan, new Vector2(14, 12) },
        { Character.QuestionMark, new Vector2(15, 12) },
        
        { Character.At, new Vector2(0, 11) },
        { Character.CapitalA, new Vector2(1, 11) },
        { Character.CapitalB, new Vector2(2, 11) },
        { Character.CapitalC, new Vector2(3, 11) },
        { Character.CapitalD, new Vector2(4, 11) },
        { Character.CapitalE, new Vector2(5, 11) },
        { Character.CapitalF, new Vector2(6, 11) },
        { Character.CapitalG, new Vector2(7, 11) },
        { Character.CapitalH, new Vector2(8, 11) },
        { Character.CapitalI, new Vector2(9, 11) },
        { Character.CapitalJ, new Vector2(10, 11) },
        { Character.CapitalK, new Vector2(11, 11) },
        { Character.CapitalL, new Vector2(12, 11) },
        { Character.CapitalM, new Vector2(13, 11) },
        { Character.CapitalN, new Vector2(14, 11) },
        { Character.CapitalO, new Vector2(15, 11) },
        
        { Character.CapitalP, new Vector2(0, 10) },
        { Character.CapitalQ, new Vector2(1, 10) },
        { Character.CapitalR, new Vector2(2, 10) },
        { Character.CapitalS, new Vector2(3, 10) },
        { Character.CapitalT, new Vector2(4, 10) },
        { Character.CapitalU, new Vector2(5, 10) },
        { Character.CapitalV, new Vector2(6, 10) },
        { Character.CapitalW, new Vector2(7, 10) },
        { Character.CapitalX, new Vector2(8, 10) },
        { Character.CapitalY, new Vector2(9, 10) },
        { Character.CapitalZ, new Vector2(10, 10) },
        { Character.LeftSquareBracket, new Vector2(11, 10) },
        { Character.BackSlash, new Vector2(12, 10) },
        { Character.RightSquareBracket, new Vector2(13, 10) },
        { Character.Caret, new Vector2(14, 10) },
        { Character.Underscore, new Vector2(15, 10) },
        
        { Character.Backtick, new Vector2(0, 9) },
        { Character.a, new Vector2(1, 9) },
        { Character.b, new Vector2(2, 9) },
        { Character.c, new Vector2(3, 9) },
        { Character.d, new Vector2(4, 9) },
        { Character.e, new Vector2(5, 9) },
        { Character.f, new Vector2(6, 9) },
        { Character.g, new Vector2(7, 9) },
        { Character.h, new Vector2(8, 9) },
        { Character.i, new Vector2(9, 9) },
        { Character.j, new Vector2(10, 9) },
        { Character.k, new Vector2(11, 9) },
        { Character.l, new Vector2(12, 9) },
        { Character.m, new Vector2(13, 9) },
        { Character.n, new Vector2(14, 9) },
        { Character.o, new Vector2(15, 9) },
        
        { Character.p, new Vector2(0, 8) },
        { Character.q, new Vector2(1, 8) },
        { Character.r, new Vector2(2, 8) },
        { Character.s, new Vector2(3, 8) },
        { Character.t, new Vector2(4, 8) },
        { Character.u, new Vector2(5, 8) },
        { Character.v, new Vector2(6, 8) },
        { Character.w, new Vector2(7, 8) },
        { Character.x, new Vector2(8, 8) },
        { Character.y, new Vector2(9, 8) },
        { Character.z, new Vector2(10, 8) },
        { Character.LeftCurlyBrace, new Vector2(11, 8)},
        { Character.Pipe, new Vector2(12, 8)},
        { Character.RightCurlyBrace, new Vector2(13, 8)},
        { Character.Tilde, new Vector2(14, 8)},
        //Last column of this row is blank in shadertoy_font.png
      };
  }
}

