﻿using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

namespace Money
{
    public class CashManager : MonoBehaviour
    {
        public float totalCashCount;
        [SerializeField] private CashProgressBar progressBar;

        private void Awake()
        {
            progressBar.SetMaxValue(totalCashCount * 7); // all cashCount * 7 
            progressBar.SetMaxValuePerLevel(totalCashCount);
            //Debug.Log(totalCashCount);
        }

       
    }
}
