using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Opertoon.Panoply
{
    [Serializable]
    public class Border
    {
        public float top;
        public float right;
        public float bottom;
        public float left;

        public Border()
        {
            top = 0;
            right = 0;
            bottom = 0;
            left = 0;
        }

        public Border (float t, float r, float b, float l)
        {
            top = t;
            right = r;
            bottom = b;
            left = l;
        }

        public void SetSize(float size)
        {
            top = right = bottom = left = size;
        }
    }
}
