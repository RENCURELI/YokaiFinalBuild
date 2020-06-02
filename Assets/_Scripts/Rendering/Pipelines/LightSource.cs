using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.RenderPipelines
{
    [ExecuteInEditMode]
    public class LightSource : MonoBehaviour
    {
        public static LightSource main;
        public static List<LightSource> all = new List<LightSource>();
        public static List<LightSource> dynamics = new List<LightSource>();
        public static List<LightSource> statics = new List<LightSource>();

        public bool dynamic = false;
        public enum WORLD { BOTH=0, HUMAN=1, SPIRIT=2}
        public WORLD world = WORLD.BOTH;
        public Color color;
        public float intensity;
        public float radius;
        public bool startDimmed = false;

        public enum SourceType { POINT=0, AMBIENT=1, SUN=2 };

        public SourceType sourceType = SourceType.POINT;
        public Texture2D cookie;

        private float dimFactor = 1;
        private const float dimSpeed = 1.0f/3.0f;

        public float flickerAmount = 0;
        public float flickerSpeed = 1;

        public List<GameObject> excludeObjects = new List<GameObject>();
        public List<GameObject> includeObjects = new List<GameObject>();

        #region PARAMETERS
        public Vector4 Position
        {
            get
            {
                return new Vector4(transform.position.x, transform.position.y, transform.position.z, radius);
            }
        }
        public Vector4 ColorVector
        {
            get
            {
                return new Vector4(color.r * intensity, color.g * intensity, color.b * intensity, (int)sourceType);
            }
        }
        public float Intensity
        {
            get
            {
                return intensity * dimFactor;
            }
        }
        public int World
        {
            get
            {
                return (int)world;
            }
        }
        public Vector4 Dir
        {
            get
            {
                return transform.forward;
            }
        }
        public Vector4 Info
        {
            get
            {
                return new Vector4(flickerAmount, flickerSpeed, World, 0);
            }
        }
        public Matrix4x4 Projection
        {
            get
            {
                return Matrix4x4.Rotate(transform.rotation).inverse * Matrix4x4.Scale(Vector3.one * radius).inverse * Matrix4x4.Translate(transform.position).inverse;
            }
        }
        #endregion PARAMETERS
        
        public void Awake()
        {
            //all.Add(this);
            if (dynamic) dynamics.Add(this);
            else statics.Add(this);
            if (!main)
            {
                main = this;
                OnAllLightsAwake();
            }
        }

        public void OnDestroy()
        {
            if (dynamic) dynamics.Remove(this);
            else statics.Remove(this);
        }

        public static async void OnAllLightsAwake()
        {
            await Task.Delay(System.TimeSpan.FromMilliseconds(4));
            AssignAllLightIndices();
        }

        public static void AssignAllLightIndices()
        {
            Debug.Log("Lighting > Assigning all dynamic light indices and static lights");
            Debug.Log("Lighting > Found: " + statics.Count + " static lights");
            Debug.Log("Lighting > Found: " + dynamics.Count + " dynamic lights");
            Debug.Log("Lighting > Be patient... This can take a while.");
            Renderer[] allRenderers = GameObject.FindObjectsOfType<Renderer>();
            foreach (var r in allRenderers)
            {
                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                AssignStaticLights(r, mpb);
                AssignLightIndices(r, mpb);
                r.SetPropertyBlock(mpb);
            }
            PipelineLighting.PopulateShaderVariables();
            Debug.Log("Lighting > assignment done.");
        }

        // for static lights only
        public static void AssignStaticLights(Renderer r, MaterialPropertyBlock mpb)
        {
            List<int> indices = new List<int>();

            // Get lights by priority
            for (int i = 0; i < statics.Count; i++)
            {
                if (indices.Count >= PipelineLighting.PER_OBJECT_MAXIMUM_STATIC_LIGHT) break;
                LightSource l = statics[i];
                if (l == null) continue;
                if (l.includeObjects.Contains(r.gameObject) || l.sourceType == SourceType.AMBIENT)
                {
                    indices.Add(i);
                }
            }

            // Get lights by distance
            sortPosition = r.transform.position;
            List<LightSource> nearest = new List<LightSource>();
            nearest.AddRange(statics);
            nearest.Sort(SortByDistance);

            for (int i = 0; i < PipelineLighting.PER_OBJECT_MAXIMUM_STATIC_LIGHT - indices.Count; i++)
            {
                if (i >= nearest.Count) break;
                LightSource l = nearest[i];
                if (!l.excludeObjects.Contains(r.gameObject))
                {
                    indices.Add(statics.IndexOf(l));
                }
            }

            //Debug.Log("Found: " + indices.Count + " static lights for " + r.gameObject.name);

            // Fill the rest of the array
            for (int i = indices.Count; i < PipelineLighting.PER_OBJECT_MAXIMUM_STATIC_LIGHT; i++)
            {
                indices.Add(-1);
            }

            PipelineLighting.PopulateStaticShaderVariables(mpb, indices.ToArray());
        }

        // for dynamic lights only
        public static void AssignLightIndices(Renderer r, MaterialPropertyBlock mpb)
        {
            List<float> indices = new List<float>();

            // Get lights by priority
            for (int i = 0; i < dynamics.Count; i++)
            {
                if (indices.Count >= PipelineLighting.PER_OBJECT_MAXIMUM_DYNAMIC_LIGHT) break;
                LightSource l = dynamics[i];
                if (l == null) continue;
                if (l.includeObjects.Contains(r.gameObject) || l.sourceType == SourceType.AMBIENT)
                {
                    indices.Add(i);
                }
            }

            // Get lights by distance
            sortPosition = r.transform.position;
            List<LightSource> nearest = new List<LightSource>();
            nearest.AddRange(dynamics);
            nearest.Sort(SortByDistance);
            
            for (int i = 0; i < PipelineLighting.PER_OBJECT_MAXIMUM_DYNAMIC_LIGHT - indices.Count; i++)
            {
                if (i >= nearest.Count) break;
                LightSource l = nearest[i];
                if (!l.excludeObjects.Contains(r.gameObject))
                {
                    indices.Add(dynamics.IndexOf(l));
                }
            }

            // Fill the rest of the array
            for (int i = indices.Count; i < PipelineLighting.PER_OBJECT_MAXIMUM_DYNAMIC_LIGHT; i++)
            {
                indices.Add(-1);
            }

            //MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mpb.SetFloatArray(PipelineLighting.lightIndicesId, indices);
            //r.SetPropertyBlock(mpb);
        }

        private static Vector3 sortPosition;
        private static int SortByDistance(LightSource a, LightSource b)
        {
            float distA = Vector3.Distance(a.Position, sortPosition);
            float distB = Vector3.Distance(b.Position, sortPosition);
            if (distA > distB) return 1;
            if (distA == distB) return 0;
            else return -1;
        }

        private void Start()
        {
            if (startDimmed) dimFactor = 0;
        }

        public void Dim()
        {
            CancelInvoke("Light");
            if (dimFactor > 0)
            {
                dimFactor -= Time.deltaTime * dimSpeed;
                Invoke("Dim", 0);
            }
        }

        public void Light()
        {
            CancelInvoke("Dim");
            if (dimFactor < 1)
            {
                dimFactor += Time.deltaTime * dimSpeed;
                Invoke("Light", 0);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(color.r, color.g, color.b, 1);
            if (sourceType == SourceType.POINT)
            {
                Gizmos.DrawWireSphere(transform.position, radius);
            }
            else if (sourceType == SourceType.SUN)
            {
                Gizmos.DrawLine(Position, Position + Dir * 5);
            }

            Gizmos.color = Color.white;
        }
    }
}