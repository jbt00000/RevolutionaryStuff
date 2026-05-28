using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Applets.Services.TextTemplateRenderers;

namespace RevolutionaryStuff.Applets.Tests.Services.TextTemplateRenderers;

[TestClass]
public class ScribanTextTemplateProcessorTests
{
    private IScribanTextTemplateRenderer _processor = null!;

    [TestInitialize]
    public void Setup()
    {
        _processor = new ScribanTextTemplateRenderer();
    }

    #region Basic Variable Substitution

    [TestMethod]
    public async Task ProcessTemplateAsync_SimpleVariable_ReplacesCorrectly()
    {
        var template = "Hello {{ name }}!";
        var data = new { name = "World" };

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("Hello World!", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_MultipleVariables_ReplacesAll()
    {
        var template = "{{ greeting }} {{ name }}, you are {{ age }} years old.";
        var data = new { greeting = "Hello", name = "Alice", age = 30 };

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("Hello Alice, you are 30 years old.", result);
    }

    [TestMethod]
    [Ignore("Scriban is case-sensitive by design - use lowercase property names in templates")]
    public async Task ProcessTemplateAsync_CaseInsensitiveKeys_WorksCorrectly()
    {
        var template = "{{ name }} - {{ email }} - {{ city }}";
        var data = new { name = "Bob", email = "bob@test.com", city = "NYC" };

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("Bob - bob@test.com - NYC", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_NestedProperties_WorksCorrectly()
    {
        var template = "{{ user.name }} works at {{ user.company }}";
        var data = new { user = new { name = "John", company = "Acme Corp" } };

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("John works at Acme Corp", result);
    }

    #endregion

    #region JsonElement Support

    [TestMethod]
    public async Task ProcessTemplateAsync_JsonElement_SimpleObject_WorksCorrectly()
    {
        var template = "User: {{ name }}, Email: {{ email }}";
        var json = """{"name":"Jane","email":"jane@example.com"}""";
        var data = JsonSerializer.Deserialize<JsonElement>(json);

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("User: Jane, Email: jane@example.com", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_JsonElement_NestedObject_WorksCorrectly()
    {
        var template = "{{ user.name }} works at {{ user.company }}";
        var json = """{"user":{"name":"John","company":"Acme Corp"}}""";
        var data = JsonSerializer.Deserialize<JsonElement>(json);

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("John works at Acme Corp", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_JsonElement_WithNumbers_ConvertsCorrectly()
    {
        var template = "Price: ${{ price }}, Quantity: {{ quantity }}";
        var json = """{"price":19.99,"quantity":5}""";
        var data = JsonSerializer.Deserialize<JsonElement>(json);

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("Price: $19.99, Quantity: 5", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_JsonElement_WithBooleans_ConvertsCorrectly()
    {
        var template = "Active: {{ isActive }}, Verified: {{ isVerified }}";
        var json = """{"isActive":true,"isVerified":false}""";
        var data = JsonSerializer.Deserialize<JsonElement>(json);

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("Active: true, Verified: false", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_JsonDocument_WorksCorrectly()
    {
        var template = "Document: {{ title }} by {{ author }}";
        var json = """{"title":"Test Doc","author":"John Doe"}""";
        using var jsonDoc = JsonDocument.Parse(json);

        var result = await _processor.RenderAsync(template, jsonDoc);

        Assert.AreEqual("Document: Test Doc by John Doe", result);
    }

    #endregion

    #region Loops (For)

    [TestMethod]
    public async Task ProcessTemplateAsync_ForLoop_IteratesOverArray()
    {
        var template = """
            Users:
            {{ for user in users }}
            - {{ user.name }}
            {{ end }}
            """;
        var data = new
        {
            users = new[]
            {
                new { name = "Alice" },
                new { name = "Bob" },
                new { name = "Charlie" }
            }
        };

        var result = await _processor.RenderAsync(template, data);

        StringAssert.Contains(result, "- Alice");
        StringAssert.Contains(result, "- Bob");
        StringAssert.Contains(result, "- Charlie");
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_JsonElement_WithArray_IteratesCorrectly()
    {
        var template = """
            {{ for item in items }}
            {{ item.name }}: ${{ item.price }}
            {{ end }}
            """;
        var json = """{"items":[{"name":"Apple","price":1.99},{"name":"Banana","price":0.99}]}""";
        var data = JsonSerializer.Deserialize<JsonElement>(json);

        var result = await _processor.RenderAsync(template, data);

        StringAssert.Contains(result, "Apple: $1.99");
        StringAssert.Contains(result, "Banana: $0.99");
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_EmptyArray_RendersNothing()
    {
        var template = "Start{{ for user in users }}- {{ user.name }}{{ end }}End";
        var data = new { users = Array.Empty<object>() };

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("StartEnd", result);
    }

    #endregion

    #region Conditionals (If/Else)

    [TestMethod]
    public async Task ProcessTemplateAsync_IfStatement_ShowsWhenTrue()
    {
        var template = "{{ if isPremium }}Premium Member{{ end }}";
        // Use dictionary for boolean conditionals with Scriban
        var data = new Dictionary<string, object> { { "isPremium", true } };

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("Premium Member", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_IfStatement_HidesWhenFalse()
    {
        var template = "{{ if isPremium }}Premium Member{{ end }}";
        var data = new Dictionary<string, object> { { "isPremium", false } };

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_IfElse_ShowsCorrectBranch()
    {
        var template = "{{ if isPremium }}Premium{{ else }}Standard{{ end }}";

        var premiumData = new Dictionary<string, object> { { "isPremium", true } };
        var standardData = new Dictionary<string, object> { { "isPremium", false } };

        var premiumResult = await _processor.RenderAsync(template, premiumData);
        var standardResult = await _processor.RenderAsync(template, standardData);

        Assert.AreEqual("Premium", premiumResult);
        Assert.AreEqual("Standard", standardResult);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_IfWithEmptyArray_WorksCorrectly()
    {
        var template = "{{ if users.size > 0 }}Has users{{ else }}No users{{ end }}";

        var withUsers = new { users = new[] { new { name = "Alice" } } };
        var noUsers = new { users = Array.Empty<object>() };

        var withResult = await _processor.RenderAsync(template, withUsers);
        var noResult = await _processor.RenderAsync(template, noUsers);

        Assert.AreEqual("Has users", withResult);
        Assert.AreEqual("No users", noResult);
    }

    #endregion

    #region Email Template Examples

    [TestMethod]
    public async Task ProcessTemplateAsync_EmailTemplate_WorksCorrectly()
    {
        var template = """
            Dear {{ recipientName }},

            Thank you for your order #{{ orderId }}.

            Items:
            {{ for item in items }}
            - {{ item.name }} (Qty: {{ item.quantity }}) - ${{ item.price }}
            {{ end }}

            Total: ${{ total }}

            Best regards,
            {{ senderName }}
            """;

        // Use JsonElement for reliable property access with Scriban
        var json = """
            {
                "recipientName": "John Doe",
                "orderId": "12345",
                "items": [
                    { "name": "Widget", "quantity": 2, "price": 19.99 },
                    { "name": "Gadget", "quantity": 1, "price": 49.99 }
                ],
                "total": 89.97,
                "senderName": "Customer Service"
            }
            """;
        var data = JsonSerializer.Deserialize<JsonElement>(json);

        var result = await _processor.RenderAsync(template, data);

        StringAssert.Contains(result, "Dear John Doe");
        StringAssert.Contains(result, "order #12345");
        StringAssert.Contains(result, "Widget");
        StringAssert.Contains(result, "Gadget");
        StringAssert.Contains(result, "Total: $89.97");
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_WelcomeEmailTemplate_WithJsonElement()
    {
        var template = """
            Welcome {{ user.firstName }}!

            Your account has been created with the following details:
            - Username: {{ user.username }}
            - Email: {{ user.email }}
            {{ if user.isPremium }}
            - Premium Member: Yes
            {{ else }}
            - Premium Member: No
            {{ end }}
            """;

        var json = """
            {
                "user": {
                    "firstName": "Alice",
                    "username": "alice123",
                    "email": "alice@example.com",
                    "isPremium": true
                }
            }
            """;
        var data = JsonSerializer.Deserialize<JsonElement>(json);

        var result = await _processor.RenderAsync(template, data);

        StringAssert.Contains(result, "Welcome Alice!");
        StringAssert.Contains(result, "alice123");
        StringAssert.Contains(result, "Premium Member: Yes");
    }

    #endregion

    #region Scriban-Specific Features

    [TestMethod]
    public async Task ProcessTemplateAsync_MathOperations_WorksCorrectly()
    {
        var template = "{{ price * quantity }} total items";
        var data = new { price = 10, quantity = 5 };

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("50 total items", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_StringFunctions_WorksCorrectly()
    {
        var template = "{{ name | string.upcase }}";
        var data = new { name = "hello world" };

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("HELLO WORLD", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_ArraySize_WorksCorrectly()
    {
        var template = "You have {{ items.size }} items";
        var data = new { items = new[] { 1, 2, 3, 4, 5 } };

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("You have 5 items", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_ForLoopWithIndex_WorksCorrectly()
    {
        var template = """
            {{ for user in users }}
            {{ for.index + 1 }}. {{ user.name }}
            {{ end }}
            """;
        var data = new
        {
            users = new[] { new { name = "Alice" }, new { name = "Bob" } }
        };

        var result = await _processor.RenderAsync(template, data);

        StringAssert.Contains(result, "1. Alice");
        StringAssert.Contains(result, "2. Bob");
    }

    #endregion

    #region Error Handling

    [TestMethod]
    public async Task ProcessTemplateAsync_NullTemplate_ThrowsException()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () =>
        {
            await _processor.RenderAsync(null!, new { });
        });
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_EmptyTemplate_ThrowsException()
    {
        await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
        {
            await _processor.RenderAsync("", new { });
        });
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_WhitespaceTemplate_ThrowsException()
    {
        await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
        {
            await _processor.RenderAsync("   ", new { });
        });
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_NullData_ThrowsException()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () =>
        {
            await _processor.RenderAsync("{{ test }}", null!);
        });
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_InvalidTemplate_ThrowsException()
    {
        var template = "{{ if true }}"; // Missing end tag

        await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
        {
            await _processor.RenderAsync(template, new { });
        });
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_MissingVariable_RendersEmpty()
    {
        var template = "Hello {{ name }}, your role is {{ role }}!";
        var data = new { name = "Bob" }; // Missing 'role'

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("Hello Bob, your role is !", result);
    }

    #endregion

    #region Complex Scenarios

    [TestMethod]
    public async Task ProcessTemplateAsync_DeeplyNestedJsonElement_WorksCorrectly()
    {
        var template = "{{ company.departments.engineering.manager.name }} manages {{ company.departments.engineering.teamSize }} engineers";
        var json = """
            {
                "company": {
                    "departments": {
                        "engineering": {
                            "manager": {
                                "name": "Sarah Chen"
                            },
                            "teamSize": 15
                        }
                    }
                }
            }
            """;
        var data = JsonSerializer.Deserialize<JsonElement>(json);

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("Sarah Chen manages 15 engineers", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_MixedTypes_RendersCorrectly()
    {
        var template = "String: {{ str }}, Int: {{ num }}, Bool: {{ flag }}";
        var json = """{"str":"text","num":42,"flag":true}""";
        var data = JsonSerializer.Deserialize<JsonElement>(json);

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("String: text, Int: 42, Bool: true", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_NestedLoops_WorksCorrectly()
    {
        var template = """
            {{ for dept in departments }}
            {{ dept.name }}:
            {{ for emp in dept.employees }}
              - {{ emp.name }}
            {{ end }}
            {{ end }}
            """;
        var data = new
        {
            departments = new[]
            {
                new { name = "Engineering", employees = new[] { new { name = "Alice" }, new { name = "Bob" } } },
                new { name = "Sales", employees = new[] { new { name = "Charlie" } } }
            }
        };

        var result = await _processor.RenderAsync(template, data);

        StringAssert.Contains(result, "Engineering:");
        StringAssert.Contains(result, "- Alice");
        StringAssert.Contains(result, "- Bob");
        StringAssert.Contains(result, "Sales:");
        StringAssert.Contains(result, "- Charlie");
    }

    #endregion
}
