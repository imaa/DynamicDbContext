using DynamicDbContext.DynamicHelper;
using DynamicDbContext.DynamicHelper.Enums;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynamicDbContext.Extentions
{
    public static class LinqExtentions
    {

        private static PropertyInfo GetPropertyInfo(Type objType, string name)
        {
            PropertyInfo[] properties = objType.GetProperties();
            PropertyInfo matchedProperty = properties.FirstOrDefault(p => p.Name == name);
            if (matchedProperty == null)
            {
                throw new ArgumentException("name");
            }

            return matchedProperty;
        }
        private static LambdaExpression GetOrderExpression(Type objType, PropertyInfo pi)
        {
            ParameterExpression paramExpr = Expression.Parameter(objType);
            MemberExpression propAccess = Expression.PropertyOrField(paramExpr, pi.Name);
            LambdaExpression expr = Expression.Lambda(propAccess, paramExpr);
            return expr;
        }

        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> query, string name)
        {
            PropertyInfo propInfo = GetPropertyInfo(typeof(T), name);
            LambdaExpression expr = GetOrderExpression(typeof(T), propInfo);

            MethodInfo method = typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == "OrderBy" && m.GetParameters().Length == 2);
            MethodInfo genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            return (IEnumerable<T>)genericMethod.Invoke(null, new object[] { query, expr.Compile() });
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string name)
        {
            PropertyInfo propInfo = GetPropertyInfo(typeof(T), name);
            LambdaExpression expr = GetOrderExpression(typeof(T), propInfo);

            MethodInfo method = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "OrderBy" && m.GetParameters().Length == 2);
            MethodInfo genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            return (IQueryable<T>)genericMethod.Invoke(null, new object[] { query, expr });
        }
        public static IEnumerable<T> OrderByDescending<T>(this IEnumerable<T> query, string name)
        {
            PropertyInfo propInfo = GetPropertyInfo(typeof(T), name);
            LambdaExpression expr = GetOrderExpression(typeof(T), propInfo);

            MethodInfo method = typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2);
            MethodInfo genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            return (IEnumerable<T>)genericMethod.Invoke(null, new object[] { query, expr.Compile() });
        }
        public static IEnumerable<object> AsEnumerable(this DbSet set)
        {
            foreach (object entity in set)
            {
                yield return entity;
            }
        }
        public static IQueryable<T> OrderByDescending<T>(this IQueryable<T> query, string name)
        {
            PropertyInfo propInfo = GetPropertyInfo(typeof(T), name);
            LambdaExpression expr = GetOrderExpression(typeof(T), propInfo);

            MethodInfo method = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2);
            MethodInfo genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            return (IQueryable<T>)genericMethod.Invoke(null, new object[] { query, expr });
        }





        public static IQueryable OrderBy(this IQueryable query, string sortColumn, SortDirection direction)
        {
            string methodName = string.Format("OrderBy{0}",
            direction == SortDirection.Asc ? string.Empty : "Descending");
            Expression queryExpr = query.Expression;
            foreach (string pp in sortColumn.Split(','))
            {
                foreach (string item in pp.Split('.'))
                {


                    ParameterExpression selectorParam = Expression.Parameter(query.ElementType, "e");
                    LambdaExpression selector = Expression.Lambda(Expression.PropertyOrField(selectorParam, item), selectorParam);
                    string method = methodName;
                    queryExpr = Expression.Call(typeof(Queryable), method,
                    new Type[] { selectorParam.Type, selector.Body.Type },
                    queryExpr, Expression.Quote(selector));
                    methodName = string.Format(direction == SortDirection.Asc ? "ThenBy" : "ThenByDescending");
                }
            }
            return query.Provider.CreateQuery(queryExpr);
        }

        public static bool ContainsNoCase(this string self, string match)
        {
            return self.IndexOf(match, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }


        public static object ConvertValue(string typeInString, string value)
        {
            Type originalType = Type.GetType(typeInString);

            Type underlyingType = Nullable.GetUnderlyingType(originalType);

            // if underlyingType has null value, it means the original type wasn't nullable
            object instance = Convert.ChangeType(value, underlyingType ?? originalType);

            return instance;
        }

        public static List<T> FullTextSearch<T>(this List<T> list, string searchKey)
        {

            ParameterExpression parameter = Expression.Parameter(typeof(T), "c");
            MethodInfo containsMethod = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
            IEnumerable<PropertyInfo> publicProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(p => p.PropertyType == typeof(string));
            Expression orExpressions = null;

            foreach (var callContainsMethod in from property in publicProperties
                                               let myProperty = Expression.Property(parameter, property.Name)
                                               let myExpression = Expression.Call(myProperty, "Contains", null, Expression.Constant(searchKey))
                                               let myNullExp = Expression.Call(typeof(string), (typeof(string).GetMethod("IsNullOrEmpty")).Name, null, myProperty)
                                               let myNotExp = Expression.Not(myNullExp)
                                               select new { myExpression, myNotExp })
            {
                BinaryExpression andAlso = Expression.AndAlso(callContainsMethod.myNotExp, callContainsMethod.myExpression);
                if (orExpressions == null)
                {
                    orExpressions = andAlso;
                }
                else
                {
                    orExpressions = Expression.Or(orExpressions, andAlso);
                }
            }

            IQueryable<T> queryable = list.AsQueryable<T>();
            MethodCallExpression whereCallExpression = Expression.Call(
            typeof(Queryable),
            "Where",
            new Type[] { queryable.ElementType },
            queryable.Expression,
            Expression.Lambda<Func<T, bool>>(orExpressions, new ParameterExpression[] { parameter }));
            List<T> results = queryable.Provider.CreateQuery<T>(whereCallExpression).ToList();
            return results;

        }

        public static IQueryable Where(this DbSet dbSet, DbContext context, PropertyInfo dbSetPropertyInfo,
        string column, object value, WhereOperation operation)
        {
            MethodInfo mAsQueryable = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "AsQueryable" && m.IsGenericMethod);
            MethodInfo genericAsQueryableMethod = mAsQueryable.MakeGenericMethod(dbSet.ElementType);
            object o = dbSetPropertyInfo.GetValue(context, null);
            object dbSetAsQueryable = genericAsQueryableMethod.Invoke(null, new object[] { o });
            return (dbSetAsQueryable as IQueryable).Where(column, value, operation);

        }
        public static IQueryable Where(this DbSet dbSet, DbContext context, PropertyInfo dbSetPropertyInfo,
       List<CustomDynamicExpression> dynamicExpressions)
        {
            MethodInfo mAsQueryable = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "AsQueryable" && m.IsGenericMethod);
            MethodInfo genericAsQueryableMethod = mAsQueryable.MakeGenericMethod(dbSet.ElementType);
            object o = dbSetPropertyInfo.GetValue(context, null);
            object dbSetAsQueryable = genericAsQueryableMethod.Invoke(null, new object[] { o });
            return (dbSetAsQueryable as IQueryable).Where(dynamicExpressions);

        }
        private static IQueryable Where(this IQueryable query, List<CustomDynamicExpression> dynamicExpressions)
        {
            if (dynamicExpressions == null || dynamicExpressions.Count() == 0)
            {
                return query;
            }

            ParameterExpression parameter = Expression.Parameter(query.ElementType, "p");

            List<Expression> expressions = GetExpresions(dynamicExpressions, parameter);

            Expression finalExpression = expressions[0];
            if (expressions.Count > 1)
            {
                for (int i = 1; i < expressions.Count; i++)
                {
                    finalExpression = Expression.And(finalExpression, expressions[i]);
                }
            }
            LambdaExpression lambda = Expression.Lambda(finalExpression, parameter);
            MethodCallExpression result = Expression.Call(
        typeof(Queryable), "Where",
        new[] { query.ElementType },
        query.Expression,
        lambda);
            return query.Provider.CreateQuery(result);

        }

        private static List<Expression> GetExpresions(List<CustomDynamicExpression> dynamicExpressions, ParameterExpression parameter)
        {
            List<Expression> expressions = new List<Expression>();
            foreach (CustomDynamicExpression
                dynamicExpression in dynamicExpressions)
            {
                if (dynamicExpression.DynamicCondition == null || string.IsNullOrEmpty(dynamicExpression.DynamicCondition.Column))
                {
                    continue;
                }

                MemberExpression memberAccess = MemberExpression.Property
                (parameter, dynamicExpression.DynamicCondition.Column);

                ConstantExpression filter;
                if (memberAccess.Type.IsGenericType && memberAccess.Type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                {
                    string val = dynamicExpression.DynamicCondition.Column.ToLower().Contains("date") || dynamicExpression.DynamicCondition.Column.ToLower().Contains("renewDay") ? DateTime.Parse(dynamicExpression.DynamicCondition.Value.ToString()).ToString() : dynamicExpression.DynamicCondition.Value.ToString();
                    filter = Expression.Constant(
                    Convert.ChangeType(val, memberAccess.Type.GetGenericArguments()[0]));
                }
                else
                {
                    string val = dynamicExpression.DynamicCondition.Column.ToLower().Contains("date") || dynamicExpression.DynamicCondition.Column.ToLower().Contains("renewDay") ? DateTime.Parse(dynamicExpression.DynamicCondition.Value.ToString()).ToString() : dynamicExpression.DynamicCondition.Value.ToString();
                    filter = Expression.Constant
                    (
                    Convert.ChangeType(val, memberAccess.Type)
                    );
                }
                Expression typeFilter = Expression.Convert(filter, memberAccess.Type);
                Expression condition = GetCondition(dynamicExpression, memberAccess, typeFilter);
                //  expressions.Add(condition);
                Expression finalExpression = condition;
                if (dynamicExpression.Operator != null && dynamicExpression.DynamicExpressions.Count() > 0)
                {
                    List<Expression> exp = GetExpresions(dynamicExpression.DynamicExpressions, parameter);

                    if (exp.Count > 0)
                    {
                        for (int i = 0; i < exp.Count; i++)
                        {
                            if (dynamicExpression.Operator == Operator.And)
                            {
                                finalExpression = Expression.And(finalExpression, exp[i]);
                            }
                            else
                            {
                                finalExpression = Expression.Or(finalExpression, exp[i]);
                            }
                        }
                    }
                }
                expressions.Add(finalExpression);
            }
            return expressions;
        }

        private static Expression GetCondition(CustomDynamicExpression dynamicExpression, MemberExpression memberAccess, Expression typeFilter)
        {
            switch (dynamicExpression.DynamicCondition.WhereOperation)
            {
                case WhereOperation.DateEqual:
                    return Expression.Equal(memberAccess, typeFilter);
                case WhereOperation.Equal:
                    return Expression.Equal(memberAccess, typeFilter);
                case WhereOperation.NotEqual:
                    return Expression.NotEqual(memberAccess, typeFilter);
                case WhereOperation.Contains:
                    return Expression.Call(memberAccess,
                    typeof(string).GetMethod("Contains"),
                    Expression.Constant(dynamicExpression.DynamicCondition.Value));
                case WhereOperation.LessThan:
                    return Expression.LessThan(memberAccess, typeFilter);
                case WhereOperation.LessThanOrEqual:
                    return Expression.LessThanOrEqual(memberAccess, typeFilter);
                case WhereOperation.GreaterThan:
                    return Expression.GreaterThan(memberAccess, typeFilter);
                case WhereOperation.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(memberAccess, typeFilter);
                case WhereOperation.BeginsWith:
                    return Expression.Call(memberAccess,
                    typeof(string).GetMethod("StartsWith", new[] { typeof(string) }),
                    Expression.Constant(dynamicExpression.DynamicCondition.Value));
                case WhereOperation.NotBeginsWith:
                    return Expression.Not(Expression.Call(memberAccess,
                    typeof(string).GetMethod("StartsWith", new[] { typeof(string) }),
                    Expression.Constant(dynamicExpression.DynamicCondition.Value)));
                case WhereOperation.In:
                    return Expression.Call(memberAccess,
                      typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                      Expression.Constant(dynamicExpression.DynamicCondition.Value));
                case WhereOperation.NotIn:
                    return Expression.Not(Expression.Call(memberAccess,
                     typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                     Expression.Constant(dynamicExpression.DynamicCondition.Value)));
                case WhereOperation.EndWith:
                    return Expression.Call(memberAccess,
                    typeof(string).GetMethod("EndsWith", new[] { typeof(string) }),
                    Expression.Constant(dynamicExpression.DynamicCondition.Value));
                case WhereOperation.NotEndWith:
                    return Expression.Not(Expression.Call(memberAccess,
                    typeof(string).GetMethod("EndsWith", new[] { typeof(string) }),
                    Expression.Constant(dynamicExpression.DynamicCondition.Value)));
                case WhereOperation.NotContains:
                    return Expression.Not(Expression.Call(memberAccess,
                     typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                     Expression.Constant(dynamicExpression.DynamicCondition.Value)));
                case WhereOperation.Null:
                    return Expression.Call(memberAccess,
                     typeof(string).GetMethod("IsNullOrEmpty"),
                     Expression.Constant(dynamicExpression.DynamicCondition.Value));
                case WhereOperation.NotNull:
                    return Expression.Not(Expression.Call(memberAccess,
                    typeof(string).GetMethod("IsNullOrEmpty"),
                    Expression.Constant(dynamicExpression.DynamicCondition.Value)));
                default:
                    throw new NotImplementedException(dynamicExpression.DynamicCondition.WhereOperation.ToString());
            }
        }

        public static IQueryable Where(this IQueryable query,
        string column, object value, WhereOperation operation)
        {
            if (string.IsNullOrEmpty(column))
            {
                return query;
            }

            ParameterExpression parameter = Expression.Parameter(query.ElementType, "p");

            MemberExpression memberAccess = null;
            foreach (string property in column.Split('.'))
            {
                memberAccess = MemberExpression.Property
                (memberAccess ?? (parameter as Expression), property);
            }

            ConstantExpression filter;
            if (memberAccess.Type.IsGenericType && memberAccess.Type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                string val = column.ToLower().Contains("date") || column.ToLower().Contains("renewDay") ? DateTime.Parse(value.ToString()).ToString("dd-MM-yyyy") : value.ToString();
                filter = Expression.Constant(
                Convert.ChangeType(val, memberAccess.Type.GetGenericArguments()[0]));
            }
            else
            {
                string val = column.ToLower().Contains("date") || column.ToLower().Contains("renewDay") ? DateTime.Parse(value.ToString()).ToString("dd-MM-yyyy") : value.ToString();
                filter = Expression.Constant
                (
                Convert.ChangeType(val, memberAccess.Type)
                );
            }
            Expression typeFilter = Expression.Convert(filter, memberAccess.Type);


            Expression condition;
            LambdaExpression lambda = null;
            switch (operation)
            {
                case WhereOperation.DateEqual:
                    condition = Expression.Equal(memberAccess, typeFilter);
                    lambda = Expression.Lambda(condition, parameter);
                    break;
                case WhereOperation.Equal:
                    condition = Expression.Equal(memberAccess, typeFilter);
                    BinaryExpression condition2 = Expression.Equal(memberAccess, typeFilter);
                    lambda = Expression.Lambda(Expression.Or(condition, condition), parameter);
                    break;
                case WhereOperation.NotEqual:
                    condition = Expression.NotEqual(memberAccess, typeFilter);
                    lambda = Expression.Lambda(condition, parameter);
                    break;
                case WhereOperation.Contains:
                    condition = Expression.Call(memberAccess,
                    typeof(string).GetMethod("Contains"),
                    Expression.Constant(value));
                    lambda = Expression.Lambda(condition, parameter);
                    break;
                case WhereOperation.LessThan:
                    condition = Expression.LessThan(memberAccess, typeFilter);
                    lambda = Expression.Lambda(condition, parameter);
                    break;
                case WhereOperation.LessThanOrEqual:
                    condition = Expression.LessThanOrEqual(memberAccess, typeFilter);
                    lambda = Expression.Lambda(condition, parameter);
                    break;
                case WhereOperation.GreaterThan:
                    condition = Expression.GreaterThan(memberAccess, typeFilter);
                    lambda = Expression.Lambda(condition, parameter);
                    break;
                case WhereOperation.GreaterThanOrEqual:
                    condition = Expression.GreaterThanOrEqual(memberAccess, typeFilter);
                    lambda = Expression.Lambda(condition, parameter);
                    break;
                case WhereOperation.BeginsWith:
                    condition = Expression.Call(memberAccess,
                    typeof(string).GetMethod("StartsWith", new[] { typeof(string) }),
                    Expression.Constant(value));
                    lambda = Expression.Lambda(condition, parameter);
                    break;
                case WhereOperation.NotBeginsWith:
                    condition = Expression.Call(memberAccess,
                    typeof(string).GetMethod("StartsWith", new[] { typeof(string) }),
                    Expression.Constant(value));
                    condition = Expression.Not(condition);
                    lambda = Expression.Lambda(condition, parameter);
                    break;
                case WhereOperation.In:
                    condition = Expression.Call(memberAccess,
                    typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                    Expression.Constant(value));
                    lambda = Expression.Lambda(condition, parameter);
                    break;
                case WhereOperation.NotIn:
                    condition = Expression.Call(memberAccess,
                    typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                    Expression.Constant(value));
                    condition = Expression.Not(condition);
                    lambda = Expression.Lambda(condition, parameter);
                    break;
                case WhereOperation.EndWith:
                    condition = Expression.Call(memberAccess,
                    typeof(string).GetMethod("EndsWith", new[] { typeof(string) }),
                    Expression.Constant(value));
                    lambda = Expression.Lambda(condition, parameter);
                    break;
                case WhereOperation.NotEndWith:
                    condition = Expression.Call(memberAccess,
                    typeof(string).GetMethod("EndsWith", new[] { typeof(string) }),
                    Expression.Constant(value));
                    condition = Expression.Not(condition);
                    lambda = Expression.Lambda(condition, parameter);
                    break;
                case WhereOperation.NotContains:
                    condition = Expression.Call(memberAccess,
                    typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                    Expression.Constant(value));
                    condition = Expression.Not(condition);
                    lambda = Expression.Lambda(condition, parameter);
                    break;
                case WhereOperation.Null:
                    condition = Expression.Call(memberAccess,
                    typeof(string).GetMethod("IsNullOrEmpty"),
                    Expression.Constant(value));
                    lambda = Expression.Lambda(condition, parameter);
                    break;
                case WhereOperation.NotNull:
                    condition = Expression.Call(memberAccess,
                    typeof(string).GetMethod("IsNullOrEmpty"),
                    Expression.Constant(value));
                    condition = Expression.Not(condition);
                    lambda = Expression.Lambda(condition, parameter);
                    break;
            }
            MethodCallExpression result = Expression.Call(
            typeof(Queryable), "Where",
            new[] { query.ElementType },
            query.Expression,
            lambda);
            return query.Provider.CreateQuery(result);
        }
        public static IQueryable Skip(this IQueryable query, int count)
        {

            MethodCallExpression result = Expression.Call(
            typeof(Queryable), "Skip",
            new[] { query.ElementType },
            query.Expression,
             Expression.Constant(count));
            return query.Provider.CreateQuery(result);
        }

        public static IQueryable Take(this IQueryable query, int count)
        {
            MethodCallExpression result = Expression.Call(
            typeof(Queryable), "Take",
            new[] { query.ElementType },
            query.Expression,
              Expression.Constant(count));
            return query.Provider.CreateQuery(result);
        }
        public static int Count(this IQueryable query)
        {
            MethodCallExpression result = Expression.Call(
            typeof(Queryable), "Count",
            new[] { query.ElementType },
            query.Expression);
            return (int)query.Provider.Execute(result);
        }
    }
}
