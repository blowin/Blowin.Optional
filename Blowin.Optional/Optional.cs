using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Blowin.Optional
{
    public readonly struct OptionalNoneTag
    {
    }
    
    public static class OptionalExt
    {
        public static Optional<T> AsOptional<T>(this T self) => Optional.From(self);
        public static Optional<T> AsOptional<T>(this T? self) 
            where T : struct => Optional.From(self);

        public static Optional<T> FirstOrNone<T>(this IEnumerable<T> self)
        {
            var res = self.FirstOrDefault();
            return Optional.From(res);
        }

        public static Optional<T> FirstOrNone<T>(this IQueryable<T> self)
        {
            var res = self.FirstOrDefault();
            return Optional.From(res);
        }
        
        public static Optional<TRes> Map<T, TRes>(this Optional<T> self, Func<T, TRes?> map)
            where TRes : struct
        {
            return self.IsSome ? Optional.From(map(self.Value)) : Optional.None();
        }
    }
    
    public static class Optional
    {
        public static Optional<T> From<T>(T? val) 
            where T : struct => val.HasValue ? new Optional<T>(val.Value) : None();
        
        public static Optional<T> From<T>(T val) => new Optional<T>(val);

        public static OptionalNoneTag None() => new OptionalNoneTag();
    }
    
    [Serializable, DataContract]
    public readonly struct Optional<T> : IEquatable<Optional<T>>, IEquatable<T>
    {
        [DataMember]
        public readonly T Value;
        
        [DataMember]
        public readonly bool IsSome;
        
        [IgnoreDataMember]
        public bool IsNone => !IsSome;

        [IgnoreDataMember]
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

        public static explicit operator T(Optional<T> d) => d.IsSome ? d.Value : throw new NullReferenceException("Optional is empty");
    
        public static implicit operator Optional<T>(T d) => Optional.From(d);

        public static implicit operator Optional<T>(OptionalNoneTag n) => new Optional<T>();

        public T ValueOr(T defaultV = default) => IsSome ? Value : defaultV;
        
        public T ValueOr(Func<T> factory) => IsSome ? Value : factory();
        
        public Optional<TRes> Map<TRes>(Func<T, TRes> mapSome) 
            => IsSome ? Optional.From(mapSome(Value)) : Optional.None();
        
        public Optional<TRes> Map<TRes>(Func<T, TRes> mapSome, Func<TRes> mapNone) => Optional.From(IsSome ? mapSome(Value) : mapNone());

        public Optional<TRes> FlatMap<TRes>(Func<T, Optional<TRes>> mapSome, Func<Optional<TRes>> mapNone)
            => IsSome ? mapSome(Value) : mapNone();
        
        public Optional<TRes> FlatMap<TRes>(Func<T, Optional<TRes>> mapSome)
            => IsSome ? mapSome(Value) : Optional.None();

        public static bool operator ==(Optional<T> left, Optional<T> right) => left.Equals(right);

        public static bool operator !=(Optional<T> left, Optional<T> right) => !left.Equals(right);

        public bool Equals(Optional<T> other)
        {
            if (IsSome != other.IsSome)
                return false;

            return !IsSome || EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override int GetHashCode() => IsSome ? EqualityComparer<T>.Default.GetHashCode(Value) : -124512;

        public bool Equals(T other) => IsSome && EqualityComparer<T>.Default.Equals(Value, other);

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case Optional<T> op:
                    return Equals(op);
                case T v:
                    return Equals(v);
                default:
                    return false;
            }
        }

        public Optional<TRes> SelectMany<TSecond, TRes>(Func<T, Optional<TSecond>> opSelector, Func<T, TSecond, TRes> projection)
        {
            if (IsNone)
                return Optional.None();

            var sec = opSelector(Value);
            if (sec.IsNone)
                return Optional.None();

            var result = projection(Value, sec.Value);
            return Optional.From(result);
        }
        
        public override string ToString() => IsSome ? $"Some({Value})" : "None";
    }
}