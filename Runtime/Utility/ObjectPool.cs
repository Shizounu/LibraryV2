using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shizounu.Library.Utility
{
    /// <summary>
    /// Generic object pool for reusing objects to reduce allocation overhead.
    /// </summary>
    /// <typeparam name="T">Type of objects to pool</typeparam>
    public class ObjectPool<T> where T : class
    {
        private readonly Stack<T> _pool;
        private readonly Func<T> _createFunc;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly Action<T> _onDestroy;
        private readonly int _maxSize;
        private readonly bool _collectionCheck;

        public int CountAll { get; private set; }
        public int CountActive => CountAll - CountInactive;
        public int CountInactive => _pool.Count;

        /// <summary>
        /// Creates a new object pool.
        /// </summary>
        /// <param name="createFunc">Function to create new instances</param>
        /// <param name="onGet">Action called when getting an object from the pool</param>
        /// <param name="onRelease">Action called when returning an object to the pool</param>
        /// <param name="onDestroy">Action called when destroying an object</param>
        /// <param name="collectionCheck">Check if object is already in pool when releasing</param>
        /// <param name="defaultCapacity">Initial capacity of the pool</param>
        /// <param name="maxSize">Maximum size of the pool (0 = unlimited)</param>
        public ObjectPool(
            Func<T> createFunc,
            Action<T> onGet = null,
            Action<T> onRelease = null,
            Action<T> onDestroy = null,
            bool collectionCheck = true,
            int defaultCapacity = 10,
            int maxSize = 100)
        {
            if (createFunc == null)
                throw new ArgumentNullException(nameof(createFunc));

            _pool = new Stack<T>(defaultCapacity);
            _createFunc = createFunc;
            _onGet = onGet;
            _onRelease = onRelease;
            _onDestroy = onDestroy;
            _maxSize = maxSize;
            _collectionCheck = collectionCheck;
            CountAll = 0;
        }

        /// <summary>
        /// Get an object from the pool or create a new one if pool is empty.
        /// </summary>
        public T Get()
        {
            T item;
            if (_pool.Count == 0)
            {
                item = _createFunc();
                CountAll++;
            }
            else
            {
                item = _pool.Pop();
            }

            _onGet?.Invoke(item);
            return item;
        }

        /// <summary>
        /// Return an object to the pool.
        /// </summary>
        public void Release(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (_collectionCheck && _pool.Contains(item))
            {
                Debug.LogError("Trying to release an object that is already in the pool.");
                return;
            }

            _onRelease?.Invoke(item);

            if (_maxSize > 0 && _pool.Count >= _maxSize)
            {
                _onDestroy?.Invoke(item);
                CountAll--;
            }
            else
            {
                _pool.Push(item);
            }
        }

        /// <summary>
        /// Clear the pool and destroy all pooled objects.
        /// </summary>
        public void Clear()
        {
            if (_onDestroy != null)
            {
                foreach (var item in _pool)
                {
                    _onDestroy(item);
                }
            }

            _pool.Clear();
            CountAll = 0;
        }

        /// <summary>
        /// Prewarm the pool by creating initial objects.
        /// </summary>
        /// <param name="count">Number of objects to create</param>
        public void Prewarm(int count)
        {
            if (count <= 0)
                return;

            var items = new T[count];
            for (int i = 0; i < count; i++)
            {
                items[i] = Get();
            }

            for (int i = 0; i < count; i++)
            {
                Release(items[i]);
            }
        }
    }

    /// <summary>
    /// Object pool for Unity GameObjects.
    /// </summary>
    public class GameObjectPool
    {
        private readonly ObjectPool<GameObject> _pool;
        private readonly GameObject _prefab;
        private readonly Transform _parent;

        public int CountAll => _pool.CountAll;
        public int CountActive => _pool.CountActive;
        public int CountInactive => _pool.CountInactive;

        /// <summary>
        /// Creates a new GameObject pool.
        /// </summary>
        /// <param name="prefab">Prefab to instantiate</param>
        /// <param name="parent">Parent transform for pooled objects</param>
        /// <param name="defaultCapacity">Initial capacity</param>
        /// <param name="maxSize">Maximum size (0 = unlimited)</param>
        public GameObjectPool(
            GameObject prefab,
            Transform parent = null,
            int defaultCapacity = 10,
            int maxSize = 100)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            _prefab = prefab;
            _parent = parent;

            _pool = new ObjectPool<GameObject>(
                createFunc: CreateGameObject,
                onGet: OnGetGameObject,
                onRelease: OnReleaseGameObject,
                onDestroy: OnDestroyGameObject,
                collectionCheck: true,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
        }

        private GameObject CreateGameObject()
        {
            var obj = UnityEngine.Object.Instantiate(_prefab, _parent);
            obj.name = _prefab.name;
            return obj;
        }

        private void OnGetGameObject(GameObject obj)
        {
            obj.SetActive(true);
        }

        private void OnReleaseGameObject(GameObject obj)
        {
            obj.SetActive(false);
        }

        private void OnDestroyGameObject(GameObject obj)
        {
            UnityEngine.Object.Destroy(obj);
        }

        /// <summary>
        /// Get a GameObject from the pool.
        /// </summary>
        public GameObject Get()
        {
            return _pool.Get();
        }

        /// <summary>
        /// Get a GameObject from the pool at a specific position and rotation.
        /// </summary>
        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            var obj = _pool.Get();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            return obj;
        }

        /// <summary>
        /// Return a GameObject to the pool.
        /// </summary>
        public void Release(GameObject obj)
        {
            _pool.Release(obj);
        }

        /// <summary>
        /// Clear the pool and destroy all objects.
        /// </summary>
        public void Clear()
        {
            _pool.Clear();
        }

        /// <summary>
        /// Prewarm the pool with initial objects.
        /// </summary>
        public void Prewarm(int count)
        {
            _pool.Prewarm(count);
        }
    }

    /// <summary>
    /// Object pool for Unity Components.
    /// </summary>
    /// <typeparam name="T">Component type</typeparam>
    public class ComponentPool<T> where T : Component
    {
        private readonly ObjectPool<T> _pool;
        private readonly T _prefab;
        private readonly Transform _parent;

        public int CountAll => _pool.CountAll;
        public int CountActive => _pool.CountActive;
        public int CountInactive => _pool.CountInactive;

        /// <summary>
        /// Creates a new Component pool.
        /// </summary>
        /// <param name="prefab">Component prefab to instantiate</param>
        /// <param name="parent">Parent transform for pooled objects</param>
        /// <param name="defaultCapacity">Initial capacity</param>
        /// <param name="maxSize">Maximum size (0 = unlimited)</param>
        public ComponentPool(
            T prefab,
            Transform parent = null,
            int defaultCapacity = 10,
            int maxSize = 100)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            _prefab = prefab;
            _parent = parent;

            _pool = new ObjectPool<T>(
                createFunc: CreateComponent,
                onGet: OnGetComponent,
                onRelease: OnReleaseComponent,
                onDestroy: OnDestroyComponent,
                collectionCheck: true,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
        }

        private T CreateComponent()
        {
            var component = UnityEngine.Object.Instantiate(_prefab, _parent);
            component.gameObject.name = _prefab.gameObject.name;
            return component;
        }

        private void OnGetComponent(T component)
        {
            component.gameObject.SetActive(true);
        }

        private void OnReleaseComponent(T component)
        {
            component.gameObject.SetActive(false);
        }

        private void OnDestroyComponent(T component)
        {
            UnityEngine.Object.Destroy(component.gameObject);
        }

        /// <summary>
        /// Get a component from the pool.
        /// </summary>
        public T Get()
        {
            return _pool.Get();
        }

        /// <summary>
        /// Get a component from the pool at a specific position and rotation.
        /// </summary>
        public T Get(Vector3 position, Quaternion rotation)
        {
            var component = _pool.Get();
            component.transform.position = position;
            component.transform.rotation = rotation;
            return component;
        }

        /// <summary>
        /// Return a component to the pool.
        /// </summary>
        public void Release(T component)
        {
            _pool.Release(component);
        }

        /// <summary>
        /// Clear the pool and destroy all objects.
        /// </summary>
        public void Clear()
        {
            _pool.Clear();
        }

        /// <summary>
        /// Prewarm the pool with initial objects.
        /// </summary>
        public void Prewarm(int count)
        {
            _pool.Prewarm(count);
        }
    }
}
