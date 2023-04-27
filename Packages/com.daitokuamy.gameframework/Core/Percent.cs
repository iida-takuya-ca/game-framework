using System;
using UnityEngine;

namespace GameFramework.Core {
    /// <summary>
    /// 百分率計算用
    /// </summary>
    [Serializable]
    public struct Percent {
        // 百分の1.0
        public const int One = 100;

        // int型の百分率の素の値
        [SerializeField]
        private int _percentValue;

        #region operators

        public static Percent operator +(Percent a, Percent b) {
            return new Percent(a._percentValue + b._percentValue);
        }

        public static Percent operator +(Percent a, float b) {
            return new Percent(a._percentValue + (int)(b * One));
        }

        public static Percent operator +(float a, Percent b) {
            return new Percent((int)(a * One) + b._percentValue);
        }

        public static Percent operator +(Percent a, int b) {
            return new Percent(a._percentValue + b);
        }

        public static Percent operator +(int a, Percent b) {
            return new Percent(a + b._percentValue);
        }

        public static Percent operator -(Percent a, Percent b) {
            return new Percent(a._percentValue - b._percentValue);
        }

        public static Percent operator -(Percent a, float b) {
            return new Percent(a._percentValue - (int)(b * One));
        }

        public static Percent operator -(float a, Percent b) {
            return new Percent((int)(a * One) - b._percentValue);
        }

        public static Percent operator -(Percent a, int b) {
            return new Percent(a._percentValue - b);
        }

        public static Percent operator -(int a, Percent b) {
            return new Percent(a - b._percentValue);
        }

        public static Percent operator *(Percent a, Percent b) {
            return new Percent(a._percentValue * b._percentValue / One);
        }

        public static Percent operator *(Percent a, float b) {
            return new Percent(a._percentValue * (int)(b * One) / One);
        }

        public static Percent operator *(float a, Percent b) {
            return new Percent((int)(a * One) * b._percentValue / One);
        }

        public static Percent operator *(Percent a, int b) {
            return new Percent(a._percentValue * b / One);
        }

        public static Percent operator *(int a, Percent b) {
            return new Percent(a * b._percentValue / One);
        }

        public static Percent operator /(Percent a, Percent b) {
            return new Percent(a._percentValue / b._percentValue * One);
        }

        public static Percent operator /(Percent a, float b) {
            return new Percent(a._percentValue / (int)(b * One) * One);
        }

        public static Percent operator /(float a, Percent b) {
            return new Percent((int)(a * One) / b._percentValue * One);
        }

        public static Percent operator /(Percent a, int b) {
            return new Percent(a._percentValue / b * One);
        }

        public static Percent operator /(int a, Percent b) {
            return new Percent(a / b._percentValue * One);
        }

        public static implicit operator Percent(int percent) {
            return new Percent(percent);
        }

        public static implicit operator int(Percent percent) {
            return percent._percentValue;
        }

        public static implicit operator Percent(float value) {
            return new Percent(value);
        }

        public static implicit operator float(Percent percent) {
            return percent._percentValue / (float)One;
        }

        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Percent(int percent) {
            _percentValue = percent;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Percent(float value) {
            _percentValue = (int)(value * One);
        }

        /// <summary>
        /// int型の値を百分率で掛ける
        /// </summary>
        public int Multiply(int value) {
            return value * _percentValue / One;
        }

        /// <summary>
        /// int型の値を百分率で割る
        /// </summary>
        public int Divide(int value) {
            return value / _percentValue * One;
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        public override string ToString() {
            return (_percentValue / (float)One).ToString();
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        public string ToString(string format) {
            return (_percentValue / (float)One).ToString(format);
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        public string ToString(System.IFormatProvider provider) {
            return (_percentValue / (float)One).ToString(provider);
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        public string ToString(string format, System.IFormatProvider provider) {
            return (_percentValue / (float)One).ToString(format, provider);
        }
    }
}