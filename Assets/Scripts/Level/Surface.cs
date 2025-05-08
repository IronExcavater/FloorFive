using UnityEngine;

namespace Level
{
    public enum SurfaceType
    {
        Carpet,
        Tile,
        Wood
    }
    
    public class Surface : MonoBehaviour
    {
        public SurfaceType surfaceType;
    }
}