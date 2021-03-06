﻿namespace AsyncUsageAnalyzers.Test.Usage
{
    using System.Threading;
    using System.Threading.Tasks;
    using AsyncUsageAnalyzers.Usage;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using TestHelper;
    using Xunit;

    public class UseConfigureAwaitUnitTests : CodeFixVerifier
    {
        [Fact]
        public async Task TestSimpleExpression()
        {
            string testCode = @"
using System.Threading.Tasks;
class ClassName
{
    async Task MethodNameAsync()
    {
        await Task.Delay(1000);
    }
}
";
            string fixedCode = @"
using System.Threading.Tasks;
class ClassName
{
    async Task MethodNameAsync()
    {
        await Task.Delay(1000).ConfigureAwait(false);
    }
}
";

            DiagnosticResult expected = CSharpDiagnostic().WithLocation(7, 15);
            await VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestNestedExpressions()
        {
            string testCode = @"
using System.Threading.Tasks;
class ClassName
{
    async Task<Task> FirstMethodAsync()
    {
        return Task.FromResult(true);
    }

    async Task MethodNameAsync()
    {
        await (await FirstMethodAsync());
    }
}
";
            string fixedCode = @"
using System.Threading.Tasks;
class ClassName
{
    async Task<Task> FirstMethodAsync()
    {
        return Task.FromResult(true);
    }

    async Task MethodNameAsync()
    {
        await (await FirstMethodAsync().ConfigureAwait(false)).ConfigureAwait(false);
    }
}
";

            DiagnosticResult[] expected =
            {
                CSharpDiagnostic().WithLocation(12, 15),
                CSharpDiagnostic().WithLocation(12, 22)
            };
            await VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UseConfigureAwaitAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new UseConfigureAwaitCodeFixProvider();
        }
    }
}
