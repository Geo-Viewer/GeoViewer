using System;
using System.Collections.Generic;

namespace GeoViewer.Controller.Util
{
    public class Bounds<T> : IEquatable<Bounds<T>> where T : IComparable
    {
        public T Min { get; }
        public T Max { get; }

        public Bounds(T min, T max)
        {
            Min = min;
            Max = max;

            if (min.CompareTo(max) > 0)
            {
                (Min, Max) = (Max, Min);
            }
        }

        public bool Contains(T value)
        {
            return value.CompareTo(Min) >= 0 && value.CompareTo(Max) <= 0;
        }

        public bool Contains(Bounds<T> other)
        {
            return other.Min.CompareTo(Min) >= 0 && other.Max.CompareTo(Max) <= 0;
        }

        public bool Overlaps(Bounds<T> other)
        {
            return Max.CompareTo(other.Min) > 0 && Min.CompareTo(other.Max) < 0;
        }

        public bool Equals(Bounds<T>? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return EqualityComparer<T>.Default.Equals(Min, other.Min) &&
                   EqualityComparer<T>.Default.Equals(Max, other.Max);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((Bounds<T>)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Min, Max);
        }
    }
}