using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

public delegate void TweenEvent();

public enum PlaybackStates
{
    Begin,
    Playing,
    Paused,
    Completed,
    Cancelled,
}

public static class ComponentHelper
{
    private static Dictionary<string, PropertyInfo> propertyCache = new Dictionary<string, PropertyInfo>();

    public static object GetPropertyValue(GameObject gameObject, string propertyName)
    {
        // Check if the property is already cached
        if (propertyCache.TryGetValue(propertyName, out PropertyInfo cachedProperty))
        {
            return GetPropertyValueFromComponent(gameObject, cachedProperty);
        }

        // Loop through all components of the GameObject
        foreach (Component component in gameObject.GetComponents<Component>())
        {
            // Get the type of the component
            Type componentType = component.GetType();

            // Get the property by name using reflection
            PropertyInfo propertyInfo = componentType.GetProperty(propertyName);

            // If the property is found, cache it and return its value
            if (propertyInfo != null)
            {
                propertyCache[propertyName] = propertyInfo;
                return propertyInfo.GetValue(component);
            }
        }

        // If the property is not found, return null
        return null;
    }

    public static void SetPropertyValue(GameObject gameObject, string propertyName, object value)
    {
        // Check if the property is already cached
        if (propertyCache.TryGetValue(propertyName, out PropertyInfo cachedProperty))
        {
            SetPropertyValueToComponent(gameObject, cachedProperty, value);
            return;
        }

        // Loop through all components of the GameObject
        foreach (Component component in gameObject.GetComponents<Component>())
        {
            // Get the type of the component
            Type componentType = component.GetType();

            // Get the property by name using reflection
            PropertyInfo propertyInfo = componentType.GetProperty(propertyName);

            // If the property is found, cache it and set its value
            if (propertyInfo != null)
            {
                propertyCache[propertyName] = propertyInfo;
                propertyInfo.SetValue(component, value);
                return;
            }
        }
    }

    private static object GetPropertyValueFromComponent(GameObject gameObject, PropertyInfo propertyInfo)
    {
        foreach (Component component in gameObject.GetComponents<Component>())
        {
            if (propertyInfo.DeclaringType == component.GetType())
            {
                return propertyInfo.GetValue(component);
            }
        }
        return null;
    }

    private static void SetPropertyValueToComponent(GameObject gameObject, PropertyInfo propertyInfo, object value)
    {
        foreach (Component component in gameObject.GetComponents<Component>())
        {
            if (propertyInfo.DeclaringType == component.GetType())
            {
                propertyInfo.SetValue(component, value);
                return;
            }
        }
    }
}

public class any
{
    private object value;

    public any(object value)
    {
        this.value = value;
    }

    public T GetValue<T>()
    {
        return (T)value; // Cast to the requested type
    }

    public object Value => value; // For direct access if needed
}

public class TweenService : MonoBehaviour
{
    
    public class Tween
    {
        private enum UpdateFuncs {
            TweenTick,
            CompensateStartTime
        }

        private IEnumerator Update(UpdateFuncs func) {
            while (true)
            {
                if (func == UpdateFuncs.TweenTick) {
                    TweenTick();
                } else if (func == UpdateFuncs.CompensateStartTime) {
                    CompensateStartTime();
                }
                yield return null;
            }
        }

        public GameObject GameObject { get; private set; }
        public Func<float, float> CurveFunc { get; private set; }
        public float Duration { get; private set; }
        public Dictionary<string, any> Goal { get; private set; }
        public PlaybackStates PlaybackState { get; private set; }
        private float Percent = 0;
        private float StartTime;
        private Dictionary<string, any> StartGoal;
        private Coroutine TweenLoop;
        private Coroutine StartTimeCompensation;

        private Coroutine StartCoroutine(IEnumerator update) {
            return GameObject.GetComponent<TweenService>().StartCoroutine(update);
        }
        private void StopCoroutine(Coroutine update) {
            GameObject.GetComponent<TweenService>().StopCoroutine(update);
        }

        public event TweenEvent OnComplete;
        protected virtual void Complete() {
            OnComplete?.Invoke();
        }

        public void AddOnCompleteListener(TweenEvent listener) {
            OnComplete += listener;
        }

        private Dictionary<string, any> InitializeStartGoal(GameObject gameObject, Dictionary<string, any> goal) {
            var _StartGoal = new Dictionary<string, any>();
            foreach (var keyValuePair in goal) {
                string propertyName = keyValuePair.Key;
                object currentValue = ComponentHelper.GetPropertyValue(gameObject, propertyName);
                _StartGoal[propertyName] = new any(currentValue);
            }

            return _StartGoal;
        }

        public Tween(GameObject gameObject, Func<float, float> curveFunc, float duration, Dictionary<string, any> goal) {
            GameObject = gameObject;
            CurveFunc = curveFunc;
            Duration = duration;
            Goal = goal;
            StartGoal = InitializeStartGoal(GameObject, goal);
            PlaybackState = PlaybackStates.Begin;
        }

        private void TweenTick() {
            if (Percent >= 1) {
                Percent = 1;
                PlaybackState = PlaybackStates.Completed;
                StopCoroutine(TweenLoop);
                Complete();
            } else {
                Percent = Mathf.Clamp01((Time.realtimeSinceStartup - StartTime) / Duration);
                foreach (KeyValuePair<string, any> entry in Goal) {
                    string propertyName = entry.Key;
                    float startValue = Convert.ToSingle(StartGoal[propertyName].Value);
                    float goalValue = Convert.ToSingle(Goal[propertyName].Value);
                    ComponentHelper.SetPropertyValue(GameObject, propertyName, startValue + CurveFunc(Percent) * (goalValue - startValue));
                }
            }
        }
        private void CompensateStartTime() {
            Duration += Time.deltaTime;
        }

        public void Play() {
            if (PlaybackState == PlaybackStates.Playing) return;
            
            if (PlaybackState == PlaybackStates.Paused) {
                StopCoroutine(StartTimeCompensation);
            } else {
                StartTime = Time.realtimeSinceStartup;
                PlaybackState = PlaybackStates.Playing;
            }
            if (StartGoal == null) {
                StartGoal = InitializeStartGoal(GameObject, Goal);
            }

            PlaybackState = PlaybackStates.Playing;
            TweenLoop = StartCoroutine(Update(UpdateFuncs.TweenTick));
        }

        public void Cancel() {
            if (!(PlaybackState == PlaybackStates.Playing) && !(PlaybackState == PlaybackStates.Paused)) return;
            
            PlaybackState = PlaybackStates.Cancelled;
            StopCoroutine(TweenLoop);
            Percent = 0;
            StartGoal = null;
        }

        public void Pause() {
            if (!(PlaybackState == PlaybackStates.Playing)) return;

            PlaybackState = PlaybackStates.Paused;
            StopCoroutine(TweenLoop);
            StartTimeCompensation = StartCoroutine(Update(UpdateFuncs.CompensateStartTime));
        }
    }

    public Tween Create(GameObject gameObject, Func<float, float> curveFunc, float duration, Dictionary<string, any> goal)
    {
        Tween tween = new Tween(gameObject, curveFunc, duration, goal);
        return tween;
    }
}