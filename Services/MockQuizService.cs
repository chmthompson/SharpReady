using SharpReady.Models;
using SharpReady.Models.Enums;

namespace SharpReady.Services;

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

    internal static List<Topic> SeedTopics() =>
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

    internal static List<Question> SeedQuestions() =>
    [
        // ── OOP (TopicId = 1) ───────────────────────────────────────────────
        new() { Id = 1, TopicId = 1, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "Which OOP principle hides the internal state of an object and only exposes a public interface?",
            Options = ["Inheritance", "Encapsulation", "Polymorphism", "Abstraction"],
            CorrectAnswer = "Encapsulation",
            Explanation = "Encapsulation bundles data (fields) with the methods that operate on them and restricts direct access using access modifiers like private/protected.",
            ExampleCode = "public class BankAccount\n{\n    private decimal _balance; // hidden state\n\n    public void Deposit(decimal amount)\n    {\n        if (amount > 0) _balance += amount;\n    }\n\n    public decimal Balance => _balance; // read-only access\n}" },

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
            Explanation = "Abstract classes can have fields, constructors, and method implementations. Interfaces (pre-C# 8) only declare members. Since C# 8, interfaces can have default implementations, but the core distinction remains: abstract classes support state and single inheritance, interfaces support multiple implementation.",
            ExampleCode = "// Abstract class — can have state and implementation\nabstract class Shape\n{\n    protected string Color = \"red\"; // state allowed\n    public abstract double Area();\n    public void Describe() => Console.WriteLine($\"A {Color} shape\");\n}\n\n// Interface — contract only (pre-C# 8)\ninterface IDrawable { void Draw(); }\n\n// A class can implement multiple interfaces but only one abstract class\nclass Circle : Shape, IDrawable\n{\n    double _r;\n    public Circle(double r) => _r = r;\n    public override double Area() => Math.PI * _r * _r;\n    public void Draw() => Console.WriteLine(\"Drawing circle\");\n}" },

        new() { Id = 4, TopicId = 1, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What does the 'virtual' keyword enable in C#?",
            Options = [
                "It prevents a method from being overridden",
                "It allows a method to be overridden in a derived class",
                "It makes a method static",
                "It marks a method as abstract"
            ],
            CorrectAnswer = "It allows a method to be overridden in a derived class",
            Explanation = "The 'virtual' keyword allows a base class method to be overridden by derived classes using the 'override' keyword. Without 'virtual', the method cannot be overridden (only hidden with 'new').",
            ExampleCode = "class Base\n{\n    public virtual void Greet() => Console.WriteLine(\"Base\");\n    public void Fixed() => Console.WriteLine(\"Cannot override\");\n}\n\nclass Child : Base\n{\n    public override void Greet() => Console.WriteLine(\"Child\");\n    // public override void Fixed() { } // compile error!\n    public new void Fixed() => Console.WriteLine(\"Hides, not overrides\");\n}\n\nBase b = new Child();\nb.Greet(); // Child  (virtual dispatch)\nb.Fixed(); // Cannot override  (no virtual dispatch)" },

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
            Explanation = "LSP states that objects of a derived class must be substitutable for objects of the base class without altering program correctness. Strengthening preconditions (demanding more from callers) or weakening postconditions (guaranteeing less) breaks substitutability.",
            ExampleCode = "// Classic LSP violation: Square extends Rectangle\nclass Rectangle\n{\n    public virtual int Width { get; set; }\n    public virtual int Height { get; set; }\n    public int Area() => Width * Height;\n}\n\nclass Square : Rectangle\n{\n    // Forces Width == Height — breaks callers that set them independently\n    public override int Width { set { base.Width = value; base.Height = value; } }\n    public override int Height { set { base.Width = value; base.Height = value; } }\n}\n\n// Caller expecting Rectangle behaviour:\nRectangle r = new Square();\nr.Width = 4;\nr.Height = 5;\nConsole.WriteLine(r.Area()); // 25, not 20 — LSP violated!" },

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
            Explanation = "IEnumerable<T> pulls all data into memory first then filters. IQueryable<T> builds an expression tree that the provider (e.g., EF Core) translates to SQL, so filtering happens on the database server — far more efficient for large datasets.",
            ExampleCode = "// IEnumerable — filters in memory (loads ALL rows first)\nIEnumerable<User> users = db.Users.AsEnumerable();\nvar admins = users.Where(u => u.IsAdmin); // C# filtering\n\n// IQueryable — translates to SQL WHERE clause\nIQueryable<User> query = db.Users;\nvar admins2 = query.Where(u => u.IsAdmin); // SQL: WHERE IsAdmin = 1\n// Only matching rows are fetched from the DB" },

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
            Explanation = "ConfigureAwait(false) prevents the continuation from marshalling back to the original context (e.g., the UI thread). Library code should use it to avoid deadlocks in synchronisation-context-heavy environments and to improve throughput.",
            ExampleCode = "// Library code — use ConfigureAwait(false)\npublic async Task<string> FetchDataAsync()\n{\n    var response = await httpClient\n        .GetAsync(url)\n        .ConfigureAwait(false); // don't need UI thread\n\n    return await response.Content\n        .ReadAsStringAsync()\n        .ConfigureAwait(false);\n}\n\n// UI/App code — omit ConfigureAwait(false)\n// so continuation runs back on UI thread\nprivate async void Button_Click(object s, EventArgs e)\n{\n    var data = await FetchDataAsync();\n    label.Text = data; // safe — back on UI thread\n}" },

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

        // ── OOP Fundamentals additional (TopicId = 1) IDs 52–91 ─────────────
        new() { Id = 52, TopicId = 1, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "Which access modifier makes a member visible only within the same class?",
            Options = ["internal", "protected", "private", "public"],
            CorrectAnswer = "private",
            Explanation = "private restricts access to the declaring class only. protected allows derived classes, internal allows the same assembly, and public allows any code to access the member." },

        new() { Id = 53, TopicId = 1, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "A static method can access instance fields of its class directly.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "Static methods belong to the type, not any instance. They cannot access instance fields or call instance methods without a reference to an object. They can only access other static members directly." },

        new() { Id = 54, TopicId = 1, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What is a constructor in C#?",
            Options = [
                "A method that destroys an object when it goes out of scope",
                "A special method called automatically when an object is instantiated",
                "A method that copies one object to another",
                "A static method that returns a new instance"
            ],
            CorrectAnswer = "A special method called automatically when an object is instantiated",
            Explanation = "A constructor has the same name as the class and no return type. It is called automatically by the 'new' keyword to initialise the object's state. If no constructor is defined, the compiler generates a default parameterless one." },

        new() { Id = 55, TopicId = 1, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "In C#, you can overload a method by changing only its return type.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "Method overloading requires a difference in parameter count, parameter types, or parameter order. Return type alone is not sufficient to distinguish overloads — the compiler would be unable to determine which overload to call." },

        new() { Id = 56, TopicId = 1, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What keyword is used in a derived class to call the base class constructor?",
            Options = ["this", "base", "super", "parent"],
            CorrectAnswer = "base",
            Explanation = "The 'base' keyword refers to the base class. 'base(args)' in a constructor initialiser invokes the base constructor. 'base.Method()' calls a base class method. C# does not have a 'super' keyword unlike Java/Kotlin." },

        new() { Id = 57, TopicId = 1, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "What does this code print?",
            CodeSnippet = "class Base\n{\n    public void Show() => Console.WriteLine(\"Base\");\n}\nclass Derived : Base\n{\n    public new void Show() => Console.WriteLine(\"Derived\");\n}\n\nBase obj = new Derived();\nobj.Show();",
            Options = ["Derived", "Base", "Compile error", "Runtime exception"],
            CorrectAnswer = "Base",
            Explanation = "The 'new' keyword hides the base method rather than overriding it. Method hiding is resolved at compile time based on the declared type of the variable ('Base'), not the runtime type. Since Show() is not virtual, there is no polymorphic dispatch — 'Base' is printed." },

        new() { Id = 58, TopicId = 1, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the purpose of the 'protected internal' access modifier?",
            Options = [
                "Accessible only within the same class",
                "Accessible within the same assembly OR from derived classes in any assembly",
                "Accessible only from derived classes in the same assembly",
                "Accessible from any assembly without restriction"
            ],
            CorrectAnswer = "Accessible within the same assembly OR from derived classes in any assembly",
            Explanation = "'protected internal' is the union of protected and internal. A member is accessible from any code in the same assembly, and also from derived classes in other assemblies. 'private protected' (C# 7.2) is the intersection — accessible only from derived classes in the same assembly." },

        new() { Id = 59, TopicId = 1, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is an object initializer in C#?",
            Options = [
                "A constructor that takes no parameters",
                "Syntax to set public properties or fields after construction without a dedicated constructor overload",
                "A pattern for lazy initialization of objects",
                "A method that resets an object to its default state"
            ],
            CorrectAnswer = "Syntax to set public properties or fields after construction without a dedicated constructor overload",
            Explanation = "Object initializers (introduced in C# 3) let you set properties inline: new Person { Name = \"Alice\", Age = 30 }. The compiler calls the constructor first, then assigns each named property. They reduce the need for many constructor overloads.",
            ExampleCode = "var p = new Person { Name = \"Alice\", Age = 30 };\n// Equivalent to:\nvar p2 = new Person();\np2.Name = \"Alice\";\np2.Age = 30;" },

        new() { Id = 60, TopicId = 1, Difficulty = DifficultyLevel.Mid, Type = QuestionType.TrueFalse,
            Text = "A partial class in C# allows a class definition to be split across multiple files.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "The 'partial' keyword allows a class, struct, or interface to be split across multiple source files. The compiler merges them into one type. This is useful for separating generated code (e.g., designer files, source generators) from hand-written code." },

        new() { Id = 61, TopicId = 1, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is an extension method?",
            Options = [
                "A method added to a class by inheriting from it",
                "A static method defined in a static class that can be called as if it were an instance method on a type",
                "A method that extends the base class behavior using 'override'",
                "A method that is automatically called when an object is extended with new properties"
            ],
            CorrectAnswer = "A static method defined in a static class that can be called as if it were an instance method on a type",
            Explanation = "Extension methods (C# 3) are static methods with 'this T param' as the first parameter. They appear as instance methods on the type in IntelliSense. LINQ operators (Select, Where, etc.) are all extension methods on IEnumerable<T>.",
            ExampleCode = "public static class StringExtensions\n{\n    public static bool IsNullOrEmpty(this string s)\n        => string.IsNullOrEmpty(s);\n}\n\n// Usage:\nstring name = \"\";\nbool empty = name.IsNullOrEmpty(); // called as instance method" },

        new() { Id = 62, TopicId = 1, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "What is the output?",
            CodeSnippet = "class Counter\n{\n    public static int Count = 0;\n    public Counter() { Count++; }\n}\n\nvar a = new Counter();\nvar b = new Counter();\nvar c = new Counter();\nConsole.WriteLine(Counter.Count);",
            Options = ["0", "1", "3", "Compile error"],
            CorrectAnswer = "3",
            Explanation = "Count is a static field shared across all instances. Each constructor call increments it. After creating three Counter objects, Count equals 3. Static fields persist for the lifetime of the AppDomain." },

        new() { Id = 63, TopicId = 1, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is covariance in the context of generic interfaces in C#?",
            Options = [
                "The ability to use a more derived type where a base type is expected for generic type parameters",
                "The ability to use a less derived type where a derived type is expected",
                "Converting between numeric types automatically",
                "Implicit conversion from interface to class"
            ],
            CorrectAnswer = "The ability to use a more derived type where a base type is expected for generic type parameters",
            Explanation = "Covariance (out keyword) allows IEnumerable<Dog> to be used where IEnumerable<Animal> is expected, since Dog is a subtype of Animal. Contravariance (in keyword) is the reverse — Action<Animal> can be used where Action<Dog> is expected. These are only valid for interfaces and delegates.",
            ExampleCode = "// Covariant: IEnumerable<out T>\nIEnumerable<Dog> dogs = new List<Dog>();\nIEnumerable<Animal> animals = dogs; // valid — covariance\n\n// Contravariant: Action<in T>\nAction<Animal> feedAnimal = a => Console.WriteLine(\"Fed\");\nAction<Dog> feedDog = feedAnimal; // valid — contravariance" },

        new() { Id = 64, TopicId = 1, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is explicit interface implementation used for?",
            Options = [
                "To mark a method as requiring explicit 'override' in derived classes",
                "To implement an interface member so it is only accessible through the interface type, resolving name conflicts",
                "To prevent a class from being used as an interface",
                "To force callers to cast before calling any method"
            ],
            CorrectAnswer = "To implement an interface member so it is only accessible through the interface type, resolving name conflicts",
            Explanation = "Explicit implementation (void IFoo.Method()) hides the member from the class's public surface. It is useful when two interfaces have identically named members with different semantics, or to keep the class API clean while still fulfilling the contract.",
            ExampleCode = "interface IShape { double Area(); }\ninterface IPrintable { void Area(); } // name conflict!\n\nclass Circle : IShape, IPrintable\n{\n    public double Area() => Math.PI * r * r; // IShape.Area\n    void IPrintable.Area() => Console.WriteLine(Area()); // explicit\n}\n\n// Only accessible via cast:\n((IPrintable)new Circle(5)).Area();" },

        new() { Id = 65, TopicId = 1, Difficulty = DifficultyLevel.Senior, Type = QuestionType.TrueFalse,
            Text = "In C#, delegates are type-safe function pointers that can reference both static and instance methods.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "A delegate type defines a method signature. Delegate instances can hold references to static methods, instance methods, or anonymous lambdas, as long as the signature matches. Multicast delegates can hold multiple method references." },

        new() { Id = 66, TopicId = 1, Difficulty = DifficultyLevel.Senior, Type = QuestionType.CodeSnippet,
            Text = "What is the output?",
            CodeSnippet = "class Animal\n{\n    public virtual string Speak() => \"...\";\n}\nclass Cat : Animal\n{\n    public override string Speak() => \"Meow\";\n}\nclass Kitten : Cat\n{\n    public override string Speak() => \"mew\";\n}\n\nAnimal a = new Kitten();\nConsole.WriteLine(a.Speak());",
            Options = ["...", "Meow", "mew", "Compile error"],
            CorrectAnswer = "mew",
            Explanation = "Virtual dispatch always resolves to the most derived override. Even though the variable is typed as Animal, the runtime type is Kitten, and Kitten.Speak() is the most-derived override. The result is 'mew'." },

        new() { Id = 67, TopicId = 1, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What does the Interface Segregation Principle (ISP) advise?",
            Options = [
                "Interfaces should have as many members as possible to be comprehensive",
                "Clients should not be forced to depend on interfaces they do not use",
                "Each interface should be implemented by exactly one class",
                "Interfaces must only contain properties, not methods"
            ],
            CorrectAnswer = "Clients should not be forced to depend on interfaces they do not use",
            Explanation = "ISP states that large 'fat' interfaces should be split into smaller, more focused ones. Classes implementing them won't be forced to implement irrelevant members. For example, IWorker should be split into ITaskWorker and IBreakable if not all workers need both." },

        new() { Id = 68, TopicId = 1, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the Open/Closed Principle?",
            Options = [
                "Classes should be open for modification and closed for extension",
                "Classes should be open for extension but closed for modification",
                "Every class must have both public and private members",
                "Abstract classes must remain open and concrete classes must be closed"
            ],
            CorrectAnswer = "Classes should be open for extension but closed for modification",
            Explanation = "OCP (Bertrand Meyer) means you should be able to add new behaviour (extension) without changing existing tested code (modification). In C#, this is achieved via interfaces, abstract classes, and polymorphism — new behaviour comes from new classes, not edits to existing ones." },

        new() { Id = 69, TopicId = 1, Difficulty = DifficultyLevel.Senior, Type = QuestionType.CodeSnippet,
            Text = "What does this code demonstrate?",
            CodeSnippet = "public class EventPublisher\n{\n    public event EventHandler<string> MessageReceived;\n\n    public void Receive(string msg)\n        => MessageReceived?.Invoke(this, msg);\n}\n\nvar pub = new EventPublisher();\npub.MessageReceived += (s, e) => Console.WriteLine(e);\npub.Receive(\"Hello\");",
            Options = ["Delegate multicast", "The Observer pattern using C# events", "The Command pattern", "Method hiding"],
            CorrectAnswer = "The Observer pattern using C# events",
            Explanation = "C# events are a built-in implementation of the Observer pattern. The publisher exposes an event; subscribers register handlers with +=. When the publisher fires the event, all registered handlers are invoked. The ?. null-conditional safely skips the invoke if no subscribers are registered." },

        new() { Id = 70, TopicId = 1, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between 'new' (hiding) and 'override' for virtual methods?",
            Options = [
                "They are identical in behaviour",
                "'override' participates in virtual dispatch; 'new' hides the base member and breaks the polymorphic chain",
                "'new' is only valid on interfaces; 'override' is only valid on abstract methods",
                "'override' requires the base method to be sealed"
            ],
            CorrectAnswer = "'override' participates in virtual dispatch; 'new' hides the base member and breaks the polymorphic chain",
            Explanation = "With 'override', calling through a base-type variable invokes the derived implementation (runtime dispatch). With 'new', the call is resolved at compile time based on the variable's declared type — the derived method is invisible through a base reference. 'new' is for intentional name shadowing, not polymorphism." },

        new() { Id = 71, TopicId = 1, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "When is it appropriate to use an abstract class rather than an interface?",
            Options = [
                "Whenever you need multiple inheritance",
                "When you want to provide shared state, default implementations, or a common base that enforces a template across related types",
                "When the implementing types should not need to override any members",
                "Abstract classes and interfaces are always interchangeable"
            ],
            CorrectAnswer = "When you want to provide shared state, default implementations, or a common base that enforces a template across related types",
            Explanation = "Use abstract classes when related types share code, fields, or non-public members. Use interfaces for unrelated types that need to fulfil a contract. Abstract classes model an 'is-a' relationship with shared plumbing; interfaces model 'can-do' capabilities." },

        new() { Id = 72, TopicId = 1, Difficulty = DifficultyLevel.Lead, Type = QuestionType.CodeSnippet,
            Text = "What design principle does this code violate?",
            CodeSnippet = "class Report\n{\n    public void GenerateReport() { /* ... */ }\n    public void SaveToDatabase()  { /* ... */ }\n    public void SendByEmail()     { /* ... */ }\n    public void PrintToPdf()      { /* ... */ }\n}",
            Options = ["Open/Closed Principle", "Single Responsibility Principle", "Liskov Substitution Principle", "Dependency Inversion Principle"],
            CorrectAnswer = "Single Responsibility Principle",
            Explanation = "The Report class has four distinct reasons to change: report generation logic, database persistence, email delivery, and PDF printing. SRP requires each class to have only one reason to change. The fix is to extract separate classes: ReportGenerator, ReportRepository, ReportEmailer, ReportPdfExporter." },

        new() { Id = 73, TopicId = 1, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the purpose of a static constructor in C#?",
            Options = [
                "To create static instances of the class",
                "To initialise static members exactly once, before the first use of the type",
                "To prevent instance construction",
                "To run initialization code on every object creation"
            ],
            CorrectAnswer = "To initialise static members exactly once, before the first use of the type",
            Explanation = "A static constructor (static ClassName()) has no access modifier, takes no parameters, and is called automatically by the runtime before the first instance is created or any static member is accessed. It runs exactly once per AppDomain. Use it for complex static field initialisation." },

        new() { Id = 74, TopicId = 1, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What does 'polymorphism' mean in OOP?",
            Options = [
                "A class can only have one form",
                "An object can take many forms — the same interface can refer to different underlying types",
                "All methods must have the same name",
                "A class must implement multiple interfaces"
            ],
            CorrectAnswer = "An object can take many forms — the same interface can refer to different underlying types",
            Explanation = "Polymorphism allows a base class reference to point to different derived types at runtime, and the correct method is dispatched based on the actual type. This is the foundation of virtual dispatch in C#." },

        new() { Id = 75, TopicId = 1, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "An interface in C# can contain fields.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "Interfaces cannot contain instance fields. They can declare properties, methods, events, and indexers. Since C# 8, interfaces can have static fields and default method implementations, but instance fields are not allowed." },

        new() { Id = 76, TopicId = 1, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the result of calling a virtual method from within a base class constructor?",
            Options = [
                "The base class version of the method always runs",
                "The derived class override may run before the derived constructor body, potentially using uninitialised state",
                "A compile error occurs",
                "The method is skipped"
            ],
            CorrectAnswer = "The derived class override may run before the derived constructor body, potentially using uninitialised state",
            Explanation = "Calling virtual methods in constructors is dangerous. When a derived class is being constructed, the base constructor runs first. If a virtual method called from the base constructor is overridden in the derived class, the override runs before the derived constructor body — meaning derived fields may still be at their default values." },

        new() { Id = 77, TopicId = 1, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is constructor chaining using 'this()'?",
            Options = [
                "Calling a method from the constructor",
                "Calling another constructor in the same class from a constructor, to avoid duplicating initialization logic",
                "Inheriting a constructor from the base class",
                "Overriding the default constructor"
            ],
            CorrectAnswer = "Calling another constructor in the same class from a constructor, to avoid duplicating initialization logic",
            Explanation = "Constructor chaining with 'this(args)' calls another constructor in the same class before the current constructor body runs. This avoids code duplication when constructors share initialization logic.",
            ExampleCode = "class Point\n{\n    public int X, Y, Z;\n    public Point(int x, int y) : this(x, y, 0) { }\n    public Point(int x, int y, int z) { X=x; Y=y; Z=z; }\n}" },

        new() { Id = 78, TopicId = 1, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between 'is' and 'as' operators in C#?",
            Options = [
                "'is' throws on failure; 'as' returns null on failure",
                "'is' returns a bool and can declare a pattern variable; 'as' attempts a cast and returns null on failure (for reference/nullable types)",
                "'as' throws InvalidCastException on failure; 'is' does not",
                "They are identical operators"
            ],
            CorrectAnswer = "'is' returns a bool and can declare a pattern variable; 'as' attempts a cast and returns null on failure (for reference/nullable types)",
            Explanation = "'is' checks if an object is compatible with a type and (since C# 7) can introduce a pattern variable: 'if (obj is Dog d)'. 'as' performs a safe cast and returns null if the cast fails, but only works with reference types and nullable value types. Direct cast '(T)x' throws InvalidCastException on failure." },

        new() { Id = 79, TopicId = 1, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "How does C# implement multiple dispatch or method selection closest to it?",
            Options = [
                "Through the 'dynamic' keyword which defers method resolution to runtime",
                "Through operator overloading",
                "Through multiple interface implementation",
                "C# does not support any form of multiple dispatch"
            ],
            CorrectAnswer = "Through the 'dynamic' keyword which defers method resolution to runtime",
            Explanation = "C# single dispatch selects methods based on the type of one receiver. 'dynamic' enables runtime resolution of overloads based on actual argument types (multiple dispatch behaviour). The Visitor pattern is a design-level workaround for double dispatch without 'dynamic'." },

        new() { Id = 80, TopicId = 1, Difficulty = DifficultyLevel.Lead, Type = QuestionType.TrueFalse,
            Text = "A sealed override method in C# prevents further overriding in derived classes.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "'sealed override' on a method means the method overrides the base class virtual member but cannot be overridden further down the inheritance hierarchy. It is the terminal point in the virtual dispatch chain for that method." },

        new() { Id = 81, TopicId = 1, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What is abstraction in OOP?",
            Options = [
                "Hiding the internal implementation details and showing only essential features",
                "Allowing a class to have multiple constructors",
                "Making all class members private",
                "Preventing a class from being instantiated"
            ],
            CorrectAnswer = "Hiding the internal implementation details and showing only essential features",
            Explanation = "Abstraction exposes only what is relevant to the caller and hides the 'how'. Abstract classes and interfaces are C# mechanisms for abstraction — callers interact with the contract, not the implementation details." },

        new() { Id = 82, TopicId = 1, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "What is the output?",
            CodeSnippet = "class A\n{\n    public A() => Console.Write(\"A \");\n}\nclass B : A\n{\n    public B() => Console.Write(\"B \");\n}\nclass C : B\n{\n    public C() => Console.Write(\"C \");\n}\nnew C();",
            Options = ["C B A", "A B C", "C", "B A C"],
            CorrectAnswer = "A B C",
            Explanation = "Constructors run from most base to most derived. When 'new C()' is called: A() runs first, then B(), then C(). Output is 'A B C'." },

        new() { Id = 83, TopicId = 1, Difficulty = DifficultyLevel.Mid, Type = QuestionType.TrueFalse,
            Text = "The 'protected' access modifier allows access from derived classes even in a different assembly.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "'protected' members are accessible from within the declaring class and all derived classes, regardless of assembly. For assembly-restricted protected access, use 'private protected' (C# 7.2)." },

        new() { Id = 84, TopicId = 1, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the purpose of the 'params' keyword on a method parameter?",
            Options = [
                "Makes the parameter optional with a default value",
                "Allows the caller to pass a variable number of arguments which are collected into an array",
                "Passes arguments by reference",
                "Marks the parameter as output-only"
            ],
            CorrectAnswer = "Allows the caller to pass a variable number of arguments which are collected into an array",
            Explanation = "'params T[] args' lets callers pass zero or more arguments of type T without explicitly constructing an array. The compiler packages the arguments into an array. It must be the last parameter in the method signature.",
            ExampleCode = "void Print(params string[] items)\n{\n    foreach (var item in items)\n        Console.WriteLine(item);\n}\n\nPrint(\"a\", \"b\", \"c\"); // no array needed at call site" },

        new() { Id = 85, TopicId = 1, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the Liskov Substitution Principle implication for method signatures in derived classes?",
            Options = [
                "Derived methods must have identical signatures to base methods",
                "Derived overrides should accept at least as broad inputs (contravariance) and return at least as specific outputs (covariance) as the base",
                "Derived classes must add new method parameters",
                "Derived classes may not change return types"
            ],
            CorrectAnswer = "Derived overrides should accept at least as broad inputs (contravariance) and return at least as specific outputs (covariance) as the base",
            Explanation = "LSP in terms of behavioral subtyping: preconditions cannot be strengthened (accept broader input) and postconditions cannot be weakened (guarantee at least as specific output). This ensures substitutability without surprising callers." },

        new() { Id = 86, TopicId = 1, Difficulty = DifficultyLevel.Lead, Type = QuestionType.CodeSnippet,
            Text = "What pattern does this code implement?",
            CodeSnippet = "public abstract class DataProcessor\n{\n    public void Process()\n    {\n        ReadData();\n        ProcessData();\n        WriteData();\n    }\n\n    protected abstract void ReadData();\n    protected abstract void ProcessData();\n    protected virtual void WriteData() => Console.WriteLine(\"Writing...\");\n}",
            Options = ["Strategy pattern", "Template Method pattern", "Factory Method pattern", "Decorator pattern"],
            CorrectAnswer = "Template Method pattern",
            Explanation = "The Template Method pattern defines the skeleton of an algorithm in a base class method (Process()), deferring some steps to subclasses via abstract or virtual methods. The overall structure is fixed; derived classes fill in the specifics. This enforces the algorithm's order while allowing customisation." },

        new() { Id = 87, TopicId = 1, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What does 'inheritance' allow a derived class to do?",
            Options = [
                "Inherit the private members of the base class",
                "Reuse fields, properties, and methods defined in a base class",
                "Override static methods from the base class",
                "Create multiple instances of the base class automatically"
            ],
            CorrectAnswer = "Reuse fields, properties, and methods defined in a base class",
            Explanation = "Inheritance lets derived classes reuse public and protected members of the base class, extending or specialising behaviour. Private members of the base class are not directly accessible in derived classes." },

        new() { Id = 88, TopicId = 1, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between an abstract method and a virtual method?",
            Options = [
                "Abstract methods have a body; virtual methods do not",
                "Abstract methods have no body and must be overridden; virtual methods have a body and may optionally be overridden",
                "Virtual methods can only be in abstract classes",
                "There is no difference"
            ],
            CorrectAnswer = "Abstract methods have no body and must be overridden; virtual methods have a body and may optionally be overridden",
            Explanation = "An abstract method declares a contract with no implementation and forces derived concrete classes to provide one. A virtual method provides a default implementation that derived classes can optionally override. Abstract methods can only exist in abstract classes." },

        new() { Id = 89, TopicId = 1, Difficulty = DifficultyLevel.Senior, Type = QuestionType.TrueFalse,
            Text = "In C#, a delegate can reference multiple methods simultaneously (multicast delegate).",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "All delegate types in C# derive from MulticastDelegate, which maintains an invocation list. Using += adds methods to the list; -= removes them. When the delegate is invoked, all methods in the list are called in order. C# events are built on multicast delegates." },

        new() { Id = 90, TopicId = 1, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What problem does the 'new' constraint on a generic type parameter solve?",
            Options = [
                "It restricts the type to value types only",
                "It guarantees the type has a public parameterless constructor, enabling 'new T()' inside the generic method",
                "It prevents the type parameter from being nullable",
                "It makes the generic method create singleton instances"
            ],
            CorrectAnswer = "It guarantees the type has a public parameterless constructor, enabling 'new T()' inside the generic method",
            Explanation = "Without the 'new()' constraint, you cannot call 'new T()' inside a generic method because the compiler cannot guarantee T has a constructor. The constraint 'where T : new()' enforces this at the call site, allowing the generic code to instantiate T.",
            ExampleCode = "T CreateInstance<T>() where T : new()\n{\n    return new T(); // valid because of new() constraint\n}" },

        new() { Id = 91, TopicId = 1, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between composition and inheritance for code reuse?",
            Options = [
                "They are equivalent approaches with the same trade-offs",
                "Inheritance creates tight coupling via 'is-a'; composition is more flexible via 'has-a', allowing behaviour change at runtime",
                "Composition is only used in functional programming",
                "Inheritance is always preferred because it uses less code"
            ],
            CorrectAnswer = "Inheritance creates tight coupling via 'is-a'; composition is more flexible via 'has-a', allowing behaviour change at runtime",
            Explanation = "'Favour composition over inheritance' (GoF) means injecting collaborating objects rather than inheriting from them. Inheritance couples derived classes to base class internals; any change in the base can break derived classes. Composition lets you swap implementations at runtime (Strategy pattern) and avoids fragile base class problems." },

        // ── LINQ additional (TopicId = 2) IDs 92–133 ────────────────────────
        new() { Id = 92, TopicId = 2, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "Which LINQ method filters a sequence based on a predicate?",
            Options = ["Select", "Where", "OrderBy", "GroupBy"],
            CorrectAnswer = "Where",
            Explanation = "Where(predicate) returns only elements for which the predicate returns true. It is equivalent to a SQL WHERE clause and is the primary filtering operator in LINQ." },

        new() { Id = 93, TopicId = 2, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "LINQ's OrderBy() sorts in descending order by default.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "OrderBy() sorts in ascending order. Use OrderByDescending() for descending order. To sort by multiple keys, chain ThenBy() or ThenByDescending() after the initial sort." },

        new() { Id = 94, TopicId = 2, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What does the LINQ Count() method return?",
            Options = [
                "The first element in the sequence",
                "The number of elements in the sequence",
                "The sum of all numeric elements",
                "A boolean indicating whether the sequence has elements"
            ],
            CorrectAnswer = "The number of elements in the sequence",
            Explanation = "Count() returns an int representing the number of elements. An optional predicate overload counts only matching elements. For sequences where you only need to check existence, Any() is more efficient than Count() > 0." },

        new() { Id = 95, TopicId = 2, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "The LINQ Any() method returns true if at least one element satisfies the predicate.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "Any(predicate) short-circuits on the first match and returns true. Any() with no argument returns true if the sequence is non-empty. It is more efficient than Count() > 0 because it stops iterating at the first match." },

        new() { Id = 96, TopicId = 2, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What does LINQ's All() method do?",
            Options = [
                "Returns all elements in the sequence",
                "Returns true only if every element satisfies the predicate",
                "Returns a new sequence containing all projected values",
                "Returns true if the sequence is empty"
            ],
            CorrectAnswer = "Returns true only if every element satisfies the predicate",
            Explanation = "All(predicate) returns true if every element matches, and false as soon as any element fails the predicate (short-circuits). By vacuous truth, All() on an empty sequence returns true." },

        new() { Id = 97, TopicId = 2, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between Single() and First() in LINQ?",
            Options = [
                "Single() is faster",
                "Single() throws if there is more than one matching element; First() returns the first match regardless of how many exist",
                "First() throws on multiple matches; Single() does not",
                "They are identical"
            ],
            CorrectAnswer = "Single() throws if there is more than one matching element; First() returns the first match regardless of how many exist",
            Explanation = "Single() asserts that exactly one element matches — it throws InvalidOperationException if zero or more than one element matches. First() returns the first matching element and ignores the rest. Use Single() when business rules guarantee uniqueness (e.g., looking up by primary key)." },

        new() { Id = 98, TopicId = 2, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "What is the output?",
            CodeSnippet = "var numbers = new[] { 1, 2, 3, 4, 5 };\nvar result = numbers\n    .Where(n => n % 2 == 0)\n    .Select(n => n * n);\nConsole.WriteLine(string.Join(\", \", result));",
            Options = ["1, 4, 9, 16, 25", "4, 16", "2, 4", "1, 9, 25"],
            CorrectAnswer = "4, 16",
            Explanation = "Where filters to even numbers: 2 and 4. Select squares them: 4 and 16. The pipeline is deferred and evaluated when string.Join enumerates the result." },

        new() { Id = 99, TopicId = 2, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What does ThenBy() do in LINQ?",
            Options = [
                "Adds a secondary sort key after an OrderBy or OrderByDescending",
                "Replaces the primary sort key",
                "Filters elements after sorting",
                "Groups sorted elements"
            ],
            CorrectAnswer = "Adds a secondary sort key after an OrderBy or OrderByDescending",
            Explanation = "ThenBy() and ThenByDescending() define additional sort criteria for elements that compare as equal on the primary key. Example: .OrderBy(p => p.LastName).ThenBy(p => p.FirstName) sorts by last name, then first name within the same last name." },

        new() { Id = 100, TopicId = 2, Difficulty = DifficultyLevel.Mid, Type = QuestionType.TrueFalse,
            Text = "LINQ's Distinct() uses the default equality comparer to remove duplicates.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "Distinct() uses the default equality comparer (Equals and GetHashCode) by default. You can pass a custom IEqualityComparer<T> to Distinct() if you need non-default comparison logic, such as case-insensitive string deduplication." },

        new() { Id = 101, TopicId = 2, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What does LINQ's Skip(n).Take(m) combination achieve?",
            Options = [
                "Sorts and then limits the sequence",
                "Skips the first n elements then returns the next m elements — commonly used for pagination",
                "Groups elements into chunks of m, skipping n groups",
                "Filters elements where index is between n and m"
            ],
            CorrectAnswer = "Skips the first n elements then returns the next m elements — commonly used for pagination",
            Explanation = "Skip(n) bypasses the first n elements; Take(m) returns at most m elements from whatever remains. Together they implement pagination: page 2 with 10 items per page is .Skip(10).Take(10). In EF Core, these translate to OFFSET/FETCH SQL clauses." },

        new() { Id = 102, TopicId = 2, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "What does this LINQ query return?",
            CodeSnippet = "var words = new[] { \"cat\", \"car\", \"dog\", \"card\" };\nvar result = words.Where(w => w.StartsWith(\"ca\"))\n                  .OrderByDescending(w => w.Length)\n                  .ToList();",
            Options = ["[\"car\", \"cat\", \"card\"]", "[\"card\", \"cat\", \"car\"]", "[\"card\", \"car\", \"cat\"]", "[\"cat\", \"car\"]"],
            CorrectAnswer = "[\"card\", \"car\", \"cat\"]",
            Explanation = "Where filters to words starting with 'ca': cat, car, card. OrderByDescending by length: card (4), car (3), cat (3). Among same-length elements (car and cat), the original order is preserved (stable sort), so car comes before cat." },

        new() { Id = 103, TopicId = 2, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What does LINQ's Aggregate() method do?",
            Options = [
                "Groups elements by a key",
                "Applies an accumulator function over a sequence to produce a single result",
                "Combines two sequences into one",
                "Counts elements by category"
            ],
            CorrectAnswer = "Applies an accumulator function over a sequence to produce a single result",
            Explanation = "Aggregate() is the general-purpose fold/reduce operation. It maintains a running accumulator and applies the provided function with each element. Sum(), Max(), Count() are all specialisations. Example: numbers.Aggregate(0, (acc, n) => acc + n) computes the sum.",
            ExampleCode = "var product = new[] { 1, 2, 3, 4 }\n    .Aggregate(1, (acc, n) => acc * n); // 24\n\nvar csv = new[] { \"a\", \"b\", \"c\" }\n    .Aggregate((acc, s) => acc + \",\" + s); // \"a,b,c\"" },

        new() { Id = 104, TopicId = 2, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between Union() and Concat() in LINQ?",
            Options = [
                "They are identical",
                "Union() combines two sequences and removes duplicates; Concat() simply appends one sequence to another",
                "Concat() removes duplicates; Union() does not",
                "Union() only works on sorted sequences"
            ],
            CorrectAnswer = "Union() combines two sequences and removes duplicates; Concat() simply appends one sequence to another",
            Explanation = "Concat() is like SQL UNION ALL — it appends all elements, including duplicates. Union() is like SQL UNION — it deduplicates using the default equality comparer. Use Concat() when performance matters and duplicates are acceptable; use Union() for set semantics." },

        new() { Id = 105, TopicId = 2, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What does LINQ's Join() do?",
            Options = [
                "Concatenates two string sequences",
                "Performs an inner join between two sequences based on matching keys",
                "Merges two sorted sequences",
                "Groups a sequence by a foreign key"
            ],
            CorrectAnswer = "Performs an inner join between two sequences based on matching keys",
            Explanation = "Join(inner, outerKey, innerKey, resultSelector) correlates elements from two sequences where the keys match — equivalent to SQL INNER JOIN. GroupJoin() performs a left outer join and returns a hierarchical result grouping inner elements under each outer element.",
            ExampleCode = "var result = orders.Join(\n    customers,\n    o => o.CustomerId,\n    c => c.Id,\n    (o, c) => new { o.Id, c.Name });" },

        new() { Id = 106, TopicId = 2, Difficulty = DifficultyLevel.Senior, Type = QuestionType.TrueFalse,
            Text = "LINQ's Intersect() returns elements that appear in both sequences.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "Intersect() returns the set intersection — only elements that exist in both sequences, deduplicated. Except() returns elements in the first sequence that are not in the second. Both use the default equality comparer unless a custom IEqualityComparer is provided." },

        new() { Id = 107, TopicId = 2, Difficulty = DifficultyLevel.Senior, Type = QuestionType.CodeSnippet,
            Text = "What does this code print?",
            CodeSnippet = "var nums = new[] { 3, 1, 4, 1, 5, 9, 2, 6 };\nvar result = nums\n    .OrderBy(n => n)\n    .Distinct()\n    .Take(4);\nConsole.WriteLine(string.Join(\", \", result));",
            Options = ["3, 1, 4, 1", "1, 2, 3, 4", "1, 4, 5, 9", "3, 1, 4, 5"],
            CorrectAnswer = "1, 2, 3, 4",
            Explanation = "OrderBy gives: 1,1,2,3,4,5,6,9. Distinct removes duplicates: 1,2,3,4,5,6,9. Take(4) returns the first 4: 1,2,3,4." },

        new() { Id = 108, TopicId = 2, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is ToLookup() and how does it differ from GroupBy()?",
            Options = [
                "They are identical",
                "ToLookup() eagerly evaluates and creates an in-memory lookup; GroupBy() is deferred",
                "GroupBy() is eager; ToLookup() is deferred",
                "ToLookup() only works with string keys"
            ],
            CorrectAnswer = "ToLookup() eagerly evaluates and creates an in-memory lookup; GroupBy() is deferred",
            Explanation = "ToLookup() immediately materialises a sequence into an ILookup<TKey, TElement> — a dictionary-like structure where each key maps to a collection. GroupBy() is deferred and re-evaluated on each enumeration. Use ToLookup() when you'll query the same groupings multiple times.",
            ExampleCode = "// Eager — query once, access many times\nILookup<string, Order> byStatus = orders.ToLookup(o => o.Status);\nvar pending = byStatus[\"Pending\"]; // O(1) lookup" },

        new() { Id = 109, TopicId = 2, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What does LINQ's Zip() operator do?",
            Options = [
                "Compresses a sequence into a smaller form",
                "Merges two sequences element-by-element using a selector function",
                "Splits a sequence into two separate sequences",
                "Groups adjacent equal elements"
            ],
            CorrectAnswer = "Merges two sequences element-by-element using a selector function",
            Explanation = "Zip() pairs elements from two (or three in .NET 6+) sequences by position and applies a result selector. It stops when the shorter sequence is exhausted. Example: names.Zip(scores, (n, s) => $\"{n}: {s}\") combines parallel arrays.",
            ExampleCode = "var names = new[] { \"Alice\", \"Bob\" };\nvar scores = new[] { 95, 87 };\nvar result = names.Zip(scores, (n, s) => $\"{n}: {s}\");\n// [\"Alice: 95\", \"Bob: 87\"]" },

        new() { Id = 110, TopicId = 2, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the query syntax equivalent of the method syntax 'source.Where(x => x.Age > 18).Select(x => x.Name)'?",
            Options = [
                "from x in source where x.Age > 18 orderby x.Name select x.Name",
                "from x in source where x.Age > 18 select x.Name",
                "select x.Name from x in source where x.Age > 18",
                "query x from source filter x.Age > 18 project x.Name"
            ],
            CorrectAnswer = "from x in source where x.Age > 18 select x.Name",
            Explanation = "LINQ query syntax always starts with 'from' and ends with 'select' or 'group'. The compiler translates query syntax to method syntax. 'from x in source where x.Age > 18 select x.Name' is exactly equivalent to source.Where(x => x.Age > 18).Select(x => x.Name)." },

        new() { Id = 111, TopicId = 2, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the 'let' clause in LINQ query syntax used for?",
            Options = [
                "Declaring a loop variable",
                "Introducing a sub-variable to store an intermediate computation, usable in later clauses",
                "Limiting the number of results returned",
                "Defining a local function within the query"
            ],
            CorrectAnswer = "Introducing a sub-variable to store an intermediate computation, usable in later clauses",
            Explanation = "'let' creates a range variable that holds a computed value, avoiding recomputation in subsequent clauses. Example: 'let upper = name.ToUpper() where upper.StartsWith(\"A\") select upper' stores the uppercase result once.",
            ExampleCode = "var result = from name in names\n             let upper = name.ToUpper()\n             where upper.StartsWith(\"A\")\n             select upper;" },

        new() { Id = 112, TopicId = 2, Difficulty = DifficultyLevel.Lead, Type = QuestionType.TrueFalse,
            Text = "Calling ToList() on an IQueryable forces immediate execution and returns results in memory.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "ToList() (and ToArray(), ToDictionary(), Count(), First(), etc.) are terminal operators that trigger execution of the LINQ pipeline immediately. For IQueryable, this translates the expression tree to SQL and fetches results; for IEnumerable, it iterates the pipeline and materialises to a list." },

        new() { Id = 113, TopicId = 2, Difficulty = DifficultyLevel.Lead, Type = QuestionType.CodeSnippet,
            Text = "What is the potential performance issue with this code?",
            CodeSnippet = "IEnumerable<string> expensiveQuery = GetAllProductNames(); // returns 1M items\n\nif (expensiveQuery.Count() > 0)\n{\n    var first = expensiveQuery.First();\n}",
            Options = [
                "Count() is not available on IEnumerable",
                "The query is evaluated twice — once for Count() and once for First(); use Any() then cache the result",
                "First() will throw an exception",
                "No issue, this is optimal"
            ],
            CorrectAnswer = "The query is evaluated twice — once for Count() and once for First(); use Any() then cache the result",
            Explanation = "Each terminal operation re-executes the deferred query. Count() iterates all 1M items; then First() re-iterates. Fix: use Any() instead of Count() > 0 (short-circuits at first element), or materialise once: var list = expensiveQuery.ToList()." },

        new() { Id = 114, TopicId = 2, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What does LINQ's GroupJoin() produce that Join() does not?",
            Options = [
                "A sorted result set",
                "A left outer join result — each outer element paired with a (possibly empty) collection of matching inner elements",
                "A cross join between sequences",
                "A deduplicated inner join"
            ],
            CorrectAnswer = "A left outer join result — each outer element paired with a (possibly empty) collection of matching inner elements",
            Explanation = "GroupJoin() is like SQL LEFT OUTER JOIN. Each element from the outer sequence is paired with an IEnumerable of matching inner elements (which may be empty). This preserves all outer elements. Join() produces only matched pairs (inner join), dropping unmatched outer elements.",
            ExampleCode = "var result = customers.GroupJoin(\n    orders,\n    c => c.Id,\n    o => o.CustomerId,\n    (c, orderGroup) => new { c.Name, Orders = orderGroup });\n// All customers included, Orders = [] if none" },

        new() { Id = 115, TopicId = 2, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "Which LINQ method converts a sequence to a Dictionary?",
            Options = ["ToLookup()", "ToDictionary()", "ToHashSet()", "ToArray()"],
            CorrectAnswer = "ToDictionary()",
            Explanation = "ToDictionary(keySelector) materialises a sequence into a Dictionary<TKey, TValue>. It throws ArgumentException if there are duplicate keys. If you expect multiple values per key, use ToLookup() instead, which handles duplicates gracefully." },

        new() { Id = 116, TopicId = 2, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What does LINQ's Sum() method do?",
            Options = [
                "Returns the maximum value",
                "Returns the arithmetic sum of numeric elements in a sequence",
                "Returns the product of all elements",
                "Counts non-null elements"
            ],
            CorrectAnswer = "Returns the arithmetic sum of numeric elements in a sequence",
            Explanation = "Sum() computes the total of a numeric sequence or a numeric projection. Example: orders.Sum(o => o.Total) adds up the Total property across all orders. It returns 0 for an empty sequence. Average(), Min(), and Max() work similarly." },

        new() { Id = 117, TopicId = 2, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "LINQ's Contains() checks whether a sequence includes a specific element.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "Contains(value) returns true if the sequence includes the specified value using the default equality comparer. For IQueryable, it translates to SQL IN or EXISTS. An overload accepts a custom IEqualityComparer<T>." },

        new() { Id = 118, TopicId = 2, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "What is printed?",
            CodeSnippet = "var data = new[] { 1, 2, 3, 4, 5 };\nbool allPositive = data.All(n => n > 0);\nbool anyNegative = data.Any(n => n < 0);\nConsole.WriteLine($\"{allPositive} {anyNegative}\");",
            Options = ["True True", "False False", "True False", "False True"],
            CorrectAnswer = "True False",
            Explanation = "All elements are positive so All(n > 0) is true. No elements are negative so Any(n < 0) is false. Output: 'True False'." },

        new() { Id = 119, TopicId = 2, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between SkipWhile() and Skip() in LINQ?",
            Options = [
                "SkipWhile() takes a count; Skip() takes a predicate",
                "Skip() skips a fixed number of elements; SkipWhile() skips elements as long as the predicate is true, then returns the rest",
                "They are identical",
                "SkipWhile() only works with sorted sequences"
            ],
            CorrectAnswer = "Skip() skips a fixed number of elements; SkipWhile() skips elements as long as the predicate is true, then returns the rest",
            Explanation = "SkipWhile(predicate) skips consecutive elements from the start while the predicate holds, then yields all remaining elements. Once the predicate returns false, skipping stops — subsequent elements matching the predicate are NOT skipped. TakeWhile() works similarly for taking." },

        new() { Id = 120, TopicId = 2, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "When using LINQ with EF Core, what does calling AsEnumerable() in the middle of a query do?",
            Options = [
                "Converts the query to use parallel execution",
                "Forces the query to execute at that point and switches subsequent operations to in-memory LINQ",
                "Prevents the query from ever executing",
                "Has no effect"
            ],
            CorrectAnswer = "Forces the query to execute at that point and switches subsequent operations to in-memory LINQ",
            Explanation = "AsEnumerable() triggers database execution and pulls results into memory. Any LINQ operators applied after AsEnumerable() run in-memory (LINQ to Objects). This is useful when EF Core cannot translate a specific operation to SQL — apply the untranslatable filter after AsEnumerable()." },

        new() { Id = 121, TopicId = 2, Difficulty = DifficultyLevel.Lead, Type = QuestionType.CodeSnippet,
            Text = "What does this code demonstrate, and what is the risk?",
            CodeSnippet = "IEnumerable<int> GetNumbers()\n{\n    Console.WriteLine(\"Generating...\");\n    yield return 1;\n    yield return 2;\n    yield return 3;\n}\n\nvar nums = GetNumbers();\nConsole.WriteLine(nums.Count());\nConsole.WriteLine(nums.Sum());",
            Options = [
                "Generates once; Count and Sum share the iteration",
                "Deferred execution — GetNumbers() body runs twice: once for Count() and once for Sum()",
                "Compile error — yield return cannot be used with LINQ",
                "Only runs once because the result is cached"
            ],
            CorrectAnswer = "Deferred execution — GetNumbers() body runs twice: once for Count() and once for Sum()",
            Explanation = "Each terminal operation on a deferred IEnumerable re-executes the iterator. 'Generating...' is printed twice. To avoid multiple iterations, materialise first: var nums = GetNumbers().ToList(). This is a common performance bug with generators." },

        new() { Id = 122, TopicId = 2, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is a compiled query in LINQ to Entities and when would you use one?",
            Options = [
                "A query stored in a string variable",
                "A query whose expression tree is translated to SQL once and cached, avoiding repeated translation overhead",
                "A query that runs on a background thread",
                "A query validated at compile time via source generators"
            ],
            CorrectAnswer = "A query whose expression tree is translated to SQL once and cached, avoiding repeated translation overhead",
            Explanation = "EF Core compiles and caches query plans automatically. In earlier EF or high-throughput scenarios, EF.CompileQuery() lets you explicitly pre-compile a query delegate. The first call pays the translation cost; subsequent calls reuse the compiled SQL plan, reducing CPU overhead for frequently executed queries." },

        new() { Id = 123, TopicId = 2, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What does LINQ's Last() method do when the sequence is empty?",
            Options = [
                "Returns null",
                "Returns the default value for T",
                "Throws InvalidOperationException",
                "Returns the first element"
            ],
            CorrectAnswer = "Throws InvalidOperationException",
            Explanation = "Last() throws InvalidOperationException if the sequence contains no elements. LastOrDefault() returns default(T) instead. Note: Last() on IEnumerable iterates the entire sequence to find the last element — for large sequences or databases, prefer ordering and First()." },

        new() { Id = 124, TopicId = 2, Difficulty = DifficultyLevel.Mid, Type = QuestionType.TrueFalse,
            Text = "LINQ's GroupBy() in LINQ-to-Objects executes immediately and returns grouped results.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "GroupBy() is a deferred operator in LINQ-to-Objects. The grouping is not computed until the result is enumerated. However, once enumeration begins, it processes the full source sequence to build the groups before yielding the first IGrouping." },

        new() { Id = 125, TopicId = 2, Difficulty = DifficultyLevel.Senior, Type = QuestionType.CodeSnippet,
            Text = "What does this SelectMany query produce?",
            CodeSnippet = "var departments = new[]\n{\n    new { Name = \"Eng\",   Members = new[] { \"Alice\", \"Bob\" } },\n    new { Name = \"Sales\", Members = new[] { \"Carol\", \"Dave\" } }\n};\n\nvar allMembers = departments.SelectMany(d => d.Members);\nConsole.WriteLine(string.Join(\", \", allMembers));",
            Options = [
                "[\"Alice\", \"Bob\"], [\"Carol\", \"Dave\"]",
                "Alice, Bob, Carol, Dave",
                "Eng, Sales",
                "Alice, Carol"
            ],
            CorrectAnswer = "Alice, Bob, Carol, Dave",
            Explanation = "SelectMany flattens the nested Members arrays into a single sequence. Each department's Members collection is concatenated in order, producing one flat sequence: Alice, Bob, Carol, Dave." },

        new() { Id = 126, TopicId = 2, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What does LINQ's Min() method return?",
            Options = [
                "The first element of the sequence",
                "The smallest value in the sequence",
                "The average of all values",
                "The element closest to zero"
            ],
            CorrectAnswer = "The smallest value in the sequence",
            Explanation = "Min() returns the minimum value in a numeric sequence, or the minimum of a projected value when a selector is provided. It uses the default comparer and throws InvalidOperationException on an empty sequence (MinOrDefault is available from .NET 6+)." },

        new() { Id = 127, TopicId = 2, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "How do you perform a case-insensitive Distinct() in LINQ?",
            Options = [
                "Distinct(StringComparer.OrdinalIgnoreCase) is not supported",
                "Pass a custom IEqualityComparer to Distinct(comparer)",
                "Use .ToUpper().Distinct()",
                "Use DistinctBy(s => s.ToUpper())"
            ],
            CorrectAnswer = "Pass a custom IEqualityComparer to Distinct(comparer)",
            Explanation = "Distinct() accepts an optional IEqualityComparer<T>. For strings: items.Distinct(StringComparer.OrdinalIgnoreCase). In .NET 6+, DistinctBy(keySelector) is another option: items.DistinctBy(s => s.ToUpperInvariant())." },

        new() { Id = 128, TopicId = 2, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is 'query expression pattern' and how does the C# compiler use it with LINQ?",
            Options = [
                "A runtime reflection mechanism for LINQ methods",
                "The compiler translates query syntax into method calls based on duck typing — any type with the right method signatures can support query syntax",
                "A design pattern for building IQueryable providers",
                "A feature exclusive to EF Core"
            ],
            CorrectAnswer = "The compiler translates query syntax into method calls based on duck typing — any type with the right method signatures can support query syntax",
            Explanation = "The C# compiler translates 'from x in source select x.Name' into source.Select(x => x.Name) regardless of the actual type. The type does not need to implement IEnumerable — it just needs a SelectMany/Select/Where/etc. method with the right signature. This enabled async streams and custom monad-like types to use query syntax." },

        new() { Id = 129, TopicId = 2, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between Except() and Where() with a negated Contains() in LINQ?",
            Options = [
                "They are identical in all cases",
                "Except() uses set semantics and deduplicates; Where(!Contains()) preserves duplicates",
                "Where(!Contains()) is always faster",
                "Except() only works with value types"
            ],
            CorrectAnswer = "Except() uses set semantics and deduplicates; Where(!Contains()) preserves duplicates",
            Explanation = "Except() returns the set difference — elements in the first sequence not in the second, deduplicated. If the first sequence has duplicates, they are also removed. Where(x => !second.Contains(x)) preserves duplicates in the first sequence. Also, Contains() on a list is O(n) per element; converting the exclusion set to a HashSet first is more efficient." },

        new() { Id = 130, TopicId = 2, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What is the purpose of LINQ's Average() method?",
            Options = [
                "Returns the median value",
                "Returns the arithmetic mean of a numeric sequence",
                "Returns the most common value",
                "Returns the geometric mean"
            ],
            CorrectAnswer = "Returns the arithmetic mean of a numeric sequence",
            Explanation = "Average() computes sum / count for a numeric sequence or projection. It returns double (or decimal for decimal inputs). It throws on empty sequences. Use Average(selector) for projections: orders.Average(o => o.Total)." },

        new() { Id = 131, TopicId = 2, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "What is the output?",
            CodeSnippet = "var list = new List<int> { 1, 2, 3 };\nvar query = list.Select(x => x * 2);\nlist.Add(4);\nConsole.WriteLine(string.Join(\", \", query));",
            Options = ["2, 4, 6", "2, 4, 6, 8", "1, 2, 3, 4", "Compile error"],
            CorrectAnswer = "2, 4, 6, 8",
            Explanation = "Deferred execution means the query is not evaluated until enumerated. By the time string.Join iterates the query, list has 4 elements. The query reflects the state of list at enumeration time, so 4 is included and doubled to 8." },

        new() { Id = 132, TopicId = 2, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the purpose of AsQueryable() on an in-memory collection?",
            Options = [
                "Converts a collection to a database-connected query",
                "Wraps an IEnumerable in an IQueryable so it can be passed to methods that expect IQueryable, using LINQ-to-Objects under the hood",
                "Enables parallel query execution",
                "Caches the query for reuse"
            ],
            CorrectAnswer = "Wraps an IEnumerable in an IQueryable so it can be passed to methods that expect IQueryable, using LINQ-to-Objects under the hood",
            Explanation = "AsQueryable() creates a thin IQueryable wrapper over any IEnumerable, useful for testing repository methods that accept IQueryable. The expression trees operate over the in-memory data using LINQ-to-Objects. It does NOT add database capabilities to an in-memory collection." },

        new() { Id = 133, TopicId = 2, Difficulty = DifficultyLevel.Lead, Type = QuestionType.TrueFalse,
            Text = "In EF Core, adding a .ToList() in the middle of a LINQ query chain always results in correct query filtering at the database.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "Inserting ToList() mid-chain materialises the data into memory at that point. Any LINQ operators applied after ToList() run in-memory against all retrieved rows, not at the database. This can cause catastrophic performance issues — pulling millions of rows when only a few are needed." },

        // ── Async/Await additional (TopicId = 3) IDs 134–175 ────────────────
        new() { Id = 134, TopicId = 3, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What return type should an async method use when it returns no value?",
            Options = ["void", "Task", "int", "object"],
            CorrectAnswer = "Task",
            Explanation = "Async methods that produce no result should return Task (not void). This allows callers to await the operation and observe exceptions. async void is reserved for event handlers only — exceptions in async void methods are unobservable and crash the process." },

        new() { Id = 135, TopicId = 3, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "An async method without an await expression inside it will still run asynchronously.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "An async method without an await runs synchronously from start to finish on the calling thread. The compiler issues a warning (CS1998). The async keyword alone does not make code run on a background thread — it only enables the await keyword and wraps the return value in a Task." },

        new() { Id = 136, TopicId = 3, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What does Task.Run() do?",
            Options = [
                "Runs a delegate synchronously on the calling thread",
                "Queues a delegate to run on the ThreadPool and returns a Task representing the work",
                "Creates a new OS thread",
                "Cancels a running task"
            ],
            CorrectAnswer = "Queues a delegate to run on the ThreadPool and returns a Task representing the work",
            Explanation = "Task.Run(delegate) offloads CPU-bound work to the ThreadPool. It should not be used for I/O-bound operations (use async I/O APIs instead). Wrapping an already-async method in Task.Run is redundant and wastes a thread." },

        new() { Id = 137, TopicId = 3, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "await can only be used inside a method marked with the async keyword.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "The await keyword is only valid inside an async method. The async modifier enables the state-machine transformation and allows await to be used within the method body. Without async, using await is a compile error." },

        new() { Id = 138, TopicId = 3, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What happens to an exception thrown inside an async Task method?",
            Options = [
                "It crashes the process immediately",
                "It is captured and stored in the Task, then rethrown when the Task is awaited",
                "It is silently swallowed",
                "It triggers the AppDomain.UnhandledException event immediately"
            ],
            CorrectAnswer = "It is captured and stored in the Task, then rethrown when the Task is awaited",
            Explanation = "Exceptions in async Task methods are caught by the runtime and stored in the Task's Exception property as an AggregateException. When the Task is awaited, the first inner exception is unwrapped and rethrown at the await site, preserving the stack trace." },

        new() { Id = 139, TopicId = 3, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "What is wrong with this code?",
            CodeSnippet = "public async void LoadDataAsync()\n{\n    var data = await FetchDataAsync();\n    UpdateUI(data);\n}",
            Options = [
                "await cannot be used with FetchDataAsync()",
                "async void cannot be awaited, so exceptions escape unobserved and the caller cannot wait for completion",
                "UpdateUI must also be async",
                "Nothing is wrong"
            ],
            CorrectAnswer = "async void cannot be awaited, so exceptions escape unobserved and the caller cannot wait for completion",
            Explanation = "async void should only be used for event handlers. Callers cannot await it, so they cannot handle its exceptions or know when it completes. An exception thrown inside async void propagates to the SynchronizationContext and typically crashes the application. Change to async Task." },

        new() { Id = 140, TopicId = 3, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What does Task.WhenAll do when one of the tasks throws an exception?",
            Options = [
                "It returns immediately with a partial result",
                "It waits for all tasks to complete, then throws an AggregateException containing all exceptions",
                "It cancels all other tasks",
                "Only the first exception is reported"
            ],
            CorrectAnswer = "It waits for all tasks to complete, then throws an AggregateException containing all exceptions",
            Explanation = "Task.WhenAll waits for every task. If any tasks fail, the returned task has a Faulted status with an AggregateException containing all exceptions. When awaited, only the first exception is rethrown. Inspect task.Exception.InnerExceptions to see all errors." },

        new() { Id = 141, TopicId = 3, Difficulty = DifficultyLevel.Mid, Type = QuestionType.TrueFalse,
            Text = "CancellationToken.ThrowIfCancellationRequested() throws OperationCanceledException if cancellation has been requested.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "ThrowIfCancellationRequested() is the standard way to check for cancellation inside a loop or at checkpoints in long-running work. It throws OperationCanceledException (which is a base of TaskCanceledException), resulting in a Canceled task status when awaited." },

        new() { Id = 142, TopicId = 3, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the synchronization context and how does it affect async/await?",
            Options = [
                "A thread-safety mechanism for shared state",
                "An object that controls where continuations run after an await — e.g., marshalling back to the UI thread",
                "A lock that prevents concurrent async operations",
                "A queue that orders async operations sequentially"
            ],
            CorrectAnswer = "An object that controls where continuations run after an await — e.g., marshalling back to the UI thread",
            Explanation = "SynchronizationContext captures the ambient context (UI thread in WPF/WinForms, request context in ASP.NET). By default, await captures it and schedules the continuation on that context. ConfigureAwait(false) opts out, allowing the continuation to run on any ThreadPool thread." },

        new() { Id = 143, TopicId = 3, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "What does this code do?",
            CodeSnippet = "var tasks = Enumerable.Range(1, 5)\n    .Select(i => Task.Run(() => i * i))\n    .ToList();\n\nint[] results = await Task.WhenAll(tasks);\nConsole.WriteLine(string.Join(\", \", results));",
            Options = [
                "Runs tasks sequentially and prints 1, 4, 9, 16, 25",
                "Runs all 5 tasks concurrently and prints their squared values",
                "Deadlocks",
                "Compile error"
            ],
            CorrectAnswer = "Runs all 5 tasks concurrently and prints their squared values",
            Explanation = "Task.Run queues each lambda to the ThreadPool. Task.WhenAll runs them all concurrently and awaits all completions. The results array preserves order matching the input tasks (1,4,9,16,25), regardless of which task finished first." },

        new() { Id = 144, TopicId = 3, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is an async stream (IAsyncEnumerable<T>) and when would you use it?",
            Options = [
                "A stream that reads files asynchronously",
                "An async sequence that yields items one at a time asynchronously, suitable for paginated or real-time data",
                "A thread-safe collection for async code",
                "An alternative to Task<List<T>> that is always faster"
            ],
            CorrectAnswer = "An async sequence that yields items one at a time asynchronously, suitable for paginated or real-time data",
            Explanation = "IAsyncEnumerable<T> (C# 8) enables 'await foreach' loops over sequences that are produced asynchronously — e.g., reading database rows page by page, gRPC streaming, or event streams. Unlike Task<List<T>>, items are processed as they arrive without waiting for the full collection.",
            ExampleCode = "async IAsyncEnumerable<int> GetNumbersAsync()\n{\n    for (int i = 0; i < 10; i++)\n    {\n        await Task.Delay(100);\n        yield return i;\n    }\n}\n\nawait foreach (var num in GetNumbersAsync())\n    Console.WriteLine(num);" },

        new() { Id = 145, TopicId = 3, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "How do you report progress from an async method to a UI?",
            Options = [
                "Use a static shared variable updated by the async method",
                "Pass an IProgress<T> to the method; call progress.Report() inside; bind a Progress<T> on the UI thread",
                "Use Task.ContinueWith to update the UI",
                "Use a CancellationToken with a progress callback"
            ],
            CorrectAnswer = "Pass an IProgress<T> to the method; call progress.Report() inside; bind a Progress<T> on the UI thread",
            Explanation = "IProgress<T> decouples progress reporting from UI concerns. The caller creates a Progress<T> (which captures the SynchronizationContext) and passes it. The async method calls progress.Report(value) without knowing about the UI. Progress<T> marshals the callback to the captured context automatically.",
            ExampleCode = "// UI code:\nvar progress = new Progress<int>(percent =>\n    progressBar.Value = percent);\nawait ProcessAsync(progress);\n\n// Library code:\nasync Task ProcessAsync(IProgress<int> progress)\n{\n    for (int i = 0; i <= 100; i += 10)\n    {\n        await Task.Delay(100);\n        progress?.Report(i);\n    }\n}" },

        new() { Id = 146, TopicId = 3, Difficulty = DifficultyLevel.Senior, Type = QuestionType.TrueFalse,
            Text = "ValueTask<T> can be safely awaited multiple times.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "ValueTask<T> must be awaited at most once. Unlike Task<T>, it is not safe to await multiple times, add multiple continuations, or store it for later. Doing so leads to undefined behaviour. If you need to await multiple times, call .AsTask() to convert it to a Task<T> first." },

        new() { Id = 147, TopicId = 3, Difficulty = DifficultyLevel.Senior, Type = QuestionType.CodeSnippet,
            Text = "What is the problem with this code?",
            CodeSnippet = "private static SemaphoreSlim _sem = new SemaphoreSlim(1, 1);\n\npublic async Task<string> GetCachedDataAsync()\n{\n    await _sem.WaitAsync();\n    try\n    {\n        return await FetchFromCacheAsync();\n    }\n    catch\n    {\n        return null;\n    }\n    // BUG: missing finally\n}",
            Options = [
                "SemaphoreSlim cannot be used with async",
                "If an exception occurs, the semaphore is never released, causing all subsequent callers to wait forever",
                "catch swallowing exceptions is fine here",
                "No bug — catch prevents the semaphore leak"
            ],
            CorrectAnswer = "If an exception occurs, the semaphore is never released, causing all subsequent callers to wait forever",
            Explanation = "The catch block returns null and exits the method without releasing the semaphore. All subsequent callers will block on WaitAsync() forever. The fix is to move _sem.Release() into a finally block, ensuring it always executes regardless of exceptions." },

        new() { Id = 148, TopicId = 3, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between Task.Delay() and Thread.Sleep() in async code?",
            Options = [
                "They are equivalent",
                "Thread.Sleep blocks the thread; Task.Delay releases the thread during the wait and resumes via continuation",
                "Task.Delay blocks the thread; Thread.Sleep does not",
                "Thread.Sleep works with async; Task.Delay does not"
            ],
            CorrectAnswer = "Thread.Sleep blocks the thread; Task.Delay releases the thread during the wait and resumes via continuation",
            Explanation = "Thread.Sleep blocks the calling thread for the duration — wasted capacity. Task.Delay returns a Task that completes after the timeout; awaiting it releases the thread back to the pool immediately. In async code, always use 'await Task.Delay()' instead of Thread.Sleep()." },

        new() { Id = 149, TopicId = 3, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the async state machine generated by the C# compiler?",
            Options = [
                "A background thread dedicated to each async method",
                "A struct implementing IAsyncStateMachine that tracks the async method's execution state across await points",
                "A Task subclass with extra state",
                "A thread-local storage structure"
            ],
            CorrectAnswer = "A struct implementing IAsyncStateMachine that tracks the async method's execution state across await points",
            Explanation = "The compiler transforms async methods into a state machine struct. It captures local variables as fields, tracks which await point the execution is at, and implements MoveNext() to resume from the correct point. This is why async methods can 'pause' without blocking a thread — state is heap-allocated in the state machine object." },

        new() { Id = 150, TopicId = 3, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "When is it appropriate to use Task.Run() inside an ASP.NET Core controller action?",
            Options = [
                "Always, to run all operations off the request thread",
                "For CPU-bound work that would otherwise block the request thread for a long time",
                "Never — Task.Run is not allowed in ASP.NET Core",
                "For all database operations"
            ],
            CorrectAnswer = "For CPU-bound work that would otherwise block the request thread for a long time",
            Explanation = "In ASP.NET Core, there is no SynchronizationContext, so ConfigureAwait(false) has little effect. Task.Run is useful to offload genuinely CPU-bound work (e.g., image processing) that would monopolise the request thread. I/O-bound work should use async I/O APIs directly, not Task.Run." },

        new() { Id = 151, TopicId = 3, Difficulty = DifficultyLevel.Lead, Type = QuestionType.CodeSnippet,
            Text = "What does this code demonstrate?",
            CodeSnippet = "using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));\n\ntry\n{\n    await LongRunningOperationAsync(cts.Token);\n}\ncatch (OperationCanceledException)\n{\n    Console.WriteLine(\"Operation timed out\");\n}",
            Options = [
                "Error handling for network failures",
                "Implementing a timeout by cancelling after 5 seconds",
                "A retry loop for failed operations",
                "Deadlock prevention"
            ],
            CorrectAnswer = "Implementing a timeout by cancelling after 5 seconds",
            Explanation = "CancellationTokenSource(TimeSpan) creates a source that automatically cancels after the specified duration. The token is passed to the operation, which should check it periodically. After 5 seconds, cancellation is requested and OperationCanceledException is thrown at the next ThrowIfCancellationRequested() or awaited I/O operation." },

        new() { Id = 152, TopicId = 3, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the purpose of CancellationTokenSource.CreateLinkedTokenSource()?",
            Options = [
                "To share a cancellation token across threads",
                "To create a token that is cancelled when any of the linked source tokens is cancelled",
                "To chain cancellation tokens sequentially",
                "To combine two tokens into a single token that requires both to cancel"
            ],
            CorrectAnswer = "To create a token that is cancelled when any of the linked source tokens is cancelled",
            Explanation = "CreateLinkedTokenSource(token1, token2) creates a new CancellationTokenSource that is cancelled when either input token is cancelled. This is used to combine a user-requested cancellation token with an internal timeout token, so the operation cancels on whichever occurs first.",
            ExampleCode = "var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));\nusing var linked = CancellationTokenSource\n    .CreateLinkedTokenSource(userToken, timeoutCts.Token);\nawait DoWorkAsync(linked.Token); // cancels on user request OR timeout" },

        new() { Id = 153, TopicId = 3, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What does Task.WhenAny() return?",
            Options = [
                "The result of the first completed task",
                "A Task<Task> that completes when the first input task completes",
                "A Task that completes when all tasks finish",
                "A cancelled Task if any input is cancelled"
            ],
            CorrectAnswer = "A Task<Task> that completes when the first input task completes",
            Explanation = "Task.WhenAny(tasks) returns Task<Task> — the outer task completes as soon as the first inner task finishes. You await the outer task to get the completed task, then await that to get the result. The other tasks continue running — WhenAny does not cancel them." },

        new() { Id = 154, TopicId = 3, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What keyword do you use to iterate an async stream in C# 8+?",
            Options = ["foreach", "await foreach", "async foreach", "for await"],
            CorrectAnswer = "await foreach",
            Explanation = "'await foreach' (C# 8) iterates over an IAsyncEnumerable<T>. Each iteration asynchronously waits for the next item. It must be used inside an async method. The loop respects CancellationToken via the WithCancellation() extension method." },

        new() { Id = 155, TopicId = 3, Difficulty = DifficultyLevel.Mid, Type = QuestionType.TrueFalse,
            Text = "In ASP.NET Core, there is no SynchronizationContext, so ConfigureAwait(false) has no practical effect.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "ASP.NET Core removed the per-request SynchronizationContext that existed in classic ASP.NET. Without a SynchronizationContext, continuations already run on ThreadPool threads. ConfigureAwait(false) is still harmless but provides no additional benefit in ASP.NET Core (unlike WinForms/WPF)." },

        new() { Id = 156, TopicId = 3, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "How can you execute multiple independent async operations concurrently and await all results?",
            Options = [
                "await op1(); await op2(); await op3();",
                "Start all tasks first, then await Task.WhenAll(task1, task2, task3)",
                "Use Parallel.ForEach",
                "Use async void for each operation"
            ],
            CorrectAnswer = "Start all tasks first, then await Task.WhenAll(task1, task2, task3)",
            Explanation = "Awaiting sequentially (await op1; await op2) runs them one after another. To run concurrently, start all tasks without awaiting, collect them, then await Task.WhenAll(). The total time is max(individual times) rather than their sum.",
            ExampleCode = "// Sequential: 3 seconds total\nvar r1 = await Op1Async(); // 1s\nvar r2 = await Op2Async(); // 1s\nvar r3 = await Op3Async(); // 1s\n\n// Concurrent: ~1 second total\nvar t1 = Op1Async();\nvar t2 = Op2Async();\nvar t3 = Op3Async();\nvar results = await Task.WhenAll(t1, t2, t3);" },

        new() { Id = 157, TopicId = 3, Difficulty = DifficultyLevel.Lead, Type = QuestionType.TrueFalse,
            Text = "Using 'await' in a catch block is supported in C# 6 and later.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "C# 6 added support for await in catch and finally blocks. Before C# 6, this was not allowed and required awkward workarounds (bool flags outside try/catch). Now you can naturally await logging or cleanup operations inside exception handlers." },

        new() { Id = 158, TopicId = 3, Difficulty = DifficultyLevel.Senior, Type = QuestionType.CodeSnippet,
            Text = "What is the potential problem with this code?",
            CodeSnippet = "var semaphore = new SemaphoreSlim(3);\n\nasync Task ProcessAllAsync(IEnumerable<Item> items)\n{\n    var tasks = items.Select(async item =>\n    {\n        await semaphore.WaitAsync();\n        await ProcessItemAsync(item);\n        semaphore.Release();\n    });\n    await Task.WhenAll(tasks);\n}",
            Options = [
                "SemaphoreSlim doesn't work with async",
                "semaphore.Release() won't be called if ProcessItemAsync throws, causing a semaphore leak",
                "Select cannot be used with async lambdas",
                "No problem with this code"
            ],
            CorrectAnswer = "semaphore.Release() won't be called if ProcessItemAsync throws, causing a semaphore leak",
            Explanation = "If ProcessItemAsync throws, Release() is never called. With many items, the semaphore count drains to zero and all subsequent items hang. Fix: wrap the body in try/finally to guarantee Release() is always called." },

        new() { Id = 159, TopicId = 3, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between Task.FromResult() and Task.Run()?",
            Options = [
                "They are the same",
                "Task.FromResult() creates a pre-completed Task with a value; Task.Run() queues work to the ThreadPool",
                "Task.Run() creates a completed Task; Task.FromResult() queues work",
                "Task.FromResult() is only for error cases"
            ],
            CorrectAnswer = "Task.FromResult() creates a pre-completed Task with a value; Task.Run() queues work to the ThreadPool",
            Explanation = "Task.FromResult(value) creates a Task<T> that is already in the Completed state — no thread is used. It is useful for implementing interfaces that return Task when you have a synchronous result. Task.Run() offloads actual work to the ThreadPool." },

        new() { Id = 160, TopicId = 3, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "An async method always runs on a separate background thread.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "async/await is not the same as multithreading. An async method runs on the calling thread until it hits an await. If the awaited operation is already complete, it may never switch threads. Thread switching only happens if the awaited Task was not yet complete, in which case the continuation may resume on a ThreadPool thread." },

        new() { Id = 161, TopicId = 3, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "How do you implement a timeout on an async operation using Task.WhenAny?",
            Options = [
                "await operation.Timeout(5000)",
                "var winner = await Task.WhenAny(operationTask, Task.Delay(timeout)); then check which task won",
                "Use CancellationToken only",
                "Call task.Wait(timeout)"
            ],
            CorrectAnswer = "var winner = await Task.WhenAny(operationTask, Task.Delay(timeout)); then check which task won",
            Explanation = "Race the operation against Task.Delay(timeout). If the delay wins, the operation timed out. Check: if (winner == delayTask) handle timeout. Note: the original operation continues running — you should also cancel it via CancellationToken for clean resource release.",
            ExampleCode = "var opTask = DoWorkAsync(cts.Token);\nvar delay = Task.Delay(5000);\nif (await Task.WhenAny(opTask, delay) == delay)\n    throw new TimeoutException();\nawait opTask; // re-await to propagate exceptions" },

        new() { Id = 162, TopicId = 3, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the purpose of ConfigureAwait(true)?",
            Options = [
                "It enables debug mode for async methods",
                "It is the default behaviour — resume on the captured SynchronizationContext",
                "It forces execution on the UI thread always",
                "It configures the task to not throw exceptions"
            ],
            CorrectAnswer = "It is the default behaviour — resume on the captured SynchronizationContext",
            Explanation = "ConfigureAwait(true) is the default and behaves identically to a plain await. It captures the current SynchronizationContext and posts the continuation back to it. Explicitly writing ConfigureAwait(true) is redundant but occasionally used for documentation clarity." },

        new() { Id = 163, TopicId = 3, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the 'async over sync' antipattern?",
            Options = [
                "Marking a synchronous method as async without adding await",
                "Wrapping blocking synchronous code in Task.Run to expose it as async, giving false impressions of scalability",
                "Using async void in library code",
                "Calling async methods from synchronous code using .Result"
            ],
            CorrectAnswer = "Wrapping blocking synchronous code in Task.Run to expose it as async, giving false impressions of scalability",
            Explanation = "'Async over sync' wraps blocking code in Task.Run to present an async API. It does not add scalability — it just offloads the block to a ThreadPool thread, burning a thread for the duration. Library authors should offer truly async implementations. Consumers can wrap sync APIs in Task.Run themselves if needed." },

        new() { Id = 164, TopicId = 3, Difficulty = DifficultyLevel.Lead, Type = QuestionType.CodeSnippet,
            Text = "What is printed and why?",
            CodeSnippet = "async Task<int> ComputeAsync()\n{\n    await Task.Yield();\n    return 42;\n}\n\nasync Task Main()\n{\n    var t = ComputeAsync();\n    Console.WriteLine(\"Before await\");\n    int result = await t;\n    Console.WriteLine($\"Result: {result}\");\n}",
            Options = [
                "Result: 42\\nBefore await",
                "Before await\\nResult: 42",
                "Before await only",
                "Compile error"
            ],
            CorrectAnswer = "Before await\\nResult: 42",
            Explanation = "ComputeAsync() starts executing synchronously until Task.Yield(), which forces an asynchronous return. Control returns to Main(), which prints 'Before await'. Later, the continuation resumes and prints 'Result: 42'. Task.Yield() ensures at least one async hop." },

        new() { Id = 165, TopicId = 3, Difficulty = DifficultyLevel.Lead, Type = QuestionType.TrueFalse,
            Text = "Task.WhenAll guarantees that results are returned in the order the tasks were passed in.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "Task.WhenAll(t1, t2, t3) returns a Task<T[]> whose result array matches the order of the input tasks, regardless of which task completed first. This makes it predictable and easy to destructure results." },

        new() { Id = 166, TopicId = 3, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the best way to cancel an IAsyncEnumerable<T> iteration?",
            Options = [
                "throw new OperationCanceledException() inside the loop",
                "Pass a CancellationToken via WithCancellation() on the IAsyncEnumerable and check it inside the generator",
                "Call Task.Cancel() on the enumerator",
                "Use a timeout on the foreach loop"
            ],
            CorrectAnswer = "Pass a CancellationToken via WithCancellation() on the IAsyncEnumerable and check it inside the generator",
            Explanation = "Use 'await foreach (var item in source.WithCancellation(ct))' on the consumer side. On the producer side, accept [EnumeratorCancellation] CancellationToken ct as a parameter and check it. WithCancellation passes the token to the underlying IAsyncEnumerator.MoveNextAsync()." },

        new() { Id = 167, TopicId = 3, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the 'sync over async' antipattern and why is it dangerous?",
            Options = [
                "Writing async methods without proper cancellation support",
                "Blocking on async operations with .Result or .Wait() from a synchronization context, risking deadlock",
                "Using async void in non-event-handler contexts",
                "Calling ConfigureAwait(false) in application code"
            ],
            CorrectAnswer = "Blocking on async operations with .Result or .Wait() from a synchronization context, risking deadlock",
            Explanation = "Calling .Result or .Wait() blocks the calling thread. If that thread is the UI thread or ASP.NET request thread (with a SynchronizationContext), the async continuation needs that thread to complete — resulting in deadlock. Fix: make the entire call chain async, or use ConfigureAwait(false) throughout the called async code." },

        new() { Id = 168, TopicId = 3, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What type does an async method returning Task<int> produce when awaited?",
            Options = ["Task", "Task<int>", "int", "object"],
            CorrectAnswer = "int",
            Explanation = "When you await a Task<T>, the result type is T. So 'int result = await SomeAsync()' where SomeAsync returns Task<int> gives an int. The Task wrapper is transparent to the caller thanks to the await operator." },

        new() { Id = 169, TopicId = 3, Difficulty = DifficultyLevel.Mid, Type = QuestionType.TrueFalse,
            Text = "You can use CancellationToken.None when you do not want cancellation support.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "CancellationToken.None is a convenience token that will never be cancelled. It is safe to pass to APIs that accept CancellationToken when you do not need cancellation, avoiding null checks. CancellationToken.None.CanBeCanceled returns false." },

        new() { Id = 170, TopicId = 3, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "What will this code print?",
            CodeSnippet = "async Task<string> GetAsync()\n{\n    return await Task.FromResult(\"hello\");\n}\n\nConsole.WriteLine(await GetAsync());",
            Options = ["Task<string>", "hello", "System.Threading.Tasks.Task`1[System.String]", "Compile error"],
            CorrectAnswer = "hello",
            Explanation = "Task.FromResult(\"hello\") creates an already-completed Task<string>. Awaiting it returns the string \"hello\". GetAsync returns this value via the async state machine. The outer await gives the string, which is printed." },

        new() { Id = 171, TopicId = 3, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is TaskCompletionSource<T> used for?",
            Options = [
                "Creating tasks from synchronous delegates",
                "Manually controlling the lifecycle (completion, failure, cancellation) of a Task<T> that wraps non-Task-based async patterns",
                "Limiting concurrent task execution",
                "Providing a result type for void async methods"
            ],
            CorrectAnswer = "Manually controlling the lifecycle (completion, failure, cancellation) of a Task<T> that wraps non-Task-based async patterns",
            Explanation = "TaskCompletionSource<T> creates a Task<T> whose state you control: SetResult(), SetException(), SetCanceled(). It is used to bridge event-based or callback-based APIs into the Task model, or to implement custom async primitives.",
            ExampleCode = "var tcs = new TaskCompletionSource<string>();\nbutton.Click += (s, e) => tcs.SetResult(\"clicked\");\nstring result = await tcs.Task; // waits until button is clicked" },

        new() { Id = 172, TopicId = 3, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is Channel<T> in .NET and how does it relate to async programming?",
            Options = [
                "A network communication primitive",
                "A thread-safe, async-friendly producer-consumer pipe for passing data between async workflows",
                "An alternative to Task for CPU-bound work",
                "A type of CancellationToken"
            ],
            CorrectAnswer = "A thread-safe, async-friendly producer-consumer pipe for passing data between async workflows",
            Explanation = "System.Threading.Channels.Channel<T> provides bounded or unbounded queues with async-friendly WriteAsync/ReadAsync. It is the recommended way to implement producer-consumer pipelines in modern .NET, replacing BlockingCollection for async scenarios. Channels integrate naturally with IAsyncEnumerable via ReadAllAsync()." },

        new() { Id = 173, TopicId = 3, Difficulty = DifficultyLevel.Lead, Type = QuestionType.TrueFalse,
            Text = "Parallel.ForEachAsync (introduced in .NET 6) supports async delegates with a configurable degree of parallelism.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "Parallel.ForEachAsync<T>(source, options, async (item, ct) => { ... }) processes items from an IEnumerable or IAsyncEnumerable with true async/await support and configurable MaxDegreeOfParallelism. Unlike Parallel.ForEach, it does not block threads — it properly uses async I/O throughout." },

        new() { Id = 174, TopicId = 3, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What happens if you forget to await a Task returned by an async method?",
            Options = [
                "The method is not called at all",
                "The task runs fire-and-forget — exceptions are silently swallowed and the caller doesn't know when it completes",
                "A compile error is produced",
                "The runtime awaits it automatically"
            ],
            CorrectAnswer = "The task runs fire-and-forget — exceptions are silently swallowed and the caller doesn't know when it completes",
            Explanation = "Forgetting to await a Task means the caller does not observe the result or exceptions. Exceptions are captured in the Task but never rethrown. The operation still runs. This is a common bug. CS4014 compiler warning ('this call is not awaited') flags this. If fire-and-forget is intentional, assign to _ or call .ContinueWith to handle exceptions." },

        new() { Id = 175, TopicId = 3, Difficulty = DifficultyLevel.Lead, Type = QuestionType.CodeSnippet,
            Text = "What is the output and what pattern does this demonstrate?",
            CodeSnippet = "async Task<int> WithRetryAsync(Func<Task<int>> operation, int maxAttempts)\n{\n    for (int i = 1; i <= maxAttempts; i++)\n    {\n        try { return await operation(); }\n        catch when (i < maxAttempts)\n        {\n            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));\n        }\n    }\n    throw new Exception(\"All retries exhausted\");\n}",
            Options = [
                "Circuit breaker pattern",
                "Async retry with exponential backoff",
                "Timeout pattern",
                "Bulkhead isolation pattern"
            ],
            CorrectAnswer = "Async retry with exponential backoff",
            Explanation = "This implements async retry with exponential backoff. On failure (except the last attempt), it waits 2^attempt seconds (2s, 4s, 8s...) before retrying. The 'catch when' filter re-throws on the final attempt. Exponential backoff prevents thundering herd on transient failures." },

        // ── Design Patterns additional (TopicId = 4) IDs 176–219 ─────────────
        new() { Id = 176, TopicId = 4, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What problem does the Builder pattern solve?",
            Options = [
                "Creating families of related objects",
                "Constructing complex objects step by step, separating construction from representation",
                "Notifying observers of state changes",
                "Routing requests through a chain of handlers"
            ],
            CorrectAnswer = "Constructing complex objects step by step, separating construction from representation",
            Explanation = "The Builder pattern separates the construction of a complex object from its final representation. It is useful when an object has many optional parts or configuration. Fluent builders (method chaining returning 'this') are a common C# idiom, seen in StringBuilder, HttpClient, and ASP.NET Core's WebApplicationBuilder." },

        new() { Id = 177, TopicId = 4, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What is the Adapter pattern?",
            Options = [
                "Adds responsibilities to an object without changing its class",
                "Converts the interface of one class into another interface that clients expect, enabling incompatible interfaces to work together",
                "Restricts access to a resource",
                "Manages object creation centrally"
            ],
            CorrectAnswer = "Converts the interface of one class into another interface that clients expect, enabling incompatible interfaces to work together",
            Explanation = "The Adapter wraps an existing class to match a required interface, bridging incompatible APIs. Example: wrapping a legacy XML service behind an IDataService interface so new code can use it without modification. The Stream-to-TextReader adapters in .NET are a real example." },

        new() { Id = 178, TopicId = 4, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "The Singleton pattern is always the best choice when you need a single shared instance.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "Singletons introduce global state, make testing harder (they can't be easily mocked), and create hidden dependencies. The preferred alternative in modern .NET is Dependency Injection with singleton lifetime — the DI container manages the single instance, and classes declare the dependency explicitly." },

        new() { Id = 179, TopicId = 4, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the Proxy pattern?",
            Options = [
                "A pattern that creates multiple objects from a template",
                "A placeholder or surrogate for another object, controlling access to it",
                "A pattern for building objects incrementally",
                "A pattern that delegates operations to a chain of handlers"
            ],
            CorrectAnswer = "A placeholder or surrogate for another object, controlling access to it",
            Explanation = "A Proxy implements the same interface as the real subject and intercepts calls to add behaviour: access control, lazy initialisation, logging, caching, or remoting. EF Core navigation properties use lazy-loading proxies; ASP.NET Core's controller action filters are proxy-like." },

        new() { Id = 180, TopicId = 4, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the Command pattern?",
            Options = [
                "A pattern that converts method calls into objects, enabling undo/redo, queuing, and logging",
                "A pattern for routing messages to the appropriate handler",
                "A pattern for translating one interface to another",
                "A pattern for creating related object families"
            ],
            CorrectAnswer = "A pattern that converts method calls into objects, enabling undo/redo, queuing, and logging",
            Explanation = "The Command pattern encapsulates a request as an object (ICommand with Execute/Undo). This decouples the sender from the receiver, enables undo/redo stacks, macro recording, and transactional behaviour. WPF's ICommand and MediatR's IRequest are implementations of this pattern." },

        new() { Id = 181, TopicId = 4, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "Which pattern does this implement?",
            CodeSnippet = "public interface ICommand { void Execute(); void Undo(); }\n\npublic class MoveCommand : ICommand\n{\n    private readonly Shape _shape;\n    private readonly Point _delta;\n\n    public MoveCommand(Shape shape, Point delta)\n    { _shape = shape; _delta = delta; }\n\n    public void Execute() => _shape.Move(_delta);\n    public void Undo()    => _shape.Move(-_delta);\n}",
            Options = ["Strategy", "Command", "Observer", "Memento"],
            CorrectAnswer = "Command",
            Explanation = "MoveCommand encapsulates a move operation as an object with Execute/Undo. This is the Command pattern — the request is objectified, enabling undo/redo by keeping a stack of ICommand objects and calling Undo() in reverse order." },

        new() { Id = 182, TopicId = 4, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the Chain of Responsibility pattern?",
            Options = [
                "Links requests through a sequence of handlers; each handler decides whether to handle or pass on",
                "Defines a family of interchangeable algorithms",
                "Distributes workload across a pool of handlers",
                "Records the history of method calls"
            ],
            CorrectAnswer = "Links requests through a sequence of handlers; each handler decides whether to handle or pass on",
            Explanation = "Each handler in the chain has a reference to the next. It either handles the request or forwards it. ASP.NET Core middleware pipeline is a textbook Chain of Responsibility — each middleware can short-circuit or call next(). Logging level filters and exception handling pipelines use the same pattern." },

        new() { Id = 183, TopicId = 4, Difficulty = DifficultyLevel.Mid, Type = QuestionType.TrueFalse,
            Text = "The Repository pattern abstracts data access logic behind a collection-like interface.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "A Repository provides a collection-like abstraction (Add, Remove, Find, GetAll) for persisting domain objects, hiding the underlying data access technology (EF Core, Dapper, REST). This decouples business logic from persistence details and enables easy substitution in tests." },

        new() { Id = 184, TopicId = 4, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the Mediator pattern and how does MediatR implement it?",
            Options = [
                "Defines a central object that encapsulates how objects interact, decoupling components",
                "Creates objects without specifying their class",
                "Adds behaviour to an object dynamically",
                "Manages the lifecycle of object groups"
            ],
            CorrectAnswer = "Defines a central object that encapsulates how objects interact, decoupling components",
            Explanation = "The Mediator centralises communication so components don't reference each other directly. MediatR implements this: handlers register for request/notification types; senders publish through IMediator without knowing which handler processes it. This reduces coupling in CQRS architectures." },

        new() { Id = 185, TopicId = 4, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the Unit of Work pattern?",
            Options = [
                "Ensures exactly one database connection is used per request",
                "Tracks changes to objects during a business transaction and coordinates writing all changes as a single atomic operation",
                "Limits the number of simultaneous database operations",
                "Caches all database reads for the lifetime of the application"
            ],
            CorrectAnswer = "Tracks changes to objects during a business transaction and coordinates writing all changes as a single atomic operation",
            Explanation = "Unit of Work batches multiple repository operations into one database transaction. EF Core's DbContext IS a Unit of Work — it tracks all changes and commits them atomically via SaveChanges(). Combining Repository + UoW provides a testable, transaction-aware data access layer." },

        new() { Id = 186, TopicId = 4, Difficulty = DifficultyLevel.Senior, Type = QuestionType.CodeSnippet,
            Text = "What pattern does this code implement?",
            CodeSnippet = "public interface ISortStrategy { void Sort(List<int> data); }\n\npublic class QuickSort : ISortStrategy\n    { public void Sort(List<int> d) => d.Sort(); }\n\npublic class BubbleSort : ISortStrategy\n    { public void Sort(List<int> d) { /* bubble sort */ } }\n\npublic class Sorter\n{\n    private ISortStrategy _strategy;\n    public Sorter(ISortStrategy s) => _strategy = s;\n    public void SetStrategy(ISortStrategy s) => _strategy = s;\n    public void Sort(List<int> d) => _strategy.Sort(d);\n}",
            Options = ["Template Method", "Decorator", "Strategy", "Factory Method"],
            CorrectAnswer = "Strategy",
            Explanation = "Sorter delegates sorting to an ISortStrategy that can be swapped at runtime. The algorithm is encapsulated and interchangeable without modifying the Sorter. This is the Strategy pattern — perfect for scenarios where multiple algorithms solve the same problem." },

        new() { Id = 187, TopicId = 4, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is Dependency Injection and which pattern is it related to?",
            Options = [
                "A pattern where classes create their own dependencies",
                "A form of Inversion of Control where dependencies are provided externally rather than created internally",
                "A pattern for lazy-loading dependencies",
                "A way to register types with a central factory"
            ],
            CorrectAnswer = "A form of Inversion of Control where dependencies are provided externally rather than created internally",
            Explanation = "DI is an implementation of Inversion of Control (IoC). Rather than a class calling 'new DbRepository()', the dependency is injected (via constructor, property, or method). This supports Dependency Inversion Principle, enables testability via mocking, and centralises lifetime management in a DI container." },

        new() { Id = 188, TopicId = 4, Difficulty = DifficultyLevel.Senior, Type = QuestionType.TrueFalse,
            Text = "The Decorator pattern and the Proxy pattern are structurally identical but differ in intent.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "Both wrap an object of the same interface and delegate calls. The difference is intent: Decorator adds new behaviour; Proxy controls access. In practice they are structurally indistinguishable — the pattern name indicates the developer's intent." },

        new() { Id = 189, TopicId = 4, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What does the Abstract Factory pattern do?",
            Options = [
                "Creates a single object without specifying the class",
                "Provides an interface for creating families of related or dependent objects without specifying concrete classes",
                "Builds complex objects step by step",
                "Ensures only one instance of a factory exists"
            ],
            CorrectAnswer = "Provides an interface for creating families of related or dependent objects without specifying concrete classes",
            Explanation = "Abstract Factory creates related object families. Example: IUIFactory with CreateButton() and CreateCheckbox() — WindowsUIFactory returns Windows controls, MacUIFactory returns Mac controls. The client uses the factory interface and remains decoupled from platform specifics." },

        new() { Id = 190, TopicId = 4, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the Facade pattern?",
            Options = [
                "Provides a simplified unified interface to a complex subsystem",
                "Adds responsibilities to objects dynamically",
                "Converts interfaces between incompatible systems",
                "Creates objects without specifying concrete classes"
            ],
            CorrectAnswer = "Provides a simplified unified interface to a complex subsystem",
            Explanation = "Facade wraps a complex subsystem with a simple interface, hiding internal complexity. Example: a VideoConverter facade that calls codec, audio, subtitle, and rendering subsystems under a single ConvertVideo() method. It reduces coupling and provides a clean API entry point." },

        new() { Id = 191, TopicId = 4, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "Which pattern does this implement?",
            CodeSnippet = "public class HtmlReport\n{\n    public string Title { get; set; }\n    public string Body  { get; set; }\n    public string Footer { get; set; }\n}\n\npublic class HtmlReportBuilder\n{\n    private readonly HtmlReport _r = new();\n    public HtmlReportBuilder WithTitle(string t)  { _r.Title  = t; return this; }\n    public HtmlReportBuilder WithBody(string b)   { _r.Body   = b; return this; }\n    public HtmlReportBuilder WithFooter(string f) { _r.Footer = f; return this; }\n    public HtmlReport Build() => _r;\n}",
            Options = ["Factory Method", "Prototype", "Builder", "Abstract Factory"],
            CorrectAnswer = "Builder",
            Explanation = "HtmlReportBuilder constructs HtmlReport step by step using a fluent interface (method chaining). Build() returns the final product. This is the Builder pattern — it separates construction from the resulting object and handles optional parts cleanly." },

        new() { Id = 192, TopicId = 4, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the Specification pattern and when is it used?",
            Options = [
                "A pattern for defining the signature of method parameters",
                "A pattern that encapsulates business rules as reusable, composable predicate objects",
                "A pattern for specifying which design pattern to use",
                "A pattern for documenting API contracts"
            ],
            CorrectAnswer = "A pattern that encapsulates business rules as reusable, composable predicate objects",
            Explanation = "Specification encapsulates a business rule as an object with an IsSatisfiedBy(T) method. Specifications can be combined with And/Or/Not. This externalises validation logic from entities and repositories, enabling reusable, testable business rules. Often seen in DDD (Domain-Driven Design).",
            ExampleCode = "interface ISpecification<T> { bool IsSatisfiedBy(T item); }\n\nclass ActiveCustomer : ISpecification<Customer>\n    { public bool IsSatisfiedBy(Customer c) => c.IsActive; }\n\nclass PremiumCustomer : ISpecification<Customer>\n    { public bool IsSatisfiedBy(Customer c) => c.Tier == \"Premium\"; }" },

        new() { Id = 193, TopicId = 4, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between Abstract Factory and Factory Method patterns?",
            Options = [
                "They are identical",
                "Factory Method uses inheritance to defer instantiation; Abstract Factory uses composition to create product families",
                "Abstract Factory creates one product; Factory Method creates families",
                "Factory Method is a class; Abstract Factory is an interface"
            ],
            CorrectAnswer = "Factory Method uses inheritance to defer instantiation; Abstract Factory uses composition to create product families",
            Explanation = "Factory Method defines a method in a base class that subclasses override to create one product. Abstract Factory is an object that creates multiple related products. Abstract Factory often uses multiple Factory Methods internally. Use Factory Method for a single product hierarchy; Abstract Factory for cross-product families." },

        new() { Id = 194, TopicId = 4, Difficulty = DifficultyLevel.Lead, Type = QuestionType.CodeSnippet,
            Text = "What pattern does this code implement?",
            CodeSnippet = "public abstract class Handler\n{\n    protected Handler _next;\n    public Handler SetNext(Handler next) { _next = next; return next; }\n    public abstract void Handle(int request);\n}\n\npublic class LowHandler : Handler\n{\n    public override void Handle(int r)\n    {\n        if (r < 10) Console.WriteLine($\"Low handles {r}\");\n        else _next?.Handle(r);\n    }\n}",
            Options = ["Command", "Mediator", "Chain of Responsibility", "Strategy"],
            CorrectAnswer = "Chain of Responsibility",
            Explanation = "Each Handler has a reference to the next handler. If it can handle the request (r < 10), it does; otherwise it passes it along the chain. This is Chain of Responsibility — classic implementation with SetNext() for building the chain fluently." },

        new() { Id = 195, TopicId = 4, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the Memento pattern?",
            Options = [
                "Records changes to object state for auditing",
                "Captures and externalises an object's internal state so it can be restored later, without violating encapsulation",
                "Provides undo functionality by storing command objects",
                "Creates snapshots of the entire application state"
            ],
            CorrectAnswer = "Captures and externalises an object's internal state so it can be restored later, without violating encapsulation",
            Explanation = "Memento stores a snapshot of an object's state in an opaque object. The originator creates and restores mementos; a caretaker holds them without inspecting contents. This enables undo/redo with full state restoration, not just command reversal. Useful for text editors and game save systems." },

        new() { Id = 196, TopicId = 4, Difficulty = DifficultyLevel.Lead, Type = QuestionType.TrueFalse,
            Text = "In the CQRS pattern, Commands change state and Queries return data, and they should never overlap.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "CQRS (Command Query Responsibility Segregation) separates write operations (Commands — state changes, no return value) from read operations (Queries — return data, no side effects). This allows independent scaling, separate models, and clearer intent. MediatR is commonly used to dispatch both in .NET." },

        new() { Id = 197, TopicId = 4, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "Which .NET class is a real-world example of the Decorator pattern?",
            Options = ["List<T>", "GZipStream wrapping FileStream", "Dictionary<TKey, TValue>", "CancellationToken"],
            CorrectAnswer = "GZipStream wrapping FileStream",
            Explanation = "GZipStream(fileStream, CompressionMode.Compress) is a classic Decorator — it wraps a Stream and adds compression behaviour while preserving the Stream interface. CryptoStream does the same for encryption. You can chain them: new GZipStream(new CryptoStream(fileStream, ...))." },

        new() { Id = 198, TopicId = 4, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "The Composite pattern allows individual objects and compositions of objects to be treated uniformly.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "Composite lets you build tree structures of objects (leaves and composites) that all implement the same interface. Clients treat a single leaf and a branch of the tree identically. File system hierarchies (files and directories both implement IFileSystemItem) are the canonical example." },

        new() { Id = 199, TopicId = 4, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the Flyweight pattern used for?",
            Options = [
                "Making heavy objects lazy-loaded",
                "Sharing fine-grained objects to efficiently support a large number of similar objects",
                "Reducing the number of classes in an application",
                "Compressing data stored in objects"
            ],
            CorrectAnswer = "Sharing fine-grained objects to efficiently support a large number of similar objects",
            Explanation = "Flyweight separates intrinsic (shared, immutable) state from extrinsic (unique, context-specific) state. Shared intrinsic state is stored once in the flyweight; extrinsic state is passed in. String interning in .NET is a Flyweight — identical string values share one heap object." },

        new() { Id = 200, TopicId = 4, Difficulty = DifficultyLevel.Senior, Type = QuestionType.CodeSnippet,
            Text = "What pattern does this code implement, and what is its key benefit?",
            CodeSnippet = "public interface IOrderRepository\n{\n    Task<Order> GetByIdAsync(int id);\n    Task AddAsync(Order order);\n    Task SaveChangesAsync();\n}\n\npublic class EfOrderRepository : IOrderRepository\n{\n    private readonly AppDbContext _ctx;\n    public EfOrderRepository(AppDbContext ctx) => _ctx = ctx;\n    public Task<Order> GetByIdAsync(int id) => _ctx.Orders.FindAsync(id).AsTask();\n    public Task AddAsync(Order o) { _ctx.Orders.Add(o); return Task.CompletedTask; }\n    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();\n}",
            Options = [
                "Singleton pattern — ensures one DbContext instance",
                "Repository pattern — abstracts data access behind an interface, enabling testability",
                "Unit of Work pattern — batches saves",
                "Proxy pattern — controls DbContext access"
            ],
            CorrectAnswer = "Repository pattern — abstracts data access behind an interface, enabling testability",
            Explanation = "IOrderRepository abstracts data access behind a collection-like interface. Tests can inject a mock or in-memory implementation without a real database. Business logic depends on the interface, not the concrete EF Core implementation — following the Dependency Inversion Principle." },

        new() { Id = 201, TopicId = 4, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "When is it appropriate to use the Singleton DI lifetime in ASP.NET Core?",
            Options = [
                "For every service registered in the container",
                "For stateless services or services with expensive initialisation that is safe to share across all requests and threads",
                "For services that hold per-request data",
                "For services that must be created fresh for each controller action"
            ],
            CorrectAnswer = "For stateless services or services with expensive initialisation that is safe to share across all requests and threads",
            Explanation = "Singleton lifetime creates one instance for the application's lifetime. It is appropriate for stateless services (e.g., IHttpClientFactory, configuration, logging) or services with costly startup (e.g., compiled regular expressions). Never use singleton for services with per-request state or those that depend on Scoped services." },

        new() { Id = 202, TopicId = 4, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the Visitor pattern and when should it be used?",
            Options = [
                "An observer that visits all event sources",
                "Adds operations to objects without modifying them by separating the algorithm from the object structure",
                "Iterates over a composite structure",
                "Provides access control to object methods"
            ],
            CorrectAnswer = "Adds operations to objects without modifying them by separating the algorithm from the object structure",
            Explanation = "Visitor lets you add new operations to a class hierarchy without changing those classes — the operation is in a Visitor class; each element calls visitor.Visit(this) (double dispatch). It is useful when you have a stable class hierarchy but frequently add new operations. Pattern matching (switch expressions) in C# is often a cleaner alternative." },

        new() { Id = 203, TopicId = 4, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "Which design pattern is used by the ASP.NET Core middleware pipeline?",
            Options = ["Decorator", "Chain of Responsibility", "Observer", "Strategy"],
            CorrectAnswer = "Chain of Responsibility",
            Explanation = "ASP.NET Core middleware is a Chain of Responsibility. Each middleware component either handles the request or calls the next delegate to pass it along. The pipeline is assembled via app.Use() / app.UseMiddleware<T>(), forming a chain where each link processes the HttpContext." },

        new() { Id = 204, TopicId = 4, Difficulty = DifficultyLevel.Mid, Type = QuestionType.TrueFalse,
            Text = "The Strategy and Template Method patterns both define an algorithm family, but Strategy uses composition and Template Method uses inheritance.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "Template Method defines the algorithm skeleton in a base class using inheritance — subclasses override steps. Strategy encapsulates the entire algorithm in a separate object injected via composition — the context delegates to a strategy. Strategy is generally preferred ('favour composition over inheritance')." },

        new() { Id = 205, TopicId = 4, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the State pattern?",
            Options = [
                "Stores an object's previous state for undo",
                "Allows an object to alter its behaviour when its internal state changes, appearing to change its class",
                "Manages shared mutable state across threads",
                "Serialises an object's state to storage"
            ],
            CorrectAnswer = "Allows an object to alter its behaviour when its internal state changes, appearing to change its class",
            Explanation = "State encapsulates state-specific behaviour in State objects. The context delegates behaviour to the current state, and transitions update which state object is active. This replaces large switch/if-else blocks for state machines. Traffic light and vending machine state machines are classic examples." },

        new() { Id = 206, TopicId = 4, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "Which pattern does this code implement?",
            CodeSnippet = "public class OrderService\n{\n    private readonly IOrderRepository _repo;\n    private readonly IEmailService _email;\n    private readonly IInventoryService _inv;\n\n    public OrderService(\n        IOrderRepository repo,\n        IEmailService email,\n        IInventoryService inv)\n    {\n        _repo  = repo;\n        _email = email;\n        _inv   = inv;\n    }\n}",
            Options = ["Service Locator", "Constructor injection (Dependency Injection)", "Abstract Factory", "Mediator"],
            CorrectAnswer = "Constructor injection (Dependency Injection)",
            Explanation = "OrderService declares its dependencies via its constructor. The DI container resolves and injects IOrderRepository, IEmailService, and IInventoryService. This is constructor injection — the most recommended DI form because dependencies are explicit, required, and the object is always in a valid state after construction." },

        new() { Id = 207, TopicId = 4, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What problem does the Service Locator pattern introduce that Dependency Injection avoids?",
            Options = [
                "Service Locator is faster than DI",
                "Service Locator hides dependencies as a global registry, making them invisible to callers and hard to test",
                "Service Locator does not support interfaces",
                "Service Locator requires all types to be registered at startup"
            ],
            CorrectAnswer = "Service Locator hides dependencies as a global registry, making them invisible to callers and hard to test",
            Explanation = "With Service Locator, classes call ServiceLocator.Get<IFoo>() — dependencies are hidden inside the implementation. Callers cannot see what a class needs. In tests, you must configure the global locator. DI makes dependencies explicit in constructors — they are visible, testable, and replaceable." },

        new() { Id = 208, TopicId = 4, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the Interpreter pattern?",
            Options = [
                "Converts machine code to source code",
                "Defines a grammar for a language and an interpreter that processes sentences in that language",
                "Translates between two object models",
                "Provides runtime type inspection"
            ],
            CorrectAnswer = "Defines a grammar for a language and an interpreter that processes sentences in that language",
            Explanation = "The Interpreter pattern represents grammar rules as classes and recursively evaluates expressions. It is suitable for simple languages (configuration DSLs, regular expressions, mathematical expressions). For complex grammars, parser generators (ANTLR) or libraries (Sprache) are preferred over hand-coded interpreters." },

        new() { Id = 209, TopicId = 4, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "The Prototype pattern creates new objects by cloning an existing object.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "Prototype creates objects by copying a prototype instance rather than calling a constructor. C# supports this via ICloneable (though its shallow vs. deep clone ambiguity is a drawback) or via copy constructors. Records with 'with' expressions provide a clean, modern prototype-like mechanism." },

        new() { Id = 210, TopicId = 4, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the Event Aggregator pattern and how does it differ from the Observer?",
            Options = [
                "They are identical",
                "Event Aggregator is a central hub that routes events between publishers and subscribers, decoupling them completely",
                "Observer is centralised; Event Aggregator is decentralised",
                "Event Aggregator only works for UI events"
            ],
            CorrectAnswer = "Event Aggregator is a central hub that routes events between publishers and subscribers, decoupling them completely",
            Explanation = "In Observer, the subject directly holds subscriber references — they are coupled. Event Aggregator introduces a mediating hub: publishers publish to it, subscribers register with it, and they never reference each other. MediatR's INotification pipeline and IEventBus implementations are Event Aggregators." },

        new() { Id = 211, TopicId = 4, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the purpose of the Null Object pattern?",
            Options = [
                "Handling null values in collections",
                "Providing a default object with do-nothing behaviour to avoid null reference checks",
                "Serialising null fields to JSON",
                "Preventing null assignment to properties"
            ],
            CorrectAnswer = "Providing a default object with do-nothing behaviour to avoid null reference checks",
            Explanation = "Instead of returning null and forcing callers to null-check, return a Null Object that implements the interface with empty/no-op behaviour. Example: NullLogger<T> in .NET implements ILogger<T> but does nothing. Eliminates repetitive null guards and reduces NullReferenceException risk." },

        new() { Id = 212, TopicId = 4, Difficulty = DifficultyLevel.Lead, Type = QuestionType.CodeSnippet,
            Text = "What pattern does this code implement?",
            CodeSnippet = "public interface INotification { }\n\npublic class OrderPlaced : INotification\n    { public int OrderId { get; init; } }\n\npublic class OrderPlacedHandler : INotificationHandler<OrderPlaced>\n{\n    public Task Handle(OrderPlaced n, CancellationToken ct)\n    {\n        Console.WriteLine($\"Order {n.OrderId} placed\");\n        return Task.CompletedTask;\n    }\n}",
            Options = ["Observer pattern via MediatR", "Command pattern", "Chain of Responsibility", "Mediator — publish/subscribe variant"],
            CorrectAnswer = "Mediator — publish/subscribe variant",
            Explanation = "MediatR's INotification/INotificationHandler implements the Mediator pattern in its publish/subscribe form (also resembling Observer). The publisher sends to IMediator without knowing handlers. Multiple handlers can process the same notification. This is the Event Aggregator variant of Mediator." },

        new() { Id = 213, TopicId = 4, Difficulty = DifficultyLevel.Senior, Type = QuestionType.TrueFalse,
            Text = "In the Decorator pattern, the decorator must implement the same interface as the component it wraps.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "The Decorator implements the same interface (or inherits from the same base) as the wrapped component, making it transparent to callers. This allows stacking decorators arbitrarily. If the decorator had a different interface, callers would need to know about it, breaking transparency." },

        new() { Id = 214, TopicId = 4, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is a thread-safe Singleton implementation in C# using Lazy<T>?",
            Options = [
                "private static Singleton _instance = new Singleton();",
                "private static readonly Lazy<Singleton> _instance = new(() => new Singleton()); public static Singleton Instance => _instance.Value;",
                "public static Singleton Instance { get; } = new Singleton();",
                "private static Singleton _instance; public static Singleton Instance => _instance ??= new Singleton();"
            ],
            CorrectAnswer = "private static readonly Lazy<Singleton> _instance = new(() => new Singleton()); public static Singleton Instance => _instance.Value;",
            Explanation = "Lazy<T> with the default LazyThreadSafetyMode.ExecutionAndPublication guarantees thread-safe, lazy initialisation. The instance is created on first access to .Value. This is the cleanest modern singleton implementation — simpler than double-checked locking." },

        new() { Id = 215, TopicId = 4, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between Scoped and Transient lifetimes in ASP.NET Core DI?",
            Options = [
                "Transient is created per request; Scoped is created per method call",
                "Scoped creates one instance per request scope; Transient creates a new instance every time it is requested",
                "They are the same",
                "Scoped persists for the application lifetime; Transient persists for the request"
            ],
            CorrectAnswer = "Scoped creates one instance per request scope; Transient creates a new instance every time it is requested",
            Explanation = "Scoped: one instance per HTTP request — all services within the same request share it (e.g., DbContext). Transient: a new instance every injection — suitable for lightweight stateless services. Singleton: one instance for the app lifetime. Injecting Scoped into Singleton is a 'captive dependency' bug." },

        new() { Id = 216, TopicId = 4, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What is the Iterator pattern?",
            Options = [
                "Converts one object type to another",
                "Provides a way to sequentially access elements of a collection without exposing its underlying representation",
                "Creates objects in a sequence",
                "Manages the order of method execution"
            ],
            CorrectAnswer = "Provides a way to sequentially access elements of a collection without exposing its underlying representation",
            Explanation = "The Iterator pattern separates traversal logic from the collection. In C#, IEnumerable<T> and IEnumerator<T> are the built-in Iterator pattern. foreach works with any type that implements GetEnumerator(). Custom iterators using yield return are a concise C# implementation." },

        new() { Id = 217, TopicId = 4, Difficulty = DifficultyLevel.Mid, Type = QuestionType.TrueFalse,
            Text = "The Repository pattern, when used with EF Core, always requires wrapping DbContext in a custom repository class.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "DbContext itself is a Unit of Work and DbSet<T> acts as a Repository. Whether to add an additional repository layer is an architectural decision. Many teams use DbContext directly in simpler applications. Adding a repository layer is valuable for testability and when you need to switch data sources." },

        new() { Id = 218, TopicId = 4, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the Bridge pattern and how does it differ from Adapter?",
            Options = [
                "They are identical patterns",
                "Bridge separates abstraction from implementation to allow them to vary independently; Adapter makes incompatible interfaces work together after the fact",
                "Adapter varies independently; Bridge adapts existing code",
                "Bridge is a structural pattern; Adapter is a behavioural pattern"
            ],
            CorrectAnswer = "Bridge separates abstraction from implementation to allow them to vary independently; Adapter makes incompatible interfaces work together after the fact",
            Explanation = "Bridge is designed upfront to decouple abstraction from implementation — both can evolve independently without combinatorial subclass explosion. Adapter is a retrofit solution for making existing incompatible classes work together. Bridge is proactive design; Adapter is reactive." },

        new() { Id = 219, TopicId = 4, Difficulty = DifficultyLevel.Lead, Type = QuestionType.CodeSnippet,
            Text = "What is the problem with this DI registration?",
            CodeSnippet = "// In Program.cs:\nbuilder.Services.AddSingleton<IReportService, ReportService>();\n\n// ReportService constructor:\npublic ReportService(AppDbContext ctx) // AppDbContext is Scoped\n{\n    _ctx = ctx;\n}",
            Options = [
                "Singleton services cannot have constructor parameters",
                "Captive dependency: Singleton ReportService captures a Scoped DbContext, reusing it across requests and causing data corruption",
                "AppDbContext must be registered as Singleton too",
                "No problem — DI handles lifetime mismatches automatically"
            ],
            CorrectAnswer = "Captive dependency: Singleton ReportService captures a Scoped DbContext, reusing it across requests and causing data corruption",
            Explanation = "A Singleton that captures a Scoped dependency keeps the Scoped instance alive for the application's lifetime, effectively turning it into a Singleton. DbContext is not thread-safe and is not designed for reuse across requests — this will cause data corruption and concurrency bugs. Fix: inject IServiceScopeFactory and create scopes on demand." },

        // ── Collections additional (TopicId = 5) IDs 220–263 ─────────────────
        new() { Id = 220, TopicId = 5, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What is the default initial capacity of a List<T> in .NET?",
            Options = ["0", "4", "8", "16"],
            CorrectAnswer = "0",
            Explanation = "A new List<T>() starts with an internal array of capacity 0. On the first Add(), it allocates capacity 4. Each time capacity is exceeded, the array doubles. You can avoid repeated reallocations by specifying the expected capacity: new List<T>(expectedCount)." },

        new() { Id = 221, TopicId = 5, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "Dictionary<TKey, TValue> preserves insertion order.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "Dictionary<TKey, TValue> does not guarantee any particular ordering of keys. In practice, CPython-style insertion-order behaviour has been observed in some .NET versions but is not guaranteed by the specification. Use SortedDictionary for sorted order or a List of KeyValuePair for insertion-order preservation." },

        new() { Id = 222, TopicId = 5, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What does List<T>.RemoveAt(0) do?",
            Options = [
                "Removes the last element",
                "Removes the element at index 0 and shifts all remaining elements left",
                "Sets the first element to its default value",
                "Throws IndexOutOfRangeException"
            ],
            CorrectAnswer = "Removes the element at index 0 and shifts all remaining elements left",
            Explanation = "RemoveAt(index) removes the element at the given index and shifts all subsequent elements one position left — O(n) operation. For frequent removals from the front, consider Queue<T> or LinkedList<T> which do it in O(1)." },

        new() { Id = 223, TopicId = 5, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "HashSet<T> allows duplicate elements.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "HashSet<T> is a set — it enforces uniqueness. Add() returns false if the element already exists and does not add a duplicate. This makes it efficient for deduplication and membership testing (O(1) average Contains)." },

        new() { Id = 224, TopicId = 5, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the time complexity of adding an element to the end of a List<T> when the list is at capacity?",
            Options = ["O(1)", "O(log n)", "O(n)", "O(n²)"],
            CorrectAnswer = "O(n)",
            Explanation = "When List<T> runs out of capacity, it allocates a new array (double the size), copies all existing elements — O(n). This is amortised O(1) over many insertions, but any single insertion that triggers a resize is O(n). Pre-allocating with new List<T>(capacity) avoids this." },

        new() { Id = 225, TopicId = 5, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between SortedDictionary<TK, TV> and SortedList<TK, TV>?",
            Options = [
                "SortedList is faster for all operations",
                "SortedDictionary uses a tree (O(log n) insert/delete); SortedList uses an array (O(n) insert but less memory and faster enumeration)",
                "They are identical",
                "SortedList allows duplicate keys"
            ],
            CorrectAnswer = "SortedDictionary uses a tree (O(log n) insert/delete); SortedList uses an array (O(n) insert but less memory and faster enumeration)",
            Explanation = "SortedDictionary: red-black tree, O(log n) for all operations, more memory. SortedList: two parallel arrays, O(n) for insertions (shifting), but uses less memory and is faster to enumerate because of array cache locality. Use SortedList when the collection is built once and iterated often." },

        new() { Id = 226, TopicId = 5, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is LinkedList<T> and when should you prefer it over List<T>?",
            Options = [
                "A doubly-linked list; prefer when frequent insertions/deletions in the middle are required",
                "A singly-linked list; prefer when memory is constrained",
                "A circular list; prefer for FIFO operations",
                "A sorted list; prefer when ordering is important"
            ],
            CorrectAnswer = "A doubly-linked list; prefer when frequent insertions/deletions in the middle are required",
            Explanation = "LinkedList<T> allows O(1) insertions and deletions at any known node position (given a LinkedListNode<T> reference). However, it has poor cache locality, no O(1) indexed access, and higher memory overhead per node. Use List<T> for most scenarios; LinkedList<T> only when you frequently insert/remove in the middle." },

        new() { Id = 227, TopicId = 5, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "What is the output?",
            CodeSnippet = "var queue = new Queue<int>();\nqueue.Enqueue(1);\nqueue.Enqueue(2);\nqueue.Enqueue(3);\nConsole.WriteLine(queue.Dequeue());\nConsole.WriteLine(queue.Peek());\nConsole.WriteLine(queue.Count);",
            Options = ["1, 2, 2", "1, 2, 3", "3, 2, 2", "1, 1, 3"],
            CorrectAnswer = "1, 2, 2",
            Explanation = "Queue is FIFO. Enqueue adds 1, 2, 3. Dequeue() removes and returns the first element: 1. Peek() returns the next element without removing it: 2. Count is now 2 (elements 2 and 3 remain)." },

        new() { Id = 228, TopicId = 5, Difficulty = DifficultyLevel.Mid, Type = QuestionType.TrueFalse,
            Text = "ConcurrentBag<T> maintains insertion order.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "ConcurrentBag<T> is an unordered, thread-safe collection optimised for scenarios where the same thread both adds and removes items (work-stealing). It provides no ordering guarantee. For ordered concurrent access, use ConcurrentQueue<T> (FIFO) or ConcurrentStack<T> (LIFO)." },

        new() { Id = 229, TopicId = 5, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What does ImmutableList<T> provide that List<T> does not?",
            Options = [
                "Faster read access",
                "Thread-safe mutations — operations return new instances without modifying the original",
                "Automatic sorting of elements",
                "Fixed size allocation"
            ],
            CorrectAnswer = "Thread-safe mutations — operations return new instances without modifying the original",
            Explanation = "ImmutableList<T> (System.Collections.Immutable) never changes after creation. All mutating operations (Add, Remove) return a new ImmutableList<T> with the modification. The original remains unchanged. This makes it safe to share across threads without locks, at the cost of more allocations." },

        new() { Id = 230, TopicId = 5, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is Span<T> and how does it relate to collections?",
            Options = [
                "A thread-safe collection wrapper",
                "A stack-allocated, ref struct view over a contiguous region of memory (array, stack, or unmanaged) with no heap allocation",
                "A lazy wrapper around IEnumerable<T>",
                "A fixed-size array type"
            ],
            CorrectAnswer = "A stack-allocated, ref struct view over a contiguous region of memory (array, stack, or unmanaged) with no heap allocation",
            Explanation = "Span<T> is a ref struct that represents a contiguous memory region. It enables slicing arrays, strings, and stackalloc buffers without copying. Because it is a ref struct, it cannot be stored on the heap. Use it for high-performance processing. ReadOnlySpan<T> is the immutable counterpart.",
            ExampleCode = "int[] arr = { 1, 2, 3, 4, 5 };\nSpan<int> slice = arr.AsSpan(1, 3); // [2, 3, 4] — no copy\nslice[0] = 99; // modifies original array\n// arr is now [1, 99, 3, 4, 5]" },

        new() { Id = 231, TopicId = 5, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What does yield return do in a custom iterator?",
            Options = [
                "Returns the entire collection at once",
                "Suspends the method and returns one element at a time, resuming execution on the next MoveNext() call",
                "Marks the method as asynchronous",
                "Returns a Task with the value"
            ],
            CorrectAnswer = "Suspends the method and returns one element at a time, resuming execution on the next MoveNext() call",
            Explanation = "yield return generates an iterator block — the compiler creates a state machine that implements IEnumerable<T>/IEnumerator<T>. Each yield return pauses execution and returns the value. On the next iteration, execution resumes from the statement after yield return. This enables lazy, memory-efficient sequence generation.",
            ExampleCode = "IEnumerable<int> InfiniteCounter()\n{\n    int i = 0;\n    while (true) yield return i++;\n}\n\nforeach (var n in InfiniteCounter().Take(5))\n    Console.Write(n + \" \"); // 0 1 2 3 4" },

        new() { Id = 232, TopicId = 5, Difficulty = DifficultyLevel.Senior, Type = QuestionType.TrueFalse,
            Text = "ReadOnlyCollection<T> prevents modification of the underlying list it wraps.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "ReadOnlyCollection<T> wraps a List<T> and exposes only read operations — callers of the wrapper cannot modify it. However, if the underlying List<T> is modified through another reference, the ReadOnlyCollection reflects those changes. For a truly immutable snapshot, use ImmutableList<T> or ToList() to copy." },

        new() { Id = 233, TopicId = 5, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is ArraySegment<T>?",
            Options = [
                "A resizable array",
                "A struct that represents a section (offset + count) of an array without copying",
                "An array with thread-safe operations",
                "A fixed-size array allocated on the stack"
            ],
            CorrectAnswer = "A struct that represents a section (offset + count) of an array without copying",
            Explanation = "ArraySegment<T> wraps an existing array with an offset and count, allowing you to pass a sub-section to methods without allocating. It is widely used in socket and I/O APIs. Span<T> is now generally preferred for most use cases as it supports more memory kinds and has a cleaner API." },

        new() { Id = 234, TopicId = 5, Difficulty = DifficultyLevel.Senior, Type = QuestionType.CodeSnippet,
            Text = "What is the output?",
            CodeSnippet = "var stack = new Stack<string>();\nstack.Push(\"a\");\nstack.Push(\"b\");\nstack.Push(\"c\");\nConsole.WriteLine(stack.Pop());\nConsole.WriteLine(stack.Peek());\nConsole.WriteLine(stack.Count);",
            Options = ["a, b, 2", "c, b, 2", "c, c, 2", "a, a, 2"],
            CorrectAnswer = "c, b, 2",
            Explanation = "Stack is LIFO. Push a, b, c. Pop() removes and returns top: 'c'. Peek() returns next top without removing: 'b'. Count is now 2 (a and b remain)." },

        new() { Id = 235, TopicId = 5, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What does ConcurrentDictionary.GetOrAdd() do atomically?",
            Options = [
                "Gets an existing value or adds a new one — guarantees only one value is stored per key",
                "Always creates a new entry, replacing the existing one",
                "Locks the entire dictionary for the duration of the operation",
                "Gets and removes a value atomically"
            ],
            CorrectAnswer = "Gets an existing value or adds a new one — guarantees only one value is stored per key",
            Explanation = "GetOrAdd(key, valueFactory) returns the existing value if the key exists, or adds and returns the new value. The factory may be called more than once under contention, but only one result is stored. For factory calls with side effects, use AddOrUpdate or external synchronisation." },

        new() { Id = 236, TopicId = 5, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is PriorityQueue<TElement, TPriority> introduced in .NET 6?",
            Options = [
                "A queue that processes items in insertion order",
                "A min-heap queue that dequeues the element with the smallest priority value first",
                "A thread-safe priority queue",
                "A queue that sorts items by a provided Comparer"
            ],
            CorrectAnswer = "A min-heap queue that dequeues the element with the smallest priority value first",
            Explanation = "PriorityQueue<TElement, TPriority> is a min-heap. Enqueue(element, priority) adds an element; Dequeue() removes the element with the lowest priority value. It does not guarantee FIFO ordering among elements with equal priority. Use a custom IComparer<TPriority> for max-heap or custom ordering.",
            ExampleCode = "var pq = new PriorityQueue<string, int>();\npq.Enqueue(\"low\",  3);\npq.Enqueue(\"high\", 1);\npq.Enqueue(\"mid\",  2);\nConsole.WriteLine(pq.Dequeue()); // \"high\" (priority 1)" },

        new() { Id = 237, TopicId = 5, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between IEnumerable<T> and ICollection<T>?",
            Options = [
                "ICollection is read-only; IEnumerable allows mutation",
                "IEnumerable supports only forward iteration; ICollection adds Count, Add, Remove, and Contains",
                "ICollection is lazy; IEnumerable is eager",
                "They are the same interface"
            ],
            CorrectAnswer = "IEnumerable supports only forward iteration; ICollection adds Count, Add, Remove, and Contains",
            Explanation = "IEnumerable<T> exposes only GetEnumerator() — forward-only, read-only traversal. ICollection<T> extends it with Count, IsReadOnly, Add(), Remove(), Clear(), Contains(). IList<T> further adds indexed access. Accepting the narrowest interface in APIs provides maximum flexibility to callers." },

        new() { Id = 238, TopicId = 5, Difficulty = DifficultyLevel.Lead, Type = QuestionType.TrueFalse,
            Text = "Modifying a collection while iterating it with foreach throws an InvalidOperationException.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "Collections like List<T> and Dictionary<TK, TV> maintain a version counter. When foreach starts, it captures the version. If Add/Remove is called during iteration, the version changes, and MoveNext() throws InvalidOperationException ('Collection was modified'). Fix: iterate a copy (.ToList()) or build a separate change list." },

        new() { Id = 239, TopicId = 5, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the purpose of EqualityComparer<T>.Default in collections?",
            Options = [
                "Provides a null-safe default comparer that uses T's Equals and GetHashCode",
                "Creates a default sorting order for T",
                "Provides the default reference equality comparer",
                "Compares T to its default value"
            ],
            CorrectAnswer = "Provides a null-safe default comparer that uses T's Equals and GetHashCode",
            Explanation = "EqualityComparer<T>.Default selects the best comparer for T: IEquatable<T>.Equals if implemented, otherwise object.Equals. It handles nulls safely. Dictionary and HashSet use it internally. Implementing IEquatable<T> on custom types ensures correct behaviour in hash-based collections." },

        new() { Id = 240, TopicId = 5, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "What is the output?",
            CodeSnippet = "var dict = new Dictionary<string, int>\n    { [\"a\"] = 1, [\"b\"] = 2 };\ndict[\"a\"] = 10;\ndict[\"c\"] = 3;\nConsole.WriteLine(dict[\"a\"] + dict[\"c\"]);",
            Options = ["4", "13", "11", "Throws KeyNotFoundException"],
            CorrectAnswer = "13",
            Explanation = "dict[\"a\"] = 10 updates the value for key 'a'. dict[\"c\"] = 3 adds a new entry. dict[\"a\"] is 10 and dict[\"c\"] is 3; 10 + 3 = 13." },

        new() { Id = 241, TopicId = 5, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between TryGetValue() and the indexer [] on Dictionary?",
            Options = [
                "They are identical",
                "TryGetValue() returns false without throwing when the key is missing; [] throws KeyNotFoundException",
                "[] returns null for missing keys; TryGetValue() throws",
                "TryGetValue() is slower"
            ],
            CorrectAnswer = "TryGetValue() returns false without throwing when the key is missing; [] throws KeyNotFoundException",
            Explanation = "dict[key] throws KeyNotFoundException if the key doesn't exist. TryGetValue(key, out value) returns false and sets value to default(T) — no exception. When missing keys are expected, TryGetValue is preferred for both performance (avoids exception overhead) and clarity." },

        new() { Id = 242, TopicId = 5, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What is the purpose of the Capacity property on List<T>?",
            Options = [
                "The number of elements currently in the list",
                "The number of elements the internal array can hold before resizing",
                "The maximum number of elements the list will ever hold",
                "The initial size of each element"
            ],
            CorrectAnswer = "The number of elements the internal array can hold before resizing",
            Explanation = "Capacity is the size of the internal array; Count is the number of actual elements. When Count == Capacity, the next Add triggers a resize (doubling). Setting Capacity = Count (via TrimExcess()) reduces memory usage when a list has grown large and won't grow further." },

        new() { Id = 243, TopicId = 5, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What does IEnumerator<T>.Reset() do and why is it rarely called?",
            Options = [
                "Restarts iteration from the beginning — commonly used in foreach loops",
                "Resets the enumerator to before the first element; rarely supported by iterators and deprecated in practice",
                "Clears the underlying collection",
                "Disposes the enumerator and creates a new one"
            ],
            CorrectAnswer = "Resets the enumerator to before the first element; rarely supported by iterators and deprecated in practice",
            Explanation = "Reset() is defined on IEnumerator for historical COM interop reasons. Compiler-generated iterators (yield return) throw NotSupportedException for Reset(). foreach never calls Reset(). It is effectively obsolete — if you need to re-iterate, call GetEnumerator() again or materialise to a list." },

        new() { Id = 244, TopicId = 5, Difficulty = DifficultyLevel.Lead, Type = QuestionType.CodeSnippet,
            Text = "What is the performance characteristic of this code?",
            CodeSnippet = "List<int> items = GetMillionItems();\n\nforeach (var target in targets)\n{\n    if (items.Contains(target))\n        Process(target);\n}",
            Options = [
                "O(n) total — efficient",
                "O(n*m) — each Contains() is O(n), repeated m times; use a HashSet<int> for O(1) lookup",
                "O(n log n) — sorted search",
                "O(1) — Dictionary-backed"
            ],
            CorrectAnswer = "O(n*m) — each Contains() is O(n), repeated m times; use a HashSet<int> for O(1) lookup",
            Explanation = "List<T>.Contains() is O(n) — it scans linearly. With m targets and n items, total cost is O(n*m). Converting items to a HashSet<int> first makes each Contains() O(1), reducing total to O(n + m). This is a very common performance issue." },

        new() { Id = 245, TopicId = 5, Difficulty = DifficultyLevel.Mid, Type = QuestionType.TrueFalse,
            Text = "BlockingCollection<T> can be used to implement a producer-consumer pattern in .NET.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "BlockingCollection<T> wraps a concurrent collection and provides blocking Add (blocks producers when full) and Take (blocks consumers when empty) operations. It is ideal for bounded producer-consumer pipelines. For async scenarios, Channel<T> is the modern replacement." },

        new() { Id = 246, TopicId = 5, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between Array and List<T> in C#?",
            Options = [
                "Arrays are reference types; List<T> is a value type",
                "Arrays have fixed size and contiguous memory; List<T> is dynamically resizable with the same O(1) random access",
                "List<T> is always faster",
                "Arrays cannot hold value types"
            ],
            CorrectAnswer = "Arrays have fixed size and contiguous memory; List<T> is dynamically resizable with the same O(1) random access",
            Explanation = "Both arrays and List<T> use contiguous memory and offer O(1) random access by index. Arrays are fixed size; List<T> grows dynamically. Arrays are slightly faster for iteration due to JIT optimisations (no bounds check elision). Use arrays when size is known; List<T> when dynamic sizing is needed." },

        new() { Id = 247, TopicId = 5, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "Which collection should you use to implement a LIFO (Last In, First Out) data structure?",
            Options = ["Queue<T>", "LinkedList<T>", "Stack<T>", "SortedSet<T>"],
            CorrectAnswer = "Stack<T>",
            Explanation = "Stack<T> implements LIFO: Push() adds to the top; Pop() removes from the top; Peek() views the top without removing. Typical uses: undo/redo, expression parsing, depth-first traversal, and call stack simulation." },

        new() { Id = 248, TopicId = 5, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between IList<T> and IReadOnlyList<T>?",
            Options = [
                "IList<T> supports mutations (Add, Remove, indexed set); IReadOnlyList<T> exposes only Count and indexed get",
                "IReadOnlyList<T> is faster",
                "IList<T> allows null elements; IReadOnlyList<T> does not",
                "They are the same interface"
            ],
            CorrectAnswer = "IList<T> supports mutations (Add, Remove, indexed set); IReadOnlyList<T> exposes only Count and indexed get",
            Explanation = "IReadOnlyList<T> provides a read-only view: indexer get and Count. Returning IReadOnlyList<T> from public APIs communicates that callers should not mutate the collection. Covariant (IReadOnlyList<out T> since .NET 4.5), so IReadOnlyList<Dog> is assignable to IReadOnlyList<Animal>." },

        new() { Id = 249, TopicId = 5, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "What does this code print?",
            CodeSnippet = "var set1 = new HashSet<int> { 1, 2, 3, 4 };\nvar set2 = new HashSet<int> { 3, 4, 5, 6 };\nset1.IntersectWith(set2);\nConsole.WriteLine(string.Join(\", \", set1));",
            Options = ["1, 2, 3, 4, 5, 6", "3, 4", "1, 2, 5, 6", "1, 2"],
            CorrectAnswer = "3, 4",
            Explanation = "IntersectWith() modifies set1 in-place to retain only elements also in set2. The intersection of {1,2,3,4} and {3,4,5,6} is {3,4}." },

        new() { Id = 250, TopicId = 5, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What does Dictionary<TK, TV>.ContainsKey() do?",
            Options = [
                "Checks if the dictionary contains a specific value",
                "Checks if a key exists in the dictionary in O(1) average time",
                "Returns the value for the key, or null if not found",
                "Adds the key if it does not exist"
            ],
            CorrectAnswer = "Checks if a key exists in the dictionary in O(1) average time",
            Explanation = "ContainsKey() uses the hash table to check key existence in O(1) amortised time. It is useful before accessing a key to avoid KeyNotFoundException. TryGetValue() combines the existence check and value retrieval in one O(1) call, which is slightly more efficient than ContainsKey() + indexer." },

        new() { Id = 251, TopicId = 5, Difficulty = DifficultyLevel.Senior, Type = QuestionType.TrueFalse,
            Text = "Memory<T> is a ref struct like Span<T> and therefore cannot be stored as a field in a class.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "Span<T> is a ref struct and cannot be stored on the heap. Memory<T> is a regular struct (not ref struct) that can be stored in fields, arrays, and closures. Memory<T>.Span gives a Span<T> view of the underlying data. Memory<T> is for long-lived references; Span<T> is for synchronous, stack-lifetime slices." },

        new() { Id = 252, TopicId = 5, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the purpose of CollectionsMarshal.AsSpan<T>(list)?",
            Options = [
                "Serialises a list to a span",
                "Returns a Span<T> directly over the internal array of a List<T>, enabling zero-copy processing",
                "Creates a copy of the list as a span",
                "Sorts the list using a span-based algorithm"
            ],
            CorrectAnswer = "Returns a Span<T> directly over the internal array of a List<T>, enabling zero-copy processing",
            Explanation = "CollectionsMarshal.AsSpan(list) (System.Runtime.InteropServices) gives a Span<T> directly over the list's internal array without copying. This enables high-performance reads and writes over the list. The span is only valid while the list is not resized — adding elements after calling AsSpan invalidates it." },

        new() { Id = 253, TopicId = 5, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "A Queue<T> in C# uses Enqueue() to add items and Dequeue() to remove them.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "Queue<T> exposes Enqueue(item) to add to the back and Dequeue() to remove from the front (FIFO). Peek() views the front without removing. TryDequeue() is the non-throwing variant. Queue<T> is backed by a circular buffer for amortised O(1) operations." },

        new() { Id = 254, TopicId = 5, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What happens when you call Dictionary<TK, TV>[] with a key that doesn't exist?",
            Options = [
                "Returns null",
                "Returns default(TV)",
                "Throws KeyNotFoundException",
                "Adds the key with default(TV)"
            ],
            CorrectAnswer = "Throws KeyNotFoundException",
            Explanation = "The dictionary indexer getter throws KeyNotFoundException when the key is absent. Use TryGetValue() for safe access, or GetValueOrDefault() (.NET 5+) which returns a default value without throwing. The indexer setter, conversely, adds or updates the key-value pair without throwing." },

        new() { Id = 255, TopicId = 5, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the purpose of HashSet<T>.ExceptWith()?",
            Options = [
                "Returns a new set excluding specified elements",
                "Modifies the HashSet in-place to remove all elements that are also in the specified collection",
                "Clears the HashSet and fills it with the specified collection",
                "Computes the symmetric difference"
            ],
            CorrectAnswer = "Modifies the HashSet in-place to remove all elements that are also in the specified collection",
            Explanation = "ExceptWith(other) performs set difference in-place — removes from 'this' all elements present in 'other'. SymmetricExceptWith() removes elements in both sets (XOR). UnionWith() adds all elements from both. These in-place operations are more efficient than creating new sets." },

        new() { Id = 256, TopicId = 5, Difficulty = DifficultyLevel.Lead, Type = QuestionType.TrueFalse,
            Text = "In .NET, arrays implement IEnumerable<T>, IList<T>, and IReadOnlyList<T>.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "Single-dimensional arrays T[] implement IEnumerable<T>, ICollection<T>, IList<T>, IReadOnlyCollection<T>, and IReadOnlyList<T>. This allows arrays to be passed wherever any of these interfaces is expected. Multi-dimensional arrays implement the non-generic IEnumerable and IList only." },

        new() { Id = 257, TopicId = 5, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "What does this custom iterator method return?",
            CodeSnippet = "IEnumerable<int> EvenNumbers(int max)\n{\n    for (int i = 0; i <= max; i++)\n        if (i % 2 == 0)\n            yield return i;\n}\n\nConsole.WriteLine(string.Join(\", \", EvenNumbers(10)));",
            Options = ["0, 2, 4, 6, 8, 10", "1, 3, 5, 7, 9", "0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10", "2, 4, 6, 8, 10"],
            CorrectAnswer = "0, 2, 4, 6, 8, 10",
            Explanation = "The iterator yields even numbers from 0 to max inclusive (i % 2 == 0 includes 0). For max=10: 0, 2, 4, 6, 8, 10." },

        new() { Id = 258, TopicId = 5, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between ConcurrentDictionary and a Dictionary with a lock?",
            Options = [
                "ConcurrentDictionary uses a single lock for all operations",
                "ConcurrentDictionary uses stripe (bucket-level) locking and lock-free reads for higher throughput; a lock serialises all access",
                "They have identical performance",
                "ConcurrentDictionary only supports string keys"
            ],
            CorrectAnswer = "ConcurrentDictionary uses stripe (bucket-level) locking and lock-free reads for higher throughput; a lock serialises all access",
            Explanation = "ConcurrentDictionary partitions the hash table into segments and locks only the relevant segment during writes, allowing concurrent writes to different segments. Reads are lock-free. A Dictionary + lock blocks all operations globally. ConcurrentDictionary is better for high-concurrency scenarios with many independent keys." },

        new() { Id = 259, TopicId = 5, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the purpose of IComparer<T> vs IComparable<T>?",
            Options = [
                "IComparable<T> is implemented by the type itself to define its natural ordering; IComparer<T> is an external comparer for custom or alternative orderings",
                "They are the same interface",
                "IComparer<T> is for value types only",
                "IComparable<T> is used by collections; IComparer<T> is used by sorting algorithms"
            ],
            CorrectAnswer = "IComparable<T> is implemented by the type itself to define its natural ordering; IComparer<T> is an external comparer for custom or alternative orderings",
            Explanation = "int, string, DateTime implement IComparable<T> for their natural sort order. IComparer<T> is an external strategy for custom ordering — e.g., sort by length then alphabetically. SortedDictionary, SortedSet, and Array.Sort accept IComparer<T>. This follows the Strategy pattern." },

        new() { Id = 260, TopicId = 5, Difficulty = DifficultyLevel.Lead, Type = QuestionType.CodeSnippet,
            Text = "What is the issue with this code?",
            CodeSnippet = "var cache = new Dictionary<string, List<int>>();\n\nvoid AddToCache(string key, int value)\n{\n    if (!cache.ContainsKey(key))\n        cache[key] = new List<int>();\n    cache[key].Add(value);\n}",
            Options = [
                "Dictionary does not support List values",
                "Thread-safety: two threads can both pass ContainsKey check before either sets the key, resulting in a lost Add",
                "ContainsKey is O(n)",
                "No issue"
            ],
            CorrectAnswer = "Thread-safety: two threads can both pass ContainsKey check before either sets the key, resulting in a lost Add",
            Explanation = "This is a classic TOCTOU (time-of-check time-of-use) race. Two threads can both see !ContainsKey return true, then both create a new List — one overwrites the other's entry. Fix with ConcurrentDictionary.GetOrAdd(), or a lock, or the TryGetValue + add pattern under a lock." },

        new() { Id = 261, TopicId = 5, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "How do you efficiently initialise a Dictionary with known key-value pairs in C#?",
            Options = [
                "Use a series of .Add() calls",
                "Use collection initialiser syntax: new Dictionary<string, int> { [\"a\"] = 1, [\"b\"] = 2 }",
                "Dictionaries cannot be initialised with literal syntax",
                "Use Dictionary.Create()"
            ],
            CorrectAnswer = "Use collection initialiser syntax: new Dictionary<string, int> { [\"a\"] = 1, [\"b\"] = 2 }",
            Explanation = "The index initialiser syntax (C# 6) is clean and readable. The older collection initialiser syntax { { \"a\", 1 } } also works. Both compile to a series of Add() calls. For read-only lookups, consider FrozenDictionary<TK, TV> (.NET 8) for maximum lookup performance." },

        new() { Id = 262, TopicId = 5, Difficulty = DifficultyLevel.Mid, Type = QuestionType.TrueFalse,
            Text = "List<T>.Sort() sorts the list in-place using an unstable sort algorithm by default.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "List<T>.Sort() performs an introspective sort (combination of quicksort, heapsort, insertion sort). As of .NET Core, it is a stable sort (preserving relative order of equal elements). Array.Sort() uses the same algorithm. LINQ's OrderBy() has always been stable." },

        new() { Id = 263, TopicId = 5, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is FrozenDictionary<TKey, TValue> introduced in .NET 8?",
            Options = [
                "An immutable dictionary that throws on mutation attempts",
                "A dictionary optimised for read-heavy scenarios — built once with maximum lookup speed, trading slower construction",
                "A thread-safe dictionary variant",
                "A dictionary that serialises its contents to disk"
            ],
            CorrectAnswer = "A dictionary optimised for read-heavy scenarios — built once with maximum lookup speed, trading slower construction",
            Explanation = "FrozenDictionary<TK, TV> (System.Collections.Frozen, .NET 8) is built once via ToFrozenDictionary() and optimised for maximum read throughput — using perfect hashing or specialised layouts. Construction is slow; lookups are as fast as possible. Ideal for static lookup tables (enum-to-string maps, configuration lookups) created once at startup." },

        // ── Memory Management additional (TopicId = 6) IDs 264–307 ───────────
        new() { Id = 264, TopicId = 6, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What is the Garbage Collector (GC) responsible for in .NET?",
            Options = [
                "Preventing stack overflows",
                "Automatically reclaiming memory occupied by objects that are no longer reachable",
                "Managing file and network handles",
                "Compiling code at runtime"
            ],
            CorrectAnswer = "Automatically reclaiming memory occupied by objects that are no longer reachable",
            Explanation = "The .NET GC manages heap memory for managed objects. It determines which objects are no longer reachable from any root (stack variable, static field, GC handle) and reclaims their memory. You do not call free() — but you must still explicitly release unmanaged resources via IDisposable." },

        new() { Id = 265, TopicId = 6, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "Calling GC.Collect() immediately frees all unreachable objects.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "GC.Collect() triggers a collection but does not guarantee that all unreachable objects are immediately freed. Objects with finalizers require an additional GC cycle. GC.Collect() in production code is almost always a mistake — it disrupts GC heuristics and typically hurts performance." },

        new() { Id = 266, TopicId = 6, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What are the three GC generations in .NET?",
            Options = [
                "Gen 0, Gen 1, Gen 2",
                "Gen A, Gen B, Gen C",
                "Short-lived, Medium-lived, Long-lived",
                "Young, Old, Ancient"
            ],
            CorrectAnswer = "Gen 0, Gen 1, Gen 2",
            Explanation = "The .NET GC uses a generational model. Gen 0: newly allocated short-lived objects (collected most frequently, cheapest). Gen 1: objects that survived one Gen 0 collection (buffer). Gen 2: long-lived objects (expensive full GC, rare). Objects promote to higher generations by surviving collections." },

        new() { Id = 267, TopicId = 6, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "Using the 'using' statement guarantees that Dispose() is called even if an exception occurs.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "The 'using' statement compiles to a try/finally block where Dispose() is called in the finally. This ensures deterministic resource release regardless of whether the body throws. The 'using' declaration (C# 8) works the same way at the enclosing scope's end." },

        new() { Id = 268, TopicId = 6, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the full IDisposable pattern (dispose pattern) for a class that directly owns unmanaged resources?",
            Options = [
                "Just implement Dispose() and call GC.Collect()",
                "Implement Dispose(bool disposing) and a finalizer; Dispose() calls Dispose(true) + GC.SuppressFinalize(this)",
                "Only implement a finalizer",
                "Implement IDisposable.Dispose() and set all fields to null"
            ],
            CorrectAnswer = "Implement Dispose(bool disposing) and a finalizer; Dispose() calls Dispose(true) + GC.SuppressFinalize(this)",
            Explanation = "The canonical pattern: Dispose(bool disposing) releases managed resources when 'disposing' is true (called from Dispose()) and always releases unmanaged resources. The finalizer calls Dispose(false) as a safety net. GC.SuppressFinalize(this) removes the finalizer from the finalization queue after explicit Dispose, avoiding a second GC cycle.",
            ExampleCode = "~MyClass() => Dispose(false);\n\npublic void Dispose()\n{\n    Dispose(true);\n    GC.SuppressFinalize(this);\n}\n\nprotected virtual void Dispose(bool disposing)\n{\n    if (_disposed) return;\n    if (disposing) _managedResource?.Dispose();\n    ReleaseUnmanagedHandle();\n    _disposed = true;\n}" },

        new() { Id = 269, TopicId = 6, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is a WeakReference<T> used for?",
            Options = [
                "A reference that prevents GC collection",
                "A reference that allows the GC to collect the object while still letting you check if it is still alive",
                "A null-safe reference type",
                "A reference counted pointer like C++"
            ],
            CorrectAnswer = "A reference that allows the GC to collect the object while still letting you check if it is still alive",
            Explanation = "WeakReference<T> holds a reference that does not keep the object alive. The GC can reclaim it if there are no strong references. TryGetTarget() returns false if collected. Use cases: caches where entries can be evicted by memory pressure, event systems that don't hold subscribers alive." },

        new() { Id = 270, TopicId = 6, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "What is the memory issue in this code?",
            CodeSnippet = "public class ImageProcessor\n{\n    private static readonly List<byte[]> _cache = new();\n\n    public void Process(byte[] imageData)\n    {\n        _cache.Add(imageData); // \"cache\" for reuse\n        // ... process ...\n    }\n}",
            Options = [
                "byte[] cannot be stored in a List",
                "Static List holds strong references to all image arrays indefinitely — memory leak",
                "imageData should be a Span<byte>",
                "No issue — static caches are fine"
            ],
            CorrectAnswer = "Static List holds strong references to all image arrays indefinitely — memory leak",
            Explanation = "The static List<byte[]> grows without bound. Static fields are GC roots — nothing in _cache can ever be collected. Every processed image's data accumulates in memory. Fix: use a bounded cache (ConcurrentDictionary with eviction), WeakReference cache, or MemoryCache." },

        new() { Id = 271, TopicId = 6, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What does GC.SuppressFinalize(this) do?",
            Options = [
                "Prevents the object from ever being garbage collected",
                "Removes the object from the finalization queue so the finalizer is not called",
                "Forces immediate garbage collection",
                "Disables the finalizer for the entire class"
            ],
            CorrectAnswer = "Removes the object from the finalization queue so the finalizer is not called",
            Explanation = "When Dispose() is called explicitly, resources are already released. Calling GC.SuppressFinalize(this) removes the object from the finalizer queue so the GC doesn't call ~ClassName() again. This avoids the overhead of an extra GC cycle and prevents double-release of resources." },

        new() { Id = 272, TopicId = 6, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is stackalloc and when should it be used?",
            Options = [
                "Allocates memory on the stack — zero GC overhead, but limited size and cannot outlive the stack frame",
                "Allocates a fixed-size array on the heap",
                "Allocates memory shared across threads",
                "Reserves virtual address space without physical allocation"
            ],
            CorrectAnswer = "Allocates memory on the stack — zero GC overhead, but limited size and cannot outlive the stack frame",
            Explanation = "stackalloc allocates a contiguous block on the call stack — no heap allocation, no GC pressure. It must be used with Span<T>: Span<byte> buffer = stackalloc byte[256]. Limitations: size should be small (stack is ~1 MB), and the memory is invalid after the method returns. Ideal for temporary small buffers.",
            ExampleCode = "Span<byte> buffer = stackalloc byte[128];\nbuffer.Fill(0);\nSomeApi(buffer); // no heap allocation" },

        new() { Id = 273, TopicId = 6, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the purpose of ArrayPool<T>?",
            Options = [
                "Creates immutable arrays",
                "Pools and reuses arrays to reduce GC pressure from frequent large array allocations",
                "Provides thread-safe arrays",
                "Limits the size of arrays in an application"
            ],
            CorrectAnswer = "Pools and reuses arrays to reduce GC pressure from frequent large array allocations",
            Explanation = "ArrayPool<T>.Shared provides Rent(minimumLength) and Return(array). Instead of allocating and abandoning large arrays in tight loops (causing LOH pressure), you rent from the pool and return when done. The pool recycles them. Used extensively in ASP.NET Core and System.Text.Json for zero-allocation processing.",
            ExampleCode = "var pool = ArrayPool<byte>.Shared;\nbyte[] buffer = pool.Rent(4096);\ntry\n{\n    // use buffer\n}\nfinally\n{\n    pool.Return(buffer, clearArray: true);\n}" },

        new() { Id = 274, TopicId = 6, Difficulty = DifficultyLevel.Senior, Type = QuestionType.TrueFalse,
            Text = "Objects with finalizers take longer to be garbage collected than objects without.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "Objects with finalizers require at least two GC cycles to be reclaimed. In the first cycle, the GC places them on the finalization queue instead of freeing them. After the finalizer runs (on the Finalizer thread), a second collection frees the memory. This also promotes the object to Gen 1, making it even more expensive." },

        new() { Id = 275, TopicId = 6, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is string interning in .NET?",
            Options = [
                "Converting strings to integers",
                "Storing a single copy of each unique string literal so identical strings share one heap object",
                "Encrypting string values for security",
                "Compressing string storage"
            ],
            CorrectAnswer = "Storing a single copy of each unique string literal so identical strings share one heap object",
            Explanation = "The CLR interns compile-time string literals — all uses of the same literal share one object. String.Intern() forces runtime interning; String.IsInterned() checks. Excessive interning of dynamic strings wastes memory (the intern pool is never GC'd). Interning is a Flyweight pattern implementation." },

        new() { Id = 276, TopicId = 6, Difficulty = DifficultyLevel.Senior, Type = QuestionType.CodeSnippet,
            Text = "What is the memory problem in this code?",
            CodeSnippet = "public class EventBroker\n{\n    public static event EventHandler OnEvent;\n}\n\npublic class Subscriber\n{\n    public Subscriber()\n    {\n        EventBroker.OnEvent += HandleEvent;\n    }\n\n    private void HandleEvent(object s, EventArgs e) { }\n}",
            Options = [
                "Static events are not valid C#",
                "Subscriber instances are kept alive forever by the static event — memory leak",
                "EventHandler cannot be static",
                "No issue"
            ],
            CorrectAnswer = "Subscriber instances are kept alive forever by the static event — memory leak",
            Explanation = "The static OnEvent holds a strong reference to each Subscriber instance via its HandleEvent delegate. Even if all other references to Subscriber are released, the static event keeps them alive. Fix: unsubscribe in Dispose() (EventBroker.OnEvent -= HandleEvent), or use WeakReference-based event patterns." },

        new() { Id = 277, TopicId = 6, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is StringBuilder and why is it more memory-efficient than string concatenation in a loop?",
            Options = [
                "StringBuilder uses value semantics to avoid copies",
                "Strings are immutable — each + creates a new string; StringBuilder maintains a mutable buffer and builds the final string once",
                "StringBuilder uses unsafe code internally",
                "StringBuilder is only faster for strings over 1000 characters"
            ],
            CorrectAnswer = "Strings are immutable — each + creates a new string; StringBuilder maintains a mutable buffer and builds the final string once",
            Explanation = "String concatenation in a loop (str += item) creates a new string object on every iteration — O(n²) in total. StringBuilder maintains an internal char array, appending in amortised O(1). Call ToString() once at the end. For a fixed number of concatenations (< ~3), the + operator is fine.",
            ExampleCode = "// Bad: O(n²) allocations\nstring result = \"\";\nforeach (var item in items) result += item;\n\n// Good: O(n)\nvar sb = new StringBuilder();\nforeach (var item in items) sb.Append(item);\nstring result = sb.ToString();" },

        new() { Id = 278, TopicId = 6, Difficulty = DifficultyLevel.Senior, Type = QuestionType.TrueFalse,
            Text = "Unsafe code in C# allows pointer arithmetic and bypasses the GC's memory management.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "The 'unsafe' keyword enables pointer types and arithmetic. You can use fixed() to pin objects in memory (prevent GC from moving them). Unsafe code bypasses type safety and GC compaction for pinned objects. It requires the 'AllowUnsafeBlocks' project setting and should be used only for performance-critical interop or algorithms." },

        new() { Id = 279, TopicId = 6, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the 'fixed' statement in C# used for?",
            Options = [
                "Makes a variable read-only for the duration of a block",
                "Pins a managed object in memory so the GC cannot move it, allowing unsafe pointer operations on it",
                "Fixes a struct's size at compile time",
                "Prevents a variable from being garbage collected"
            ],
            CorrectAnswer = "Pins a managed object in memory so the GC cannot move it, allowing unsafe pointer operations on it",
            Explanation = "The GC compacts the heap by moving objects, which would invalidate raw pointers. 'fixed' tells the GC not to move the object while the fixed block executes: 'fixed(byte* ptr = array)'. Excessive pinning increases GC fragmentation. Use stackalloc, Span<T>, or Memory<T> where possible to avoid pinning." },

        new() { Id = 280, TopicId = 6, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is GC pressure and how do you reduce it?",
            Options = [
                "CPU usage during GC; reduce by adding more RAM",
                "The rate of heap allocations that trigger GC collections; reduce by pooling, using structs, Span<T>, and avoiding boxing",
                "The time threads are suspended during GC; reduce by using background GC only",
                "The number of finalizable objects; reduce by avoiding all class types"
            ],
            CorrectAnswer = "The rate of heap allocations that trigger GC collections; reduce by pooling, using structs, Span<T>, and avoiding boxing",
            Explanation = "GC pressure is high allocation rate forcing frequent collections with stop-the-world pauses. Reduce it by: using struct for small, short-lived value objects; ArrayPool/MemoryPool for buffers; Span<T>/stackalloc for stack-based work; avoiding boxing by using generic collections; reusing objects via object pooling." },

        new() { Id = 281, TopicId = 6, Difficulty = DifficultyLevel.Lead, Type = QuestionType.CodeSnippet,
            Text = "What is the output, and what memory concept does it illustrate?",
            CodeSnippet = "int x = 5;\nobject boxed = x;    // boxing\nint y = (int)boxed;  // unboxing\n\nx = 10;\nConsole.WriteLine(x);     // ?\nConsole.WriteLine(y);     // ?\nConsole.WriteLine(boxed); // ?",
            Options = [
                "10, 5, 5",
                "10, 10, 10",
                "5, 5, 5",
                "10, 5, 10"
            ],
            CorrectAnswer = "10, 5, 5",
            Explanation = "Boxing copies the value of x (5) to a new heap object. Unboxing copies from the heap object to y (5). Changing x to 10 does not affect boxed (a separate heap copy) or y (a copy made at unbox time). Each step creates independent copies." },

        new() { Id = 282, TopicId = 6, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between Gen 0, Gen 1, and Gen 2 GC collections in terms of cost?",
            Options = [
                "They cost the same",
                "Gen 0 is cheapest (small heap, frequent); Gen 1 is medium; Gen 2 is most expensive (entire managed heap, stop-the-world)",
                "Gen 2 is cheapest; Gen 0 is most expensive",
                "Only Gen 2 causes stop-the-world pauses"
            ],
            CorrectAnswer = "Gen 0 is cheapest (small heap, frequent); Gen 1 is medium; Gen 2 is most expensive (entire managed heap, stop-the-world)",
            Explanation = "Gen 0 is small and collected often — pauses are very short. Gen 1 is collected when Gen 0 survivors are promoted. Gen 2 collects the full heap including the LOH — the most expensive, causing the longest pauses. Server GC and background GC reduce Gen 2 impact in production." },

        new() { Id = 283, TopicId = 6, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is object pooling and why is it beneficial?",
            Options = [
                "Storing objects in a database pool",
                "Reusing pre-allocated objects from a pool instead of creating and garbage collecting them on each use",
                "Grouping related objects into collections",
                "Sharing static objects across threads"
            ],
            CorrectAnswer = "Reusing pre-allocated objects from a pool instead of creating and garbage collecting them on each use",
            Explanation = "Object pooling reduces GC pressure by recycling expensive-to-create objects (database connections, HttpClient, buffers). Microsoft.Extensions.ObjectPool provides a generic implementation. ObjectPool<T> has Rent()/Return() semantics. The pool must be carefully managed — returned objects should be reset to a clean state." },

        new() { Id = 284, TopicId = 6, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "A struct in C# can implement an interface.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "Structs can implement interfaces. However, assigning a struct to an interface variable causes boxing — the struct is copied to a heap allocation. This boxing negates the performance benefit of structs in tight loops. Using generic constraints 'where T : IFoo' avoids boxing when calling interface methods on structs." },

        new() { Id = 285, TopicId = 6, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between shallow copy and deep copy?",
            Options = [
                "Shallow copy duplicates value fields only; deep copy duplicates reference fields",
                "Shallow copy duplicates the object and copies field values, but references still point to the same nested objects; deep copy recursively copies all referenced objects",
                "They are identical for value types",
                "Deep copy only applies to arrays"
            ],
            CorrectAnswer = "Shallow copy duplicates the object and copies field values, but references still point to the same nested objects; deep copy recursively copies all referenced objects",
            Explanation = "MemberwiseClone() creates a shallow copy — all value fields are copied but reference fields share the same heap objects. Deep copy requires manual implementation (copy constructors, serialisation-round-trip, or explicit recursive copying). C# records' 'with' expressions create shallow copies." },

        new() { Id = 286, TopicId = 6, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What causes the 'gen2 heap fragmentation' problem and how can it be mitigated?",
            Options = [
                "Too many Gen 0 collections",
                "Long-lived objects mixed with short-lived ones in Gen 2, leaving holes when short-lived ones are collected; mitigate with object pooling and limiting Gen 2 promotions",
                "Using too many interfaces",
                "String concatenation"
            ],
            CorrectAnswer = "Long-lived objects mixed with short-lived ones in Gen 2, leaving holes when short-lived ones are collected; mitigate with object pooling and limiting Gen 2 promotions",
            Explanation = "When short-lived objects are promoted to Gen 2 (e.g., large temp buffers), their collection leaves fragmented free space. Future allocations may not fit these gaps. Mitigation: use ArrayPool for large temporary buffers, avoid storing large temporary objects in long-lived containers, and configure GCSettings.LargeObjectHeapCompactionMode." },

        new() { Id = 287, TopicId = 6, Difficulty = DifficultyLevel.Lead, Type = QuestionType.TrueFalse,
            Text = "In .NET, the GC can run concurrently with application threads during background (server) GC.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "Background GC (default in server and desktop modes since .NET 4) runs Gen 2 collections on dedicated GC threads concurrently with application threads. Gen 0/1 collections are still blocking, but they're short. This reduces pause times significantly for server applications compared to blocking Gen 2 collections." },

        new() { Id = 288, TopicId = 6, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "What memory issue does this code have?",
            CodeSnippet = "void ProcessRequest()\n{\n    var sb = new StringBuilder();\n    for (int i = 0; i < 10_000; i++)\n        sb.Append(i.ToString());\n    var result = sb.ToString();\n    // result used here, then method exits\n}",
            Options = [
                "StringBuilder should not be used in a loop",
                "No issue — this is the correct pattern; the large string may go to the LOH but that is unavoidable here",
                "i.ToString() causes excessive boxing",
                "The result string is never freed"
            ],
            CorrectAnswer = "No issue — this is the correct pattern; the large string may go to the LOH but that is unavoidable here",
            Explanation = "This is actually the correct way to build large strings. Using StringBuilder avoids O(n²) string allocations. The final result string may land on the LOH if ≥85KB, but that's expected for large outputs. The local sb and result go out of scope when the method returns and are eligible for collection." },

        new() { Id = 289, TopicId = 6, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the 'pinned object heap' (POH) introduced in .NET 5?",
            Options = [
                "A heap segment for objects that are never moved by GC — reducing fragmentation from pinning",
                "A heap for immutable objects",
                "A heap for large objects over 1MB",
                "A debug feature for tracking object pinning"
            ],
            CorrectAnswer = "A heap segment for objects that are never moved by GC — reducing fragmentation from pinning",
            Explanation = "The Pinned Object Heap (POH) stores objects that are always pinned (e.g., buffers used by SocketAsyncEventArgs). Before POH, pinned objects fragmented the regular heap. POH is a separate segment where fragmentation is expected and managed separately, reducing interference with the main compacting heap." },

        new() { Id = 290, TopicId = 6, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the NativeMemory class in .NET 6+ used for?",
            Options = [
                "Accessing native OS libraries",
                "Allocating, reallocating, and freeing unmanaged memory outside the GC heap",
                "Pinning managed objects in memory",
                "Accessing memory-mapped files"
            ],
            CorrectAnswer = "Allocating, reallocating, and freeing unmanaged memory outside the GC heap",
            Explanation = "NativeMemory (System.Runtime.InteropServices) provides Alloc(), Realloc(), Free() for raw unmanaged memory. It is a safer, modern replacement for Marshal.AllocHGlobal(). Memory allocated this way is completely outside the GC — you must explicitly free it to avoid leaks. Use for interop scenarios and extreme low-latency requirements." },

        new() { Id = 291, TopicId = 6, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between 'ref' and 'out' parameters in C#?",
            Options = [
                "They are identical",
                "'ref' requires the variable to be initialised before passing; 'out' does not and must be assigned before the method returns",
                "'out' requires initialisation; 'ref' does not",
                "'ref' is for value types only; 'out' works with both"
            ],
            CorrectAnswer = "'ref' requires the variable to be initialised before passing; 'out' does not and must be assigned before the method returns",
            Explanation = "'ref' passes a variable by reference — it must be initialised before the call. 'out' also passes by reference but the method is expected to assign it — the caller doesn't need to initialise it, and the compiler enforces that the method assigns it before returning. int.TryParse uses 'out'." },

        new() { Id = 292, TopicId = 6, Difficulty = DifficultyLevel.Mid, Type = QuestionType.TrueFalse,
            Text = "Implementing IDisposable on a class that only contains managed IDisposable fields still provides value.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "If a class holds managed disposable resources (SqlConnection, Stream, etc.), implementing IDisposable and calling Dispose() on them in your Dispose() method is essential for deterministic release. Without it, callers cannot clean up through your type and must know about internals." },

        new() { Id = 293, TopicId = 6, Difficulty = DifficultyLevel.Senior, Type = QuestionType.CodeSnippet,
            Text = "What is wrong with this code?",
            CodeSnippet = "public class ResourceHolder : IDisposable\n{\n    private Stream _stream = File.OpenRead(\"data.txt\");\n    private bool _disposed;\n\n    public void Dispose()\n    {\n        if (_disposed) return;\n        _stream.Dispose();\n        _disposed = true;\n        // Missing: GC.SuppressFinalize(this)\n    }\n}",
            Options = [
                "Streams cannot be disposed manually",
                "Missing GC.SuppressFinalize(this) — if a finalizer is added later, it will still run unnecessarily",
                "The _disposed flag should be volatile",
                "No issue"
            ],
            CorrectAnswer = "Missing GC.SuppressFinalize(this) — if a finalizer is added later, it will still run unnecessarily",
            Explanation = "If this class ever gains a finalizer (directly or through inheritance), skipping GC.SuppressFinalize(this) means the finalizer still runs after Dispose(), causing a double-dispose. The canonical pattern always calls GC.SuppressFinalize(this) at the end of Dispose() as defensive practice." },

        new() { Id = 294, TopicId = 6, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the IAsyncDisposable interface and when should you use it?",
            Options = [
                "A way to dispose objects on background threads",
                "An interface for objects that need to perform asynchronous cleanup, implemented with async DisposeAsync()",
                "An alternative to IDisposable for reference types only",
                "A pattern for disposing collections of disposable objects"
            ],
            CorrectAnswer = "An interface for objects that need to perform asynchronous cleanup, implemented with async DisposeAsync()",
            Explanation = "IAsyncDisposable (C# 8 / .NET Core 3+) provides ValueTask DisposeAsync() for cleanup that involves async I/O — e.g., flushing a network stream, awaiting pending callbacks. Use 'await using' to call DisposeAsync() correctly. Many .NET types implement both IDisposable and IAsyncDisposable.",
            ExampleCode = "await using var connection = new AsyncDbConnection();\n// DisposeAsync() called automatically at scope end" },

        new() { Id = 295, TopicId = 6, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What happens to a local variable when the method it is declared in returns?",
            Options = [
                "It is immediately deleted from memory",
                "It goes out of scope — if it is a value type on the stack, it is popped; if it is a reference type, the reference is released and the object becomes GC-eligible if unreachable",
                "It is copied to static storage",
                "It is serialised to disk"
            ],
            CorrectAnswer = "It goes out of scope — if it is a value type on the stack, it is popped; if it is a reference type, the reference is released and the object becomes GC-eligible if unreachable",
            Explanation = "When a method returns, its stack frame is popped, releasing local value types instantly. Local reference variables on the stack are also removed, but the heap object they referenced is only freed by the GC if no other roots still reference it." },

        new() { Id = 296, TopicId = 6, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is 'GC.KeepAlive()' used for?",
            Options = [
                "Prevents the GC from collecting an object until the KeepAlive call is reached in the code",
                "Permanently prevents an object from being collected",
                "Forces an object to Gen 2",
                "Registers a finalizer for an object"
            ],
            CorrectAnswer = "Prevents the GC from collecting an object until the KeepAlive call is reached in the code",
            Explanation = "The JIT may determine an object is no longer reachable before its variable is visually out of scope (especially in unsafe/interop code). GC.KeepAlive(obj) is a fence — it prevents the object from being collected before that point. Used in P/Invoke scenarios where a GC handle must stay alive while unmanaged code runs." },

        new() { Id = 297, TopicId = 6, Difficulty = DifficultyLevel.Mid, Type = QuestionType.TrueFalse,
            Text = "Value types stored as fields inside a reference type are allocated on the heap, not the stack.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "A value type field inside a class lives on the heap as part of the class's heap allocation. The stack vs. heap distinction applies to local variables, not to fields. A class object is entirely heap-allocated, including all its value-type fields inline within the object's memory layout." },

        new() { Id = 298, TopicId = 6, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is MemoryPool<T> and how does it relate to ArrayPool<T>?",
            Options = [
                "MemoryPool allocates on the stack; ArrayPool on the heap",
                "MemoryPool<T> leases Memory<T> instances; ArrayPool<T> leases raw arrays; MemoryPool can return slices",
                "They are identical",
                "MemoryPool is for strings; ArrayPool is for numeric types"
            ],
            CorrectAnswer = "MemoryPool<T> leases Memory<T> instances; ArrayPool<T> leases raw arrays; MemoryPool can return slices",
            Explanation = "MemoryPool<T>.Shared.Rent(minLength) returns an IMemoryOwner<T> with a Memory<T> property. It is built on top of ArrayPool but provides a cleaner ownership model (IDisposable lease). ArrayPool<T> is lower-level and marginally faster. Use MemoryPool when you need to pass Memory<T> slices; ArrayPool when you need the raw array." },

        new() { Id = 299, TopicId = 6, Difficulty = DifficultyLevel.Lead, Type = QuestionType.CodeSnippet,
            Text = "What does this code demonstrate and what is the risk?",
            CodeSnippet = "unsafe void ProcessBuffer(byte[] data)\n{\n    fixed (byte* ptr = data)\n    {\n        for (int i = 0; i < data.Length; i++)\n            *(ptr + i) = (byte)(*(ptr + i) ^ 0xFF);\n    }\n}",
            Options = [
                "Memory leak via unpinned pointer",
                "Pointer arithmetic inside a 'fixed' block to XOR all bytes — valid unsafe code that pins the array",
                "Stack overflow from large array",
                "Compile error — unsafe requires explicit cast"
            ],
            CorrectAnswer = "Pointer arithmetic inside a 'fixed' block to XOR all bytes — valid unsafe code that pins the array",
            Explanation = "The fixed block pins data[] so the GC cannot move it. ptr is a raw byte pointer to the first element. The loop XORs each byte with 0xFF (bitwise NOT). This is valid unsafe C# — the risk is that no bounds checking is done; a bug in length calculation could corrupt adjacent memory. Span<T> is a safer alternative." },

        new() { Id = 300, TopicId = 6, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the 'Dispose pattern' variation for sealed classes?",
            Options = [
                "Sealed classes cannot implement IDisposable",
                "Sealed classes can simplify the pattern: implement Dispose() directly without the virtual Dispose(bool) overload, since no subclass can override it",
                "Sealed classes must use finalizers instead of IDisposable",
                "No difference — sealed classes must use the full pattern"
            ],
            CorrectAnswer = "Sealed classes can simplify the pattern: implement Dispose() directly without the virtual Dispose(bool) overload, since no subclass can override it",
            Explanation = "The virtual Dispose(bool) overload exists to allow derived classes to clean up. For sealed classes, inheritance is impossible, so you can simplify: just implement Dispose() directly and call GC.SuppressFinalize(this). Microsoft's guidelines explicitly allow this simplification for sealed types." },

        new() { Id = 301, TopicId = 6, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What does 'in' parameter modifier do?",
            Options = [
                "Passes a value type by value",
                "Passes a value type by reference but prevents the callee from modifying it — avoids copying large structs",
                "Makes the parameter optional",
                "Passes an array by reference"
            ],
            CorrectAnswer = "Passes a value type by reference but prevents the callee from modifying it — avoids copying large structs",
            Explanation = "'in' (C# 7.2) passes a value by readonly reference. Unlike 'ref', the callee cannot modify it. This avoids copying large structs (e.g., a 100-byte struct is passed as a pointer instead of copied). The compiler may create a defensive copy if the 'in' parameter's immutability cannot be guaranteed." },

        new() { Id = 302, TopicId = 6, Difficulty = DifficultyLevel.Senior, Type = QuestionType.TrueFalse,
            Text = "String concatenation using + operator on two strings always creates exactly one new string object.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "Concatenating two strings (a + b) allocates one new string containing the result. The original strings remain unmodified (strings are immutable). The problem arises in loops — n concatenations create n new strings, discarding n-1 intermediaries. For two strings, + is perfectly fine." },

        new() { Id = 303, TopicId = 6, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the 'ref struct' constraint and why can Span<T> not be stored in a class field?",
            Options = [
                "ref struct is a performance optimisation with no semantic restrictions",
                "ref struct instances must live on the stack — they cannot be allocated on the heap, boxed, stored in fields, or used in async methods",
                "ref struct is for read-only structs only",
                "ref struct prevents the struct from being copied"
            ],
            CorrectAnswer = "ref struct instances must live on the stack — they cannot be allocated on the heap, boxed, stored in fields, or used in async methods",
            Explanation = "ref struct guarantees stack-only lifetime. This allows Span<T> to safely point to stack memory (from stackalloc) without the risk of the pointer outliving the memory. Cannot be: boxed, stored in class fields, used in async/iterator methods, captured in lambdas, or assigned to object/interface variables." },

        new() { Id = 304, TopicId = 6, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the purpose of GC.AddMemoryPressure() and GC.RemoveMemoryPressure()?",
            Options = [
                "Manually triggers GC collection",
                "Informs the GC of unmanaged memory allocations so it can account for true memory usage when deciding collection frequency",
                "Reserves memory for future allocations",
                "Compacts the LOH"
            ],
            CorrectAnswer = "Informs the GC of unmanaged memory allocations so it can account for true memory usage when deciding collection frequency",
            Explanation = "The GC only tracks managed heap size. If you allocate large unmanaged memory (via P/Invoke or Marshal.AllocHGlobal), the GC is unaware and may not collect soon enough, causing OOM. AddMemoryPressure(bytes) tells the GC about the extra allocation so it can make better collection decisions." },

        new() { Id = 305, TopicId = 6, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "Setting a reference-type variable to null allows the referenced object to be garbage collected (assuming no other references exist).",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "Setting a variable to null removes that variable's contribution to the object's reference count in the GC's reachability analysis. If no other roots reference the object, it becomes eligible for collection. This is why nulling fields in Dispose() can help release resources earlier." },

        new() { Id = 306, TopicId = 6, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is 'conservative GC' vs. 'precise GC' and which does .NET use?",
            Options = [
                ".NET uses conservative GC which scans all memory",
                ".NET uses precise GC — the runtime knows exactly which memory locations are references via type metadata",
                "Conservative GC is faster; precise GC is safer",
                ".NET uses both depending on the CPU architecture"
            ],
            CorrectAnswer = ".NET uses precise GC — the runtime knows exactly which memory locations are references via type metadata",
            Explanation = "Precise (exact) GC uses type information to know exactly which fields and stack slots are references. This allows safe compaction (moving objects) and eliminates false positives (treating non-pointer values as pointers). .NET's JIT generates GC info maps so the GC knows precisely where all references are at every safe point." },

        new() { Id = 307, TopicId = 6, Difficulty = DifficultyLevel.Lead, Type = QuestionType.CodeSnippet,
            Text = "What memory management technique does this code use?",
            CodeSnippet = "private static readonly ObjectPool<StringBuilder> _pool =\n    ObjectPool.Create<StringBuilder>();\n\npublic string BuildString(IEnumerable<string> parts)\n{\n    var sb = _pool.Get();\n    try\n    {\n        foreach (var p in parts) sb.Append(p);\n        return sb.ToString();\n    }\n    finally\n    {\n        sb.Clear();\n        _pool.Return(sb);\n    }\n}",
            Options = [
                "String interning",
                "Object pooling — reusing StringBuilder instances to avoid repeated allocations and GC pressure",
                "Stack allocation",
                "Weak references"
            ],
            CorrectAnswer = "Object pooling — reusing StringBuilder instances to avoid repeated allocations and GC pressure",
            Explanation = "ObjectPool<StringBuilder> holds pre-created instances. Get() retrieves one; Return() puts it back after clearing. This eliminates repeated StringBuilder allocations in hot paths. The try/finally ensures the instance is always returned — a leaked instance reduces pool effectiveness but doesn't cause a resource leak." },

        // ── .NET Modern Features additional (TopicId = 7) IDs 308–350 ────────
        new() { Id = 308, TopicId = 7, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What is a switch expression in C# 8?",
            Options = [
                "A new keyword that replaces switch statements",
                "An expression form of switch that returns a value and uses => arms instead of case/break",
                "A switch that only works with enum types",
                "A switch that can match on multiple variables simultaneously"
            ],
            CorrectAnswer = "An expression form of switch that returns a value and uses => arms instead of case/break",
            Explanation = "Switch expressions (C# 8) are concise: 'result = variable switch { pattern => value, ... }'. They return a value, require exhaustiveness (or a discard arm _), and use '=>' instead of 'case'/'break'. The compiler warns if arms are unreachable.",
            ExampleCode = "string grade = score switch\n{\n    >= 90 => \"A\",\n    >= 80 => \"B\",\n    >= 70 => \"C\",\n    _      => \"F\"\n};" },

        new() { Id = 309, TopicId = 7, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "File-scoped namespaces (namespace Foo;) reduce indentation for all types in a file.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "C# 10 file-scoped namespaces ('namespace Foo;' without braces) apply the namespace to the entire file. All types no longer need one level of indentation. The file can have only one file-scoped namespace. Traditional block-form namespaces still work for nesting." },

        new() { Id = 310, TopicId = 7, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What does the 'with' expression do for records?",
            Options = [
                "Merges two records into one",
                "Creates a new record instance that is a copy of the original with specified properties changed",
                "Updates the record in-place",
                "Converts a record to an anonymous type"
            ],
            CorrectAnswer = "Creates a new record instance that is a copy of the original with specified properties changed",
            Explanation = "'with' creates a non-destructive mutation — it copies the record and overwrites only the specified properties. The original is unchanged (records are immutable by default). Example: 'var updated = original with { Name = \"Bob\" }' creates a new Person with the same values except Name.",
            ExampleCode = "record Person(string Name, int Age);\n\nvar alice = new Person(\"Alice\", 30);\nvar bob = alice with { Name = \"Bob\" };\n// alice unchanged; bob is new Person(\"Bob\", 30)" },

        new() { Id = 311, TopicId = 7, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "Records in C# use value-based equality by default.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "Records synthesise Equals(), GetHashCode(), and == based on all declared properties. Two record instances with identical property values are considered equal. This is unlike classes, which use reference equality by default. Record structs (C# 10) also have value-based equality." },

        new() { Id = 312, TopicId = 7, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is a positional record in C# 9?",
            Options = [
                "A record that inherits positional members from a base record",
                "A record declared with a primary constructor whose parameters become init-only properties with a synthesised Deconstruct method",
                "A record where properties must be in alphabetical order",
                "A record with positional indexers"
            ],
            CorrectAnswer = "A record declared with a primary constructor whose parameters become init-only properties with a synthesised Deconstruct method",
            Explanation = "Positional records: 'record Point(int X, int Y)' auto-generates public init-only properties X and Y, a constructor, Deconstruct(out int x, out int y), Equals, GetHashCode, and ToString. They support pattern deconstruction: 'if (p is Point(var x, var y))'.",
            ExampleCode = "record Point(int X, int Y);\n\nvar p = new Point(3, 4);\nvar (x, y) = p;         // deconstruction\nConsole.WriteLine(p);  // Point { X = 3, Y = 4 }" },

        new() { Id = 313, TopicId = 7, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is a property pattern in C# 8?",
            Options = [
                "A pattern that matches public property names to field names",
                "A pattern that tests an object's properties against sub-patterns in a switch expression or is expression",
                "A code generator for boilerplate properties",
                "A pattern for validating property values at runtime"
            ],
            CorrectAnswer = "A pattern that tests an object's properties against sub-patterns in a switch expression or is expression",
            Explanation = "Property patterns: 'if (person is { Age: > 18, Country: \"US\" })' tests that person.Age > 18 and person.Country equals \"US\". They can be nested and combined with other patterns. Used extensively in switch expressions for complex dispatch logic.",
            ExampleCode = "string label = order switch\n{\n    { Status: \"New\",  Total: > 100 } => \"Priority new\",\n    { Status: \"New\" }               => \"Standard new\",\n    { Status: \"Shipped\" }           => \"In transit\",\n    _                               => \"Other\"\n};" },

        new() { Id = 314, TopicId = 7, Difficulty = DifficultyLevel.Mid, Type = QuestionType.TrueFalse,
            Text = "Nullable reference types enabled in C# 8 change the runtime behaviour of null dereferences.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "Nullable reference types (NRT) are purely a compile-time analysis feature. They add flow-sensitive warnings for potential null dereferences but do not change the compiled IL or runtime behaviour. A NullReferenceException at runtime is still possible — NRT helps you find and fix these issues before they occur." },

        new() { Id = 315, TopicId = 7, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What does the null-forgiving operator (!) do in C#?",
            Options = [
                "Throws NullReferenceException if the value is null",
                "Suppresses a nullable reference type warning by telling the compiler you know the value is not null",
                "Converts a nullable value type to its non-nullable form",
                "Checks if a value is not null and returns a bool"
            ],
            CorrectAnswer = "Suppresses a nullable reference type warning by telling the compiler you know the value is not null",
            Explanation = "The ! postfix operator (null-forgiving) asserts 'I know this is not null' to suppress NRT warnings. Example: 'string s = GetOrNull()!'. It is purely a compile-time hint with no runtime effect. Use sparingly — each use is a potential NullReferenceException if the assertion is wrong." },

        new() { Id = 316, TopicId = 7, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "What does this code print?",
            CodeSnippet = "record Color(string Name, int R, int G, int B);\n\nvar red   = new Color(\"Red\",   255, 0, 0);\nvar red2  = new Color(\"Red\",   255, 0, 0);\nvar pink  = red with { G = 192 };\n\nConsole.WriteLine(red == red2);\nConsole.WriteLine(red == pink);",
            Options = ["True, True", "False, False", "True, False", "False, True"],
            CorrectAnswer = "True, False",
            Explanation = "Records use value-based equality. red and red2 have identical property values, so red == red2 is true. pink has G=192 (different from red's G=0), so red == pink is false." },

        new() { Id = 317, TopicId = 7, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What are raw string literals in C# 11?",
            Options = [
                "Strings without escape sequences, enclosed in triple-quotes, preserving whitespace and line breaks",
                "Strings stored as byte arrays",
                "Verbatim strings (@\"\") with improved performance",
                "Strings that cannot be interned"
            ],
            CorrectAnswer = "Strings without escape sequences, enclosed in triple-quotes, preserving whitespace and line breaks",
            Explanation = "Raw string literals use at least three double-quotes: \"\"\"...\"\"\"'. They can contain any content including backslashes and embedded quotes without escaping. Indentation is stripped based on the closing triple-quote position. They can be used with $ for raw interpolated strings.",
            ExampleCode = "string json = \"\"\"\n    {\n        \"name\": \"Alice\",\n        \"age\": 30\n    }\n    \"\"\";" },

        new() { Id = 318, TopicId = 7, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is a tuple pattern in C# 8?",
            Options = [
                "Pattern matching on ValueTuple to destructure and test multiple values simultaneously",
                "A pattern for creating tuples from objects",
                "A pattern that matches any tuple type",
                "A pattern for comparing tuples by reference"
            ],
            CorrectAnswer = "Pattern matching on ValueTuple to destructure and test multiple values simultaneously",
            Explanation = "Tuple patterns match multiple values at once: '(state, input) switch { (State.Idle, \"start\") => ..., ... }'. The compiler applies patterns to each tuple element. This replaces nested if/switch chains for multi-dimensional state machines.",
            ExampleCode = "string result = (isLoggedIn, isAdmin) switch\n{\n    (true,  true)  => \"Admin dashboard\",\n    (true,  false) => \"User dashboard\",\n    (false, _)     => \"Login page\"\n};" },

        new() { Id = 319, TopicId = 7, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is a list pattern in C# 11?",
            Options = [
                "A LINQ pattern for querying lists",
                "Pattern matching syntax that matches sequences by position, range, or length",
                "A collection expression for creating lists",
                "A pattern for matching List<T> types specifically"
            ],
            CorrectAnswer = "Pattern matching syntax that matches sequences by position, range, or length",
            Explanation = "List patterns (C# 11) match arrays/lists by element: '[1, 2, ..]' matches a sequence starting with 1 and 2; '[_, _, var third]' matches a 3-element sequence capturing the third. '..' is the slice pattern matching zero or more elements.",
            ExampleCode = "int[] nums = { 1, 2, 3, 4, 5 };\nif (nums is [1, 2, .. var rest])\n    Console.WriteLine(rest.Length); // 3" },

        new() { Id = 320, TopicId = 7, Difficulty = DifficultyLevel.Senior, Type = QuestionType.TrueFalse,
            Text = "Record structs (introduced in C# 10) are value types with value-based equality.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "'record struct Point(int X, int Y)' is a value type (stack-allocated for locals) with synthesised value-based Equals, GetHashCode, and ToString. Unlike record classes (reference type), record structs are copied on assignment. 'readonly record struct' makes all properties init-only." },

        new() { Id = 321, TopicId = 7, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is generic math in C# 11 (.NET 7) and what interface enables it?",
            Options = [
                "A way to use generics with mathematical types only",
                "Allows writing numeric algorithms once for any numeric type via interfaces like INumber<T>, IAdditionOperators<T,T,T>",
                "Enables SIMD operations through generics",
                "A compile-time math evaluation feature"
            ],
            CorrectAnswer = "Allows writing numeric algorithms once for any numeric type via interfaces like INumber<T>, IAdditionOperators<T,T,T>",
            Explanation = "Generic math (C# 11 / .NET 7) introduces static abstract interface members, allowing operators (+, -, *, /) to be declared in interfaces. INumber<T> combines many math interfaces. Now you can write 'T Sum<T>(T[] arr) where T : INumber<T>' that works for int, double, decimal, etc.",
            ExampleCode = "T Sum<T>(IEnumerable<T> values) where T : INumber<T>\n    => values.Aggregate(T.Zero, (acc, x) => acc + x);\n\nint intSum   = Sum(new[] { 1, 2, 3 });  // 6\ndouble dblSum = Sum(new[] { 1.5, 2.5 }); // 4.0" },

        new() { Id = 322, TopicId = 7, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is an interpolated string handler in C# 10?",
            Options = [
                "A way to format strings at compile time",
                "A mechanism for libraries to intercept and process interpolated strings efficiently without allocating intermediate strings",
                "A handler that validates interpolated string arguments",
                "A class that stores the parts of an interpolated string"
            ],
            CorrectAnswer = "A mechanism for libraries to intercept and process interpolated strings efficiently without allocating intermediate strings",
            Explanation = "Interpolated string handlers allow libraries to receive the format string parts and arguments separately (via compiler-generated code) before any string is allocated. If the handler decides not to process (e.g., log level too low), no string is built at all. ILogger uses this for near-zero-cost conditional logging.",
            ExampleCode = "// Logger uses interpolated string handler:\nlogger.LogDebug($\"Processing {itemCount} items\");\n// If Debug is disabled, no string is allocated at all" },

        new() { Id = 323, TopicId = 7, Difficulty = DifficultyLevel.Senior, Type = QuestionType.CodeSnippet,
            Text = "What does this code print?",
            CodeSnippet = "var nums = new[] { 1, 2, 3, 4, 5 };\n\nvar result = nums switch\n{\n    [var first, .., var last] => $\"{first} to {last}\",\n    [] => \"empty\",\n    _ => \"other\"\n};\nConsole.WriteLine(result);",
            Options = ["1 to 5", "empty", "other", "Compile error"],
            CorrectAnswer = "1 to 5",
            Explanation = "The list pattern '[var first, .., var last]' matches any array with at least two elements, capturing the first and last. nums has 5 elements: first=1, last=5. Output: '1 to 5'." },

        new() { Id = 324, TopicId = 7, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is a static abstract member in an interface (C# 11)?",
            Options = [
                "A static method with a body in an interface",
                "An interface member that is static and must be implemented by each implementing type — enabling operator overloading and factory patterns in interfaces",
                "A static field declaration in an interface",
                "A feature that makes abstract classes unnecessary"
            ],
            CorrectAnswer = "An interface member that is static and must be implemented by each implementing type — enabling operator overloading and factory patterns in interfaces",
            Explanation = "Static abstract interface members allow declaring static methods and operators that implementing types must provide. This is the foundation of generic math. Example: 'interface IFactory<T> { static abstract T Create(); }' — each implementing type provides its own Create() factory without needing a virtual dispatch." },

        new() { Id = 325, TopicId = 7, Difficulty = DifficultyLevel.Lead, Type = QuestionType.TrueFalse,
            Text = "In C# 12, primary constructors on classes capture parameters as fields automatically.",
            Options = ["True", "False"],
            CorrectAnswer = "False",
            Explanation = "Primary constructor parameters on classes (C# 12) are captured as hidden instance state — but they are NOT surfaced as fields or properties automatically. Unlike record primary constructor parameters (which become properties), class primary constructor parameters are just in-scope throughout the class body. You can assign them to explicit fields if you need property access." },

        new() { Id = 326, TopicId = 7, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What are collection expressions in C# 12?",
            Options = [
                "A new LINQ syntax for collection queries",
                "Unified literal syntax '[...]' for creating arrays, List<T>, Span<T>, and other collection types with compiler-selected optimal implementation",
                "A way to declare immutable collections",
                "An alternative to object initializers for collections"
            ],
            CorrectAnswer = "Unified literal syntax '[...]' for creating arrays, List<T>, Span<T>, and other collection types with compiler-selected optimal implementation",
            Explanation = "Collection expressions (C# 12) use '[...]' syntax for any collection type. 'int[] arr = [1, 2, 3]', 'List<int> list = [1, 2, 3]', 'Span<int> span = [1, 2, 3]'. The spread operator '..' flattens another collection inline: '[..existing, 4, 5]'. The compiler picks the most efficient construction strategy.",
            ExampleCode = "List<int> list   = [1, 2, 3];\nint[]     arr    = [4, 5, 6];\nint[]     combined = [..list, ..arr]; // [1,2,3,4,5,6]" },

        new() { Id = 327, TopicId = 7, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "What does this code print?",
            CodeSnippet = "var p = new { Name = \"Alice\", Score = 95 };\nConsole.WriteLine($\"{p.Name}: {p.Score}\");",
            Options = ["Alice: 95", "{ Name = Alice, Score = 95 }", "Compile error", "Name: Score"],
            CorrectAnswer = "Alice: 95",
            Explanation = "Anonymous types are projected objects with compiler-generated properties. p.Name is 'Alice' and p.Score is 95. The string interpolation formats them normally." },

        new() { Id = 328, TopicId = 7, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between 'var' and 'dynamic' in C#?",
            Options = [
                "Both defer type resolution to runtime",
                "var is statically typed — the compiler infers the type at compile time; dynamic defers type checking to runtime",
                "dynamic is faster than var",
                "var only works with anonymous types"
            ],
            CorrectAnswer = "var is statically typed — the compiler infers the type at compile time; dynamic defers type checking to runtime",
            Explanation = "'var' is compiler inference — the type is known at compile time (IntelliSense works, errors are compile-time). 'dynamic' bypasses the type system — member access is resolved at runtime via the DLR. Errors with dynamic are runtime exceptions. Use var freely; use dynamic only for COM interop, reflection-heavy code, or consuming dynamic languages." },

        new() { Id = 329, TopicId = 7, Difficulty = DifficultyLevel.Mid, Type = QuestionType.TrueFalse,
            Text = "C# 9 introduced 'not' and 'and'/'or' logical pattern combinators.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "C# 9 added logical patterns: 'not null', 'x is > 0 and < 100', 'x is null or 0'. These combine patterns with boolean logic inline in 'is' expressions and switch arms. They replace compound if conditions with more readable, composable patterns." },

        new() { Id = 330, TopicId = 7, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is a 'required' property member and what C# version introduced it?",
            Options = [
                "A property that is non-nullable, introduced in C# 8",
                "A property that callers must set in object initialisers or constructors, enforced at compile time — introduced in C# 11",
                "A property with a backing field, introduced in C# 9",
                "A read-only property with a default value"
            ],
            CorrectAnswer = "A property that callers must set in object initialisers or constructors, enforced at compile time — introduced in C# 11",
            Explanation = "'required' (C# 11) marks a property as mandatory. The compiler enforces that all required members are set when creating an instance via object initialiser. This eliminates the need for constructor overloads just to ensure mandatory properties are set. Combined with 'init', it creates clean immutable DTO types." },

        new() { Id = 331, TopicId = 7, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What does the '?.' (null-conditional) operator do in C#?",
            Options = [
                "Throws NullReferenceException if the left side is null",
                "Evaluates the right side only if the left side is non-null, otherwise returns null",
                "Converts a nullable type to a non-nullable type",
                "Assigns null to the variable"
            ],
            CorrectAnswer = "Evaluates the right side only if the left side is non-null, otherwise returns null",
            Explanation = "'obj?.Property' returns null if obj is null, otherwise returns obj.Property. This eliminates verbose null guards. Chain multiple: 'a?.B?.C?.D'. Combine with ?? for defaults: 'a?.Name ?? \"Unknown\"'. Works for method calls too: 'handler?.Invoke()'." },

        new() { Id = 332, TopicId = 7, Difficulty = DifficultyLevel.Junior, Type = QuestionType.TrueFalse,
            Text = "The '??' (null-coalescing) operator returns the left operand if it is not null, otherwise returns the right operand.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "'a ?? b' returns a if a is not null, otherwise b. '??=' assigns the right side only if the left side is null: 'x ??= defaultValue'. These operators simplify null-default patterns and replace ternary null checks." },

        new() { Id = 333, TopicId = 7, Difficulty = DifficultyLevel.Mid, Type = QuestionType.CodeSnippet,
            Text = "What does this switch expression print?",
            CodeSnippet = "object obj = 42;\n\nstring result = obj switch\n{\n    int n when n > 100 => \"big int\",\n    int n              => $\"int: {n}\",\n    string s           => $\"str: {s}\",\n    null               => \"null\",\n    _                  => \"other\"\n};\nConsole.WriteLine(result);",
            Options = ["big int", "int: 42", "str: 42", "other"],
            CorrectAnswer = "int: 42",
            Explanation = "obj is an int with value 42. The first arm requires n > 100 — false for 42. The second arm matches 'int n' with no guard — matches, binds n=42, returns 'int: 42'. Arms are evaluated in order; the first match wins." },

        new() { Id = 334, TopicId = 7, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the 'CallerMemberName' attribute used for?",
            Options = [
                "Gets the name of the class calling the method",
                "Automatically fills a string parameter with the name of the calling member (method, property, constructor) at compile time",
                "Validates that the caller's name matches a pattern",
                "Gets the file path of the calling code"
            ],
            CorrectAnswer = "Automatically fills a string parameter with the name of the calling member (method, property, constructor) at compile time",
            Explanation = "[CallerMemberName] on an optional string parameter causes the compiler to insert the calling member's name automatically. Used in INotifyPropertyChanged: SetProperty(value, ref _field, propertyName: CallerMemberName) — so you don't hardcode 'nameof(PropertyName)'.",
            ExampleCode = "void Log(string msg, [CallerMemberName] string caller = \"\")\n    => Console.WriteLine($\"[{caller}] {msg}\");\n\nvoid DoWork() => Log(\"done\"); // prints \"[DoWork] done\"" },

        new() { Id = 335, TopicId = 7, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between record and record struct in C#?",
            Options = [
                "They are identical",
                "record is a reference type (heap); record struct is a value type (stack/inline) — both have value-based equality",
                "record struct cannot use 'with' expressions",
                "record is immutable; record struct is mutable"
            ],
            CorrectAnswer = "record is a reference type (heap); record struct is a value type (stack/inline) — both have value-based equality",
            Explanation = "'record' (class) is heap-allocated, supports inheritance, and has == value equality. 'record struct' is value-type, no inheritance, stack-allocatable, and also has value equality. 'readonly record struct' is fully immutable. Choose record struct for small, short-lived data models to reduce GC pressure." },

        new() { Id = 336, TopicId = 7, Difficulty = DifficultyLevel.Senior, Type = QuestionType.TrueFalse,
            Text = "Pattern matching with 'is' in C# 7+ can introduce a variable that is in scope for the remaining expression.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "The declaration pattern 'if (obj is Dog dog)' introduces 'dog' scoped to the if block (and the condition itself when used with &&). 'if (obj is string s && s.Length > 5)' — s is in scope for s.Length. This is definite assignment flow analysis for pattern variables." },

        new() { Id = 337, TopicId = 7, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the 'nameof' operator in C#?",
            Options = [
                "Gets the assembly name of a type",
                "Returns the simple name of a variable, type, or member as a compile-time string constant",
                "Converts a type to its fully qualified name",
                "Gets the method name at runtime via reflection"
            ],
            CorrectAnswer = "Returns the simple name of a variable, type, or member as a compile-time string constant",
            Explanation = "'nameof(Person.Name)' produces the string \"Name\" at compile time — it is not a runtime operation. This avoids magic strings in ArgumentException, INotifyPropertyChanged, and attribute usage. Rename refactoring updates nameof references automatically, unlike hardcoded strings." },

        new() { Id = 338, TopicId = 7, Difficulty = DifficultyLevel.Lead, Type = QuestionType.CodeSnippet,
            Text = "What does this C# 12 primary constructor code demonstrate?",
            CodeSnippet = "public class OrderService(IOrderRepository repo, ILogger<OrderService> logger)\n{\n    public async Task<Order> GetOrderAsync(int id)\n    {\n        logger.LogInformation(\"Getting order {Id}\", id);\n        return await repo.GetByIdAsync(id);\n    }\n}",
            Options = [
                "An abstract factory for order services",
                "A primary constructor on a class capturing dependencies directly in the class header",
                "Constructor injection with private fields",
                "A record class with services"
            ],
            CorrectAnswer = "A primary constructor on a class capturing dependencies directly in the class header",
            Explanation = "C# 12 primary constructors on classes allow dependencies to be declared in the class header. 'repo' and 'logger' are captured and available throughout the class body without explicit field declarations. This reduces boilerplate for DI-heavy services while keeping the constructor declaration at the class level." },

        new() { Id = 339, TopicId = 7, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the purpose of the 'scoped' keyword in C# 11?",
            Options = [
                "Limits a variable's accessibility to the current method",
                "Constrains a ref or Span<T> parameter so it cannot escape the method scope — enables the compiler to guarantee stack safety",
                "Creates a scoped using declaration",
                "Marks a type as available only within its namespace"
            ],
            CorrectAnswer = "Constrains a ref or Span<T> parameter so it cannot escape the method scope — enables the compiler to guarantee stack safety",
            Explanation = "'scoped' (C# 11) on a ref parameter or Span<T> tells the compiler the reference cannot be stored in a field or returned — it stays within the method scope. This allows the compiler to prove stack safety more precisely and enables some optimisations that were previously forbidden.",
            ExampleCode = "void Fill(scoped Span<int> data)\n{\n    data.Fill(0); // OK\n    // _storedSpan = data; // compile error — scoped cannot escape\n}" },

        new() { Id = 340, TopicId = 7, Difficulty = DifficultyLevel.Junior, Type = QuestionType.MultipleChoice,
            Text = "What does the '??' operator assign in '??='?",
            Options = [
                "Assigns the right side if the left side is not null",
                "Assigns the right side to the left side only if the left side is currently null",
                "Compares two nullable values",
                "Creates a new nullable type"
            ],
            CorrectAnswer = "Assigns the right side to the left side only if the left side is currently null",
            Explanation = "'x ??= value' is equivalent to 'x = x ?? value' or 'if (x == null) x = value'. Introduced in C# 8, it is a convenient null-coalescing assignment used for lazy initialisation: '_cache ??= new Dictionary<string, Item>()'." },

        new() { Id = 341, TopicId = 7, Difficulty = DifficultyLevel.Mid, Type = QuestionType.TrueFalse,
            Text = "Implicit usings in .NET 6+ automatically include commonly-used namespaces like System, System.Linq, and System.Threading.Tasks.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "Implicit global usings (enabled by default in .NET 6+ SDK-style projects) add global using directives for common namespaces based on the project type. Console apps get System, System.Collections.Generic, System.Linq, System.Threading.Tasks, etc. You can add or remove them in the project file." },

        new() { Id = 342, TopicId = 7, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is top-level statements in C# 9?",
            Options = [
                "Statements that run before the Main method",
                "A feature that allows a program's entry point code to be written directly in a file without a class or Main method declaration",
                "Global variables declared outside any class",
                "Compile-time constants declared at the file level"
            ],
            CorrectAnswer = "A feature that allows a program's entry point code to be written directly in a file without a class or Main method declaration",
            Explanation = "Top-level statements (C# 9) allow writing 'Console.WriteLine(\"Hello\");' directly in Program.cs without 'class Program { static void Main(string[] args) { ... } }'. The compiler generates the class and Main method automatically. Only one file per project can have top-level statements." },

        new() { Id = 343, TopicId = 7, Difficulty = DifficultyLevel.Senior, Type = QuestionType.CodeSnippet,
            Text = "What does this C# 11 pattern matching code do?",
            CodeSnippet = "int[] data = { 2, 4, 6, 8 };\n\nbool allEven = data is [.. var items] && items.All(x => x % 2 == 0);\nConsole.WriteLine(allEven);",
            Options = ["True", "False", "Compile error", "Throws exception"],
            CorrectAnswer = "True",
            Explanation = "The list pattern '[.. var items]' matches any array and captures all elements into items (as a Span<int> slice). Then All(x => x % 2 == 0) checks that all are even. {2,4,6,8} are all even, so allEven is true." },

        new() { Id = 344, TopicId = 7, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the 'interceptors' feature preview in C# 12?",
            Options = [
                "A way to intercept exceptions before they propagate",
                "A source-generator mechanism to replace specific method call sites with different code at compile time",
                "A middleware feature for intercepting HTTP requests",
                "A way to intercept property setters"
            ],
            CorrectAnswer = "A source-generator mechanism to replace specific method call sites with different code at compile time",
            Explanation = "Interceptors (C# 12 preview, used internally by EF Core and ASP.NET Core) allow source generators to replace specific call sites with optimised generated code at compile time. EF Core uses them to pre-compile LINQ queries. They are identified by file path and line number of the original call site." },

        new() { Id = 345, TopicId = 7, Difficulty = DifficultyLevel.Senior, Type = QuestionType.MultipleChoice,
            Text = "What is the difference between 'init' and 'set' accessors?",
            Options = [
                "They are identical",
                "'set' allows mutation at any time; 'init' only allows setting during object initialisation (construction or object initialiser)",
                "'init' is public; 'set' can have any access modifier",
                "'init' properties cannot be used in object initialisers"
            ],
            CorrectAnswer = "'set' allows mutation at any time; 'init' only allows setting during object initialisation (construction or object initialiser)",
            Explanation = "'init' (C# 9) creates a property that behaves like 'set' during construction and object initialisers, but becomes read-only afterwards. This enables immutable-style types that still support 'new Thing { Prop = value }' syntax without writing custom constructors for every combination." },

        new() { Id = 346, TopicId = 7, Difficulty = DifficultyLevel.Lead, Type = QuestionType.TrueFalse,
            Text = "In C# 11, 'required' members can be suppressed using the [SetsRequiredMembers] attribute on a constructor.",
            Options = ["True", "False"],
            CorrectAnswer = "True",
            Explanation = "[SetsRequiredMembers] tells the compiler that a constructor sets all required members, so callers using that constructor don't need to set them again in an object initialiser. Used for constructors that set everything explicitly — prevents double-initialisation from being enforced by the compiler." },

        new() { Id = 347, TopicId = 7, Difficulty = DifficultyLevel.Mid, Type = QuestionType.MultipleChoice,
            Text = "What is the 'global using' directive and which C# version introduced it?",
            Options = [
                "A using that imports all types from an assembly; introduced in C# 9",
                "A using directive that applies to all files in the project; introduced in C# 10",
                "A using that imports static members globally; introduced in C# 6",
                "A namespace alias applied project-wide; introduced in C# 8"
            ],
            CorrectAnswer = "A using directive that applies to all files in the project; introduced in C# 10",
            Explanation = "'global using System.Linq;' in any file makes System.Linq available throughout the project. Introduced in C# 10. Typically placed in a dedicated GlobalUsings.cs file. SDK projects with ImplicitUsings enabled generate these automatically for common .NET namespaces." },

        new() { Id = 348, TopicId = 7, Difficulty = DifficultyLevel.Senior, Type = QuestionType.CodeSnippet,
            Text = "What does this code demonstrate?",
            CodeSnippet = "interface ICreatable<T> where T : ICreatable<T>\n{\n    static abstract T Create();\n}\n\nrecord Point(int X, int Y) : ICreatable<Point>\n{\n    public static Point Create() => new(0, 0);\n}\n\nT MakeDefault<T>() where T : ICreatable<T> => T.Create();",
            Options = [
                "Reflection-based factory",
                "Static abstract interface member enabling a generic factory pattern",
                "Abstract class factory method",
                "Prototype pattern"
            ],
            CorrectAnswer = "Static abstract interface member enabling a generic factory pattern",
            Explanation = "ICreatable<T> declares 'static abstract T Create()' — each implementing type must provide a static factory. MakeDefault<T> can call T.Create() generically without knowing the concrete type. This is generic math's static abstract member feature enabling factory, operator, and parse patterns in interfaces." },

        new() { Id = 349, TopicId = 7, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What are 'inline arrays' introduced in C# 12?",
            Options = [
                "Arrays allocated inline in struct fields without indirection, enabling stack-like fixed-size buffers without unsafe code",
                "Arrays declared on a single line",
                "Arrays with compile-time constant lengths",
                "Arrays embedded in string interpolation"
            ],
            CorrectAnswer = "Arrays allocated inline in struct fields without indirection, enabling stack-like fixed-size buffers without unsafe code",
            Explanation = "Inline arrays (C# 12) use [InlineArray(N)] on a struct to create a fixed-size, inline buffer. The runtime lays out N elements contiguously inside the struct without a heap pointer. Used internally by .NET for Span<T> creation from stack buffers. Safer alternative to unsafe fixed buffers.",
            ExampleCode = "[System.Runtime.CompilerServices.InlineArray(4)]\npublic struct Buffer4\n{\n    private int _element0;\n}\n\n// Usage:\nBuffer4 buf = default;\nbuf[0] = 42; // Span-like indexing" },

        new() { Id = 350, TopicId = 7, Difficulty = DifficultyLevel.Lead, Type = QuestionType.MultipleChoice,
            Text = "What is the 'params ReadOnlySpan<T>' overload resolution improvement in C# 13?",
            Options = [
                "Allows passing spans as params arguments",
                "The compiler prefers a params ReadOnlySpan<T> overload over params T[] to avoid heap allocations when calling with inline arguments",
                "Makes all params parameters use spans automatically",
                "Enables params on Span<T> fields"
            ],
            CorrectAnswer = "The compiler prefers a params ReadOnlySpan<T> overload over params T[] to avoid heap allocations when calling with inline arguments",
            Explanation = "C# 13 extended 'params' to work with any type supporting collection expressions (ReadOnlySpan<T>, List<T>, etc.). When calling a method with 'params ReadOnlySpan<T>', the compiler can use a stack-allocated span instead of a heap array, eliminating the GC pressure of params T[] for small argument lists. Console.WriteLine overloads use this." },
    ];
}
