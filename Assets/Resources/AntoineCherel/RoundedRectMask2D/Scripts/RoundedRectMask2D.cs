using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using TMPro;

#if UNITY_2021_1_OR_NEWER
using UnityEngine.Pool;
#endif

namespace ACRoundedRectMask
{
    /// <summary>
    /// Overrides the RectMask2D.PerformClipping method to add extra checks before doing exhaustive culling on 
    /// each maskable target.
    /// </summary>
    public class RoundedRectMask2D : RectMask2D
    {
        public static readonly string RadiiPropertyName = "_ClipRectRadii";
        
        // --- PENAMBAHAN NAMA PROPERTI SHADER BARU ---
        public static readonly string FillCenterPropertyName = "_ClipFillCenter";
        public static readonly string BorderThicknessPropertyName = "_ClipBorderThickness";


        [SerializeField]
        private bool independantRadii;
        [Tooltip("The four corner radii of the rounded rect. (x: top left, y: top right, z: bottom left, w: bottom right)")]
        [SerializeField]
        private Vector4 radii = Vector4.one * 10.0f;
        public Vector4 Radii
        {
            get => radii;
            set
            {
                radii = value;
                MaskUtilities.Notify2DMaskStateChanged(this);
                ForceClip = true;
            }
        }

        // --- PENAMBAHAN FITUR FILL CENTER ---
        [Tooltip("Jika dimatikan, mask hanya akan menampilkan bagian pinggir (border) saja.")]
        [SerializeField]
        private bool fillCenter = true;
        public bool FillCenter
        {
            get => fillCenter;
            set
            {
                fillCenter = value;
                MaskUtilities.Notify2DMaskStateChanged(this);
                ForceClip = true;
            }
        }

        // --- PENAMBAHAN KETEBALAN BORDER ---
        [Tooltip("Ketebalan garis batas mask jika Fill Center dimatikan.")]
        [SerializeField]
        private float borderThickness = 5.0f;
        public float BorderThickness
        {
            get => borderThickness;
            set
            {
                borderThickness = value;
                MaskUtilities.Notify2DMaskStateChanged(this);
                ForceClip = true;
            }
        }

        [Tooltip("If not set to true, you will need to handle that all masked UI elements have their own material instances")]
        [SerializeField]
        private bool cloneMaskableMaterialsOnStart = true;

        private static int clipRectRadiiID = 0;
        
        // --- PENAMBAHAN ID SHADER ---
        private static int clipFillCenterID = 0;
        private static int clipBorderThicknessID = 0;


        private HashSet<IClippable> clipTargets = null;
        private HashSet<MaskableGraphic> maskableTargets = null;
        private int lastclipTargetsCount = 0;
        private int lastmaskableTargetsCount = 0;
        private bool shouldRecalculateClipRects = false;

        private Canvas cachedCanvas = null;
        private Vector3[] cachedCorners = new Vector3[4];
        private Rect lastClipRectCanvasSpace = new Rect();
        private Vector2Int lastSoftness = new Vector2Int();
        private List<RectMask2D> clippers = new List<RectMask2D>();

        #region MonoBehaviour Implementation
        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            shouldRecalculateClipRects = true;
            ForceClip = true;
        }

#if UNITY_EDITOR
        /// <inheritdoc />
        protected override void OnValidate()
        {
            base.OnValidate();
            shouldRecalculateClipRects = true;
            ForceClip = true;
        }
#endif
        /// <inheritdoc />
        protected override void OnDidApplyAnimationProperties()
        {
            base.OnDidApplyAnimationProperties();

            shouldRecalculateClipRects = true;
            ForceClip = true;
        }

        #endregion MonoBehaviour Implementation

        #region RectMask2D Implementation

        /// <inheritdoc />
        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            shouldRecalculateClipRects = true;
        }

        /// <inheritdoc />
        protected override void OnCanvasHierarchyChanged()
        {
            cachedCanvas = null;
            base.OnCanvasHierarchyChanged();
            shouldRecalculateClipRects = true;
        }

        // FIX TAMBAHAN: Memastikan Mask tidak error saat ukuran panel berubah (misal ditarik kursor)
        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            shouldRecalculateClipRects = true;
            ForceClip = true;
        }


        protected override void Start()
        {
            base.Start();

            shouldRecalculateClipRects = true;
            PerformClipping();

            if (cloneMaskableMaterialsOnStart && maskableTargets != null)
            {
                foreach (MaskableGraphic mg in maskableTargets)
                {
                    if (mg.materialForRendering.Equals(mg.material))
                    {
                        Material m = new Material(mg.material);
                        mg.material = m;
                    }
                    else if (mg is TMP_Text tmpText)
                    {
                        Material m = new Material(tmpText.fontMaterial);
                        tmpText.fontMaterial = m;
                    }
                    else
                    {
                        Debug.Log("[RoundedRectMask2d] Can't clone material for " + mg.name + ". This will result in same rounded corners for all assets sharing its materiel " + mg.materialForRendering);
                        continue;
                    }
                    
                    OnSetClipRect(mg);
                }
            }
        }

        public override void PerformClipping()
        {
            // FIX UTAMA: Cek juga apakah clipFillCenterID masih 0 agar tidak terjadi bug ID collision
            if (clipRectRadiiID == 0 || clipFillCenterID == 0)
            {
                clipRectRadiiID = Shader.PropertyToID(RadiiPropertyName);
                clipFillCenterID = Shader.PropertyToID(FillCenterPropertyName);
                clipBorderThicknessID = Shader.PropertyToID(BorderThicknessPropertyName);
            }

            Initialize();

            if (ReferenceEquals(Canvas, null))
            {
                return;
            }

            if (shouldRecalculateClipRects || ForceClip)
            {
                MaskUtilities.GetRectMasksForClip(this, clippers);
                shouldRecalculateClipRects = false;
            }

            bool validRect = true;
            Rect clipRect = Clipping.FindCullAndClipWorldRect(clippers, out validRect);

            RenderMode renderMode = Canvas.rootCanvas.renderMode;
            bool maskIsCulled =
                (renderMode == RenderMode.ScreenSpaceCamera || renderMode == RenderMode.ScreenSpaceOverlay) &&
                !clipRect.Overlaps(RootCanvasRect, true);

            if (maskIsCulled)
            {
                clipRect = Rect.zero;
                validRect = false;
            }

            if (clipRect != lastClipRectCanvasSpace || softness != lastSoftness)
            {
                foreach (IClippable clipTarget in clipTargets)
                {
                    clipTarget.SetClipRect(clipRect, validRect);
                    clipTarget.SetClipSoftness(softness);
                }

                foreach (MaskableGraphic maskableTarget in maskableTargets)
                {
                    maskableTarget.SetClipRect(clipRect, validRect);
                    maskableTarget.SetClipSoftness(softness);
                    OnSetClipRect(maskableTarget);

                    maskableTarget.Cull(clipRect, validRect);
                }
            }
            else if (ForceClip)
            {
                foreach (IClippable clipTarget in clipTargets)
                {
                    clipTarget.SetClipRect(clipRect, validRect);
                    clipTarget.SetClipSoftness(softness);
                }

                foreach (MaskableGraphic maskableTarget in maskableTargets)
                {
                    maskableTarget.SetClipRect(clipRect, validRect);
                    maskableTarget.SetClipSoftness(softness);
                    OnSetClipRect(maskableTarget);

                    if (maskableTarget.canvasRenderer.hasMoved)
                    {
                        maskableTarget.Cull(clipRect, validRect);
                    }
                }
            }
            else
            {
                foreach (MaskableGraphic maskableTarget in maskableTargets)
                {
                    if (!maskableTarget.canvasRenderer.hasMoved)
                    {
                        continue;
                    }

                    maskableTarget.Cull(clipRect, validRect);
                }
            }

            ForceClip = false;
            lastClipRectCanvasSpace = clipRect;
            lastSoftness = softness;
        }

        #endregion RectMask2D Implementation

        public bool ForceClip
        {
            get
            {
                return clipTargets.Count != lastclipTargetsCount ||
                       maskableTargets.Count != lastmaskableTargetsCount;
            }
            set
            {
                if (value == true)
                {
                    lastclipTargetsCount = 0;
                    lastmaskableTargetsCount = 0;
                }
                else
                {
                    Initialize();

                    lastclipTargetsCount = clipTargets.Count;
                    lastmaskableTargetsCount = maskableTargets.Count;
                }
            }
        }

        protected virtual void OnSetClipRect(IClippable clippable)
        { 
            
        }

        protected virtual void OnSetClipRect(MaskableGraphic maskableTarget)
        {
            Material targetMaterial = maskableTarget.materialForRendering;

            if (targetMaterial != null)
            {
                targetMaterial.SetVector(clipRectRadiiID, Radii);
                
                // MENGIRIM DATA FILL CENTER DAN THICKNESS KE SHADER
                targetMaterial.SetFloat(clipFillCenterID, fillCenter ? 1.0f : 0.0f);
                targetMaterial.SetFloat(clipBorderThicknessID, borderThickness);
            }
        }

        private void Initialize()
        {
            if (clipTargets != null)
            {
                return;
            }

            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            clipTargets = (HashSet<IClippable>)typeof(RectMask2D).GetField("m_ClipTargets", bindFlags).GetValue(this);
            maskableTargets = (HashSet<MaskableGraphic>)typeof(RectMask2D).GetField("m_MaskableTargets", bindFlags).GetValue(this);
        }

        private Canvas Canvas
        {
            get
            {
                if (cachedCanvas == null)
                {
#if UNITY_2021_1_OR_NEWER
                    var list = ListPool<Canvas>.Get();
                    gameObject.GetComponentsInParent(false, list);
                    if (list.Count > 0)
                        cachedCanvas = list[list.Count - 1];
                    else
                        cachedCanvas = null;
                    ListPool<Canvas>.Release(list);
#else
                    var list = gameObject.GetComponentsInParent<Canvas>(false);
                    if (list.Length > 0)
                        cachedCanvas = list[list.Length - 1];
                    else
                        cachedCanvas = null;
#endif
                }

                return cachedCanvas;
            }
        }

        private Rect RootCanvasRect
        {
            get
            {
                rectTransform.GetWorldCorners(cachedCorners);

                if (!ReferenceEquals(Canvas, null))
                {
                    Canvas rootCanvas = Canvas.rootCanvas;
                    for (int i = 0; i < 4; ++i)
                        cachedCorners[i] = rootCanvas.transform.InverseTransformPoint(cachedCorners[i]);
                }

                return new Rect(cachedCorners[0].x, cachedCorners[0].y, cachedCorners[2].x - cachedCorners[0].x, cachedCorners[2].y - cachedCorners[0].y);
            }
        }
    }
}