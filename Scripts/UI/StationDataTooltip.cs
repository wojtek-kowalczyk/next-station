using System;
using DG.Tweening;
using NS.Core;
using NS.Utility;
using TMPro;
using UnityEngine;

namespace NS.UI
{
    public class StationDataTooltip : MonoBehaviour
    {
        [SerializeField] private TMP_Text wantsText;
        [SerializeField] private TMP_Text givesText;
        [SerializeField] private Color steelColor = Color.white;
        [SerializeField] private Color woodColor = Color.white;
        [SerializeField] private Color peopleColor = Color.white;
        [SerializeField] private Color goldColor = Color.white;

        private Tween wantsAnimation;
        private Tween givesAnimation;

        public StationData StationData { get; private set; }

        public void Refresh(StationData data, bool animate = false)
        {
            if (StationData.Equals(data))
            {
                return;
            }

            wantsAnimation.Kill(complete: true);
            givesAnimation.Kill(complete: true);
            ResetAnimatableState(wantsText);
            ResetAnimatableState(givesText);

            if (animate == false)
            {
                SetTexts(data);
            }
            else
            {
                AnimateUpdate(data);
            }

            StationData = data;
        }

        public void SetPosition(Vector3 worldPos)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            transform.position = screenPos;
        }

        private void SetTexts(StationData data)
        {
            SetWantsText(data);
            SetGivesText(data);
        }

        private void SetGivesText(StationData data)
        {
            if (data.givesResourceAmount > 0)
            {
                givesText.gameObject.SetActive(true);
                givesText.text = $"gives: {data.givesResourceAmount} {GetColoredResourceString(data.givesResource)}";
            }
            else
            {
                givesText.gameObject.SetActive(false);
            }
        }

        private void SetWantsText(StationData data)
        {
            if (data.wantsResourceAmount > 0)
            {
                wantsText.gameObject.SetActive(true);
                wantsText.text = $"wants: {data.wantsResourceAmount} {GetColoredResourceString(data.wantsResource)}";
            }
            else
            {
                wantsText.gameObject.SetActive(false);
            }
        }

        private void AnimateUpdate(StationData newData)
        {
            if (StationData.wantsResourceAmount != newData.wantsResourceAmount)
            {
                wantsAnimation = Animate(wantsText, SetWantsText, newData.wantsResourceAmount);
            }

            if (StationData.givesResourceAmount != newData.givesResourceAmount)
            {
                givesAnimation = Animate(givesText, SetGivesText, newData.givesResourceAmount);
            }

            StationData = newData;

            return;

            Sequence Animate(TMP_Text text, Action<StationData> updateFunc, int newAmount)
            {
                const float timeStep = 0.33f;

                Sequence s = DOTween.Sequence();
                if (newAmount > 0)
                {
                    s.Insert(timeStep * 0.0f, text.transform.DOScale(Vector3.one * 1.7f, timeStep).SetEase(Ease.OutQuad));
                    // wait 1
                    s.InsertCallback(timeStep * 2.0f, () => updateFunc(newData));
                    s.Insert(timeStep * 2.0f, text.transform.DOScale(Vector3.one * 2.7f, timeStep * 0.5f).SetEase(Ease.OutQuad));
                    s.Insert(timeStep * 2.5f, text.transform.DOScale(Vector3.one * 1.7f, timeStep * 0.5f).SetEase(Ease.InQuad));
                    // wait 1
                    s.Insert(timeStep * 4.0f, text.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutQuad));
                }
                else
                {
                    s.Insert(timeStep * 0.0f, text.transform.DOScale(Vector3.one * 1.7f, timeStep).SetEase(Ease.OutQuad));
                    // wait 2
                    s.Insert(timeStep * 2.0f, text.transform.DOScale(Vector3.one * 3.0f, timeStep * 2.0f).SetEase(Ease.OutQuad));
                    s.Insert(timeStep * 2.0f, text.DOAlpha(0, timeStep * 2.0f).SetEase(Ease.OutQuad));
                    s.InsertCallback(timeStep * 5.0f, () => updateFunc(newData));
                }

                return s;
            }
        }

        private static void ResetAnimatableState(TMP_Text text)
        {
            text.transform.localScale = Vector3.one;
            text.alpha = 1.0f;
        }

        private string GetColoredResourceString(ResourceType resourceType)
        {
            return GetColoredString(resourceType.ToString(), GetColor(resourceType));
        }

        private string GetColoredString(string str, Color color)
        {
            string hex = ColorUtility.ToHtmlStringRGBA(color);
            return $"<color=#{hex}>{str}</color>";
        }

        private Color GetColor(ResourceType resourceType)
        {
            switch (resourceType)
            {
                case ResourceType.People:
                    return peopleColor;
                case ResourceType.Wood:
                    return woodColor;
                case ResourceType.Steel:
                    return steelColor;
                case ResourceType.Gold:
                    return goldColor;

                case ResourceType.None:
                case ResourceType.All:
                default:
                    throw new ArgumentOutOfRangeException(nameof(resourceType), resourceType, null);
            }
        }
    }
}
