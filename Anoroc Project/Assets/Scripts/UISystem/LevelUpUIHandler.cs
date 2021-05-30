using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UISystem
{
    public class LevelUpUIHandler : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Image _img;
        [SerializeField] private float _time;
        [SerializeField] private float _speed;

        private float _currentTime;
        
        public string Text
        {
            get => _text.text;
            set => _text.SetText(value);
        }

        private void Start()
        {
            _currentTime = 0;
        }

        private void Update()
        {
            
            _currentTime += Time.deltaTime * _speed;
            _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, 1 - NormalizeValue(_currentTime,0,_time));
            _img.color = new Color(_img.color.r, _img.color.g, _img.color.b, 1 - NormalizeValue(_currentTime,0,_time));

            transform.position += (Vector3)Vector2.up * (Time.deltaTime * _speed);
            
            if (_currentTime > _time)
                Destroy(gameObject);
        }

        private float NormalizeValue(float val, float min, float max)
        {
            if (val > max) return 1;
            if (val < min) return 0;
            return (val - min) / (max - min);
        }
    }
}