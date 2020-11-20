using System;
using System.Collections.Generic;

namespace Blowin.Optional
{
    public static class OptionalExt
    {
        public static Optional<T> AsOptional<T>(this T self) => Optional.From(self);
    }
    
    public static class Optional
    {
        public static Optional<T> From<T>(T val) => new Optional<T>(val);

        public static Optional<T> None<T>() => new Optional<T>();
    }
    
    public readonly struct Optional<T> : IEquatable<Optional<T>>
    {
        public readonly T Value;
        
        public readonly bool IsSome;
        
        public bool IsNone => !IsSome;

        public T ValueOrThrow
        {
            get
            {
                if (IsSome)
                    return Value;
                throw new ArgumentNullException(nameof(Value));
            }
        }
        
        internal Optional(T val)
        {
            Value = val;
            IsSome = val != null;
        }

        public static explicit operator T(Optional<T> d) => d.IsSome ? d.Value : throw new NullReferenceException("Optinal is empty");
    
        public static implicit operator Optional<T>(T d) => Optional.From(d);
        
        public Optional<TRes> Map<TRes>(Func<T, TRes> mapSome) 
            => IsSome ? Optional.From(mapSome(Value)) : Optional.None<TRes>();
        
        public Optional<TRes> Map<TRes>(Func<T, TRes> mapSome, Func<TRes> mapNone) => Optional.From(IsSome ? mapSome(Value) : mapNone());

        public Optional<TRes> MapMany<TRes>(Func<T, Optional<TRes>> mapSome, Func<Optional<TRes>> mapNone)
            => IsSome ? mapSome(Value) : mapNone();
        
        public Optional<TRes> MapMany<TRes>(Func<T, Optional<TRes>> mapSome)
            => IsSome ? mapSome(Value) : Optional.None<TRes>();

        public static bool operator ==(Optional<T> left, Optional<T> right) => left.Equals(right);

        public static bool operator !=(Optional<T> left, Optional<T> right) => !left.Equals(right);

        public bool Equals(Optional<T> other)
        {
            if (IsSome != other.IsSome)
                return false;

            return !IsSome || EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override int GetHashCode() => IsSome ? EqualityComparer<T>.Default.GetHashCode(Value) : -124512;

        public override bool Equals(object obj) => obj is Optional<T> o && Equals(o);

        public override string ToString() => IsSome ? $"Some({Value})" : "None";
    }
}