using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animation
{
    public enum Easing
    {
        Linear,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseOutElastic,
        EaseOutBounce
    }
    
    public class AnimationManager : MonoBehaviour
{
    private static AnimationManager _instance;
    
    private readonly List<ITween> _tweens = new();
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(_instance);
        }
        else Destroy(gameObject);
    }

    private interface ITween
    {
        Object Target { get; }
        bool IsDirty { get; set; }
        void Update();
    }

    public class Tween<T> : ITween
    {
        public Object Target { get; }
        private readonly Action<T> _setter;
        private readonly T _startValue;
        private readonly T _endValue;
        
        private readonly float _startTime;
        private readonly float _duration;
        private readonly Easing _easing;
        private readonly bool _scaled;

        public bool IsDirty { get; set; }

        public Tween(Object target, Action<T> setter, T startValue, T endValue, float duration, Easing easing, bool scaled = true)
        {
            Target = target;
            _setter = setter;
            _startValue = startValue;
            _endValue = endValue;
            _setter(startValue);
            
            _startTime = scaled ? Time.time : Time.unscaledTime;
            _duration = duration;
            _easing = easing;
            _scaled = scaled;

            IsDirty = false;
            _instance._tweens.Add(this);
        }
        
        private float ElapsedTime => (_scaled ? Time.time : Time.unscaledTime) - _startTime;

        public void Update()
        {
            var t = Mathf.Clamp01(ElapsedTime / _duration);
            _setter(Lerp(_startValue, _endValue, EasingT(t, _easing)));
            if (t >= 1) IsDirty = true;
        }
        
        private static T Lerp(T a, T b, float t)
        {
            return a switch
            {
                float af when b is float bf => (T)(object)Mathf.Lerp(af, bf, t),
                Vector2 av2 when b is Vector2 bv2 => (T)(object)Vector2.Lerp(av2, bv2, t),
                Vector3 av3 when b is Vector3 bv3 => (T)(object)Vector3.Lerp(av3, bv3, t),
                Quaternion aq when b is Quaternion bq => (T)(object)Quaternion.Slerp(aq, bq, t),
                Color ac when b is Color bc => (T)(object)Color.Lerp(ac, bc, t),
                _ => throw new InvalidOperationException($"Unsupported type for tweening: {typeof(T)}")
            };
        }
    }

    public static Tween<T> CreateTween<T>(Object target, Action<T> setter, T startValue, T endValue, float duration,
        Easing easing, bool scaled = true)
    {
        return new Tween<T>(target, setter, startValue, endValue, duration, easing, scaled);
    }

    public static bool HasTween(Object target)
    {
        return _instance._tweens.Any(tween => tween.Target == target);
    }

    public static List<Tween<T>> GetTweens<T>(Object target)
    {
        return _instance._tweens.Where(tween => tween.Target == target).Cast<Tween<T>>().ToList();
    }
    
    public static void RemoveTweens(Object target)
    {
        _instance._tweens.Where(tween => tween.Target == target).ToList().ForEach(tween => tween.IsDirty = true);
    }

    public void Update()
    {
        for (int i = _tweens.Count - 1; i >= 0; i--)
        {
            _tweens[i].Update();
            if (_tweens[i].IsDirty) _tweens.RemoveAt(i);
        }
    }

    private static float EasingT(float t, Easing easing) => easing switch
    {
        Easing.EaseInCubic => Mathf.Pow(t, 3),
        Easing.EaseOutCubic => 1 - Mathf.Pow(1 - t, 3),
        Easing.EaseInOutCubic => t < 0.5f ? 4 * Mathf.Pow(t, 3) : 1 - Mathf.Pow(-2 * t + 2, 3) / 2,
        Easing.EaseOutElastic => t switch
        {
            0 => 0,
            1 => 1,
            _ => Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * (2 * Mathf.PI) / 3) + 1
        },
        Easing.EaseOutBounce => t switch
        {
            _ when t < 1 / 2.75f => 7.5625f * t * t,
            _ when t < 2 / 2.75f => 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f,
            _ when t < 2.5f / 2.75f => 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f,
            _ => 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f
        },
        _ => t
    };
}

}