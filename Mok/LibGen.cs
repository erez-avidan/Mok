﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Generators
{
    internal class LibGen
    {
        internal static void Generate(GeneratorExecutionContext context)
        {
            GenerateIt(context);
            GenerateIMatcher(context);
            GenerateAnyMatcher(context);
            GenerateConstantMatcher(context);
            GenerateMatcherFactory(context);
            GenerateISetup(context);
            GenerateVoidSetup(context);
            GenerateSetupT(context);
            GenerateMockHandler(context);
            GenerateTimes(context);
            GenerateException(context);
        }

        private static void GenerateException(GeneratorExecutionContext context)
        {
            var str = $@"
namespace MokMock;
[Serializable]
public class IncorrectNumberOfCallsException : Exception
{{
    public IncorrectNumberOfCallsException()
    {{
    }}

    public IncorrectNumberOfCallsException(string? message) : base(message)
    {{
    }}

    public IncorrectNumberOfCallsException(string? message, Exception? innerException) : base(message, innerException)
    {{
    }}
}}";
            context.AddSource("IncorrectNumberOfCallsException.g.cs", CSharpSyntaxTree.ParseText(str).GetRoot().NormalizeWhitespace().ToFullString());

        }

        private static void GenerateTimes(GeneratorExecutionContext context)
        {
            var str = $@"
namespace MokMock;
public class Times
{{
    public int ExpectedCalls {{ get; }}

    private Times(int expectedCalls)
    {{
        ExpectedCalls = expectedCalls;
    }}

    public static Times Once => new Times(1);
    public static Times Exactly(int times) => new Times(times);
    public static Times Never => new Times(0);
}}";

            context.AddSource("Times.g.cs", CSharpSyntaxTree.ParseText(str).GetRoot().NormalizeWhitespace().ToFullString());
        }

        private static void GenerateMockHandler(GeneratorExecutionContext context)
        {
            var str = $@"
using System.Linq.Expressions;
using System.Collections;

namespace MokMock;
internal class MockHandler
{{
    internal Dictionary<string, List<ISetup>> setups = [];
    internal Dictionary<string, List<object[]>> calls = [];

    internal MockHandler()
    {{
    }}

    internal void CallVoid(string methodName, object[] parameters)
    {{
        AuditCall(methodName, parameters);

        var setup = FindMatchingSetup(methodName, parameters);
        if (setup != null)
        {{
            ((VoidSetup)setup).action?.Invoke();
        }}
    }}

     internal T? CallReturnValue<T>(string methodName, object[] parameters)
    {{
        AuditCall(methodName, parameters);

        var setup = FindMatchingSetup(methodName, parameters);
        if (setup != null)
        {{
            var returnFunc = ((Setup<T>)setup).returnValue;
            if (returnFunc != null)
            {{
                return returnFunc();
            }}
        }}

        return default;
    }}

    private void AuditCall(string methodName, object[] parameters)
    {{
        if (!calls.TryGetValue(methodName, out List<object[]>? value))
        {{
            value = [];
            calls[methodName] = value;
        }}

        var cloned = parameters.Select(DeepClone).ToArray();

        value.Add(cloned);
    }}

    private object DeepClone(object obj)
    {{
        if (obj == null) return null;

        if (obj.GetType().IsValueType || obj is string)
            return obj;

        if (obj is ICloneable cloneable)
            return cloneable.Clone();

        if (obj is IEnumerable enumerable)
            return CloneCollection(enumerable);

        return SerializationClone(obj);
    }}

 private object CloneCollection(IEnumerable collection)
    {{
        var type = collection.GetType();
        if (type.IsArray)
        {{
            var array = (Array)collection;
            var clonedArray = Array.CreateInstance(type.GetElementType(), array.Length);
            for (int i = 0; i < array.Length; i++)
            {{
                clonedArray.SetValue(DeepClone(array.GetValue(i)), i);
            }}
            return clonedArray;
        }}

        // Handle other collection types like List<T>, HashSet<T>, etc.
        if (type.IsGenericType)
        {{
            var genericArgs = type.GetGenericArguments();
            var constructedListType = typeof(List<>).MakeGenericType(genericArgs);
            var clonedList = (IList)Activator.CreateInstance(constructedListType);

            foreach (var item in collection)
            {{
                clonedList.Add(DeepClone(item));
            }}
            return clonedList;
        }}

        return collection;
    }}

    private object SerializationClone(object obj)
    {{
        try
        {{
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            var bytes = (byte[])obj;
            writer.Write(bytes);
            stream.Position = 0;
            using var reader = new BinaryReader(stream);
            return reader.ReadBytes(bytes.Length);
        }}
        catch
        {{
            return $""[Uncloneable object: {{obj}}]"";
        }}
    }}

    private ISetup? FindMatchingSetup(string methodName, object[] parameters)
    {{
        if (!setups.TryGetValue(methodName, out List<ISetup>? candidates))
        {{
            return null;
        }}

        foreach ( var candidate in candidates )
        {{
            int index = 0;
            bool match = true;
            foreach (var matcher in candidate.matchers)
            {{
                if (!matcher.IsMatching(parameters[index]))
                {{
                    match = false; 
                    break;
                }}
                
                index++;
            }}

            if (match)
            {{
                return candidate;
            }}
        }}

        return null;
    }}

    internal void AddSetup(MethodCallExpression body, ISetup setup)
    {{
        if (!setups.TryGetValue(body.Method.Name, out List<ISetup>? value))
        {{
            value = [];
            setups.Add(body.Method.Name, value);
        }}

        value.Add(setup);
    }}
}}";
            context.AddSource("MockHandler.g.cs", CSharpSyntaxTree.ParseText(str).GetRoot().NormalizeWhitespace().ToFullString());
        }

        private static void GenerateSetupT(GeneratorExecutionContext context)
        {
            var str = $@"
namespace MokMock;
public class Setup<T> : ISetup
{{
    public IEnumerable<IMatcher> matchers {{ get; }}
    public Func<T> returnValue {{ get; set; }}

    public Setup(IEnumerable<IMatcher> matchers)
    {{
        this.matchers = matchers;
    }}

    public void Returns(T value)
    {{
        returnValue = () => value;
    }}

    public void Returns(Func<T> value)
    {{
        returnValue = value;
    }}

    public void Throws(Exception exception)
    {{
        returnValue = () => {{ throw exception; }};
    }}
}}";
            context.AddSource("SetupT.g.cs", CSharpSyntaxTree.ParseText(str).GetRoot().NormalizeWhitespace().ToFullString());
        }

        private static void GenerateVoidSetup(GeneratorExecutionContext context)
        {
            var str = $@"
namespace MokMock;
public class VoidSetup : ISetup
{{
    public IEnumerable<IMatcher> matchers {{ get; }}
    public Action action {{ get; set; }}

    public VoidSetup(IEnumerable<IMatcher> matchers)
    {{
        this.matchers = matchers;
    }}

    public void Callback(Action callback)
    {{
        action = callback;
    }}

    public void Throws(Exception exception)
    {{
        action = () => {{ throw exception; }};
    }}
}}";
            context.AddSource("VoidSetup.g.cs", CSharpSyntaxTree.ParseText(str).GetRoot().NormalizeWhitespace().ToFullString());
        }

        private static void GenerateISetup(GeneratorExecutionContext context)
        {
            var str = $@"
namespace MokMock;
public interface ISetup
{{
    IEnumerable<IMatcher> matchers {{ get; }}
}}";
            context.AddSource("ISetup.g.cs", CSharpSyntaxTree.ParseText(str).GetRoot().NormalizeWhitespace().ToFullString());
        }

        private static void GenerateMatcherFactory(GeneratorExecutionContext context)
        {
            var str = $@"
using System.Linq.Expressions;
namespace MokMock;
internal static class MatcherFactory
{{
    private static AnyMatcher AnyMatcher = new AnyMatcher();
    internal static IMatcher GetMatcher(Expression expression)
    {{
        if (expression is ConstantExpression constantExp)
        {{
            return new ConstantMatcher(constantExp.Value);
        }}

        if (expression is MethodCallExpression methodExp)
        {{
            if (methodExp.Method.DeclaringType?.Name == ""It"" && methodExp.Method.Name == ""IsAny"")
            {{
                return AnyMatcher;
            }}
        }}

        if (expression is MemberExpression memberExp)
        {{
            LambdaExpression lambda = Expression.Lambda(expression);
            Delegate fn = lambda.Compile();
            return new ConstantMatcher(Expression.Constant(fn.DynamicInvoke(null), expression.Type).Value);
        }}

        return null;
    }}
}}";
            context.AddSource("MatcherFactory.g.cs", CSharpSyntaxTree.ParseText(str).GetRoot().NormalizeWhitespace().ToFullString());
        }

        private static void GenerateConstantMatcher(GeneratorExecutionContext context)
        {
            var str = $@"
namespace MokMock;
internal class ConstantMatcher : IMatcher
{{
    object Value {{ get; set; }}
    internal ConstantMatcher(object val)
    {{
        Value = val;
    }}

    public bool IsMatching(object arg)
    {{
        return object.Equals(Value, arg);
    }}
}}";
            context.AddSource("ConstantMatcher.g.cs", CSharpSyntaxTree.ParseText(str).GetRoot().NormalizeWhitespace().ToFullString());
        }

        private static void GenerateAnyMatcher(GeneratorExecutionContext context)
        {
            var str = $@"
namespace MokMock;
internal class AnyMatcher : IMatcher
{{
    public bool IsMatching(object arg) => true;
}}";
            context.AddSource("AnyMatcher.g.cs", CSharpSyntaxTree.ParseText(str).GetRoot().NormalizeWhitespace().ToFullString());
        }

        private static void GenerateIMatcher(GeneratorExecutionContext context)
        {
            var str = $@"
            namespace MokMock;
            public interface IMatcher
            {{
                bool IsMatching(object arg);
            }}";

            context.AddSource("IMatcher.g.cs", CSharpSyntaxTree.ParseText(str).GetRoot().NormalizeWhitespace().ToFullString());
        }

        private static void GenerateIt(GeneratorExecutionContext context)
        {
            var str = $@"
namespace MokMock;
public static class It
{{
    public static TValue IsAny<TValue>()
    {{
        return default;
    }}
}}";

            context.AddSource("It.g.cs", CSharpSyntaxTree.ParseText(str).GetRoot().NormalizeWhitespace().ToFullString());
        }
    }
}