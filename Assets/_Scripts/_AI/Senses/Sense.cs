using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace Assets._Scripts._AI.Senses
{
    public class Sense : MonoBehaviour
    {
        public float detectRate = 1.0f;

        protected float elapsedTime = 0.0f;

        protected virtual void Initialize() { }
        protected virtual void UpdateSenses() { }

        private void Start()
        {
            elapsedTime = 0.0f;
            Initialize();
        }

        private void Update()
        {
            UpdateSenses();
        }
    }
}
