using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace NS.UI.HUD
{
    public class GUIStationAwardProgressBar : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private float wrapAroundCoroutineWaitTime = 1.0f;

        private Tween tween1;
        private Tween tween2;

        private VisualElement fillEager;
        private VisualElement fillDelayed;
        private float lastSetProgress01;
        private Coroutine onWrapAroundCoroutine;

        private void Awake()
        {
            fillEager = document.rootVisualElement.Q("FillEager");
            fillDelayed = document.rootVisualElement.Q("FillDelayed");
        }

        private void Start()
        {
            SetSliders(Game.Instance.GameMode.StationSpawner.CurrentProgress01, true);
        }

        private void OnEnable()
        {
            Game.Instance.GameMode.StationSpawner.StationAwarded += OnStationAwarded;
            Game.Instance.GameMode.StationSpawner.ProgressChanged += OnStationUnlockProgressChanged;
        }

        private void OnDisable()
        {
            Game.Instance.GameMode.StationSpawner.StationAwarded -= OnStationAwarded;
            Game.Instance.GameMode.StationSpawner.ProgressChanged -= OnStationUnlockProgressChanged;
        }

        private void OnStationAwarded()
        {
            tween1.Kill();
            tween2.Kill();
        }

        private void OnStationUnlockProgressChanged(float newProgress01, bool wrappedAround, bool becauseOfPlacement)
        {
            if (Mathf.Approximately(lastSetProgress01, newProgress01))
            {
                return;
            }

            lastSetProgress01 = newProgress01;

            if (becauseOfPlacement == false)
            {
                // game load, undo, etc.
                SetSliders(newProgress01, immediately: true);
                return;
            }

            if (wrappedAround)
            {
                if (onWrapAroundCoroutine != null)
                {
                    StopCoroutine(onWrapAroundCoroutine);
                }

                onWrapAroundCoroutine = StartCoroutine(GoToEndThenToDestinationValue(newProgress01));
            }
            else
            {
                SetSliders(newProgress01);
            }
        }

        private IEnumerator GoToEndThenToDestinationValue(float destination01)
        {
            SetSliders(1.0f);

            yield return new WaitForSeconds(wrapAroundCoroutineWaitTime);

            SetSliders(0, immediately: true);
            SetSliders(destination01);

            onWrapAroundCoroutine = null;
        }

        private void SetSliders(float value01, bool immediately = false)
        {
            SliderSetValue(fillEager, value01, immediately);
            SliderSetValue(fillDelayed, value01, immediately);
        }

        private static void SliderSetValue(VisualElement fill, float value01, bool immediately)
        {
            if (!immediately)
            {
                // uss will animate
                SetWidth();
            }
            else
            {
                // disable animation duration
                StyleList<TimeValue> originalDuration = fill.style.transitionDuration;
                StyleList<TimeValue> originalDelay = fill.style.transitionDelay;
                List<TimeValue> list = new();
                TimeValue entry = new()
                {
                    value = 0
                };
                list.Add(entry);
                fill.style.transitionDuration = list;
                fill.style.transitionDelay = list;

                // set immediately
                SetWidth();

                // bring back animation duration
                fill.style.transitionDuration = originalDuration;
                fill.style.transitionDelay = originalDelay;
            }

            return;

            void SetWidth()
            {
                fill.style.width = new StyleLength(new Length(value01 * 100, LengthUnit.Percent));
            }
        }
    }
}
