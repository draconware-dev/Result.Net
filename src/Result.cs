using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Draconware.Result
{
    /// <summary>
    /// A light-weight result implementation that either contains a <see cref="Value"/> of type <typeparamref name="T"/> upon successful completion or a <see cref="Error"/> of type <typeparamref name="E"/> upon Failure. 
    /// </summary>
    /// <typeparam name="T">The type of the potential value contained on success.</typeparam>
    /// <typeparam name="E">The type of the potential error contained on failure.</typeparam>
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct Result<T, E> : IEquatable<Result<T, E>>
        where T : notnull
        where E : notnull
    {
        [FieldOffset(1)]
        readonly T value;

        [FieldOffset(1)]
        readonly E error;

        [FieldOffset(0)]
        readonly bool success;

        /// <summary>
        /// The default constructor for creating a <see cref="Result{T, E}"/> with a <paramref name="value"/> and a status of Success.
        /// </summary>
        /// <param name="value">The value encapsulated by this instance of <see cref="Result{T, E}"/></param>
        /// <remarks>In practive using this constructor should never be required since there exists an implicit conversion from <typeparamref name="T"/> to <see cref="Result{T, E}"/>.
        /// In the case of ambiguity as to which overload to choose from <see cref="Result{T, E}(T)"/> and <see cref="Result{T, E}(E)"/>, <see cref="Result{T, E}.Success(T)"/> may be used instead.
        /// Here is an example of what that might look like: 
        /// <code>
        /// using static Result&lt;ConcreteT&#44; ConcreteE&gt;;
        /// 
        /// [...] 
        /// 
        /// Result&lt;ConcreteT&#44; ConcreteE&gt; Sample(TConcrete value)
        /// {
        ///     return Success(value);  
        /// }
        /// </code>  
        /// </remarks>
        public Result(T value)
        {
#if NETSTANDARD2_1
            error = default!;
#endif
            this.value = value;
            success = true;
#if NETCOREAPP1_0_OR_GREATER
            Unsafe.SkipInit(out error);
#endif
        }

        /// <summary>
        /// The default constructor for creating a <see cref="Result{T, E}"/> with a <paramref name="error"/> and a status of Failure.
        /// </summary>
        /// <param name="error">The error encapsulated by this instance of <see cref="Result{T, E}"/></param>
        /// <remarks>In practive using this constructor should never be required since there exists an implicit conversion from <typeparamref name="E"/> to <see cref="Result{T, E}"/>.
        /// In the case of ambiguity as to which overload to choose from <see cref="Result{T, E}(T)"/> and <see cref="Result{T, E}(E)"/>, <see cref="Result{T, E}.Failure(E)"/> may be used instead.
        /// Here is an example of what that might look like: 
        /// <code>
        /// using static Result&lt;ConcreteT&#44; ConcreteE&gt;;
        /// 
        /// [...] 
        /// 
        /// Result&lt;ConcreteT&#44; ConcreteE&gt; Sample(ConcreteE error)
        /// {
        ///     return Failure(value);  
        /// }
        /// </code>  
        /// </remarks>
        public Result(E error)
        {
#if NETSTANDARD2_1
            value = default!;
#endif
            this.error = error;
            success = false;
#if NETCOREAPP1_0_OR_GREATER
            Unsafe.SkipInit(out value);
#endif
        }

#if NET5_0_OR_GREATER
        /// <summary>
        /// The Value contained in this struct upon success.
        /// </summary>
        /// <remarks>Please notice that either <see cref="IsSuccess"/> or <c>!</c><see cref="IsFailure"/> must be checked before access.
        /// Despite the implied return type nullability of this property, checking for <c>not null</c> is insufficient for determining the validity of the value as we're working with uninitialized memory. 
        /// Access of this property without previous checks on either <see cref="IsSuccess"/> or <c>!</c><see cref="IsFailure"/> is UNDEFINED BEHAVIOUR. 
        /// </remarks> 
        public readonly T? Value => value;

        /// <summary>
        /// The error contained in this struct upon failure.
        /// </summary>
        /// <remarks>Please notice that either <c>!</c><see cref="IsSuccess"/> or <see cref="IsFailure"/> must be checked before access.
        /// Despite the implied return type nullability of this property, checking for <c>not null</c> is insufficient for determining the validity of the error as we're working with uninitialized memory. 
        /// Access of this property without previous checks on either <c>!</c><see cref="IsSuccess"/> or <see cref="IsFailure"/> is UNDEFINED BEHAVIOUR. 
        /// </remarks> 
        public readonly E? Error => error;
#else
        /// <summary>
        /// The Value contained in this struct upon success.
        /// </summary>
        /// <remarks>Please notice that either <see cref="IsSuccess"/> or <c>!</c><see cref="IsFailure"/> must be checked before access.
        /// Despite the implied return type nullability of this property, checking for <c>not null</c> is insufficient for determining the validity of the value as we're working with uninitialized memory. 
        /// Access of this property without previous checks on either <see cref="IsSuccess"/> or <c>!</c><see cref="IsFailure"/> is UNDEFINED BEHAVIOUR. 
        /// </remarks> 
        public readonly T Value => value;

        /// <summary>
        /// The error contained in this struct upon failure.
        /// </summary>
        /// <remarks>Please notice that either <c>!</c><see cref="IsSuccess"/> or <see cref="IsFailure"/> must be checked before access.
        /// Despite the implied return type nullability of this property, checking for <c>not null</c> is insufficient for determining the validity of the error as we're working with uninitialized memory. 
        /// Access of this property without previous checks on either <c>!</c><see cref="IsSuccess"/> or <see cref="IsFailure"/> is UNDEFINED BEHAVIOUR. 
        /// </remarks> 
        public readonly E Error => error;
#endif

        /// <summary>
        /// Determines whether this result is in success state and thus whether <see cref="Value"/> can be acessed safely.
        /// </summary>
#if NETCOREAPP1_0_OR_GREATER
        [MemberNotNullWhen(true, nameof(Value))]
        [MemberNotNullWhen(false, nameof(Error))]
#endif

        public readonly bool IsSuccess => success;

        /// <summary>
        /// Determines whether this result is in failure state and thus whether <see cref="Error"/> can be acessed safely.
        /// </summary>
#if NETCOREAPP1_0_OR_GREATER
        [MemberNotNullWhen(false, nameof(Value))]
        [MemberNotNullWhen(true, nameof(Error))]
#endif
        public readonly bool IsFailure => !success;

        /// <summary>
        /// Indicates whether the current <see cref="Result{T, E}"/> is equal to another <see cref="Result{T, E}"/>.
        /// </summary>
        /// <param name="other">The <see cref="Result{T, E}"/> to compare with the current.</param>
        /// <returns><see langword="true"/> if the current <see cref="Result{T, E}"/> is equal to the <paramref name="other"/> parameter; otherwise, <see langword="false"/>.</returns>
        public readonly bool Equals(in Result<T, E> other)
        {
            return this == other;
        }

        /// <summary>
        /// Indicates whether the current <see cref="Result{T, E}"/> is equal to another <see cref="Result{T, E}"/>.
        /// </summary>
        /// <param name="other">The <see cref="Result{T, E}"/> to compare with the current.</param>
        /// <returns><see langword="true"/> if the current <see cref="Result{T, E}"/> is equal to the <paramref name="other"/> parameter; otherwise, <see langword="false"/>.</returns>
        public readonly bool Equals(Result<T, E> other)
        {
            return this == other;
        }

        /// <summary>
        /// Indicates whether the current <see cref="Result{T, E}"/> is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current.</param>
        /// <returns><see langword="true"/> if the current <see cref="Result{T, E}"/> is equal to the <paramref name="obj"/> parameter; otherwise, <see langword="false"/>.</returns>
        public readonly override bool Equals(object? obj)
        {
            return obj is Result<T, E> result && Equals(result);
        }

        /// <summary>
        /// Indicates whether two <see cref="Result{T, E}"/> objects are equal to one another.
        /// </summary>
        /// <returns><see langword="true"/> if they are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(Result<T, E> left, Result<T, E> right)
        {
            return left.IsSuccess && right.IsSuccess && left.Value.Equals(right.Value);
        }

        /// <summary>
        /// Indicates whether two <see cref="Result{T, E}"/> objects are equal to one another.
        /// </summary>
        /// <returns><see langword="true"/> if they are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Result<T, E> left, Result<T, E> right)
        {
            return !(left == right);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static implicit operator Result<T, E>(T value)

        {
            return new Result<T, E>(value);
        }

        public static implicit operator Result<T, E>(E error)
        {
            return new Result<T, E>(error);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <exception cref="InvalidCastException"></exception>
        public static explicit operator T(in Result<T, E> result)
        {
            return result.IsSuccess ? result.value : throw new InvalidCastException(nameof(result.Value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <exception cref="InvalidCastException"></exception>
        public static explicit operator E(in Result<T, E> result)
        {
            return result.IsFailure ? result.error : throw new InvalidCastException(nameof(result.Error));
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current instance.</returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(IsSuccess, value);
        }

        public readonly void Match(Action<T> onSuccess, Action<E> onError)
        {
            if (IsSuccess)
            {
                onSuccess(value);
                return;
            }

            onError(Error);
        }

        public readonly TResult Match<TResult>(Func<T, TResult> onSuccess, Func<E, TResult> onError)
        {
            if (IsSuccess)
            {
                return onSuccess(value);
            }

            return onError(Error);
        }

        public readonly Result<U, E> Map<U>(Func<T, U> map)
        {
            if (IsFailure)
            {
                return error;
            }

            return map(value);
        }

        public readonly Result<T, U> MapError<U>(Func<E, U> map)
        {
            if (IsSuccess)
            {
                return value;
            }

            return map(error);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override readonly string ToString()
        {
            return IsSuccess ? $"Success -> Value: {value}" : $"Failure -> Error: {error}";
        }

        /// <summary>
        /// Gets the value of a successful result.
        /// </summary>
        /// <param name="value">When this method returns, contains the value contained within this <see cref="Result{T, E}"/>, if <see cref="IsSuccess"/> is <see langword="true"/>; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if <see cref="IsSuccess"/> is <see langword="true"/>; otherwise <see langword="false"/>.</returns>
        /// <remarks>This method is functionally equivalent to checking <see cref="IsSuccess"/> prior to accessing <see cref="Value"/>.</remarks>
        public bool TryGetValue(
#if NET5_0_OR_GREATER
            [NotNullWhen(true)] out T? value
#else
            out T value
#endif
            )
        {
            value = default!;

            if (IsSuccess)
            {
                value = Value;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the error of a failed result.
        /// </summary>
        /// <param name="error">When this method returns, contains the error contained within this <see cref="Result{T, E}"/>, if <see cref="IsFailure"/> is <see langword="true"/>; otherwise, the default value for the type of the <paramref name="error"/> parameter. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if <see cref="IsFailure"/> is <see langword="true"/>; otherwise <see langword="false"/>.</returns>
        /// <remarks>This method is functionally equivalent to checking <see cref="IsFailure"/> prior to accessing <see cref="Error"/>.</remarks>
        public bool TryGetError(
#if NET5_0_OR_GREATER
            [NotNullWhen(true)] out E? error
#else
            out E error
#endif
            )
        {
            error = default!;

            if (IsFailure)
            {
                error = Error;
                return true;
            }

            return false;
        }

        public static Result<T, E> Success(T value)
        {
            return new Result<T, E>(value);
        }

        public static Result<T, E> Failure(E error)
        {
            return new Result<T, E>(error);
        }
    }
}