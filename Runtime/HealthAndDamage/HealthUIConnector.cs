using UnityEngine;
using UnityEngine.UI;
using Shizounu.Library.ScriptableArchitecture;

namespace Shizounu.Library.HealthAndDamage
{
    /// <summary>
    /// UI connector that automatically updates UI elements based on health scriptable variables.
    /// This allows complete decoupling of UI from health logic through scriptable objects.
    /// 
    /// Usage:
    /// 1. Create ScriptableFloat/ScriptableBool assets for health values
    /// 2. Assign them to the HealthComponent scriptable variable fields
    /// 3. Add this component to your UI canvas/panel
    /// 4. Assign UI elements and the scriptable variables to this component
    /// </summary>
    public class HealthUIConnector : MonoBehaviour
    {
        [Header("Health Display")]
        [SerializeField]
        [Tooltip("Text element for current health (e.g., '50/100')")]
        private Text _healthText;

        [SerializeField]
        [Tooltip("Slider/Image for health bar")]
        private Image _healthBarImage;

        [SerializeField]
        [Tooltip("Text element for health percentage (e.g., '50%')")]
        private Text _healthPercentText;

        [Header("Status Display")]
        [SerializeField]
        [Tooltip("Text element for status (e.g., 'ALIVE', 'DEAD', 'LOW HEALTH')")]
        private Text _statusText;

        [SerializeField]
        [Tooltip("Slider component for health bar (alternative to Image)")]
        private Slider _healthSlider;

        [Header("Scriptable Variables")]
        [SerializeField]
        [Tooltip("ScriptableFloat for current health")]
        private ScriptableFloat _currentHealthVariable;

        [SerializeField]
        [Tooltip("ScriptableFloat for max health")]
        private ScriptableFloat _maxHealthVariable;

        [SerializeField]
        [Tooltip("ScriptableFloat for health percentage (0-1)")]
        private ScriptableFloat _healthPercentVariable;

        [SerializeField]
        [Tooltip("ScriptableBool for dead state")]
        private ScriptableBool _isDeadVariable;

        [Header("Styling")]
        [SerializeField]
        [Tooltip("Color for normal health (> 50%)")]
        private Color _normalHealthColor = Color.green;

        [SerializeField]
        [Tooltip("Color for low health (25-50%)")]
        private Color _lowHealthColor = Color.yellow;

        [SerializeField]
        [Tooltip("Color for critical health (< 25%)")]
        private Color _criticalHealthColor = Color.red;

        [SerializeField]
        [Tooltip("Color for dead state")]
        private Color _deadColor = new Color(0.5f, 0.5f, 0.5f);

        private void OnEnable()
        {
            // Subscribe to scriptable variable changes
            if (_currentHealthVariable != null && _currentHealthVariable.OnRuntimeValueChange != null)
            {
                _currentHealthVariable.OnRuntimeValueChange.RegisterListener(OnHealthChanged);
            }

            if (_healthPercentVariable != null && _healthPercentVariable.OnRuntimeValueChange != null)
            {
                _healthPercentVariable.OnRuntimeValueChange.RegisterListener(OnHealthChanged);
            }

            if (_isDeadVariable != null && _isDeadVariable.OnRuntimeValueChange != null)
            {
                _isDeadVariable.OnRuntimeValueChange.RegisterListener(OnHealthChanged);
            }

            // Initial update
            OnHealthChanged();
        }

        private void OnDisable()
        {
            // Unsubscribe from scriptable variable changes
            if (_currentHealthVariable != null && _currentHealthVariable.OnRuntimeValueChange != null)
            {
                _currentHealthVariable.OnRuntimeValueChange.RemoveListener(OnHealthChanged);
            }

            if (_healthPercentVariable != null && _healthPercentVariable.OnRuntimeValueChange != null)
            {
                _healthPercentVariable.OnRuntimeValueChange.RemoveListener(OnHealthChanged);
            }

            if (_isDeadVariable != null && _isDeadVariable.OnRuntimeValueChange != null)
            {
                _isDeadVariable.OnRuntimeValueChange.RemoveListener(OnHealthChanged);
            }
        }

        /// <summary>
        /// Called whenever any health value changes, updates all UI elements.
        /// </summary>
        private void OnHealthChanged()
        {
            if (_currentHealthVariable == null || _maxHealthVariable == null || _healthPercentVariable == null)
            {
                Debug.LogWarning("HealthUIConnector is missing required scriptable variables", this);
                return;
            }

            float currentHealth = _currentHealthVariable.RuntimeValue;
            float maxHealth = _maxHealthVariable.RuntimeValue;
            float healthPercent = _healthPercentVariable.RuntimeValue;
            bool isDead = _isDeadVariable != null && _isDeadVariable.RuntimeValue;

            // Update health text (e.g., "50/100")
            if (_healthText != null)
            {
                _healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
            }

            // Update health percentage text (e.g., "50%")
            if (_healthPercentText != null)
            {
                _healthPercentText.text = $"{healthPercent * 100:F0}%";
            }

            // Update health bar (Image fill amount)
            if (_healthBarImage != null)
            {
                _healthBarImage.fillAmount = Mathf.Clamp01(healthPercent);
                _healthBarImage.color = GetHealthColor(healthPercent, isDead);
            }

            // Update health slider
            if (_healthSlider != null)
            {
                _healthSlider.value = Mathf.Clamp01(healthPercent);
            }

            // Update status text
            if (_statusText != null)
            {
                if (isDead)
                {
                    _statusText.text = "DEAD";
                    _statusText.color = _deadColor;
                }
                else if (healthPercent < 0.25f)
                {
                    _statusText.text = "CRITICAL";
                    _statusText.color = _criticalHealthColor;
                }
                else if (healthPercent < 0.5f)
                {
                    _statusText.text = "LOW HEALTH";
                    _statusText.color = _lowHealthColor;
                }
                else
                {
                    _statusText.text = "HEALTHY";
                    _statusText.color = _normalHealthColor;
                }
            }
        }

        /// <summary>
        /// Get the color for the health bar based on health percentage and state.
        /// </summary>
        private Color GetHealthColor(float healthPercent, bool isDead)
        {
            if (isDead)
                return _deadColor;

            if (healthPercent < 0.25f)
                return _criticalHealthColor;

            if (healthPercent < 0.5f)
                return _lowHealthColor;

            return _normalHealthColor;
        }

        /// <summary>
        /// Set all scriptable variable references at once.
        /// Useful for programmatic setup.
        /// </summary>
        public void SetScriptableVariables(ScriptableFloat currentHealth, ScriptableFloat maxHealth, 
            ScriptableFloat healthPercent, ScriptableBool isDead)
        {
            _currentHealthVariable = currentHealth;
            _maxHealthVariable = maxHealth;
            _healthPercentVariable = healthPercent;
            _isDeadVariable = isDead;

            if (gameObject.activeInHierarchy)
            {
                OnHealthChanged();
            }
        }

        /// <summary>
        /// Set UI element references at once.
        /// Useful for programmatic setup.
        /// </summary>
        public void SetUIElements(Text healthText, Image healthBarImage, Text healthPercentText, Text statusText, Slider healthSlider)
        {
            _healthText = healthText;
            _healthBarImage = healthBarImage;
            _healthPercentText = healthPercentText;
            _statusText = statusText;
            _healthSlider = healthSlider;
        }

        /// <summary>
        /// Set the colors used for health visualization.
        /// </summary>
        public void SetHealthColors(Color normal, Color low, Color critical, Color dead)
        {
            _normalHealthColor = normal;
            _lowHealthColor = low;
            _criticalHealthColor = critical;
            _deadColor = dead;
        }
    }

    /// <summary>
    /// Alternative simple health bar that only updates based on health percent.
    /// Useful for lightweight UI needs without text or complex styling.
    /// </summary>
    public class SimpleHealthBar : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The image component to fill (health bar visual)")]
        private Image _healthBarImage;

        [SerializeField]
        [Tooltip("ScriptableFloat representing health percentage (0-1)")]
        private ScriptableFloat _healthPercentVariable;

        [SerializeField]
        [Tooltip("Color gradient from critical to healthy")]
        private Gradient _healthGradient;

        private void OnEnable()
        {
            if (_healthPercentVariable != null && _healthPercentVariable.OnRuntimeValueChange != null)
            {
                _healthPercentVariable.OnRuntimeValueChange.RegisterListener(UpdateHealthBar);
            }
            UpdateHealthBar();
        }

        private void OnDisable()
        {
            if (_healthPercentVariable != null && _healthPercentVariable.OnRuntimeValueChange != null)
            {
                _healthPercentVariable.OnRuntimeValueChange.RemoveListener(UpdateHealthBar);
            }
        }

        private void UpdateHealthBar()
        {
            if (_healthPercentVariable == null || _healthBarImage == null)
                return;

            float healthPercent = Mathf.Clamp01(_healthPercentVariable.RuntimeValue);
            _healthBarImage.fillAmount = healthPercent;

            if (_healthGradient != null)
            {
                _healthBarImage.color = _healthGradient.Evaluate(healthPercent);
            }
        }
    }

    /// <summary>
    /// Display damage numbers floating above the entity.
    /// Listens to health changes via scriptable variables.
    /// </summary>
    public class DamageNumberDisplay : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Prefab for floating damage text")]
        private Text _damageNumberPrefab;

        [SerializeField]
        [Tooltip("ScriptableFloat for current health")]
        private ScriptableFloat _currentHealthVariable;

        [SerializeField]
        [Tooltip("Canvas to spawn damage numbers in")]
        private Canvas _canvas;

        [SerializeField]
        [Tooltip("World position to spawn damage numbers from")]
        private Transform _spawnPoint;

        [SerializeField]
        [Tooltip("Duration the damage number stays visible")]
        private float _displayDuration = 1f;

        [SerializeField]
        [Tooltip("Float speed of damage numbers")]
        private float _floatSpeed = 50f;

        private float _lastDisplayedHealth;

        private void OnEnable()
        {
            if (_currentHealthVariable != null && _currentHealthVariable.OnRuntimeValueChange != null)
            {
                _currentHealthVariable.OnRuntimeValueChange.RegisterListener(OnHealthChanged);
            }

            if (_currentHealthVariable != null)
            {
                _lastDisplayedHealth = _currentHealthVariable.RuntimeValue;
            }
        }

        private void OnDisable()
        {
            if (_currentHealthVariable != null && _currentHealthVariable.OnRuntimeValueChange != null)
            {
                _currentHealthVariable.OnRuntimeValueChange.RemoveListener(OnHealthChanged);
            }
        }

        private void OnHealthChanged()
        {
            if (_currentHealthVariable == null || _damageNumberPrefab == null)
                return;

            float currentHealth = _currentHealthVariable.RuntimeValue;
            float damage = _lastDisplayedHealth - currentHealth;

            if (damage > 0)
            {
                DisplayDamageNumber(damage);
            }

            _lastDisplayedHealth = currentHealth;
        }

        private void DisplayDamageNumber(float damage)
        {
            if (_canvas == null)
                return;

            Text damageText = Instantiate(_damageNumberPrefab, _canvas.transform);
            damageText.text = $"-{damage:F0}";
            damageText.color = Color.red;

            if (_spawnPoint != null)
            {
                damageText.rectTransform.position = _spawnPoint.position;
            }

            StartCoroutine(FloatAndDestroy(damageText.gameObject));
        }

        private System.Collections.IEnumerator FloatAndDestroy(GameObject damageNumber)
        {
            float elapsed = 0f;
            RectTransform rectTransform = damageNumber.GetComponent<RectTransform>();

            while (elapsed < _displayDuration)
            {
                elapsed += Time.deltaTime;
                rectTransform.anchoredPosition += Vector2.up * _floatSpeed * Time.deltaTime;

                // Fade out
                CanvasGroup canvasGroup = damageNumber.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f - (elapsed / _displayDuration);
                }

                yield return null;
            }

            Destroy(damageNumber);
        }
    }
}
