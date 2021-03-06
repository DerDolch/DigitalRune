// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
#if !NETFX_CORE && !PORTABLE
using DigitalRune.Mathematics.Algebra.Design;
#endif
#if XNA || MONOGAME
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Mathematics.Algebra
{
  /// <summary>
  /// Defines a 2-dimensional vector (double-precision).
  /// </summary>
  /// <remarks>
  /// The two components (x, y) are stored with double-precision.
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
#if !NETFX_CORE && !PORTABLE
  [TypeConverter(typeof(Vector2DConverter))]
#endif
#if !XBOX && !UNITY
  [DataContract]
#endif
  public struct Vector2D : IEquatable<Vector2D>
  {
    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// Returns a <see cref="Vector2D"/> with all of its components set to zero.
    /// </summary>
    public static readonly Vector2D Zero = new Vector2D(0, 0);

    /// <summary>
    /// Returns a <see cref="Vector2D"/> with all of its components set to one.
    /// </summary>
    public static readonly Vector2D One = new Vector2D(1, 1);

    /// <summary>
    /// Returns the x unit <see cref="Vector2D"/> (1, 0).
    /// </summary>
    public static readonly Vector2D UnitX = new Vector2D(1, 0);

    /// <summary>
    /// Returns the value2 unit <see cref="Vector2D"/> (0, 1).
    /// </summary>
    public static readonly Vector2D UnitY = new Vector2D(0, 1);
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The x component.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
#if !XBOX && !UNITY
    [DataMember]
#endif
    public double X;

    /// <summary>
    /// The y component.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
#if !XBOX && !UNITY
    [DataMember]
#endif
    public double Y;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the component at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <value>The component at <paramref name="index"/>.</value>
    /// <remarks>
    /// The index is zero based: x = vector[0], y = vector[1].
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The <paramref name="index"/> is out of range.
    /// </exception>
    public double this[int index]
    {
      get
      {
        switch (index)
        {
          case 0: return X;
          case 1: return Y;
          default: throw new ArgumentOutOfRangeException("index", "The index is out of range. Allowed values are 0 and 1.");
        }
      }
      set
      {
        switch (index)
        {
          case 0: X = value; break;
          case 1: Y = value; break;
          default: throw new ArgumentOutOfRangeException("index", "The index is out of range. Allowed values are 0 and 1.");
        }
      }
    }


    /// <summary>
    /// Gets a value indicating whether a component of the vector is <see cref="double.NaN"/>.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if a component of the vector is <see cref="double.NaN"/>; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool IsNaN
    {
      get { return Numeric.IsNaN(X) || Numeric.IsNaN(Y); }
    }


    /// <summary>
    /// Returns a value indicating whether this vector is normalized (the length is numerically
    /// equal to 1).
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this <see cref="Vector2D"/> is normalized; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <see cref="IsNumericallyNormalized"/> compares the length of this vector against 1.0 using
    /// the default tolerance value (see <see cref="Numeric.EpsilonD"/>).
    /// </remarks>
    public bool IsNumericallyNormalized
    {
      get { return Numeric.AreEqual(LengthSquared, 1.0); }
    }


    /// <summary>
    /// Returns a value indicating whether this vector has zero size (the length is numerically
    /// equal to 0).
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this vector is numerically zero; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// The length of this vector is compared to 0
    /// using the default tolerance value (see <see cref="Numeric.EpsilonD"/>).
    /// </remarks>
    public bool IsNumericallyZero
    {
      get { return Numeric.IsZero(LengthSquared, Numeric.EpsilonDSquared); }
    }


    /// <summary>
    /// Gets or sets the length of this vector.
    /// </summary>
    /// <returns>The length of the this vector.</returns>
    /// <exception cref="MathematicsException">
    /// The vector has a length of 0. The length cannot be changed.
    /// </exception>
    [XmlIgnore]
#if XNA || MONOGAME
    [ContentSerializerIgnore]
#endif
    public double Length
    {
      get
      {
        return Math.Sqrt(X * X + Y * Y);
      }
      set
      {
        double length = Length;
        if (Numeric.IsZero(length))
          throw new MathematicsException("Cannot change length of a vector with length 0.");

        double scale = value / length;
        X *= scale;
        Y *= scale;
      }
    }


    /// <summary>
    /// Returns the squared length of this vector.
    /// </summary>
    /// <returns>The squared length of this vector.</returns>
    public double LengthSquared
    {
      get
      {
        return X * X + Y * Y;
      }
    }


    /// <summary>
    /// Returns the normalized vector.
    /// </summary>
    /// <value>The normalized vector.</value>
    /// <remarks>
    /// The property does not change this instance. To normalize this instance you need to call 
    /// <see cref="Normalize"/>.
    /// </remarks>
    /// <exception cref="DivideByZeroException">
    /// The length of the vector is zero. The quaternion cannot be normalized.
    /// </exception>
    public Vector2D Normalized
    {
      get
      {
        Vector2D v = this;
        v.Normalize();
        return v;
      }
    }


    /// <summary>
    /// Returns an arbitrary normalized <see cref="Vector2D"/> that is orthogonal to this vector.
    /// </summary>
    /// <value>An arbitrary normalized orthogonal <see cref="Vector2D"/>.</value>
    public Vector2D Orthonormal
    {
      get
      {
        Vector2D v;
        v.X = -Y;
        v.Y = X;
        v.Normalize();
        return v;
      }
    }


    /// <summary>
    /// Gets the value of the largest component.
    /// </summary>
    /// <value>The value of the largest component.</value>
    public double LargestComponent
    {
      get
      {
        if (X >= Y)
          return X;

        return Y;
      }
    }


    /// <summary>
    /// Gets the index (zero-based) of the largest component.
    /// </summary>
    /// <value>The index (zero-based) of the largest component.</value>
    /// <remarks>
    /// <para>
    /// This method returns the index of the component (X or Y) which has the largest value. The 
    /// index is zero-based, i.e. the index of X is 0. 
    /// </para>
    /// <para>
    /// If both components are equal, 0 is returned.
    /// </para>
    /// </remarks>
    public int IndexOfLargestComponent
    {
      get
      {
        if (X >= Y)
          return 0;

        return 1;
      }
    }


    /// <summary>
    /// Gets the value of the smallest component.
    /// </summary>
    /// <value>The value of the smallest component.</value>
    public double SmallestComponent
    {
      get
      {
        if (X <= Y)
          return X;

        return Y;
      }
    }


    /// <summary>
    /// Gets the index (zero-based) of the smallest component.
    /// </summary>
    /// <value>The index (zero-based) of the smallest component.</value>
    /// <remarks>
    /// <para>
    /// This method returns the index of the component (X or Y) which has the smallest value. The 
    /// index is zero-based, i.e. the index of X is 0. 
    /// </para>
    /// <para>
    /// If both components are equal, 0 is returned.
    /// </para>
    /// </remarks>
    public int IndexOfSmallestComponent
    {
      get
      {
        if (X <= Y)
          return 0;

        return 1;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of <see cref="Vector2D"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of <see cref="Vector2D"/>.
    /// </summary>
    /// <param name="x">Initial value for the x component.</param>
    /// <param name="y">Initial value for the y component.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public Vector2D(double x, double y)
    {
      X = x;
      Y = y;
    }


    /// <summary>
    /// Initializes a new instance of <see cref="Vector2D"/>.
    /// </summary>
    /// <param name="componentValue">The initial value for 2 the vector components.</param>
    /// <remarks>
    /// All components are set to <paramref name="componentValue"/>.
    /// </remarks>
    public Vector2D(double componentValue)
    {
      X = componentValue;
      Y = componentValue;
    }


    /// <summary>
    /// Initializes a new instance of <see cref="Vector2D"/>.
    /// </summary>
    /// <param name="components">Array with the initial values for the components x, and y.</param>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="components"/> has less than 2 elements.
    /// </exception>
    /// <exception cref="NullReferenceException">
    /// <paramref name="components"/> must not be <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public Vector2D(double[] components)
    {
      X = components[0];
      Y = components[1];
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Vector2D"/> class.
    /// </summary>
    /// <param name="components">List with the initial values for the components x, and y.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="components"/> has less than 2 elements.
    /// </exception>
    /// <exception cref="NullReferenceException">
    /// <paramref name="components"/> must not be <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public Vector2D(IList<double> components)
    {
      X = components[0];
      Y = components[1];
    }
    #endregion


    //--------------------------------------------------------------
    #region Interfaces and Overrides
    //--------------------------------------------------------------

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode()
    {
      // ReSharper disable NonReadonlyFieldInGetHashCode
      unchecked
      {
        int hashCode = X.GetHashCode();
        hashCode = (hashCode * 397) ^ Y.GetHashCode();
        return hashCode;
      }
      // ReSharper restore NonReadonlyFieldInGetHashCode
    }


    /// <overloads>
    /// <summary>
    /// Indicates whether a vector and a another object are equal.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Indicates whether this instance and a specified object are equal.
    /// </summary>
    /// <param name="obj">Another object to compare to.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> and this instance are the same type and
    /// represent the same value; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is Vector2D && this == (Vector2D)obj;
    }


    #region IEquatable<Vector2D> Members
    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the other parameter; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Equals(Vector2D other)
    {
      return this == other;
    }
    #endregion


    /// <overloads>
    /// <summary>
    /// Returns the string representation of a vector.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Returns the string representation of this vector.
    /// </summary>
    /// <returns>The string representation of this vector.</returns>
    public override string ToString()
    {
      return ToString(CultureInfo.CurrentCulture);
    }


    /// <summary>
    /// Returns the string representation of this vector using the specified culture-specific format
    /// information.
    /// </summary>
    /// <param name="provider">
    /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.
    /// </param>
    /// <returns>The string representation of this vector.</returns>
    public string ToString(IFormatProvider provider)
    {
      return string.Format(provider, "({0}; {1})", X, Y);
    }
    #endregion


    //--------------------------------------------------------------
    #region Overloaded Operators
    //--------------------------------------------------------------

    /// <summary>
    /// Negates a vector.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>The negated vector.</returns>
    public static Vector2D operator -(Vector2D vector)
    {
      vector.X = -vector.X;
      vector.Y = -vector.Y;
      return vector;
    }


    /// <summary>
    /// Negates a vector.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>The negated vector.</returns>
    public static Vector2D Negate(Vector2D vector)
    {
      vector.X = -vector.X;
      vector.Y = -vector.Y;
      return vector;
    }


    /// <summary>
    /// Adds two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The sum of the two vectors.</returns>
    public static Vector2D operator +(Vector2D vector1, Vector2D vector2)
    {
      vector1.X += vector2.X;
      vector1.Y += vector2.Y;
      return vector1;
    }


    /// <summary>
    /// Adds two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The sum of the two vectors.</returns>
    public static Vector2D Add(Vector2D vector1, Vector2D vector2)
    {
      vector1.X += vector2.X;
      vector1.Y += vector2.Y;
      return vector1;
    }


    /// <summary>
    /// Subtracts a vector from a vector.
    /// </summary>
    /// <param name="minuend">The first vector (minuend).</param>
    /// <param name="subtrahend">The second vector (subtrahend).</param>
    /// <returns>The difference of the two vectors.</returns>
    public static Vector2D operator -(Vector2D minuend, Vector2D subtrahend)
    {
      minuend.X -= subtrahend.X;
      minuend.Y -= subtrahend.Y;
      return minuend;
    }


    /// <summary>
    /// Subtracts a vector from a vector.
    /// </summary>
    /// <param name="minuend">The first vector (minuend).</param>
    /// <param name="subtrahend">The second vector (subtrahend).</param>
    /// <returns>The difference of the two vectors.</returns>
    public static Vector2D Subtract(Vector2D minuend, Vector2D subtrahend)
    {
      minuend.X -= subtrahend.X;
      minuend.Y -= subtrahend.Y;
      return minuend;
    }



    /// <overloads>
    /// <summary>
    /// Multiplies a vector by a scalar or a vector.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Multiplies a vector by a scalar.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The vector with each component multiplied by <paramref name="scalar"/>.</returns>
    public static Vector2D operator *(Vector2D vector, double scalar)
    {
      vector.X *= scalar;
      vector.Y *= scalar;
      return vector;
    }


    /// <summary>
    /// Multiplies a vector by a scalar.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The vector with each component multiplied by <paramref name="scalar"/>.</returns>
    public static Vector2D operator *(double scalar, Vector2D vector)
    {
      vector.X *= scalar;
      vector.Y *= scalar;
      return vector;
    }


    /// <overloads>
    /// <summary>
    /// Multiplies a vector by a scalar or a vector.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Multiplies a vector by a scalar.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The vector with each component multiplied by <paramref name="scalar"/>.</returns>
    public static Vector2D Multiply(double scalar, Vector2D vector)
    {
      vector.X *= scalar;
      vector.Y *= scalar;
      return vector;
    }


    /// <summary>
    /// Multiplies the components of two vectors by each other.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The component-wise product of the two vectors.</returns>
    public static Vector2D operator *(Vector2D vector1, Vector2D vector2)
    {
      vector1.X *= vector2.X;
      vector1.Y *= vector2.Y;
      return vector1;
    }


    /// <summary>
    /// Multiplies the components of two vectors by each other.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The component-wise product of the two vectors.</returns>
    public static Vector2D Multiply(Vector2D vector1, Vector2D vector2)
    {
      vector1.X *= vector2.X;
      vector1.Y *= vector2.Y;
      return vector1;
    }


    /// <overloads>
    /// <summary>
    /// Divides the vector by a scalar or a vector.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Divides a vector by a scalar.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The vector with each component divided by <paramref name="scalar"/>.</returns>
    public static Vector2D operator /(Vector2D vector, double scalar)
    {
      double f = 1 / scalar;
      vector.X *= f;
      vector.Y *= f;
      return vector;
    }


    /// <overloads>
    /// <summary>
    /// Divides the vector by a scalar or a vector.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Divides a vector by a scalar.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The vector with each component divided by <paramref name="scalar"/>.</returns>
    public static Vector2D Divide(Vector2D vector, double scalar)
    {
      double f = 1 / scalar;
      vector.X *= f;
      vector.Y *= f;
      return vector;
    }


    /// <summary>
    /// Divides the components of a vector by the components of another vector.
    /// </summary>
    /// <param name="dividend">The first vector (dividend).</param>
    /// <param name="divisor">The second vector (divisor).</param>
    /// <returns>The component-wise product of the two vectors.</returns>
    public static Vector2D operator /(Vector2D dividend, Vector2D divisor)
    {
      dividend.X /= divisor.X;
      dividend.Y /= divisor.Y;
      return dividend;
    }


    /// <summary>
    /// Divides the components of a vector by the components of another vector.
    /// </summary>
    /// <param name="dividend">The first vector (dividend).</param>
    /// <param name="divisor">The second vector (divisor).</param>
    /// <returns>The component-wise division of the two vectors.</returns>
    public static Vector2D Divide(Vector2D dividend, Vector2D divisor)
    {
      dividend.X /= divisor.X;
      dividend.Y /= divisor.Y;
      return dividend;
    }


    /// <summary>
    /// Tests if two vectors are equal.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>
    /// <see langword="true"/> if the vectors are equal; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// For the test the corresponding components of the vectors are compared.
    /// </remarks>
    public static bool operator ==(Vector2D vector1, Vector2D vector2)
    {
      return vector1.X == vector2.X
             && vector1.Y == vector2.Y;
    }


    /// <summary>
    /// Tests if two vectors are not equal.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>
    /// <see langword="true"/> if the vectors are different; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// For the test the corresponding components of the vectors are compared.
    /// </remarks>
    public static bool operator !=(Vector2D vector1, Vector2D vector2)
    {
      return vector1.X != vector2.X
          || vector1.Y != vector2.Y;
    }


    /// <summary>
    /// Tests if each component of a vector is greater than the corresponding component of another
    /// vector.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>
    /// <see langword="true"/> if each component of <paramref name="vector1"/> is greater than its
    /// counterpart in <paramref name="vector2"/>; otherwise, <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
    public static bool operator >(Vector2D vector1, Vector2D vector2)
    {
      return vector1.X > vector2.X
          && vector1.Y > vector2.Y;
    }


    /// <summary>
    /// Tests if each component of a vector is greater or equal than the corresponding component of
    /// another vector.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>
    /// <see langword="true"/> if each component of <paramref name="vector1"/> is greater or equal
    /// than its counterpart in <paramref name="vector2"/>; otherwise, <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
    public static bool operator >=(Vector2D vector1, Vector2D vector2)
    {
      return vector1.X >= vector2.X
          && vector1.Y >= vector2.Y;
    }


    /// <summary>
    /// Tests if each component of a vector is less than the corresponding component of another
    /// vector.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>
    /// <see langword="true"/> if each component of <paramref name="vector1"/> is less than its
    /// counterpart in <paramref name="vector2"/>; otherwise, <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
    public static bool operator <(Vector2D vector1, Vector2D vector2)
    {
      return vector1.X < vector2.X
          && vector1.Y < vector2.Y;
    }


    /// <summary>
    /// Tests if each component of a vector is less or equal than the corresponding component of
    /// another vector.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>
    /// <see langword="true"/> if each component of <paramref name="vector1"/> is less or equal than
    /// its counterpart in <paramref name="vector2"/>; otherwise, <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
    public static bool operator <=(Vector2D vector1, Vector2D vector2)
    {
      return vector1.X <= vector2.X
          && vector1.Y <= vector2.Y;
    }


    /// <overloads>
    /// <summary>
    /// Converts a vector to another data type.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Converts a vector to an array of 2 <see langword="double"/> values.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>
    /// The array with 2 <see langword="double"/> values. The order of the elements is: x, y
    /// </returns>
    public static explicit operator double[](Vector2D vector)
    {
      return new[] { vector.X, vector.Y };
    }


    /// <summary>
    /// Converts this vector to an array of 2 <see langword="double"/> values.
    /// </summary>
    /// <returns>
    /// The array with 2 <see langword="double"/> values. The order of the elements is: x, y
    /// </returns>
    public double[] ToArray()
    {
      return (double[])this;
    }


    /// <summary>
    /// Converts a vector to a list of 2 <see langword="double"/> values.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>
    /// The list with 2 <see langword="double"/> values. The order of the elements is: x, y
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
    public static explicit operator List<double>(Vector2D vector)
    {
      List<double> result = new List<double>(2) { vector.X, vector.Y };
      return result;
    }


    /// <summary>
    /// Converts this vector to a list of 2 <see langword="double"/> values.
    /// </summary>
    /// <returns>
    /// The list with 2 <see langword="double"/> values. The order of the elements is: x, y
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
    public List<double> ToList()
    {
      return (List<double>)this;
    }


    /// <summary>
    /// Performs an explicit conversion from <see cref="Vector2D"/> to <see cref="Vector2F"/>.
    /// </summary>
    /// <param name="vector">The DigitalRune <see cref="Vector2D"/>.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator Vector2F(Vector2D vector)
    {
      return new Vector2F((float)vector.X, (float)vector.Y);
    }


    /// <summary>
    /// Converts this <see cref="Vector2D"/> to <see cref="Vector2F"/>.
    /// </summary>
    /// <returns>The result of the conversion.</returns>
    public Vector2F ToVector2F()
    {
      return new Vector2F((float)X, (float)Y);
    }


    /// <summary>
    /// Performs an implicit conversion from <see cref="Vector2D"/> to <see cref="VectorD"/>.
    /// </summary>
    /// <param name="vector">The DigitalRune <see cref="Vector2D"/>.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator VectorD(Vector2D vector)
    {
      VectorD result = new VectorD(2);
      result[0] = vector.X; result[1] = vector.Y;
      return result;
    }


    /// <summary>
    /// Converts this <see cref="Vector2D"/> to <see cref="VectorD"/>.
    /// </summary>
    /// <returns>The result of the conversion.</returns>
    public VectorD ToVectorD()
    {
      return this;
    }


#if XNA || MONOGAME
    /// <summary>
    /// Performs an conversion from <see cref="Vector2"/> (XNA Framework) to <see cref="Vector2D"/> 
    /// (DigitalRune Mathematics).
    /// </summary>
    /// <param name="vector">The <see cref="Vector2"/> (XNA Framework).</param>
    /// <returns>The <see cref="Vector2D"/> (DigitalRune Mathematics).</returns>
    /// <remarks>
    /// This method is available only in the XNA-compatible build of the
    /// DigitalRune.Mathematics.dll.
    /// </remarks>
    public static explicit operator Vector2D(Vector2 vector)
    {
      return new Vector2D(vector.X, vector.Y);
    }


    /// <summary>
    /// Converts this <see cref="Vector2D"/> (DigitalRune Mathematics) to <see cref="Vector2"/> 
    /// (XNA Framework).
    /// </summary>
    /// <param name="vector">The <see cref="Vector2"/> (XNA Framework).</param>
    /// <returns>The <see cref="Vector2D"/> (DigitalRune Mathematics).</returns>
    /// <remarks>
    /// This method is available only in the XNA-compatible build of the
    /// DigitalRune.Mathematics.dll.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static Vector2D FromXna(Vector2 vector)
    {
      return new Vector2D(vector.X, vector.Y);
    }

    
    /// <summary>
    /// Performs an conversion from <see cref="Vector2D"/> (DigitalRune Mathematics) to 
    /// <see cref="Vector2"/> (XNA Framework).
    /// </summary>
    /// <param name="vector">The <see cref="Vector2D"/> (DigitalRune Mathematics).</param>
    /// <returns>The <see cref="Vector2"/> (XNA Framework).</returns>
    /// <remarks>
    /// This method is available only in the XNA-compatible build of the
    /// DigitalRune.Mathematics.dll.
    /// </remarks>
    public static explicit operator Vector2(Vector2D vector)
    {
      return new Vector2((float)vector.X, (float)vector.Y);
    }


    /// <summary>
    /// Converts this <see cref="Vector2D"/> (DigitalRune Mathematics) to <see cref="Vector2"/> 
    /// (XNA Framework).
    /// </summary>
    /// <returns>The <see cref="Vector2"/> (XNA Framework).</returns>
    /// <remarks>
    /// This method is available only in the XNA-compatible build of the
    /// DigitalRune.Mathematics.dll.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public Vector2 ToXna()
    {
      return new Vector2((float)X, (float)Y);
    }
#endif
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Sets each vector component to its absolute value.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets each vector component to its absolute value.
    /// </summary>
    public void Absolute()
    {
      X = Math.Abs(X);
      Y = Math.Abs(Y);
    }


    /// <overloads>
    /// <summary>
    /// Clamps the vector components to the range [min, max].
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Clamps the vector components to the range [min, max].
    /// </summary>
    /// <param name="min">The min limit.</param>
    /// <param name="max">The max limit.</param>
    /// <remarks>
    /// This operation is carried out per component. Component values less than 
    /// <paramref name="min"/> are set to <paramref name="min"/>. Component values greater than 
    /// <paramref name="max"/> are set to <paramref name="max"/>.
    /// </remarks>
    public void Clamp(double min, double max)
    {
      X = MathHelper.Clamp(X, min, max);
      Y = MathHelper.Clamp(Y, min, max);
    }


    /// <overloads>
    /// <summary>
    /// Clamps near-zero vector components to zero.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Clamps near-zero vector components to zero.
    /// </summary>
    /// <remarks>
    /// Each vector component (X and Y) is compared to zero. If the component is in the interval 
    /// [-<see cref="Numeric.EpsilonD"/>, +<see cref="Numeric.EpsilonD"/>] it is set to zero, 
    /// otherwise it remains unchanged.
    /// </remarks>
    public void ClampToZero()
    {
      X = Numeric.ClampToZero(X);
      Y = Numeric.ClampToZero(Y);
    }


    /// <summary>
    /// Clamps near-zero vector components to zero.
    /// </summary>
    /// <param name="epsilon">The tolerance value.</param>
    /// <remarks>
    /// Each vector component (X and Y) is compared to zero. If the component is in the interval 
    /// [-<paramref name="epsilon"/>, +<paramref name="epsilon"/>] it is set to zero, otherwise it 
    /// remains unchanged.
    /// </remarks>
    public void ClampToZero(double epsilon)
    {
      X = Numeric.ClampToZero(X, epsilon);
      Y = Numeric.ClampToZero(Y, epsilon);
    }


    /// <summary>
    /// Normalizes the vector.
    /// </summary>
    /// <remarks>
    /// A vectors is normalized by dividing its components by the length of the vector.
    /// </remarks>
    /// <exception cref="DivideByZeroException">
    /// The length of this vector is zero. The vector cannot be normalized.
    /// </exception>
    public void Normalize()
    {
      double length = Length;
      if (Numeric.IsZero(length))
        throw new DivideByZeroException("Cannot normalize a vector with length 0.");

      double scale = 1.0 / length;
      X *= scale;
      Y *= scale;
    }


    /// <summary>
    /// Tries to normalize the vector.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the vector was normalized; otherwise, <see langword="false"/> if 
    /// the vector could not be normalized. (The length is numerically zero.)
    /// </returns>
    public bool TryNormalize()
    {
      double lengthSquared = LengthSquared;
      if (Numeric.IsZero(lengthSquared, Numeric.EpsilonDSquared))
        return false;

      double length = Math.Sqrt(lengthSquared);

      double scale = 1.0 / length;
      X *= scale;
      Y *= scale;

      return true;
    }


    /// <overloads>
    /// <summary>
    /// Projects a vector onto another vector.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets this vector to its projection onto the axis given by the target vector.
    /// </summary>
    /// <param name="target">The target vector.</param>
    public void ProjectTo(Vector2D target)
    {
      this = Dot(this, target) / target.LengthSquared * target;
    }
    #endregion


    //--------------------------------------------------------------
    #region Static Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Returns a vector with the absolute values of the elements of the given vector.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>A vector with the absolute values of the elements of the given vector.</returns>
    public static Vector2D Absolute(Vector2D vector)
    {
      return new Vector2D(Math.Abs(vector.X), Math.Abs(vector.Y));
    }


    /// <overloads>
    /// <summary>
    /// Determines whether two vectors are equal (regarding a given tolerance).
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether two vectors are equal (regarding the tolerance 
    /// <see cref="Numeric.EpsilonD"/>).
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>
    /// <see langword="true"/> if the vectors are equal (within the tolerance 
    /// <see cref="Numeric.EpsilonD"/>); otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The two vectors are compared component-wise. If the differences of the components are less
    /// than <see cref="Numeric.EpsilonD"/> the vectors are considered as being equal.
    /// </remarks>
    public static bool AreNumericallyEqual(Vector2D vector1, Vector2D vector2)
    {
      return Numeric.AreEqual(vector1.X, vector2.X)
          && Numeric.AreEqual(vector1.Y, vector2.Y);
    }


    /// <summary>
    /// Determines whether two vectors are equal (regarding a specific tolerance).
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <param name="epsilon">The tolerance value.</param>
    /// <returns>
    /// <see langword="true"/> if the vectors are equal (within the tolerance 
    /// <paramref name="epsilon"/>); otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The two vectors are compared component-wise. If the differences of the components are less
    /// than <paramref name="epsilon"/> the vectors are considered as being equal.
    /// </remarks>
    public static bool AreNumericallyEqual(Vector2D vector1, Vector2D vector2, double epsilon)
    {
      return Numeric.AreEqual(vector1.X, vector2.X, epsilon)
          && Numeric.AreEqual(vector1.Y, vector2.Y, epsilon);
    }


    /// <summary>
    /// Returns a vector with the vector components clamped to the range [min, max].
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="min">The min limit.</param>
    /// <param name="max">The max limit.</param>
    /// <returns>A vector with clamped components.</returns>
    /// <remarks>
    /// This operation is carried out per component. Component values less than 
    /// <paramref name="min"/> are set to <paramref name="min"/>. Component values greater than 
    /// <paramref name="max"/> are set to <paramref name="max"/>.
    /// </remarks>
    public static Vector2D Clamp(Vector2D vector, double min, double max)
    {
      return new Vector2D(MathHelper.Clamp(vector.X, min, max),
                          MathHelper.Clamp(vector.Y, min, max));
    }


    /// <summary>
    /// Returns a vector with near-zero vector components clamped to 0.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>The vector with small components clamped to zero.</returns>
    /// <remarks>
    /// Each vector component (X and Y) is compared to zero. If the component is in the interval 
    /// [-<see cref="Numeric.EpsilonD"/>, +<see cref="Numeric.EpsilonD"/>] it is set to zero, 
    /// otherwise it remains unchanged.
    /// </remarks>
    public static Vector2D ClampToZero(Vector2D vector)
    {
      vector.X = Numeric.ClampToZero(vector.X);
      vector.Y = Numeric.ClampToZero(vector.Y);
      return vector;
    }


    /// <summary>
    /// Returns a vector with near-zero vector components clamped to 0.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="epsilon">The tolerance value.</param>
    /// <returns>The vector with small components clamped to zero.</returns>
    /// <remarks>
    /// Each vector component (X and Y) is compared to zero. If the component is in the interval 
    /// [-<paramref name="epsilon"/>, +<paramref name="epsilon"/>] it is set to zero, otherwise it 
    /// remains unchanged.
    /// </remarks>
    public static Vector2D ClampToZero(Vector2D vector, double epsilon)
    {
      vector.X = Numeric.ClampToZero(vector.X, epsilon);
      vector.Y = Numeric.ClampToZero(vector.Y, epsilon);
      return vector;
    }


    /// <summary>
    /// Calculates the dot product of two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The dot product.</returns>
    /// <remarks>
    /// The method calculates the dot product (also known as scalar product or inner product).
    /// </remarks>
    public static double Dot(Vector2D vector1, Vector2D vector2)
    {
      return vector1.X * vector2.X + vector1.Y * vector2.Y;
    }


    /// <summary>
    /// Calculates the angle between two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The angle between the given vectors, such that 0 ≤ angle ≤ π.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="vector1"/> or <paramref name="vector2"/> has a length of 0.
    /// </exception>
    public static double GetAngle(Vector2D vector1, Vector2D vector2)
    {
      if (!vector1.TryNormalize() || !vector2.TryNormalize())
        throw new ArgumentException("vector1 and vector2 must not have 0 length.");

      double α = Dot(vector1, vector2);

      // Inaccuracy in the floating-point operations can cause
      // the result be outside of the valid range.
      // Ensure that the dot product α lies in the interval [-1, 1].
      // Math.Acos() returns Double.NaN if the argument lies outside
      // of this interval.
      α = MathHelper.Clamp(α, -1.0, 1.0);

      return Math.Acos(α);
    }


    /// <summary>
    /// Returns a vector that contains the lowest value from each matching pair of components.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The minimized vector.</returns>
    public static Vector2D Min(Vector2D vector1, Vector2D vector2)
    {
      vector1.X = Math.Min(vector1.X, vector2.X);
      vector1.Y = Math.Min(vector1.Y, vector2.Y);
      return vector1;
    }


    /// <summary>
    /// Returns a vector that contains the highest value from each matching pair of components.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The maximized vector.</returns>
    public static Vector2D Max(Vector2D vector1, Vector2D vector2)
    {
      vector1.X = Math.Max(vector1.X, vector2.X);
      vector1.Y = Math.Max(vector1.Y, vector2.Y);
      return vector1;
    }


    /// <summary>
    /// Projects a vector onto an axis given by the target vector.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <param name="target">The target vector.</param>
    /// <returns>
    /// The projection of <paramref name="vector"/> onto <paramref name="target"/>.
    /// </returns>
    public static Vector2D ProjectTo(Vector2D vector, Vector2D target)
    {
      return Dot(vector, target) / target.LengthSquared * target;
    }


    /// <overloads>
    /// <summary>
    /// Converts the string representation of a 2-dimensional vector to its <see cref="Vector2D"/>
    /// equivalent.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Converts the string representation of a 2-dimensional vector to its <see cref="Vector2D"/>
    /// equivalent.
    /// </summary>
    /// <param name="s">A string representation of a 2-dimensional vector.</param>
    /// <returns>
    /// A <see cref="Vector2D"/> that represents the vector specified by the <paramref name="s"/>
    /// parameter.
    /// </returns>
    /// <remarks>
    /// This version of <see cref="Parse(string)"/> uses the <see cref="CultureInfo"/> associated
    /// with the current thread.
    /// </remarks>
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is not a valid <see cref="Vector2D"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static Vector2D Parse(string s)
    {
      return Parse(s, CultureInfo.CurrentCulture);
    }


    /// <summary>
    /// Converts the string representation of a 2-dimensional vector in a specified culture-specific
    /// format to its <see cref="Vector2D"/> equivalent.
    /// </summary>
    /// <param name="s">A string representation of a 2-dimensional vector.</param>
    /// <param name="provider">
    /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information about
    /// <paramref name="s"/>. 
    /// </param>
    /// <returns>
    /// A <see cref="Vector2D"/> that represents the vector specified by the <paramref name="s"/>
    /// parameter.
    /// </returns>
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is not a valid <see cref="Vector2D"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static Vector2D Parse(string s, IFormatProvider provider)
    {
      Match m = Regex.Match(s, @"\((?<x>.*);(?<y>.*)\)", RegexOptions.None);
      if (m.Success)
      {
        return new Vector2D(double.Parse(m.Groups["x"].Value, provider),
          double.Parse(m.Groups["y"].Value, provider));
      }

      throw new FormatException("String is not a valid Vector2D.");
    }
    #endregion
  }
}
