using System;
using UnityEngine;

namespace GameFramework.Core {
    /// <summary>
    /// 千分率計算用
    /// </summary>
    [Serializable]
    public struct Permil {
        // 千分率の1.0
        public const int One = 1000;

        // int型の千分率の素の値
        [SerializeField]
        private int _permilValue;

        #region operators

        public static Permil operator +(Permil a, Permil b) {
            return new Permil(a._permilValue + b._permilValue);
        }

        public static Permil operator +(Permil a, float b) {
            return new Permil(a._permilValue + (int)(b * One));
        }

        public static Permil operator +(float a, Permil b) {
            return new Permil((int)(a * One) + b._permilValue);
        }

        public static Permil operator +(Permil a, int b) {
            return new Permil(a._permilValue + b);
        }

        public static Permil operator +(int a, Permil b) {
            return new Permil(a + b._permilValue);
        }

        public static Permil operator -(Permil a, Permil b) {
            return new Permil(a._permilValue - b._permilValue);
        }

        public static Permil operator -(Permil a, float b) {
            return new Permil(a._permilValue - (int)(b * One));
        }

        public static Permil operator -(float a, Permil b) {
            return new Permil((int)(a * One) - b._permilValue);
        }

        public static Permil operator -(Permil a, int b) {
            return new Permil(a._permilValue - b);
        }

        public static Permil operator -(int a, Permil b) {
            return new Permil(a - b._permilValue);
        }

        public static Permil operator *(Permil a, Permil b) {
            return new Permil(a._permilValue * b._permilValue / One);
        }

        public static Permil operator *(Permil a, float b) {
            return new Permil(a._permilValue * (int)(b * One) / One);
        }

        public static Permil operator *(float a, Permil b) {
            return new Permil((int)(a * One) * b._permilValue / One);
        }

        public static Permil operator *(Permil a, int b) {
            return new Permil(a._permilValue * b / One);
        }

        public static Permil operator *(int a, Permil b) {
            return new Permil(a * b._permilValue / One);
        }

        public static Permil operator /(Permil a, Permil b) {
            return new Permil(a._permilValue / b._permilValue * One);
        }

        public static Permil operator /(Permil a, float b) {
            return new Permil(a._permilValue / (int)(b * One) * One);
        }

        public static Permil operator /(float a, Permil b) {
            return new Permil((int)(a * One) / b._permilValue * One);
        }

        public static Permil operator /(Permil a, int b) {
            return new Permil(a._permilValue / b * One);
        }

        public static Permil operator /(int a, Permil b) {
            return new Permil(a / b._permilValue * One);
        }

        public static implicit operator Permil(int permil) {
            return new Permil(permil);
        }

        public static implicit operator int(Permil permil) {
            return permil._permilValue;
        }

        public static implicit operator Permil(float value) {
            return new Permil(value);
        }

        public static implicit operator float(Permil permil) {
            return permil._permilValue / (float)One;
        }

        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Permil(int permil) {
            _permilValue = permil;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Permil(float value) {
            _permilValue = (int)(value * One);
        }

        /// <summary>
        /// int型の値を千分率で掛ける
        /// </summary>
        public int Multiply(int value) {
            return value * _permilValue / One;
        }

        /// <summary>
        /// int型の値を千分率で割る
        /// </summary>
        public int Divide(int value) {
            return value / _permilValue * One;
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        public override string ToString() {
            return (_permilValue / (float)One).ToString();
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        public string ToString(string format) {
            return (_permilValue / (float)One).ToString(format);
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        public string ToString(System.IFormatProvider provider) {
            return (_permilValue / (float)One).ToString(provider);
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        public string ToString(string format, System.IFormatProvider provider) {
            return (_permilValue / (float)One).ToString(format, provider);
        }
    }
}