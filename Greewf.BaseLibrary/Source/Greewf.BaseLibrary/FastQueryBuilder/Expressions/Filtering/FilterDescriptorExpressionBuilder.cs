// (c) Copyright 2002-2010 Telerik 
// This source is subject to the GNU General Public License, version 2
// See http://www.gnu.org/licenses/gpl-2.0.html. 
// All other rights reserved.

namespace Greewf.BaseLibrary.FastQueryBuilder.Infrastructure.Implementation.Expressions
{
    using System;
    using System.Globalization;
    using System.Linq.Expressions;

    using Extensions;
    using System.Collections;
    using System.Collections.Generic;

    internal class FilterDescriptorExpressionBuilder : FilterExpressionBuilder
    {
        private readonly FilterDescriptor descriptor;

        public FilterDescriptorExpressionBuilder(ParameterExpression parameterExpression, FilterDescriptor descriptor)
            : base(parameterExpression)
        {
            this.descriptor = descriptor;
        }

        public FilterDescriptor FilterDescriptor
        {
            get
            {
                return this.descriptor;
            }
        }

        /// <exception cref="ArgumentException"><c>ArgumentException</c>.</exception>
        public override Expression CreateBodyExpression()
        {
            Expression memberExpression = this.CreateMemberExpression();

            Type memberType = memberExpression.Type;

            Expression valueExpression = CreateValueExpression(memberType, this.descriptor.Value, CultureInfo.InvariantCulture);

            bool isConversionSuccessful = true;

            if (TypesAreDifferent(this.descriptor, memberExpression, valueExpression))
            {
                if (!TryConvertExpressionTypes(ref memberExpression, ref valueExpression))
                {
                    isConversionSuccessful = false;
                }
            }
            else if (!valueExpression.Type.IsArray && (memberExpression.Type.IsEnumType() || valueExpression.Type.IsEnumType())) //edited by moravej to support arrays
            {
                if (!TryPromoteNullableEnums(ref memberExpression, ref valueExpression))
                {
                    isConversionSuccessful = false;
                }
            }
            else if (!valueExpression.Type.IsArray && (memberType.IsNullableType() && (memberExpression.Type != valueExpression.Type))) //edited by moravej to support arrays
            {
                if (!TryConvertNullableValue(memberExpression, ref valueExpression))
                {
                    isConversionSuccessful = false;
                }
            }

            if (!isConversionSuccessful)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Operator '{0}' is incompatible with operand types '{1}' and '{2}'",
                        this.descriptor.Operator,
                        memberExpression.Type.GetTypeName(),
                        valueExpression.Type.GetTypeName()));
            }

            return this.descriptor.Operator.CreateExpression(memberExpression, valueExpression, Options.LiftMemberAccessToNull);
        }

        public FilterDescription CreateFilterDescription()
        {
            LambdaExpression filterExpression = this.CreateFilterExpression();

            Delegate predicate = filterExpression.Compile();

            return new PredicateFilterDescription(predicate);
        }

        protected virtual Expression CreateMemberExpression()
        {
            var memberType = this.FilterDescriptor.MemberType;

            var memberAccessBuilder =
                            ExpressionBuilderFactory.MemberAccess(this.ParameterExpression.Type, memberType, this.FilterDescriptor.Member);
            memberAccessBuilder.Options.CopyFrom(this.Options);

            memberAccessBuilder.ParameterExpression = this.ParameterExpression;

            Expression memberAccessExpression = memberAccessBuilder.CreateMemberAccessExpression();

            if (memberType != null && memberAccessExpression.Type.GetNonNullableType() != memberType.GetNonNullableType())
            {
                memberAccessExpression = Expression.Convert(memberAccessExpression, memberType);
            }

            return memberAccessExpression;
        }

        private static Expression CreateConstantExpression(object value)
        {
            if (value == null)
            {
                return ExpressionConstants.NullLiteral;
            }
            return Expression.Constant(value);
        }

        private static Expression CreateValueExpression(Type targetType, object value, CultureInfo culture)//edited by moravej
        {
            if (((targetType != typeof(string)) && (!targetType.IsValueType || targetType.IsNullableType())) &&
                (string.Compare(value as string, "null", StringComparison.OrdinalIgnoreCase) == 0))
            {
                value = null;
            }
            if (value != null)
            {
                Type nonNullableTargetType = targetType.GetNonNullableType();
                var valueType = value.GetType();//added by moravej
                if (valueType != nonNullableTargetType)
                {
                    if (nonNullableTargetType.IsEnum)
                    {
                        if (valueType.IsArray)//by moravej
                            value = CreateArrayValue(targetType, nonNullableTargetType, value, true, false, null);
                        else
                            value = Enum.Parse(nonNullableTargetType, value.ToString(), true);


                    }
                    else if (value is IConvertible || (valueType.IsArray && valueType.GetElementType().IsAssignableFrom(typeof(IConvertible))))
                    {
                        if (valueType.IsArray)//by moravej
                            value = CreateArrayValue(targetType, nonNullableTargetType, value, false, true, culture);
                        else
                            value = Convert.ChangeType(value, nonNullableTargetType, culture);
                    }

                }
            }

            return CreateConstantExpression(value);
        }

        private static Array CreateArrayValue(Type targetType, Type nonNullableTargetType, object value, bool isEnumType, bool isIConvertible, CultureInfo culture)
        {
            var result = Array.CreateInstance(targetType, ((Array)value).Length);//we use targetType instead of nonNullableTargetType because nullable list can check not nullable value , but the viceversa is not correct
            int idx = 0;
            bool isNullableType = targetType != nonNullableTargetType;


            foreach (var itemValue in (IEnumerable)value)
            {
                if (!isNullableType && itemValue == null)
                    throw new Exception("ExtJsGridAction : You passed a null value for a list item, but the target field does not support null values. ");
                
                else if (itemValue == null)
                    result.SetValue(null, idx++);
                
                else if (isEnumType)
                    result.SetValue(Enum.Parse(nonNullableTargetType, itemValue.ToString(), true), idx++);
                
                else if (isIConvertible)
                    result.SetValue(Convert.ChangeType(itemValue, nonNullableTargetType, culture), idx++);
            }
            return result;
        }

        private static Expression PromoteExpression(Expression expr, Type type, bool exact)
        {
            if (expr.Type == type)
            {
                return expr;
            }
            var ce = expr as ConstantExpression;
            //TODO: check here
            if (((ce != null) && (ce == ExpressionConstants.NullLiteral)) && !(type.IsValueType && !type.IsNullableType()))
            {
                return Expression.Constant(null, type);
            }
            if (expr.Type.IsCompatibleWith(type))
            {
                if (type.IsValueType || exact)
                {
                    return Expression.Convert(expr, type);
                }
                return expr;
            }
            return null;
        }

        private static bool TryConvertExpressionTypes(ref Expression memberExpression, ref Expression valueExpression)
        {
            if (memberExpression.Type != valueExpression.Type)
            {
                if (!memberExpression.Type.IsAssignableFrom(valueExpression.Type))
                {
                    if (!valueExpression.Type.IsAssignableFrom(memberExpression.Type))
                    {
                        return false;
                    }
                    memberExpression = Expression.Convert(memberExpression, valueExpression.Type);
                }
                else
                {
                    valueExpression = Expression.Convert(valueExpression, memberExpression.Type);
                }
            }

            return true;
        }

        private static bool TryConvertNullableValue(Expression memberExpression, ref Expression valueExpression)
        {
            var ce = valueExpression as ConstantExpression;
            if (ce != null)
            {
                try
                {
                    valueExpression = Expression.Constant(ce.Value, memberExpression.Type);
                }
                catch (ArgumentException)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool TryPromoteNullableEnums(ref Expression memberExpression, ref Expression valueExpression)
        {
            if (memberExpression.Type != valueExpression.Type)
            {
                Expression e = PromoteExpression(valueExpression, memberExpression.Type, true);
                if (e == null)
                {
                    e = PromoteExpression(memberExpression, valueExpression.Type, true);
                    if (e == null)
                    {
                        return false;
                    }
                    memberExpression = e;
                }
                else
                {
                    valueExpression = e;
                }
            }
            return true;
        }

        private static bool TypesAreDifferent(FilterDescriptor descriptor, Expression memberExpression, Expression valueExpression)
        {
            bool isEqualityOperator = descriptor.Operator == FilterOperator.IsEqualTo ||
                                      descriptor.Operator == FilterOperator.IsNotEqualTo;

            return isEqualityOperator && !memberExpression.Type.IsValueType && !valueExpression.Type.IsValueType;
        }
    }
}