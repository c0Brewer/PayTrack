# Backend Test Patterns

**Project:** `backend/PayTrack.Tests/` — xUnit + Moq + FluentAssertions + EF InMemory

## Service unit test

Mock the repository, test business logic:

```csharp
public class MyServiceTests {
    private readonly Mock<IMyRepository> repoMock = new();
    private readonly MyService service;
    public MyServiceTests() => service = new MyService(repoMock.Object);

    [Fact] public async Task Method_Condition_ExpectedResult() {
        repoMock.Setup(r => r.GetAsync(...)).ReturnsAsync(...);
        var result = await service.MethodAsync(...);
        result.Should().NotBeNull();
    }

    [Theory, InlineData(...)] public async Task ... { }
}
```

## Endpoint integration test

WebApplicationFactory with mock services:

```csharp
public class MyEndpointsTests(MyApiFactory factory) : IClassFixture<MyApiFactory> { ... }

public class MyApiFactory : WebApplicationFactory<Program> {
    public Mock<IAuthService> AuthServiceMock { get; } = new();
    public Mock<IMyService> MyServiceMock { get; } = new();
    protected override void ConfigureWebHost(IWebHostBuilder builder) {
        builder.UseEnvironment("Test");
        builder.ConfigureServices(services => {
            // Replace DB with InMemory
            // Register TestAuthHandler for "Test" scheme
            // Replace service registrations with mocks via services.AddSingleton(mock.Object)
        });
    }
}
```

`TestAuthHandler` (in `Helper/`) always authenticates successfully. Add `client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test")` to every authenticated request.

## Positional record DTOs

Use positional constructor syntax:

```csharp
new BankStatementUpdateRequestDto("entry-0", MatchedTransactionId: 5, Skipped: false)
// NOT: new() { EntryId = ..., ... }
```

## Repository mock for ITransactionRepository

```csharp
repoMock.Setup(r => r.GetAllAsync(It.IsAny<GetTransactionQuery>()))
    .ReturnsAsync((new List<Transaction> { tx }, totalCount));
repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<GetTransactionQueryById>()))
    .ReturnsAsync(transaction);
repoMock.Setup(r => r.UpdateAsync(It.IsAny<Transaction>()))
    .ReturnsAsync((Transaction t) => t);
```
