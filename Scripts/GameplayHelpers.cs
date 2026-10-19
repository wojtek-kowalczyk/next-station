using NS.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using Plane = UnityEngine.Plane;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace NS
{
    public static class GameplayHelpers
    {
        public static Vector3 GetMousePositionOnGroundPlane(Camera cam)
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

            return RaycastGroundPlane(ray);
        }

        public static Vector3 GetLookAtPoint(Camera cam)
        {
            var ray = new Ray(cam.transform.position, cam.transform.forward);

            return RaycastGroundPlane(ray);
        }

        public static Vector3 RaycastGroundPlane(Ray ray)
        {
            // this should be faster than using a physics system for this
            var groundPlane = new Plane(Vector3.up, Vector3.zero);
            groundPlane.Raycast(ray, out float distance);

            return ray.origin + ray.direction.normalized * distance;
        }

        public static Vector3 To3(this Vector2 v) => new Vector3(v.x, 0, v.y);
        public static Vector2 To2(this Vector3 v) => new Vector2(v.x, v.z);

        public static Vector2Int GetHoveredCell(HexGrid grid, Camera camera)
        {
            Vector3 mousePosOnGround = GetMousePositionOnGroundPlane(camera);
            Vector2Int hoveredCell = grid.Math.WorldToHex(mousePosOnGround);
            return hoveredCell;
        }
    }
}
