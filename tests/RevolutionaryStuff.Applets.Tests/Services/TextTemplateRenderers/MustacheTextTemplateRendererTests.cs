using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Applets.Services.TextTemplateRenderers;


namespace RevolutionaryStuff.Applets.Tests.Services.TextTemplateRenderers;

[TestClass]
public class MustacheTextTemplateProcessorTests
{
    private IMustacheTextTemplateRenderer _processor = null!;

    [TestInitialize]
    public void Setup()
    {
        _processor = new MustacheTextTemplateRenderer();
    }

    #region Basic Variable Substitution

    [TestMethod]
    public async Task ProcessTemplateAsync_SimpleVariable_ReplacesCorrectly()
    {
        var template = "Hello {{name}}!";
        var data = new { name = "World" };

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("Hello World!", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_MultipleVariables_ReplacesAll()
    {
        var template = "{{greeting}} {{name}}, you are {{age}} years old.";
        var data = new { greeting = "Hello", name = "Alice", age = 30 };

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("Hello Alice, you are 30 years old.", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_CaseInsensitiveKeys_WorksCorrectly()
    {
        var template = "{{NAME}} - {{Email}} - {{CITY}}";
        var data = new { name = "Bob", email = "bob@test.com", city = "NYC" };

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("Bob - bob@test.com - NYC", result);
    }

    #endregion

    #region JsonElement Support

    [TestMethod]
    public async Task ProcessTemplateAsync_JsonElement_SimpleObject_WorksCorrectly()
    {
        var template = "User: {{name}}, Email: {{email}}";
        var json = """{"name":"Jane","email":"jane@example.com"}""";
        var data = JsonSerializer.Deserialize<JsonElement>(json);

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("User: Jane, Email: jane@example.com", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_JsonElement_NestedObject_WorksCorrectly()
    {
        var template = "{{user.name}} works at {{user.company}}";
        var json = """{"user":{"name":"John","company":"Acme Corp"}}""";
        var data = JsonSerializer.Deserialize<JsonElement>(json);

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("John works at Acme Corp", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_JsonElement_WithNumbers_ConvertsCorrectly()
    {
        var template = "Price: ${{price}}, Quantity: {{quantity}}";
        var json = """{"price":19.99,"quantity":5}""";
        var data = JsonSerializer.Deserialize<JsonElement>(json);

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("Price: $19.99, Quantity: 5", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_JsonElement_WithBooleans_ConvertsCorrectly()
    {
        var template = "Active: {{isActive}}, Verified: {{isVerified}}";
        var json = """{"isActive":true,"isVerified":false}""";
        var data = JsonSerializer.Deserialize<JsonElement>(json);

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("Active: True, Verified: False", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_JsonDocument_WorksCorrectly()
    {
        var template = "Document: {{title}} by {{author}}";
        var json = """{"title":"Test Doc","author":"John Doe"}""";
        using var jsonDoc = JsonDocument.Parse(json);

        var result = await _processor.RenderAsync(template, jsonDoc);

        Assert.AreEqual("Document: Test Doc by John Doe", result);
    }

    #endregion

    #region Sections (Loops)

    [TestMethod]
    public async Task ProcessTemplateAsync_Section_IteratesOverArray()
    {
        var template = "Users:\n{{#users}}- {{name}}\n{{/users}}";
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

        Assert.AreEqual("Users:\n- Alice\n- Bob\n- Charlie\n", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_JsonElement_WithArray_IteratesCorrectly()
    {
        var template = "{{#items}}{{name}}: ${{price}}\n{{/items}}";
        var json = """{"items":[{"name":"Apple","price":1.99},{"name":"Banana","price":0.99}]}""";
        var data = JsonSerializer.Deserialize<JsonElement>(json);

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("Apple: $1.99\nBanana: $0.99\n", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_EmptySection_RendersNothing()
    {
        var template = "Start{{#users}}- {{name}}{{/users}}End";
        var data = new { users = Array.Empty<object>() };

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("StartEnd", result);
    }

    #endregion

    #region Inverted Sections

    [TestMethod]
    public async Task ProcessTemplateAsync_InvertedSection_ShowsWhenEmpty()
    {
        var template = "{{^users}}No users found{{/users}}";
        var data = new { users = Array.Empty<object>() };

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("No users found", result);
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_InvertedSection_HidesWhenNotEmpty()
    {
        var template = "{{^users}}No users{{/users}}{{#users}}{{name}}{{/users}}";
        var data = new { users = new[] { new { name = "Alice" } } };

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("Alice", result);
    }

    #endregion

    #region Email Template Examples

    [TestMethod]
    public async Task ProcessTemplateAsync_EmailTemplate_WorksCorrectly()
    {
        var template = """
            Dear {{recipientName}},

            Thank you for your order #{{orderId}}.

            Items:
            {{#items}}
            - {{name}} (Qty: {{quantity}}) - ${{price}}
            {{/items}}

            Total: ${{total}}

            Best regards,
            {{senderName}}
            """;

        var data = new
        {
            recipientName = "John Doe",
            orderId = "12345",
            items = new[]
            {
                new { name = "Widget", quantity = 2, price = 19.99 },
                new { name = "Gadget", quantity = 1, price = 49.99 }
            },
            total = 89.97,
            senderName = "Customer Service"
        };

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
            Welcome {{user.firstName}}!

            Your account has been created with the following details:
            - Username: {{user.username}}
            - Email: {{user.email}}
            {{#user.isPremium}}
            - Premium Member: Yes
            {{/user.isPremium}}

            {{^user.isPremium}}
            Upgrade to Premium for more features!
            {{/user.isPremium}}
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
        Assert.IsFalse(result.Contains("Upgrade to Premium"));
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
            await _processor.RenderAsync("{{test}}", null!);
        });
    }

    [TestMethod]
    public async Task ProcessTemplateAsync_MissingVariable_RendersEmpty()
    {
        var template = "Hello {{name}}, your role is {{role}}!";
        var data = new { name = "Bob" }; // Missing 'role'

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("Hello Bob, your role is !", result);
    }

    #endregion

    #region Complex Scenarios

    [TestMethod]
    public async Task ProcessTemplateAsync_DeeplyNestedJsonElement_WorksCorrectly()
    {
        var template = "{{company.departments.engineering.manager.name}} manages {{company.departments.engineering.teamSize}} engineers";
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
        var template = "String: {{str}}, Int: {{num}}, Bool: {{flag}}, Null: {{nothing}}";
        var json = """{"str":"text","num":42,"flag":true,"nothing":null}""";
        var data = JsonSerializer.Deserialize<JsonElement>(json);

        var result = await _processor.RenderAsync(template, data);

        Assert.AreEqual("String: text, Int: 42, Bool: True, Null: ", result);
    }

    #endregion
}
