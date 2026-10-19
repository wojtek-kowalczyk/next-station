using System.Collections.Generic;
using NS.Audio;
using NS.Core;
using NS.Core.Model;
using NS.UI;
using NS.Utility;
using NS.Visuals;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace NS
{
    public class GameplayInput : MonoBehaviour
    {
        [SerializeField] private InputActionReference panInputAction;
        [SerializeField] private InputActionReference rotateCameraInputAction;
        [SerializeField] private InputActionReference rotateTileInputAction;
        [SerializeField] private InputActionReference tilePlaceInputAction;
        [SerializeField] private bool useTactileOrbiting;
        [SerializeField] private float orbitMouseSpeed = 1f;
        [SerializeField] private float orbitButtonSpeed = 10f;

        [SerializeField] private float panSpeed = 0.5f;
        [SerializeField] private float zoomSpeed = 10f;
        [SerializeField] private float zoomLerpSpeed = 10f;
        [SerializeField] private float minZoomDistance = 5f;
        [SerializeField] private float maxZoomDistance = 50f;
        [SerializeField] private float defaultZoomDistance = 15f;

        [SerializeField] private TileStackVisuals tileStackVisuals;
        [SerializeField] private PreviewTile previewTile;

        private Camera cam;

        private Vector3 panPoint;
        private bool isMousePanning;
        private Vector3 orbitPoint;
        private bool isOrbiting;
        private float targetZoomDistance;
        private float currentZoomDistance;
        private int currentTileRotationSteps;
        private Vector2Int lastHoveredCell;

        private void Awake()
        {
            cam = Camera.main;
            previewTile.SetVisible(false);

            // position camera at appropriate zoom level
            currentZoomDistance = defaultZoomDistance;
            targetZoomDistance = defaultZoomDistance;
            SetCameraZoom(currentZoomDistance); // prevent lerp at game start
        }

        private void Update()
        {
            if (GUIDialog.CurrentlyVisibleDialog != null)
            {
                return;
            }

            if (Game.Instance.CurrentState == Game.State.GameOver)
            {
                previewTile.SetVisible(false);

                HandleOrbit();
                HandlePan();
                HandleZoom();
            }
            else if (Game.Instance.CurrentState == Game.State.Playing)
            {
                HandleCheats();
                HandleOrbit();
                HandlePan();
                HandleTileHover(); // updates preview tile
                HandleTileRotation(); // must happen after HandleTileHover because depends on preview tile
                HandleZoom(); // must happen after HandleTileHover because depends on preview tile
                HandleTilePlacement();
                HandleUndoRedo();
            }
        }

        private void HandleUndoRedo()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
            {
                Game.Instance.TryUndoTilePlacement();
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y))
            {
                Game.Instance.TryRedoTilePlacement();
            }
        }

        private void HandleCheats()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                var debugTileStack = Game.Instance.DebugTileStack;
                if (debugTileStack == null)
                {
                    return;
                }

                debugTileStack.enabled = !debugTileStack.enabled;
            }
        }

        private void HandleTilePlacement()
        {
            // TODO : tile stack case shows an interesting dilemma: you'd usually wanna access the model,
            //  but in case of tilestack it might sometimes be swapped out for a debug tilestack

            HexTileData tile = Game.Instance.TileStack.PeekTile();
            if (tile == null || !tilePlaceInputAction.action.WasPressedThisFrame())
            {
                return;
            }

            Vector3 worldPos = GameplayHelpers.GetMousePositionOnGroundPlane(cam);
            Vector2Int cell = Game.Instance.Grid.Math.WorldToHex(worldPos);

            var tpd = new TilePlacementData(tile, cell, currentTileRotationSteps, true);

            if (Game.Instance.CanPlaceTile(tpd) == false)
            {
                return;
            }

            Game.Instance.PlaceTile(tpd);

            AudioManager.Instance?.TryPlaySoundEffect(Sounds.TILE_PLACE);
        }

        private void HandleTileHover()
        {
            GameModel model = Game.Instance.Model;
            Vector2Int hoveredCell = GameplayHelpers.GetHoveredCell(model.Grid, cam);
            HexTileData nextTileToPlace = Game.Instance.TileStack.PeekTile();

            HashSet<Vector2Int> ghostTileCells = TileVisualsController.GetCellsForGhostTiles(model);

            bool shouldBeVisible = nextTileToPlace != null && ghostTileCells.Contains(hoveredCell);
            if (shouldBeVisible)
            {
                previewTile.SetupForTile(nextTileToPlace);

                Vector3 newPos = model.Grid.Math.HexToWorld(hoveredCell).WithY(Game.Instance.Config.Decorations.BaseYOffset);
                if (previewTile.IsVisible() == false)
                {
                    // if coming back from invisible, do not lerp, as that looks weird.
                    previewTile.SetPosition(newPos, true);
                }
                else
                {
                    previewTile.SetPosition(newPos);
                    var tpd = new TilePlacementData(nextTileToPlace, hoveredCell, currentTileRotationSteps, true);
                    previewTile.SetCanPlace(Game.Instance.GameMode.CanPlaceTile(model, tpd));
                }
            }

            previewTile.SetVisible(shouldBeVisible);

            if (lastHoveredCell != hoveredCell)
            {
                lastHoveredCell = hoveredCell;
                if (shouldBeVisible) // only play sound if we're seeing the preview tile, otherwise it's weird
                {
                    AudioManager.Instance?.TryPlaySoundEffect(Sounds.TILE_HOVER);
                }
            }
        }

        private void HandleOrbit()
        {
            float angleDelta = 0;

            if (useTactileOrbiting)
            {
                angleDelta = HandleOrbitTactile();
            }
            else if (Mouse.current.rightButton.isPressed)
            {
                angleDelta = -Mouse.current.delta.left.value + Mouse.current.delta.right.value;
                angleDelta *= orbitMouseSpeed * Time.deltaTime;
            }

            if (rotateCameraInputAction.action.IsPressed())
            {
                angleDelta = rotateCameraInputAction.action.ReadValue<float>();
                angleDelta *= orbitButtonSpeed * Time.deltaTime;
            }

            if (angleDelta == 0)
            {
                return;
            }

            cam.transform.RotateAround(GameplayHelpers.GetLookAtPoint(cam), Vector3.up, angleDelta);

            tileStackVisuals.Rotate(-angleDelta);
        }

        /// Handle orbiting the camera using drag on world plane logic. Some users may prefer this.
        private float HandleOrbitTactile()
        {
            if (Mouse.current.rightButton.isPressed == false)
            {
                isOrbiting = false;
                return 0.0f;
            }

            Vector3 raycastPos = GameplayHelpers.GetMousePositionOnGroundPlane(cam);

            if (isOrbiting == false)
            {
                isOrbiting = true;

                orbitPoint = raycastPos;
            }

            Vector3 lookAtPoint = GameplayHelpers.GetLookAtPoint(cam);

            Vector3 v0 = orbitPoint - lookAtPoint; // from look at to initial click point
            Vector3 vc = raycastPos - lookAtPoint; // from look at to current mouse point

            float angle = Vector3.SignedAngle(v0, vc, Vector3.up);

            return -angle;
        }

        private void HandlePan()
        {
            Vector3 movement = Vector2.zero;

            if (panInputAction.action.IsPressed())
            {
                var input = panInputAction.action.ReadValue<Vector2>();
                movement.x = input.x;
                movement.z = input.y;
                // rotate to match current camera rotation
                movement = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0) * movement;
                movement *= panSpeed * Time.deltaTime; // pan speed only affects button panning, not drag panning.
            }
            else
            {
                if (Mouse.current.middleButton.isPressed == false)
                {
                    isMousePanning = false;
                    return;
                }

                Vector3 raycastPos = GameplayHelpers.GetMousePositionOnGroundPlane(cam);

                if (isMousePanning == false)
                {
                    panPoint = raycastPos; // the point in 3d space that we always want under mouse
                    isMousePanning = true;
                }

                // We want to place the camera so that the panPoint is always under the mouse. This makes dragging feel natural.
                Vector3 toPanPoint = panPoint - raycastPos;
                movement = toPanPoint;
            }

            cam.transform.Translate(movement, Space.World);
        }

        private void HandleZoom()
        {
            // TODO : ugly hack : prevent zooming when cheat tile stack is enabled.
            // This should not require outside knowledge, input must be abstracted somehow later.
            if (Input.GetKey(KeyCode.LeftControl))
            {
                return;
            }

            // invert cuz it feels more natural.
            float zoomDelta = Mouse.current.scroll.ReadValue().y * -1f;

            if (Mathf.Abs(zoomDelta) > 0.01f && previewTile.IsVisible() == false)
            {
                // normalized distance from min to max zoom
                float n = (currentZoomDistance - minZoomDistance) / (maxZoomDistance - minZoomDistance);
                // magic zoom curve - makes zooming out faster when further away
                float mult = 1 + n * n * 2;

                // Note: DO NOT multiply with delta time here, since mouse wheel is a discrete input, not a continuous one.
                float newZoom = targetZoomDistance + zoomDelta * zoomSpeed * mult;
                targetZoomDistance = Mathf.Clamp(newZoom, minZoomDistance, maxZoomDistance);
            }

            // lerp zoom
            currentZoomDistance = Mathf.Lerp(currentZoomDistance, targetZoomDistance, Time.deltaTime * zoomLerpSpeed);
            SetCameraZoom(currentZoomDistance);
        }

        private void HandleTileRotation()
        {
            // TODO : ugly hack : prevent rotating tile when cheat tile stack is enabled.
            // This should not require outside knowledge, input must be abstracted somehow later.
            if (Input.GetKey(KeyCode.LeftControl))
            {
                return;
            }

            bool rotate = false;
            int direction = 1;

            if (GameplaySettingsManager.Instance?.Get(GameplaySettingsManager.Setting.ScrollToRotateTile) ?? true)
            {
                float zoomDelta = Mouse.current.scroll.ReadValue().y;
                rotate |= Mathf.Approximately(zoomDelta, 0) == false && previewTile.IsVisible();
                direction = zoomDelta > 0 ? 1 : -1;
            }

            rotate |= rotateTileInputAction.action.WasPerformedThisFrame();

            if (rotate == false)
            {
                return;
            }

            currentTileRotationSteps = (currentTileRotationSteps + direction) % 6;
            previewTile.RotateOneStep(direction);
            tileStackVisuals.Rotate(60.0f);

            Game.Instance.OnCurrentTileRotated(currentTileRotationSteps);

            AudioManager.Instance?.TryPlaySoundEffect(Sounds.TILE_ROTATE);
        }

        private void SetCameraZoom(float distance)
        {
            float clampedDistance = Mathf.Clamp(distance, minZoomDistance, maxZoomDistance);
            cam.transform.position = GameplayHelpers.GetLookAtPoint(cam) + -cam.transform.forward * clampedDistance;
        }
    }
}
