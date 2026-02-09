using System;
using UnityEngine;

namespace Shizounu.Library.Tweening
{
    /// <summary>
    /// Extension methods for tweening common types.
    /// Provides convenient high-level APIs for tweening floats, vectors, colors, and more.
    /// </summary>
    public static class TweenExtensions
    {
        #region Float Tweening

        /// <summary>
        /// Creates a tween that interpolates a float value.
        /// </summary>
        public static TweenBuilder TweenFloat(float duration, float from, float to, Action<float> onUpdate)
        {
            return Tweener.Create(duration)
                .OnUpdate(progress =>
                {
                    float value = Mathf.Lerp(from, to, progress);
                    onUpdate(value);
                });
        }

        /// <summary>
        /// Creates a tween that interpolates a float property.
        /// </summary>
        public static TweenBuilder TweenFloat(float duration, Func<float> getter, float to, Action<float> onValueChanged)
        {
            float from = getter();
            return TweenFloat(duration, from, to, onValueChanged);
        }

        #endregion

        #region Vector2 Tweening

        /// <summary>
        /// Creates a tween that interpolates a Vector2 value.
        /// </summary>
        public static TweenBuilder TweenVector2(float duration, Vector2 from, Vector2 to, Action<Vector2> onUpdate)
        {
            return Tweener.Create(duration)
                .OnUpdate(progress =>
                {
                    Vector2 value = Vector2.Lerp(from, to, progress);
                    onUpdate(value);
                });
        }

        /// <summary>
        /// Creates a tween that interpolates a Vector2 property.
        /// </summary>
        public static TweenBuilder TweenVector2(float duration, Func<Vector2> getter, Vector2 to, Action<Vector2> onValueChanged)
        {
            Vector2 from = getter();
            return TweenVector2(duration, from, to, onValueChanged);
        }

        #endregion

        #region Vector3 Tweening

        /// <summary>
        /// Creates a tween that interpolates a Vector3 value.
        /// </summary>
        public static TweenBuilder TweenVector3(float duration, Vector3 from, Vector3 to, Action<Vector3> onUpdate)
        {
            return Tweener.Create(duration)
                .OnUpdate(progress =>
                {
                    Vector3 value = Vector3.Lerp(from, to, progress);
                    onUpdate(value);
                });
        }

        /// <summary>
        /// Creates a tween that interpolates a Vector3 property.
        /// </summary>
        public static TweenBuilder TweenVector3(float duration, Func<Vector3> getter, Vector3 to, Action<Vector3> onValueChanged)
        {
            Vector3 from = getter();
            return TweenVector3(duration, from, to, onValueChanged);
        }

        #endregion

        #region Vector4/Quaternion Tweening

        /// <summary>
        /// Creates a tween that interpolates a Quaternion (rotation).
        /// </summary>
        public static TweenBuilder TweenQuaternion(float duration, Quaternion from, Quaternion to, Action<Quaternion> onUpdate)
        {
            return Tweener.Create(duration)
                .OnUpdate(progress =>
                {
                    Quaternion value = Quaternion.Lerp(from, to, progress);
                    onUpdate(value);
                });
        }

        /// <summary>
        /// Creates a tween that interpolates a Quaternion property.
        /// </summary>
        public static TweenBuilder TweenQuaternion(float duration, Func<Quaternion> getter, Quaternion to, Action<Quaternion> onValueChanged)
        {
            Quaternion from = getter();
            return TweenQuaternion(duration, from, to, onValueChanged);
        }

        #endregion

        #region Color Tweening

        /// <summary>
        /// Creates a tween that interpolates a Color value.
        /// </summary>
        public static TweenBuilder TweenColor(float duration, Color from, Color to, Action<Color> onUpdate)
        {
            return Tweener.Create(duration)
                .OnUpdate(progress =>
                {
                    Color value = Color.Lerp(from, to, progress);
                    onUpdate(value);
                });
        }

        /// <summary>
        /// Creates a tween that interpolates a Color property.
        /// </summary>
        public static TweenBuilder TweenColor(float duration, Func<Color> getter, Color to, Action<Color> onValueChanged)
        {
            Color from = getter();
            return TweenColor(duration, from, to, onValueChanged);
        }

        #endregion

        #region Transform Tweening

        /// <summary>
        /// Creates a tween that moves a transform's position.
        /// </summary>
        public static TweenBuilder TweenPosition(this Transform transform, float duration, Vector3 to, bool isLocal = false)
        {
            Vector3 from = isLocal ? transform.localPosition : transform.position;
            return Tweener.Create(duration)
                .OnUpdate(progress =>
                {
                    Vector3 value = Vector3.Lerp(from, to, progress);
                    if (isLocal)
                        transform.localPosition = value;
                    else
                        transform.position = value;
                });
        }

        /// <summary>
        /// Creates a tween that rotates a transform.
        /// </summary>
        public static TweenBuilder TweenRotation(this Transform transform, float duration, Quaternion to, bool isLocal = false)
        {
            Quaternion from = isLocal ? transform.localRotation : transform.rotation;
            return Tweener.Create(duration)
                .OnUpdate(progress =>
                {
                    Quaternion value = Quaternion.Lerp(from, to, progress);
                    if (isLocal)
                        transform.localRotation = value;
                    else
                        transform.rotation = value;
                });
        }

        /// <summary>
        /// Creates a tween that scales a transform.
        /// </summary>
        public static TweenBuilder TweenScale(this Transform transform, float duration, Vector3 to)
        {
            Vector3 from = transform.localScale;
            return Tweener.Create(duration)
                .OnUpdate(progress =>
                {
                    transform.localScale = Vector3.Lerp(from, to, progress);
                });
        }

        /// <summary>
        /// Creates a tween that scales a transform uniformly.
        /// </summary>
        public static TweenBuilder TweenScale(this Transform transform, float duration, float to)
        {
            return TweenScale(transform, duration, new Vector3(to, to, to));
        }

        #endregion

        #region RectTransform Tweening

        /// <summary>
        /// Creates a tween that moves a RectTransform.
        /// </summary>
        public static TweenBuilder TweenAnchoredPosition(this RectTransform rectTransform, float duration, Vector2 to)
        {
            Vector2 from = rectTransform.anchoredPosition;
            return Tweener.Create(duration)
                .OnUpdate(progress =>
                {
                    rectTransform.anchoredPosition = Vector2.Lerp(from, to, progress);
                });
        }

        /// <summary>
        /// Creates a tween that resizes a RectTransform.
        /// </summary>
        public static TweenBuilder TweenSizeDelta(this RectTransform rectTransform, float duration, Vector2 to)
        {
            Vector2 from = rectTransform.sizeDelta;
            return Tweener.Create(duration)
                .OnUpdate(progress =>
                {
                    rectTransform.sizeDelta = Vector2.Lerp(from, to, progress);
                });
        }

        #endregion

        #region CanvasGroup Tweening

        /// <summary>
        /// Creates a tween that fades a CanvasGroup.
        /// </summary>
        public static TweenBuilder TweenAlpha(this CanvasGroup canvasGroup, float duration, float to)
        {
            float from = canvasGroup.alpha;
            return Tweener.Create(duration)
                .OnUpdate(progress =>
                {
                    canvasGroup.alpha = Mathf.Lerp(from, to, progress);
                });
        }

        #endregion

        #region Renderer Tweening

        /// <summary>
        /// Creates a tween that changes a material's color.
        /// </summary>
        public static TweenBuilder TweenColor(this Renderer renderer, float duration, Color to, string propertyName = "_Color")
        {
            Material mat = renderer.material;
            Color from = mat.GetColor(propertyName);
            return Tweener.Create(duration)
                .OnUpdate(progress =>
                {
                    mat.SetColor(propertyName, Color.Lerp(from, to, progress));
                });
        }

        #endregion

        #region Action Tweening

        /// <summary>
        /// Creates a wait/delay tween that invokes a callback after the duration.
        /// </summary>
        public static Tween Wait(float duration, Action onComplete = null)
        {
            var tween = Tweener.Create(duration).Build();
            if (onComplete != null)
                tween.OnComplete += onComplete;
            return tween;
        }

        /// <summary>
        /// Creates a tween that runs a callback every frame with the progress value.
        /// </summary>
        public static Tween DoAction(float duration, Action<float> action)
        {
            return Tweener.Create(duration)
                .OnUpdate(action)
                .Build();
        }

        #endregion

        #region Fluent API Chainers

        /// <summary>
        /// Chains a tween to run after another tween completes.
        /// </summary>
        public static Tween Then(this Tween tween, Action callback)
        {
            tween.OnComplete += callback;
            return tween;
        }

        /// <summary>
        /// Chains a tween to run after another tween completes.
        /// </summary>
        public static Tween Then(this Tween tween, Tween nextTween)
        {
            tween.OnComplete += () => nextTween.Play();
            return nextTween;
        }

        /// <summary>
        /// Creates a parallel group of tweens starting from this tween.
        /// </summary>
        public static Tween With(this Tween tween, Tween otherTween)
        {
            if (!tween.IsPlaying)
                tween.Play();
            if (!otherTween.IsPlaying)
                otherTween.Play();
            return tween;
        }

        #endregion
    }
}
