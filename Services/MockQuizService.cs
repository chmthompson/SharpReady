using DotNetStudyAssistant.Models;
using DotNetStudyAssistant.Models.Enums;

namespace DotNetStudyAssistant.Services;

public class MockQuizService : IQuizService
{
    private readonly List<Topic> _topics;
    private readonly List<Question> _questions;
    private readonly List<QuizSession> _sessions = [];
    private readonly List<UserProgress> _progress = [];

    public MockQuizService()
    {
        _topics = SeedTopics();
        _questions = SeedQuestions();
    }

    public Task<List<Topic>> GetTopicsAsync() => Task.FromResult(_topics);

    public Task<List<Question>> GetQuestionsAsync(int topicId, DifficultyLevel? difficulty = null, int count = 10)
    {
        var allForTopic = _questions.Where(q => q.TopicId == topicId).ToList();
        IEnumerable<Question> pool = allForTopic;

        if (difficulty.HasValue)
        {
            var atLevel = allForTopic.Where(q => q.Difficulty == difficulty.Value).ToList();
            // Fall back to all difficulties when not enough questions at the selected level
            pool = atLevel.Count >= count ? atLevel : allForTopic;
        }

        var result = pool.OrderBy(_ => Guid.NewGuid()).Take(count).ToList();
        return Task.FromResult(result);
    }

    public Task SaveSessionAsync(QuizSession session)
    {
        session.Id = _sessions.Count + 1;
        _sessions.Add(session);
        return Task.CompletedTask;
    }

    public Task<List<QuizSession>> GetSessionsAsync(int? topicId = null)
    {
        var query = topicId.HasValue
            ? _sessions.Where(s => s.TopicId == topicId.Value)
            : _sessions.AsEnumerable();
        return Task.FromResult(query.OrderByDescending(s => s.StartTime).ToList());
    }

    public Task<List<UserProgress>> GetProgressAsync() => Task.FromResult(_progress);

    public Task<UserProgress?> GetTopicProgressAsync(int topicId)
        => Task.FromResult(_progress.FirstOrDefault(p => p.TopicId == topicId));

    public Task UpdateProgressAsync(int topicId, QuizSession session)
    {
        var progress = _progress.FirstOrDefault(p => p.TopicId == topicId);
        if (progress == null)
        {
            progress = new UserProgress { Id = _progress.Count + 1, TopicId = topicId };
            _progress.Add(progress);
        }

        progress.QuestionsAnswered += session.TotalQuestions;
        progress.LastAttempted = DateTime.UtcNow;

        // Weighted average: existing mastery (70%) + new score (30%)
        progress.MasteryPercent = progress.QuestionsAnswered == session.TotalQuestions
            ? session.ScorePercent
            : progress.MasteryPercent * 0.7 + session.ScorePercent * 0.3;

        return Task.CompletedTask;
    }

    // -------------------------------------------------------------------------
    // Seed data
    // -------------------------------------------------------------------------

    private static List<Topic> SeedTopics() =>
    [
        new() { Id = 1, Name = "OOP Fundamentals", Category = CSharpCategory.OOP,
            Description = "Classes, interfaces, inheritance, polymorphism, and SOLID principles." },
        new() { Id = 2, Name = "LINQ", Category = CSharpCategory.LINQ,
            Description = "Language-Integrated Query — deferred execution, projections, and aggregations." },
        new() { Id = 3, Name = "Async / Await", Category = CSharpCategory.AsyncAwait,
            Description = "Task-based async model, ConfigureAwait, deadlocks, and cancellation." },
        new() { Id = 4, Name = "Design Patterns", Category = CSharpCategory.DesignPatterns,
            Description = "Common GoF patterns and their idiomatic use in C#." },
        new() { Id = 5, Name = "Collections", Category = CSharpCategory.Collections,
            Description = "List, Dictionary, HashSet, Queue, Stack, and IEnumerable semantics." },
        new() { Id = 6, Name = "Memory Management", Category = CSharpCategory.MemoryManagement,
            Description = "GC, IDisposable, value vs reference types, boxing, and the LOH." },
        new() { Id = 7, Name = ".NET Modern Features", Category = CSharpCategory.DotNetFeatures,
            Description = "Records, pattern matching, nullable reference types, and primary constructors." },
    ];

    private static List<Question> SeedQuestions() =>
    [
        // ── OOP (TopicId = 1) ───────────────────────────────────────────────
        new() { Id = 1, TopicId = 1, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "Which OOP principle hides the internal state of an object and only exposes a public interface?",
            Options = ["Inheritance", "Encapsulation", "Polymorphism", "Abstraction"],
            CorrectAnswer = "Encapsulation",
            Explanation = "Encapsulation bundles data (fields) with the methods that operate on them and restricts direct access using access modifiers like private/protected." },

        new() { Id = 2, TopicId = 1, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "A C# struct can inherit from another struct.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "Structs are value types and do not support inheritance. They can implement interfaces, but cannot inherit from other structs or classes." },

        new() { Id = 3, TopicId = 1, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the key difference between an abstract class and an interface in C#?",
            Options = [
                "An interface can have implementation; an abstract class cannot",
                "An abstract class can have implementation and state; an interface defines only a contract",
                "A class can implement multiple abstract classes",
                "Abstract classes cannot have constructors"
            ],
            CorrectAnswer = "An abstract class can have implementation and state; an interface defines only a contract",
            Explanation = "Abstract classes can have fields, constructors, and method implementations. Interfaces (pre-C# 8) only declare members. Since C# 8, interfaces can have default implementations, but the core distinction remains: abstract classes support state and single inheritance, interfaces support multiple implementation." },

        new() { Id = 4, TopicId = 1, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What does the 'virtual' keyword enable in C#?",
            Options = [
                "It prevents a method from being overridden",
                "It allows a method to be overridden in a derived class",
                "It makes a method static",
                "It marks a method as abstract"
            ],
            CorrectAnswer = "It allows a method to be overridden in a derived class",
            Explanation = "The 'virtual' keyword allows a base class method to be overridden by derived classes using the 'override' keyword. Without 'virtual', the method cannot be overridden (only hidden with 'new')." },

        new() { Id = 5, TopicId = 1, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "Which SOLID principle states that a class should have only one reason to change?",
            Options = ["Open/Closed Principle", "Liskov Substitution Principle", "Single Responsibility Principle", "Interface Segregation Principle"],
            CorrectAnswer = "Single Responsibility Principle",
            Explanation = "SRP states that a class should have one, and only one, reason to change — meaning it should only have one job or responsibility. This improves cohesion and makes the class easier to maintain and test." },

        new() { Id = 6, TopicId = 1, Difficulty = DifficultyLevel.Senior, Type = QuestionType.CodeSnippet,
            Text = "What will this code print?",
            CodeSnippet = "class Animal { public virtual string Sound() => \"...\"; }\nclass Dog : Animal { public override string Sound() => \"Woof\"; }\n\nAnimal a = new Dog();\nConsole.WriteLine(a.Sound());",
            Options = ["...", "Woof", "Compile error", "Runtime exception"],
            CorrectAnswer = "Woof",
            Explanation = "This is polymorphism in action. Even though 'a' is typed as Animal, it holds a Dog instance. Because Sound() is virtual and overridden, the runtime dispatches to Dog.Sound(), printing 'Woof'." },

        new() { Id = 7, TopicId = 1, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "Liskov Substitution Principle (LSP) is violated when:",
            Options = [
                "A derived class adds new public methods",
                "A derived class strengthens preconditions or weakens postconditions compared to the base class",
                "A base class has more methods than an interface it implements",
                "A class implements more than one interface"
            ],
            CorrectAnswer = "A derived class strengthens preconditions or weakens postconditions compared to the base class",
            Explanation = "LSP states that objects of a derived class must be substitutable for objects of the base class without altering program correctness. Strengthening preconditions (demanding more from callers) or weakening postconditions (guaranteeing less) breaks substitutability." },

        // ── LINQ (TopicId = 2) ───────────────────────────────────────────────
        new() { Id = 8, TopicId = 2, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is deferred execution in LINQ?",
            Options = [
                "The query runs at the point of declaration",
                "The query is only executed when its results are iterated",
                "LINQ queries never execute until explicitly called with .Run()",
                "Queries are cached and executed once"
            ],
            CorrectAnswer = "The query is only executed when its results are iterated",
            Explanation = "LINQ uses deferred execution — the query expression builds a pipeline but does not run until you enumerate it (e.g., with foreach, ToList(), First()). This allows composing queries efficiently before triggering database or in-memory evaluation." },

        new() { Id = 9, TopicId = 2, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "How many times does the database get queried?",
            CodeSnippet = "IQueryable<User> query = db.Users.Where(u => u.IsActive);\nvar count = query.Count();\nvar list  = query.ToList();",
            Options = ["0 times", "1 time", "2 times", "3 times"],
            CorrectAnswer = "2 times",
            Explanation = "Each terminal operator (Count() and ToList()) triggers a separate database round-trip. To avoid this, materialise once with ToList() and then work in memory." },

        new() { Id = 10, TopicId = 2, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the key difference between IEnumerable<T> and IQueryable<T>?",
            Options = [
                "IQueryable is faster in all scenarios",
                "IEnumerable executes queries in memory; IQueryable translates queries to the data source (e.g., SQL)",
                "IQueryable only works with collections, IEnumerable works with databases",
                "There is no functional difference"
            ],
            CorrectAnswer = "IEnumerable executes queries in memory; IQueryable translates queries to the data source (e.g., SQL)",
            Explanation = "IEnumerable<T> pulls all data into memory first then filters. IQueryable<T> builds an expression tree that the provider (e.g., EF Core) translates to SQL, so filtering happens on the database server — far more efficient for large datasets." },

        new() { Id = 11, TopicId = 2, Difficulty = DifficultyLevel.Senior, Type = QuestionType.TrueFalse,
            Text = "GroupBy() in LINQ returns an IEnumerable<IGrouping<TKey, TElement>>.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "GroupBy produces a sequence of IGrouping<TKey, TElement> objects. Each IGrouping has a Key property and is itself an IEnumerable<TElement> containing the elements that share that key." },

        // ── Async/Await (TopicId = 3) ────────────────────────────────────────
        new() { Id = 12, TopicId = 3, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the purpose of ConfigureAwait(false)?",
            Options = [
                "It disables the async state machine",
                "It tells the awaiter not to capture and resume on the original synchronisation context",
                "It makes the task run synchronously",
                "It cancels the task if it takes too long"
            ],
            CorrectAnswer = "It tells the awaiter not to capture and resume on the original synchronisation context",
            Explanation = "ConfigureAwait(false) prevents the continuation from marshalling back to the original context (e.g., the UI thread). Library code should use it to avoid deadlocks in synchronisation-context-heavy environments and to improve throughput." },

        new() { Id = 13, TopicId = 3, Difficulty = DifficultyLevel.Mid, Type = QuestionType.TrueFalse,
            Text = "Using 'async void' is recommended for event handlers but should be avoided elsewhere.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "async void cannot be awaited, so exceptions thrown inside it are unobserved and crash the process. It is acceptable only for event handlers (which require a void signature). All other async methods should return Task or Task<T>." },

        new() { Id = 14, TopicId = 3, Difficulty = DifficultyLevel.Senior, Type = QuestionType.CodeSnippet,
            Text = "What issue does this code have?",
            CodeSnippet = "// WPF / WinForms with a SynchronisationContext\npublic string GetData()\n{\n    return FetchAsync().Result; // blocks calling thread\n}\n\nprivate async Task<string> FetchAsync()\n{\n    await Task.Delay(1000);\n    return \"done\";\n}",
            Options = ["None, this is fine", "Deadlock — the UI thread blocks waiting for a Task that needs the UI thread to continue", "NullReferenceException", "The Task.Delay will be skipped"],
            CorrectAnswer = "Deadlock — the UI thread blocks waiting for a Task that needs the UI thread to continue",
            Explanation = ".Result blocks the calling (UI) thread. The continuation after Task.Delay tries to resume on that same UI thread — which is blocked — causing a deadlock. Fix: make the caller async and use await, or call ConfigureAwait(false) inside FetchAsync." },

        new() { Id = 15, TopicId = 3, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "When should you prefer ValueTask<T> over Task<T>?",
            Options = [
                "Always — ValueTask is strictly better",
                "When the method frequently completes synchronously, avoiding heap allocation for the common fast path",
                "When you need to await the result multiple times",
                "Only when using .NET Framework"
            ],
            CorrectAnswer = "When the method frequently completes synchronously, avoiding heap allocation for the common fast path",
            Explanation = "ValueTask<T> is a struct that avoids the heap allocation of Task<T> when the operation completes synchronously (e.g., cache hit). It has constraints: it cannot be awaited more than once and carries additional complexity. Use it only when profiling shows Task allocation is a bottleneck." },

        // ── Design Patterns (TopicId = 4) ────────────────────────────────────
        new() { Id = 16, TopicId = 4, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "Which pattern ensures a class has only one instance and provides a global access point?",
            Options = ["Factory", "Singleton", "Prototype", "Builder"],
            CorrectAnswer = "Singleton",
            Explanation = "The Singleton pattern restricts instantiation to one object. In C#, the most common thread-safe approach uses a static readonly field or Lazy<T>. Note: singletons make testing harder — prefer dependency injection with a singleton lifetime instead." },

        new() { Id = 17, TopicId = 4, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What does the Decorator pattern do?",
            Options = [
                "Creates objects without specifying the exact class",
                "Attaches additional responsibilities to an object dynamically without modifying its class",
                "Provides a simplified interface to a complex subsystem",
                "Converts the interface of a class into another interface clients expect"
            ],
            CorrectAnswer = "Attaches additional responsibilities to an object dynamically without modifying its class",
            Explanation = "The Decorator wraps an existing object and adds behaviour. It implements the same interface as the wrapped object, so callers are unaware of the decoration. Example: Stream decorators in .NET (GZipStream, CryptoStream wrapping FileStream)." },

        new() { Id = 18, TopicId = 4, Difficulty = DifficultyLevel.Lead, Type = QuestionType.CodeSnippet,
            Text = "Which pattern is being used here?",
            CodeSnippet = "public interface ILogger { void Log(string msg); }\n\npublic class FileLogger : ILogger\n{\n    public void Log(string msg) => File.AppendAllText(\"log.txt\", msg);\n}\n\npublic class TimestampLogger : ILogger\n{\n    private readonly ILogger _inner;\n    public TimestampLogger(ILogger inner) => _inner = inner;\n    public void Log(string msg) => _inner.Log($\"[{DateTime.Now}] {msg}\");\n}",
            Options = ["Singleton", "Proxy", "Decorator", "Adapter"],
            CorrectAnswer = "Decorator",
            Explanation = "TimestampLogger wraps another ILogger and adds behaviour (prepending a timestamp) before delegating to the inner logger. This is the Decorator pattern — it enhances an object's behaviour without changing its interface." },

        // ── Collections (TopicId = 5) ─────────────────────────────────────────
        new() { Id = 19, TopicId = 5, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What is the time complexity of looking up a value by key in a Dictionary<TKey, TValue>?",
            Options = ["O(n)", "O(log n)", "O(1) amortised", "O(n log n)"],
            CorrectAnswer = "O(1) amortised",
            Explanation = "Dictionary<TKey, TValue> uses a hash table internally. Key lookups compute the hash and go directly to the bucket, giving O(1) amortised performance. Worst case is O(n) if all keys hash to the same bucket, but this is rare with a good hash function." },

        new() { Id = 20, TopicId = 5, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "A List<T> in C# is backed by a dynamically resizing array.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "List<T> uses an internal array that doubles in size when capacity is exceeded. This gives O(1) amortised Add, O(1) indexed access, but O(n) insertion or removal at arbitrary positions." },

        new() { Id = 21, TopicId = 5, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "Which collection should you choose when you need to ensure uniqueness of elements and fast membership testing?",
            Options = ["List<T>", "Queue<T>", "HashSet<T>", "LinkedList<T>"],
            CorrectAnswer = "HashSet<T>",
            Explanation = "HashSet<T> stores unique elements using a hash table and provides O(1) amortised Contains, Add, and Remove. It is ideal for set operations (union, intersection, difference) and deduplication." },

        // ── Memory Management (TopicId = 6) ─────────────────────────────────
        new() { Id = 22, TopicId = 6, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the purpose of implementing IDisposable?",
            Options = [
                "To make an object eligible for garbage collection",
                "To release unmanaged resources (file handles, network connections, etc.) deterministically",
                "To override the finaliser",
                "To prevent an object from being garbage collected"
            ],
            CorrectAnswer = "To release unmanaged resources (file handles, network connections, etc.) deterministically",
            Explanation = "The GC only manages managed memory. IDisposable.Dispose() provides a way to release unmanaged resources immediately when the object is no longer needed. Use the 'using' statement or 'using' declaration to guarantee Dispose is called even if an exception occurs." },

        new() { Id = 23, TopicId = 6, Difficulty = DifficultyLevel.Senior, Type = QuestionType.CodeSnippet,
            Text = "What is the problem with this code?",
            CodeSnippet = "public class DataProcessor\n{\n    private SqlConnection _conn = new SqlConnection(connStr);\n\n    public void Process()\n    {\n        _conn.Open();\n        // ... process data ...\n        _conn.Close();\n    }\n}",
            Options = [
                "SqlConnection cannot be a field",
                "The connection is never disposed, leaking unmanaged resources if an exception is thrown",
                "Close() and Dispose() are the same",
                "No problem — Close() releases all resources"
            ],
            CorrectAnswer = "The connection is never disposed, leaking unmanaged resources if an exception is thrown",
            Explanation = "If an exception occurs between Open() and Close(), the connection is never closed or disposed, leaking the underlying socket/handle. The correct pattern is to create the connection in a 'using' block inside the method, not as a long-lived field." },

        new() { Id = 24, TopicId = 6, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is boxing in C#?",
            Options = [
                "Wrapping a reference type in a value type",
                "Converting a value type to an object reference by allocating it on the heap",
                "Casting between numeric types",
                "Sealing a class to prevent inheritance"
            ],
            CorrectAnswer = "Converting a value type to an object reference by allocating it on the heap",
            Explanation = "Boxing copies a value type (e.g., int, struct) from the stack into a new heap-allocated object. It enables value types to be treated as objects but has a cost: heap allocation, GC pressure, and the copy overhead. Avoid boxing in hot paths." },

        // ── .NET Modern Features (TopicId = 7) ──────────────────────────────
        new() { Id = 25, TopicId = 7, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is a C# record type?",
            Options = [
                "A class that cannot be instantiated",
                "A reference type with value-based equality, immutability by default, and a synthesised ToString()",
                "A struct with automatic property generation",
                "A type alias for Dictionary<string, object>"
            ],
            CorrectAnswer = "A reference type with value-based equality, immutability by default, and a synthesised ToString()",
            Explanation = "Records (introduced in C# 9) are primarily intended for immutable data models. The compiler synthesises Equals, GetHashCode, and ToString based on the declared properties. 'with' expressions allow non-destructive mutation. Record structs were added in C# 10." },

        new() { Id = 26, TopicId = 7, Difficulty = DifficultyLevel.Mid, Type = QuestionType.TrueFalse,
            Text = "Nullable reference types (enabled via <Nullable>enable</Nullable>) generate runtime exceptions for null dereferences.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "Nullable reference types are a compile-time static analysis feature only — they produce warnings, not runtime exceptions. They help catch potential NullReferenceExceptions at build time without changing runtime behaviour." },

        new() { Id = 27, TopicId = 7, Difficulty = DifficultyLevel.Senior, Type = QuestionType.CodeSnippet,
            Text = "Which pattern matching feature is used here?",
            CodeSnippet = "object shape = new Circle(radius: 5);\n\nstring desc = shape switch\n{\n    Circle c when c.Radius > 10 => \"Large circle\",\n    Circle c                    => \"Small circle\",\n    Rectangle r                 => $\"{r.Width}x{r.Height} rect\",\n    _                           => \"Unknown\"\n};",
            Options = ["Type pattern only", "Switch expression with type patterns and when guards", "Tuple pattern", "Property pattern"],
            CorrectAnswer = "Switch expression with type patterns and when guards",
            Explanation = "This uses a switch expression (C# 8+) with type patterns (Circle c, Rectangle r) and a when guard clause (when c.Radius > 10). The discard pattern (_) acts as the default. Switch expressions are expressions that return a value, unlike switch statements." },

        // ── OOP additional (TopicId = 1) ─────────────────────────────────────
        new() { Id = 28, TopicId = 1, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What does the 'sealed' keyword do when applied to a class?",
            Options = ["Makes all members private", "Prevents the class from being inherited", "Prevents instantiation", "Makes all members static"],
            CorrectAnswer = "Prevents the class from being inherited",
            Explanation = "'sealed' on a class means no other class can inherit from it. String is a well-known example of a sealed class. Use sealed when you want to prevent inheritance for security or performance reasons." },

        new() { Id = 29, TopicId = 1, Difficulty = DifficultyLevel.Mid, Type = QuestionType.TrueFalse,
            Text = "In C#, a class can implement multiple interfaces but inherit from only one base class.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "C# supports single class inheritance but multiple interface implementation. This is by design — multiple inheritance of classes causes the diamond problem. Interfaces provide a clean way to compose behaviours without that ambiguity." },

        new() { Id = 30, TopicId = 1, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What does the Dependency Inversion Principle (DIP) state?",
            Options = [
                "High-level modules should depend on low-level modules",
                "High-level modules should not depend on low-level modules; both should depend on abstractions",
                "Every class should depend on a single concrete implementation",
                "Modules should be isolated and never depend on each other"
            ],
            CorrectAnswer = "High-level modules should not depend on low-level modules; both should depend on abstractions",
            Explanation = "DIP states that high-level policy code should not import low-level detail code. Both should depend on abstractions (interfaces/abstract classes). This is the foundation of dependency injection — you inject an IRepository, not a SqlRepository." },

        // ── LINQ additional (TopicId = 2) ────────────────────────────────────
        new() { Id = 31, TopicId = 2, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What does the LINQ Select() operator do?",
            Options = ["Filters elements based on a condition", "Projects each element into a new form", "Sorts elements ascending", "Groups elements by a key"],
            CorrectAnswer = "Projects each element into a new form",
            Explanation = "Select() is a projection operator — it transforms each element in a sequence using a selector function, similar to 'map' in functional programming. Example: users.Select(u => u.Name) returns an IEnumerable<string> of names." },

        new() { Id = 32, TopicId = 2, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between First() and FirstOrDefault() in LINQ?",
            Options = [
                "First() is faster",
                "First() throws InvalidOperationException if no element is found; FirstOrDefault() returns the default value",
                "FirstOrDefault() only works with nullable types",
                "There is no difference"
            ],
            CorrectAnswer = "First() throws InvalidOperationException if no element is found; FirstOrDefault() returns the default value",
            Explanation = "First() throws if the sequence is empty or no element matches the predicate. FirstOrDefault() returns default(T) instead — null for reference types, 0 for int, etc. Use FirstOrDefault() when an empty result is a valid scenario." },

        new() { Id = 33, TopicId = 2, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What does SelectMany() do differently from Select()?",
            Options = [
                "SelectMany is used for grouping",
                "SelectMany flattens nested sequences into a single sequence",
                "SelectMany filters while projecting",
                "SelectMany projects to a single scalar value"
            ],
            CorrectAnswer = "SelectMany flattens nested sequences into a single sequence",
            Explanation = "Select() returns IEnumerable<IEnumerable<T>> when projecting to a collection. SelectMany() flattens this into a single IEnumerable<T>. Example: orders.SelectMany(o => o.Items) yields all items across all orders in one flat sequence." },

        new() { Id = 34, TopicId = 2, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is an Expression Tree in LINQ and why does IQueryable use it?",
            Options = [
                "A binary tree sorting data by expression value",
                "A data structure representing code as data, allowing LINQ providers to translate queries to SQL",
                "A compiled lambda cache",
                "A tree of LINQ operators for caching query results"
            ],
            CorrectAnswer = "A data structure representing code as data, allowing LINQ providers to translate queries to SQL",
            Explanation = "Expression<Func<T, bool>> captures the structure of a lambda as an in-memory tree rather than compiling it to IL. IQueryable providers (like EF Core) walk this tree and translate it to SQL. This is why LINQ-to-SQL can push Where() predicates to the database — the expression is inspectable at runtime." },

        // ── Async additional (TopicId = 3) ───────────────────────────────────
        new() { Id = 35, TopicId = 3, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What does the 'await' keyword do in C#?",
            Options = [
                "It blocks the thread until the task completes",
                "It asynchronously waits for a Task to complete, freeing the thread in the meantime",
                "It creates a new Task on a background thread",
                "It cancels a running Task"
            ],
            CorrectAnswer = "It asynchronously waits for a Task to complete, freeing the thread in the meantime",
            Explanation = "'await' suspends the current async method until the awaited Task completes, but it does NOT block the thread. The thread is released back to the thread pool (or UI loop) and the method resumes when the task finishes, typically on the original synchronisation context." },

        new() { Id = 36, TopicId = 3, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is CancellationToken used for?",
            Options = [
                "To catch async exceptions",
                "To signal that an operation should be cancelled cooperatively",
                "To limit the number of concurrent tasks",
                "To delay task execution"
            ],
            CorrectAnswer = "To signal that an operation should be cancelled cooperatively",
            Explanation = "CancellationToken provides a cooperative cancellation mechanism. A CancellationTokenSource creates the token; calling .Cancel() signals cancellation. The running operation checks the token (token.ThrowIfCancellationRequested()) and stops gracefully. Pass tokens to all async APIs for responsive cancellation." },

        new() { Id = 37, TopicId = 3, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between Task.WhenAll and Task.WhenAny?",
            Options = [
                "WhenAll runs tasks sequentially; WhenAny runs them in parallel",
                "WhenAll completes when all tasks finish; WhenAny completes when the first task finishes",
                "WhenAny cancels remaining tasks automatically",
                "There is no functional difference"
            ],
            CorrectAnswer = "WhenAll completes when all tasks finish; WhenAny completes when the first task finishes",
            Explanation = "Task.WhenAll(tasks) returns a Task that completes when every task in the collection has completed (or any throws). Task.WhenAny(tasks) returns a Task<Task> that completes as soon as the first task finishes. WhenAny is useful for timeouts: Task.WhenAny(actualTask, Task.Delay(timeout))." },

        new() { Id = 38, TopicId = 3, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is SemaphoreSlim used for in async code?",
            Options = [
                "To cancel tasks cooperatively",
                "To limit the number of tasks that can access a resource concurrently",
                "To synchronise the UI thread with background tasks",
                "To replace locks in all async scenarios"
            ],
            CorrectAnswer = "To limit the number of tasks that can access a resource concurrently",
            Explanation = "SemaphoreSlim(n) allows at most n concurrent entrants. It has an async-friendly WaitAsync() method that doesn't block a thread. Use it to cap concurrency — e.g., SemaphoreSlim(4) limits parallel HTTP requests to 4, preventing server overload or rate-limit violations." },

        // ── Design Patterns additional (TopicId = 4) ─────────────────────────
        new() { Id = 39, TopicId = 4, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What does the Factory Method pattern do?",
            Options = [
                "Restricts a class to one instance",
                "Defines an interface for creating an object, letting subclasses decide which class to instantiate",
                "Provides a simplified interface to a complex subsystem",
                "Converts one interface into another"
            ],
            CorrectAnswer = "Defines an interface for creating an object, letting subclasses decide which class to instantiate",
            Explanation = "Factory Method defines a creator method in a base class that subclasses override to instantiate specific types. Callers use the factory method rather than 'new', decoupling them from concrete implementations. ILoggerFactory in .NET is a real-world example." },

        new() { Id = 40, TopicId = 4, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What problem does the Observer pattern solve?",
            Options = [
                "Creating families of related objects",
                "Notifying multiple objects when the state of another object changes",
                "Restricting access to a resource",
                "Converting between incompatible interfaces"
            ],
            CorrectAnswer = "Notifying multiple objects when the state of another object changes",
            Explanation = "The Observer pattern defines a one-to-many dependency: when a subject changes state, all registered observers are notified automatically. C# events and INotifyPropertyChanged (used in MVVM) are built-in implementations of the Observer pattern." },

        new() { Id = 41, TopicId = 4, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the Strategy pattern?",
            Options = [
                "A pattern that caches strategy objects for reuse",
                "Defines a family of algorithms, encapsulates each, and makes them interchangeable at runtime",
                "A pattern for building complex objects step by step",
                "A pattern that routes requests to handler chains"
            ],
            CorrectAnswer = "Defines a family of algorithms, encapsulates each, and makes them interchangeable at runtime",
            Explanation = "Strategy lets you swap algorithms (sorting, payment processing, compression) without changing the client code. The client holds a reference to a strategy interface and delegates the algorithm to whichever implementation is injected. This follows the Open/Closed Principle." },

        // ── Collections additional (TopicId = 5) ─────────────────────────────
        new() { Id = 42, TopicId = 5, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the main difference between Stack<T> and Queue<T>?",
            Options = [
                "Stack is thread-safe; Queue is not",
                "Stack is LIFO (last in, first out); Queue is FIFO (first in, first out)",
                "Queue has O(1) access by index; Stack does not",
                "Stack can only hold value types"
            ],
            CorrectAnswer = "Stack is LIFO (last in, first out); Queue is FIFO (first in, first out)",
            Explanation = "Stack<T> uses Push/Pop — the last item added is the first removed (LIFO). Queue<T> uses Enqueue/Dequeue — the first item added is the first removed (FIFO). Use Stack for undo/redo or call-stack emulation; use Queue for work queues and breadth-first traversal." },

        new() { Id = 43, TopicId = 5, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "When should you use ConcurrentDictionary<TKey, TValue> instead of Dictionary<TKey, TValue>?",
            Options = [
                "When you need keys to be sorted",
                "When multiple threads read and write the dictionary concurrently",
                "When the dictionary needs to persist to disk",
                "When the number of entries exceeds 10,000"
            ],
            CorrectAnswer = "When multiple threads read and write the dictionary concurrently",
            Explanation = "Dictionary<TKey, TValue> is not thread-safe — concurrent reads are fine, but concurrent writes (or mixed read/write) require external locking. ConcurrentDictionary uses fine-grained locking internally and provides atomic operations like AddOrUpdate and GetOrAdd, avoiding manual lock management." },

        new() { Id = 44, TopicId = 5, Difficulty = DifficultyLevel.Senior, Type = QuestionType.TrueFalse,
            Text = "SortedDictionary<TKey, TValue> maintains keys in sorted order and provides O(log n) lookup.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "SortedDictionary uses a red-black tree internally, so keys are always in sorted order and lookup/insert/delete are O(log n). In contrast, Dictionary uses a hash table with O(1) average lookup but no ordering guarantee." },

        // ── Memory additional (TopicId = 6) ──────────────────────────────────
        new() { Id = 45, TopicId = 6, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "Value types (like int and struct) are always allocated on the stack in C#.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "Value types live on the stack when declared as local variables, but they live on the heap when they are fields of a class, captured in a closure, or boxed. The runtime decides allocation — you cannot guarantee stack allocation from C# code." },

        new() { Id = 46, TopicId = 6, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is a finalizer and when should you implement one?",
            Options = [
                "A method called after construction to validate state",
                "A destructor (~ClassName) called by the GC before reclaiming memory, used to release unmanaged resources when Dispose was not called",
                "A method that seals a class from further modification",
                "An event raised when an object goes out of scope"
            ],
            CorrectAnswer = "A destructor (~ClassName) called by the GC before reclaiming memory, used to release unmanaged resources when Dispose was not called",
            Explanation = "Finalizers (~ClassName) are called by the GC as a safety net when Dispose() was not called. They delay GC collection by one cycle and add overhead, so implement them only when directly holding unmanaged handles. The recommended pattern: implement IDisposable, call GC.SuppressFinalize(this) in Dispose(), and implement the finalizer as a fallback." },

        new() { Id = 47, TopicId = 6, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the Large Object Heap (LOH) and why does it matter for performance?",
            Options = [
                "A separate heap for objects used by the OS kernel",
                "A heap for objects ≥ 85,000 bytes that is collected less frequently and never compacted by default",
                "A heap that stores only string objects",
                "A reserved memory area that bypasses garbage collection"
            ],
            CorrectAnswer = "A heap for objects ≥ 85,000 bytes that is collected less frequently and never compacted by default",
            Explanation = "The LOH holds large allocations (≥ 85KB, 85,000 bytes for most types). Unlike the small-object heap, the LOH is not compacted during GC by default, leading to fragmentation over time. LOH collections are expensive (Gen 2). Mitigation: use ArrayPool<T>, Memory<T>/Span<T>, or set GCSettings.LargeObjectHeapCompactionMode." },

        // ── .NET Features additional (TopicId = 7) ───────────────────────────
        new() { Id = 48, TopicId = 7, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "Global using directives (global using) apply to all files in the project without needing to repeat the using statement.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "Introduced in C# 10, 'global using SomeNamespace;' in any file makes that namespace available throughout the project. .NET 6+ projects often put these in a GlobalUsings.cs file. The SDK automatically adds common ones (System, System.Collections.Generic, etc.) via implicit global usings." },

        new() { Id = 49, TopicId = 7, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What are init-only setters (the 'init' accessor) in C# 9?",
            Options = [
                "Properties that can only be set inside the constructor",
                "Properties that can be set during object initialisation (construction time) but become read-only afterwards",
                "Properties that are initialised lazily on first access",
                "Properties that can only be set from within the same assembly"
            ],
            CorrectAnswer = "Properties that can be set during object initialisation (construction time) but become read-only afterwards",
            Explanation = "'init' is like 'set' but only callable during object initialisation — in constructors and object initialisers. After the object is fully constructed, the property is effectively read-only. This allows immutable-like types that still support object initialiser syntax: new Person { Name = \"Alice\" }." },

        new() { Id = 50, TopicId = 7, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What does the 'required' modifier on a property enforce in C# 11?",
            Options = [
                "The property must be set in the class constructor only",
                "Callers must provide a value for the property in object initialisers or constructors",
                "The property cannot be null at runtime",
                "The property is validated against a regex pattern"
            ],
            CorrectAnswer = "Callers must provide a value for the property in object initialisers or constructors",
            Explanation = "'required' is a compile-time enforcement that callers must initialise the property. Combined with init, it replaces constructor overloads for mandatory-field data classes: 'public required string Name { get; init; }'. Forgetting to set a required member is a compile error, not a runtime surprise." },

        new() { Id = 51, TopicId = 7, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the primary benefit of primary constructors in C# 12 for classes?",
            Options = [
                "They allow multiple constructors to be declared on a single line",
                "They make constructor parameters available throughout the class body without manually assigning to fields",
                "They prevent derived classes from adding additional constructors",
                "They automatically generate IDisposable for the class"
            ],
            CorrectAnswer = "They make constructor parameters available throughout the class body without manually assigning to fields",
            Explanation = "Primary constructors (C# 12 for classes/structs, C# 9 for records) declare parameters in the class header. These parameters are captured and available in all instance methods and property initialisers, eliminating the boilerplate of manually assigning constructor parameters to private fields." },
    ];
}
