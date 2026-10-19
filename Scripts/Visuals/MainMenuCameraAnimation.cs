using UnityEngine;

namespace NS.Visuals
{
    public class MainMenuCameraAnimation : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private float rotationSpeed;

        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private void Start()
        {
            Vector3 camLookAtPoint = GameplayHelpers.GetLookAtPoint(Camera.main);
            Vector3 targetPoint = Game.Instance.Model.Grid.GetTilesCenterWorld();
            Vector3 offset = targetPoint - camLookAtPoint;
            offset.y = 0;

            cam.transform.Translate(offset, Space.World);
        }

        private void Update()
        {
            if (Game.Instance?.CurrentState != Game.State.MainMenu)
            {
                return;
            }

            Vector3 lookAtPoint = GameplayHelpers.GetLookAtPoint(cam);
            cam.transform.RotateAround(lookAtPoint, Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}
