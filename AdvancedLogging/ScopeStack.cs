namespace AdvancedLogging
{
    // Ambient scope storage using AsyncLocal
    internal static class ScopeStack
    {
        private static readonly AsyncLocal<Stack<Dictionary<string, object>>> _scopes = new();
        public static IDisposable Push(Dictionary<string, object> values)
        {
            var stack = _scopes.Value ??= new Stack<Dictionary<string, object>>();
            stack.Push(values);
            return new PopWhenDisposed(stack);
        }
        public static void Enrich(LogEvent e)
        {
            var stack = _scopes.Value;
            if (stack == null || stack.Count == 0) return;
            foreach (var map in stack.Reverse())
            {
                foreach (var kv in map)
                    e.Properties[kv.Key] = kv.Value;
            }
        }
        private sealed class PopWhenDisposed : IDisposable
        {
            private readonly Stack<Dictionary<string, object>> _stack;
            private bool _disposed;
            public PopWhenDisposed(Stack<Dictionary<string, object>> stack) { _stack = stack; }
            public void Dispose()
            {
                if (_disposed) return; _disposed = true;
                if (_stack.Count > 0) _stack.Pop();
            }
        }
    }
}
